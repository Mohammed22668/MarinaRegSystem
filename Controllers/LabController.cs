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
using System.Collections.Generic;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;


namespace MarinaRegSystem.Controllers
{
    [Authorize(Roles = "LabDirector,LabStaff")]
    public class LabController : BaseController


    {


        private readonly IWebHostEnvironment _hostingEnvironment;

        public LabController(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment) : base(context)
        {

            _hostingEnvironment = hostingEnvironment;

        }

        [HttpGet]
        public override async Task<IActionResult> Index()
        {


            // جلب إحصائيات عامة

            var totalAppointments = await _context.Appointments.CountAsync();
            var totalAppointmentsToday = await _context.Appointments
    .Where(a => a.AppointmentDate.Date == DateTime.Now.Date)
    .CountAsync();

            // إنشاء نموذج الإحصائيات







            ViewBag.TotalAppointments = totalAppointments;
            ViewBag.TotalAppointmentsToday = totalAppointmentsToday;




            // تمرير النموذج إلى العرض
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];

            return View();

        }


        public async Task<IActionResult> AllLabTest()
        {
            var tests = await _context.LabTests.ToListAsync();
            return View(tests);
        }

        // GET: LabTest/CreateLabTest
        public IActionResult CreateLabTest()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLabTest(LabTest model)
        {
            if (ModelState.IsValid)
            {
                _context.Add(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم إضافة التحليل بنجاح.";
                return RedirectToAction(nameof(AllLabTest));
            }
            return View(model);
        }



        // GET: LabTest/Edit/5
        public async Task<IActionResult> EditLabTest(int id)
        {
            var test = await _context.LabTests.FindAsync(id);
            if (test == null)
                return NotFound();

            return View(test);
        }

        // POST: LabTest/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLabTest(int id, LabTest model)
        {
            if (id != model.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم تعديل التحليل بنجاح.";
                return RedirectToAction(nameof(AllLabTest));
            }

            return View(model);
        }

        // POST: LabTest/Delete/5
        [HttpPost]
        public async Task<IActionResult> DeleteLabTest(int id)
        {
            var test = await _context.LabTests.FindAsync(id);
            if (test == null)
                return NotFound();

            _context.LabTests.Remove(test);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(AllLabTest));
        }



        [Authorize(Roles = "LabDirector,LabStaff")]
        public IActionResult CreateLabInvoice()
        {
            var model = new CreateLabInvoiceViewModel
            {
                AvailableLabTests = _context.LabTests.ToList(), // ✅ نستخدم هذه الآن لعرض الكمية
                AvailableTests = _context.LabTests.ToList(),     // ✅ نُبقي على هذه لدعم أي استخدامات أخرى لديك
                Patients = _context.Patients.ToList()            // ✅ إذا كانت هناك قائمة مرضى في الصفحة
            };

            return View(model);
        }

        [Authorize(Roles = "LabDirector")]
        public IActionResult ManageStock()
        {
            var tests = _context.LabTests.ToList();
            return View(tests);
        }

