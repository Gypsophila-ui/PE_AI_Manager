using System.Security.Cryptography;
using System.Text;

namespace SE_PE_AI_Manager.Basic
{
    public class Basic_calculate // 基础计算
    {
        public static int Check_null(string data)//检测字符串是否为NULL
        {
            //Console.WriteLine(data);
            if (data == "NULL"||data=="null")
            {
                return 1;
            }
            return 0;
        }

        public static string ComputeSHA256(string input)//计算sha-256编码
        {
            // 验证输入
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentException("Input cannot be null or empty");
            }
            // 使用SHA256.Create()创建实例（推荐方式）
            using (SHA256 sha256 = SHA256.Create())
            {
                // 将字符串转换为字节数组（UTF-8编码）
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);

                // 计算哈希值
                byte[] hashBytes = sha256.ComputeHash(inputBytes);

                // 将字节数组转换为十六进制字符串
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        public static bool Check_login_time(string jwt_time)//检查登陆时间是否超过15min
        {
            DateTime jwtDateTime = DateTime.ParseExact(jwt_time, "yyyyMMddHHmmss", null);//先转换回date
            DateTime current_time = DateTime.Now;//获取当前时间
            TimeSpan difference = current_time - jwtDateTime;//计算时间差
            TimeSpan absoluteDifference = difference.Duration();//取绝对值
            return absoluteDifference.TotalMinutes > 15;//判断是否达到15min
        }
    }
}
