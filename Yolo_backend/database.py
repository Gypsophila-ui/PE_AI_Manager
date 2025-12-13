import sqlite3
import json
import os
from typing import Optional, Dict, Any
from datetime import datetime, timezone

# 数据库文件路径
DB_PATH = "exercise_feedback.db"

def init_database():
    """
    初始化数据库，创建exercise_feedback表
    """
    conn = sqlite3.connect(DB_PATH)
    cursor = conn.cursor()
    
    # 创建表
    cursor.execute("""
        CREATE TABLE IF NOT EXISTS exercise_feedback (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            
            -- 基本信息
            homework_id VARCHAR(50) NOT NULL,           -- 作业ID
            student_id VARCHAR(50) NOT NULL,           -- 学生ID
            pose_type VARCHAR(20) NOT NULL,            -- 动作类型: pushup/squat/abworkout等
            
            -- 上传信息
            uploaded_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,  -- 视频上传时间
            
            -- 存储路径
            original_video_path VARCHAR(255),                -- 原始视频路径（可选）
            processed_video_path VARCHAR(255) NOT NULL,      -- 处理后视频路径
            
            -- 分析结果
            total_count INTEGER DEFAULT 0,                   -- 总动作次数
            correct_count INTEGER DEFAULT 0,                 -- 正确动作次数
            incorrect_count INTEGER DEFAULT 0,               -- 错误动作次数
            
            -- AI反馈数据（JSON格式）
            feedback_json TEXT,                              -- AI详细反馈，可为空但通常有内容
            
            -- 性能指标
            video_duration FLOAT,                            -- 视频时长（秒）
            
            -- 索引
            UNIQUE (homework_id, student_id, pose_type)      -- 同一作业同一学生的同类型动作唯一
        )
    """)
    
    # 创建索引
    cursor.execute("CREATE INDEX IF NOT EXISTS idx_homework_student ON exercise_feedback (homework_id, student_id)")
    cursor.execute("CREATE INDEX IF NOT EXISTS idx_upload_time ON exercise_feedback (uploaded_at)")
    
    conn.commit()
    conn.close()

def insert_exercise_feedback(
    homework_id: str,
    student_id: str,
    pose_type: str,
    processed_video_path: str,
    total_count: int = 0,
    correct_count: int = 0,
    incorrect_count: int = 0,
    feedback_json: Optional[str] = None,
    video_duration: Optional[float] = None,
    original_video_path: Optional[str] = None
) -> int:
    """
    插入或更新一条健身动作分析反馈记录
    
    Args:
        homework_id: 作业ID
        student_id: 学生ID
        pose_type: 动作类型
        processed_video_path: 处理后视频路径
        total_count: 总动作次数
        correct_count: 正确动作次数
        incorrect_count: 错误动作次数
        feedback_json: AI反馈数据（JSON格式）
        video_duration: 视频时长（秒）
        original_video_path: 原始视频路径
        
    Returns:
        int: 记录的ID
    """
    conn = sqlite3.connect(DB_PATH)
    cursor = conn.cursor()
    
    # 获取当前本地时间并格式化为字符串
    local_time = datetime.now().astimezone().strftime("%Y-%m-%d %H:%M:%S")
    
    # 首先检查记录是否已存在
    cursor.execute("""
        SELECT id FROM exercise_feedback 
        WHERE homework_id = ? AND student_id = ? AND pose_type = ?
    """, (homework_id, student_id, pose_type))
    
    existing_record = cursor.fetchone()
    
    if existing_record:
        # 如果记录已存在，则更新它
        record_id = existing_record[0]
        cursor.execute("""
            UPDATE exercise_feedback SET
                uploaded_at = ?,  -- 更新上传时间为当前本地时间
                original_video_path = ?,
                processed_video_path = ?,
                total_count = ?,
                correct_count = ?,
                incorrect_count = ?,
                feedback_json = ?,
                video_duration = ?
            WHERE id = ?
        """, (
            local_time, original_video_path, processed_video_path,
            total_count, correct_count, incorrect_count,
            feedback_json, video_duration,
            record_id
        ))
    else:
        # 如果记录不存在，则插入新记录，使用本地时间替代默认的UTC时间
        cursor.execute("""
            INSERT INTO exercise_feedback (
                homework_id, student_id, pose_type, uploaded_at, original_video_path,
                processed_video_path, total_count, correct_count, incorrect_count,
                feedback_json, video_duration
            ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
        """, (
            homework_id, student_id, pose_type, local_time, original_video_path,
            processed_video_path, total_count, correct_count, incorrect_count,
            feedback_json, video_duration
        ))
        record_id = cursor.lastrowid
    
    conn.commit()
    conn.close()
    
    return record_id

