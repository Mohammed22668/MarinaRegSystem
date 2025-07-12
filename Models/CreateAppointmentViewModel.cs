using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MarinaRegSystem.Models
{
    public class CreateAppointmentViewModel
    {
        // المرحلة 1: اختيار نوع الحجز
        [Display(Name = "الحجز لنفسي")]
        public bool IsBookingForSelf { get; set; } = true; // القيمة الافتراضية لنفسي

        // المرحلة 2: بيانات المريض (في حال الحجز لشخص آخر)
        [Display(Name = "الاسم الأول")]
        public string? FirstName { get; set; }
        [Display(Name = "الاسم الثاني")]
        public string? SecondName { get; set; }
        [Display(Name = "الاسم الثالث")]
        public string? ThirdName { get; set; }
        [Display(Name = "الاسم الرابع")]
        public string? FourthName { get; set; }

        [Display(Name = "الجنس")]
        public string? Gender { get; set; }

        [Display(Name = "تاريخ الميلاد")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "فصيلة الدم")]
        public string? BloodType { get; set; }

        // المرحلة 3: بيانات الحجز
        [Required(ErrorMessage = "القسم مطلوب")]
        [Display(Name = "القسم")]
        public int DepartmentId { get; set; }

        [Display(Name = "القسم الفرعي")]
        public int? SubDepartmentId { get; set; }

        [Display(Name = "الطبيب")]
        public int? DoctorId { get; set; }

        [Required(ErrorMessage = "تاريخ الموعد مطلوب")]
        [Display(Name = "تاريخ الموعد")]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; }

        [Display(Name = "وقت الموعد")]
        [DataType(DataType.Time)]
        public TimeSpan? AppointmentTime { get; set; }

        [Display(Name = "الشفت")]
        public ShiftType? Shift { get; set; }

        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }

        // ملف التشخيص
        public IFormFile? DiagnosisFile { get; set; }
    }



}