        [HttpPost]
        [Authorize(Roles = "LabDirector")]
        public async Task<IActionResult> AddStock(int id, decimal quantity)
        {
            var test = await _context.LabTests.FindAsync(id);
            if (test == null)
                return NotFound();

            if (quantity > 0)
            {
                test.StockQuantity += quantity;
                _context.Update(test);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("ManageStock");
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "LabDirector,LabStaff")]
        public async Task<IActionResult> CreateLabInvoice(CreateLabInvoiceViewModel model)
        {
            if (ModelState.IsValid)
            {
                string? savedFileName = null;

                // حفظ صورة الهوية إن وجدت
                if (model.NationalIdImageFile != null && model.NationalIdImageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.NationalIdImageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.NationalIdImageFile.CopyToAsync(stream);
                    }

                    savedFileName = "/uploads/" + uniqueFileName;
                }

                // تحقق من توفر الكمية في المخزون أولاً
                foreach (var testId in model.SelectedTestIds)
                {
                    var labTest = await _context.LabTests.FindAsync(testId);
                    if (labTest == null)
                    {
                        ModelState.AddModelError("", $"التحليل برقم {testId} غير موجود.");
                        model.AvailableTests = _context.LabTests.ToList();
                        return View(model);
                    }

                    decimal requiredQty = labTest.UsagePerPatient != 0 ? labTest.UsagePerPatient : 1m;

                    if (labTest.StockQuantity < requiredQty)
                    {
                        ModelState.AddModelError("", $"الكمية المتوفرة للتحليل ({labTest.Name}) غير كافية.");
                        model.AvailableTests = _context.LabTests.ToList();
                        return View(model);
                    }
                }

                // إنشاء الفاتورة
                var invoice = new LabInvoice
                {
                    FullName = model.FullName,
                    Age = model.Age,
                    DoctorName = model.DoctorName,
                    NationalIdImage = savedFileName,
                    CreatedAt = DateTime.Now
                };

                _context.LabInvoices.Add(invoice);
                await _context.SaveChangesAsync(); // للحصول على ID

                // ربط الفحوصات وخصم الكميات
                foreach (var testId in model.SelectedTestIds)
                {
                    var labTest = await _context.LabTests.FindAsync(testId);
                    if (labTest != null)
                    {
                        decimal usedQty = labTest.UsagePerPatient != 0 ? labTest.UsagePerPatient : 1m;

                        // خصم الكمية من المخزن
                        labTest.StockQuantity -= usedQty;

                        _context.LabInvoiceTests.Add(new LabInvoiceTest
                        {
                            LabInvoiceId = invoice.Id,
                            LabTestId = testId,
                            Price = labTest.Price,
                            QuantityUsed = usedQty
                        });

                        _context.LabTests.Update(labTest); // تحديث الكمية
                    }
                }

                await _context.SaveChangesAsync();

                return RedirectToAction("ListLabInvoices");
            }

