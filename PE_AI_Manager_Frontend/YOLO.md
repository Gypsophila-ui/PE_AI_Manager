# AI Gym 后端 API 文档

## 概述

AI Gym 是一个基于计算机视觉的姿态识别系统，能够识别和计数多种健身动作。本系统采用 FastAPI 构建 RESTful API 接口，使用 YOLOv8-pose 模型进行人体姿态估计，可以准确识别用户执行的各种健身动作并实时计数。

## 技术栈

- Python 3.x
- FastAPI - Web 框架
- YOLOv8-pose - 人体姿态估计模型
- OpenCV - 视频处理
- Utralytics - YOLO 模型库

## 启动服务

```bash
python main.py
```

服务默认运行在 `http://localhost:8000`

## API 接口说明

### 1. 健康检查

**接口地址**: `GET /health`

**功能描述**: 检查后端服务是否正常运行

**请求示例**:
```bash
curl -X GET http://localhost:8000/health
```

**响应示例**:
```json
{
  "status": "healthy",
  "model_loaded": true,
  "active_sessions": 0
}
```

**响应字段说明**:
- `status`: 服务状态，"healthy"表示正常
- `model_loaded`: 模型是否加载成功
- `active_sessions`: 当前活跃会话数量

### 2. 获取支持的动作类型

**接口地址**: `GET /supported_poses`

**功能描述**: 获取后端支持的所有健身动作类型

**请求示例**:
```bash
curl -X GET http://localhost:8000/supported_poses
```

**响应示例**:
```json
{
  "supported_poses": [
    "pushup",
    "abworkout", 
    "squat",
    "deadlift",
    "benchpress"
  ]
}
```

### 3. 视频处理接口

**接口地址**: `POST /process_video`

**功能描述**: 上传视频文件进行健身动作识别和计数，并返回标注后的视频

