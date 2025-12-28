import sqlite3
import time

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

    # 创建消息表
    cursor.execute('''
            CREATE TABLE IF NOT EXISTS messages (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                session_id INTEGER NOT NULL,
                role TEXT NOT NULL,  -- user/assistant/system
                content TEXT NOT NULL,
                model TEXT,
                timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (session_id) REFERENCES sessions (id) ON DELETE CASCADE
            )
        ''')

    # 检查messages表是否已有model字段，如果没有则添加
    try:
        cursor.execute("ALTER TABLE messages ADD COLUMN model TEXT")
    except sqlite3.OperationalError:
        # 字段可能已经存在，忽略错误
        pass
    
    # 创建AI分析报告表
    cursor.execute('''
        CREATE TABLE IF NOT EXISTS ai_analysis_reports (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            homework_id TEXT,
            student_id TEXT NOT NULL,
            pose_type TEXT,
            analysis_type TEXT,
            query_content TEXT,
            report_content TEXT,
            raw_data TEXT,
            model_used TEXT,
            student_info TEXT,
            created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
            updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
            
            -- 建立复合唯一索引，避免重复生成相同报告
            UNIQUE(homework_id, student_id, analysis_type)
        )
    ''')
    
    # 检查ai_analysis_reports表是否已有student_info字段，如果没有则添加
    try:
        cursor.execute("ALTER TABLE ai_analysis_reports ADD COLUMN student_info TEXT")
    except sqlite3.OperationalError:
        # 字段可能已经存在，忽略错误
        pass
    
    conn.commit()
    conn.close()