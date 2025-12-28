using Oracle.ManagedDataAccess.Client;
using SE_PE_AI_Manager.Basic;
using System.Reflection;

namespace SE_PE_AI_Manager.Operation
{
    public class Course_student
    {
        public static Function_result Add_course(string student_id, string jwt, string course_code, OracleConnection connection)//学生加入课程
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
            string check = "SELECT COUNT(*) FROM course WHERE code = '" + course_code + "'";
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
                return_result.Code = -13;//-13表示查询课程是否存在的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            if (count != 1)
            {
                return_result.Code = -25;//-25表示要加入的班级不存在或数量不正确，不准加入
                return_result.Message = "User not found";
                return return_result;
            }
            check = "SELECT id FROM course WHERE code = '" + course_code + "'";
            string ans = "";
            try
            {
                using (OracleCommand command = new OracleCommand(check, connection))
                {
                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)//如果result非空
                    {
                        ans = Convert.ToString(result);
                    }
                }
            }
            catch (Exception)
            {
                return_result.Code = -14;//-14表示查询课程ID的SQL操作错误
                return_result.Message = "SQL Error";
                return return_result;
            }
            check = "SELECT max(id) FROM student_course";
            int now_id = 0;
            try
            {
                using (OracleCommand command = new OracleCommand(check, connection))
                {
                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)//如果result非空
                    {
                        now_id = Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception)
            {
                return_result.Code = -15;//-15表示查询当前最大ID的操作错误
                return_result.Message = "SQL Error";
                return return_result;
            }
            now_id++;
            //以下为数据库插入操作
            string action = "INSERT INTO student_course(id, student_id, course_id, joined_TIME)\n";
            action = action + "values('" + Convert.ToString(now_id) + "',";
            action = action + "'" + student_id + "',";
            action = action + "'" + ans + "',";
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
                return_result.Code = -16;//-16表示插入课程的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            return_result.Code = 0;
            return_result.Message = "";
            return return_result;
        }