**请求参数**:
- `file`: 视频文件（必填），支持格式：mp4, avi, mov
- `pose_type`: 动作类型（选填，默认为 "pushup"），可选值参考 [/supported_poses](file:///G:/Third_year_first_semester/SoftwareEngineering/Yolo_backend/main.py#L297-L303) 接口

**请求示例**:
```bash
curl -X POST \
  -F "file=@test_video.mp4" \
  -F "pose_type=pushup" \
  http://localhost:8000/process_video \
  --output processed_video.mp4
```

**响应说明**:
- 成功：返回处理后的视频文件（MP4格式）
- 失败：返回JSON格式的错误信息

**响应头信息**:
处理成功时，响应头会包含以下统计信息：
- `X-Final-Count`: 最终计数结果
- `X-Total-Frames`: 处理的总帧数
- `X-Total-Time`: 视频总时长（秒）

**响应示例**（成功）:
HTTP 状态码: 200
响应体: 处理后的视频文件（二进制）
响应头:
```
Content-Type: video/mp4
Content-Disposition: attachment; filename=processed_pushup.mp4
X-Final-Count: 12
X-Total-Frames: 360
X-Total-Time: 12.0
```

**响应示例**（失败）:
```json
{
  "status": "error",
  "message": "处理视频时出错: 错误详情",
  "traceback": "堆栈跟踪信息"
}
```

### 4. 处理并保存视频接口

**接口地址**: `POST /process_and_save_video`

**功能描述**: 上传视频文件进行健身动作识别和计数，并将处理后的视频保存到指定目录

**请求参数**:
- `homework_id`: 作业ID（必填，路径参数）
- `student_id`: 学生ID（必填，路径参数）
- `pose_type`: 动作类型（选填，默认为 "pushup"）
- `file`: 视频文件（必填），支持格式：mp4, avi, mov

**请求示例**:
```bash
curl -X POST \
  -F "file=@test_video.mp4" \
  -F "pose_type=pushup" \
  "http://localhost:8000/process_and_save_video?homework_id=hw001&student_id=stu001"
```

**响应示例**（成功）:
```json
{
  "status": "success",
  "message": "视频处理完成并已保存",
  "final_count": 12,
  "total_frames": 360,
  "total_time": 12.0,
  "video_url": "/get_processed_video?homework_id=hw001&student_id=stu001"
}
```

### 5. 获取已处理视频接口

**接口地址**: `GET /get_processed_video`

**功能描述**: 根据作业ID和学生ID获取已处理的视频文件

**请求参数**:
- `homework_id`: 作业ID（必填，查询参数）
- `student_id`: 学生ID（必填，查询参数）

**请求示例**:
```bash
curl -X GET "http://localhost:8000/get_processed_video?homework_id=hw001&student_id=stu001" \
  --output processed_video.mp4
```

**响应说明**:
- 成功：返回处理后的视频文件（MP4格式）
- 失败：返回404错误（文件不存在）

### 6. 删除作业接口

**接口地址**: `DELETE /delete_homework`

**功能描述**: 删除指定作业ID下的所有视频文件

**请求参数**:
- `homework_id`: 作业ID（必填，查询参数）

**请求示例**:
```bash
curl -X DELETE "http://localhost:8000/delete_homework?homework_id=hw001"
```

**响应示例**（成功）:
```json
{
  "status": "success",
  "message": "作业 hw001 的所有视频已成功删除"
}
```

**响应示例**（失败）:
```json
{
  "detail": "作业ID hw001 不存在"
}
```

## 前端调用指南

### JavaScript (使用 fetch API) 示例

```
// 设置基础URL
const BASE_URL = 'http://localhost:8000';

// 上传视频并处理
const processVideo = async (file, poseType = 'pushup') => {
  const formData = new FormData();
  formData.append('file', file);
  formData.append('pose_type', poseType);

  try {
    const response = await fetch(`${BASE_URL}/process_video`, {
      method: 'POST',
      body: formData
    });

    if (response.ok) {
      // 获取响应头中的统计数据
      const finalCount = response.headers.get('X-Final-Count');
      const totalFrames = response.headers.get('X-Total-Frames');
      const totalTime = response.headers.get('X-Total-Time');
      
      // 获取处理后的视频
      const blob = await response.blob();
      const videoUrl = URL.createObjectURL(blob);
      
      console.log(`动作计数: ${finalCount}`);
      console.log(`总帧数: ${totalFrames}`);
      console.log(`视频时长: ${totalTime} 秒`);
      
      return {
        videoUrl,
        stats: {
          count: parseInt(finalCount),
          frames: parseInt(totalFrames),
          time: parseFloat(totalTime)
        }
      };
    } else {
      const errorData = await response.json();
      throw new Error(errorData.message);
    }
  } catch (error) {
    console.error('处理视频时发生错误:', error);
    throw error;
  }
};

// 上传视频并保存到指定目录
const processAndSaveVideo = async (homeworkId, studentId, file, poseType = 'pushup') => {
  const formData = new FormData();
  formData.append('file', file);

  try {
    const response = await fetch(
      `${BASE_URL}/process_and_save_video?homework_id=${encodeURIComponent(homeworkId)}&student_id=${encodeURIComponent(studentId)}&pose_type=${encodeURIComponent(poseType)}`,
      {
        method: 'POST',
        body: formData
      }
    );

    if (response.ok) {
      const result = await response.json();
      return result;
    } else {
      const errorData = await response.json();
      throw new Error(errorData.message);
    }
  } catch (error) {
    console.error('处理视频时发生错误:', error);
    throw error;
  }
};

// 获取已处理的视频
const getProcessedVideo = async (homeworkId, studentId) => {
  try {
    const response = await fetch(
      `${BASE_URL}/get_processed_video?homework_id=${encodeURIComponent(homeworkId)}&student_id=${encodeURIComponent(studentId)}`
    );

    if (response.ok) {
      const blob = await response.blob();
      const videoUrl = URL.createObjectURL(blob);
      return videoUrl;
    } else if (response.status === 404) {
      throw new Error('未找到处理后的视频文件');
    } else {
      throw new Error('获取视频时发生错误');
    }
  } catch (error) {
    console.error('获取视频时发生错误:', error);
    throw error;
  }
};

// 删除作业
const deleteHomework = async (homeworkId) => {
  try {
    const response = await fetch(
      `${BASE_URL}/delete_homework?homework_id=${encodeURIComponent(homeworkId)}`,
      {
        method: 'DELETE'
      }
    );

    if (response.ok) {
      const result = await response.json();
      return result;
    } else {
      const errorData = await response.json();
      throw new Error(errorData.detail);
    }
  } catch (error) {
    console.error('删除作业时发生错误:', error);
    throw error;
  }
};

// 使用示例
const fileInput = document.getElementById('videoInput');
fileInput.addEventListener('change', async (event) => {
  const file = event.target.files[0];
  if (file) {
    try {
      // 处理并保存视频
      const result = await processAndSaveVideo('hw001', 'stu001', file, 'pushup');
      console.log('处理结果:', result);
      
      // 获取处理后的视频
      const videoUrl = await getProcessedVideo('hw001', 'stu001');
      
      // 在页面上显示结果
      const videoElement = document.getElementById('processedVideo');
      videoElement.src = videoUrl;
      document.getElementById('countResult').textContent = `动作计数: ${result.final_count}`;
    } catch (error) {
      alert('处理失败: ' + error.message);
    }
  }
});
```

### Axios 示例

```
import axios from 'axios';

// 设置基础URL
const BASE_URL = 'http://localhost:8000';

const processVideoWithAxios = async (file, poseType = 'pushup') => {
  const formData = new FormData();
  formData.append('file', file);
  formData.append('pose_type', poseType);

  try {
    const response = await axios.post(
      `${BASE_URL}/process_video`,
      formData,
      {
        responseType: 'blob', // 重要：指定响应类型为 blob
        onDownloadProgress: (progressEvent) => {
          // 可选：处理下载进度
          if (progressEvent.lengthComputable) {
            const percentCompleted = Math.round(
              (progressEvent.loaded * 100) / progressEvent.total
            );
            console.log(`下载进度: ${percentCompleted}%`);
          }
        },
      }
    );

    // 从响应头中提取统计数据
    const finalCount = response.headers['x-final-count'];
    const totalFrames = response.headers['x-total-frames'];
    const totalTime = response.headers['x-total-time'];

    // 创建视频 URL
    const videoUrl = URL.createObjectURL(response.data);

    return {
      videoUrl,
      stats: {
        count: parseInt(finalCount),
        frames: parseInt(totalFrames),
        time: parseFloat(totalTime)
      }
    };
  } catch (error) {
    if (error.response && error.response.data) {
      // 尝试解析错误响应
      const reader = new FileReader();
      reader.onload = () => {
        try {
          const errorData = JSON.parse(reader.result);
          throw new Error(errorData.message);
        } catch {
          throw new Error('处理视频时发生未知错误');
        }
      };
      reader.readAsText(error.response.data);
    } else {
      throw new Error('网络错误或服务器无响应');
    }
  }
};
```

## 支持的动作类型

| 动作类型 | 英文名称 | 说明 |
|---------|---------|------|
| 俯卧撑 | pushup | 手臂弯曲和伸直的运动 |
| 腹肌训练 | abworkout | 腹部肌肉训练动作 |
| 深蹲 | squat | 下肢力量训练动作 |
| 硬拉 | deadlift | 后背及下肢综合训练动作 |
| 卧推 | benchpress | 上肢力量训练动作 |

## 数据返回格式

### 成功响应
当视频处理成功时，后端会返回以下数据：

1. HTTP 状态码: `200 OK`
2. 响应体: 处理后的视频文件（二进制 MP4 格式）
3. 响应头包含统计信息:
   - `X-Final-Count`: 最终动作计数结果（整数）
   - `X-Total-Frames`: 处理的视频总帧数（整数）
   - `X-Total-Time`: 视频总时长（浮点数，单位：秒）

### 错误响应
当处理过程中出现错误时，后端会返回 JSON 格式的错误信息：

```json
{
  "status": "error",
  "message": "错误描述信息",
  "traceback": "详细的堆栈跟踪信息（仅在开发环境中提供）"
}
```

## 错误处理

接口可能返回以下 HTTP 状态码：

| 状态码 | 说明 |
|-------|------|
| 200 | 请求成功 |
| 400 | 请求参数错误，如无法读取视频文件 |
| 404 | 请求的资源不存在（如视频文件未找到） |
| 422 | 请求参数验证失败 |
| 500 | 服务器内部错误 |
| 503 | 服务暂时不可用 |

## 使用流程

1. 前端调用 [/health](file:///G:/Third_year_first_semester/SoftwareEngineering/Yolo_backend/main.py#L353-L359) 接口确认后端服务正常运行
2. 调用 [/supported_poses](file:///G:/Third_year_first_semester/SoftwareEngineering/Yolo_backend/main.py#L361-L367) 获取支持的动作类型列表
3. 用户选择动作类型并上传视频文件
4. 调用 [/process_video](file:///G:/Third_year_first_semester/SoftwareEngineering/Yolo_backend/main.py#L109-L286) 接口处理视频（直接返回）
5. 或者调用 [/process_and_save_video](file:///G:/Third_year_first_semester/SoftwareEngineering/Yolo_backend/main.py#L288-L333) 接口处理视频并保存到指定目录
6. 使用 [/get_processed_video](file:///G:/Third_year_first_semester/SoftwareEngineering/Yolo_backend/main.py#L335-L345) 接口获取已保存的处理视频
7. 如需删除作业，可调用 [/delete_homework](file:///G:/Third_year_first_semester/SoftwareEngineering/Yolo_backend/main.py#L347-L361) 接口删除指定作业的所有视频

## 注意事项

1. 视频文件大小建议控制在合理范围内，过大的文件可能导致处理超时
2. 处理时间与视频长度和复杂度有关，请设置合适的请求超时时间（建议5分钟）
3. 确保上传的视频中人物清晰可见，背景不要太复杂
4. 当前版本仅支持单人动作识别
5. 建议在弱光环境下拍摄时注意光线均匀，避免逆光
6. 前端在接收视频文件时需要正确处理 Blob 对象，并使用 `URL.createObjectURL()` 创建可播放的 URL
7. 由于视频处理需要一定时间，建议在 UI 上提供加载状态指示
8. 使用 [/process_and_save_video](file:///G:/Third_year_first_semester/SoftwareEngineering/Yolo_backend/main.py#L288-L333) 接口时，相同 homework_id 和 student_id 的视频会被新上传的视频覆盖
9. 使用 [/delete_homework](file:///G:/Third_year_first_semester/SoftwareEngineering/Yolo_backend/main.py#L347-L361) 接口时请谨慎操作，删除操作不可恢复
