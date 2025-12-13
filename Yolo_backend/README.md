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

### 3. 流式视频处理接口

**接口地址**: `POST /stream_process_video`

**功能描述**: 上传视频文件进行健身动作识别和计数，通过 Server-Sent Events (SSE) 流式返回处理过程和结果

**请求参数**:
- `file`: 视频文件（必填），支持格式：mp4, avi, mov
- `pose_type`: 动作类型（选填，默认为 "pushup"），可选值参考 [/supported_poses](file:///G:/Third_year_first_semester/SoftwareEngineering/Yolo_backend/main.py#L297-L303) 接口

**请求示例**:
```bash
curl -X POST \
  -F "file=@test_video.mp4" \
  -F "pose_type=pushup" \
  http://localhost:8000/stream_process_video
```

**响应说明**:
- 成功：返回 SSE 流，包含处理过程中的各个阶段信息
- 失败：返回JSON格式的错误信息

**SSE 事件类型**:
1. `init`: 初始化信息，包含视频基本信息
2. `frame`: 每帧处理结果，包含当前计数等信息
3. `final_stats`: 最终统计信息，包含最终计数和处理结果
4. `error`: 错误信息

**响应示例**（成功）:
```
data: {"event": "init", "data": {"message": "开始处理视频"}}

data: {"event": "init", "data": {"message": "视频上传完成", "path": "/tmp/tmpxxxxxx/input_video.mp4"}}

data: {"event": "init", "data": {"message": "视频信息获取完成", "fps": 30, "width": 1920, "height": 1080, "frame_count": 360}}

...

data: {"event": "frame", "data": {"frame_index": 10, "processed_frame_count": 5, "count": 2, "max_count": 2, "image": "base64_encoded_image"}}

...

data: {"event": "final_stats", "data": {"message": "视频处理完成", "max_count": 12, "processed_frame_count": 180, "total_time": 12.0, "output_path": "/tmp/tmpxxxxxx/output_video.mp4", "video_size": 1234567}}

data: {"event": "final_stats", "data": {"message": "视频可供下载", "download_url": "/download_processed_video?temp_id=xxxxx"}}
```

### 4. 视频处理接口

**接口地址**: `POST /process_video`

**功能描述**: 上传视频文件进行健身动作识别和计数，通过 Server-Sent Events (SSE) 流式返回处理过程和结果（与 [/stream_process_video](file:///G:/Third_year_first_semester/SoftwareEngineering/Yolo_backend/main.py#L246-L251) 功能相同）

**请求参数**:
- `file`: 视频文件（必填），支持格式：mp4, avi, mov
- `pose_type`: 动作类型（选填，默认为 "pushup"），可选值参考 [/supported_poses](file:///G:/Third_year_first_semester/SoftwareEngineering/Yolo_backend/main.py#L297-L303) 接口

**请求示例**:
```bash
curl -X POST \
  -F "file=@test_video.mp4" \
  -F "pose_type=pushup" \
  http://localhost:8000/process_video
```

### 5. 处理并保存视频接口

**接口地址**: `POST /process_and_save_video`

**功能描述**: 上传视频文件进行健身动作识别和计数，并将处理后的视频保存到 homework/homework_id/student_id 目录下

**请求参数**:
- `homework_id`: 作业ID（必填，查询参数）
- `student_id`: 学生ID（必填，查询参数）
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
```
data: {"event": "init", "data": {"message": "开始处理视频"}}

...

data: {"event": "final_stats", "data": {"message": "视频处理完成", "max_count": 12, "processed_frame_count": 180, "total_time": 12.0, "output_path": "/tmp/tmpxxxxxx/output_video.mp4", "saved_path": "homework/hw001/stu001/processed_video.mp4", "video_url": "/get_processed_video?homework_id=hw001&student_id=stu001"}}

data: {"event": "final_stats", "data": {"message": "视频可供下载", "download_url": "/download_processed_video?temp_id=xxxxx"}}
```

### 6. 获取已处理视频接口

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

### 7. 下载处理后视频接口

**接口地址**: `GET /download_processed_video`

**功能描述**: 下载处理后的视频文件

**请求参数**:
- `temp_id`: 临时ID（必填，查询参数），由 [/stream_process_video](file:///G:/Third_year_first_semester/SoftwareEngineering/Yolo_backend/main.py#L246-L251) 或 [/process_video](file:///G:/Third_year_first_semester/SoftwareEngineering/Yolo_backend/main.py#L280-L286) 接口返回

**请求示例**:
```bash
curl -X GET "http://localhost:8000/download_processed_video?temp_id=temp_xxxxx" \
  --output processed_video.mp4
```

### 8. 删除作业接口

**接口地址**: `DELETE /delete_homework`

**功能描述**: 删除指定作业ID下的所有视频文件

**请求参数**:
- `homework_id`: 作业ID（必填，查询参数）

**请求示例**:
```bash
curl -X DELETE "http://localhost:8000/delete_homework?homework_id=hw001"
```

**响应示例**（成功）:
```
{
  "status": "success",
  "message": "作业 hw001 的所有视频已成功删除"
}
```

**响应示例**（失败）:
```
{
  "detail": "作业ID hw001 不存在"
}
```

### 9. 查询记录接口

**接口地址**: `GET /query_records`

**功能描述**: 根据作业ID、学生ID和动作类型查询反馈记录

**请求参数**:
- `homework_id`: 作业ID（必填，查询参数）
- `student_id`: 学生ID（必填，查询参数）
- `pose_type`: 动作类型（可选，查询参数）

**请求示例**:
```bash
curl -X GET "http://localhost:8000/query_records?homework_id=hw001&student_id=stu001&pose_type=pushup"
```

**响应示例**（成功）:
```
[
  {
    "id": 1,
    "homework_id": "hw001",
    "student_id": "stu001",
    "pose_type": "pushup",
    "processed_video_path": "homework/hw001/stu001/processed_video.mp4",
    "total_count": 12,
    "correct_count": 10,
    "incorrect_count": 2,
    "feedback_json": "{\"events\":[],\"performance\":{\"total_count\":12,\"correct_count\":10,\"incorrect_count\":2,\"accuracy_rate\":83.33}}",
    "video_duration": 12.0,
    "uploaded_at": "2023-01-01 12:00:00"
  }
]
```

### 10. 查询所有记录接口

**接口地址**: `GET /query_all_records`

**功能描述**: 查询所有反馈记录

**请求示例**:
```bash
curl -X GET "http://localhost:8000/query_all_records"
```

**响应示例**（成功）:
```
[
  {
    "id": 1,
    "homework_id": "hw001",
    "student_id": "stu001",
    "pose_type": "pushup",
    "processed_video_path": "homework/hw001/stu001/processed_video.mp4",
    "total_count": 12,
    "correct_count": 10,
    "incorrect_count": 2,
    "feedback_json": "{\"events\":[],\"performance\":{\"total_count\":12,\"correct_count\":10,\"incorrect_count\":2,\"accuracy_rate\":83.33}}",
    "video_duration": 12.0,
    "uploaded_at": "2023-01-01 12:00:00"
  },
  ...
]
```

### 11. 获取学生所有记录接口

**接口地址**: `GET /api/student/all-records/{student_id}`

**功能描述**: 获取指定学生的所有健身动作记录，按上传时间倒序排列

**请求参数**:
- `student_id`: 学生ID（路径参数，必填）

**请求示例**:
```bash
curl -X GET http://localhost:8000/api/student/all-records/stu001
```

**响应示例**:
```
[
  {
    "id": 12,
    "homework_id": "hw001",
    "student_id": "stu001",
    "pose_type": "pushup",
    "uploaded_at": "2023-12-01 10:30:00",
    "original_video_path": null,
    "processed_video_path": "homework/hw001/stu001/processed_video.mp4",
    "total_count": 15,
    "correct_count": 12,
    "incorrect_count": 3,
    "feedback_json": "{\"events\":[],\"performance\":{\"total_count\":15,\"correct_count\":12,\"incorrect_count\":3,\"accuracy_rate\":80.0},\"video_info\":{\"total_frames\":180,\"processed_frames\":90,\"fps\":30,\"duration\":6.0}}",
    "video_duration": 6.0
  },
  {
    "id": 8,
    "homework_id": "hw002",
    "student_id": "stu001",
    "pose_type": "squat",
    "uploaded_at": "2023-11-28 14:15:00",
    "original_video_path": null,
    "processed_video_path": "homework/hw002/stu001/processed_video.mp4",
    "total_count": 20,
    "correct_count": 18,
    "incorrect_count": 2,
    "feedback_json": "{\"events\":[],\"performance\":{\"total_count\":20,\"correct_count\":18,\"incorrect_count\":2,\"accuracy_rate\":90.0},\"video_info\":{\"total_frames\":240,\"processed_frames\":120,\"fps\":30,\"duration\":8.0}}",
    "video_duration": 8.0
  }
]
```

**响应字段说明**:
- `id`: 记录ID
- `homework_id`: 作业ID
- `student_id`: 学生ID
- `pose_type`: 动作类型
- `uploaded_at`: 视频上传时间
- `original_video_path`: 原始视频路径（可能为空）
- `processed_video_path`: 处理后视频路径
- `total_count`: 总动作次数
- `correct_count`: 正确动作次数
- `incorrect_count`: 错误动作次数
- `feedback_json`: AI反馈数据（JSON格式）
- `video_duration`: 视频时长（秒）

**错误响应**:
- 404: 未找到该学生记录
- 500: 服务器内部错误

### 12. 获取记录详情接口

**接口地址**: `GET /get_record_details/{record_id}`

**功能描述**: 获取指定记录的详细信息

**请求参数**:
- `record_id`: 记录ID（必填，路径参数）

**请求示例**:
```bash
curl -X GET "http://localhost:8000/get_record_details/1"
```

**响应示例**（成功）:
```
{
  "id": 1,
  "homework_id": "hw001",
  "student_id": "stu001",
  "pose_type": "pushup",
  "processed_video_path": "homework/hw001/stu001/processed_video.mp4",
  "total_count": 12,
  "correct_count": 10,
  "incorrect_count": 2,
  "feedback_json": "{\"events\":[],\"performance\":{\"total_count\":12,\"correct_count\":10,\"incorrect_count\":2,\"accuracy_rate\":83.33}}",
  "video_duration": 12.0,
  "uploaded_at": "2023-01-01 12:00:00"
}
```

## 前端调用指南

### JavaScript (使用 fetch API) 示例

```
// 设置基础URL
const BASE_URL = 'http://localhost:8000';

// 上传视频并处理 (使用SSE)
const streamProcessVideo = async (file, poseType = 'pushup') => {
  const formData = new FormData();
  formData.append('file', file);
  formData.append('pose_type', poseType);

  try {
    const response = await fetch(`${BASE_URL}/stream_process_video`, {
      method: 'POST',
      body: formData
    });

    if (response.ok && response.body) {
      const reader = response.body.getReader();
      const decoder = new TextDecoder();

      let downloadUrl = null;
      
      while (true) {
        const { value, done } = await reader.read();
        if (done) break;

        const chunk = decoder.decode(value, { stream: true });
        const lines = chunk.split('\n\n');
        
        for (const line of lines) {
          if (line.startsWith('data: ')) {
            try {
              const eventData = JSON.parse(line.slice(6));
              const { event, data } = eventData;
              
              switch (event) {
                case 'init':
                  console.log('初始化:', data.message);
                  break;
                case 'frame':
                  console.log(`处理帧 ${data.frame_index}, 当前计数: ${data.count}`);
                  break;
                case 'final_stats':
                  if (data.download_url) {
                    downloadUrl = data.download_url;
                    console.log('视频可供下载:', downloadUrl);
                  } else {
                    console.log('最终统计:', data);
                  }
                  break;
                case 'error':
                  console.error('处理出错:', data.message);
                  break;
              }
            } catch (e) {
              console.error('解析事件数据出错:', e);
            }
          }
        }
      }
      
      return { success: true, downloadUrl };
    } else {
      throw new Error('请求失败');
    }
  } catch (error) {
    console.error('处理视频时发生错误:', error);
    throw error;
  }
};

// 下载处理后的视频
const downloadProcessedVideo = async (downloadUrl, filename = 'processed_video.mp4') => {
  try {
    const response = await fetch(BASE_URL + downloadUrl);
    if (response.ok) {
      const blob = await response.blob();
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = filename;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      URL.revokeObjectURL(url);
    } else {
      throw new Error('下载失败');
    }
  } catch (error) {
    console.error('下载视频时发生错误:', error);
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

    if (response.ok && response.body) {
      const reader = response.body.getReader();
      const decoder = new TextDecoder();

      let result = null;
      
      while (true) {
        const { value, done } = await reader.read();
        if (done) break;

        const chunk = decoder.decode(value, { stream: true });
        const lines = chunk.split('\n\n');
        
        for (const line of lines) {
          if (line.startsWith('data: ')) {
            try {
              const eventData = JSON.parse(line.slice(6));
              const { event, data } = eventData;
              
              if (event === 'final_stats' && data.video_url) {
                result = data;
              }
            } catch (e) {
              console.error('解析事件数据出错:', e);
            }
          }
        }
      }
      
      return result;
    } else {
      throw new Error('请求失败');
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

// 查询记录
const queryRecords = async (homeworkId, studentId, poseType = null) => {
  try {
    let url = `${BASE_URL}/query_records?homework_id=${encodeURIComponent(homeworkId)}&student_id=${encodeURIComponent(studentId)}`;
    if (poseType) {
      url += `&pose_type=${encodeURIComponent(poseType)}`;
    }
    
    const response = await fetch(url);
    if (response.ok) {
      const result = await response.json();
      return result;
    } else {
      const errorData = await response.json();
      throw new Error(errorData.detail);
    }
  } catch (error) {
    console.error('查询记录时发生错误:', error);
    throw error;
  }
};

// 使用示例
const fileInput = document.getElementById('videoInput');
fileInput.addEventListener('change', async (event) => {
  const file = event.target.files[0];
  if (file) {
    try {
      // 流式处理视频
      const result = await streamProcessVideo(file, 'pushup');
      console.log('处理结果:', result);
      
      if (result.downloadUrl) {
        // 下载处理后的视频
        await downloadProcessedVideo(result.downloadUrl);
      }
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

// 上传视频并处理 (使用SSE)
const streamProcessVideoWithAxios = async (file, poseType = 'pushup') => {
  const formData = new FormData();
  formData.append('file', file);
  formData.append('pose_type', poseType);

  try {
    const response = await axios.post(
      `${BASE_URL}/stream_process_video`,
      formData,
      {
        responseType: 'stream'
      }
    );
    
    // 处理 SSE 流
    // 注意：Axios 不直接支持 SSE，需要手动处理流数据
    
    return response;
  } catch (error) {
    if (error.response && error.response.data) {
      throw new Error(JSON.stringify(error.response.data));
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

### SSE 流式响应
当视频处理时，后端会通过 SSE 流式返回多个事件：

1. `init` 事件：初始化信息，包含视频基本信息
2. `frame` 事件：每帧处理结果，包含当前计数等信息
3. `final_stats` 事件：最终统计信息，包含最终计数和处理结果
4. `error` 事件：错误信息

### 成功响应
当视频处理成功时，后端会返回以下数据：

1. HTTP 状态码: `200 OK`
2. 响应体: SSE 流式数据，包含处理过程和结果

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
4. 调用 [/stream_process_video](file:///G:/Third_year_first_semester/SoftwareEngineering/Yolo_backend/main.py#L246-L251) 接口处理视频（推荐使用流式处理）
5. 或者调用 [/process_and_save_video](file:///G:/Third_year_first_semester/SoftwareEngineering/Yolo_backend/main.py#L310-L327) 接口处理视频并保存到指定目录
6. 使用 [/get_processed_video](file:///G:/Third_year_first_semester/SoftwareEngineering/Yolo_backend/main.py#L335-L345) 接口获取已保存的处理视频
7. 如需删除作业，可调用 [/delete_homework](file:///G:/Third_year_first_semester/SoftwareEngineering/Yolo_backend/main.py#L347-L361) 接口删除指定作业的所有视频
8. 使用 [/query_records](file:///G:/Third_year_first_semester/SoftwareEngineering/Yolo_backend/main.py#L415-L441) 查询处理记录
9. 使用 [/api/student/all-records/{student_id}](file:///G:/Third_year_first_semester/SoftwareEngineering/Yolo_backend/main.py#L832-L870) 获取学生所有记录用于个性化分析

## 注意事项

1. 视频文件大小建议控制在合理范围内，过大的文件可能导致处理超时
2. 处理时间与视频长度和复杂度有关，请设置合适的请求超时时间（建议5分钟）
3. 确保上传的视频中人物清晰可见，背景不要太复杂
4. 当前版本仅支持单人动作识别
5. 建议在弱光环境下拍摄时注意光线均匀，避免逆光
6. 前端在接收视频文件时需要正确处理 Blob 对象，并使用 `URL.createObjectURL()` 创建可播放的 URL
7. 由于视频处理需要一定时间，建议在 UI 上提供加载状态指示
8. 使用 [/process_and_save_video](file:///G:/Third_year_first_semester/SoftwareEngineering/Yolo_backend/main.py#L310-L327) 接口时，相同 homework_id 和 student_id 的视频会被新上传的视频覆盖
9. 使用 [/delete_homework](file:///G:/Third_year_first_semester/SoftwareEngineering/Yolo_backend/main.py#L347-L361) 接口时请谨慎操作，删除操作不可恢复
10. 处理完视频后，可以通过 [/query_records](file:///G:/Third_year_first_semester/SoftwareEngineering/Yolo_backend/main.py#L415-L441) 接口查询历史记录