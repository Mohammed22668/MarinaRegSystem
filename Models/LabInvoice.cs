using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace MarinaRegSystem.Models
{
    [Table("LabInvoices")]
    public class LabInvoice
    {
        public long Id { get; set; }

        public long? PatientId { get; set; } // مرتبط بجدول المرضى إن وجد

        [Display(Name = "اسم الطبيب")]
        public string DoctorName { get; set; } // اختياري

        [Display(Name = "العمر")]
        public int? Age { get; set; } // احتياطي في حال لم يكن مرتبط بمريض

        [Display(Name = "اسم المريض")]
        public string FullName { get; set; } // إذا لم يكن هناك سجل مريض

        [Display(Name = "رقم الهوية الوطنية")]
        public string NationalNumber { get; set; }

        [Display(Name = "صورة الهوية")]
        public string NationalIdImagePath { get; set; } // يتم رفعها اختياريًا

        public string? NationalIdImage { get; set; }

        public string? Status { get; set; }



        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsPaid { get; set; } = false;

        // العلاقة
        public virtual Patient Patient { get; set; }
        public virtual ICollection<LabInvoiceTest> LabInvoiceTests { get; set; }

        public LabInvoice()
        {
            LabInvoiceTests = new HashSet<LabInvoiceTest>();
        }
    }
}
