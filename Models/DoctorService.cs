using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarinaRegSystem.Models
{
    [Table("DoctorServices")]
    public class DoctorService
    {
        [Key]
        public int Id { get; set; }

        public string Uid { get; set; }

        [Required(ErrorMessage = "الطبيب مطلوب")]
        [Display(Name = "الطبيب")]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "الخدمة مطلوبة")]
        [Display(Name = "الخدمة")]
        public int ServiceId { get; set; }

        [Display(Name = "السعر الخاص")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? SpecialPrice { get; set; }

        [Display(Name = "ملاحظات")]
        [StringLength(500, ErrorMessage = "الملاحظات يجب أن لا تتجاوز 500 حرف")]
        public string Notes { get; set; }

        [Display(Name = "نشط")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "تاريخ الإنشاء")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "تاريخ التحديث")]
        public DateTime? UpdatedAt { get; set; }

        // العلاقات
        [ForeignKey("DoctorId")]
        public virtual Doctor Doctor { get; set; }

        [ForeignKey("ServiceId")]
        public virtual Service Service { get; set; }
    }
} 