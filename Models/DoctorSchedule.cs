using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarinaRegSystem.Models
{
    [Table("DoctorSchedules")]
    public class DoctorSchedule
    {
        [Key]
        public int Id { get; set; }

        public string Uid { get; set; }

        [Required(ErrorMessage = "الطبيب مطلوب")]
        [Display(Name = "الطبيب")]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "يوم الأسبوع مطلوب")]
        [Range(0, 6, ErrorMessage = "يوم الأسبوع غير صحيح")]
        [Display(Name = "يوم الأسبوع")]
        public DayOfWeek DayOfWeek { get; set; }

        [Required(ErrorMessage = "وقت البدء مطلوب")]
        [Display(Name = "وقت البدء")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "وقت الانتهاء مطلوب")]
        [Display(Name = "وقت الانتهاء")]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        [Display(Name = "مدة الموعد (بالدقائق)")]
        [Range(5, 480, ErrorMessage = "مدة الموعد يجب أن تكون بين 5 و 480 دقيقة")]
        public int AppointmentDuration { get; set; } = 30;

        [Display(Name = "فترة الراحة (بالدقائق)")]
        [Range(0, 60, ErrorMessage = "فترة الراحة يجب أن تكون بين 0 و 60 دقيقة")]
        public int BreakDuration { get; set; } = 0;

        [Display(Name = "نشط")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "تاريخ الإنشاء")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "تاريخ التحديث")]
        public DateTime? UpdatedAt { get; set; }

        [Required]
        public int MaxAppointmentsPerDay { get; set; }

        // العلاقات
        [ForeignKey("DoctorId")]
        public virtual Doctor Doctor { get; set; }
    }
}