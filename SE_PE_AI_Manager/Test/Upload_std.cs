using CsvHelper;
using Oracle.ManagedDataAccess.Client;
using SE_PE_AI_Manager.Basic;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace SE_PE_AI_Manager.Test
{
    public class Upload_std
    {
        public Function_result std_student(OracleConnection connection)
        {
            Function_result return_result = new Function_result();
            string csvFile = "test_student.csv"; // 用CSV文件代替Excel

            // 1. 检查文件是否存在
            if (!File.Exists(csvFile))
            {
                throw new FileNotFoundException($"找不到文件: {csvFile}");
            }

            // 2. 读取CSV文件
            var students = new List<CSV_Info>();
            using (var reader = new StreamReader(csvFile))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                students = csv.GetRecords<CSV_Info>().ToList();
            }

            // 3. 上传到数据库
            foreach (var student in students)
            {
                string action = $"INSERT INTO std_student(id, name) VALUES ('{student.ID}', '{student.name}')";
                try
                {
                    using (OracleCommand command = new OracleCommand(action, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    return_result.Code = -13;
                    return_result.Message = $"SQL Error: {ex.Message}";
                    return return_result;
                }
            }
            return return_result;
        }

        public Function_result std_teacher(OracleConnection connection)
        {
            Function_result return_result = new Function_result();
            string csvFile = "std_teacher.csv"; // 用CSV文件代替Excel

            // 1. 检查文件是否存在
            if (!File.Exists(csvFile))
            {
                throw new FileNotFoundException($"找不到文件: {csvFile}");
            }

            // 2. 读取CSV文件
            var teacher = new List<CSV_Info>();
            using (var reader = new StreamReader(csvFile))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                teacher = csv.GetRecords<CSV_Info>().ToList();
            }

            // 3. 上传到数据库
            foreach (var teach in teacher)
            {
                string action = $"INSERT INTO std_teacher(id, name) VALUES ('{teach.ID}', '{teach.name}')";
                try
                {
                    using (OracleCommand command = new OracleCommand(action, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    return_result.Code = -13;
                    return_result.Message = $"SQL Error: {ex.Message}";
                    return return_result;
                }
            }
            return return_result;
        }

        public Function_result std_course(OracleConnection connection)
        {
            Function_result return_result = new Function_result();

            for (int i =1; i <= 50;i++)
            {
                Random random = new Random();
                int val = 300000 + random.Next(0, 10000);
                string action = $"INSERT INTO std_course(id) VALUES ('{val}')";
                try
                {
                    using (OracleCommand command = new OracleCommand(action, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    return_result.Code = -13;
                    return_result.Message = $"SQL Error: {ex.Message}";
                    return return_result;
                }
            }
            return return_result;
        }
    }

    public class Upload_data
    {
        public Function_result student(OracleConnection connection)
        {
            Function_result return_result = new Function_result();
            string csvFile = "student.csv"; // 用CSV文件代替Excel

            // 1. 检查文件是否存在
            if (!File.Exists(csvFile))
            {
                throw new FileNotFoundException($"找不到文件: {csvFile}");
            }

            // 2. 读取CSV文件
            var students = new List<CSV_Info>();
            using (var reader = new StreamReader(csvFile))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                students = csv.GetRecords<CSV_Info>().ToList();
            }

            // 3. 上传到数据库
            foreach (var student in students)
            {
                string action = $"INSERT INTO student(id, name, password, gender, college) VALUES ('{student.ID}', '{student.name}', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', '软件工程', '计算机科学与技术学院')";
                //Console.WriteLine(action);
                try
                {
                    using (OracleCommand command = new OracleCommand(action, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    return_result.Code = -13;
                    return_result.Message = $"SQL Error: {ex.Message}";
                    return return_result;
                }
            }
            return return_result;
        }

        public Function_result student_course(OracleConnection connection)
        {
            Function_result return_result = new Function_result();
            string csvFile = "student.csv"; // 用CSV文件代替Excel

            // 1. 检查文件是否存在
            if (!File.Exists(csvFile))
            {
                throw new FileNotFoundException($"找不到文件: {csvFile}");
            }

            // 2. 读取CSV文件
            var students = new List<CSV_Info>();
            using (var reader = new StreamReader(csvFile))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                students = csv.GetRecords<CSV_Info>().ToList();
            }
            int id = 0;
            // 3. 上传到数据库
            foreach (var student in students)
            {
                id++;
                if(id>50)
                {
                    break;
                }
                string action = $"INSERT INTO student_course(id,student_id, course_id) VALUES ('{Convert.ToString(id)}', '{student.ID}', '307337')";
                //Console.WriteLine(action);
                try
                {
                    using (OracleCommand command = new OracleCommand(action, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    return_result.Code = -13;
                    return_result.Message = $"SQL Error: {ex.Message}";
                    return return_result;
                }
            }
            return return_result;
        }

        public Function_result submit(OracleConnection connection)
        {
            Function_result return_result = new Function_result();
            string csvFile = "student.csv"; // 用CSV文件代替Excel

            // 1. 检查文件是否存在
            if (!File.Exists(csvFile))
            {
                throw new FileNotFoundException($"找不到文件: {csvFile}");
            }

            // 2. 读取CSV文件
            var students = new List<CSV_Info>();
            using (var reader = new StreamReader(csvFile))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                students = csv.GetRecords<CSV_Info>().ToList();
            }
            int id = 400;
            // 3. 上传到数据库
            foreach (var student in students)
            {
                id++;
                if(id>450)
                {
                    break;
                }
                if (id % 4 != 3) 
                {
                    continue;
                }
                string action = $"INSERT INTO submit(id,student_id, homework_id,CREATE_TIME,score,video_url) VALUES ('{Convert.ToString(id)}', '{student.ID}', '9', SYSDATE, '{Convert.ToString((id*id+id)%100)}','test.ztz11')";
                //Console.WriteLine(action);
                try
                {
                    using (OracleCommand command = new OracleCommand(action, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    return_result.Code = -13;
                    return_result.Message = $"SQL Error: {ex.Message}";
                    return return_result;
                }
            }
            return return_result;
        }
    }

    // CSV对应的模型
    public class CSV_Info
    {
        public string ID { get; set; }
        public string name { get; set; }
    }
}