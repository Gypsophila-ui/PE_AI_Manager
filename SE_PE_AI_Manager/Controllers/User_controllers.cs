using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using SE_PE_AI_Manager.Basic;
using SE_PE_AI_Manager.Operation;
using System.Reflection;
using System.Xml.Linq;


namespace SE_PE_AI_Manager.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        [HttpPost("login_teacher")]//教师登录
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

        [HttpPost("login_student")]//学生登录
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

        [HttpPost("new_teacher")]//新建教师用户
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

        [HttpPost("new_student")]//新建学生用户
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

        [HttpPost("change_teacher_info")]//教师修改自己的个人信息
        public ActionResult<API_result> Change_teacher_info([FromBody] Request_seven request)
        {
            string id = request.First;//获取参数
            string jwt = request.Second;
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
            if (id == null || jwt == null || name == null || gender == null || college == null)//如果参数错误
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
            Function_result val = SE_PE_AI_Manager.Operation.User.Change_teacher_info(id, jwt, name, gender, title, college, department, connection);
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

        [HttpPost("change_student_info")]//学生修改自己的个人信息
        public ActionResult<API_result> Change_student_info([FromBody] Request_seven request)
        {
            string id = request.First;//获取参数
            string jwt = request.Second;
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
            if (id == null || jwt == null || name == null || gender == null || college == null)//如果参数错误
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
            Function_result val = SE_PE_AI_Manager.Operation.User.Change_student_info(id, jwt, name, gender, major, college, department, connection);
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

        [HttpPost("change_teacher_password")]//教工修改自己的密码
        public ActionResult<API_result> Change_teacher_password([FromBody] Request_three request)
        {
            string id = request.First;//获取参数
            string old_password = request.Second;
            string new_password = request.Third;
            var API_result = new API_result//初始化的返回值
            {
                Success = true,
                Message = "NULL",
                Data = "NULL",
                ErrorCode = 0
            };
            if (id == null || old_password == null || new_password == null) //如果参数错误
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
            Function_result val = SE_PE_AI_Manager.Operation.User.Change_teacher_password(id, old_password ,new_password , connection);
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

        [HttpPost("change_student_password")]//学生修改自己的密码
        public ActionResult<API_result> Change_student_password([FromBody] Request_three request)
        {
            string id = request.First;//获取参数
            string old_password = request.Second;
            string new_password = request.Third;
            var API_result = new API_result//初始化的返回值
            {
                Success = true,
                Message = "NULL",
                Data = "NULL",
                ErrorCode = 0
            };
            if (id == null || old_password == null || new_password == null) //如果参数错误
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
            Function_result val = SE_PE_AI_Manager.Operation.User.Change_student_password(id, old_password, new_password, connection);
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

        [HttpPost("get_teacher_info")]//获取教师信息
        public ActionResult<API_result> Get_teacher_info([FromBody] Request_four request)
        {
            string id = request.First;//获取参数
            string jwt = request.Second;
            string user_type = request.Third;
            string teacher_id = request.Fourth;
            var API_result = new API_result//初始化的返回值
            {
                Success = true,
                Message = "NULL",
                Data = "NULL",
                ErrorCode = 0
            };
            if (id == null || jwt == null || user_type == null || teacher_id == null)//如果参数错误
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
            Function_result val = SE_PE_AI_Manager.Operation.User.Get_teacher_info(id, jwt, user_type, teacher_id, connection);
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

        [HttpPost("get_student_info")]//获取学生信息
        public ActionResult<API_result> Get_student_info([FromBody] Request_four request)
        {
            string id = request.First;//获取参数
            string jwt = request.Second;
            string user_type = request.Third;
            string student_id = request.Fourth;
            var API_result = new API_result//初始化的返回值
            {
                Success = true,
                Message = "NULL",
                Data = "NULL",
                ErrorCode = 0
            };
            if (id == null || jwt == null || user_type == null || student_id == null)//如果参数错误
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
            Function_result val = SE_PE_AI_Manager.Operation.User.Get_student_info(id, jwt, user_type, student_id, connection);
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
    }
}
