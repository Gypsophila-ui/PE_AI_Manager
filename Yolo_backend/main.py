import asyncio
import base64
import json
import uuid
from fastapi import FastAPI, File, UploadFile, HTTPException, Query
from fastapi.responses import StreamingResponse, Response, FileResponse
from fastapi.middleware.cors import CORSMiddleware
import cv2
from ultralytics import YOLO
from ai_gym import AIGym
import tempfile
import os
import uvicorn
import traceback
import logging
import time
import shutil

app = FastAPI(title="AI Gym API", description="健身动作识别后端API")

# 设置日志
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# 允许跨域请求
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
    expose_headers=["X-Final-Count", "X-Total-Frames", "X-Total-Time"],  # 添加这行
)


class SSEStreamingResponse(StreamingResponse):
    """自定义StreamingResponse以支持SSE"""
    def __init__(self, content, status_code=200, headers=None, media_type="text/event-stream"):
        super().__init__(content, status_code, headers, media_type)


class VideoProcessor:
    def __init__(self):
        self.model = None
        self.gym_objects = {}

    def load_model(self):
        """加载YOLO模型"""
        if self.model is None:
            try:
                self.model = YOLO("yolov8n-pose.pt")
                print("YOLO模型加载成功")
            except Exception as e:
                print(f"模型加载失败: {e}")
                # 尝试从当前目录的models文件夹加载
                try:
                    self.model = YOLO(os.path.join("models", "yolov8n-pose.pt"))
                    print("从models文件夹加载模型成功")
                except Exception as e2:
                    print(f"备用模型加载也失败: {e2}")
                    raise e2

    def create_gym_object(self, session_id: str, pose_type: str, fps: int = 30):
        """创建AIGym对象"""
        # 根据姿势类型设定关键点
        if pose_type in ["pushup", "pullup"]:
            kpts_to_check = [6, 8, 10]
        elif pose_type in ["squat", "burpees"]:
            kpts_to_check = [12, 14, 16]
        elif pose_type in ["deadlift"]:
            kpts_to_check = [6, 12, 14]
        elif pose_type in ["abworkout"]:
            kpts_to_check = [6, 12, 16]
        elif pose_type in ["benchpress"]:
            kpts_to_check = [5, 7, 9]  # 左肩、左肘、左腕
        else:
            kpts_to_check = [6, 8, 10]  # 默认关键点

        gym_object = AIGym(
            kpts_to_check=kpts_to_check,
            line_thickness=2,
            view_img=True,  # 开启显示图像
            pose_type=pose_type,
            fps=fps
        )

        self.gym_objects[session_id] = gym_object
        return gym_object


video_processor = VideoProcessor()


@app.on_event("startup")
async def startup_event():
    """启动时加载模型"""
    video_processor.load_model()


@app.on_event("shutdown")
async def shutdown_event():
    """关闭时清理临时目录"""
    for temp_info in temp_dirs.values():
        try:
            shutil.rmtree(temp_info['path'])
        except Exception as e:
            logger.error(f"关闭时清理临时目录时出错: {e}")


def sse_format(event_type, data):
    """格式化SSE事件数据"""
    json_data = json.dumps({"event": event_type, "data": data})
    return f"data: {json_data}\n\n"


