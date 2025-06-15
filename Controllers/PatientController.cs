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




namespace MarinaRegSystem.Controllers
{
    [Authorize(Roles = "Patient")]
    public class PatientController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PatientController(ApplicationDbContext context)
        {
            _context = context;
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

            // تعيين PatientName من بيانات المريض
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
            appointment.PatientName = "غير محدد";

            if (appointment.DepartmentId <= 0)
                ModelState.AddModelError("DepartmentId", "القسم مطلوب");

            if (appointment.DoctorId <= 0)
                ModelState.AddModelError("DoctorId", "الطبيب مطلوب");
            if (appointment.AppointmentDate == default)
                ModelState.AddModelError("AppointmentDate", "تاريخ الموعد مطلوب");
            if (appointment.AppointmentTime == default)
                ModelState.AddModelError("AppointmentTime", "وقت الموعد مطلوب");

            if (!ModelState.IsValid)
            {
                ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                ViewBag.Departments = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name");
                var doctors = await _context.Doctors.Where(d => d.Status == true).ToListAsync();
                ViewBag.Doctors = new SelectList(doctors, "Id", "Name");
                return View(appointment);
            }

            // معالجة رفع الملف
            if (diagnosisFile != null && diagnosisFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "diagnosis");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid() + "_" + Path.GetFileName(diagnosisFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await diagnosisFile.CopyToAsync(stream);

                appointment.DiagnosisFileUrl = "/uploads/diagnosis/" + uniqueFileName;
            }

            appointment.Status = AppointmentStatus.Pending;
            appointment.CreatedAt = DateTime.Now;

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم حجز الموعد بنجاح";
            return RedirectToAction(nameof(MyAppointments));
        }

        // يتم إضافة الفقرات التالية لاحقًا:
        // - MyAppointments
        // - EditSchedule
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
    }
}
