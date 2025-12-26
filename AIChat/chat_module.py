import os
import time
import openai
from typing import Dict, List
import tempfile
import json
from flask import Flask, request, jsonify, send_file
import sqlite3
from datetime import datetime
from database import init_db

# ================= 配置区域 =================
# 模型配置
MODEL_CONFIG = {
    # 用os库优先从环境变量读取，否则使用默认值
    # 通义千问
    "Qwen": {
        "api_key": os.getenv("QWEN_API_KEY", "sk-667eb4d069424ecbbd24c19b44fc5f32"),
        "base_url": "https://dashscope.aliyuncs.com/compatible-mode/v1",
        "model_name": "qwen-turbo"
    },
    # 文心一言
    "ERNIE": {
        "api_key": os.getenv("ERNIE_API_KEY",
                             "bce-v3/ALTAK-6nS6ATIr9PUWSOnZWjAn5/fcccaab611188a339246748a13c803c99fe64caf"),
        "base_url": "https://qianfan.baidubce.com/v2",
        "model_name": "ernie-4.0-turbo-8k"
    },
    # Moonshot
    "Moonshot": {
        "api_key": os.getenv("MOONSHOT_API_KEY", "sk-CoXsoYPoiK9SxwLb58PZiWw3KqObnMDLoo3Etwnrd5sMWzMR"),
        "base_url": "https://api.moonshot.cn/v1",
        "model_name": "moonshot-v1-8k"
    }
}

# ================= 系统提示词 =================
SYSTEM_PROMPTS = {
    "platform_assistant": """
    你是一个高校体育教学平台的智能助手，主要职责包括：
    1. 根据学生的体测数据、训练目标生成个性化健身计划；
    2. 解答体育训练、动作规范、营养补充等相关问题；
    3. 分析学生的训练数据并提供专业建议；
    4. 鼓励学生坚持锻炼，培养良好的运动习惯。
    
    回答要求：
    - 使用亲切、专业的语言风格
    - 回答要有科学依据，避免误导用户
    - 遇到不确定的问题，建议用户咨询专业教练
    - 适当使用表情符号让交流更生动
    """
}


# ================= 核心功能 =================
class ChatManager:
    """
    对话会话管理类:
    1.创建和管理多个对话会话
    2.实现对话记忆（将文本储存到数据库）
    3.管理当前活动会话
    """
    def __init__(self):
        # 初始化数据库
        init_db()
        
    def create_session(self, user_id: str, model_name: str = None) -> int:
        """
        为指定用户创建新会话
        :param user_id: 用户ID
        :param model_name: 使用的默认模型名称（可选）
        :return: 新创建的会话ID
        """
        title = f"新对话-{time.strftime('%Y-%m-%d %H:%M')}"
        
        # 保存到数据库
        conn = sqlite3.connect('chat_history.db')
        cursor = conn.cursor()
        cursor.execute(
            "INSERT INTO sessions (user_id, title, model) VALUES (?, ?, ?)",
            (user_id, title, model_name)
        )
        session_id = cursor.lastrowid
        conn.commit()
        conn.close()
        
        return session_id
        
    def get_session_by_user(self, user_id: str) -> Dict:
        """
        获取指定用户的最新会话
        :param user_id: 用户ID
        :return: 会话数据字典
        """
        conn = sqlite3.connect('chat_history.db')
        conn.row_factory = sqlite3.Row
        cursor = conn.cursor()
        
        # 获取用户最新会话信息
        cursor.execute("SELECT * FROM sessions WHERE user_id = ? ORDER BY updated_at DESC LIMIT 1", (user_id,))
        session_row = cursor.fetchone()
        
        if not session_row:
            conn.close()
            return None
            
        # 获取会话消息 - 过滤掉system消息
        cursor.execute(
            "SELECT role, content, model FROM messages WHERE session_id = ? AND role IN ('user', 'assistant') ORDER BY timestamp",
            (session_row["id"],)
        )
        messages = [{"role": row[0], "content": row[1], "model": row[2]} for row in cursor.fetchall()]
        
        conn.close()
        
        return {
            "session_id": session_row["id"],
            "messages": messages,
            "model": session_row["model"],
            "title": session_row["title"]
        }
        
    def get_session_by_id(self, session_id: int) -> Dict:
        """
        根据会话ID获取会话数据
        :param session_id: 会话ID
        :return: 会话数据字典
        """
        conn = sqlite3.connect('chat_history.db')
        conn.row_factory = sqlite3.Row
        cursor = conn.cursor()
        
        # 获取会话信息
        cursor.execute("SELECT * FROM sessions WHERE id = ?", (session_id,))
        session_row = cursor.fetchone()
        
        if not session_row:
            conn.close()
            return None
            
        # 获取会话消息 - 过滤掉system消息
        cursor.execute(
            "SELECT role, content, model FROM messages WHERE session_id = ? AND role IN ('user', 'assistant') ORDER BY timestamp",
            (session_id,)
        )
        messages = [{"role": row[0], "content": row[1], "model": row[2]} for row in cursor.fetchall()]
        
        conn.close()
        
        return {
            "session_id": session_row["id"],
            "messages": messages,
            "model": session_row["model"],
            "title": session_row["title"]
        }
        
    def update_session_title(self, session_id: int, user_input: str) -> None:
        """
        基于用户第一条消息给会话一个标题
        :param session_id: 会话ID
        :param user_input: 用户输入内容
        """
        if len(user_input) > 15:
            user_input = user_input[:15] + "..."  # 标题截断处理
            
        conn = sqlite3.connect('chat_history.db')
        cursor = conn.cursor()
        cursor.execute(
            "UPDATE sessions SET title = ? WHERE id = ?",
            (user_input, session_id)
        )
        conn.commit()
        conn.close()
        
    def delete_session(self, session_id: int) -> bool:
        """
        删除指定会话
        :param session_id: 会话ID
        :return: 删除是否成功
        """
        conn = sqlite3.connect('chat_history.db')
        cursor = conn.cursor()
        cursor.execute("DELETE FROM sessions WHERE id = ?", (session_id,))
        success = cursor.rowcount > 0
        conn.commit()
        conn.close()
        return success
        
    def list_sessions_by_user(self, user_id: str) -> List[Dict]:
        """
        获取指定用户的所有会话列表
        :param user_id: 用户ID
        :return: 会话列表
        """
        conn = sqlite3.connect('chat_history.db')
        conn.row_factory = sqlite3.Row
        cursor = conn.cursor()
        cursor.execute("SELECT id, title, model FROM sessions WHERE user_id = ? ORDER BY updated_at DESC", (user_id,))
        rows = cursor.fetchall()
        conn.close()
        
        return [{"session_id": row[0], "title": row[1], "model": row[2]} for row in rows]
        
    def add_message(self, session_id: int, role: str, content: str, model: str = None) -> bool:
        """
        向会话添加消息
        :param session_id: 会话ID
        :param role: 消息角色(user/assistant/system)
        :param content: 消息内容
        :param model: 使用的模型（可选）
        :return: 添加是否成功
        """
        conn = sqlite3.connect('chat_history.db')
        cursor = conn.cursor()
        cursor.execute(
            "INSERT INTO messages (session_id, role, content, model) VALUES (?, ?, ?, ?)",
            (session_id, role, content, model)
        )
        # 更新会话的更新时间
        cursor.execute(
            "UPDATE sessions SET updated_at = CURRENT_TIMESTAMP WHERE id = ?",
            (session_id,)
        )
        conn.commit()
        conn.close()
        return True
        
    def clear_session_messages(self, session_id: int) -> bool:
        """
        清空会话消息
        :param session_id: 会话ID
        :return: 清空是否成功
        """
        conn = sqlite3.connect('chat_history.db')
        cursor = conn.cursor()
        cursor.execute("DELETE FROM messages WHERE session_id = ?", (session_id,))
        conn.commit()
        conn.close()
        return True


