using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MarinaRegSystem.Data;
using MarinaRegSystem.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;


namespace MarinaRegSystem.Controllers
{
    [Authorize(Roles = "Cashier")]
    public class CashierController : BaseController


    {



        public CashierController(ApplicationDbContext context) : base(context) { }


        [HttpGet]
        [Authorize(Roles = "Cashier")]
        public override async Task<IActionResult> Index()
        {
            var invoices = _context.LabInvoices
        .Include(i => i.LabInvoiceTests)
        .ToList();

            var viewModel = new CashierDashboardViewModel
            {
                TotalInvoices = invoices.Count,
                PendingInvoices = invoices.Count(i => i.Status == "Pending"),
                CanceledInvoices = invoices.Count(i => i.Status == "Canceled"),
                TotalPaidAmount = invoices
                    .Where(i => i.Status == "Paid")
                    .Sum(i => i.LabInvoiceTests.Sum(t => t.Price))
            };

            return View(viewModel);


        }


        // صفحة الفواتير
        public IActionResult Invoices()
        {
            var invoices = _context.LabInvoices
                .Include(i => i.LabInvoiceTests)
                .ToList();

            return View(invoices);
        }


        // تغيير حالة الفاتورة
        [HttpPost]
        [Authorize(Roles = "Cashier")]
        public async Task<IActionResult> UpdateStatus(long id, string status)
        {
            var invoice = await _context.LabInvoices.FindAsync(id);
            if (invoice == null)
            {
                return NotFound();
            }

            invoice.Status = status;
            await _context.SaveChangesAsync();

            return RedirectToAction("Invoices");
        }

        // صفحة التقارير

        [Authorize(Roles = "Cashier")]
        public async Task<IActionResult> Reports(DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.LabInvoices
                .Include(i => i.LabInvoiceTests)
                .ThenInclude(t => t.LabTest)
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(i => i.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(i => i.CreatedAt <= toDate.Value);

            var invoices = await query.ToListAsync();

            var model = new ReportFilterViewModel
            {
                FromDate = fromDate,
                ToDate = toDate,
                FilteredInvoices = invoices,
                TotalPaid = invoices.Where(i => i.Status == "Paid").Sum(i => i.LabInvoiceTests.Sum(t => t.Price)),
                TotalPending = invoices.Where(i => i.Status == "Pending").Sum(i => i.LabInvoiceTests.Sum(t => t.Price)),
                TotalCanceled = invoices.Where(i => i.Status == "Canceled").Sum(i => i.LabInvoiceTests.Sum(t => t.Price)),
            };

            return View(model);
        }

        [Authorize(Roles = "Cashier")]
        public async Task<IActionResult> PrintReport(DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.LabInvoices
                .Include(i => i.LabInvoiceTests)
                .ThenInclude(t => t.LabTest)
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(i => i.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(i => i.CreatedAt <= toDate.Value);

            var invoices = await query.ToListAsync();

            var model = new ReportFilterViewModel
            {
                FromDate = fromDate,
                ToDate = toDate,
                FilteredInvoices = invoices,
                TotalPaid = invoices.Where(i => i.Status == "Paid").Sum(i => i.LabInvoiceTests.Sum(t => t.Price)),
                TotalPending = invoices.Where(i => i.Status == "Pending").Sum(i => i.LabInvoiceTests.Sum(t => t.Price)),
                TotalCanceled = invoices.Where(i => i.Status == "Canceled").Sum(i => i.LabInvoiceTests.Sum(t => t.Price)),
            };

            return View(model);
        }


    }
}