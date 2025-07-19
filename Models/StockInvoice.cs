using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarinaRegSystem.Models
{
    [Table("StockInvoices")]
    public class StockInvoice
    {
        public int Id { get; set; }

        [Display(Name = "رقم الفاتورة")]
        [Required]
        public string InvoiceNumber { get; set; }

        [Display(Name = "تاريخ الفاتورة")]
        [Required]
        public DateTime InvoiceDate { get; set; } = DateTime.Now;

        [Display(Name = "المستخدم الذي أنشأ الفاتورة")]
        public long CreatedByUserId { get; set; } // ← انتبه لنوع Id في جدول cUsers

        [ForeignKey("CreatedByUserId")]
        public virtual cUsers CreatedByUser { get; set; }

        public virtual ICollection<StockInvoiceItem> Items { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
