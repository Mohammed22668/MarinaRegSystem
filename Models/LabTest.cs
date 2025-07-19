using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MarinaRegSystem.Models
{
    [Table("LabTests")]
    public class LabTest
    {
        public int Id { get; set; }

        [Display(Name = "اسم التحليل")]
        [Required]
        public string Name { get; set; }

        [Display(Name = "الوحدة")]
        public string Unit { get; set; }

        [Display(Name = "القيمة الدنيا")]
        [Range(0, double.MaxValue)]
        public decimal? MinValue { get; set; }

        [Display(Name = "القيمة العليا")]
        [Range(0, double.MaxValue)]
        public decimal? MaxValue { get; set; }

        [Display(Name = "السعر")]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Display(Name = "الكمية المستهلكة لكل مريض")]
        [Range(0, double.MaxValue)]
        public decimal UsagePerPatient { get; set; }

        [Display(Name = "ملاحظات")]
        public string Notes { get; set; }

        [Display(Name = "تاريخ الإضافة")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;


        [Display(Name = "التصنيف")]
        public int? LabTestCategoryId { get; set; }

        [ForeignKey("LabTestCategoryId")]
        public virtual LabTestCategory LabTestCategory { get; set; }

        public virtual ICollection<LabInvoiceTest> LabInvoiceTests { get; set; } = new HashSet<LabInvoiceTest>();


        public virtual ICollection<StockEntry> StockEntries { get; set; } = new HashSet<StockEntry>();


        [NotMapped]
        public decimal AvailableStock => StockEntries?.Sum(se => se.QuantityAdded) ?? 0;







    }
}
