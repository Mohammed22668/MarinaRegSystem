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
using MarinaRegSystem.Filters;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;

namespace MarinaRegSystem.Controllers
{
    [Authorize(Roles = "Patient")]
    public class PatientController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public PatientController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> AddSchedule()
        {
            // التحقق من أن المستخدم مسجل دخول
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long userId))
            {
                return RedirectToAction("Login", "Home"); // تسجيل الدخول في HomeController
            }

            // جلب بيانات المريض المرتبطة بالمستخدم
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
            if (patient == null)
            {
                TempData["Error"] = "يرجى إدخال بيانات المريض أولاً";
                return RedirectToAction("Create", "Patient"); // تأكد من وجود هذا الإجراء
            }

            // تحميل القوائم المنسدلة للحجز
            ViewBag.Departments = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name");

            ViewBag.Doctors = new SelectList(
                await _context.Doctors.Where(d => d.Status == true).ToListAsync(),
                "Id",
                "Name"
            );

            // إرسال بيانات المريض للعرض في الصفحة
            ViewBag.Patient = patient;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddSchedule(Appointment appointment, IFormFile diagnosisFile)
        {
            // جلب معرف المستخدم من الكلايمز
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long userId))
            {
                return RedirectToAction("Login", "Home");
            }

            // تعيين UserId للمستخدم الحالي
            appointment.UserId = userId;

            // جلب بيانات المريض
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
            if (patient == null)
            {
                TempData["Error"] = "يرجى إدخال بيانات المريض أولاً";
                return RedirectToAction("Create", "Patient");
            }

            // ربط بيانات المريض
            appointment.PatientId = patient.Id;
            appointment.PatientName = patient.FullName;

            // التحقق من صحة البيانات
            if (!ModelState.IsValid)
            {
                ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                ViewBag.Departments = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name");
                var doctors = await _context.Doctors.Where(d => d.Status == true).ToListAsync();
                ViewBag.Doctors = new SelectList(doctors, "Id", "Name");
                return View(appointment);
            }

            // معالجة ملف التشخيص
            if (diagnosisFile != null && diagnosisFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads/diagnosis");
                Directory.CreateDirectory(uploadsFolder);
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(diagnosisFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await diagnosisFile.CopyToAsync(fileStream);
                }

                appointment.DiagnosisFileUrl = "/uploads/diagnosis/" + fileName;
            }

            appointment.Status = AppointmentStatus.Pending;
            appointment.CreatedAt = DateTime.Now;

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم حجز الموعد بنجاح";
            return RedirectToAction(nameof(MyAppointments));
        }

        [HttpGet]
        public IActionResult GetDoctorsByDepartmentAndShift(int departmentId, int shift)
        {
            var doctors = _context.Doctors
                .Where(d => d.DepartmentId == departmentId && (int)d.Shift == shift)
                .Select(d => new
                {
                    id = d.Id,
                    name = d.Name
                })
                .ToList();

            return Json(doctors);
        }

        // يتم إضافة الفقرات التالية لاحقًا:
        // - MyAppointments

        // - EditSchedule
        [HttpGet]
        public IActionResult EditSchedule(int id)
        {
            var appointment = _context.Appointments.FirstOrDefault(a => a.Id == id);
            if (appointment == null)
            {
                TempData["Error"] = "لم يتم العثور على الموعد.";
                return RedirectToAction("Index");
            }

            ViewBag.Departments = new SelectList(_context.Departments, "Id", "Name");
            ViewBag.Doctors = new SelectList(_context.Doctors, "Id", "Name");

            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSchedule(int id, Appointment appointment, IFormFile diagnosisFile)
        {
            if (id != appointment.Id)
            {
                return NotFound();
            }

            var existingAppointment = await _context.Appointments.FindAsync(id);
            if (existingAppointment == null)
            {
                return NotFound();
            }

            // التحقق من صلاحية المريض (من الجلسة)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long userId) || userId != existingAppointment.UserId)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Departments = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name", appointment.DepartmentId);
                ViewBag.Doctors = new SelectList(await _context.Doctors.Where(d => d.Status).ToListAsync(), "Id", "Name", appointment.DoctorId);
                return View(appointment);
            }

            // تحديث البيانات
            existingAppointment.DepartmentId = appointment.DepartmentId;
            existingAppointment.DoctorId = appointment.DoctorId;
            existingAppointment.AppointmentDate = appointment.AppointmentDate;
            existingAppointment.AppointmentTime = appointment.AppointmentTime;
            existingAppointment.Notes = appointment.Notes;
            existingAppointment.UpdatedAt = DateTime.Now;

            // معالجة الملف إن تم رفعه
            if (diagnosisFile != null && diagnosisFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "diagnosis");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(diagnosisFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await diagnosisFile.CopyToAsync(stream);
                }

                // حذف الملف القديم (اختياري)
                if (!string.IsNullOrEmpty(existingAppointment.DiagnosisFileUrl))
                {
                    var oldFilePath = Path.Combine(_environment.WebRootPath, existingAppointment.DiagnosisFileUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                        System.IO.File.Delete(oldFilePath);
                }

                existingAppointment.DiagnosisFileUrl = $"/uploads/diagnosis/{uniqueFileName}";
            }

            try
            {
                _context.Update(existingAppointment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم تعديل الموعد بنجاح";
            }
            catch (Exception ex)
            {
                if (!_context.Appointments.Any(e => e.Id == appointment.Id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(MyAppointments));
        }


        // - DeleteSchedule

        [HttpGet]
        public async Task<IActionResult> Doctors()
        {
            var doctors = await _context.Doctors
                .Include(d => d.Department)
                .Where(d => d.Status == true)
                .ToListAsync();

            return View(doctors);
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // صفحة عرض الحجوزات السابقة للمريض
        public async Task<IActionResult> MyAppointments()
        {
            // جرب أولاً الحصول على UserId من Claim الشائع استخدامه
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // لو ما حصلنا على UserId من ClaimTypes.NameIdentifier جرب "UserId"
            if (string.IsNullOrEmpty(userIdClaim))
            {
                userIdClaim = User.FindFirst("UserId")?.Value;
            }

            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out long userId))
            {
                // لم يتم التعرف على المستخدم، إعادة التوجيه لتسجيل الدخول
                return RedirectToAction("Login", "Home");
            }

            var appointments = await _context.Appointments
                .Where(a => a.UserId == userId)
                .Include(a => a.Department)
                .Include(a => a.Doctor)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.AppointmentTime)
                .ToListAsync();

            return View(appointments);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Patient patient)
        {
            // محاولة جلب هوية المستخدم من الجلسة (Claims)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long userId))
            {
                // إذا لم يكن المستخدم مسجل دخول، إعادته إلى صفحة تسجيل الدخول
                return RedirectToAction("Login", "Home");
            }

            // التحقق إن كان المستخدم قد أدخل بياناته سابقًا
            var existing = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
            if (existing != null)
            {
                TempData["Info"] = "لقد قمت بإدخال بيانات المريض مسبقًا.";
                return RedirectToAction("AddSchedule", "Patient");
            }

            // التحقق من صحة البيانات المدخلة في النموذج
            if (!ModelState.IsValid)
            {
                return View(patient);
            }

            // ربط المريض بالمستخدم الحالي
            patient.UserId = userId;
            patient.CreatedAt = DateTime.Now;

            // حفظ البيانات
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم حفظ بيانات المريض بنجاح.";
            return RedirectToAction("AddSchedule", "Patient");
        }

        public IActionResult Departments()
        {
            var departments = _context.Departments
                .Where(d => d.Status)  // فقط الأقسام الفعالة
                .OrderBy(d => d.Name)
                .ToList();

            return View(departments);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long userId))
                return RedirectToAction("Login", "Home");

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
            if (patient == null)
                return RedirectToAction("Create"); // أو صفحة إنشاء بيانات المريض

            return View(patient);
        }


        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long userId))
                return RedirectToAction("Login", "Home");

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
            if (patient == null)
                return NotFound();

            return View(patient);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(Patient updatedPatient)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long userId))
                return RedirectToAction("Login", "Home");

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
            if (patient == null)
                return NotFound();

            if (!ModelState.IsValid)
                return View(updatedPatient);

            // تحديث الحقول
            patient.FullName = updatedPatient.FullName;
            patient.DateOfBirth = updatedPatient.DateOfBirth;
            patient.Gender = updatedPatient.Gender;
            patient.Address = updatedPatient.Address;
            patient.NationalNumber = updatedPatient.NationalNumber;
            patient.Country = updatedPatient.Country;
            patient.Province = updatedPatient.Province;
            patient.BloodType = updatedPatient.BloodType;
            patient.Allergies = updatedPatient.Allergies;
            patient.ChronicDiseases = updatedPatient.ChronicDiseases;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم تحديث البيانات بنجاح";
            return RedirectToAction("Profile");
        }




    }
}
