using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MarinaRegSystem.Models;

public class ReportFilterViewModel
{
    [Display(Name = "من التاريخ")]
    [DataType(DataType.Date)]
    public DateTime? FromDate { get; set; }

    [Display(Name = "إلى التاريخ")]
    [DataType(DataType.Date)]
    public DateTime? ToDate { get; set; }

    public List<LabInvoice> FilteredInvoices { get; set; }

    public decimal TotalPaid { get; set; }
    public decimal TotalPending { get; set; }
    public decimal TotalCanceled { get; set; }
}
