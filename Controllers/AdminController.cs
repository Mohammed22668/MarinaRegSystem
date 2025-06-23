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


namespace MarinaRegSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : BaseController
    {
        public AdminController(ApplicationDbContext context) : base(context) { }

        [HttpGet]
        public override async Task<IActionResult> Index()
        {


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
        [HttpGet]
        public IActionResult Appointments(AppointmentFilterViewModel filter)
        {
            // جلب البيانات الأساسية
            var query = _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Department)
                .Include(a => a.User)
                .Include(a => a.Patient)
                .AsQueryable();

            // ✅ البحث
            if (!string.IsNullOrEmpty(filter.PatientName))
                query = query.Where(a => a.PatientName.Contains(filter.PatientName));

            if (!string.IsNullOrEmpty(filter.Username))
                query = query.Where(a => a.User.Username.Contains(filter.Username));

            if (!string.IsNullOrEmpty(filter.DoctorName))
                query = query.Where(a => a.Doctor.Name.Contains(filter.DoctorName));

            // ✅ الفلاتر
            if (filter.DepartmentId.HasValue)
                query = query.Where(a => a.DepartmentId == filter.DepartmentId.Value);

            if (filter.AppointmentDate.HasValue)
                query = query.Where(a => a.AppointmentDate.Date == filter.AppointmentDate.Value.Date);

            if (filter.Shift.HasValue)
            {
                var shiftValue = (ShiftType)filter.Shift.Value;
                query = query.Where(a => a.Shift == shiftValue);
            }

            if (filter.Status.HasValue)
                query = query.Where(a => a.Status == filter.Status.Value);

            // تحويل النتائج إلى ViewModel
            var results = query
                .Select(a => new AppointmentViewModel
                {
                    Appointment = a,
                    Patient = _context.Patients.FirstOrDefault(p => p.UserId == a.UserId)
                }).ToList();

            // تعبئة القوائم المنسدلة
            filter.Results = results;
            filter.Departments = _context.Departments
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.Name
                }).ToList();

            return View(filter);
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



    }

}
