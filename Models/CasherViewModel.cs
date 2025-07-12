using System;
using System.Collections.Generic;
using System.Linq;
using MarinaRegSystem.Models;



public class CashierDashboardViewModel
{
    public int TotalInvoices { get; set; }
    public int PendingInvoices { get; set; }
    public int CanceledInvoices { get; set; }
    public decimal TotalPaidAmount { get; set; }
}

public class CashierReportViewModel
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? Status { get; set; }

    public List<LabInvoice> FilteredInvoices { get; set; } = new();
    public decimal TotalAmount => FilteredInvoices.Sum(i => i.LabInvoiceTests.Sum(t => t.Price));
}

