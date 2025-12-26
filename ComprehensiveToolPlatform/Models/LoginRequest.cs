using System.ComponentModel.DataAnnotations;

namespace ComprehensiveToolPlatform
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "账号不能为空")]
        [Display(Name = "账号")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "密码不能为空")]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        //[Display(Name = "记住我")]
        //public bool RememberMe { get; set; }
    }
}