async def stream_video_processing(file_content: bytes, pose_type: str):
    """流式处理视频并逐帧返回结果"""
    session_id = None
    cap = None
    out = None
    
    try:
        logger.info("开始流式处理视频上传...")
        
        # 发送初始化事件
        yield sse_format("init", {"message": "开始处理视频"})
        
        # 创建临时目录
        temp_dir = tempfile.mkdtemp()
        tmp_file_path = os.path.join(temp_dir, "input_video.mp4")
        output_path = os.path.join(temp_dir, "output_video.mp4")
        
        # 生成临时目录ID并存储
        temp_id = str(uuid.uuid4())
        temp_dirs[temp_id] = {
            'path': temp_dir,
            'created_at': time.time(),
            'last_access': time.time()
        }
        
        # 将文件内容写入临时文件
        with open(tmp_file_path, "wb") as buffer:
            buffer.write(file_content)
            
        logger.info(f"视频已保存到 {tmp_file_path}")
        yield sse_format("init", {"message": "视频上传完成", "path": tmp_file_path})

        # 读取视频
        logger.info("尝试打开视频文件...")
        cap = cv2.VideoCapture(tmp_file_path)
        
        if not cap.isOpened():
            logger.error("无法打开视频文件")
            yield sse_format("error", {"message": "无法打开视频文件"})
            return
            
        # 获取视频信息
        fps = int(cap.get(cv2.CAP_PROP_FPS))
        if fps == 0:
            fps = 30
            
        width = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH))
        height = int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))
        frame_count = int(cap.get(cv2.CAP_PROP_FRAME_COUNT))
        
        logger.info(f"视频信息: FPS={fps}, 宽度={width}, 高度={height}, 总帧数={frame_count}")
        yield sse_format("init", {
            "message": "视频信息获取完成",
            "fps": fps,
            "width": width,
            "height": height,
            "frame_count": frame_count
        })

        # 视频编码器设置
        SKIP_FACTOR = 2
        output_fps = fps / SKIP_FACTOR
        fourcc = cv2.VideoWriter_fourcc(*'mp4v')
        out = cv2.VideoWriter(output_path, fourcc, output_fps, (width, height), True)
        
        if not out.isOpened():
            logger.warning("使用默认API创建视频写入器")
            out = cv2.VideoWriter()
            out.open(output_path, cv2.CAP_FFMPEG, fourcc, output_fps, (width, height), True)
            
        if not out.isOpened():
            yield sse_format("error", {"message": "无法创建输出视频文件"})
            return
            
        logger.info(f"输出视频文件创建成功: {output_path}")
        yield sse_format("init", {"message": "输出视频文件创建成功", "output_path": output_path})

        # 创建gym对象
        session_id = f"session_{int(time.time() * 1000000) % 1000000}"
        logger.info(f"创建gym对象，session_id: {session_id}")
        gym_object = video_processor.create_gym_object(session_id, pose_type, fps)
        
        processed_frame_count = 0
        max_count = 0
        frame_index = 0
        
        # 处理每一帧
        logger.info("开始逐帧处理视频...")
        yield sse_format("init", {"message": "开始逐帧处理视频"})
        
        while True:
            success, frame = cap.read()
            if not success:
                logger.info("视频读取完成")
                break
                
            # 跳帧处理
            if frame_index % SKIP_FACTOR != 0:
                frame_index += 1
                continue
                
            try:
                processed_frame, count = gym_object.monitor(frame)
                if processed_frame is not None:
                    # 写入处理后的帧到输出视频
                    out.write(processed_frame)
                    
                    max_count = max(max_count, count)
                    processed_frame_count += 1
                    
                    # 每隔一定帧数发送进度更新
                    if processed_frame_count % 2 == 0:  # 每2帧发送一次更新
                        # 将处理后的帧编码为JPEG并转换为base64
                        _, buffer = cv2.imencode('.jpg', processed_frame)
                        jpg_as_text = base64.b64encode(buffer).decode('utf-8')
                        
                        yield sse_format("frame", {
                            "frame_index": frame_index,
                            "processed_frame_count": processed_frame_count,
                            "count": count,
                            "max_count": max_count,
                            "image": jpg_as_text
                        })
                        
                        # 短暂休眠以避免过快发送数据
                        await asyncio.sleep(0.01)
                        
                frame_index += 1
                
            except Exception as e:
                error_msg = f"处理第 {frame_index} 帧时出错: {str(e)}"
                logger.error(error_msg)
                logger.error(traceback.format_exc())
                yield sse_format("error", {"message": error_msg})
                continue
                
        logger.info(f"视频处理完成，共处理 {processed_frame_count} 帧")
        
        # 确保视频文件正确关闭
        out.release()
        cap.release()
        
        # 读取处理后的视频文件
        with open(output_path, "rb") as video_file:
            processed_video = video_file.read()
            
        # 发送最终统计信息
        yield sse_format("final_stats", {
            "message": "视频处理完成",
            "max_count": max_count,
            "processed_frame_count": processed_frame_count,
            "total_time": frame_index / fps if fps > 0 else 0,
            "output_path": output_path,
            "video_size": len(processed_video)
        })
        
        # 保存视频以便下载
        download_path = os.path.join(temp_dir, "download_video.mp4")
        with open(download_path, "wb") as f:
            f.write(processed_video)
            
        yield sse_format("final_stats", {
            "message": "视频可供下载",
            "download_url": f"/download_processed_video?temp_id={temp_id}"
        })
        
    except Exception as e:
        error_msg = f"处理视频时出错: {str(e)}"
        logger.error(error_msg)
        logger.error(traceback.format_exc())
        yield sse_format("error", {"message": error_msg})
        
    finally:
        # 释放资源
        try:
            if cap is not None:
                cap.release()
            if out is not None:
                out.release()
        except Exception as e:
            logger.error(f"释放视频资源时出错: {e}")
            
        # 清理gym对象
        try:
            if session_id is not None and session_id in video_processor.gym_objects:
                del video_processor.gym_objects[session_id]
        except Exception as e:
            logger.error(f"清理gym对象时出错: {e}")
            
        # 注意：我们不立即清理temp_dir，因为用户可能需要下载视频
        # 清理将在下载完成后进行，或定期清理任务中进行


