using System;
using System.Collections.Generic;
using MarinaRegSystem.Models;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class CreateLabInvoiceViewModel
{
    // معرف الفاتورة (مطلوب للتعديل)
    public int Id { get; set; }

    public long? PatientId { get; set; }

    [Required]
    public string FullName { get; set; }

    public int? Age { get; set; }

    public string DoctorName { get; set; }

    public string NationalNumber { get; set; }

    // مسار صورة الهوية (محفوظ في قاعدة البيانات)
    public string? NationalIdImage { get; set; }

    // ملف الصورة المرفوعة (غير محفوظ بقاعدة البيانات)
    [NotMapped]
    public IFormFile? NationalIdImageFile { get; set; }

    public List<int> SelectedTestIds { get; set; } = new List<int>();

    // تستخدم في صفحة الإنشاء/التعديل
    [NotMapped]
    public List<LabTestWithStockViewModel> AvailableLabTests { get; set; }

    // تستخدم في صفحة الإنشاء فقط (اختياري)
    [NotMapped]
    public List<LabTestWithStockViewModel> AvailableTests { get; set; }

    [NotMapped]
    public List<Patient> Patients { get; set; }

    public List<LabInvoiceTestViewModel> TestsWithResults { get; set; } = new List<LabInvoiceTestViewModel>();


}
