using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MarinaRegSystem.Models
{
    public class EditAppointmentViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "يرجى اختيار القسم")]
        [Display(Name = "القسم")]
        public int DepartmentId { get; set; }

        [Display(Name = "القسم الفرعي")]
        public int? SubDepartmentId { get; set; }


        [Display(Name = "الطبيب")]
        public int? DoctorId { get; set; }

        [Required(ErrorMessage = "يرجى إدخال تاريخ الموعد")]
        [Display(Name = "تاريخ الموعد")]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; }

        [Display(Name = "وقت الموعد")]
        [DataType(DataType.Time)]
        public TimeSpan? AppointmentTime { get; set; }

        [Display(Name = "شفت الدوام")]
        public ShiftType? Shift { get; set; }

        [Display(Name = "ملاحظات")]
        [MaxLength(500, ErrorMessage = "لا يمكن أن تتجاوز الملاحظات 500 حرف")]
        public string Notes { get; set; }

        [Display(Name = "ملف التشخيص")]
        public IFormFile DiagnosisFile { get; set; }

        public string DiagnosisFileUrl { get; set; }
    }
}
