import streamlit as st
import requests
from io import BytesIO
import pandas as pd
import json
import time

# 后端API地址
API_BASE_URL = "http://localhost:8000"

st.title("AI Gym 后端测试前端")

# 检查后端连接
try:
    health_response = requests.get(f"{API_BASE_URL}/health")
    if health_response.status_code == 200:
        st.success("✅ 后端连接正常")
    else:
        st.error("❌ 后端连接失败")
except Exception as e:
    st.error(f"❌ 无法连接到后端: {e}")

# 简单的参数设置
pose_type = st.selectbox("选择动作类型", ["pushup", "pullup", "squat", "deadlift", "abworkout", "benchpress"])

# 视频处理
st.subheader("视频处理")

uploaded_file = st.file_uploader("上传视频", type=["mp4", "avi", "mov"])

if uploaded_file is not None and st.button("处理视频"):
    # 显示上传的视频
    st.video(uploaded_file.getvalue())
    
    # 准备文件上传
    video_file = BytesIO(uploaded_file.getvalue())
    files = {"file": (uploaded_file.name, video_file, "video/mp4")}

    params = {
        "pose_type": pose_type
    }

    with st.spinner("正在处理视频..."):
        try:
            start_time = time.time()
            response = requests.post(
                f"{API_BASE_URL}/process_video",
                files=files,
                params=params,
                timeout=300  # 5分钟超时
            )

            if response.status_code == 200:
                elapsed_time = time.time() - start_time
                
                # 从响应头中获取统计信息
                final_count = response.headers.get('X-Final-Count', 'N/A')
                total_frames = response.headers.get('X-Total-Frames', 'N/A')
                total_time = response.headers.get('X-Total-Time', 'N/A')
                
                st.success(f"视频处理完成! 总耗时: {elapsed_time:.2f}秒")
                
                # 显示统计信息
                st.write(f"总帧数: {total_frames}")
                st.write(f"总时长(秒): {total_time}")
                st.write(f"最终计数: {final_count}")
                
                # 直接显示处理后的视频
                st.subheader("处理后的视频")
                st.video(response.content)
            else:
                # 尝试解析错误响应
                try:
                    error_result = response.json()
                    st.error(f"视频处理失败: {error_result.get('message', '未知错误')}")
                    if 'traceback' in error_result:
                        with st.expander("详细错误信息"):
                            st.text(error_result['traceback'])
                except:
                    st.error(f"视频处理失败: {response.status_code} - {response.text}")

        except requests.exceptions.Timeout:
            st.error("请求超时，请检查后端服务是否正常运行")
        except requests.exceptions.ConnectionError:
            st.error("无法连接到后端服务，请检查服务是否启动")
        except Exception as e:
            st.error(f"请求失败: {str(e)}")

# 显示API信息
with st.expander("API端点信息"):
    st.write("""
    **可用的API端点:**
    - `POST /process_video` - 处理视频并返回完整结果
    - `GET /health` - 健康检查
    - `GET /supported_poses` - 获取支持的姿势类型
    """)

# 使用说明
with st.expander("使用说明"):
    st.write("""
    1. **启动后端**: 运行 `python main.py`
    2. **上传文件**: 选择视频文件（建议小于50MB）
    3. **设置参数**: 选择动作类型、难度模式等
    4. **点击处理**: 等待处理完成查看结果
    5. **注意事项**: 
       - 确保后端服务正常运行
       - 视频文件不宜过大
       - 处理时间取决于视频长度和复杂度
    """)

# 清理session state
if uploaded_file is None and 'current_file' in st.session_state:
    del st.session_state.current_file