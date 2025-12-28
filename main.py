import os
import time
import openai
from typing import Dict, List
import tempfile
import json
import requests
from fastapi import FastAPI, Request, Response
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import JSONResponse, FileResponse
import uvicorn
import sqlite3
from datetime import datetime
from database import init_db

# 导入chat模块
from chat_module import (
    MODEL_CONFIG,
    SYSTEM_PROMPTS,
    ChatManager,
    get_client,
    model_predict,
    export_markdown
)

# 导入report模块
from report_module import (
    AI_PROMPTS,
    generate_analysis_report,
    query_analysis_report,
    get_recent_analyses
)

# ================= FastAPI应用 =================
app = FastAPI(title="AI Chat API", description="AI聊天后端API")

# 允许跨域请求
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=False,
    allow_methods=["GET", "POST", "PUT", "DELETE", "OPTIONS", "PATCH"],
    allow_headers=["*"],
    expose_headers=["*"],
    max_age=3600,
)

chat_mgr = ChatManager()

# 应用启动时初始化数据库
init_db()

@app.get('/api/sessions')
async def list_sessions(user_id: str):
    """获取指定用户的所有会话列表"""
    try:
        if not user_id:
            return JSONResponse({
                "success": False,
                "error": "用户ID不能为空"
            }, status_code=400)
            
        sessions = chat_mgr.list_sessions_by_user(user_id)
        return JSONResponse({
            "success": True,
            "data": sessions
        })
    except Exception as e:
        return JSONResponse({
            "success": False,
            "error": str(e)
        }, status_code=500)

@app.post('/api/sessions')
async def create_session(request: Request):
    """为指定用户创建新会话"""
    try:
        data = await request.json()
        user_id = data.get('user_id')
        model_name = data.get('model')  # 模型是可选的，默认值在ChatManager中处理
        
        if not user_id:
            return JSONResponse({
                "success": False,
                "error": "用户ID不能为空"
            }, status_code=400)
            
        # 验证模型是否支持（如果指定了模型）
        if model_name and model_name not in MODEL_CONFIG:
            return JSONResponse({
                "success": False,
                "error": f"不支持的模型: {model_name}"
            }, status_code=400)
            
        session_id = chat_mgr.create_session(user_id, model_name)
        session = chat_mgr.get_session_by_id(session_id)
        
        # 发送欢迎消息
        system_prompt = SYSTEM_PROMPTS["platform_assistant"]
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
        
        return JSONResponse({
            "success": True,
            "data": {
                "session_id": session_id,
                "session": session,
                "welcome_message": welcome_msg
            }
        })
    except Exception as e:
        return JSONResponse({
            "success": False,
            "error": str(e)
        }, status_code=500)

@app.get('/api/sessions/user/{user_id}')
async def get_session_by_user(user_id: str):
    """获取指定用户的最新会话"""
    try:
        session = chat_mgr.get_session_by_user(user_id)
        if not session:
            # 如果用户没有会话，返回空数据
            return JSONResponse({
                "success": False,
                "data": None
            })
            
        return JSONResponse({
            "success": True,
            "data": session
        })
    except Exception as e:
        return JSONResponse({
            "success": False,
            "error": str(e)
        }, status_code=500)

@app.get('/api/sessions/{session_id}')
async def get_session(session_id: int):
    """获取指定会话详情"""
    try:
        session = chat_mgr.get_session_by_id(session_id)
        if not session:
            return JSONResponse({
                "success": False,
                "error": "会话不存在"
            }, status_code=404)
            
        return JSONResponse({
            "success": True,
            "data": session
        })
    except Exception as e:
        return JSONResponse({
            "success": False,
            "error": str(e)
        }, status_code=500)

@app.delete('/api/sessions/{session_id}')
async def delete_session(session_id: int):
    """删除会话"""
    try:
        success = chat_mgr.delete_session(session_id)
        if not success:
            return JSONResponse({
                "success": False,
                "error": "会话不存在"
            }, status_code=404)
            
        return JSONResponse({
            "success": True,
            "message": "会话已删除"
        })
    except Exception as e:
        return JSONResponse({
            "success": False,
            "error": str(e)
        }, status_code=500)

@app.post('/api/sessions/{session_id}/messages')
async def send_message(session_id: int, request: Request):
    """发送消息"""
    try:
        session = chat_mgr.get_session_by_id(session_id)
        if not session:
            return JSONResponse({
                "success": False,
                "error": "会话不存在"
            }, status_code=404)
            
        data = await request.json()
        user_message = data.get('message')
        model_name = data.get('model')  # 允许前端指定模型

        if not user_message:
            return JSONResponse({
                "success": False,
                "error": "消息内容不能为空"
            }, status_code=400)
            
        # 验证模型是否支持（如果指定了模型）
        if model_name and model_name not in MODEL_CONFIG:
            return JSONResponse({
                "success": False,
                "error": f"不支持的模型: {model_name}"
            }, status_code=400)
            
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
        
        return JSONResponse({
            "success": True,
            "data": {
                "session": final_session,
                "response": response
            }
        })
    except Exception as e:
        return JSONResponse({
            "success": False,
            "error": str(e)
        }, status_code=500)

@app.post('/api/sessions/{session_id}/clear')
async def clear_session(session_id: int):
    """清空会话消息"""
    try:
        session = chat_mgr.get_session_by_id(session_id)
        if not session:
            return JSONResponse({
                "success": False,
                "error": "会话不存在"
            }, status_code=404)
            
        success = chat_mgr.clear_session_messages(session_id)
        if not success:
            return JSONResponse({
                "success": False,
                "error": "清空会话失败"
            }, status_code=500)
            
        return JSONResponse({
            "success": True,
            "message": "会话已清空"
        })
    except Exception as e:
        return JSONResponse({
            "success": False,
            "error": str(e)
        }, status_code=500)

@app.get('/api/sessions/{session_id}/export')
async def export_session(session_id: int):
    """导出会话为Markdown文件"""
    try:
        temp_path = export_markdown(session_id, chat_mgr)
        if not temp_path:
            return JSONResponse({
                "success": False,
                "error": "会话不存在"
            }, status_code=404)
            
        return FileResponse(temp_path, filename=f"会话_{session_id}.md")
    except Exception as e:
        return JSONResponse({
            "success": False,
            "error": str(e)
        }, status_code=500)

@app.get('/api/models')
async def list_models():
    """获取支持的模型列表"""
    try:
        models = list(MODEL_CONFIG.keys())
        return JSONResponse({
            "success": True,
            "data": models
        })
    except Exception as e:
        return JSONResponse({
            "success": False,
            "error": str(e)
        }, status_code=500)


# ================= AI分析接口 =================
@app.post('/api/analysis/generate')
async def api_generate_analysis_report(request: Request):
    """
    生成智能分析报告
    """
    return await generate_analysis_report(model_predict, request)

@app.get('/api/analysis/query')
async def api_query_analysis_report(request: Request):
    """
    查询已生成的报告
    """
    return await query_analysis_report(request)

@app.get('/api/analysis/recent')
async def api_get_recent_analyses(request: Request):
    """
    获取最近的智能分析记录
    """
    return await get_recent_analyses(request)


if __name__ == "__main__":
    uvicorn.run("main:app", host="0.0.0.0", port=5000, reload=True)