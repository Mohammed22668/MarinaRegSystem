using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MarinaRegSystem.Data;
using MarinaRegSystem.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using MarinaRegSystem.Helpers;
using Microsoft.AspNetCore.Http;


namespace MarinaRegSystem.Controllers
{
    [RoleAuthorize("Admin")]
    public class AdminController : BaseController
    {
        public AdminController(ApplicationDbContext context) : base(context) { }

        public override async Task<IActionResult> Index()
        {
            var user = HttpContext.Items["CurrentUser"] as cUsers;
            ViewBag.Username = user?.Username;

            ViewBag.DepartmentsCount = await _context.Departments.CountAsync();
            ViewBag.DoctorsCount = await _context.Doctors.CountAsync();

            ViewBag.AppointmentsCount = await _context.Appointments.CountAsync();

            ViewBag.RecentAppointments = await _context.Appointments
                .Include(a => a.Doctor)
                .OrderByDescending(a => a.AppointmentDate)
                .Take(5)
                .Select(a => new RecentAppointmentViewModel
                {
                    PatientName = a.PatientName,
                    DoctorName = a.Doctor.Name,
                    AppointmentDate = a.AppointmentDate,
                    Status = a.Status
                })
                .ToListAsync();

            var activeDoctors = await _context.Doctors
                .Include(d => d.Department)
                .Include(d => d.Appointments)
                .Where(d => d.Status == true)
                .Select(d => new ActiveDoctorViewModel
                {
                    Name = d.Name,
                    ImageUrl = d.ImageUrl ?? "/assets/img/doctors/doctor-thumb-03.jpg",
                    DepartmentName = d.Department.Name,
                    AppointmentsCount = d.Appointments.Count,
                    Rating = (int)Math.Round(d.Rating)
                })
                .ToListAsync();
            ViewBag.ActiveDoctors = activeDoctors;

            return View();
        }

        public IActionResult Departments()
        {
            var departments = _context.Departments.ToList();
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





        public IActionResult Doctors()
        {
            var doctors = _context.Doctors.Include(d => d.Department).ToList();
            return View(doctors);
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
                // حفظ الصورة إن وُجدت
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    // توليد اسم عشوائي للصورة
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);

                    // تحديد المسار الكامل للحفظ
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/doctors");

                    // إنشاء المجلد إذا لم يكن موجودًا
                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    // المسار النهائي للصورة
                    var filePath = Path.Combine(uploadPath, fileName);

                    // حفظ الصورة في السيرفر
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    // حفظ رابط الصورة داخل الكائن
                    doctor.ImageUrl = "/uploads/doctors/" + fileName;
                }

                // إضافة الطبيب إلى قاعدة البيانات
                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Doctors));
            }

            // في حال وجود خطأ في النموذج، نعيد تعبئة القائمة المنسدلة
            var departments = await _context.Departments.ToListAsync();
            ViewBag.Departments = new SelectList(departments, "Id", "Name");

            return View(doctor);
        }

        [HttpGet]
        public async Task<IActionResult> EditDoctor(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
            {
                return NotFound();
            }

            var departments = await _context.Departments.ToListAsync();
            ViewBag.Departments = new SelectList(departments, "Id", "Name", doctor.DepartmentId);

            return View(doctor);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDoctor(int id, Doctor doctor, IFormFile ImageFile)
        {
            if (id != doctor.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        // تحميل الصورة وحفظها في مجلد (مثلاً wwwroot/uploads/doctors)
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/doctors");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(fileStream);
                        }

                        // تحديث رابط الصورة في الكائن doctor
                        doctor.ImageUrl = "/uploads/doctors/" + uniqueFileName;
                    }

                    _context.Update(doctor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Doctors.Any(e => e.Id == doctor.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Doctors));
            }

            var departments = await _context.Departments.ToListAsync();
            ViewBag.Departments = new SelectList(departments, "Id", "Name", doctor.DepartmentId);
            return View(doctor);
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
                return NotFound();

            _context.DoctorSchedules.Remove(schedule);
            _context.SaveChanges();

            TempData["Success"] = "تم حذف الجدول بنجاح";
            return RedirectToAction(nameof(DoctorSchedules));
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



        // قائمة الحجوزات
        public IActionResult Appointments()
        {
            var appointments = _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Department)
                .Include(a => a.User)

                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.AppointmentTime)
                .ToList();

            return View(appointments);
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

            return View(appointment);
        }

        // POST: تعديل حجز
        [HttpPost]
        public async Task<IActionResult> EditAppointment(Appointment appointment)
        {
            if (ModelState.IsValid)
            {
                var existing = await _context.Appointments.FindAsync(appointment.Id);
                if (existing == null) return NotFound();

                // تحديث الحقول المراد تعديلها
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
                return RedirectToAction(nameof(Appointments));
            }

            ViewBag.Doctors = new SelectList(_context.Doctors, "Id", "Name", appointment.DoctorId);
            ViewBag.Departments = new SelectList(_context.Departments, "Id", "Name", appointment.DepartmentId);
            return View(appointment);
        }

        // حذف الحجز
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Appointments));
        }
    }
}
