# PE_AI_Manager
同济大学软件工程课程设计--高校体育智慧管理系统

## 项目简介

本项目是一个面向高校体育教学的智慧课堂平台，利用计算机视觉和人工智能技术实现智能化的体育教学管理。系统集成了YOLOv8动作识别、AI智能对话、课程管理、作业管理等功能，为教师和学生提供全面的体育教学支持。

## 技术栈

### 后端
- **主后端**: C# (.NET Core) - 运行在 `localhost:5000`
- **动作识别后端**: Python (FastAPI + YOLOv8) - 运行在 `localhost:8000`
- **AI聊天后端**: Python (Flask + OpenAI) - 运行在 `localhost:5555`
- **数据库**: Oracle

### 前端
- **框架**: Vue 3
- **UI组件**: Element Plus
- **构建工具**: Vite
- **样式**: Tailwind CSS
- **HTTP客户端**: Axios

### AI技术
- **动作识别**: YOLOv8-pose (人体姿态估计)
- **智能对话**: OpenAI GPT

## 项目结构

```
PE_AI_Manager/
├── SE_PE_AI_Manager/          # C# 主后端
│   ├── Controllers/           # 控制器
│   ├── Operation/             # 业务逻辑
│   ├── Basic/                 # 基础配置
│   └── Test/                  # 测试文件
├── Yolo_backend/              # 动作识别后端 (Python)
│   ├── ai_gym.py             # 动作识别核心
│   ├── main.py               # FastAPI主程序
│   └── start.py              # 启动脚本
├── AIChat/                    # AI聊天后端 (Python)
│   ├── chat_module.py        # 聊天模块
│   ├── main.py               # Flask主程序
│   └── start.py              # 启动脚本
├── PE_AI_Manager_Frontend/    # Vue前端
│   ├── src/
│   │   ├── pages/           # 页面组件
│   │   ├── components/      # 通用组件
│   │   ├── services/        # API服务
│   │   └── router/          # 路由配置
│   └── package.json
├── sql/                       # Oracle数据库脚本
├── 答辩文档/                  # 项目文档
└── pic/                       # 系统架构图
```

## 快速开始

### 环境要求

- Node.js >= 20.19.0 或 >= 22.12.0
- Python 3.x
- .NET Core SDK
- Oracle Database

### 安装步骤

#### 1. 数据库配置

执行 `sql/` 目录下的SQL脚本，按顺序创建表空间和表：
- 新建表空间.txt
- 新建数据库用户.txt
- 新建学生用户表.txt
- 新建教师用户表.txt
- 新建课程表.txt
- 新建学生课程关系表.txt
- 新建作业信息表.txt
- 新建作业提交表.txt
- 新建教学内容表.txt
- 新建作业AI类型表.txt
- 新建学生成绩视图.txt

#### 2. 启动主后端 (C#)

```bash
cd SE_PE_AI_Manager
dotnet run
```

服务将运行在 `http://localhost:5000`

#### 3. 启动动作识别后端 (Python)

```bash
cd Yolo_backend
pip install -r requirements.txt
python start.py
```

服务将运行在 `http://localhost:8000`

#### 4. 启动AI聊天后端 (Python)

```bash
cd AIChat
pip install -r requirements.txt
python start.py
```

服务将运行在 `http://localhost:5555`

#### 5. 启动前端 (Vue)

```bash
cd PE_AI_Manager_Frontend
npm install
npm run dev
```

前端将运行在开发服务器（默认端口由Vite配置）

## 主要功能

### 教师功能
- 课程管理：创建、编辑、删除课程
- 学生管理：查看课程学生列表
- 作业管理：发布作业、查看提交、批改作业
- 教学视频：上传和管理教学视频
- 成绩管理：查看和管理学生成绩

### 学生功能
- 课程浏览：查看已选课程
- 作业提交：上传作业视频
- 提交历史：查看历史提交记录
- 教学视频：观看教学视频
- AI助手：获取学习建议和指导

### AI功能
- **动作识别**：支持俯卧撑、深蹲、硬拉等动作的自动识别和计数
- **智能反馈**：实时分析动作标准度，提供改进建议
- **AI聊天**：基于GPT的智能问答，解答体育相关问题

## 支持的动作类型

| 动作类型 | 英文名称 | 说明 |
|---------|---------|------|
| 俯卧撑 | pushup | 手臂弯曲和伸直的运动 |
| 深蹲 | squat | 下肢力量训练动作 |
| 硬拉 | deadlift | 后背及下肢综合训练动作 |

## API文档

### 主后端API (C#)
- 用户认证：登录、注册
- 课程管理：CRUD操作
- 作业管理：发布、提交、批改
- 成绩管理：查询、统计

### 动作识别API (YOLOv8)
详细API文档请参考：[Yolo_backend/README.md](Yolo_backend/README.md)

主要接口：
- `POST /stream_process_video` - 流式视频处理
- `POST /process_and_save_video` - 处理并保存视频
- `GET /get_processed_video` - 获取已处理视频
- `GET /api/student/all-records/{student_id}` - 获取学生所有记录

### AI聊天API (Flask)
- 智能对话接口
- 报告生成接口

## 系统架构

系统采用前后端分离架构，主要包含以下组件：

1. **前端层**：Vue 3单页应用，提供用户界面
2. **API网关层**：C#后端，处理业务逻辑和用户认证
3. **AI服务层**：
   - 动作识别服务：YOLOv8姿态识别
   - AI聊天服务：基于OpenAI的智能对话
4. **数据层**：Oracle数据库存储业务数据

详细的系统架构图请参考：[pic/系统架构图.jpg](pic/系统架构图.jpg)

## 开发指南

### 前端开发

```bash
cd PE_AI_Manager_Frontend
npm run dev      # 启动开发服务器
npm run build    # 构建生产版本
npm run lint     # 代码检查
```

### 后端开发

#### C#后端
```bash
cd SE_PE_AI_Manager
dotnet build     # 编译项目
dotnet run       # 运行项目
```

#### Python后端
```bash
# 动作识别后端
cd Yolo_backend
python start.py

# AI聊天后端
cd AIChat
python start.py
```

## 测试数据

项目包含测试数据生成脚本：
- `测试数据生成/std_student.py` - 生成学生测试数据
- `测试数据生成/std_teacher.py` - 生成教师测试数据

## 注意事项

1. **启动顺序**：建议按照以下顺序启动服务
   - Oracle数据库
   - C#主后端 (localhost:5000)
   - 动作识别后端 (localhost:8000)
   - AI聊天后端 (localhost:5555)
   - Vue前端

2. **端口配置**：确保以下端口未被占用
   - 5000: C#主后端
   - 8000: 动作识别后端
   - 5555: AI聊天后端
   - 前端端口：由Vite配置决定

3. **文件上传**：教学视频和作业视频请上传至文件服务器（参考 `文件端口.md`）

4. **AI服务**：AI聊天服务需要配置OpenAI API密钥

## 文档资源

- [前端README](PE_AI_Manager_Frontend/README.md)
- [YOLO后端API文档](Yolo_backend/README.md)
- [文件端口配置](文件端口.md)
- [系统架构图](pic/系统架构图.jpg)
- [数据流图](pic/数据流图.jpg)

## 许可证

本项目为课程设计项目，仅供学习参考使用。
