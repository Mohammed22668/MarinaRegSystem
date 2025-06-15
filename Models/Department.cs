using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;


namespace MarinaRegSystem.Models
{
    [Table("Departments")]
    public class Department
    {
        [Key]
        public int Id { get; set; }

        public string Uid { get; set; }

        [Required(ErrorMessage = "اسم القسم مطلوب")]
        [StringLength(100, ErrorMessage = "اسم القسم يجب أن لا يتجاوز 100 حرف")]
        [Display(Name = "اسم القسم")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "الوصف يجب أن لا يتجاوز 500 حرف")]
        [Display(Name = "الوصف")]
        public string Description { get; set; }

        [Display(Name = "الحالة")]
        public bool Status { get; set; } = true;

        [Display(Name = "تاريخ الإنشاء")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "تاريخ التحديث")]
        public DateTime? UpdatedAt { get; set; }

        // العلاقات

        public virtual ICollection<Appointment> Appointments { get; set; }
        public virtual ICollection<Doctor> Doctors { get; set; }
    }
}