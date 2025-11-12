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
    public class HomeworkController : ControllerBase
    {
        [HttpPost("new_homework")]//新建作业
        public ActionResult<API_result> New_homework([FromBody] Request_six request)
        {
            string teacher_id = request.First;//获取参数
            string jwt = request.Second;
            string course_id = request.Third;
            string title = request.Fourth;
            string description = request.Fifth;
            string deadline = request.Sixth;
            var API_result = new API_result//初始化的返回值
            {
                Success = true,
                Message = "NULL",
                Data = "NULL",
                ErrorCode = 0
            };
            if (course_id == null || teacher_id == null || jwt == null || title == null || deadline == null)//如果参数错误
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
            Function_result val = SE_PE_AI_Manager.Operation.Homework.New_homework(teacher_id, jwt, course_id, title, description, deadline, connection);
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

        [HttpPost("edit_homework")]//修改作业
        public ActionResult<API_result> Edit_homework([FromBody] Request_seven request)
        {
            string teacher_id = request.First;//获取参数
            string jwt = request.Second;
            string course_id = request.Third;
            string homework_id = request.Fourth;
            string title = request.Fifth;
            string description = request.Sixth;
            string deadline = request.Seventh;
            var API_result = new API_result//初始化的返回值
            {
                Success = true,
                Message = "NULL",
                Data = "NULL",
                ErrorCode = 0
            };
            if (course_id == null || teacher_id == null || jwt == null || title == null || deadline == null || homework_id == null) //如果参数错误
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
            Function_result val = SE_PE_AI_Manager.Operation.Homework.Edit_homework(teacher_id, jwt, course_id, homework_id, title, description, deadline, connection);
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

        [HttpPost("delete_homework")]//删除作业
        public ActionResult<API_result> Delete_homework([FromBody] Request_four request)
        {
            string teacher_id = request.First;//获取参数
            string jwt = request.Second;
            string course_id = request.Third;
            string homework_id = request.Fourth;
            var API_result = new API_result//初始化的返回值
            {
                Success = true,
                Message = "NULL",
                Data = "NULL",
                ErrorCode = 0
            };
            if (course_id == null || teacher_id == null || jwt == null)//如果参数错误
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
            Function_result val = SE_PE_AI_Manager.Operation.Homework.Delete_homework(teacher_id, jwt, course_id, homework_id, connection);
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

        [HttpPost("get_homework_id_by_course")]//获取一个课程下的所有作业
        public ActionResult<API_result> Get_homework_id_by_course([FromBody] Request_four request)
        {
            string user_type = request.First;//获取参数
            string user_id = request.Second;
            string jwt = request.Third;
            string course_id = request.Fourth;
            var API_result = new API_result//初始化的返回值
            {
                Success = true,
                Message = "NULL",
                Data = "NULL",
                ErrorCode = 0
            };
            if (course_id == null || user_id == null || jwt == null || user_type == null)//如果参数错误
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
            Function_result val = SE_PE_AI_Manager.Operation.Homework.Get_homework_id_by_course(user_type, user_id, jwt, course_id, connection);
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

        [HttpPost("get_info_by_homework_id")]//获取指定课程的作业的信息
        public ActionResult<API_result> Get_info_by_homework_id([FromBody] Request_two request)
        {
            string course_id = request.First;//获取参数
            string homework_id = request.Second;
            var API_result = new API_result//初始化的返回值
            {
                Success = true,
                Message = "NULL",
                Data = "NULL",
                ErrorCode = 0
            };
            if (course_id == null || homework_id == null)//如果参数错误
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
            Function_result val = SE_PE_AI_Manager.Operation.Homework.Get_info_by_homework_id(course_id, homework_id, connection);
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

        [HttpPost("submit_homework")]//提交作业
        public ActionResult<API_result> Submit_homework([FromBody] Request_five request)
        {
            string student_id = request.First;//获取参数
            string jwt = request.Second;
            string course_id = request.Third;
            string homework_id = request.Fourth;
            string video_url = request.Fifth;
            var API_result = new API_result//初始化的返回值
            {
                Success = true,
                Message = "NULL",
                Data = "NULL",
                ErrorCode = 0
            };
            if (course_id == null || homework_id == null || student_id == null || video_url == null || jwt == null)//如果参数错误
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
            Function_result val = SE_PE_AI_Manager.Operation.Homework.Submit_homework(student_id, jwt, course_id, homework_id, video_url, connection);
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

        [HttpPost("get_submit_id_by_homework")]//获取一个作业下的所有提交
        public ActionResult<API_result> Get_submit_id_by_homework([FromBody] Request_four request)
        {
            string user_type = request.First;//获取参数
            string user_id = request.Second;
            string jwt = request.Third;
            string homework_id = request.Fourth;
            var API_result = new API_result//初始化的返回值
            {
                Success = true,
                Message = "NULL",
                Data = "NULL",
                ErrorCode = 0
            };
            if (homework_id == null || user_id == null || jwt == null || user_type == null)//如果参数错误
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
            Function_result val = SE_PE_AI_Manager.Operation.Homework.Get_submit_id_by_homework(user_type, user_id, jwt, homework_id, connection);
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

        [HttpPost("get_submit_id_by_AI_not_test")]//获取一个作业下的所有AI没有测试的提交
        public ActionResult<API_result> Get_submit_id_by_AI_not_test([FromBody] Request_one request)
        {
            string homework_id = request.First;
            var API_result = new API_result//初始化的返回值
            {
                Success = true,
                Message = "NULL",
                Data = "NULL",
                ErrorCode = 0
            };
            if (homework_id == null)//如果参数错误
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
            Function_result val = SE_PE_AI_Manager.Operation.Homework.Get_submit_id_by_AI_not_test(homework_id, connection);
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

        [HttpPost("get_submit_id_by_teacher_not_test")]//获取一个作业下的所有教师没有评分的提交
        public ActionResult<API_result> Get_submit_id_by_teacher_not_test([FromBody] Request_one request)
        {
            string homework_id = request.First;
            var API_result = new API_result//初始化的返回值
            {
                Success = true,
                Message = "NULL",
                Data = "NULL",
                ErrorCode = 0
            };
            if (homework_id == null)//如果参数错误
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
            Function_result val = SE_PE_AI_Manager.Operation.Homework.Get_submit_id_by_teacher_not_test(homework_id, connection);
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

        [HttpPost("set_AI_type")]//增加AI测评识别码
        public ActionResult<API_result> Set_AI_type([FromBody] Request_five request)
        {
            string teacher_id = request.First;//获取参数
            string jwt = request.Second;
            string course_id = request.Third;
            string homework_id = request.Fourth;
            string AI_type = request.Fifth;
            var API_result = new API_result//初始化的返回值
            {
                Success = true,
                Message = "NULL",
                Data = "NULL",
                ErrorCode = 0
            };
            if (course_id == null || homework_id == null || teacher_id == null || AI_type == null || jwt == null)//如果参数错误
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
            Function_result val = SE_PE_AI_Manager.Operation.Homework.Set_AI_type(teacher_id, jwt, course_id, homework_id, AI_type, connection);
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

        [HttpPost("edit_AI_type")]//修改AI测评识别码
        public ActionResult<API_result> Edit_AI_type([FromBody] Request_five request)
        {
            string teacher_id = request.First;//获取参数
            string jwt = request.Second;
            string course_id = request.Third;
            string homework_id = request.Fourth;
            string AI_type = request.Fifth;
            var API_result = new API_result//初始化的返回值
            {
                Success = true,
                Message = "NULL",
                Data = "NULL",
                ErrorCode = 0
            };
            if (course_id == null || homework_id == null || teacher_id == null || AI_type == null || jwt == null)//如果参数错误
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
            Function_result val = SE_PE_AI_Manager.Operation.Homework.Edit_AI_type(teacher_id, jwt, course_id, homework_id, AI_type, connection);
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

        [HttpPost("get_AI_type")]//获取一个作业下的所有教师没有评分的提交
        public ActionResult<API_result> Get_AI_type([FromBody] Request_one request)
        {
            string homework_id = request.First;
            var API_result = new API_result//初始化的返回值
            {
                Success = true,
                Message = "NULL",
                Data = "NULL",
                ErrorCode = 0
            };
            if (homework_id == null)//如果参数错误
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
            Function_result val = SE_PE_AI_Manager.Operation.Homework.Get_AI_type(homework_id, connection);
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

        [HttpPost("AI_test")]//AI测评后将结果写回数据库
        public ActionResult<API_result> AI_test([FromBody] Request_four request)
        {
            string submit_id = request.First;//获取参数
            string video_url = request.Second;
            string score = request.Third;
            string AI_feedback = request.Fourth;
            var API_result = new API_result//初始化的返回值
            {
                Success = true,
                Message = "NULL",
                Data = "NULL",
                ErrorCode = 0
            };
            if (submit_id == null || video_url == null || score == null || AI_feedback == null)//如果参数错误
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
            Function_result val = SE_PE_AI_Manager.Operation.Homework.AI_test(submit_id, video_url, score, AI_feedback, connection);
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

        [HttpPost("teacher_test")]//教师批阅作业
        public ActionResult<API_result> Teacher_test([FromBody] Request_seven request)
        {
            string teacher_id = request.First;//获取参数
            string jwt = request.Second;
            string course_id = request.Third;
            string homework_id = request.Fourth;
            string submit_id = request.Fifth;
            string score = request.Sixth;
            string teacher_feedback = request.Seventh;
            var API_result = new API_result//初始化的返回值
            {
                Success = true,
                Message = "NULL",
                Data = "NULL",
                ErrorCode = 0
            };
            if (course_id == null || teacher_id == null || jwt == null || submit_id == null || score == null || homework_id == null || teacher_feedback == null) //如果参数错误
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
            Function_result val = SE_PE_AI_Manager.Operation.Homework.Teacher_test(teacher_id, jwt, course_id, homework_id, submit_id, score, teacher_feedback, connection);
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

        [HttpPost("get_submit_id_by_student")]//获取一个学生名下下的所有作业
        public ActionResult<API_result> Get_submit_id_by_student([FromBody] Request_five request)
        {
            string user_type = request.First;//获取参数
            string user_id = request.Second;
            string jwt = request.Third;
            string homework_id = request.Fourth;
            string student_id = request.Fifth;
            var API_result = new API_result//初始化的返回值
            {
                Success = true,
                Message = "NULL",
                Data = "NULL",
                ErrorCode = 0
            };
            if (student_id == null || homework_id == null || user_id == null || jwt == null || user_type == null)//如果参数错误
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
            Function_result val = SE_PE_AI_Manager.Operation.Homework.Get_submit_id_by_student(user_type, user_id, jwt, homework_id, student_id, connection);
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

        [HttpPost("get_submit_info")]//获取指定评测的具体信息
        public ActionResult<API_result> Get_submit_info([FromBody] Request_four request)
        {
            string user_type = request.First;//获取参数
            string user_id = request.Second;
            string jwt = request.Third;
            string submit_id = request.Fourth;
            var API_result = new API_result//初始化的返回值
            {
                Success = true,
                Message = "NULL",
                Data = "NULL",
                ErrorCode = 0
            };
            if (submit_id == null || jwt == null || user_id == null || user_type == null)//如果参数错误
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
            Function_result val = SE_PE_AI_Manager.Operation.Homework.Get_submit_info(user_type, user_id, jwt, submit_id, connection);
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

        [HttpPost("get_final_score")]//获取某个学生某个作业的最终得分
        public ActionResult<API_result> Get_final_score([FromBody] Request_five request)
        {
            string user_type = request.First;//获取参数
            string user_id = request.Second;
            string jwt = request.Third;
            string student_id = request.Fourth;
            string homework_id= request.Fifth;
            var API_result = new API_result//初始化的返回值
            {
                Success = true,
                Message = "NULL",
                Data = "NULL",
                ErrorCode = 0
            };
            if (user_type == null || student_id == null || jwt == null || homework_id == null || user_id == null) //如果参数错误
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
            Function_result val = SE_PE_AI_Manager.Operation.Homework.Get_final_score(user_type, user_id, jwt, student_id, homework_id, connection);
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
