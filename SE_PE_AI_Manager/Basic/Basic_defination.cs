using System.ComponentModel.DataAnnotations;

namespace SE_PE_AI_Manager.Basic
{
    public class API_result//接口返回值类
    {
        // 核心属性
        public bool Success { get; set; }      // 操作是否成功
        public string Message { get; set; }    // 结果消息（成功/错误信息）
        public string Data { get; set; }       // 返回的业务数据（任意类型）
        public int? ErrorCode { get; set; }    // 错误代码（用于分类错误）

    }

    public class Function_result
    {
        public int Code;
        public string Message;
    }

    public class Request_one//单请求参数
    {
        [Required]
        public string First { get; set; } = null!;
    }

    public class Request_two//双请求参数
    {
        [Required]
        public string First { get; set; } = null!;

        [Required]
        public string Second { get; set; } = null!;
    }

    public class Request_three//三请求参数
    {
        [Required]
        public string First { get; set; } = null!;

        [Required]
        public string Second { get; set; } = null!;

        [Required]
        public string Third { get; set; } = null!;
    }

    public class Request_four//四请求参数
    {
        [Required]
        public string First { get; set; } = null!;

        [Required]
        public string Second { get; set; } = null!;

        [Required]
        public string Third { get; set; } = null!;

        [Required]
        public string Fourth { get; set; } = null!;
    }

    public class Request_five//五请求参数
    {
        [Required]
        public string First { get; set; } = null!;

        [Required]
        public string Second { get; set; } = null!;

        [Required]
        public string Third { get; set; } = null!;

        [Required]
        public string Fourth { get; set; } = null!;

        [Required]
        public string Fifth { get; set; } = null!;
    }

    public class Request_six//六请求参数
    {
        [Required]
        public string First { get; set; } = null!;

        [Required]
        public string Second { get; set; } = null!;

        [Required]
        public string Third { get; set; } = null!;

        [Required]
        public string Fourth { get; set; } = null!;

        [Required]
        public string Fifth { get; set; } = null!;

        [Required]
        public string Sixth { get; set; } = null!;

    }
    public class Request_seven//七请求参数
    {
        [Required]
        public string First { get; set; } = null!;

        [Required]
        public string Second { get; set; } = null!;

        [Required]
        public string Third { get; set; } = null!;

        [Required]
        public string Fourth { get; set; } = null!;
        
        [Required]
        public string Fifth { get; set; } = null!;

        [Required]
        public string Sixth { get; set; } = null!;

        [Required]
        public string Seventh { get; set; } = null!;
    }
}
