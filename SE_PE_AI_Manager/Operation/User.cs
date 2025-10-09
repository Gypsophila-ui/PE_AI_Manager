using Oracle.ManagedDataAccess.Client;
using SE_PE_AI_Manager.Basic;

namespace SE_PE_AI_Manager.Operation
{
    public class User
    {
        public static Function_result Teacher_login(string id, string password, OracleConnection connection)//登录，教工
        {
            Function_result return_result = new Function_result();
            return_result.Code = -99;
            return_result.Message = "Unknown Error";
            string check = "SELECT COUNT(*) FROM teacher WHERE id = '" + id + "'";
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
                return_result.Code = -11;//-11表示查询用户是否存在的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            if (count == 0)
            {
                return_result.Code = -21;//-21表示要登录的用户不存在错误
                return_result.Message = "User not found";
                return return_result;
            }
            check = "SELECT password FROM teacher WHERE id = '" + id + "'";
            string ans = "";
            try
            {
                using (OracleCommand command = new OracleCommand(check, connection))
                {
                    object result = command.ExecuteScalar();//查询密码值
                    if (result != null && result != DBNull.Value)//如果result非空
                    {
                        ans = Convert.ToString(result);
                    }
                    else
                    {
                        return_result.Code = -22;//-22表示查询成功但是无法获得密码
                        return_result.Message = "Password not found";
                        return return_result;
                    }
                }
            }
            catch (Exception)
            {
                return_result.Code = -12;//-12表示查询密码的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            //string SHA = Basic_calculate.ComputeSHA256(ans);//计算密码的SHA-256码
            //if (SHA != password)//与前端传来的SHA256码进行比较，调试时使用明码调试故不启用
            if (ans != password)
            {
                return_result.Code = -23;//-23表示密码错误
                return_result.Message = "Wrong passord";
                return return_result;
            }
            DateTime current_time = DateTime.Now;//获取当前时间
            string action = "UPDATE teacher\n";
            action = action + "SET login_time = SYSDATE\n";
            action = action + " WHERE id = '" + id + "'\n";
            try
            {
                using (var command = new OracleCommand(action, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
                return_result.Code = -13;//-13表示更新登录时间的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            ans = current_time.ToString("yyyyMMddHHmmss") + id + ans;//ans拼接为时间+id+密码的格式
            ans = Basic_calculate.ComputeSHA256(ans);//计算ans的SHA-256码
            ans = current_time.ToString("yyyyMMddHHmmss") + ans;
            //return ans;
            return_result.Code = 0;//-12表示查询密码的sql操作无法顺利执行
            return_result.Message = ans;
            return return_result;
        }

        public static Function_result Student_login(string id, string password, OracleConnection connection)//登录，学生
        {
            Function_result return_result = new Function_result();
            return_result.Code = -99;
            return_result.Message = "Unknown Error";
            string check = "SELECT COUNT(*) FROM student WHERE id = '" + id + "'";
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
                return_result.Code = -11;//-11表示查询用户是否存在的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            if (count == 0)
            {
                return_result.Code = -21;//-21表示要登录的用户不存在错误
                return_result.Message = "User not found";
                return return_result;
            }
            check = "SELECT password FROM student WHERE id = '" + id + "'";
            string ans = "";
            try
            {
                using (OracleCommand command = new OracleCommand(check, connection))
                {
                    object result = command.ExecuteScalar();//查询密码值
                    if (result != null && result != DBNull.Value)//如果result非空
                    {
                        ans = Convert.ToString(result);
                    }
                    else
                    {
                        return_result.Code = -22;//-22表示查询成功但是无法获得密码
                        return_result.Message = "Password not found";
                        return return_result;
                    }
                }
            }
            catch (Exception)
            {
                return_result.Code = -12;//-12表示查询密码的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            //string SHA = Basic_calculate.ComputeSHA256(ans);//计算密码的SHA-256码
            //if (SHA != password)//与前端传来的SHA256码进行比较，调试时使用明码调试故不启用
            if (ans != password)
            {
                return_result.Code = -23;//-23表示密码错误
                return_result.Message = "Wrong passord";
                return return_result;
            }
            DateTime current_time = DateTime.Now;//获取当前时间
            string action = "UPDATE student\n";
            action = action + "SET login_time = SYSDATE\n";
            action = action + " WHERE id = '" + id + "'\n";
            try
            {
                using (var command = new OracleCommand(action, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
                return_result.Code = -13;//-13表示更新登录时间的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            ans = current_time.ToString("yyyyMMddHHmmss") + id + ans;//ans拼接为时间+id+密码的格式
            ans = Basic_calculate.ComputeSHA256(ans);//计算ans的SHA-256码
            ans = current_time.ToString("yyyyMMddHHmmss") + ans;
            //return ans;
            return_result.Code = 0;//-12表示查询密码的sql操作无法顺利执行
            return_result.Message = ans;
            return return_result;
        }
    
        public static Function_result Check_jwt(int capacity, string id, string jwt, OracleConnection connection)//鉴定jwt语句是否有效
        {
            Function_result return_result = new Function_result();
            return_result.Code = -99;
            return_result.Message = "Unknown Error";
            //先检测jwt码是否正确
            //capacity为0表示为学生，为1表示为教师
            if (capacity != 0 && capacity != 1)//如果身份错误
            {
                return_result.Code = -2;
                return_result.Message = "Error Capacity";//返回身份错误
                return return_result;
            }
            if (capacity == 0)//如果是学生
            {
                return Check_student_jwt(id, jwt, connection);
            }
            else//否则就是老师
            {
                return Check_teacher_jwt(id, jwt, connection);
            }
        }

        public static Function_result Check_student_jwt(string id, string jwt, OracleConnection connection)//鉴定学生的jwt语句是否有效
        {
            Function_result return_result = new Function_result();
            return_result.Code = -99;
            return_result.Message = "Unknown Error";
            //先查询id是否存在
            string check = "SELECT COUNT(*) FROM student WHERE id = '" + id + "'";
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
                return_result.Code = -11;//-11表示查询用户是否存在的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            if (count == 0)
            {
                return_result.Code = -21;//-21表示要校验的用户不存在错误
                return_result.Message = "User not found";
                return return_result;
            }
            //再验证jwt编码是否正确
            //先获取密码
            check = "SELECT password FROM student WHERE id = '" + id + "'";
            string ans = "";
            try
            {
                using (OracleCommand command = new OracleCommand(check, connection))
                {
                    object result = command.ExecuteScalar();//查询密码值
                    if (result != null && result != DBNull.Value)//如果result非空
                    {
                        ans = Convert.ToString(result);
                    }
                    else
                    {
                        return_result.Code = -22;//-22表示查询成功但是无法获得密码
                        return_result.Message = "Password not found";
                        return return_result;
                    }
                }
            }
            catch (Exception)
            {
                return_result.Code = -12;//-12表示查询密码的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            //再计算jwt码
            string jwt_time = jwt.Substring(0, 14);
            ans = jwt_time + id + ans;
            ans = Basic_calculate.ComputeSHA256(ans);
            ans = jwt_time + ans;
            if (ans != jwt)//如果我计算出来的jwt码和前端传来的不一样
            {
                return_result.Code = -23;//-23表示jwt码无效
                return_result.Message = "JWT Error";
                return return_result;
            }
            //最后检查jwt是否超时
            bool flag=Basic_calculate.Check_login_time(jwt_time);
            if(flag==true)//如果超时
            {
                return_result.Code = -24;//-24表示jwt码已经超时
                return_result.Message = "JWT TLE";
                return return_result;
            }
            else
            {
                return_result.Code = 0;
                return_result.Message = "";
                return return_result;
            }
        }

        public static Function_result Check_teacher_jwt(string id, string jwt, OracleConnection connection)//鉴定教师的jwt语句是否有效
        {
            Function_result return_result = new Function_result();
            return_result.Code = -99;
            return_result.Message = "Unknown Error";
            //先查询id是否存在
            string check = "SELECT COUNT(*) FROM teacher WHERE id = '" + id + "'";
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
                return_result.Code = -11;//-11表示查询用户是否存在的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            if (count == 0)
            {
                return_result.Code = -21;//-21表示要登录的用户不存在错误
                return_result.Message = "User not found";
                return return_result;
            }
            //再验证jwt编码是否正确
            //先获取密码
            check = "SELECT password FROM teacher WHERE id = '" + id + "'";
            string ans = "";
            try
            {
                using (OracleCommand command = new OracleCommand(check, connection))
                {
                    object result = command.ExecuteScalar();//查询密码值
                    if (result != null && result != DBNull.Value)//如果result非空
                    {
                        ans = Convert.ToString(result);
                    }
                    else
                    {
                        return_result.Code = -22;//-22表示查询成功但是无法获得密码
                        return_result.Message = "Password not found";
                        return return_result;
                    }
                }
            }
            catch (Exception)
            {
                return_result.Code = -12;//-12表示查询密码的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            //再计算jwt码
            string jwt_time = jwt.Substring(0, 14);
            ans = jwt_time + id + ans;
            ans = Basic_calculate.ComputeSHA256(ans);
            ans = jwt_time + ans;
            if (ans != jwt)//如果我计算出来的jwt码和前端传来的不一样
            {
                return_result.Code = -23;//-23表示jwt码无效
                return_result.Message = "JWT Error";
                return return_result;
            }
            //最后检查jwt是否超时
            bool flag = Basic_calculate.Check_login_time(jwt_time);
            if (flag == true)//如果超时
            {
                return_result.Code = -24;//-24表示jwt码已经超时
                return_result.Message = "JWT TLE";
                return return_result;
            }
            else
            {
                return_result.Code = 0;
                return_result.Message = "";
                return return_result;
            }
        }

        public static Function_result New_teacher(string id, string password, string name, string gender, string title, string college, string department, OracleConnection connection)//注册教工
        {
            Function_result return_result = new Function_result();
            return_result.Code = -99;
            return_result.Message = "Unknown Error";
            //先检查用户有没有重复
            string check = "SELECT COUNT(*) FROM teacher WHERE id = '" + id + "'";
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
                return_result.Code = -11;//-11表示查询用户是否存在的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            if (count != 0)
            {
                return_result.Code = -21;//-21表示要注册的用户存在，发生冲突
                return_result.Message = "User Collide";
                return return_result;
            }
            //再检查要注册的教师信息是否在基准教师表中
            check = "SELECT COUNT(*) FROM std_teacher WHERE id = '" + id + "'";
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
                return_result.Code = -12;//-12表示查询用户是否存在的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            if (count == 0)
            {
                return_result.Code = -22;//-22表示要注册的用户不在基准数据库中，不准注册
                return_result.Message = "User not found";
                return return_result;
            }
            string action = "INSERT INTO teacher(id,password,name,gender,title,college,department,CREATED_TIME)\n";
            action = action + "values('" + id + "',";
            action = action + "'" + password + "',";
            action = action + "'" + name + "',";
            action = action + "'" + gender + "',";
            action = action + "'" + title + "',";
            action = action + "'" + college + "',";
            action = action + "'" + department + "',";
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
                return_result.Code = -13;//-13表示插入用户的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            return_result.Code = 0;
            return_result.Message = "";
            return return_result;
        }

        public static Function_result New_student(string id, string password, string name, string gender, string major, string college, string department, OracleConnection connection)//注册学生
        {
            Function_result return_result = new Function_result();
            return_result.Code = -99;
            return_result.Message = "Unknown Error";
            //先检查用户有没有重复
            string check = "SELECT COUNT(*) FROM student WHERE id = '" + id + "'";
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
                return_result.Code = -11;//-11表示查询用户是否存在的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            if (count != 0)
            {
                return_result.Code = -21;//-21表示要注册的用户存在，发生冲突
                return_result.Message = "User Collide";
                return return_result;
            }
            //再检查要注册的学生信息是否在基准学生表中
            check = "SELECT COUNT(*) FROM std_student WHERE id = '" + id + "'";
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
                return_result.Code = -12;//-12表示查询用户是否存在的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            if (count == 0)
            {
                return_result.Code = -22;//-22表示要注册的用户不在基准数据库中，不准注册
                return_result.Message = "User not found";
                return return_result;
            }
            string action = "INSERT INTO student(id,password,name,gender,major,college,department,CREATED_TIME)\n";
            action = action + "values('" + id + "',";
            action = action + "'" + password + "',";
            action = action + "'" + name + "',";
            action = action + "'" + gender + "',";
            action = action + "'" + major + "',";
            action = action + "'" + college + "',";
            action = action + "'" + department + "',";
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
                return_result.Code = -13;//-13表示插入用户的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            return_result.Code = 0;
            return_result.Message = "";
            return return_result;
        }

        public static Function_result Change_teacher_info(string id, string jwt, string name, string gender, string title, string college, string department, OracleConnection connection)//注册教工
        {
            Function_result return_result = new Function_result();
            return_result.Code = -99;
            return_result.Message = "Unknown Error";
            //先鉴权
            return_result = Check_jwt(1, id, jwt, connection);//执行鉴权操作
            if( return_result.Code < 0 )//如果鉴权不通过
            {
                return return_result;//驳回操作
            }
            //鉴权通过的话，就执行修改操作
            string action = "update teacher\n";
            action = action + "set name = '" + name + "',\n";
            action = action + "gender = '" + gender + "',\n";
            action = action + "title = '" + title + "',\n";
            action = action + "college = '" + college + "',\n";
            action = action + "department = '" + department + "'\n";
            action = action + "where id = '" + id + "'\n";
            try
            {
                using (OracleCommand command = new OracleCommand(action, connection))
                {
                    command.ExecuteScalar();//执行修改操作
                }
            }
            catch (Exception)
            {
                return_result.Code = -13;//-13表示修改用户信息的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            return_result.Code = 0;
            return_result.Message = "";
            return return_result;
        }

        public static Function_result Change_student_info(string id, string jwt, string name, string gender, string major, string college, string department, OracleConnection connection)//注册教工
        {
            Function_result return_result = new Function_result();
            return_result.Code = -99;
            return_result.Message = "Unknown Error";
            //先鉴权
            return_result = Check_jwt(0, id, jwt, connection);//执行鉴权操作
            if (return_result.Code < 0)//如果鉴权不通过
            {
                return return_result;//驳回操作
            }
            //鉴权通过的话，就执行修改操作
            string action = "update student\n";
            action = action + "set name = '" + name + "',\n";
            action = action + "gender = '" + gender + "',\n";
            action = action + "major = '" + major + "',\n";
            action = action + "college = '" + college + "',\n";
            action = action + "department = '" + department + "'\n";
            action = action + "where id = '" + id + "'\n";
            try
            {
                using (OracleCommand command = new OracleCommand(action, connection))
                {
                    command.ExecuteScalar();//执行修改操作
                }
            }
            catch (Exception)
            {
                return_result.Code = -13;//-13表示修改用户信息的sql操作无法顺利执行
                return_result.Message = "SQL Error";
                return return_result;
            }
            return_result.Code = 0;
            return_result.Message = "";
            return return_result;
        }
    }
}
