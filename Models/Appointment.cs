using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarinaRegSystem.Models
{
    [Table("Appointments")]
    public class Appointment
    {
        [Key]
        public int Id { get; set; }



        [Display(Name = "الاسم الرباعي")]
        public string? PatientName { get; set; }

        [Required(ErrorMessage = "المستخدم مطلوب")]
        [Display(Name = "المستخدم")]
        public long UserId { get; set; }

        [Required(ErrorMessage = "القسم مطلوب")]
        [Display(Name = "القسم الرئيسي")]
        public int DepartmentId { get; set; }


        [Display(Name = "القسم الفرعي")]
        public int? SubDepartmentId { get; set; }

        [ForeignKey("SubDepartmentId")]
        public virtual SubDepartment SubDepartment { get; set; }



        [Display(Name = "الطبيب")]
        public int? DoctorId { get; set; }

        [Required(ErrorMessage = "تاريخ الموعد مطلوب")]
        [Display(Name = "تاريخ الموعد")]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; }


        [Display(Name = "وقت الموعد")]
        [DataType(DataType.Time)]
        public TimeSpan AppointmentTime { get; set; }

        [Display(Name = "حالة الموعد")]
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        [Display(Name = "ملاحظات")]
        [StringLength(500, ErrorMessage = "الملاحظات يجب أن لا تتجاوز 500 حرف")]
        public string Notes { get; set; }

        [Display(Name = "سبب الرفض")]
        [StringLength(500, ErrorMessage = "سبب الرفض يجب أن لا يتجاوز 500 حرف")]
        public string RejectionReason { get; set; }


        [Display(Name = "سبب الالغاء")]
        [StringLength(500, ErrorMessage = "سبب الالغاء يجب أن لا يتجاوز 500 حرف")]
        public string CancelReason { get; set; }

        [Display(Name = "شرح السبب")]
        [StringLength(500, ErrorMessage = "شرح السبب يجب أن لا يتجاوز 500 حرف")]
        public string ExplainReason { get; set; }


        [Display(Name = "تاريخ التحديث")]
        public DateTime? TimeCancel { get; set; }

        [Display(Name = "السعر")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Display(Name = "تاريخ الإنشاء")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "تاريخ التحديث")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "ملف التشخيص")]
        public string DiagnosisFileUrl { get; set; }

        // العلاقات
        [ForeignKey("UserId")]
        public virtual cUsers User { get; set; }


        public long? PatientId { get; set; }

        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; }



        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }


        [ForeignKey("DoctorId")]
        public virtual Doctor Doctor { get; set; }

        [Display(Name = "الشفت")]
        public ShiftType? Shift { get; set; }



        // 
        [Display(Name = "تاريخ الميلاد")]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "الجنس")]
        public string? Gender { get; set; }

        [Display(Name = "فصيلة الدم")]
        public string? BloodType { get; set; }
    }

    public enum AppointmentStatus
    {
        [Display(Name = "قيد الانتظار")]
        Pending = 0,

        [Display(Name = "تم التأكيد")]
        Confirmed = 1,

        [Display(Name = "مرفوض")]
        Rejected = 2,

        [Display(Name = "مكتمل")]
        Completed = 3,

        [Display(Name = "ملغي")]
        Cancelled = 4
    }




}