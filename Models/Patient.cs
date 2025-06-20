using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MarinaRegSystem.Models
{
    [Table("Patients")]
    public class Patient
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        [Required(ErrorMessage = "الاسم الأول مطلوب")]
        [Display(Name = "الاسم الأول")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "الاسم الثاني مطلوب")]
        [Display(Name = "الاسم الثاني")]
        public string SecondName { get; set; }

        [Required(ErrorMessage = "الاسم الثالث مطلوب")]
        [Display(Name = "الاسم الثالث")]
        public string ThirdName { get; set; }

        [Required(ErrorMessage = "الاسم الرابع مطلوب")]
        [Display(Name = "الاسم الرابع")]
        public string FourthName { get; set; }

        [Required(ErrorMessage = "تاريخ الميلاد مطلوب")]
        [Display(Name = "تاريخ الميلاد")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "الجنس مطلوب")]
        [Display(Name = "الجنس")]
        public string Gender { get; set; }


        public string Address { get; set; }

        [Required(ErrorMessage = "رقم الهوية الوطنية مطلوب")]
        [Display(Name = "رقم الهوية الوطنية")]
        public string NationalNumber { get; set; }

        [Display(Name = "البلد")]
        [Required(ErrorMessage = "البلد مطلوبة")]
        public string Country { get; set; }

        [Display(Name = "المدينة")]
        [Required(ErrorMessage = "المدينة مطلوبة")]
        public string Province { get; set; }

        [Display(Name = "فصيلة الدم")]
        [Required(ErrorMessage = "فصيلة الدم مطلوبة")]
        public string BloodType { get; set; }

        [Display(Name = "الحساسية")]
        public string Allergies { get; set; }

        [Display(Name = "الأمراض المزمنة")]
        public string ChronicDiseases { get; set; }



        [Display(Name = "شخص مقرب")]
        public string ClosePerson { get; set; }


        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Display(Name = "رقم الهاتف")]
        public string PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;



        // العلاقة
        public virtual cUsers User { get; set; }

        public virtual ICollection<Appointment> Appointments { get; set; }

        public Patient()
        {
            Appointments = new HashSet<Appointment>();
        }


    }
}