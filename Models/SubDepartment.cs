using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarinaRegSystem.Models
{
    [Table("SubDepartments")]
    public class SubDepartment
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم القسم الفرعي مطلوب")]
        [StringLength(100)]
        [Display(Name = "اسم القسم الفرعي")]
        public string Name { get; set; }

        [Required(ErrorMessage = "الوصف مطلوب")]
        [StringLength(500)]
        [Display(Name = "الوصف")]
        public string Description { get; set; }

        [Display(Name = "القسم الرئيسي")]
        public int DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }

        [Display(Name = "الحالة")]
        public bool Status { get; set; } = true;

        [Display(Name = "تاريخ الإنشاء")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "تاريخ التحديث")]
        public DateTime? UpdatedAt { get; set; }


        // علاقات
        public virtual ICollection<Doctor> Doctors { get; set; }
    }
}
