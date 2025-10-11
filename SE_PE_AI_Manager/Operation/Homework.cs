using Oracle.ManagedDataAccess.Client;
using SE_PE_AI_Manager.Basic;
using System;
using System.Reflection;

namespace SE_PE_AI_Manager.Operation
{
    public class Homework
    {
        public static Function_result New_homework(string teacher_id, string jwt, string course_id, string title, string description,string deadline, OracleConnection connection)//新建教学课程
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
            if (count != 1)
            {
                return_result.Code = -25;//-25表示要新建教学内容的课程不在数据库中，不准新建
                return_result.Message = "Course not found";
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
                        return_result.Message = "Teacher Not Found";
                        return return_result;
                    }
                }
            }
            catch (Exception)
            {
                return_result.Code = -14;//-14表示查询课程教师的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            if (ans != teacher_id)
            {
                return_result.Code = -27;//-27表示修改课程的教师并非课程所有者，不准修改
                return_result.Message = "Error owner teacher";
                return return_result;
            }
            //如果课程的确是当前教师拥有的，那么就查询当前作业列表的最大编号
            check = "SELECT max(id) FROM homework\n";
            count = 0;
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
                return_result.Code = -15;//-15表示查询最大作业编号的sql操作失败
                return_result.Message = "SQL Error";
                return return_result;
            }
            count++;//当前教学任务的ID为当前课程对应的最大教学任务ID+1
            //以下为数据库插入操作
            string action = "INSERT INTO homework(id, course_id, title, description, deadline, CREATE_TIME)\n";
            action = action + "values('" + Convert.ToString(count) + "',";
            action = action + "'" + course_id + "',";
            action = action + "'" + title + "',";
            action = action + "'" + description + "',";
            action = action + "to_date ('" + deadline + "','YYYY/MM/DD HH24:MI:SS'),";
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
                return_result.Code = -16;//-16表示插入作业的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            return_result.Code = 0;
            return_result.Message = "";
            return return_result;
        }

        public static Function_result Edit_homework(string teacher_id, string jwt, string course_id, string homework_id, string title, string description, string deadline, OracleConnection connection)//修改教学课程
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
            if (count != 1)
            {
                return_result.Code = -25;//-25表示要修改教学内容的课程不在数据库中，不准新建
                return_result.Message = "Course not found";
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
                        return_result.Message = "Teacher Not Found";
                        return return_result;
                    }
                }
            }
            catch (Exception)
            {
                return_result.Code = -14;//-14表示查询课程教师的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            if (ans != teacher_id)
            {
                return_result.Code = -27;//-27表示修改课程的教师并非课程所有者，不准修改
                return_result.Message = "Error Owner Teacher";
                return return_result;
            }
            //如果课程的确是当前教师拥有的，那么就查询当前要修改的作业号是否在当前课程中
            check = "SELECT course_id FROM homework WHERE id = '" + homework_id + "'\n";
            ans = "";
            try
            {
                using (OracleCommand command = new OracleCommand(check, connection))
                {
                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)//如果result非空
                    {
                        ans = Convert.ToString(result);//尝试转化为数字
                    }
                }
            }
            catch (Exception)
            {
                return_result.Code = -15;//-15表示查询教学任务是否在当前课程中的sql操作失败
                return_result.Message = "SQL Error";
                return return_result;
            }
            if(ans != course_id)
            {
                return_result.Code = -28;//-28表示当前作业不在当前课程中，不准修改
                return_result.Message = "Error Homework_ID";
                return return_result;
            }
            //以下为数据库更新操作
            string action = "update homework\n";
            action = action + "set title = '" + title + "',\n";
            action = action + "description = '" + description + "',\n";
            action = action + "to_date ('" + deadline + "','YYYY/MM/DD HH24:MI:SS'),";
            action = action + "where course_id = '" + course_id + "'\n";
            action = action + "and id = '" + homework_id + "'\n";
            Console.WriteLine( action );
            try
            {
                using (OracleCommand command = new OracleCommand(action, connection))
                {
                    command.ExecuteScalar();//执行插入操作
                }
            }
            catch (Exception)
            {
                return_result.Code = -16;//-16表示修改教学任务的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            return_result.Code = 0;
            return_result.Message = "";
            return return_result;
        }

        public static Function_result Delete_homework(string teacher_id, string jwt, string course_id, string homework_id, OracleConnection connection)//删除作业
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
            //登陆状态，存在性，权限都正确的话，验证当前作业是否存在
            check = "SELECT count(*) FROM homework WHERE id = '" + course_id + "'\n";
            check = check + "and id = '" + homework_id + "'";
            count = 0;
            try
            {
                using (OracleCommand command = new OracleCommand(check, connection))
                {
                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)//如果result非空
                    {
                        count = Convert.ToInt32(result);//尝试转化为数字
                    }
                }
            }
            catch (Exception)
            {
                return_result.Code = -15;//-15表示查询教学任务是否在当前课程中的sql操作失败
                return_result.Message = "SQL Error";
                return return_result;
            }
            if (count != 0)
            {
                return_result.Code = -28;//-28表示当前教学任务不在当前课程中，不准修改
                return_result.Message = "Error Class_ID";
                return return_result;
            }
            //鉴权，课程存在性，课程拥有者和教学内容存在性都满足的话，执行删除操作
            string action = "delete from homework\n";
            action = action + "where course_id = '" + course_id + "'\n";
            action = action + "and id = '" + homework_id + "'\n";
            try
            {
                using (OracleCommand command = new OracleCommand(action, connection))
                {
                    command.ExecuteScalar();//执行删除操作
                }
            }
            catch (Exception)
            {
                return_result.Code = -16;//-16表示删除教学内容的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            return_result.Code = 0;
            return_result.Message = "";
            return return_result;
        }

        public static Function_result Get_homework_id_by_course(string user_type, string user_id, string jwt, string course_id, OracleConnection connection)//用户查询一门课程包含的作业
        {
            Function_result return_result = new Function_result();
            return_result.Code = -99;
            return_result.Message = "Unknown Error";
            //先鉴权
            return_result = User.Check_jwt(Convert.ToInt32(user_type), user_id, jwt, connection);//执行鉴权操作
            if (return_result.Code < 0)//如果鉴权不通过
            {
                return return_result;//驳回操作
            }
            //鉴权通过的话，就先查询教学任务的数量
            string check = "SELECT COUNT(*) FROM homework WHERE course_id = '" + course_id + "'";
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
                return_result.Code = -13;//-13表示查询作业数量的sql操作无法顺利执行
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
            check = "SELECT id FROM homework WHERE course_id = '" + course_id + "'";
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
                return_result.Code = -14;//-14表示查询教作业ID的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            return_result.Code = count;
            return_result.Message = ans;
            return return_result;
        }

        public static Function_result Get_info_by_homework_id(string course_id, string homework_id, OracleConnection connection)//根据课程ID和作业ID获取作业信息
        {
            Function_result return_result = new Function_result();
            return_result.Code = -99;
            return_result.Message = "Unknown Error";
            string action = "SELECT title,description,deadline,create_time FROM homework WHERE course_id = '" + course_id + "'";
            action = action + "and id = '" + homework_id + "'\n";
            string ans = "";
            try
            {
                using (OracleCommand cmd = new OracleCommand(action, connection))
                {
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            ans = ans + reader["title"].ToString() + "\t\r";
                            ans = ans + reader["description"].ToString() + "\t\r";
                            ans = ans + reader["deadline"].ToString() + "\t\r";
                            ans = ans + reader["create_time"].ToString() + "\t\r";
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
