using Microsoft.AspNetCore.Mvc;
using MarinaRegSystem.Data;
using MarinaRegSystem.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace MarinaRegSystem.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // public async Task<IActionResult> AddSchedule()
        // {
        //     // تحميل الأقسام
        //     ViewBag.Departments = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name");

        //     // تحميل الخدمات
        //     ViewBag.Services = new SelectList(await _context.Services.ToListAsync(), "Id", "Name");

        //     // تحميل الأطباء
        //     var doctors = await _context.Doctors
        //         .Include(d => d.Department)
        //         .Where(d => d.Status == true)
        //         .ToListAsync();
        //     ViewBag.Doctors = new SelectList(doctors, "Id", "Name");

        //     return View();
        // }

        // [HttpPost]
        // public async Task<IActionResult> AddSchedule(Appointment appointment, IFormFile diagnosisFile)
        // {
        //     try
        //     {
        //         // تحويل التاريخ والوقت
        //         if (appointment.AppointmentDate != default(DateTime))
        //         {
        //             var dateStr = appointment.AppointmentDate.ToString("dd/MM/yyyy");
        //             if (DateTime.TryParseExact(dateStr, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
        //             {
        //                 appointment.AppointmentDate = parsedDate;
        //             }
        //         }

        //         // التحقق من صحة البيانات
        //         if (string.IsNullOrEmpty(appointment.PatientName))
        //         {
        //             ModelState.AddModelError("PatientName", "اسم المريض مطلوب");
        //         }

        //         if (appointment.DepartmentId <= 0)
        //         {
        //             ModelState.AddModelError("DepartmentId", "القسم مطلوب");
        //         }

        //         if (appointment.ServiceId <= 0)
        //         {
        //             ModelState.AddModelError("ServiceId", "الخدمة مطلوبة");
        //         }

        //         if (appointment.DoctorId <= 0)
        //         {
        //             ModelState.AddModelError("DoctorId", "الطبيب مطلوب");
        //         }

        //         if (appointment.AppointmentDate == default(DateTime))
        //         {
        //             ModelState.AddModelError("AppointmentDate", "تاريخ الموعد مطلوب");
        //         }

        //         if (appointment.AppointmentTime == default(TimeSpan))
        //         {
        //             ModelState.AddModelError("AppointmentTime", "وقت الموعد مطلوب");
        //         }

        //         if (ModelState.IsValid)
        //         {
        //             // معالجة ملف التشخيص إذا تم تحميله
        //             if (diagnosisFile != null && diagnosisFile.Length > 0)
        //             {
        //                 var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "diagnosis");
        //                 if (!Directory.Exists(uploadsFolder))
        //                 {
        //                     Directory.CreateDirectory(uploadsFolder);
        //                 }

        //                 var uniqueFileName = Guid.NewGuid().ToString() + "_" + diagnosisFile.FileName;
        //                 var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        //                 using (var fileStream = new FileStream(filePath, FileMode.Create))
        //                 {
        //                     await diagnosisFile.CopyToAsync(fileStream);
        //                 }

        //                 appointment.DiagnosisFileUrl = "/uploads/diagnosis/" + uniqueFileName;
        //             }

        //             // تعيين القيم الافتراضية
        //             appointment.Status = AppointmentStatus.Pending;
        //             appointment.CreatedAt = DateTime.Now;
        //             appointment.UserId = 1; // يجب تغيير هذا لاستخدام معرف المستخدم الحالي

        //             _context.Appointments.Add(appointment);
        //             await _context.SaveChangesAsync();

        //             TempData["SuccessMessage"] = "تم إضافة الحجز بنجاح";
        //             return RedirectToAction(nameof(Index));
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         ModelState.AddModelError("", "حدث خطأ أثناء إضافة الحجز: " + ex.Message);
        //     }

        //     // إعادة تحميل البيانات في حالة فشل التحقق
        //     ViewBag.Departments = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name");
        //     ViewBag.Services = new SelectList(await _context.Services.ToListAsync(), "Id", "Name");
        //     var doctors = await _context.Doctors
        //         .Include(d => d.Department)
        //         .Where(d => d.Status == true)
        //         .ToListAsync();
        //     ViewBag.Doctors = new SelectList(doctors, "Id", "Name");

        //     return View(appointment);
        // }

        public async Task<IActionResult> Index()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Department)
                .Include(a => a.Doctor)
                .ToListAsync();
            return View(appointments);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }
            return View(appointment);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Appointment appointment)
        {
            if (id != appointment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Update(appointment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(appointment);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }
            return View(appointment);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }




    }
}