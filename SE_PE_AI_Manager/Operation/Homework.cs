using Oracle.ManagedDataAccess.Client;
using SE_PE_AI_Manager.Basic;
using System;
using System.Globalization;
using System.Reflection;

namespace SE_PE_AI_Manager.Operation
{
    public class Homework
    {
        public static Function_result New_homework(string teacher_id, string jwt, string course_id, string title, string description,string deadline, OracleConnection connection)//新建作业
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

        public static Function_result Edit_homework(string teacher_id, string jwt, string course_id, string homework_id, string title, string description, string deadline, OracleConnection connection)//修改作业
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

        public static Function_result Submit_homework(string student_id, string jwt, string course_id, string homework_id, string video_url, OracleConnection connection)//学生提交作业
        {
            Function_result return_result = new Function_result();
            return_result.Code = -99;
            return_result.Message = "Unknown Error";
            //先鉴权
            return_result = User.Check_jwt(0, student_id, jwt, connection);//执行鉴权操作
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
                return_result.Code = -25;//-25表示要提交作业的课程不在数据库中，不准新建
                return_result.Message = "Course not found";
                return return_result;
            }
            //若课程存在，就验当前课程是否是当前学生所加入的
            check = "SELECT count(*) FROM student_course WHERE course_id = '" + course_id + "'\n";
            check = check + "and student_id = '" + student_id + "'\n";
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
                    else
                    {
                        return_result.Code = -26;//-26表示查询学生是否已经加入课程的操作成功，但是没有结果
                        return_result.Message = "Student Not Found";
                        return return_result;
                    }
                }
            }
            catch (Exception)
            {
                return_result.Code = -14;//-14表示查询学生是否加入当前课程的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            if (count != 1)
            {
                return_result.Code = -27;//-27表示学生并未加入当前课程，不准提交
                return_result.Message = "Error Student";
                return return_result;
            }
            //如果课程的确是当前学生加入的，那么就先检查当前作业是不是属于当前课程的
            check = "SELECT course_id FROM homework WHERE id = '" + homework_id + "'\n";
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
                        return_result.Code = -28;//-28表示查询作业对应课程的操作成功，但是没有结果
                        return_result.Message = "Student Not Found";
                        return return_result;
                    }
                }
            }
            catch (Exception)
            {
                return_result.Code = -15;//-15表示查询作业对应课程的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            if (ans != course_id)
            {
                return_result.Code = -29;//-29表示课程作业不对应，不准提交
                return_result.Message = "Error Student";
                return return_result;
            }
            //提交是否超时
            DateTime currentTime = DateTime.Now;//获取当前时间
            //查询作业的截至时间
            check = "SELECT deadline FROM homework WHERE id = '" + homework_id + "'";
            ans = "";
            try
            {
                using (OracleCommand command = new OracleCommand(check, connection))
                {
                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)//如果result非空
                    {
                        ans = Convert.ToString(result);//尝试转化为字符串
                    }
                    else
                    {
                        return_result.Code = -28;//-26表示查询课程截止时间的操作成功，但是没有结果
                        return_result.Message = "Deadline Not Found";
                        return return_result;
                    }
                }
            }
            catch (Exception)
            {
                return_result.Code = -16;//-16表示查询作业截至时间的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            DateTime deadline = DateTime.ParseExact(
                ans,
                "yyyy/MM/dd HH:mm:ss",
                CultureInfo.InvariantCulture
            );
            if (currentTime > deadline)
            {
                return_result.Code = -30;//-30表示学生提交时作业已经截至，不准提交
                return_result.Message = "TLE";
                return return_result;
            }
            //如果所有校验都通过，那么就查询当前作业列表的最大编号
            check = "SELECT max(id) FROM submit\n";
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
                return_result.Code = -17;//-17表示查询最大提交编号的sql操作失败
                return_result.Message = "SQL Error";
                return return_result;
            }
            count++;//当前提交的ID为当前最大的提交ID+1
            //以下为数据库插入操作
            string action = "INSERT INTO submit(id, homework_id, student_id, VIDEO_URL, CREATE_TIME)\n";
            action = action + "values('" + Convert.ToString(count) + "',";
            action = action + "'" + homework_id + "',";
            action = action + "'" + student_id + "',";
            action = action + "'" + video_url + "',";
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
                return_result.Code = -18;//-17表示提交作业的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            return_result.Code = 0;
            return_result.Message = "";
            return return_result;
        }

        public static Function_result Get_submit_id_by_homework(string user_type, string user_id, string jwt, string homework_id, OracleConnection connection)//教师或者AI查询一门科的所有提交编号
        {
            Function_result return_result = new Function_result();
            return_result.Code = -99;
            return_result.Message = "Unknown Error";
            //先鉴权
            if (Convert.ToInt32(user_type) != 3)//只要不是AI在查询 
            {
                return_result = User.Check_jwt(1, user_id, jwt, connection);//执行鉴权操作
                if (return_result.Code < 0)//如果鉴权不通过
                {
                    return return_result;//驳回操作
                }
            }
            //鉴权通过的话，就先查询当前作业的存在性
            string check = "SELECT COUNT(*) FROM homework WHERE id = '" + homework_id + "'";
            int count = 0;
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
                return_result.Code = -13;//-13表示当前作业存在性的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            if (count == 0)
            {
                return_result.Code = -25;//为0表示当前作业不存在
                return_result.Message = "Homework Not found";
                return return_result;
            }
            check = "SELECT COUNT(*) FROM submit WHERE homework_id = '" + homework_id + "'";
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
                return_result.Code = -14;//-14表示查询提交数量的sql操作无法顺利执行
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
            check = "SELECT id FROM submit WHERE homework_id = '" + homework_id + "'";
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
                return_result.Code = -15;//-15表示查询提交ID的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            return_result.Code = count;
            return_result.Message = ans;
            return return_result;
        }

        public static Function_result Get_submit_id_by_AI_not_test(string homework_id, OracleConnection connection)//AI查询一门课的所有未评分的提交
        {
            Function_result return_result = new Function_result();
            return_result.Code = -99;
            return_result.Message = "Unknown Error";
            //就先查询当前作业的存在性
            string check = "SELECT COUNT(*) FROM homework WHERE id = '" + homework_id + "'";
            int count = 0;
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
                return_result.Code = -13;//-13表示当前作业存在性的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            if (count == 0)
            {
                return_result.Code = -25;//为0表示当前作业不存在
                return_result.Message = "Homework Not found";
                return return_result;
            }
            check = "SELECT COUNT(*) FROM submit WHERE homework_id = '" + homework_id + "'\n";
            check = check + "and AI_FEEDBACK IS NULL\n";
            count = 0;
            Console.WriteLine(check);
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
                return_result.Code = -14;//-14表示查询提交数量的sql操作无法顺利执行
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
            check = "SELECT id FROM submit WHERE homework_id = '" + homework_id + "'\n";
            check = check + "and AI_FEEDBACK IS NULL\n";
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
                return_result.Code = -15;//-15表示查询提交ID的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            return_result.Code = count;
            return_result.Message = ans;
            return return_result;
        }

        public static Function_result Get_submit_id_by_teacher_not_test(string homework_id, OracleConnection connection)//查询一门课的所有未被老师评分的提交
        {
            Function_result return_result = new Function_result();
            return_result.Code = -99;
            return_result.Message = "Unknown Error";
            //就先查询当前作业的存在性
            string check = "SELECT COUNT(*) FROM homework WHERE id = '" + homework_id + "'";
            int count = 0;
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
                return_result.Code = -13;//-13表示当前作业存在性的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            if (count == 0)
            {
                return_result.Code = -25;//为0表示当前作业不存在
                return_result.Message = "Homework Not found";
                return return_result;
            }
            check = "SELECT COUNT(*) FROM submit WHERE homework_id = '" + homework_id + "'\n";
            check = check + "and teacher_FEEDBACK IS NULL\n";
            count = 0;
            Console.WriteLine(check);
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
                return_result.Code = -14;//-14表示查询提交数量的sql操作无法顺利执行
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
            check = "SELECT id FROM submit WHERE homework_id = '" + homework_id + "'\n";
            check = check + "and teacher_FEEDBACK IS NULL\n";
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
                return_result.Code = -15;//-15表示查询提交ID的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            return_result.Code = count;
            return_result.Message = ans;
            return return_result;
        }

        public static Function_result Set_AI_type(string teacher_id, string jwt, string course_id, string homework_id, string AI_type, OracleConnection connection)//给作业设置AI测试类型
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
            if (ans != course_id)
            {
                return_result.Code = -28;//-28表示当前作业不在当前课程中，不准修改
                return_result.Message = "Error Homework_ID";
                return return_result;
            }
            //以下为数据库插入操作
            string action = "INSERT INTO AI_type(homework_id, type)\n";
            action = action + "values('" + homework_id + "',";
            action = action + "'" + AI_type + "')\n";
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

        public static Function_result Edit_AI_type(string teacher_id, string jwt, string course_id, string homework_id, string AI_type, OracleConnection connection)//给作业修改AI测试类型
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
            if (ans != course_id)
            {
                return_result.Code = -28;//-28表示当前作业不在当前课程中，不准修改
                return_result.Message = "Error Homework_ID";
                return return_result;
            }
            //以下为数据库修改操作
            string action = "update AI_type\n";
            action = action + "set type = '" + AI_type + "'\n";
            action = action + "where homework_id = '" + homework_id + "'\n";
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

        public static Function_result Get_AI_type(string homework_id, OracleConnection connection)//查询一门课的AI评测基准
        {
            Function_result return_result = new Function_result();
            return_result.Code = -99;
            return_result.Message = "Unknown Error";
            //就先查询当前作业的存在性
            string check = "SELECT COUNT(*) FROM homework WHERE id = '" + homework_id + "'";
            int count = 0;
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
                return_result.Code = -11;//-11表示当前作业存在性的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            if (count == 0)
            {
                return_result.Code = -21;//为0表示当前作业不存在
                return_result.Message = "Homework Not found";
                return return_result;
            }
            check = "SELECT COUNT(*) FROM submit WHERE homework_id = '" + homework_id + "'\n";
            check = check + "and teacher_FEEDBACK IS NULL\n";
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
                return_result.Code = -12;//-14表示查询提交数量的sql操作无法顺利执行
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
            check = "SELECT type FROM AI_type WHERE homework_id = '" + homework_id + "'\n";
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
                            ans += reader["type"].ToString();
                        }
                    }
                }
            }
            catch (Exception)
            {
                return_result.Code = -13;//-15表示查询提交ID的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            return_result.Code = count;
            return_result.Message = ans;
            return return_result;
        }

        public static Function_result AI_test(string submit_id, string VIDEO_URL, string score, string AI_feedback, OracleConnection connection)//AI执行测试后将结果写回
        {
            Function_result return_result = new Function_result();
            return_result.Code = -99;
            return_result.Message = "Unknown Error";
            //先验证提交记录是否存在
            string check = "SELECT count(*) FROM submit WHERE id = '" + submit_id + "'\n";
            int count = 0;
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
                return_result.Code = -11;//-15表示查询教学任务是否在当前课程中的sql操作失败
                return_result.Message = "SQL Error";
                return return_result;
            }
            if (count != 1)
            {
                return_result.Code = -21;//-28表示当前作业不在当前课程中，不准修改
                return_result.Message = "Error Homework_ID";
                return return_result;
            }
            //以下为数据库更新操作
            string action = "update submit\n";
            action = action + "set VIDEO_URL = '" + VIDEO_URL + "',\n";
            action = action + "score = '" + score + "',\n";
            action = action + "AI_feedback = '" + AI_feedback + "'\n";
            action = action + "where id = '" + submit_id + "'\n";
            Console.WriteLine(action);
            try
            {
                using (OracleCommand command = new OracleCommand(action, connection))
                {
                    command.ExecuteScalar();//执行插入操作
                }
            }
            catch (Exception)
            {
                return_result.Code = -12;//-12表示修改评价的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            return_result.Code = 0;
            return_result.Message = "";
            return return_result;
        }

        public static Function_result Teacher_test(string teacher_id, string jwt, string course_id, string homework_id, string submit_id, string score, string teacher_feedback, OracleConnection connection)//教师执行测试后将结果写回
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
                return_result.Code = -27;//-27表示评分的教师并非课程所有者，不准修改
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
            if (ans != course_id)
            {
                return_result.Code = -28;//-28表示当前作业不在当前课程中，不准修改
                return_result.Message = "Error Homework_ID";
                return return_result;
            }
            //再检查当前的提交记录是否属于当前作业
            check = "SELECT homework_id FROM submit WHERE id = '" + submit_id + "'\n";
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
                return_result.Code = -16;//-16表示查询提交是否在当前作业中的sql操作失败
                return_result.Message = "SQL Error";
                return return_result;
            }
            if (ans != homework_id)
            {
                return_result.Code = -29;//-29表示当前提交不在当前作业中，不准评分
                return_result.Message = "Error Homework_ID";
                return return_result;
            }
            //以下为数据库更新操作
            string action = "update submit\n";
            action = action + "set score = '" + score + "',\n";
            action = action + "teacher_feedback = '" + teacher_feedback + "'\n";
            action = action + "where id = '" + submit_id + "'\n";
            Console.WriteLine(action);
            try
            {
                using (OracleCommand command = new OracleCommand(action, connection))
                {
                    command.ExecuteScalar();//执行插入操作
                }
            }
            catch (Exception)
            {
                return_result.Code = -17;//-17教师评分的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            return_result.Code = 0;
            return_result.Message = "";
            return return_result;
        }

        public static Function_result Get_submit_id_by_student(string user_type, string user_id, string jwt, string homework_id, string student_id, OracleConnection connection)//查询一次作业某个学生的所有提交编号
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
            //如果是学生在查看，则限定只能他自己看
            if (user_id != student_id && user_type == "0")
            {
                return_result.Code = -25;
                return_result.Message = "No permission";//-25表示无权查看
                return return_result;   
            }
            //鉴权通过的话，就先查询当前作业的存在性
            string check = "SELECT COUNT(*) FROM homework WHERE id = '" + homework_id + "'";
            int count = 0;
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
                return_result.Code = -13;//-13表示当前作业存在性的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            if (count == 0)
            {
                return_result.Code = -26;//为0表示当前作业不存在
                return_result.Message = "Homework Not found";
                return return_result;
            }
            check = "SELECT COUNT(*) FROM submit WHERE homework_id = '" + homework_id + "'\n";
            check = check + "and student_id = '" + student_id + "'\n";
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
                return_result.Code = -14;//-14表示查询提交数量的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            if (count == 0)
            {
                return_result.Code = 0;//为0就直接返回值，不用后续查询，因为没有
                return_result.Message = "";
                return return_result;
            }
            //若提交记录存在，获得查询值
            check = "SELECT id FROM submit WHERE homework_id = '" + homework_id + "'\n";
            check = check + "and student_id = '" + student_id + "'\n";
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
                return_result.Code = -15;//-15表示查询提交ID的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            return_result.Code = count;
            return_result.Message = ans;
            return return_result;
        }

        public static Function_result Get_submit_info(string user_type, string user_id, string jwt, string submit_id, OracleConnection connection)//查询某条提交记录的具体信息
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
            string action = "select VIDEO_URL,SCORE,AI_FEEDBACK,TEACHER_FEEDBACK,CREATE_TIME from submit\n";
            action = action + "where id = ' " + submit_id + "'\n";
            string ans = "";
            try
            {
                using (OracleCommand cmd = new OracleCommand(action, connection))
                {
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            ans = ans + reader["VIDEO_URL"].ToString() + "\t\r";
                            ans = ans + reader["SCORE"].ToString() + "\t\r";
                            ans = ans + reader["AI_FEEDBACK"].ToString() + "\t\r";
                            ans = ans + reader["TEACHER_FEEDBACK"].ToString() + "\t\r";
                            ans = ans + reader["CREATE_TIME"].ToString() + "\t\r";
                        }
                        else
                        {
                            return_result.Code = -25;
                            return_result.Message = "No Result";
                            return return_result;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return_result.Code = -14;//-14表示查询提交信息的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            return_result.Code = 0;
            return_result.Message = ans;
            return return_result;
        }

        public static Function_result Get_final_score(string user_type, string user_id, string jwt, string student_id, string homework_id, OracleConnection connection)//查询某个学生某项作业的最终得分
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
            string action = "select id,score from final_grade\n";
            action = action + "where student_id = '" + student_id + "'\n";
            action = action + "and homework_id = '" + homework_id + "'\n";
            int count = 0;
            string id = "";
            string ans = "";
            try
            {
                using (OracleCommand cmd = new OracleCommand(action, connection))
                {
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        if(reader.Read())
                        {
                            id = reader["ID"].ToString();
                            ans = reader["score"].ToString();
                        }
                        else
                        {
                            return_result.Code = -25;//为0表示当前作业不存在
                            return_result.Message = "Homework Not found";
                            return return_result;
                        }
                    }
                    if (ans != null)//如果ans非空
                    {
                        count = Convert.ToInt32(ans);//尝试转化为数字
                    }
                    else
                    {
                        return_result.Code = -26;//ans为空表示当前作业还没有被批阅过的作业
                        return_result.Message = "No Judgement";
                        return return_result;
                    }
                }
            }
            catch (Exception)
            {
                return_result.Code = -13;//-13表示当前查询成绩的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            return_result.Code = 0;
            return_result.Message = id + "\t\r" + Convert.ToString(count);
            return return_result;
        }
    }
}
