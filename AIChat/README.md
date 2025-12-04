# AI聊天应用

一个基于Flask的AI聊天应用后端，支持多用户会话管理和历史记录持久化。

## 技术栈

- 后端：Python + Flask
- 数据库：SQLite
- 前端：HTML + JavaScript
- AI模型：支持多种大语言模型（通义千问、文心一言、Moonshot）

## 接口文档

### 1. 获取用户会话列表

**URL**: `GET /api/sessions`

**功能**: 获取指定用户的所有会话列表

**参数**:
- `user_id` (query string): 用户ID

**示例请求**:
```
GET /api/sessions?user_id=user123
```

**成功响应**:
```json
{
  "success": true,
  "data": [
    {
      "session_id": 1,
      "title": "新对话-2023-10-20 14:30",
      "model": "Qwen"
    },
    {
      "session_id": 2,
      "title": "健身计划讨论",
      "model": "ERNIE"
    }
  ]
}
```

### 2. 为用户创建新会话

**URL**: `POST /api/sessions`

**功能**: 为指定用户创建一个新的会话

**参数** (JSON):
- `user_id` (string): 用户ID
- `model` (string, 可选): 使用的AI模型，默认为"Qwen"

**示例请求**:
```
POST /api/sessions
Content-Type: application/json

{
  "user_id": "user123",
  "model": "Qwen"
}
```

**成功响应**:
```json
{
  "success": true,
  "data": {
    "session_id": 1,
    "session": {
      "session_id": 1,
      "messages": [
        {
          "role": "system",
          "content": "你是一个体育健身数智化教学平台的AI助手..."
        },
        {
          "role": "assistant",
          "content": "🏋️‍♂️ 欢迎使用体育健身数智化教学平台！..."
        }
      ],
      "model": "Qwen",
      "title": "新对话-2023-10-20 14:30"
    },
    "welcome_message": {
      "role": "assistant",
      "content": "🏋️‍♂️ 欢迎使用体育健身数智化教学平台！..."
    }
  }
}
```

### 3. 获取用户的最新会话

**URL**: `GET /api/sessions/user/<user_id>`

**功能**: 获取指定用户的最新会话

**参数**:
- `user_id` (URL参数): 用户ID

**示例请求**:
```
GET /api/sessions/user/user123
```

**成功响应**:
```json
{
  "success": true,
  "data": {
    "session_id": 1,
    "messages": [
      {
        "role": "system",
        "content": "你是一个体育健身数智化教学平台的AI助手..."
      },
      {
        "role": "assistant",
        "content": "🏋️‍♂️ 欢迎使用体育健身数智化教学平台！..."
      }
    ],
    "model": "Qwen",
    "title": "新对话-2023-10-20 14:30"
  }
}
```

**无会话时的响应**:
```json
{
  "success": false,
  "data": null
}
```

### 4. 获取指定会话详情

**URL**: `GET /api/sessions/<session_id>`

**功能**: 获取指定会话的详细信息，包括所有历史消息

**参数**:
- `session_id` (URL参数): 会话ID

**示例请求**:
```
GET /api/sessions/1
```

**成功响应**:
```json
{
  "success": true,
  "data": {
    "session_id": 1,
    "messages": [
      {
        "role": "system",
        "content": "你是一个体育健身数智化教学平台的AI助手...",
        "model": "Qwen"
      },
      {
        "role": "assistant",
        "content": "🏋️‍♂️ 欢迎使用体育健身数智化教学平台！...",
        "model": "Qwen"
      }
    ],
    "model": "Qwen",
    "title": "新对话-2023-10-20 14:30"
  }
}
```

### 5. 删除会话

**URL**: `DELETE /api/sessions/<session_id>`

**功能**: 删除指定的会话及其所有消息

**参数**:
- `session_id` (URL参数): 会话ID

**示例请求**:
```
DELETE /api/sessions/1
```

**成功响应**:
```json
{
  "success": true,
  "message": "会话已删除"
}
```

### 6. 发送消息

**URL**: `POST /api/sessions/<session_id>/messages`