# ================= 业务逻辑 =================
def get_client(model_name: str):
    """
    获取对应模型的OpenAI客户端
    :param model_name: 模型名称(Qwen/ERNIE/Moonshot)
    :return: OpenAI客户端实例
    """
    cfg = MODEL_CONFIG[model_name]
    return openai.OpenAI(
        api_key=cfg["api_key"],
        base_url=cfg["base_url"]
    )

def model_predict(model_name: str, messages: List[Dict]) -> str:
    """
    调用模型API生成回复
    :param model_name: 模型名称
    :param messages: 消息历史列表
    :return: 模型生成的回复内容或错误信息
    """
    try:
        client = get_client(model_name)
        # 调用模型API，将历史+新消息一起发送给AI
        response = client.chat.completions.create(
            model=MODEL_CONFIG[model_name]["model_name"],
            messages=messages
        )
        return response.choices[0].message.content
    except Exception as e:
        # 错误处理
        return f"错误：{str(e)}"

def export_markdown(session_id: int, chat_mgr: ChatManager):
    """
    将指定会话导出为Markdown格式文件。
    :param session_id: 会话ID，通过此ID获取对应会话的数据
    :param chat_mgr: ChatManager实例
    :return: 导出的Markdown文件路径，如果会话不存在则返回None
    """
    # 获取会话数据
    session = chat_mgr.get_session_by_id(session_id)
    if not session:
        # 如果会话不存在，返回None
        return None
    # 初始化Markdown内容，包含会话标题
    md_lines = [f"# 会话导出 - {session['title']}"]
    # 遍历消息历史，将每条消息按"角色：内容"格式转换成Markdown
    for msg in session["messages"]:
        role = "用户" if msg["role"] == "user" else "AI_Chat"  # 判断消息的角色
        md_lines.append(f"**{role}：**\n{msg['content']}\n")
    # 将所有行拼接成完整的Markdown文本
    md_content = "\n\n".join(md_lines)
    # 保存Markdown内容到临时文件
    with tempfile.NamedTemporaryFile(mode='w+', delete=False, suffix=".md", encoding="utf-8") as f:
        f.write(md_content)  # 写入Markdown内容
        temp_path = f.name  # 获取文件路径
    # 返回保存的文件路径
    return temp_path