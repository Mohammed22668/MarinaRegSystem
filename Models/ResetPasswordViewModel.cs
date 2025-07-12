using System.ComponentModel.DataAnnotations;

namespace MarinaRegSystem.Models
{
    public class ResetPasswordViewModel
    {
        public long UserId { get; set; }

        [Display(Name = "اسم المستخدم")]
        public string Username { get; set; }

        [Required(ErrorMessage = "كلمة السر الجديدة مطلوبة")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "كلمة السر يجب أن تكون على الأقل 6 أحرف")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "تأكيد كلمة السر مطلوب")]
        [Compare("NewPassword", ErrorMessage = "كلمة السر غير متطابقة")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}
