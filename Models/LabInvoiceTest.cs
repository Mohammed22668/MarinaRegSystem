using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarinaRegSystem.Models
{


    [Table("LabInvoiceTests")]
    public class LabInvoiceTest
    {
        public long Id { get; set; }

        public long LabInvoiceId { get; set; }

        public int LabTestId { get; set; }

        public decimal Price { get; set; }

        [Display(Name = "الوحدة المستخدمة")]
        public decimal QuantityUsed { get; set; } = 1;

        [Display(Name = "النتيجة")]
        public decimal? ResultValue { get; set; }

        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }

        public virtual LabInvoice LabInvoice { get; set; }
        public virtual LabTest LabTest { get; set; }
    }



}