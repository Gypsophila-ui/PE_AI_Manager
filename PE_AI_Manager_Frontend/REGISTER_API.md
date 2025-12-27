# 注册页面API接入说明

## 📋 概述
注册页面已完成对后端 `/new_student` 和 `/new_teacher` 接口的接入，支持学生和教师两种角色的注册功能。

## ⚠️ 重要：API参数命名规则

### 参数命名约定
后端API使用特殊的参数命名规则，所有参数按照顺序命名为：
- `first` - 第一个参数（通常是ID）
- `second` - 第二个参数（通常是密码）
- `third` - 第三个参数（通常是姓名）
- `fourth` - 第四个参数（通常是性别）
- `fifth` - 第五个参数（根据接口不同，可能是专业、职称等）
- `sixth` - 第六个参数（通常是学院）
- `seventh` - 第七个参数（通常是系/部门）

### 各接口参数映射

#### 学生注册 `/User/new_student`
```javascript
{
  first: id,           // 学号
  second: password,    // 密码
  third: name,         // 姓名
  fourth: gender,      // 性别
  fifth: major,        // 专业
  sixth: college,      // 学院
  seventh: department  // 系/部门
}
```

#### 教师注册 `/User/new_teacher`
```javascript
{
  first: id,           // 教师ID
  second: password,    // 密码
  third: name,         // 姓名
  fourth: gender,      // 性别
  fifth: title,        // 职称
  sixth: college,      // 学院
  seventh: department  // 系/部门
}
```

#### 学生登录 `/User/login_student`
```javascript
{
  first: student_id,   // 学号
  second: password     // 密码（SHA-256加密）
}
```

#### 教师登录 `/User/login_teacher`
```javascript
{
  first: teacher_id,   // 教师ID
  second: password     // 密码（SHA-256加密）
}
```

#### 学生修改密码 `/User/change_student_password`
```javascript
{
  first: id,           // 学号
  second: old_password,  // 旧密码（SHA-256加密）
  third: new_password    // 新密码（SHA-256加密）
}
```

#### 教师修改密码 `/User/change_teacher_password`
```javascript
{
  first: id,           // 教师ID
  second: old_password,  // 旧密码（SHA-256加密）
  third: new_password    // 新密码（SHA-256加密）
}
```

## ⚠️ 重要：CORS问题已解决

### 问题描述
如果遇到以下CORS错误：
```
Access to XMLHttpRequest at 'http://localhost:5555/new_student' from origin 'http://localhost:5173'
has been blocked by CORS policy: Response to preflight request doesn't pass access control check:
No 'Access-Control-Allow-Origin' header is present on the requested resource.
```

或者遇到404错误：
```
POST http://localhost:5173/new_student 404 (Not Found)
```

### 根本原因
**API路径配置不正确**：
- 后端API使用不同的路径结构：
  - AI聊天服务使用 `/api` 前缀（如 `/api/sessions`）
  - 用户认证服务不使用前缀（如 `/User/new_student`）
- 前端需要正确配置代理规则，将不同类型的请求转发到正确的后端路径

### 解决方案
已通过Vite代理配置解决CORS问题：

1. **修改了 `vite.config.js`**：
     - 配置 `/User`、`/Class`、`/Course`、`/Homework` 路径的代理规则（用于用户认证和课程管理）
     - AI聊天服务不使用代理，直接请求后端
     - 只有用户认证和课程管理API请求会被代理，静态资源和根路径请求不会被代理

2. **修改了 `src/services/axios.js`**：
   - 将 `apiClient` 的 `baseURL` 改为空字符串
   - 使用相对路径，让请求通过Vite代理转发

3. **修改了 `src/services/auth.js`**：
   - 为所有API调用添加正确的后端路径前缀：
     - `/User/new_student` - 学生注册
     - `/User/new_teacher` - 教师注册
     - `/User/login_student` - 学生登录
     - `/User/login_teacher` - 教师登录
     - `/User/change_student_password` - 学生修改密码
     - `/User/change_teacher_password` - 教师修改密码

4. **修改了 `src/services/aiChat.js`**：
   - 将 `aiChatClient` 的 `baseURL` 改为 `http://localhost:5555/api`
   - AI聊天服务直接请求后端，不使用Vite代理

5. **重启开发服务器**：
   ```bash
   # 停止当前服务器（Ctrl+C）
   # 然后重新启动
   npm run dev
   ```

### 工作原理
 
 #### 用户认证请求（注册、登录等）
 ```
 前端调用: /User/new_student
     ↓
 Vite开发服务器 (localhost:5173)
     ↓
 匹配代理规则: /User → http://localhost:5555
     ↓
 代理转发 → http://localhost:5555/User/new_student
     ↓
 后端服务器 (localhost:5555)
 ```
 
 #### AI聊天请求
 ```
 前端调用: http://localhost:5555/api/sessions
     ↓
 直接请求后端服务器 (localhost:5555)
     ↓
 后端服务器 (localhost:5555)
 ```

### 后端API路径结构

#### 用户认证API（无前缀）
- `/User/new_student` - 学生注册
- `/User/new_teacher` - 教师注册
- `/User/login_student` - 学生登录
- `/User/login_teacher` - 教师登录
- `/User/change_student_password` - 学生修改密码
- `/User/change_teacher_password` - 教师修改密码
- `/User/get_student_info` - 获取学生信息
- `/User/get_teacher_info` - 获取教师信息

