using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarinaRegSystem.Models
{
    [Table("Doctors")]
    public class Doctor
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "الرجاء إدخال اسم الطبيب")]
        [Display(Name = "اسم الطبيب")]
        public string Name { get; set; }

        [Required(ErrorMessage = "الرجاء إدخال التخصص")]
        [Display(Name = "التخصص")]
        public string Speciality { get; set; }

        [Display(Name = "رقم الهاتف")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "الرجاء إدخال سنوات الخبرة")]
        [Display(Name = "سنوات الخبرة")]
        public int Experience { get; set; }

        [Display(Name = "نبذة")]
        public string Bio { get; set; }

        [Display(Name = "صورة الطبيب")]
        public string ImageUrl { get; set; }

        [Display(Name = "التقييم")]
        [Column(TypeName = "decimal(3,1)")]
        public decimal Rating { get; set; }

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

        // العلاقات
        public virtual ICollection<DoctorService> DoctorServices { get; set; }
        public virtual ICollection<DoctorSchedule> DoctorSchedules { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; }
    }
} 