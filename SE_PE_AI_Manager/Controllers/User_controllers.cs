using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using SE_PE_AI_Manager.Operation;
using SE_PE_AI_Manager.Basic;


namespace SE_PE_AI_Manager.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        [HttpPost("login_teacher")]
        public ActionResult<API_result> Login_teacher([FromBody] Request_two request)
        {
            string name = request.First;//获取姓名
            string password = request.Second;//获取密码
            var API_result = new API_result//初始化的返回值
            {
                Success = true,
                Message = "NULL",
                Data = "NULL",
                ErrorCode = 0
            };
            if (name == null || password == null)//如果参数错误
            {
                API_result.Success = false;
                API_result.Message = "Error parameters";
                API_result.Data = "NULL";
                API_result.ErrorCode = -1;
                return BadRequest(API_result);
            }
            int link_val = Basic_link.Start_link();
            if (link_val == -31)//如果无法打开数据库链接
            {
                API_result.Success = false;
                API_result.Message = "Connection can not open";
                API_result.Data = "NULL";
                API_result.ErrorCode = link_val;
                return NotFound(API_result);
            }
            OracleConnection connection = Basic_link.Get_connection();
            if (connection == null)//如果未能正确获取链接
            {
                API_result.Success = false;
                API_result.Message = "Connection Error";
                API_result.Data = "NULL";
                API_result.ErrorCode = -32;
                Basic_link.Close_link();
                return NotFound(API_result);
            }
            Function_result val = SE_PE_AI_Manager.Operation.User.Teacher_login(name, password, connection);
            if (val.Code < 0)//如果返回结果存在错误
            {
                API_result.Success = false;
                API_result.Message = val.Message;
                API_result.Data = "NULL";
                API_result.ErrorCode = val.Code;
                return BadRequest(API_result);
            }
            else//如果正常返回
            {
                API_result.Success = true;
                API_result.Data = val.Message;
                API_result.Message = "NULL";
                API_result.ErrorCode = val.Code;
                return Ok(API_result);
            }
        }

        [HttpPost("login_student")]
        public ActionResult<API_result> Login_student([FromBody] Request_two request)
        {
            string name = request.First;//获取姓名
            string password = request.Second;//获取密码
            var API_result = new API_result//初始化的返回值
            {
                Success = true,
                Message = "NULL",
                Data = "NULL",
                ErrorCode = 0
            };
            if (name == null || password == null)//如果参数错误
            {
                API_result.Success = false;
                API_result.Message = "Error parameters";
                API_result.Data = "NULL";
                API_result.ErrorCode = -1;
                return BadRequest(API_result);
            }
            int link_val = Basic_link.Start_link();
            if (link_val == -31)//如果无法打开数据库链接
            {
                API_result.Success = false;
                API_result.Message = "Connection can not open";
                API_result.Data = "NULL";
                API_result.ErrorCode = link_val;
                return NotFound(API_result);
            }
            OracleConnection connection = Basic_link.Get_connection();
            if (connection == null)//如果未能正确获取链接
            {
                API_result.Success = false;
                API_result.Message = "Connection Error";
                API_result.Data = "NULL";
                API_result.ErrorCode = -32;
                Basic_link.Close_link();
                return NotFound(API_result);
            }
            Function_result val = SE_PE_AI_Manager.Operation.User.Student_login(name, password, connection);
            if (val.Code < 0)//如果返回结果存在错误
            {
                API_result.Success = false;
                API_result.Message = val.Message;
                API_result.Data = "NULL";
                API_result.ErrorCode = val.Code;
                return BadRequest(API_result);
            }
            else//如果正常返回
            {
                API_result.Success = true;
                API_result.Data = val.Message;
                API_result.Message = "NULL";
                API_result.ErrorCode = val.Code;
                return Ok(API_result);
            }
        }

        [HttpPost("new_teacher")]
        public ActionResult<API_result> New_teacher([FromBody] Request_seven request)
        {
            string id = request.First;//获取参数
            string password = request.Second;
            string name = request.Third;
            string gender = request.Fourth;
            string title = request.Fifth;
            string college = request.Sixth;
            string department = request.Seventh;
            var API_result = new API_result//初始化的返回值
            {
                Success = true,
                Message = "NULL",
                Data = "NULL",
                ErrorCode = 0
            };
            if (id == null || password == null || name == null || gender == null || college == null)//如果参数错误
            {
                API_result.Success = false;
                API_result.Message = "Error parameters";
                API_result.Data = "NULL";
                API_result.ErrorCode = -1;
                return BadRequest(API_result);
            }
            int link_val = Basic_link.Start_link();
            if (link_val == -31)//如果无法打开数据库链接
            {
                API_result.Success = false;
                API_result.Message = "Connection can not open";
                API_result.Data = "NULL";
                API_result.ErrorCode = link_val;
                return NotFound(API_result);
            }
            OracleConnection connection = Basic_link.Get_connection();

            if (connection == null)//如果未能正确获取链接
            {
                API_result.Success = false;
                API_result.Message = "Connection Error";
                API_result.Data = "NULL";
                API_result.ErrorCode = -32;
                Basic_link.Close_link();
                return NotFound(API_result);
            }
            Function_result val = SE_PE_AI_Manager.Operation.User.New_teacher(id, password, name, gender, title, college, department, connection);
            if (val.Code < 0)//如果返回结果存在错误
            {
                API_result.Success = false;
                API_result.Message = val.Message;
                API_result.Data = "NULL";
                API_result.ErrorCode = val.Code;
                return BadRequest(API_result);
            }
            else//如果正常返回
            {
                API_result.Success = true;
                API_result.Data = val.Message;
                API_result.Message = "NULL";
                API_result.ErrorCode = val.Code;
                return Ok(API_result);
            }
        }

        [HttpPost("new_student")]
        public ActionResult<API_result> New_student([FromBody] Request_seven request)
        {
            string id = request.First;//获取参数
            string password = request.Second;
            string name = request.Third;
            string gender = request.Fourth;
            string major = request.Fifth;
            string college = request.Sixth;
            string department = request.Seventh;
            var API_result = new API_result//初始化的返回值
            {
                Success = true,
                Message = "NULL",
                Data = "NULL",
                ErrorCode = 0
            };
            if (id == null || password == null || name == null || gender == null || college == null)//如果参数错误
            {
                API_result.Success = false;
                API_result.Message = "Error parameters";
                API_result.Data = "NULL";
                API_result.ErrorCode = -1;
                return BadRequest(API_result);
            }
            int link_val = Basic_link.Start_link();
            if (link_val == -31)//如果无法打开数据库链接
            {
                API_result.Success = false;
                API_result.Message = "Connection can not open";
                API_result.Data = "NULL";
                API_result.ErrorCode = link_val;
                return NotFound(API_result);
            }
            OracleConnection connection = Basic_link.Get_connection();

            if (connection == null)//如果未能正确获取链接
            {
                API_result.Success = false;
                API_result.Message = "Connection Error";
                API_result.Data = "NULL";
                API_result.ErrorCode = -32;
                Basic_link.Close_link();
                return NotFound(API_result);
            }
            Function_result val = SE_PE_AI_Manager.Operation.User.New_student(id, password, name, gender, major, college, department, connection);
            if (val.Code < 0)//如果返回结果存在错误
            {
                API_result.Success = false;
                API_result.Message = val.Message;
                API_result.Data = "NULL";
                API_result.ErrorCode = val.Code;
                return BadRequest(API_result);
            }
            else//如果正常返回
            {
                API_result.Success = true;
                API_result.Data = val.Message;
                API_result.Message = "NULL";
                API_result.ErrorCode = val.Code;
                return Ok(API_result);
            }
        }

        [HttpPost("change_purview")]
        public ActionResult<API_result> Change_purview([FromBody] Request_one request)
        {
            string parameters = request.First;//获取参数
            var API_result = new API_result//初始化的返回值
            {
                Success = true,
                Message = "NULL",
                Data = "NULL",
                ErrorCode = 0
            };
            if (parameters == null)//如果参数错误
            {
                API_result.Success = false;
                API_result.Message = "Error parameters";
                API_result.Data = "NULL";
                API_result.ErrorCode = -1;
                return BadRequest(API_result);
            }
            int link_val = Basic_link.Start_link();
            if (link_val == -31)//如果无法打开数据库链接
            {
                API_result.Success = false;
                API_result.Message = "Connection can not open";
                API_result.Data = "NULL";
                API_result.ErrorCode = link_val;
                return NotFound(API_result);
            }
            OracleConnection connection = Basic_link.Get_connection();

            if (connection == null)//如果未能正确获取链接
            {
                API_result.Success = false;
                API_result.Message = "Connection Error";
                API_result.Data = "NULL";
                API_result.ErrorCode = -32;
                Basic_link.Close_link();
                return NotFound(API_result);
            }
            int val = 0;// user.Change_purview(parameters, connection);
            if (val >= 0)
            {
                API_result.Success = true;
                API_result.Message = "NULL";
                API_result.Data = Convert.ToString(val);//返回影响的行数
                API_result.ErrorCode = 0;
                Basic_link.Close_link();
                return Ok(API_result);
            }
            if (val < 0 && val > -10) //参数出错
            {
                API_result.Success = false;
                API_result.Message = "parameters";
                API_result.Data = "NULL";
                API_result.ErrorCode = val;
                Basic_link.Close_link();
                return BadRequest(API_result);
            }
            if (val >= -19 && val <= -10)//sql操作出错
            {
                API_result.Success = false;
                API_result.Message = "Error SQL action";
                API_result.Data = "NULL";
                API_result.ErrorCode = val;
                Basic_link.Close_link();
                return BadRequest(API_result);
            }
            if (val == -21)//用户名不存在
            {
                API_result.Success = false;
                API_result.Message = "Wrong Name";
                API_result.Data = "NULL";
                API_result.ErrorCode = val;
                Basic_link.Close_link();
                return NotFound(API_result);
            }
            //对于其他的意料之外的问题
            API_result.Success = false;
            API_result.Message = "Unknown Error";
            API_result.Data = "NULL";
            Basic_link.Close_link();
            API_result.ErrorCode = val;
            return BadRequest(API_result);
        }

        [HttpPost("update_user_info")]
        public ActionResult<API_result> Update_user_info([FromBody] Request_two request)
        {
            string name = request.First;//获取参数
            string parameters = request.Second;
            var API_result = new API_result//初始化的返回值
            {
                Success = true,
                Message = "NULL",
                Data = "NULL",
                ErrorCode = 0
            };
            if (parameters == null || name == null)//如果参数错误
            {
                API_result.Success = false;
                API_result.Message = "Error parameters";
                API_result.Data = "NULL";
                API_result.ErrorCode = -1;
                return BadRequest(API_result);
            }
            int link_val = Basic_link.Start_link();
            if (link_val == -31)//如果无法打开数据库链接
            {
                API_result.Success = false;
                API_result.Message = "Connection can not open";
                API_result.Data = "NULL";
                API_result.ErrorCode = link_val;
                return NotFound(API_result);
            }
            OracleConnection connection = Basic_link.Get_connection();

            if (connection == null)//如果未能正确获取链接
            {
                API_result.Success = false;
                API_result.Message = "Connection Error";
                API_result.Data = "NULL";
                API_result.ErrorCode = -32;
                Basic_link.Close_link();
                return NotFound(API_result);
            }
            int val = 0;// user.Update_user_info(name, parameters, connection);
            if (val >= 0)
            {
                API_result.Success = true;
                API_result.Message = "NULL";
                API_result.Data = Convert.ToString(val);//返回影响的行数
                API_result.ErrorCode = 0;
                Basic_link.Close_link();
                return Ok(API_result);
            }
            if (val < 0 && val > -10) //参数出错
            {
                API_result.Success = false;
                API_result.Message = "parameters";
                API_result.Data = "NULL";
                API_result.ErrorCode = val;
                Basic_link.Close_link();
                return BadRequest(API_result);
            }
            if (val >= -19 && val <= -10)//sql操作出错
            {
                API_result.Success = false;
                API_result.Message = "Error SQL action";
                API_result.Data = "NULL";
                API_result.ErrorCode = val;
                Basic_link.Close_link();
                return BadRequest(API_result);
            }
            if (val == -21)//用户名不存在
            {
                API_result.Success = false;
                API_result.Message = "Wrong Name";
                API_result.Data = "NULL";
                API_result.ErrorCode = val;
                Basic_link.Close_link();
                return NotFound(API_result);
            }
            //对于其他的意料之外的问题
            API_result.Success = false;
            API_result.Message = "Unknown Error";
            API_result.Data = "NULL";
            Basic_link.Close_link();
            API_result.ErrorCode = val;
            return BadRequest(API_result);
        }

        [HttpPost("delete_user")]
        public ActionResult<API_result> Delete_user([FromBody] Request_one request)
        {
            string parameters = request.First;//获取参数
            var API_result = new API_result//初始化的返回值
            {
                Success = true,
                Message = "NULL",
                Data = "NULL",
                ErrorCode = 0
            };
            if (parameters == null)//如果参数错误
            {
                API_result.Success = false;
                API_result.Message = "Error parameters";
                API_result.Data = "NULL";
                API_result.ErrorCode = -1;
                return BadRequest(API_result);
            }
            int link_val = Basic_link.Start_link();
            if (link_val == -31)//如果无法打开数据库链接
            {
                API_result.Success = false;
                API_result.Message = "Connection can not open";
                API_result.Data = "NULL";
                API_result.ErrorCode = link_val;
                return NotFound(API_result);
            }
            OracleConnection connection = Basic_link.Get_connection();

            if (connection == null)//如果未能正确获取链接
            {
                API_result.Success = false;
                API_result.Message = "Connection Error";
                API_result.Data = "NULL";
                API_result.ErrorCode = -32;
                Basic_link.Close_link();
                return NotFound(API_result);
            }
            int val = 0;//user.Delete_user(parameters, connection);
            if (val >= 0)
            {
                API_result.Success = true;
                API_result.Message = "NULL";
                API_result.Data = Convert.ToString(val);//返回影响的行数
                API_result.ErrorCode = 0;
                Basic_link.Close_link();
                return Ok(API_result);
            }
            if (val < 0 && val > -10) //参数出错
            {
                API_result.Success = false;
                API_result.Message = "parameters";
                API_result.Data = "NULL";
                API_result.ErrorCode = val;
                Basic_link.Close_link();
                return BadRequest(API_result);
            }
            if (val >= -19 && val <= -10)//sql操作出错
            {
                API_result.Success = false;
                API_result.Message = "Error SQL action";
                API_result.Data = "NULL";
                API_result.ErrorCode = val;
                Basic_link.Close_link();
                return BadRequest(API_result);
            }
            if (val == -21)//用户名对应的权限值越界，包括用户不存在
            {
                API_result.Success = false;
                API_result.Message = "Wrong purview";
                API_result.Data = "NULL";
                API_result.ErrorCode = val;
                Basic_link.Close_link();
                return BadRequest(API_result);
            }
            if (val == -22)//尝试删除了超级管理员
            {
                API_result.Success = false;
                API_result.Message = "Low Purview";
                API_result.Data = "NULL";
                API_result.ErrorCode = val;
                Basic_link.Close_link();
                return BadRequest(API_result);
            }

            //对于其他的意料之外的问题
            API_result.Success = false;
            API_result.Message = "Unknown Error";
            API_result.Data = "NULL";
            Basic_link.Close_link();
            API_result.ErrorCode = val;
            return BadRequest(API_result);
        }

        [HttpPost("get_user_info")]
        public ActionResult<API_result> Get_user_info([FromBody] Request_one request)
        {
            string parameters = request.First;//获取参数
            var API_result = new API_result//初始化的返回值
            {
                Success = true,
                Message = "NULL",
                Data = "NULL",
                ErrorCode = 0
            };
            if (parameters == null)//如果参数错误
            {
                API_result.Success = false;
                API_result.Message = "Error parameters";
                API_result.Data = "NULL";
                API_result.ErrorCode = -1;
                return BadRequest(API_result);
            }
            int link_val = Basic_link.Start_link();
            if (link_val == -31)//如果无法打开数据库链接
            {
                API_result.Success = false;
                API_result.Message = "Connection can not open";
                API_result.Data = "NULL";
                API_result.ErrorCode = link_val;
                return NotFound(API_result);
            }
            OracleConnection connection = Basic_link.Get_connection();

            if (connection == null)//如果未能正确获取链接
            {
                API_result.Success = false;
                API_result.Message = "Connection Error";
                API_result.Data = "NULL";
                API_result.ErrorCode = -32;
                Basic_link.Close_link();
                return NotFound(API_result);
            }
            string val = "0";// user.Get_user_info(parameters, connection);
            if (val == "-1")//如果参数错误
            {
                API_result.Success = false;
                API_result.Message = "Error parameters";
                API_result.Data = "NULL";
                API_result.ErrorCode = -1;
                return BadRequest(API_result);
            }
            if (val.Length >= 5)
            {
                API_result.Success = true;
                API_result.Message = "NULL";
                API_result.Data = val;
                API_result.ErrorCode = 0;
                Basic_link.Close_link();
                return Ok(API_result);
            }
            if (val == "-11")//sql操作出错
            {
                API_result.Success = false;
                API_result.Message = "Error SQL action";
                API_result.Data = "NULL";
                API_result.ErrorCode = -11;
                Basic_link.Close_link();
                return BadRequest(API_result);
            }
            if (val == "-21")//查询结果为空
            {
                API_result.Success = false;
                API_result.Message = "Error Name";
                API_result.Data = "NULL";
                API_result.ErrorCode = -21;
                Basic_link.Close_link();
                return NotFound(API_result);
            }

            //对于其他的意料之外的问题
            API_result.Success = false;
            API_result.Message = "Unknown Error";
            API_result.Data = "NULL";
            Basic_link.Close_link();
            API_result.ErrorCode = -99;
            return BadRequest(API_result);
        }

        [HttpPost("change_self_password")]
        public ActionResult<API_result> Change_self_password([FromBody] Request_two request)
        {
            string name = request.First;
            string parameters = request.Second;//获取参数
            var API_result = new API_result//初始化的返回值
            {
                Success = true,
                Message = "NULL",
                Data = "NULL",
                ErrorCode = 0
            };
            if (parameters == null || name == null)//如果参数错误
            {
                API_result.Success = false;
                API_result.Message = "Error parameters";
                API_result.Data = "NULL";
                API_result.ErrorCode = -1;
                return BadRequest(API_result);
            }
            int link_val = Basic_link.Start_link();
            if (link_val == -31)//如果无法打开数据库链接
            {
                API_result.Success = false;
                API_result.Message = "Connection can not open";
                API_result.Data = "NULL";
                API_result.ErrorCode = link_val;
                return NotFound(API_result);
            }
            OracleConnection connection = Basic_link.Get_connection();

            if (connection == null)//如果未能正确获取链接
            {
                API_result.Success = false;
                API_result.Message = "Connection Error";
                API_result.Data = "NULL";
                API_result.ErrorCode = -32;
                Basic_link.Close_link();
                return NotFound(API_result);
            }
            int val = 0;// user.Change_self_password(name, parameters, connection);
            if (val == -1)//如果参数错误
            {
                API_result.Success = false;
                API_result.Message = "Error parameters";
                API_result.Data = "NULL";
                API_result.ErrorCode = -1;
                return BadRequest(API_result);
            }
            if (val >= 0)
            {
                API_result.Success = true;
                API_result.Message = "NULL";
                API_result.Data = Convert.ToString(val);
                API_result.ErrorCode = 0;
                Basic_link.Close_link();
                return Ok(API_result);
            }
            if (val >= -19 && val <= -10)//sql操作出错
            {
                API_result.Success = false;
                API_result.Message = "Error SQL action";
                API_result.Data = "NULL";
                API_result.ErrorCode = val;
                Basic_link.Close_link();
                return BadRequest(API_result);
            }
            if (val == -21)//没有用户名对应的用户
            {
                API_result.Success = false;
                API_result.Message = "Error Name";
                API_result.Data = "NULL";
                API_result.ErrorCode = val;
                Basic_link.Close_link();
                return NotFound(API_result);
            }
            if (val == -22)//原始密码错误
            {
                API_result.Success = false;
                API_result.Message = "Error Pre_password";
                API_result.Data = "NULL";
                API_result.ErrorCode = val;
                Basic_link.Close_link();
                return BadRequest(API_result);
            }

            //对于其他的意料之外的问题
            API_result.Success = false;
            API_result.Message = "Unknown Error";
            API_result.Data = "NULL";
            Basic_link.Close_link();
            API_result.ErrorCode = val;
            return BadRequest(API_result);
        }

    }
}
