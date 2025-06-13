using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarinaRegSystem.Models
{
    [Table("Services")]
    public class Service
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "الرجاء إدخال اسم الخدمة")]
        [Display(Name = "اسم الخدمة")]
        public string Name { get; set; }

        [Display(Name = "الوصف")]
        public string Description { get; set; }

        [Required(ErrorMessage = "الرجاء إدخال مدة الخدمة")]
        [Display(Name = "المدة (دقائق)")]
        public int Duration { get; set; }

        [Required(ErrorMessage = "الرجاء إدخال سعر الخدمة")]
        [Display(Name = "السعر")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Display(Name = "الحالة")]
        public bool Status { get; set; } = true;

        [Display(Name = "القسم")]
        public int DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public Department Department { get; set; }

        [Display(Name = "تاريخ الإنشاء")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "تاريخ التحديث")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<DoctorService> DoctorServices { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; }
    }
} 