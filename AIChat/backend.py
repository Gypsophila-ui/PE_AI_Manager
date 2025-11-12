import os
import time
import openai
from typing import Dict, List
import tempfile
import json
from flask import Flask, request, jsonify, send_file
from flask_cors import CORS
import sqlite3
from datetime import datetime

# ================= 配置区域 =================
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

# ================= 数据库初始化 =================
def init_db():
    """初始化数据库"""
    conn = sqlite3.connect('chat_history.db')
    cursor = conn.cursor()
    
    # 创建会话表
    cursor.execute('''
        CREATE TABLE IF NOT EXISTS sessions (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            user_id TEXT NOT NULL,
            title TEXT NOT NULL,
            model TEXT,
            created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
            updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
        )
    ''')
    
    # 检查messages表是否已有model字段，如果没有则添加
    try:
        cursor.execute("ALTER TABLE messages ADD COLUMN model TEXT")
    except sqlite3.OperationalError:
        # 字段可能已经存在，忽略错误
        pass
    
    # 创建消息表（如果不存在）
    cursor.execute('''
        CREATE TABLE IF NOT EXISTS messages (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            session_id INTEGER NOT NULL,
            role TEXT NOT NULL,
            content TEXT NOT NULL,
            model TEXT,
            timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
            FOREIGN KEY (session_id) REFERENCES sessions(id) ON DELETE CASCADE
        )
    ''')
    
    conn.commit()
    conn.close()

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
            
        # 获取会话消息
        cursor.execute("SELECT role, content, model FROM messages WHERE session_id = ? ORDER BY timestamp", (session_row["id"],))
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
            
        # 获取会话消息
        cursor.execute("SELECT role, content, model FROM messages WHERE session_id = ? ORDER BY timestamp", (session_id,))
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

# ================= Flask应用 =================
app = Flask(__name__)
CORS(app)  # 允许跨域请求
chat_mgr = ChatManager()

@app.route('/api/sessions', methods=['GET'])
def list_sessions():
    """获取指定用户的所有会话列表"""
    try:
        user_id = request.args.get('user_id')
        if not user_id:
            return jsonify({
                "success": False,
                "error": "用户ID不能为空"
            }), 400
            
        sessions = chat_mgr.list_sessions_by_user(user_id)
        return jsonify({
            "success": True,
            "data": sessions
        })
    except Exception as e:
        return jsonify({
            "success": False,
            "error": str(e)
        }), 500

@app.route('/api/sessions', methods=['POST'])
def create_session():
    """为指定用户创建新会话"""
    try:
        data = request.json
        user_id = data.get('user_id')
        model_name = data.get('model')  # 模型是可选的，默认值在ChatManager中处理
        
        if not user_id:
            return jsonify({
                "success": False,
                "error": "用户ID不能为空"
            }), 400
            
        # 验证模型是否支持（如果指定了模型）
        if model_name and model_name not in MODEL_CONFIG:
            return jsonify({
                "success": False,
                "error": f"不支持的模型: {model_name}"
            }), 400
            
        session_id = chat_mgr.create_session(user_id, model_name)
        session = chat_mgr.get_session_by_id(session_id)
        
        # 发送欢迎消息
        system_prompt = """
        你是一个体育健身数智化教学平台的AI助手，主要职责包括：
        1. 根据学生的体测数据、训练目标生成个性化健身计划；
        2. 解答体育训练、动作规范、营养补充等相关问题；
        """
        chat_mgr.add_message(session_id, "system", system_prompt, model_name)
        
        welcome_msg = {
            "role": "assistant",
            "content": (
                "欢迎使用体育健身数智化教学平台！\n\n"
                "我可以为您提供：\n"
                "1. 个性化训练计划定制（请告知您的身高/体重/目标）\n"
                "2. 健身动作纠正指导\n"
                "3. 体育理论知识与营养建议\n\n"
                "请直接输入您的需求或体测数据~"
            )
        }
        chat_mgr.add_message(session_id, "assistant", welcome_msg["content"], model_name)
        
        return jsonify({
            "success": True,
            "data": {
                "session_id": session_id,
                "session": session,
                "welcome_message": welcome_msg
            }
        })
    except Exception as e:
        return jsonify({
            "success": False,
            "error": str(e)
        }), 500

@app.route('/api/sessions/user/<user_id>', methods=['GET'])
def get_session_by_user(user_id):
    """获取指定用户的最新会话"""
    try:
        session = chat_mgr.get_session_by_user(user_id)
        if not session:
            # 如果用户没有会话，返回空数据
            return jsonify({
                "success": False,
                "data": None
            })
            
        return jsonify({
            "success": True,
            "data": session
        })
    except Exception as e:
        return jsonify({
            "success": False,
            "error": str(e)
        }), 500

