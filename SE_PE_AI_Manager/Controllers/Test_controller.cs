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
    public class TestController : ControllerBase
    {
        [HttpPost("std_student")]//新建学生校验
        public ActionResult<API_result> Std_student()
        {
            var API_result = new API_result//初始化的返回值
            {
                Success = true,
                Message = "NULL",
                Data = "NULL",
                ErrorCode = 0
            };
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
            var uploadStd = new SE_PE_AI_Manager.Test.Upload_std(); // 创建实例
            Function_result val = uploadStd.std_student(connection); // 调用实例方法
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

        [HttpPost("std_teacher")]//新建教师校验
        public ActionResult<API_result> Std_teacher()
        {
            var API_result = new API_result//初始化的返回值
            {
                Success = true,
                Message = "NULL",
                Data = "NULL",
                ErrorCode = 0
            };
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
            var uploadStd = new SE_PE_AI_Manager.Test.Upload_std(); // 创建实例
            Function_result val = uploadStd.std_teacher(connection); // 调用实例方法
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

        [HttpPost("std_course")]//新建教师校验
        public ActionResult<API_result> Std_course()
        {
            var API_result = new API_result//初始化的返回值
            {
                Success = true,
                Message = "NULL",
                Data = "NULL",
                ErrorCode = 0
            };
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
            var uploadStd = new SE_PE_AI_Manager.Test.Upload_std(); // 创建实例
            Function_result val = uploadStd.std_course(connection); // 调用实例方法
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