@app.post("/stream_process_video")
async def stream_process_video(file: UploadFile = File(...), pose_type: str = "pushup"):
    """流式处理视频并实时返回处理结果"""
    # 读取上传的文件内容
    file_content = await file.read()
    logger.info(f"视频文件读取完成，大小: {len(file_content)} 字节")
    
    return SSEStreamingResponse(stream_video_processing(file_content, pose_type))


@app.get("/download_processed_video")
async def download_processed_video(temp_id: str):
    """下载处理后的视频"""
    try:
        # 更新最后访问时间
        if temp_id in temp_dirs:
            temp_dirs[temp_id]['last_access'] = time.time()
        
        # 构建文件路径
        if temp_id not in temp_dirs:
            raise HTTPException(status_code=404, detail="临时目录不存在或已过期")
            
        temp_dir = temp_dirs[temp_id]['path']
        video_path = os.path.join(temp_dir, "download_video.mp4")
        
        if not os.path.exists(video_path):
            raise HTTPException(status_code=404, detail="视频文件不存在")
            
        # 返回文件响应
        return FileResponse(
            video_path,
            media_type="video/mp4",
            headers={"Content-Disposition": "attachment; filename=processed_video.mp4"}
        )
    except Exception as e:
        logger.error(f"下载视频时出错: {e}")
        raise HTTPException(status_code=500, detail="下载视频时出错")


