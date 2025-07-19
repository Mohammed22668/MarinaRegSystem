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
        public long? UserId { get; set; }
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

        [Display(Name = "العمر")]
        public int? Age { get; set; }

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


        [Display(Name = "المنطقة")]
        public string City { get; set; }

        [Display(Name = "الحي")]
        public string Neighborhood { get; set; }

        [Display(Name = "فصيلة الدم")]
        [Required(ErrorMessage = "فصيلة الدم مطلوبة")]
        public string BloodType { get; set; }

        [Display(Name = "الحساسية")]
        public string Allergies { get; set; }

        [Display(Name = "الأمراض المزمنة")]
        public string ChronicDiseases { get; set; }



        [Display(Name = "شخص مقرب")]
        public string ClosePerson { get; set; }



        [Display(Name = "رقم الهاتف")]
        public string PhoneNumber { get; set; }


        [Display(Name = "الاسم باللغة الإنجليزية")]
        public string EnglishFullName { get; set; }

        [Display(Name = "رقم الجواز")]
        public string PassportNumber { get; set; }

        [Display(Name = "رقم الضمان")]
        public string InsuranceNumber { get; set; }

        [Display(Name = "الطول (سم)")]
        public double? Height { get; set; }

        [Display(Name = "الوزن (كغم)")]
        public double? Weight { get; set; }

        [Display(Name = "الأعراض")]
        public string Symptoms { get; set; }

        [Display(Name = "مدة الدورة الشهرية")]
        public string MenstrualCycleDuration { get; set; }

        [Display(Name = "ملاحظات إضافية")]
        public string AdditionalNotes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;


        [Display(Name = "رقم المريض")]
        public string PatientNumber { get; set; }
        // العلاقة
        public virtual cUsers User { get; set; }

        public virtual ICollection<Appointment> Appointments { get; set; }
        public virtual ICollection<LabInvoice> LabInvoices { get; set; } = new List<LabInvoice>();

        public Patient()
        {
            Appointments = new HashSet<Appointment>();
            LabInvoices = new HashSet<LabInvoice>();

        }




    }
}