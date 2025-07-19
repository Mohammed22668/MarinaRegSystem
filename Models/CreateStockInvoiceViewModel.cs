using System;
using System.Collections.Generic;
using MarinaRegSystem.Models;

public class CreateStockInvoiceViewModel
{
    // بيانات الفاتورة
    public string InvoiceNumber { get; set; }

    public DateTime InvoiceDate { get; set; } = DateTime.Now;

    public string CreatedByUsername { get; set; }

    // تفاصيل التحاليل المضافة
    public List<StockInvoiceItemViewModel> Items { get; set; } = new List<StockInvoiceItemViewModel>();

    // لعرض قائمة التحاليل في صفحة الإنشاء
    public List<LabTest> AvailableLabTests { get; set; }
}

public class StockInvoiceItemViewModel
{
    public int LabTestId { get; set; }

    public decimal QuantityAdded { get; set; }

    public string Notes { get; set; }

    public DateTime? ExpiryDate { get; set; }
}
