using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MarinaRegSystem.Data;
using MarinaRegSystem.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Net.Http;



namespace MarinaRegSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : BaseController
    {

        private const string WhatsAppApiUrl = "http://91.227.40.38/api/create-message";
        private const string WhatsAppAppKey = "80bfe418-f930-45de-96d4-18caab17a2ea";
        private const string WhatsAppAuthKey = "In31s77aNxvFxvR9CvexJnM1wcWAXpJ3ltg8d8JfEuTmxTFnpG";
        public AdminController(ApplicationDbContext context) : base(context) { }

        [HttpGet]
        public override async Task<IActionResult> Index()
        {


            // جلب إحصائيات عامة
            var totalDepartments = await _context.Departments.CountAsync();
            var totalDoctors = await _context.Doctors.CountAsync();
            var totalAppointments = await _context.Appointments.CountAsync();
            var totalAppointmentsToday = await _context.Appointments
    .Where(a => a.AppointmentDate.Date == DateTime.Now.Date)
    .CountAsync();
            var totalPatients = await _context.Patients.CountAsync();
            var totalUsers = await _context.Users.CountAsync();
            var totalSubDepartments = await _context.SubDepartments.CountAsync();
            var totalDoctorSchedules = await _context.DoctorSchedules.CountAsync();
            // إنشاء نموذج الإحصائيات




            var Pending = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Pending);
            var Confirmed = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Confirmed);
            var Rejected = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Rejected);
            var Completed = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Completed);
            var Cancelled = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Cancelled);


            ViewBag.TotalAppointments = totalAppointments;
            ViewBag.TotalAppointmentsToday = totalAppointmentsToday;
            ViewBag.TotalDepartments = totalDepartments;
            ViewBag.TotalDoctors = totalDoctors;
            ViewBag.TotalPatients = totalPatients;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalSubDepartments = totalSubDepartments;
            ViewBag.TotalDoctorSchedules = totalDoctorSchedules;
            ViewBag.PendingAppointments = Pending;
            ViewBag.ConfirmedAppointments = Confirmed;
            ViewBag.RejectedAppointments = Rejected;
            ViewBag.CompletedAppointments = Completed;
            ViewBag.CancelledAppointments = Cancelled;


            // تمرير النموذج إلى العرض
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];

            return View();

        }

        public IActionResult Departments()
        {
            var departments = _context.Departments
                .Include(d => d.Doctors)
                .Include(d => d.Appointments)
                .ToList();
            return View(departments);
        }

        [HttpGet]
        public IActionResult AddDepartment()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddDepartment(Department department)
        {
            if (ModelState.IsValid)
            {
                _context.Departments.Add(department);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Departments));
            }
            return View(department);
        }



        public async Task<IActionResult> EditDepartment(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments.FindAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            return View(department);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDepartment(int id, Department department)
        {
            if (id != department.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(department);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "تم تعديل القسم بنجاح";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DepartmentExists(department.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Departments));
            }
            return View(department);
        }

        private bool DepartmentExists(int id)
        {
            return _context.Departments.Any(e => e.Id == id);
        }

        // GET: Departments/Delete/5
        public async Task<IActionResult> DeleteDepartment(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments
                .Include(d => d.Doctors)
                .Include(d => d.Appointments)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (department == null)
            {
                return NotFound();
            }

            // إذا القسم له أطباء أو حجوزات، لا تسمح بالحذف
            if ((department.Doctors?.Count > 0) || (department.Appointments?.Count > 0))
            {
                TempData["ErrorMessage"] = "لا يمكن حذف القسم لأنه يحتوي على أطباء أو حجوزات.";
                return RedirectToAction(nameof(Departments));
            }

            // حذف القسم
            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم حذف القسم بنجاح";
            return RedirectToAction(nameof(Departments));
        }



        public IActionResult Doctors(DoctorFilterViewModel filter)
        {
            var query = _context.Doctors
        .Include(d => d.Department)
        .Include(d => d.SubDepartment)
        .Include(d => d.Appointments)
        .AsQueryable();

            // البحث
            if (!string.IsNullOrEmpty(filter.Name))
                query = query.Where(d => d.Name.Contains(filter.Name));

            if (!string.IsNullOrEmpty(filter.Speciality))
                query = query.Where(d => d.Speciality.Contains(filter.Speciality));

            // الفلاتر
            if (filter.DepartmentId.HasValue)
                query = query.Where(d => d.DepartmentId == filter.DepartmentId);

            if (filter.SubDepartmentId.HasValue)
                query = query.Where(d => d.SubDepartmentId == filter.SubDepartmentId);

            if (filter.Shift.HasValue)
                query = query.Where(d => d.Shift == filter.Shift.Value);

            // تعبئة النتائج
            filter.Results = query.ToList();

            // تعبئة القوائم المنسدلة
            filter.Departments = _context.Departments
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.Name
                }).ToList();

            filter.SubDepartments = _context.SubDepartments
                .Select(sd => new SelectListItem
                {
                    Value = sd.Id.ToString(),
                    Text = sd.Name
                }).ToList();

            return View(filter);
        }

        [HttpGet]
        public async Task<IActionResult> AddDoctor()
        {
            var departments = await _context.Departments.ToListAsync();
            ViewBag.Departments = new SelectList(departments, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddDoctor(Doctor doctor, IFormFile ImageFile)
        {
            if (ModelState.IsValid)
            {
                // حفظ الصورة
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/doctors");

                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    var filePath = Path.Combine(uploadPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    doctor.ImageUrl = "/uploads/doctors/" + fileName;
                }

                // التأكد من حفظ القسم الفرعي إن وجد
                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Doctors));
            }

            // في حالة فشل التحقق، نعيد تعبئة القائمة
            var departments = await _context.Departments.ToListAsync();
            ViewBag.Departments = new SelectList(departments, "Id", "Name");

            return View(doctor);
        }

        [HttpGet]
        public IActionResult GetSubDepartmentsByDepartment(int id)
        {
            var subs = _context.SubDepartments
                .Where(sd => sd.DepartmentId == id)
                .Select(sd => new { id = sd.Id, name = sd.Name })
                .ToList();

            return Json(subs);
        }


        [HttpGet]
        public async Task<IActionResult> EditDoctor(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
                return NotFound();

            ViewBag.Departments = new SelectList(_context.Departments, "Id", "Name", doctor.DepartmentId);

            var subDepartments = _context.SubDepartments
                .Where(sd => sd.DepartmentId == doctor.DepartmentId)
                .Select(sd => new SelectListItem { Value = sd.Id.ToString(), Text = sd.Name })
                .ToList();

            ViewBag.SubDepartments = subDepartments;

            return View(doctor);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDoctor(int id, Doctor doctor, IFormFile ImageFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Departments = new SelectList(_context.Departments, "Id", "Name", doctor.DepartmentId);
                ViewBag.SubDepartments = _context.SubDepartments
                    .Where(sd => sd.DepartmentId == doctor.DepartmentId)
                    .Select(sd => new SelectListItem { Value = sd.Id.ToString(), Text = sd.Name })
                    .ToList();
                return View(doctor);
            }

            var existing = await _context.Doctors.FindAsync(doctor.Id);
            if (existing == null)
                return NotFound();

            existing.Name = doctor.Name;
            existing.DepartmentId = doctor.DepartmentId;
            existing.SubDepartmentId = doctor.SubDepartmentId;
            existing.Speciality = doctor.Speciality;
            existing.Experience = doctor.Experience;
            existing.Bio = doctor.Bio;
            existing.Rating = doctor.Rating;
            existing.Shift = doctor.Shift;

            // تحديث الصورة إن تم رفعها
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/doctors", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                existing.ImageUrl = "/uploads/doctors/" + fileName;
            }

            existing.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Doctors));
        }

        [HttpGet]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
            {
                return NotFound();
            }

            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Doctors));
        }


        public IActionResult DoctorSchedules()
        {
            var schedules = _context.DoctorSchedules
                                .Include(d => d.Doctor)
                                .OrderBy(d => d.DoctorId)
                                .ThenBy(d => d.DayOfWeek)
                                .ToList();

            return View(schedules);
        }

        [HttpGet]
        public IActionResult AddDoctorSchedule()
        {
            ViewBag.Doctors = new SelectList(_context.Doctors.ToList(), "Id", "Name");
            return View();
        }

        [HttpPost]
        public IActionResult AddDoctorSchedule(DoctorSchedule model)
        {
            if (ModelState.IsValid)
            {
                model.Uid = Guid.NewGuid().ToString();
                model.CreatedAt = DateTime.Now;

                _context.DoctorSchedules.Add(model);
                _context.SaveChanges();

                TempData["Success"] = "تم حفظ الجدول بنجاح";
                return RedirectToAction("AddDoctorSchedule");
            }

            // إعادة تحميل قائمة الأطباء في حالة وجود خطأ
            ViewBag.Doctors = new SelectList(_context.Doctors.ToList(), "Id", "Name", model.DoctorId);
            return View(model);
        }

        // POST: /Admin/DeleteDoctorSchedule/{id}
        [HttpPost]
        public IActionResult DeleteDoctorSchedule(int id)
        {
            var schedule = _context.DoctorSchedules.Find(id);
            if (schedule == null)
                return Json(new { success = false, message = "الجدول غير موجود" });

            try
            {
                _context.DoctorSchedules.Remove(schedule);
                _context.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // يمكن تسجيل الخطأ في اللوج هنا
                return Json(new { success = false, message = "حدث خطأ أثناء الحذف." });
            }
        }
        // جلب صفحة التعديل مع ملأ البيانات
        [HttpGet]
        public IActionResult EditDoctorSchedule(int id)
        {
            var schedule = _context.DoctorSchedules.Find(id);
            if (schedule == null)
            {
                return NotFound();
            }

            ViewBag.Doctors = new SelectList(_context.Doctors.ToList(), "Id", "Name", schedule.DoctorId);
            return View(schedule);
        }

        // حفظ التعديل
        [HttpPost]
        public IActionResult EditDoctorSchedule(DoctorSchedule model)
        {
            if (ModelState.IsValid)
            {
                var schedule = _context.DoctorSchedules.Find(model.Id);
                if (schedule == null)
                    return NotFound();

                schedule.DoctorId = model.DoctorId;
                schedule.DayOfWeek = model.DayOfWeek;
                schedule.StartTime = model.StartTime;
                schedule.EndTime = model.EndTime;
                schedule.AppointmentDuration = model.AppointmentDuration;
                schedule.BreakDuration = model.BreakDuration;
                schedule.MaxAppointmentsPerDay = model.MaxAppointmentsPerDay;
                schedule.IsActive = model.IsActive;

                _context.SaveChanges();

                TempData["Success"] = "تم تعديل الجدول بنجاح";
                return RedirectToAction(nameof(DoctorSchedules));
            }

            ViewBag.Doctors = new SelectList(_context.Doctors.ToList(), "Id", "Name", model.DoctorId);
            return View(model);
        }



        [HttpGet]
        public IActionResult AddSchedule()
        {
            ViewBag.Doctors = new SelectList(_context.Doctors, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddSchedule(DoctorSchedule schedule)
        {
            if (ModelState.IsValid)
            {
                _context.DoctorSchedules.Add(schedule);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(DoctorSchedules));
            }
            ViewBag.Doctors = new SelectList(_context.Doctors, "Id", "Name");
            return View(schedule);
        }


        [HttpGet]
        public IActionResult Appointments(AppointmentFilterViewModel filter)
        {
            var query = _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Department)
                .Include(a => a.User)
                .Include(a => a.Patient)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.AppointmentTime)
                .AsQueryable();

            // ✅ تطبيق الفلاتر مثل السابق
            if (!string.IsNullOrEmpty(filter.PatientName))
                query = query.Where(a => a.PatientName.Contains(filter.PatientName));
            if (!string.IsNullOrEmpty(filter.Username))
                query = query.Where(a => a.User.Username.Contains(filter.Username));
            if (!string.IsNullOrEmpty(filter.DoctorName))
                query = query.Where(a => a.Doctor.Name.Contains(filter.DoctorName));
            if (filter.DepartmentId.HasValue)
                query = query.Where(a => a.DepartmentId == filter.DepartmentId.Value);
            if (filter.FromDate.HasValue)
                query = query.Where(a => a.AppointmentDate.Date >= filter.FromDate.Value.Date);

            if (filter.ToDate.HasValue)
                query = query.Where(a => a.AppointmentDate.Date <= filter.ToDate.Value.Date);
            if (filter.Shift.HasValue)
                query = query.Where(a => a.Shift == (ShiftType)filter.Shift.Value);
            if (filter.Status.HasValue)
                query = query.Where(a => a.Status == filter.Status.Value);

            var results = query
                .Select(a => new AppointmentViewModel
                {
                    Appointment = a,
                    Patient = _context.Patients.FirstOrDefault(p => p.UserId == a.UserId)
                }).ToList();

            // ✅ إذا كان طلب التصدير
            if (Request.Query["export"] == "excel")
            {
                var excelBytes = GenerateExcel(results);
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Appointments.xlsx");
            }

            // تعبئة القائمة المنسدلة
            filter.Results = results;
            filter.Departments = _context.Departments.Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.Name
            }).ToList();

            return View(filter);
        }

        private byte[] GenerateExcel(List<AppointmentViewModel> data)
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Appointments");

            // العناوين
            worksheet.Cell(1, 1).Value = "اسم المريض";
            worksheet.Cell(1, 2).Value = "العمر";
            worksheet.Cell(1, 3).Value = "الجنس";
            worksheet.Cell(1, 4).Value = "رقم الهاتف";

            worksheet.Cell(1, 5).Value = "القسم";
            worksheet.Cell(1, 6).Value = "القسم الفرعي";
            worksheet.Cell(1, 7).Value = "الطبيب";
            worksheet.Cell(1, 8).Value = "تاريخ الموعد";
            worksheet.Cell(1, 9).Value = "وقت الموعد";
            worksheet.Cell(1, 10).Value = "الشفت";
            worksheet.Cell(1, 11).Value = "الحالة";
            worksheet.Cell(1, 12).Value = "السعر";
            worksheet.Cell(1, 13).Value = "سبب الإلغاء";

            // البيانات
            for (int i = 0; i < data.Count; i++)
            {
                var a = data[i].Appointment;
                worksheet.Cell(i + 2, 1).Value = a.PatientName;
                worksheet.Cell(i + 2, 2).Value = a.Patient?.Age;
                worksheet.Cell(i + 2, 3).Value = a.Patient?.Gender;
                worksheet.Cell(i + 2, 4).Value = a.User?.PhoneNumber;
                worksheet.Cell(i + 2, 5).Value = a.Department?.Name;
                worksheet.Cell(i + 2, 6).Value = a.SubDepartment?.Name;
                worksheet.Cell(i + 2, 7).Value = a.Doctor?.Name;
                worksheet.Cell(i + 2, 8).Value = a.AppointmentDate.ToString("yyyy-MM-dd");
                worksheet.Cell(i + 2, 9).Value = a.AppointmentTime.ToString(@"hh\:mm");
                worksheet.Cell(i + 2, 10).Value = a.Shift.ToString();
                worksheet.Cell(i + 2, 11).Value = a.Status.ToString();
                worksheet.Cell(i + 2, 12).Value = a.Price;
                worksheet.Cell(i + 2, 13).Value = a.CancelReason;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }



        // GET: تعديل حجز
        [HttpGet]
        public IActionResult EditAppointment(int id)
        {
            var appointment = _context.Appointments.Find(id);
            if (appointment == null)
            {
                return NotFound();
            }

            // تجهيز بيانات للقوائم المنسدلة
            ViewBag.Doctors = new SelectList(_context.Doctors, "Id", "Name", appointment.DoctorId);
            ViewBag.Departments = new SelectList(_context.Departments, "Id", "Name", appointment.DepartmentId);
            ViewBag.Users = new SelectList(_context.Users, "Id", "UserName", appointment.UserId);
            ViewBag.Patients = new SelectList(_context.Patients, "UserId", "FirstName", appointment.UserId);

            return View(appointment);
        }

        // POST: تعديل حجز
        [HttpPost]
        public async Task<IActionResult> EditAppointment(Appointment appointment)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Doctors = new SelectList(_context.Doctors, "Id", "Name", appointment.DoctorId);
                ViewBag.Departments = new SelectList(_context.Departments, "Id", "Name", appointment.DepartmentId);
                return View(appointment);
            }

            var existing = await _context.Appointments.FindAsync(appointment.Id);
            if (existing == null) return NotFound();

            // تحديث البيانات
            existing.PatientName = appointment.PatientName;
            existing.UserId = appointment.UserId;
            existing.DepartmentId = appointment.DepartmentId;
            existing.DoctorId = appointment.DoctorId;
            existing.AppointmentDate = appointment.AppointmentDate;
            existing.AppointmentTime = appointment.AppointmentTime;
            existing.Status = appointment.Status;
            existing.Notes = appointment.Notes;
            existing.RejectionReason = appointment.RejectionReason;
            existing.Price = appointment.Price;
            existing.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            // جلب رقم المريض
            var user = await _context.cUsers.FindAsync(existing.UserId);
            if (user != null && !string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                string msg = "";

                if (existing.Status == AppointmentStatus.Confirmed)
                {
                    var doctor = await _context.Doctors.FindAsync(existing.DoctorId);
                    msg =
                        "✅ تم تأكيد موعدك في مستشفى مارينا الأهلي\n\n" +
                        $"🧑‍⚕️ الطبيب: {doctor?.Name ?? "غير محدد"}\n" +
                        $"📅 التاريخ: {existing.AppointmentDate:yyyy-MM-dd}\n" +
                        $"⏰ الوقت: {existing.AppointmentTime}\n" +
                         $"📝 الملاحظات: {existing.Notes}\n";
                }
                if (existing.Status == AppointmentStatus.Rejected)

                {
                    msg =
                        "❌ نأسف، تم رفض الحجز الخاص بك.\n\n" +
                        $"📄 السبب: {existing.RejectionReason ?? "غير مذكور"}";
                }

                if (!string.IsNullOrWhiteSpace(msg))
                    await SendMsgViaWhatsAppAsync(user.PhoneNumber, msg);
            }

            return RedirectToAction(nameof(Appointments));
        }

        // حذف الحجز
        public IActionResult DeleteAppointment(int id)
        {
            var appointment = _context.Appointments.Find(id);
            if (appointment == null)
                return NotFound();

            _context.Appointments.Remove(appointment);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "تم حذف الموعد بنجاح";
            return RedirectToAction(nameof(Appointments));
        }



        [HttpGet]
        public IActionResult AllPatients()
        {

            var patients = _context.Patients
      .Include(p => p.User) // لو عندك علاقة مع جدول المستخدمين
      .OrderBy(p => p.FirstName)
      .ToList();

            return View(patients);
        }

        [HttpGet]
        public async Task<IActionResult> Profile(int id)
        {
            var patient = await _context.Patients
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.Doctor)
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.Department)
                .Include(p => p.LabInvoices) // ✅ لجلب فواتير المختبر
                    .ThenInclude(i => i.LabInvoiceTests) // لجلب تفاصيل الفاتورة
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }
        public IActionResult SubDepartments(SubDepartmentFilterViewModel filter)
        {
            var query = _context.SubDepartments
        .Include(sd => sd.Department)
        .Include(sd => sd.Doctors)

        .AsQueryable();

            // 🔍 بحث بالاسم
            if (!string.IsNullOrEmpty(filter.SubDepartmentName))
            {
                query = query.Where(sd => sd.Name.Contains(filter.SubDepartmentName));
            }

            // ✅ فلترة بالقسم الرئيسي
            if (filter.DepartmentId.HasValue)
            {
                query = query.Where(sd => sd.DepartmentId == filter.DepartmentId.Value);
            }

            filter.Results = query.ToList();

            // تعبئة القائمة المنسدلة للأقسام
            filter.Departments = _context.Departments
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.Name
                }).ToList();

            return View(filter);
        }


        public IActionResult CreateSubDepartment()
        {
            ViewBag.Departments = new SelectList(_context.Departments, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSubDepartment(SubDepartment subDepartment)
        {
            if (ModelState.IsValid)
            {
                _context.SubDepartments.Add(subDepartment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم إضافة القسم الفرعي بنجاح";
                return RedirectToAction(nameof(SubDepartments));
            }

            ViewBag.Departments = new SelectList(_context.Departments, "Id", "Name", subDepartment.DepartmentId);
            return View(subDepartment);
        }

        public IActionResult EditSubDepartment(int id)
        {
            var subDept = _context.SubDepartments.Find(id);
            if (subDept == null) return NotFound();

            ViewBag.Departments = new SelectList(_context.Departments, "Id", "Name", subDept.DepartmentId);
            return View(subDept);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditSubDepartment(int id, SubDepartment subDepartment)
        {
            if (ModelState.IsValid)
            {
                _context.SubDepartments.Update(subDepartment);
                _context.SaveChanges();
                return RedirectToAction(nameof(SubDepartments));
            }

            ViewBag.Departments = new SelectList(_context.Departments, "Id", "Name", subDepartment.DepartmentId);
            return View(subDepartment);
        }

        // Delete SubDepartment
        public IActionResult DeleteSubDepartment(int id)
        {
            var subDept = _context.SubDepartments.Find(id);
            if (subDept == null) return NotFound();

            _context.SubDepartments.Remove(subDept);
            _context.SaveChanges();
            return RedirectToAction(nameof(SubDepartments));
        }


        [Authorize(Roles = "Admin")]
        public IActionResult CreatePatient()
        {
            return View();
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePatient(Patient patient)
        {
            if (ModelState.IsValid)
            {
                // حساب العمر تلقائيًا إن لم يتم إدخاله
                if (!patient.Age.HasValue)
                {
                    var today = DateTime.Today;
                    patient.Age = today.Year - patient.DateOfBirth.Year;
                    if (patient.DateOfBirth > today.AddYears(-patient.Age.Value))
                        patient.Age--;
                }

                // توليد رقم مريض فريد (مثل: PT00023)
                var lastPatient = _context.Patients
                    .OrderByDescending(p => p.Id)
                    .FirstOrDefault();

                long nextNumber = (lastPatient?.Id ?? 0) + 1;
                patient.PatientNumber = $"PT{nextNumber.ToString("D5")}";

                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();

                TempData["Success"] = "تمت إضافة المريض بنجاح";
                return RedirectToAction("AllPatients");
            }

            TempData["Error"] = "حدث خطأ أثناء حفظ البيانات";
            return View(patient);
        }

        [HttpGet]
        public IActionResult EditPatient(long id)
        {
            var patient = _context.Patients.Find(id);
            if (patient == null)
            {
                return NotFound();
            }
            return View(patient);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPatient(long id, Patient updatedPatient)
        {
            if (id != updatedPatient.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var patient = await _context.Patients.FindAsync(id);
                    if (patient == null)
                        return NotFound();

                    // تحديث الحقول
                    _context.Entry(patient).CurrentValues.SetValues(updatedPatient);

                    // تحديث العمر إذا لم يُدخل يدويًا
                    if (!updatedPatient.Age.HasValue)
                    {
                        var today = DateTime.Today;
                        patient.Age = today.Year - updatedPatient.DateOfBirth.Year;
                        if (updatedPatient.DateOfBirth > today.AddYears(-patient.Age.Value))
                            patient.Age--;
                    }

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "تم تحديث بيانات المريض بنجاح";
                    return RedirectToAction("AllPatients");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "حدث خطأ أثناء التحديث: " + ex.Message);
                }
            }

            return View(updatedPatient);
        }



        private async Task SendMsgViaWhatsAppAsync(string rawPhone, string message)
        {
            string receiver = NormalizePhone(rawPhone);

            using var client = new HttpClient();
            using var form = new MultipartFormDataContent
    {
        { new StringContent(WhatsAppAppKey),  "appkey"  },
        { new StringContent(WhatsAppAuthKey), "authkey" },
        { new StringContent(receiver),        "to"      },
        { new StringContent(message),         "message" }
    };

            await client.PostAsync(WhatsAppApiUrl, form);
        }

        /// <summary>
        /// يحوِّل ‎0771 234 5678 → 6947712345678 (مثال)
        /// عدّل حسب احتياجك الفعلي.
        /// </summary>
        private string NormalizePhone(string phone)
        {
            // أزل أي محارف غير أرقام
            var digits = new string(phone.Where(char.IsDigit).ToArray());

            // إذا كان يبدأ بـ "0" نحذفها ثم نضيف "964"
            if (digits.StartsWith("0"))
                digits = "964" + digits.Substring(1);
            // نحول ‎964… إلى ‎694… حسب المثال


            return digits;
        }



        public async Task<IActionResult> DoctorSchedulesBySubDepartments()
        {
            var subDepartments = await _context.SubDepartments
                .Include(sd => sd.Department)
                .ToListAsync();

            var schedules = await _context.DoctorSchedules
                .Include(s => s.Doctor)
                .ToListAsync();

            var viewModel = new DoctorScheduleTabsViewModel
            {
                SubDepartments = subDepartments,
                Schedules = schedules
            };

            return View(viewModel);
        }



        public async Task<IActionResult> DoctorScheduleCalendar()
        {
            var subDepartments = await _context.SubDepartments.ToListAsync();

            var schedules = await _context.DoctorSchedules
                .Include(ds => ds.Doctor)
                .Include(ds => ds.Doctor.SubDepartment)
                .ToListAsync();

            var result = new Dictionary<int, List<DoctorScheduleEvent>>();

            foreach (var subDept in subDepartments)
            {
                var events = schedules
                    .Where(s => s.Doctor.SubDepartmentId == subDept.Id)
                    .Select(s => new DoctorScheduleEvent
                    {
                        Title = $"{s.Doctor.Name}",
                        Start = GetNextDateForDay(s.DayOfWeek).Add(s.StartTime).ToString("s"),
                        End = GetNextDateForDay(s.DayOfWeek).Add(s.EndTime).ToString("s")
                    }).ToList();

                result[subDept.Id] = events;
            }

            var model = new DoctorScheduleCalendarViewModel
            {
                SubDepartments = subDepartments,
                EventsBySubDepartment = result
            };

            return View(model);
        }

        private DateTime GetNextDateForDay(DayOfWeek day)
        {
            var today = DateTime.Today;
            int daysUntil = ((int)day - (int)today.DayOfWeek + 7) % 7;
            return today.AddDays(daysUntil);
        }








    }

}
