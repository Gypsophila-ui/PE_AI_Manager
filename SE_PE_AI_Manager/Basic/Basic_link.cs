using Oracle.ManagedDataAccess.Client;

namespace SE_PE_AI_Manager.Basic
{
    public class Basic_link
    {
        private static OracleConnection connection;
        public static int Start_link()//启动数据库连接
        {
            if (connection != null && connection.State == System.Data.ConnectionState.Open)
            {
                int val = Close_link();//表示当前链接未关闭，先关掉再打开
                if (val != 0)
                {
                    return val;
                }
            }
            string connectionString = "User Id=SE;" +
                                      "Password=2025SE;" +
                                      "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=47.121.177.100)(PORT=1521))" +
                                      "(CONNECT_DATA=(SERVICE_NAME=XEPDB1)))";
            //启动数据库链接
            connection = new OracleConnection(connectionString);
            try
            {
                connection.Open();
                return 0;
            }
            catch (Exception)
            {
                return -31;//链接打开失败
            }
        }
        public static int Close_link()//关闭数据库链接
        {
            if (connection == null || connection.State != System.Data.ConnectionState.Open)
            {
                return -32;//链接根本没打开的错误
            }
            try
            {
                connection.Close();
                return 0;
            }
            catch (Exception)
            {
                return -31;//链接关闭失败
            }
        }
        public static OracleConnection Get_connection()//获取数据库链接
        {
            if(connection.State == System.Data.ConnectionState.Open)//如果链接已经打开
            {
                return connection;//返回链接
            }
            return null;//否则返回空值
        }
    }
}