#### 课程管理API（无前缀）
- `/Class/*` - 课程相关接口
- `/Course/*` - 课程相关接口
- `/Homework/*` - 作业相关接口

#### AI聊天API（带/api前缀）
- `/api/sessions` - 会话管理
- `/api/sessions/{id}/messages` - 消息发送
- `/api/sessions/{id}/clear` - 清空会话
- `/api/sessions/{id}/export` - 导出会话
- `/api/models` - 模型列表

### 验证方法
重启开发服务器后，在浏览器开发者工具的Network标签中检查：

#### 用户认证请求
- 请求URL应该是 `http://localhost:5173/User/new_student`
- 请求应该成功返回200状态码
- 不会出现CORS错误或404错误

#### AI聊天请求
- 请求URL应该是 `http://localhost:5555/api/sessions`
- 请求应该成功返回200状态码
- 不会出现CORS错误或404错误

## 🔌 API接口信息

### 学生注册接口
- **接口路径**: `/new_student`
- **请求方法**: POST
- **请求参数**:
  - `id` (必填): 学号
  - `password` (必填): 密码（SHA-256加密）
  - `name` (必填): 姓名
  - `gender` (必填): 性别
  - `major` (必填): 专业
  - `college` (必填): 学院
  - `department` (必填): 系别

### 教师注册接口
- **接口路径**: `/new_teacher`
- **请求方法**: POST
- **请求参数**:
  - `id` (必填): 工号
  - `password` (必填): 密码（SHA-256加密）
  - `name` (必填): 姓名
  - `gender` (必填): 性别
  - `title` (必填): 职称
  - `college` (必填): 学院
  - `department` (必填): 系别

## 📁 文件结构

### 1. API服务层 (`src/services/auth.js`)
- **函数**: `registerStudent(id, password, name, gender, major, college, department)`
- **函数**: `registerTeacher(id, password, name, gender, title, college, department)`
- **功能**: 
  - 调用后端注册接口
  - 处理请求和响应
  - 详细的错误日志记录
  - 统一的错误处理

### 2. 注册页面 (`src/pages/UserRegister.vue`)
- **功能**:
  - 角色选择（学生/教师）
  - 表单验证
  - 根据角色调用不同的注册API
  - 注册成功后跳转到登录页
  - 详细的流程日志记录

### 3. Axios配置 (`src/services/axios.js`)
- **配置**:
  - 基础URL: `http://localhost:5000`
  - 超时时间: 10000ms
  - 请求拦截器: 自动添加token
  - 响应拦截器: 统一错误处理

## 🔐 安全特性

1. **密码加密**: 使用SHA-256算法对密码进行加密后传输
2. **表单验证**: 
   - 必填字段检查
   - 密码一致性验证
   - 角色特定字段验证
3. **错误处理**: 详细的错误信息和日志记录

## 📊 请求流程

```
用户填写表单
    ↓
表单验证
    ↓
密码加密 (SHA-256)
    ↓
调用注册API (/new_student 或 /new_teacher)
    ↓
后端处理
    ↓
返回结果
    ↓
成功 → 跳转登录页
失败 → 显示错误信息
```

## 🐛 调试日志

系统已添加详细的日志记录，包括：

### API调用日志
- 📝 请求URL和数据
- ✅ 成功响应数据
- ❌ 错误详情（状态码、响应数据、请求配置）

### 注册流程日志
- 🚀 开始注册流程
- 📤 准备发送的请求数据
- 📞 调用的API端点
- 📥 API返回结果
- ✅ 注册成功
- ❌ 注册失败
- 🏁 流程结束

## 🔍 故障排查

### 常见问题

1. **注册失败 - 网络错误**
   - 检查后端服务是否运行在 `http://localhost:5555`
   - 检查网络连接
   - 查看浏览器控制台日志

2. **注册失败 - 400错误**
   - 检查必填字段是否完整
   - 检查数据格式是否正确
   - 查看错误响应详情

3. **注册失败 - 409错误**
   - 用户ID已存在
   - 提示用户使用其他ID

4. **注册失败 - 500错误**
   - 后端服务器错误
   - 查看后端日志
   - 联系系统管理员

## ✅ 测试建议

1. **学生注册测试**:
   - 填写完整的表单信息
   - 验证密码一致性
   - 测试必填字段验证
   - 测试重复ID注册

2. **教师注册测试**:
   - 填写完整的表单信息
   - 验证职称字段
   - 测试必填字段验证
   - 测试重复ID注册

3. **错误处理测试**:
   - 测试网络断开情况
   - 测试服务器错误情况
   - 测试数据格式错误

## 📝 注意事项

1. 后端服务必须运行在 `http://localhost:5000`
2. 所有密码传输前都会进行SHA-256加密
3. 用户ID（学号/工号）必须唯一
4. 注册成功后会自动跳转到登录页
5. 所有操作都有详细的日志记录，便于调试

## 🎯 完成状态

- ✅ API接口接入完成
- ✅ 学生注册功能实现
- ✅ 教师注册功能实现
- ✅ 表单验证实现
- ✅ 错误处理实现
- ✅ 日志记录增强
- ✅ 密码加密实现
- ✅ 响应式UI设计
