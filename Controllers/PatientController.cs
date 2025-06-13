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
            ViewBag.Departments = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name");
            ViewBag.Services = new SelectList(await _context.Services.ToListAsync(), "Id", "Name");

            var doctors = await _context.Doctors
                .Where(d => d.Status == true)
                .ToListAsync();
            ViewBag.Doctors = new SelectList(doctors, "Id", "Name");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddSchedule(Appointment appointment, IFormFile diagnosisFile)
        {
            try
            {
                // التحقق من صحة البيانات
                if (string.IsNullOrEmpty(appointment.PatientName))
                {
                    ModelState.AddModelError("PatientName", "اسم المريض مطلوب");
                }

                if (appointment.DepartmentId <= 0)
                {
                    ModelState.AddModelError("DepartmentId", "القسم مطلوب");
                }

                if (appointment.ServiceId <= 0)
                {
                    ModelState.AddModelError("ServiceId", "الخدمة مطلوبة");
                }

                if (appointment.DoctorId <= 0)
                {
                    ModelState.AddModelError("DoctorId", "الطبيب مطلوب");
                }

                if (appointment.AppointmentDate == default)
                {
                    ModelState.AddModelError("AppointmentDate", "تاريخ الموعد مطلوب");
                }

                if (appointment.AppointmentTime == default)
                {
                    ModelState.AddModelError("AppointmentTime", "وقت الموعد مطلوب");
                }

                if (ModelState.IsValid)
                {
                    // رفع الملف إن وُجد
                    if (diagnosisFile != null && diagnosisFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "diagnosis");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var uniqueFileName = Guid.NewGuid() + "_" + diagnosisFile.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await diagnosisFile.CopyToAsync(stream);
                        }

                        appointment.DiagnosisFileUrl = "/uploads/diagnosis/" + uniqueFileName;
                    }

                    appointment.Status = AppointmentStatus.Pending;
                    appointment.CreatedAt = DateTime.Now;
                    // appointment.UserId = 1; // مؤقتًا - لاحقًا نربطه بالمستخدم الحالي
                    // appointment.UserId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                    if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                    {
                        ModelState.AddModelError("", "حدث خطأ: المستخدم غير معروف.");
                        ViewBag.Departments = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name");
                        ViewBag.Services = new SelectList(await _context.Services.ToListAsync(), "Id", "Name");
                        var doctors2 = await _context.Doctors.Where(d => d.Status == true).ToListAsync();
                        ViewBag.Doctors = new SelectList(doctors2, "Id", "Name");
                        return View(appointment);
                    }
                    appointment.UserId = userId;




                    _context.Appointments.Add(appointment);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "تم حجز الموعد بنجاح";
                    return RedirectToAction(nameof(MyAppointments));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "حدث خطأ أثناء الحجز: " + ex.Message);
            }

            ViewBag.Departments = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name");
            ViewBag.Services = new SelectList(await _context.Services.ToListAsync(), "Id", "Name");

            var doctors = await _context.Doctors
                .Where(d => d.Status == true)
                .ToListAsync();
            ViewBag.Doctors = new SelectList(doctors, "Id", "Name");

            return View(appointment);
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
            // استرجاع UserId من الـ Claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return RedirectToAction("Login", "Account"); // أو حسب اسم صفحة الدخول لديك
            }

            var userId = long.Parse(userIdClaim.Value);

            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Department)
                .Include(a => a.Service)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.AppointmentTime)
                .ToListAsync();

            return View(appointments);
        }

    }
}