**功能**: 向指定会话发送消息并获取AI回复

**参数** (JSON):
- `session_id` (URL参数): 会话ID
- `message` (string): 用户发送的消息内容
- `model` (string, 可选): 指定使用的AI模型，如果不指定则使用会话默认模型

**示例请求**:
```
POST /api/sessions/1/messages
Content-Type: application/json

{
  "message": "我想制定一个健身计划",
  "model": "ERNIE"
}
```

**成功响应**:
```json
{
  "success": true,
  "data": {
    "session": {
      "session_id": 1,
      "messages": [
        {
          "role": "system",
          "content": "你是一个体育健身数智化教学平台的AI助手...",
          "model": "Qwen"
        },
        {
          "role": "assistant",
          "content": "🏋️‍♂️ 欢迎使用体育健身数智化教学平台！...",
          "model": "Qwen"
        },
        {
          "role": "user",
          "content": "我想制定一个健身计划",
          "model": "ERNIE"
        },
        {
          "role": "assistant",
          "content": "好的，我很乐意帮您制定健身计划。首先，请告诉我您的身高、体重和健身目标。",
          "model": "ERNIE"
        }
      ],
      "model": "Qwen",
      "title": "我想制定一个健身计划"
    },
    "response": "好的，我很乐意帮您制定健身计划。首先，请告诉我您的身高、体重和健身目标。"
  }
}
```

### 7. 清空会话消息

**URL**: `POST /api/sessions/<session_id>/clear`

**功能**: 清空指定会话的所有消息，但保留会话本身

**参数**:
- `session_id` (URL参数): 会话ID

**示例请求**:
```
POST /api/sessions/1/clear
```

**成功响应**:
```json
{
  "success": true,
  "message": "会话已清空"
}
```

### 8. 导出会话

**URL**: `GET /api/sessions/<session_id>/export`

**功能**: 将指定会话导出为Markdown文件

**参数**:
- `session_id` (URL参数): 会话ID

**示例请求**:
```
GET /api/sessions/1/export
```

**成功响应**:
- 返回一个Markdown文件下载

### 9. 获取支持的模型列表

**URL**: `GET /api/models`

**功能**: 获取系统支持的所有AI模型列表

**示例请求**:
```
GET /api/models
```

**成功响应**:
```json
{
  "success": true,
  "data": [
    "Qwen",
    "ERNIE",
    "Moonshot"
  ]
}
```

## 使用说明

1. 启动后端服务:
   ```bash
   python backend.py
   ```

2. 在浏览器中打开 `test_frontend.html` 进行测试

3. 输入用户ID，点击"加载会话"开始使用

## 数据库设计

### 会话表 (sessions)
| 字段名 | 类型 | 说明 |
|--------|------|------|
| id | INTEGER PRIMARY KEY AUTOINCREMENT | 会话ID，自增主键 |
| user_id | TEXT NOT NULL | 用户ID |
| title | TEXT NOT NULL | 会话标题 |
| model | TEXT | 默认使用的AI模型（可选） |
| created_at | TIMESTAMP | 创建时间 |
| updated_at | TIMESTAMP | 更新时间 |

### 消息表 (messages)
| 字段名 | 类型 | 说明 |
|--------|------|------|
| id | INTEGER PRIMARY KEY AUTOINCREMENT | 消息ID，自增主键 |
| session_id | INTEGER NOT NULL | 会话ID，外键关联sessions表 |
| role | TEXT NOT NULL | 消息角色 (system/user/assistant) |
| content | TEXT NOT NULL | 消息内容 |
| model | TEXT | 使用的AI模型（可选） |
| timestamp | TIMESTAMP | 时间戳 |

## 注意事项

1. 数据库会在首次启动时自动创建
2. 每个用户可以拥有多个会话
3. 会话和消息ID均为自增整数
4. 用户通过用户ID管理自己的会话
5. 会话与模型不强绑定，同一会话中可以使用不同模型进行对话
6. 如果发送消息时未指定模型，则使用会话创建时指定的默认模型
7. 前端应在用户首次访问时显式调用创建会话接口，而不是依赖后端自动创建