def process_video_logic(file_content: bytes, pose_type: str):
    """处理视频的核心逻辑，供不同接口复用"""
    session_id = None
    cap = None
    out = None

    try:
        logger.info("开始处理视频上传...")

        # 创建临时目录
        temp_dir = tempfile.mkdtemp()
        tmp_file_path = os.path.join(temp_dir, "input_video.mp4")

        # 将文件内容写入临时文件
        with open(tmp_file_path, "wb") as buffer:
            buffer.write(file_content)

        logger.info(f"视频已保存到 {tmp_file_path}")

        # 读取视频
        logger.info("尝试打开视频文件...")
        logger.info(f"视频文件路径: {tmp_file_path}")

        # 检查文件是否存在
        if not os.path.exists(tmp_file_path):
            logger.error(f"视频文件不存在: {tmp_file_path}")
            raise HTTPException(status_code=400, detail="视频文件未找到")

        # 检查文件大小
        file_size = os.path.getsize(tmp_file_path)
        logger.info(f"视频文件大小: {file_size} 字节")

        # 检查文件是否可读
        if not os.access(tmp_file_path, os.R_OK):
            logger.error(f"视频文件不可读: {tmp_file_path}")
            raise HTTPException(status_code=400, detail="视频文件不可读")

        # 使用默认方式打开视频
        cap = cv2.VideoCapture(tmp_file_path)

        if not cap.isOpened():
            logger.error("无法打开视频文件")
            raise HTTPException(status_code=400, detail="无法打开视频文件")

        logger.info("视频文件打开成功")
        # 记录使用的后端
        backend_name = cap.getBackendName()
        logger.info(f"使用的后端名称: {backend_name}")

        # 获取视频信息
        fps = int(cap.get(cv2.CAP_PROP_FPS))
        if fps == 0:  # 如果无法获取FPS，使用默认值
            fps = 30

        width = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH))
        height = int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))
        frame_count = int(cap.get(cv2.CAP_PROP_FRAME_COUNT))
        logger.info(f"视频信息: FPS={fps}, 宽度={width}, 高度={height}, 总帧数={frame_count}")

        # 创建输出视频文件，保持原始FPS不变
        output_path = os.path.join(temp_dir, "output_video.mp4")

        # 跳帧因子，skip_factor=1表示不跳帧，skip_factor=2表示每隔1帧处理1帧
        SKIP_FACTOR = 2  # 可根据需要调整，值越大跳过的帧越多，处理越快但精度可能下降

        # 视频编码器
        output_fps = fps / SKIP_FACTOR
        fourcc = cv2.VideoWriter_fourcc(*'mp4v')
        out = cv2.VideoWriter(output_path, fourcc, output_fps, (width, height), True)
        # 如果上面的方法失败，尝试使用不同的方法
        if not out.isOpened():
            logger.warning("使用默认API创建视频写入器")
            out = cv2.VideoWriter()
            out.open(output_path, cv2.CAP_FFMPEG, fourcc, output_fps, (width, height), True)

        if not out.isOpened():
            raise HTTPException(status_code=500, detail="无法创建输出视频文件")
        logger.info(f"输出视频文件创建成功: {output_path}")

        # 创建gym对象
        # session_id保证并行处理和释放资源
        session_id = f"session_{int(time.time() * 1000000) % 1000000}"  # 使用时间戳生成唯一ID
        logger.info(f"创建gym对象，session_id: {session_id}")
        gym_object = video_processor.create_gym_object(
            session_id, pose_type, fps
        )
        processed_frame_count = 0
        max_count = 0
        counts_data = []

        # 处理每一帧
        logger.info("开始逐帧处理视频...")
        frame_index = 0

        while True:
            success, frame = cap.read()
            if not success:
                logger.info("视频读取完成")
                break

            # 跳帧处理，加快处理速度
            if frame_index % SKIP_FACTOR != 0:
                frame_index += 1
                continue

            try:
                processed_frame, count = gym_object.monitor(frame)
                if processed_frame is not None:
                    # 写入处理后的帧到输出视频
                    out.write(processed_frame)

                    counts_data.append({
                        "frame": processed_frame_count,
                        "time": processed_frame_count / fps if fps > 0 else 0,
                        "count": count
                    })
                    max_count = max(max_count, count)
                    processed_frame_count += 1

                frame_index += 1

                # 每处理10帧记录一次进度
                if frame_index % 10 == 0:
                    logger.debug(f"已处理 {frame_index} 帧")

            except Exception as e:
                error_msg = f"处理第 {frame_index} 帧时出错: {str(e)}"
                logger.error(error_msg)
                logger.error(traceback.format_exc())
                continue

        logger.info(f"视频处理完成，共处理 {processed_frame_count} 帧")

        # 确保视频文件正确关闭
        out.release()
        cap.release()

        # 重新打开输出视频文件以确保其完整性
        test_cap = cv2.VideoCapture(output_path)
        if not test_cap.isOpened():
            raise HTTPException(status_code=500, detail="处理后的视频文件无法打开")
        test_cap.release()

        # 读取处理后的视频文件
        with open(output_path, "rb") as video_file:
            processed_video = video_file.read()

        # 返回处理结果
        return {
            "processed_video": processed_video, # 二进制数据视频
            "max_count": max_count,
            "processed_frame_count": processed_frame_count,
            "total_time": frame_index / fps if fps > 0 else 0,  # 使用原始帧数计算总时间
            "temp_dir": temp_dir,
            "output_path": output_path
        }

    except Exception as e:
        error_msg = f"处理视频时出错: {str(e)}"
        logger.error(error_msg)
        logger.error(traceback.format_exc())
        raise HTTPException(status_code=500, detail=error_msg)

    finally:
        # 释放资源
        try:
            if cap is not None:
                cap.release()
            if out is not None:
                out.release()
        except Exception as e:
            logger.error(f"释放视频资源时出错: {e}")

        # 清理gym对象
        try:
            if session_id is not None and session_id in video_processor.gym_objects:
                del video_processor.gym_objects[session_id]
        except Exception as e:
            logger.error(f"清理gym对象时出错: {e}")


@app.post("/process_video")
async def process_video(file: UploadFile = File(...), pose_type: str = "pushup"):
    """处理视频并返回处理后的完整视频"""
    # 读取上传的文件内容
    file_content = await file.read()
    logger.info(f"视频文件读取完成，大小: {len(file_content)} 字节")
    
    # 处理视频
    result = process_video_logic(file_content, pose_type)
    
    # 清理临时目录
    try:
        if os.path.exists(result["temp_dir"]):
            shutil.rmtree(result["temp_dir"])
    except Exception as e:
        logger.error(f"清理临时目录时出错: {e}")

    # 直接返回视频流，统计信息放在header中
    return Response(
        content=result["processed_video"],
        media_type="video/mp4",
        headers={
            "Content-Disposition": f"attachment; filename=processed_{pose_type}.mp4",
            "X-Final-Count": str(result["max_count"]),
            "X-Total-Frames": str(result["processed_frame_count"]),
            "X-Total-Time": str(result["total_time"])
        }
    )


