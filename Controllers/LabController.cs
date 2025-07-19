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
using Microsoft.AspNetCore.Identity;


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

            var totalAppointments = await _context.Appointments.CountAsync();
            var totalAppointmentsToday = await _context.Appointments
    .Where(a => a.AppointmentDate.Date == DateTime.Now.Date)
    .CountAsync();
            ViewBag.TotalAppointments = totalAppointments;
            ViewBag.TotalAppointmentsToday = totalAppointmentsToday;
            // تمرير النموذج إلى العرض
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            return View();
        }


        public IActionResult AllLabTest(string search, int? categoryId)
        {
            var testsQuery = _context.LabTests
                .Include(t => t.LabInvoiceTests)
                .Include(t => t.LabTestCategory)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                testsQuery = testsQuery.Where(t => t.Name.Contains(search));
            }

            if (categoryId.HasValue)
            {
                testsQuery = testsQuery.Where(t => t.LabTestCategoryId == categoryId.Value);
            }

            var tests = testsQuery
                .Select(t => new LabTestUsageViewModel
                {
                    Id = t.Id,
                    Name = t.Name,
                    Unit = t.Unit,
                    Price = t.Price,
                    LabTestCategoryName = t.LabTestCategory != null ? t.LabTestCategory.Name : "",
                    UsagePerPatient = t.UsagePerPatient,

                    MinValue = t.MinValue,
                    MaxValue = t.MaxValue,
                    UsageCount = t.LabInvoiceTests.Count()
                })
                .ToList();

            ViewBag.Categories = _context.LabTestCategories.ToList();
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.Search = search;

            return View(tests);
        }

        // GET: LabTest/CreateLabTest
        public IActionResult CreateLabTest()
        {
            ViewBag.Categories = _context.LabTestCategories.ToList();
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
            ViewBag.Categories = _context.LabTestCategories.ToList();
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
            var labTests = _context.LabTests
                .Select(t => new LabTestWithStockViewModel
                {
                    Id = t.Id,
                    Name = t.Name,
                    Unit = t.Unit,
                    Price = t.Price,
                    UsagePerPatient = t.UsagePerPatient,
                    LabTestCategoryId = t.LabTestCategoryId,

                    StockQuantity =
                        (_context.StockEntries.Where(e => e.LabTestId == t.Id).Sum(e => (decimal?)e.QuantityAdded) ?? 0)
                      - (_context.LabInvoiceTests.Where(e => e.LabTestId == t.Id).Sum(e => (decimal?)e.QuantityUsed) ?? 0)
                })
                .ToList();

            var model = new CreateLabInvoiceViewModel
            {
                AvailableLabTests = labTests,
                AvailableTests = labTests,
                Patients = _context.Patients.ToList()
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



            return RedirectToAction("ManageStock");
        }


        /// 
        /// 
        private List<LabTestWithStockViewModel> GetLabTestsWithStock()
        {
            return _context.LabTests
                .Select(t => new LabTestWithStockViewModel
                {
                    Id = t.Id,
                    Name = t.Name,
                    Unit = t.Unit,
                    Price = t.Price,
                    UsagePerPatient = t.UsagePerPatient,
                    LabTestCategoryId = t.LabTestCategoryId,
                    StockQuantity =
                        (_context.StockEntries.Where(e => e.LabTestId == t.Id).Sum(e => (decimal?)e.QuantityAdded) ?? 0)
                      - (_context.LabInvoiceTests.Where(e => e.LabTestId == t.Id).Sum(e => (decimal?)e.QuantityUsed) ?? 0)
                })
                .ToList();
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "LabDirector,LabStaff")]
        public async Task<IActionResult> CreateLabInvoice(CreateLabInvoiceViewModel model)
        {
            if (ModelState.IsValid)
            {
                string? savedFileName = null;

                // ✅ حفظ صورة الهوية إذا تم رفعها
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

                // ✅ التحقق من توفر الكمية لكل تحليل
                foreach (var testId in model.SelectedTestIds)
                {
                    var labTest = await _context.LabTests.FindAsync(testId);
                    if (labTest == null)
                    {
                        ModelState.AddModelError("", $"التحليل برقم {testId} غير موجود.");
                        model.AvailableTests = GetLabTestsWithStock();

                        return View(model);
                    }

                    decimal requiredQty = labTest.UsagePerPatient != 0 ? labTest.UsagePerPatient : 1m;

                    decimal totalInStock = _context.StockEntries
                        .Where(e => e.LabTestId == labTest.Id)
                        .Sum(e => (decimal?)e.QuantityAdded) ?? 0;

                    decimal totalUsed = _context.LabInvoiceTests
                        .Where(e => e.LabTestId == labTest.Id)
                        .Sum(e => (decimal?)e.QuantityUsed) ?? 0;

                    decimal availableStock = totalInStock - totalUsed;

                    if (availableStock < requiredQty)
                    {
                        ModelState.AddModelError("", $"الكمية المتوفرة للتحليل ({labTest.Name}) غير كافية. الكمية المتاحة: {availableStock}");
                        model.AvailableTests = GetLabTestsWithStock();

                        return View(model);
                    }
                }

                // ✅ إنشاء الفاتورة
                var invoice = new LabInvoice
                {
                    FullName = model.FullName,
                    Age = model.Age,
                    DoctorName = model.DoctorName,
                    NationalIdImagePath = savedFileName,
                    CreatedAt = DateTime.Now,
                    IsPaid = false,
                    Status = "Pending",
                    PatientId = model.PatientId
                };

                _context.LabInvoices.Add(invoice);
                await _context.SaveChangesAsync(); // لحفظ الفاتورة أولًا قبل إضافة التحاليل

                // ✅ إضافة التحاليل إلى الفاتورة
                foreach (var testId in model.SelectedTestIds)
                {
                    var labTest = await _context.LabTests.FindAsync(testId);
                    if (labTest == null) continue;

                    decimal quantityUsed = labTest.UsagePerPatient != 0 ? labTest.UsagePerPatient : 1m;

                    var invoiceTest = new LabInvoiceTest
                    {
                        LabInvoiceId = invoice.Id,
                        LabTestId = labTest.Id,
                        Price = labTest.Price,
                        QuantityUsed = quantityUsed
                    };

                    _context.LabInvoiceTests.Add(invoiceTest);
                }

                await _context.SaveChangesAsync();

                return RedirectToAction("ListLabInvoices");
            }

            // إعادة البيانات في حالة الخطأ
            model.AvailableTests = GetLabTestsWithStock();

            model.Patients = _context.Patients.ToList();
            return View(model);
        }





        [Authorize(Roles = "LabDirector,LabStaff")]
        public IActionResult ListLabInvoices(string search, string statusFilter)
        {
            var invoices = _context.LabInvoices
    .Include(i => i.LabInvoiceTests)
        .ThenInclude(t => t.LabTest)
    .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                invoices = invoices.Where(i => i.FullName.Contains(search) || i.DoctorName.Contains(search));
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                invoices = invoices.Where(i => i.Status == statusFilter);
            }

            var model = invoices.ToList();

            return View(model);
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
                AvailableLabTests = GetLabTestsWithStock(),
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
                model.AvailableLabTests = GetLabTestsWithStock();
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


        [Authorize(Roles = "LabDirector")]
        public IActionResult LabTestCategories()
        {
            var categories = _context.LabTestCategories.ToList();
            return View(categories);
        }

        [HttpGet]
        [Authorize(Roles = "LabDirector")]
        public IActionResult CreateCategory()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "LabDirector")]
        public IActionResult CreateCategory(LabTestCategory category)
        {
            if (ModelState.IsValid)
            {
                _context.LabTestCategories.Add(category);
                _context.SaveChanges();
                return RedirectToAction("LabTestCategories");
            }

            return View(category);
        }

        [HttpGet]
        [Authorize(Roles = "LabDirector")]
        public IActionResult EditCategory(int id)
        {
            var category = _context.LabTestCategories.Find(id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "LabDirector")]
        public IActionResult EditCategory(LabTestCategory category)
        {
            if (ModelState.IsValid)
            {
                _context.LabTestCategories.Update(category);
                _context.SaveChanges();
                return RedirectToAction("LabTestCategories");
            }

            return View(category);
        }

        [Authorize(Roles = "LabDirector")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLabTestCategory(int id)
        {
            var category = await _context.LabTestCategories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            // تأكد من عدم وجود تحاليل مرتبطة
            var hasTests = _context.LabTests.Any(t => t.LabTestCategoryId == id);
            if (hasTests)
            {
                TempData["Error"] = "لا يمكن حذف التصنيف لأنه مرتبط بتحاليل.";
                return RedirectToAction("LabTestCategories");
            }

            _context.LabTestCategories.Remove(category);
            await _context.SaveChangesAsync();
            TempData["Success"] = "تم حذف التصنيف بنجاح.";
            return RedirectToAction("LabTestCategories");
        }


        // StockEntry CRUD

        public IActionResult ListStockEntry()
        {
            var entries = _context.StockEntries
                .Include(e => e.LabTest)
                .Include(e => e.CreatedByUser)
                .OrderByDescending(e => e.CreatedAt)
                .ToList();

            return View(entries);
        }

        public IActionResult CreateStockEntry()
        {
            // نحضّر قائمة التحاليل لعرضها في القائمة المنسدلة
            ViewBag.LabTests = _context.LabTests.ToList();

            // نعيد كائن فارغ لإنشاء إدخال جديد
            var model = new StockEntry();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize] // تأكد أن المستخدم مسجل دخول
        public async Task<IActionResult> CreateStockEntry(StockEntry entry)
        {
            if (ModelState.IsValid)
            {
                entry.CreatedAt = DateTime.Now;

                // جلب معرف المستخدم من الكلايمز (والذي يجب أن يكون Id من نوع long)
                var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out long userId))
                {
                    TempData["Error"] = "يجب تسجيل الدخول أولاً أو تعذر التعرف على المستخدم.";
                    return RedirectToAction("Login", "Home");
                }

                entry.CreatedByUserId = userId;

                _context.StockEntries.Add(entry);
                await _context.SaveChangesAsync();

                TempData["Success"] = "تمت إضافة الكمية بنجاح.";
                return RedirectToAction("ListStockEntry");
            }

            // إعادة تحميل قائمة التحاليل في حالة وجود أخطاء لعرضها في الفورم
            ViewBag.LabTests = _context.LabTests.ToList();

            return View(entry);
        }



        //edit 
        public async Task<IActionResult> EditStockEntry(int id)
        {
            var entry = await _context.StockEntries
        .Include(e => e.LabTest)
        .FirstOrDefaultAsync(e => e.Id == id);

            if (entry == null)
            {
                return NotFound();
            }

            return View(entry);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStockEntry(int id, [Bind("Id,QuantityAdded,Notes")] StockEntry updatedEntry)
        {
            if (!ModelState.IsValid)
            {
                return View(updatedEntry);
            }

            var existingEntry = await _context.StockEntries.FindAsync(id);
            if (existingEntry == null)
            {
                return NotFound();
            }

            existingEntry.QuantityAdded = updatedEntry.QuantityAdded;
            existingEntry.Notes = updatedEntry.Notes;
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم تعديل الإدخال بنجاح.";
            return RedirectToAction("ListStockEntry");
        }







        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteStockEntry(int id)
        {
            var entry = await _context.StockEntries.FindAsync(id);
            if (entry == null)
            {
                TempData["Error"] = "لم يتم العثور على السجل الذي تريد حذفه.";
                return RedirectToAction("ListStockEntry");
            }

            _context.StockEntries.Remove(entry);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم حذف السجل بنجاح.";
            return RedirectToAction("ListStockEntry");
        }



        [HttpGet]
        [Authorize(Roles = "LabDirector,LabStaff")]
        public IActionResult CreateStockInvoice()
        {
            var model = new CreateStockInvoiceViewModel
            {
                InvoiceNumber = $"INV-{DateTime.Now.Ticks}", // توليد رقم عشوائي
                Items = new List<StockInvoiceItemViewModel>
        {
            new StockInvoiceItemViewModel() // صنف واحد افتراضي
        },
                AvailableLabTests = _context.LabTests.ToList()
            };

            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "LabDirector,LabStaff")]
        public async Task<IActionResult> CreateStockInvoice(CreateStockInvoiceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableLabTests = _context.LabTests.ToList();
                return View(model);
            }

            // جلب بيانات المستخدم الحالي
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "يجب تسجيل الدخول أولاً.";
                return RedirectToAction("Login", "Home");
            }

            // إنشاء الفاتورة
            var invoice = new StockInvoice
            {
                InvoiceNumber = model.InvoiceNumber,
                InvoiceDate = model.InvoiceDate,
                CreatedByUserId = long.Parse(userId),
                CreatedAt = DateTime.Now
            };

            _context.StockInvoices.Add(invoice);
            await _context.SaveChangesAsync(); // لحفظ الـ ID

            // إضافة التفاصيل وربطها بالفاتورة
            foreach (var item in model.Items)
            {
                var invoiceItem = new StockInvoiceItem
                {
                    StockInvoiceId = invoice.Id,
                    LabTestId = item.LabTestId,
                    QuantityAdded = item.QuantityAdded,
                    Notes = item.Notes,
                    ExpiryDate = item.ExpiryDate
                };

                _context.StockInvoiceItems.Add(invoiceItem);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "تم حفظ الفاتورة وإضافة الكميات بنجاح.";
            return RedirectToAction("Index"); // أو أي صفحة مناسبة
        }





    }
}