def update_exercise_feedback(
    record_id: int,
    total_count: Optional[int] = None,
    correct_count: Optional[int] = None,
    incorrect_count: Optional[int] = None,
    feedback_json: Optional[str] = None,
    video_duration: Optional[float] = None
) -> bool:
    """
    更新健身动作分析反馈记录
    
    Args:
        record_id: 记录ID
        total_count: 总动作次数
        correct_count: 正确动作次数
        incorrect_count: 错误动作次数
        feedback_json: AI反馈数据（JSON格式）
        video_duration: 视频时长（秒）
        
    Returns:
        bool: 更新是否成功
    """
    conn = sqlite3.connect(DB_PATH)
    cursor = conn.cursor()
    
    # 构建更新语句
    updates = []
    params = []
        
    if total_count is not None:
        updates.append("total_count = ?")
        params.append(total_count)
        
    if correct_count is not None:
        updates.append("correct_count = ?")
        params.append(correct_count)
        
    if incorrect_count is not None:
        updates.append("incorrect_count = ?")
        params.append(incorrect_count)
        
    if feedback_json is not None:
        updates.append("feedback_json = ?")
        params.append(feedback_json)
        
    if video_duration is not None:
        updates.append("video_duration = ?")
        params.append(video_duration)
    
    if not updates:
        return False
    
    params.append(record_id)
    update_sql = f"UPDATE exercise_feedback SET {', '.join(updates)} WHERE id = ?"
    
    cursor.execute(update_sql, params)
    conn.commit()
    success = cursor.rowcount > 0
    conn.close()
    
    return success

def get_exercise_feedback(homework_id: str, student_id: str, pose_type: str) -> Optional[Dict[str, Any]]:
    """
    根据作业ID、学生ID和动作类型查询反馈记录
    
    Args:
        homework_id: 作业ID
        student_id: 学生ID
        pose_type: 动作类型
        
    Returns:
        dict: 反馈记录信息，如果不存在则返回None
    """
    conn = sqlite3.connect(DB_PATH)
    conn.row_factory = sqlite3.Row  # 使结果可以通过列名访问
    cursor = conn.cursor()
    
    cursor.execute("""
        SELECT * FROM exercise_feedback 
        WHERE homework_id = ? AND student_id = ? AND pose_type = ?
    """, (homework_id, student_id, pose_type))
    
    row = cursor.fetchone()
    conn.close()
    
    if row is None:
        return None
    
    # 转换为字典
    return dict(row)

def get_exercise_feedback_by_id(record_id: int) -> Optional[Dict[str, Any]]:
    """
    根据记录ID查询反馈记录
    
    Args:
        record_id: 记录ID
        
    Returns:
        dict: 反馈记录信息，如果不存在则返回None
    """
    conn = sqlite3.connect(DB_PATH)
    conn.row_factory = sqlite3.Row  # 使结果可以通过列名访问
    cursor = conn.cursor()
    
    cursor.execute("SELECT * FROM exercise_feedback WHERE id = ?", (record_id,))
    
    row = cursor.fetchone()
    conn.close()
    
    if row is None:
        return None
    
    # 转换为字典
    return dict(row)