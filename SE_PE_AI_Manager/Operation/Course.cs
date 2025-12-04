using Oracle.ManagedDataAccess.Client;
using SE_PE_AI_Manager.Basic;
using System.Reflection;

namespace SE_PE_AI_Manager.Operation
{
    public class Course
    {
        public static Function_result New_course(string course_id, string teacher_id, string jwt, string name, string semester, OracleConnection connection)//新建课程（基本信息）
        {
            Function_result return_result = new Function_result();
            return_result.Code = -99;
            return_result.Message = "Unknown Error";
            //先鉴权
            return_result = User.Check_jwt(1, teacher_id, jwt, connection);//执行鉴权操作
            if (return_result.Code < 0)//如果鉴权不通过
            {
                return return_result;//驳回操作
            }
            //鉴权通过的话，就验证课程的存在性
            string check = "SELECT COUNT(*) FROM std_course WHERE id = '" + course_id + "'";
            int count = 0;
            try
            {
                using (OracleCommand command = new OracleCommand(check, connection))
                {
                    object result = command.ExecuteScalar();//查询同名用户数量
                    if (result != null && result != DBNull.Value)//如果result非空
                    {
                        count = Convert.ToInt32(result);//尝试转化为数字
                    }
                }
            }
            catch (Exception)
            {
                return_result.Code = -13;//-13表示查询课程是否存在的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            if (count == 0)
            {
                return_result.Code = -25;//-25表示要新建的课程不在基准数据库中，不准新建
                return_result.Message = "User not found";
                return return_result;
            }
            string code = course_id;
            code = code + teacher_id;
            code = Basic_calculate.ComputeSHA256(code);//计算课程的邀请码
            //以下为数据库插入操作
            string action = "INSERT INTO course(id, teacher_id, name, code, semester, is_active, CREATED_TIME)\n";
            action = action + "values('" + course_id + "',";
            action = action + "'" + teacher_id + "',";
            action = action + "'" + name + "',";
            action = action + "'" + code + "',";
            action = action + "'" + semester + "',";
            action = action + "'" + "0" + "',";//默认建立后未发布
            action = action + "SYSDATE)\n";
            try
            {
                using (OracleCommand command = new OracleCommand(action, connection))
                {
                    command.ExecuteScalar();//执行插入操作
                }
            }
            catch (Exception)
            {
                return_result.Code = -14;//-14表示插入课程的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            return_result.Code = 0;
            return_result.Message = code;
            return return_result;
        }

        public static Function_result Edit_course(string course_id, string teacher_id, string jwt, string name, string semester, OracleConnection connection)//修改课程（基本信息）
        {
            Function_result return_result = new Function_result();
            return_result.Code = -99;
            return_result.Message = "Unknown Error";
            //先鉴权
            return_result = User.Check_jwt(1, teacher_id, jwt, connection);//执行鉴权操作
            if (return_result.Code < 0)//如果鉴权不通过
            {
                return return_result;//驳回操作
            }
            //鉴权通过的话，就验证课程的存在性
            string check = "SELECT COUNT(*) FROM course WHERE id = '" + course_id + "'";
            int count = 0;
            try
            {
                using (OracleCommand command = new OracleCommand(check, connection))
                {
                    object result = command.ExecuteScalar();//查询同名用户数量
                    if (result != null && result != DBNull.Value)//如果result非空
                    {
                        count = Convert.ToInt32(result);//尝试转化为数字
                    }
                }
            }
            catch (Exception)
            {
                return_result.Code = -13;//-13表示查询课程是否存在的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            if (count == 0)
            {
                return_result.Code = -25;//-25表示要修改的课程不在课程库中，不准修改
                return_result.Message = "User not found";
                return return_result;
            }
            //若课程存在，就验当前课程是否是当前老师所拥有的
            check = "SELECT teacher_id FROM course WHERE id = '" + course_id + "'";
            string ans = "";
            try
            {
                using (OracleCommand command = new OracleCommand(check, connection))
                {
                    object result = command.ExecuteScalar();//查询同名用户数量
                    if (result != null && result != DBNull.Value)//如果result非空
                    {
                        ans = Convert.ToString(result);//尝试转化为数字
                    }
                    else
                    {
                        return_result.Code = -26;//-26表示查询课程教师的操作成功，但是没有结果
                        return_result.Message = "SQL Error";
                        return return_result;
                    }
                }
            }
            catch (Exception)
            {
                return_result.Code = -14;//-13表示查询课程教师的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            if (ans != teacher_id)
            {
                return_result.Code = -27;//-27表示修改课程的教师并非课程所有者，不准修改
                return_result.Message = "SQL Error";
                return return_result;
            }
            //登陆状态，存在性，权限都正确的话，允许执行修改
            string action = "update course\n";
            action = action + "set name = '" + name + "',\n";
            action = action + "semester = '" + semester + "'\n";
            action = action + "where id = '" + course_id + "'\n";
            try
            {
                using (OracleCommand command = new OracleCommand(action, connection))
                {
                    command.ExecuteScalar();//执行修改操作
                }
            }
            catch (Exception)
            {
                return_result.Code = -15;//-15表示修改课程基本信息的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            return_result.Code = 0;
            return_result.Message = "";
            return return_result;
        }

        public static Function_result Edit_info(string course_id, string teacher_id, string jwt, string info, OracleConnection connection)//修改课程的描述信息
        {
            Function_result return_result = new Function_result();
            return_result.Code = -99;
            return_result.Message = "Unknown Error";
            //先鉴权
            return_result = User.Check_jwt(1, teacher_id, jwt, connection);//执行鉴权操作
            if (return_result.Code < 0)//如果鉴权不通过
            {
                return return_result;//驳回操作
            }
            //鉴权通过的话，就验证课程的存在性
            string check = "SELECT COUNT(*) FROM course WHERE id = '" + course_id + "'";
            int count = 0;
            try
            {
                using (OracleCommand command = new OracleCommand(check, connection))
                {
                    object result = command.ExecuteScalar();//查询同名用户数量
                    if (result != null && result != DBNull.Value)//如果result非空
                    {
                        count = Convert.ToInt32(result);//尝试转化为数字
                    }
                }
            }
            catch (Exception)
            {
                return_result.Code = -13;//-13表示查询课程是否存在的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            if (count == 0)
            {
                return_result.Code = -25;//-25表示要修改的课程不在课程库中，不准修改
                return_result.Message = "User not found";
                return return_result;
            }
            //若课程存在，就验当前课程是否是当前老师所拥有的
            check = "SELECT teacher_id FROM course WHERE id = '" + course_id + "'";
            string ans = "";
            try
            {
                using (OracleCommand command = new OracleCommand(check, connection))
                {
                    object result = command.ExecuteScalar();//查询同名用户数量
                    if (result != null && result != DBNull.Value)//如果result非空
                    {
                        ans = Convert.ToString(result);//尝试转化为数字
                    }
                    else
                    {
                        return_result.Code = -26;//-26表示查询课程教师的操作成功，但是没有结果
                        return_result.Message = "SQL Error";
                        return return_result;
                    }
                }
            }
            catch (Exception)
            {
                return_result.Code = -14;//-13表示查询课程教师的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            if (ans != teacher_id)
            {
                return_result.Code = -27;//-27表示修改课程的教师并非课程所有者，不准修改
                return_result.Message = "SQL Error";
                return return_result;
            }
            //登陆状态，存在性，权限都正确的话，允许执行修改
            string action = "update course\n";
            action = action + "set info = '" + info + "'\n";
            action = action + "where id = '" + course_id + "'\n";
            try
            {
                using (OracleCommand command = new OracleCommand(action, connection))
                {
                    command.ExecuteScalar();//执行修改操作
                }
            }
            catch (Exception)
            {
                return_result.Code = -15;//-15表示修改课程描述信息的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            return_result.Code = 0;
            return_result.Message = "";
            return return_result;
        }

        public static Function_result Edit_is_active(string course_id, string teacher_id, string jwt, string is_active, OracleConnection connection)//修改课程发布状态
        {
            Function_result return_result = new Function_result();
            return_result.Code = -99;
            return_result.Message = "Unknown Error";
            //先鉴权
            return_result = User.Check_jwt(1, teacher_id, jwt, connection);//执行鉴权操作
            if (return_result.Code < 0)//如果鉴权不通过
            {
                return return_result;//驳回操作
            }
            //鉴权通过的话，就验证课程的存在性
            string check = "SELECT COUNT(*) FROM course WHERE id = '" + course_id + "'";
            int count = 0;
            try
            {
                using (OracleCommand command = new OracleCommand(check, connection))
                {
                    object result = command.ExecuteScalar();//查询同名用户数量
                    if (result != null && result != DBNull.Value)//如果result非空
                    {
                        count = Convert.ToInt32(result);//尝试转化为数字
                    }
                }
            }
            catch (Exception)
            {
                return_result.Code = -13;//-13表示查询课程是否存在的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            if (count == 0)
            {
                return_result.Code = -25;//-25表示要修改的课程不在课程库中，不准修改
                return_result.Message = "User not found";
                return return_result;
            }
            //若课程存在，就验当前课程是否是当前老师所拥有的
            check = "SELECT teacher_id FROM course WHERE id = '" + course_id + "'";
            string ans = "";
            try
            {
                using (OracleCommand command = new OracleCommand(check, connection))
                {
                    object result = command.ExecuteScalar();//查询同名用户数量
                    if (result != null && result != DBNull.Value)//如果result非空
                    {
                        ans = Convert.ToString(result);//尝试转化为数字
                    }
                    else
                    {
                        return_result.Code = -26;//-26表示查询课程教师的操作成功，但是没有结果
                        return_result.Message = "SQL Error";
                        return return_result;
                    }
                }
            }
            catch (Exception)
            {
                return_result.Code = -14;//-13表示查询课程教师的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            if (ans != teacher_id)
            {
                return_result.Code = -27;//-27表示修改课程的教师并非课程所有者，不准修改
                return_result.Message = "SQL Error";
                return return_result;
            }
            //登陆状态，存在性，权限都正确的话，允许执行修改
            //先判定参数是否合规
            int attitude = Convert.ToInt32(is_active);
            if (attitude != 0 && attitude != 1) 
            {
                return_result.Code = -28;//-28表示要修改的课程状态无效，不准修改
                return_result.Message = "Attitude Error";
                return return_result;
            }
            string action = "update course\n";
            action = action + "set is_active = '" + is_active + "'\n";
            action = action + "where id = '" + course_id + "'\n";
            try
            {
                using (OracleCommand command = new OracleCommand(action, connection))
                {
                    command.ExecuteScalar();//执行修改操作
                }
            }
            catch (Exception)
            {
                return_result.Code = -15;//-15表示修改课程描述信息的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            return_result.Code = 0;
            return_result.Message = "";
            return return_result;
        }

        public static Function_result Delete_course(string course_id, string teacher_id, string jwt, OracleConnection connection)//删除课程
        {
            Function_result return_result = new Function_result();
            return_result.Code = -99;
            return_result.Message = "Unknown Error";
            //先鉴权
            return_result = User.Check_jwt(1, teacher_id, jwt, connection);//执行鉴权操作
            if (return_result.Code < 0)//如果鉴权不通过
            {
                return return_result;//驳回操作
            }
            //鉴权通过的话，就验证课程的存在性
            string check = "SELECT COUNT(*) FROM course WHERE id = '" + course_id + "'";
            int count = 0;
            try
            {
                using (OracleCommand command = new OracleCommand(check, connection))
                {
                    object result = command.ExecuteScalar();//查询同名用户数量
                    if (result != null && result != DBNull.Value)//如果result非空
                    {
                        count = Convert.ToInt32(result);//尝试转化为数字
                    }
                }
            }
            catch (Exception)
            {
                return_result.Code = -13;//-13表示查询课程是否存在的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            if (count == 0)
            {
                return_result.Code = -25;//-25表示要修改的课程不在课程库中，不准修改
                return_result.Message = "User not found";
                return return_result;
            }
            //若课程存在，就验当前课程是否是当前老师所拥有的
            check = "SELECT teacher_id FROM course WHERE id = '" + course_id + "'";
            string ans = "";
            try
            {
                using (OracleCommand command = new OracleCommand(check, connection))
                {
                    object result = command.ExecuteScalar();//查询同名用户数量
                    if (result != null && result != DBNull.Value)//如果result非空
                    {
                        ans = Convert.ToString(result);//尝试转化为数字
                    }
                    else
                    {
                        return_result.Code = -26;//-26表示查询课程教师的操作成功，但是没有结果
                        return_result.Message = "SQL Error";
                        return return_result;
                    }
                }
            }
            catch (Exception)
            {
                return_result.Code = -14;//-13表示查询课程教师的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            if (ans != teacher_id)
            {
                return_result.Code = -27;//-27表示修改课程的教师并非课程所有者，不准修改
                return_result.Message = "SQL Error";
                return return_result;
            }
            //登陆状态，存在性，权限都正确的话，允许执行删除
            string action = "delete from course\n";
            action = action + "where id = '" + course_id + "'\n";
            try
            {
                using (OracleCommand command = new OracleCommand(action, connection))
                {
                    command.ExecuteScalar();//执行删除操作
                }
            }
            catch (Exception)
            {
                return_result.Code = -15;//-15表示修改课程描述信息的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            return_result.Code = 0;
            return_result.Message = "";
            return return_result;
        }

        public static Function_result Get_course_id_by_teacher(string teacher_id, string jwt, OracleConnection connection)//教师查询属于自己的课程
        {
            Function_result return_result = new Function_result();
            return_result.Code = -99;
            return_result.Message = "Unknown Error";
            //先鉴权
            return_result = User.Check_jwt(1, teacher_id, jwt, connection);//执行鉴权操作
            if (return_result.Code < 0)//如果鉴权不通过
            {
                return return_result;//驳回操作
            }
            //鉴权通过的话，就先查询课程的数量
            string check = "SELECT COUNT(*) FROM course WHERE teacher_id = '" + teacher_id + "'";
            int count = 0;
            try
            {
                using (OracleCommand command = new OracleCommand(check, connection))
                {
                    object result = command.ExecuteScalar();//查询同名用户数量
                    if (result != null && result != DBNull.Value)//如果result非空
                    {
                        count = Convert.ToInt32(result);//尝试转化为数字
                    }
                }
            }
            catch (Exception)
            {
                return_result.Code = -13;//-13表示查询课程是否存在的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            if (count == 0)
            {
                return_result.Code = 0;//为0就直接返回值，不用后续查询，因为没有
                return_result.Message = "";
                return return_result;
            }
            //若课程存在，获得查询值
            check = "SELECT id FROM course WHERE teacher_id = '" + teacher_id + "'";
            string ans = "";
            try
            {
                using (OracleCommand cmd = new OracleCommand(check, connection))
                {
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        bool first = true;
                        while (reader.Read())
                        {
                            if (!first)
                            {
                                ans += "\t\r"; // 每两个结果之间用\t\r间隔
                            }
                            else
                            {
                                first = false;
                            }

                            ans += reader["id"].ToString();
                        }
                    }
                }
            }
            catch (Exception)
            {
                return_result.Code = -14;//-14表示查询课程ID的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            return_result.Code = count;
            return_result.Message = ans;
            return return_result;
        }

        public static Function_result Get_info_by_course_id(string course_id, OracleConnection connection)//根据课程ID获取课程信息
        {
            Function_result return_result = new Function_result();
            return_result.Code = -99;
            return_result.Message = "Unknown Error";
            string action = "SELECT teacher_id,name,info,code,semester,is_active,created_time FROM course WHERE id = '" + course_id + "'";
            string ans = "";
            try
            {
                using (OracleCommand cmd = new OracleCommand(action, connection))
                {
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            ans = ans + reader["teacher_id"].ToString() + "\t\r";
                            ans = ans + reader["name"].ToString() + "\t\r";
                            ans = ans + reader["info"].ToString() + "\t\r";
                            ans = ans + reader["code"].ToString() + "\t\r";
                            ans = ans + reader["semester"].ToString() + "\t\r";
                            ans = ans + reader["is_active"].ToString() + "\t\r";
                            ans = ans + reader["created_time"].ToString() + "\t\r";
                        }
                        else
                        {
                            return_result.Code = -21;
                            return_result.Message = "Read Error";
                            return return_result;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return_result.Code = -11;//-11表示查询课程信息的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            return_result.Code = 0;
            return_result.Message = ans;
            return return_result;
        }
    }
}