        public static Function_result Exit_course_by_student(string student_id, string jwt, string course_id, OracleConnection connection)//学生退出课程
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
            //鉴权通过的话，就验证课程关系的存在性
            string check = "SELECT COUNT(*) FROM student_course\n";
            check = check + " WHERE course_id = '" + course_id + "'\n";
            check = check + " and student_id = '" + student_id + "'\n";
            int count = 0;
            try
            {
                using (OracleCommand command = new OracleCommand(check, connection))
                {
                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)//如果result非空
                    {
                        count = Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception)
            {
                return_result.Code = -13;//-13表示查询课程关系存在性的SQL操作错误
                return_result.Message = "SQL Error";
                return return_result;
            }
            if (count == 0)
            {
                return_result.Code = -25;//-25表示该学生-课程关系不存在
                return_result.Message = "No Record";
                return return_result;
            }
            //以下为数据库删除操作
            string action = "delete from student_course\n";
            action = action + " WHERE course_id = '" + course_id + "'\n";
            action = action + " and student_id = '" + student_id + "'\n";
            try
            {
                using (OracleCommand command = new OracleCommand(action, connection))
                {
                    command.ExecuteScalar();//执行插入操作
                }
            }
            catch (Exception)
            {
                return_result.Code = -14;//-14表示退出课程的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            return_result.Code = 0;
            return_result.Message = "";
            return return_result;
        }

        public static Function_result Exit_course_by_teacher(string teacher_id, string jwt, string course_id, string student_id, OracleConnection connection)//教师将指定学生踢出课程
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
            //鉴权通过的话，就验证课程关系的存在性
            string check = "SELECT COUNT(*) FROM student_course\n";
            check = check + " WHERE course_id = '" + course_id + "'\n";
            check = check + " and student_id = '" + student_id + "'\n";
            int count = 0;
            try
            {
                using (OracleCommand command = new OracleCommand(check, connection))
                {
                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)//如果result非空
                    {
                        count = Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception)
            {
                return_result.Code = -13;//-13表示查询课程关系存在性的SQL操作错误
                return_result.Message = "SQL Error";
                return return_result;
            }
            if (count == 0)
            {
                return_result.Code = -25;//-25表示该学生-课程关系不存在
                return_result.Message = "No Record";
                return return_result;
            }
            //如果课程存在，就要验证当前教师是否为课程的所有者
            check = "SELECT teacher_id FROM course\n";
            check = check + " WHERE id = '" + course_id + "'\n";
            string ans = "";
            try
            {
                using (OracleCommand command = new OracleCommand(check, connection))
                {
                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)//如果result非空
                    {
                        ans = Convert.ToString(result);
                    }
                }
            }
            catch (Exception)
            {
                return_result.Code = -14;//-14表示查询课程开课人的SQL操作错误
                return_result.Message = "SQL Error";
                return return_result;
            }
            if (ans != teacher_id)
            {
                return_result.Code = -26;//-26表示这门课不是这个老师开的，不准执行踢人操作
                return_result.Message = "The course not belongs to the teacher";
                return return_result;
            }
            //以下为数据库删除操作
            string action = "delete from student_course\n";
            action = action + " WHERE course_id = '" + course_id + "'\n";
            action = action + " and student_id = '" + student_id + "'\n";
            try
            {
                using (OracleCommand command = new OracleCommand(action, connection))
                {
                    command.ExecuteScalar();//执行插入操作
                }
            }
            catch (Exception)
            {
                return_result.Code = -15;//-15表示踢人的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            return_result.Code = 0;
            return_result.Message = "";
            return return_result;
        }

        public static Function_result Get_student_id_by_course(string teacher_id, string jwt, string course_id, OracleConnection connection)//教师查询属于自己的课程的学生id
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
            //鉴权通过的话，就先验证课程的所有者
            string check = "SELECT teacher_id FROM course\n";
            check = check + " WHERE id = '" + course_id + "'\n";
            string ans = "";
            try
            {
                using (OracleCommand command = new OracleCommand(check, connection))
                {
                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)//如果result非空
                    {
                        ans = Convert.ToString(result);
                    }
                }
            }
            catch (Exception)
            {
                return_result.Code = -13;//-14表示查询课程开课人的SQL操作错误
                return_result.Message = "SQL Error";
                return return_result;
            }
            if (ans != teacher_id)
            {
                return_result.Code = -25;//-25表示这门课不是这个老师开的，不准执行查询操作
                return_result.Message = "The course not belongs to the teacher";
                return return_result;
            }
            //再查询学生的数量
            check = "SELECT COUNT(*) FROM student_course WHERE course_id = '" + course_id + "'";
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
                return_result.Code = -14;//-14表示查询学生数量的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            if (count == 0)
            {
                return_result.Code = 0;//为0就直接返回值，不用后续查询，因为没有
                return_result.Message = "";
                return return_result;
            }
            //若课程存在，查询ID值
            check = "SELECT student_id FROM student_course WHERE course_id = '" + course_id + "'";
            ans = "";
            Console.WriteLine(check);
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

                            ans += reader["student_id"].ToString();
                        }
                    }
                }
            }
            catch (Exception)
            {
                return_result.Code = -15;//-15表示查询学生ID的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            return_result.Code = count;
            return_result.Message = ans;
            return return_result;
        }

        public static Function_result Get_course_id_by_student(string student_id, string jwt, OracleConnection connection)//学生查询自己目前已经加入的课程
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
            //鉴权通过的话，就先查询课程的数量
            string check = "SELECT COUNT(*) FROM student_course WHERE student_id = '" + student_id + "'";
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
                return_result.Code = -13;//-13表示查询课程数量的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            if (count == 0)
            {
                return_result.Code = 0;//为0就直接返回值，不用后续查询，因为没有
                return_result.Message = "";
                return return_result;
            }
            //若课程存在，查询课程ID
            check = "SELECT course_id FROM student_course WHERE student_id = '" + student_id + "'";
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

                            ans += reader["course_id"].ToString();
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
    }
}