            model.AvailableTests = _context.LabTests.ToList();
            return View(model);
        }



        [Authorize(Roles = "LabDirector,LabStaff")]
        public IActionResult ListLabInvoices(string search)
        {
            var query = _context.LabInvoices
                .Include(i => i.LabInvoiceTests)
                    .ThenInclude(t => t.LabTest)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(i =>
                    i.FullName.Contains(search) ||
                    i.DoctorName.Contains(search));
            }

            var invoices = query.OrderByDescending(i => i.CreatedAt).ToList();
            return View(invoices);
        }



        [Authorize(Roles = "LabDirector,LabStaff")]
        public IActionResult ViewLabInvoiceDetails(int id)
        {
            var invoice = _context.LabInvoices
     .Include(i => i.LabInvoiceTests)
         .ThenInclude(t => t.LabTest) // ضروري لجلب الفحوصات المرتبطة
     .FirstOrDefault(i => i.Id == id);

            if (invoice == null)
                return NotFound();

            return View(invoice);
        }


        [HttpGet]
        public IActionResult EditLabInvoice(long id)
        {
            var invoice = _context.LabInvoices
                .Include(i => i.LabInvoiceTests)
                .ThenInclude(t => t.LabTest)
                .FirstOrDefault(i => i.Id == id);

            if (invoice == null) return NotFound();

            var testsWithResults = invoice.LabInvoiceTests.Select(t => new LabInvoiceTestViewModel
            {
                LabTestId = t.LabTestId,
                LabTestName = t.LabTest?.Name,
                MinValue = t.LabTest?.MinValue,
                MaxValue = t.LabTest?.MaxValue,
                ResultValue = t.ResultValue
            }).ToList();

            var viewModel = new CreateLabInvoiceViewModel
            {
                Id = (int)invoice.Id,
                FullName = invoice.FullName,
                Age = invoice.Age,
                DoctorName = invoice.DoctorName,
                NationalIdImage = invoice.NationalIdImage,
                SelectedTestIds = invoice.LabInvoiceTests.Select(t => (int)t.LabTestId).ToList(),
                AvailableLabTests = _context.LabTests.ToList(),
                TestsWithResults = testsWithResults
            };

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLabInvoice(CreateLabInvoiceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableLabTests = _context.LabTests.ToList();
                model.TestsWithResults ??= new List<LabInvoiceTestViewModel>();
                return View(model);
            }

            var invoice = await _context.LabInvoices
                .Include(i => i.LabInvoiceTests)
                .FirstOrDefaultAsync(i => i.Id == model.Id);

            if (invoice == null) return NotFound();

            invoice.FullName = model.FullName;
            invoice.Age = model.Age;
            invoice.DoctorName = model.DoctorName;

            // حفظ صورة الهوية لو رفعت ملف جديد (كود الحفظ هنا)

            var selectedTestIds = model.SelectedTestIds.ToHashSet();

            // حذف الفحوصات الغير مختارة
            var toRemove = invoice.LabInvoiceTests.Where(t => !selectedTestIds.Contains((int)t.LabTestId)).ToList();
            foreach (var rem in toRemove)
                invoice.LabInvoiceTests.Remove(rem);

            // إضافة الفحوصات الجديدة
            var existingIds = invoice.LabInvoiceTests.Select(t => (int)t.LabTestId).ToHashSet();
            var toAdd = selectedTestIds.Except(existingIds);
            foreach (var addId in toAdd)
            {
                var labTest = await _context.LabTests.FirstOrDefaultAsync(t => t.Id == addId);
                if (labTest != null)
                {
                    invoice.LabInvoiceTests.Add(new LabInvoiceTest
                    {
                        LabTestId = addId,
                        LabInvoiceId = invoice.Id,
                        Price = labTest.Price, // ← نسحب السعر من جدول LabTests
                        QuantityUsed = labTest.UsagePerPatient != 0 ? labTest.UsagePerPatient : 1m

                    });
                }
            }

            // تحديث نتائج الفحوصات
            if (model.TestsWithResults != null)
            {
                foreach (var testResult in model.TestsWithResults)
                {
                    var invoiceTest = invoice.LabInvoiceTests.FirstOrDefault(t => t.LabTestId == (long)testResult.LabTestId);

                    if (invoiceTest != null)
                        invoiceTest.ResultValue = testResult.ResultValue;
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("ListLabInvoices");
        }

        // دالة مساعده لحفظ الملف (يمكنك تعديلها حسب حاجتك)
        private async Task<string> SaveFileAsync(IFormFile file)
        {
            var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return "/uploads/" + fileName;  // المسار الذي سيُخزن في قاعدة البيانات
        }




        [Authorize(Roles = "LabDirector,LabStaff")]
        public async Task<IActionResult> DeleteLabInvoice(int id)
        {
            var invoice = _context.LabInvoices
                .Include(i => i.LabInvoiceTests)
                .FirstOrDefault(i => i.Id == id);

            if (invoice == null) return NotFound();

            _context.LabInvoiceTests.RemoveRange(invoice.LabInvoiceTests);
            _context.LabInvoices.Remove(invoice);
            await _context.SaveChangesAsync();

            return RedirectToAction("ListLabInvoices");
        }


        [Authorize(Roles = "LabDirector,LabStaff")]
        public IActionResult PrintLabInvoice(int id)
        {
            var invoice = _context.LabInvoices
                .Include(i => i.LabInvoiceTests)
                    .ThenInclude(t => t.LabTest)
                .FirstOrDefault(i => i.Id == id);

            if (invoice == null)
                return NotFound();

            // توليد QR code من رقم الفاتورة


            return View(invoice); // View خاص للطباعة
        }


        [HttpGet]
        public IActionResult LabAppointments()
        {
            var labDepartment = _context.Departments.FirstOrDefault(d => d.Name == "مختبر مارينا للتحليلات المرضية");

            if (labDepartment == null)
                return View(new List<Appointment>());

            var labAppointments = _context.Appointments
                .Include(a => a.Doctor)
                .Where(a => a.DepartmentId == labDepartment.Id)
                .OrderBy(a => a.AppointmentDate)
                .ToList();

            return View(labAppointments);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAppointment(int Id, DateTime AppointmentDate, TimeSpan AppointmentTime, AppointmentStatus Status, string Notes)
        {
            var appointment = await _context.Appointments.FindAsync(Id);
            if (appointment == null)
                return NotFound();

            appointment.AppointmentDate = AppointmentDate;
            appointment.AppointmentTime = AppointmentTime;
            appointment.Status = Status;
            appointment.Notes = Notes;
            appointment.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return RedirectToAction("LabAppointments");
        }


    }
}