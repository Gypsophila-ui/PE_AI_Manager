import requests
import json
import sqlite3
from fastapi import Request, Response
from fastapi.responses import JSONResponse

# ================= 配置区域 =================
# YOLO后端服务基础URL
YOLO_BASE_URL = "http://localhost:8000"

# ================= AI分析配置 =================
AI_PROMPTS = {
    "homework_feedback": """
    你是一个专业的体育教练AI助手，请根据学生的运动数据分析其表现。

    分析要求：
    1. 总体表现评价：基于正确次数、错误次数、准确率给出综合评价
    2. 主要问题分析：识别学生最常见的错误类型（如膝盖位置、姿势倾斜等）
    3. 进步空间：指出可以改进的具体方面
    4. 建议训练计划：给出针对性的训练建议
    5. 鼓励性话语：包含积极的反馈和鼓励

    输出格式要求：
    - 使用中文回复
    - 包含以下部分：总体评价、错误分析、改进建议、训练计划
    - 保持专业但友好的语气

    学生数据：
    {student_data}
    
    请生成完整的分析报告。
    """,
    
    "personalized_tips": """
    你是一个专业的健身教练和健康顾问，请为学生提供个性化的长期训练和发展建议。

    分析要求：
    1. 身体条件分析：基于学生的身高、体重等身体特征分析其优势和需要注意的问题
    2. 历史趋势分析：分析学生长期的进步轨迹、稳定表现和波动情况
    3. 个性化训练规划：结合身体特征和历史表现，制定长期训练目标和阶段性计划
    4. 健康管理建议：提供营养、休息和伤病预防方面的专业建议
    5. 鼓励性话语：包含积极的反馈和长期坚持的激励

    输出格式要求：
    - 使用中文回复
    - 包含以下部分：身体条件分析、历史趋势分析、个性化训练规划、健康管理建议
    - 保持专业但友好的语气

    学生个人信息：
    {student_info}
    
    学生历史数据：
    {student_data}
    
    请生成完整的个性化建议报告。
    """
}


# ================= 业务逻辑 =================
def get_yolo_record_details(homework_id, student_id):
    """
    调用YOLO后端的/get_record_details接口获取记录详情
    """
    url = f"{YOLO_BASE_URL}/get_record_details"
    
    try:
        response = requests.get(url, params={
            "homework_id": homework_id,
            "student_id": student_id
        }, timeout=10)
        
        if response.status_code == 200:
            return response.json()
        else:
            return None
    except Exception as e:
        print(f"调用YOLO后端接口失败: {e}")
        return None

def get_yolo_student_all_records(student_id):
    """
    调用YOLO后端的/api/student/all-records/{student_id}接口获取学生所有记录
    """
    url = f"{YOLO_BASE_URL}/api/student/all-records/{student_id}"
    
    try:
        response = requests.get(url, timeout=10)
        
        if response.status_code == 200:
            return response.json()
        else:
            return None
    except Exception as e:
        print(f"调用YOLO后端接口失败: {e}")
        return None

def save_ai_analysis_report(homework_id, student_id, pose_type, analysis_type, 
                           query_content, report_content, raw_data, model_used, student_info=None):
    """
    保存AI分析报告到数据库
    """
    conn = sqlite3.connect('chat_history.db')
    cursor = conn.cursor()
    
    cursor.execute('''
        INSERT OR REPLACE INTO ai_analysis_reports 
        (homework_id, student_id, pose_type, analysis_type, query_content, 
         report_content, raw_data, model_used, student_info)
        VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)
    ''', (homework_id, student_id, pose_type, analysis_type, query_content,
          report_content, raw_data, model_used, json.dumps(student_info) if student_info else None))
    
    conn.commit()
    report_id = cursor.lastrowid
    conn.close()
    
    return report_id

def get_ai_analysis_report(homework_id, student_id, analysis_type=None):
    """
    查询AI分析报告
    """
    conn = sqlite3.connect('chat_history.db')
    conn.row_factory = sqlite3.Row
    cursor = conn.cursor()
    
    if analysis_type:
        if homework_id:
            cursor.execute('''
                SELECT * FROM ai_analysis_reports 
                WHERE homework_id = ? AND student_id = ? AND analysis_type = ?
                ORDER BY created_at DESC
            ''', (homework_id, student_id, analysis_type))
        else:
            cursor.execute('''
                SELECT * FROM ai_analysis_reports 
                WHERE student_id = ? AND analysis_type = ?
                ORDER BY created_at DESC
            ''', (student_id, analysis_type))
    else:
        if homework_id:
            cursor.execute('''
                SELECT * FROM ai_analysis_reports 
                WHERE homework_id = ? AND student_id = ?
                ORDER BY created_at DESC
            ''', (homework_id, student_id))
        else:
            cursor.execute('''
                SELECT * FROM ai_analysis_reports 
                WHERE student_id = ?
                ORDER BY created_at DESC
            ''', (student_id,))
    
    rows = cursor.fetchall()
    conn.close()
    
    return [dict(row) for row in rows]