@app.post("/process_and_save_video")
async def process_and_save_video(
    homework_id: str = Query(..., description="作业ID"),
    student_id: str = Query(..., description="学生ID"),
    pose_type: str = Query("pushup", description="动作类型"),
    file: UploadFile = File(..., description="上传的视频文件")
):
    """处理视频并将处理后的视频保存到 homework/homework_id/student_id 目录下"""
    # 读取上传的文件内容
    file_content = await file.read()
    logger.info(f"视频文件读取完成，大小: {len(file_content)} 字节")
    
    # 处理视频
    result = process_video_logic(file_content, pose_type)
    
    # 创建保存目录
    save_dir = os.path.join("homework", homework_id, student_id)
    os.makedirs(save_dir, exist_ok=True)
    
    # 保存处理后的视频
    save_path = os.path.join(save_dir, "processed_video.mp4")
    with open(save_path, "wb") as f:
        f.write(result["processed_video"])
    
    # 清理临时目录
    try:
        if os.path.exists(result["temp_dir"]):
            shutil.rmtree(result["temp_dir"])
    except Exception as e:
        logger.error(f"清理临时目录时出错: {e}")

    # 返回处理结果
    return {
        "status": "success",
        "message": "视频处理完成并已保存",
        "final_count": result["max_count"],
        "total_frames": result["processed_frame_count"],
        "total_time": result["total_time"],
        "video_url": f"/get_processed_video?homework_id={homework_id}&student_id={student_id}"
    }


@app.get("/get_processed_video")
async def get_processed_video(
    homework_id: str = Query(..., description="作业ID"),
    student_id: str = Query(..., description="学生ID")
):
    """获取已处理的视频文件"""
    video_path = os.path.join("homework", homework_id, student_id, "processed_video.mp4")
    
    if not os.path.exists(video_path):
        raise HTTPException(status_code=404, detail="视频文件不存在")
    
    return FileResponse(
        video_path,
        media_type="video/mp4",
        headers={"Content-Disposition": "inline"}
    )


@app.delete("/delete_homework")
async def delete_homework(homework_id: str = Query(..., description="作业ID")):
    """删除指定作业ID下的所有视频文件"""
    homework_dir = os.path.join("homework", homework_id)
    
    if not os.path.exists(homework_dir):
        raise HTTPException(status_code=404, detail=f"作业ID {homework_id} 不存在")
    
    try:
        # 删除整个作业目录及其内容
        shutil.rmtree(homework_dir)
        return {
            "status": "success",
            "message": f"作业 {homework_id} 的所有视频已成功删除"
        }
    except Exception as e:
        logger.error(f"删除作业 {homework_id} 时出错: {str(e)}")
        raise HTTPException(status_code=500, detail=f"删除作业时出错: {str(e)}")


@app.get("/health")
async def health_check():
    """健康检查端点"""
    return {
        "status": "healthy",
        "model_loaded": video_processor.model is not None,
        "active_sessions": len(video_processor.gym_objects)
    }


@app.get("/supported_poses")
async def get_supported_poses():
    """获取支持的姿势类型"""
    return {
        "supported_poses": [
            "pushup", "abworkout", "squat",
            "deadlift", "benchpress"
        ]
    }


# 存储临时目录，以便稍后清理
temp_dirs = {}  # 存储临时目录信息，包括创建时间和路径

def cleanup_temp_dirs():
    """定期清理临时目录"""
    current_time = time.time()
    to_delete = []
    for temp_id, temp_info in temp_dirs.items():
        # 如果临时目录超过1小时未被访问，则删除
        if current_time - temp_info['last_access'] > 3600:
            try:
                shutil.rmtree(temp_info['path'])
                to_delete.append(temp_id)
            except Exception as e:
                logger.error(f"清理临时目录时出错: {e}")
    
    # 从字典中移除已删除的条目
    for temp_id in to_delete:
        del temp_dirs[temp_id]

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8000)