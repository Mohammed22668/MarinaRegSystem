using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarinaRegSystem.Models
{
    [Table("StockEntries")]
    public class StockEntry
    {
        public int Id { get; set; }

        [Display(Name = "التحليل")]
        public int LabTestId { get; set; }

        [ForeignKey("LabTestId")]
        public virtual LabTest LabTest { get; set; }

        [Display(Name = "الكمية المضافة")]
        [Range(0, double.MaxValue, ErrorMessage = "الرجاء إدخال قيمة صحيحة")]
        public decimal QuantityAdded { get; set; }

        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }

        [Display(Name = "تاريخ الإضافة")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "المستخدم")]
        public long CreatedByUserId { get; set; }  // ← غير من string إلى long

        [ForeignKey("CreatedByUserId")]
        public virtual cUsers CreatedByUser { get; set; }


    }
}