# ================= API接口 =================
async def generate_analysis_report(model_predict_func, request: Request):
    """
    生成智能分析报告
    请求参数：
    对于作业反馈：
    {
        "homework_id": "hw123",
        "student_id": "stu456",
        "analysis_type": "homework_feedback",
        "query": "分析我的深蹲表现"  # 用户的具体查询问题
    }
    
    对于个性化建议：
    {
        "student_id": "stu456",
        "analysis_type": "personalized_tips",
        "student_info": {"height": 175, "weight": 65},  # 可选的学生个人信息
        "query": "根据我的情况给出长期训练建议"  # 用户的具体查询问题
    }
    """
    try:
        data = await request.json()
        homework_id = data.get('homework_id')
        student_id = data.get('student_id')
        analysis_type = data.get('analysis_type', 'homework_feedback')
        query_content = data.get('query', '')
        student_info = data.get('student_info', {})  # 学生个人信息（身高、体重等）
        
        if not student_id:
            return JSONResponse({
                "success": False,
                "error": "学生ID不能为空"
            }, status_code=400)
            
        # 检查分析类型是否支持
        if analysis_type not in AI_PROMPTS:
            return JSONResponse({
                "success": False,
                "error": f"不支持的分析类型: {analysis_type}"
            }, status_code=400)
            
        # 根据分析类型获取相应的数据
        if analysis_type == "homework_feedback":
            # 作业反馈需要作业ID
            if not homework_id:
                return JSONResponse({
                    "success": False,
                    "error": "作业反馈类型需要提供作业ID"
                }, status_code=400)
                
            # 调用YOLO后端获取作业数据
            yolo_data = get_yolo_record_details(homework_id, student_id)
            if not yolo_data:
                return JSONResponse({
                    "success": False,
                    "error": "未能从YOLO后端获取作业数据"
                }, status_code=500)
                
            # 构造AI提示词
            prompt = AI_PROMPTS[analysis_type].format(student_data=json.dumps(yolo_data, ensure_ascii=False, indent=2))
            raw_data = json.dumps(yolo_data, ensure_ascii=False)
            pose_type = yolo_data[0].get('pose_type', '') if isinstance(yolo_data, list) and len(yolo_data) > 0 else ''
            
        elif analysis_type == "personalized_tips":
            # 个性化建议获取学生所有记录
            yolo_data = get_yolo_student_all_records(student_id)
            if not yolo_data:
                return JSONResponse({
                    "success": False,
                    "error": "未能从YOLO后端获取学生历史数据"
                }, status_code=500)
                
            # 构造AI提示词
            prompt = AI_PROMPTS[analysis_type].format(
                student_info=json.dumps(student_info, ensure_ascii=False, indent=2),
                student_data=json.dumps(yolo_data, ensure_ascii=False, indent=2)
            )
            raw_data = json.dumps(yolo_data, ensure_ascii=False)
            pose_type = ''
            
        # 准备发送给AI的消息
        messages = [
            {"role": "system", "content": "你是一个专业的体育教练AI助手。"},
            {"role": "user", "content": prompt}
        ]
        
        # 调用AI模型生成分析报告
        model_name = "Qwen"  # 默认使用通义千问
        report_content = model_predict_func(model_name, messages)
        
        # 保存报告到数据库
        save_ai_analysis_report(
            homework_id, student_id, pose_type, analysis_type, 
            query_content, report_content, raw_data, model_name, student_info
        )
        
        return JSONResponse({
            "success": True,
            "data": {
                "report": report_content,
                "analysis_type": analysis_type
            }
        })
        
    except Exception as e:
        return JSONResponse({
            "success": False,
            "error": str(e)
        }, status_code=500)


async def query_analysis_report(request: Request):
    """
    查询已生成的报告
    参数：
    - homework_id: 作业ID（可选，针对作业反馈类型）
    - student_id: 学生ID
    - analysis_type: 分析类型（可选）
    """
    try:
        # 获取查询参数
        homework_id = request.query_params.get('homework_id')
        student_id = request.query_params.get('student_id')
        analysis_type = request.query_params.get('analysis_type')
        
        if not student_id:
            return JSONResponse({
                "success": False,
                "error": "学生ID不能为空"
            }, status_code=400)
            
        reports = get_ai_analysis_report(homework_id, student_id, analysis_type)
        
        return JSONResponse({
            "success": True,
            "data": reports
        })
        
    except Exception as e:
        return JSONResponse({
            "success": False,
            "error": str(e)
        }, status_code=500)


async def get_recent_analyses(request: Request):
    """
    获取最近的智能分析记录
    参数：
    - student_id: 学生ID（可选）
    - limit: 返回数量限制
    """
    try:
        student_id = request.query_params.get('student_id')
        limit = request.query_params.get('limit', 10)
        
        conn = sqlite3.connect('chat_history.db')
        conn.row_factory = sqlite3.Row
        cursor = conn.cursor()
        
        if student_id:
            cursor.execute('''
                SELECT * FROM ai_analysis_reports 
                WHERE student_id = ? 
                ORDER BY created_at DESC 
                LIMIT ?
            ''', (student_id, limit))
        else:
            cursor.execute('''
                SELECT * FROM ai_analysis_reports 
                ORDER BY created_at DESC 
                LIMIT ?
            ''', (limit,))
        
        rows = cursor.fetchall()
        conn.close()
        
        return JSONResponse({
            "success": True,
            "data": [dict(row) for row in rows]
        })
        
    except Exception as e:
        return JSONResponse({
            "success": False,
            "error": str(e)
        }, status_code=500)