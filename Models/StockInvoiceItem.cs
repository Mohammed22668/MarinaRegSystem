// Models/StockInvoiceItem.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarinaRegSystem.Models
{
    [Table("StockInvoiceItems")]
    public class StockInvoiceItem
    {
        public int Id { get; set; }

        [Display(Name = "رقم الفاتورة")]
        public int StockInvoiceId { get; set; }

        [ForeignKey("StockInvoiceId")]
        public virtual StockInvoice StockInvoice { get; set; }

        [Display(Name = "التحليل")]
        public int LabTestId { get; set; }

        [ForeignKey("LabTestId")]
        public virtual LabTest LabTest { get; set; }

        [Display(Name = "الكمية المضافة")]
        [Range(0.01, double.MaxValue, ErrorMessage = "الكمية يجب أن تكون أكبر من 0")]
        public decimal QuantityAdded { get; set; }

        [Display(Name = "تاريخ انتهاء الصلاحية")]
        public DateTime? ExpiryDate { get; set; }

        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }
    }
}