@app.route('/api/sessions/<int:session_id>', methods=['GET'])
def get_session(session_id):
    """获取指定会话详情"""
    try:
        session = chat_mgr.get_session_by_id(session_id)
        if not session:
            return jsonify({
                "success": False,
                "error": "会话不存在"
            }), 404
            
        return jsonify({
            "success": True,
            "data": session
        })
    except Exception as e:
        return jsonify({
            "success": False,
            "error": str(e)
        }), 500

@app.route('/api/sessions/<int:session_id>', methods=['DELETE'])
def delete_session(session_id):
    """删除会话"""
    try:
        success = chat_mgr.delete_session(session_id)
        if not success:
            return jsonify({
                "success": False,
                "error": "会话不存在"
            }), 404
            
        return jsonify({
            "success": True,
            "message": "会话已删除"
        })
    except Exception as e:
        return jsonify({
            "success": False,
            "error": str(e)
        }), 500

@app.route('/api/sessions/<int:session_id>/messages', methods=['POST'])
def send_message(session_id):
    """发送消息"""
    try:
        session = chat_mgr.get_session_by_id(session_id)
        if not session:
            return jsonify({
                "success": False,
                "error": "会话不存在"
            }), 404
            
        data = request.json
        user_message = data.get('message')
        model_name = data.get('model')  # 允许前端指定模型

        if not user_message:
            return jsonify({
                "success": False,
                "error": "消息内容不能为空"
            }), 400
            
        # 验证模型是否支持（如果指定了模型）
        if model_name and model_name not in MODEL_CONFIG:
            return jsonify({
                "success": False,
                "error": f"不支持的模型: {model_name}"
            }), 400
            
        # 如果没有指定模型，则使用会话默认模型或Qwen
        if not model_name:
            model_name = session.get('model', 'Qwen')
            
        # 如果是第一条用户消息，更新会话标题
        user_messages = [m for m in session["messages"] if m["role"] == "user"]
        if len(user_messages) == 0:
            chat_mgr.update_session_title(session_id, user_message)
            
        # 添加用户消息到历史（记录使用的模型）
        chat_mgr.add_message(session_id, "user", user_message, model_name)
        
        # 获取更新后的会话（包含新消息）
        updated_session = chat_mgr.get_session_by_id(session_id)
        
        # 调用模型生成回复
        response = model_predict(model_name, updated_session["messages"])
        
        # 添加AI回复到历史（记录使用的模型）
        chat_mgr.add_message(session_id, "assistant", response, model_name)
        
        # 获取最终的会话
        final_session = chat_mgr.get_session_by_id(session_id)
        
        return jsonify({
            "success": True,
            "data": {
                "session": final_session,
                "response": response
            }
        })
    except Exception as e:
        return jsonify({
            "success": False,
            "error": str(e)
        }), 500

@app.route('/api/sessions/<int:session_id>/clear', methods=['POST'])
def clear_session(session_id):
    """清空会话消息"""
    try:
        session = chat_mgr.get_session_by_id(session_id)
        if not session:
            return jsonify({
                "success": False,
                "error": "会话不存在"
            }), 404
            
        success = chat_mgr.clear_session_messages(session_id)
        if not success:
            return jsonify({
                "success": False,
                "error": "清空会话失败"
            }), 500
            
        return jsonify({
            "success": True,
            "message": "会话已清空"
        })
    except Exception as e:
        return jsonify({
            "success": False,
            "error": str(e)
        }), 500

@app.route('/api/sessions/<int:session_id>/export', methods=['GET'])
def export_session(session_id):
    """导出会话为Markdown文件"""
    try:
        temp_path = export_markdown(session_id, chat_mgr)
        if not temp_path:
            return jsonify({
                "success": False,
                "error": "会话不存在"
            }), 404
            
        return send_file(temp_path, as_attachment=True, download_name=f"会话_{session_id}.md")
    except Exception as e:
        return jsonify({
            "success": False,
            "error": str(e)
        }), 500

@app.route('/api/models', methods=['GET'])
def list_models():
    """获取支持的模型列表"""
    try:
        models = list(MODEL_CONFIG.keys())
        return jsonify({
            "success": True,
            "data": models
        })
    except Exception as e:
        return jsonify({
            "success": False,
            "error": str(e)
        }), 500

# ================= 应用启动 =================
if __name__ == "__main__":
    app.run(host="0.0.0.0", port=5000, debug=True)