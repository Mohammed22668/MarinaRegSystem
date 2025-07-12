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
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long userId))
            {
                return RedirectToAction("Login", "Home");
            }

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
            ViewBag.Patient = patient; // البيانات الخاصة بالحجز لنفسي

            ViewBag.Departments = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name");

            return View(new CreateAppointmentViewModel
            {
                AppointmentDate = DateTime.Today // يمكن وضع تاريخ اليوم افتراضياً
            });
        }
        [HttpPost]
        public async Task<IActionResult> AddSchedule(CreateAppointmentViewModel model)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long userId))
            {
                return RedirectToAction("Login", "Home");
            }

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
            ViewBag.Patient = patient;
            ViewBag.Departments = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name");

            if (!ModelState.IsValid)
            {
                // في حال وجود أخطاء في التحقق، أعاد عرض الصفحة
                return View(model);
            }

            var appointment = new Appointment
            {
                UserId = userId,
                DepartmentId = model.DepartmentId,
                SubDepartmentId = model.SubDepartmentId,
                DoctorId = model.DoctorId,
                AppointmentDate = model.AppointmentDate,
                AppointmentTime = model.AppointmentTime ?? TimeSpan.Zero,
                Shift = model.Shift,
                Notes = model.Notes,
                Status = AppointmentStatus.Pending,
                CreatedAt = DateTime.Now,
            };

            if (model.IsBookingForSelf)
            {
                if (patient == null)
                {
                    TempData["Error"] = "يرجى إدخال بيانات المريض أولاً";
                    return RedirectToAction("Create", "Patient");
                }
                appointment.PatientId = patient.Id;
                appointment.PatientName = $"{patient.FirstName} {patient.SecondName} {patient.ThirdName} {patient.FourthName}";
                appointment.Gender = patient.Gender;
                appointment.DateOfBirth = patient.DateOfBirth;
                appointment.BloodType = patient.BloodType;
            }
            else
            {
                // الحجز لشخص آخر
                appointment.PatientName = $"{model.FirstName} {model.SecondName} {model.ThirdName} {model.FourthName}";
                appointment.Gender = model.Gender;
                appointment.DateOfBirth = model.DateOfBirth;
                appointment.BloodType = model.BloodType;
            }

            // معالجة ملف التشخيص
            if (model.DiagnosisFile != null && model.DiagnosisFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads/diagnosis");
                Directory.CreateDirectory(uploadsFolder);
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.DiagnosisFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.DiagnosisFile.CopyToAsync(fileStream);
                }

                appointment.DiagnosisFileUrl = "/uploads/diagnosis/" + fileName;
            }

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم حجز الموعد بنجاح";
            return RedirectToAction(nameof(MyAppointments));
        }



        [HttpGet]
        public IActionResult GetSubDepartments(int departmentId)
        {
            var subDepartments = _context.SubDepartments
                .Where(sd => sd.DepartmentId == departmentId)
                .Select(sd => new
                {
                    value = sd.Id,
                    text = sd.Name
                })
                .ToList();

            return Json(subDepartments);
        }

        [HttpGet]
        public IActionResult GetDoctorsByDepartment(int departmentId, int? subDepartmentId)
        {
            var doctorsQuery = _context.Doctors.Where(d => d.DepartmentId == departmentId && d.Status == true);

            if (subDepartmentId.HasValue && subDepartmentId.Value != 0)
            {
                doctorsQuery = doctorsQuery.Where(d => d.SubDepartmentId == subDepartmentId);
            }
            else
            {
                doctorsQuery = doctorsQuery.Where(d => d.SubDepartmentId == null);
            }

            var doctors = doctorsQuery
                .AsEnumerable() // التحويل إلى LINQ in-memory
                .Select(d => new
                {
                    value = d.Id,
                    text = d.Name + $" ({GetShiftDisplayName(d.Shift)})"
                })
                .ToList();

            return Json(doctors);
        }

        // مساعد لجلب اسم الشفت
        private static string GetShiftDisplayName(ShiftType shift)
        {
            return shift switch
            {
                ShiftType.Morning => "صباحي",
                ShiftType.Evening => "مسائي",
                ShiftType.Night => "خفر",
                _ => ""
            };
        }



        [HttpGet]
        public IActionResult GetDoctorsByDepartmentAndShift(int departmentId, int shift)
        {
            var doctors = _context.Doctors
                .Where(d => d.DepartmentId == departmentId && (int)d.Shift == shift)
                .Select(d => new
                {
                    id = d.Id,
                    name = d.Name + " - (" + d.Speciality + ")" // ← الاسم + التخصص
                })
                .ToList();

            return Json(doctors);
        }

        // يتم إضافة الفقرات التالية لاحقًا:
        // - MyAppointments

        // - EditSchedule

        [HttpGet]
        public async Task<IActionResult> EditSchedule(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            var model = new EditAppointmentViewModel
            {
                Id = appointment.Id,
                DepartmentId = appointment.DepartmentId,
                SubDepartmentId = appointment.SubDepartmentId,
                DoctorId = appointment.DoctorId.HasValue ? appointment.DoctorId.Value : 0,

                AppointmentDate = appointment.AppointmentDate,
                AppointmentTime = appointment.AppointmentTime,
                Shift = appointment.Shift,
                Notes = appointment.Notes,

                DiagnosisFileUrl = appointment.DiagnosisFileUrl
            };

            ViewBag.Departments = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name");
            ViewBag.SubDepartments = new SelectList(await _context.SubDepartments.Where(s => s.DepartmentId == appointment.DepartmentId).ToListAsync(), "Id", "Name");
            ViewBag.Doctors = new SelectList(await _context.Doctors.Where(d => d.DepartmentId == appointment.DepartmentId).ToListAsync(), "Id", "Name");

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> EditSchedule(EditAppointmentViewModel model, IFormFile DiagnosisFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Departments = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name");
                ViewBag.SubDepartments = new SelectList(await _context.SubDepartments.Where(s => s.DepartmentId == model.DepartmentId).ToListAsync(), "Id", "Name");
                ViewBag.Doctors = new SelectList(await _context.Doctors.Where(d => d.DepartmentId == model.DepartmentId).ToListAsync(), "Id", "Name");
                return View(model);
            }

            var appointment = await _context.Appointments.FindAsync(model.Id);
            if (appointment == null) return NotFound();

            appointment.DepartmentId = model.DepartmentId;
            appointment.SubDepartmentId = model.SubDepartmentId;
            appointment.DoctorId = model.DoctorId;
            appointment.AppointmentDate = model.AppointmentDate;
            appointment.AppointmentTime = model.AppointmentTime ?? TimeSpan.Zero;
            appointment.Shift = model.Shift;
            appointment.Notes = model.Notes;

            if (DiagnosisFile != null && DiagnosisFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads/diagnosis");
                Directory.CreateDirectory(uploadsFolder);
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(DiagnosisFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await DiagnosisFile.CopyToAsync(fileStream);
                }
                appointment.DiagnosisFileUrl = "/uploads/diagnosis/" + fileName;
            }

            _context.Update(appointment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم تحديث الموعد بنجاح";
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

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                userIdClaim = User.FindFirst("UserId")?.Value;
            }

            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out long userId))
            {
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
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long userId))
            {
                return RedirectToAction("Login", "Home");
            }

            var existing = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
            if (existing != null)
            {
                TempData["Info"] = "لقد قمت بإدخال بيانات المريض مسبقًا.";
                return RedirectToAction("AddSchedule", "Patient");
            }

            if (!ModelState.IsValid)
            {
                return View(patient);
            }

            patient.UserId = userId;
            patient.CreatedAt = DateTime.Now;

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم حفظ بيانات المريض بنجاح.";
            return RedirectToAction("AddSchedule", "Patient");
        }

        public IActionResult Departments()
        {
            var departments = _context.Departments
                .Where(d => d.Status)
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
                return RedirectToAction("Create");

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
            patient.FirstName = updatedPatient.FirstName;
            patient.SecondName = updatedPatient.SecondName;
            patient.ThirdName = updatedPatient.ThirdName;
            patient.FourthName = updatedPatient.FourthName;
            patient.DateOfBirth = updatedPatient.DateOfBirth;
            patient.Gender = updatedPatient.Gender;
            patient.Address = updatedPatient.Address;
            patient.NationalNumber = updatedPatient.NationalNumber;
            patient.Country = updatedPatient.Country;
            patient.Province = updatedPatient.Province;
            patient.BloodType = updatedPatient.BloodType;
            patient.Allergies = updatedPatient.Allergies;
            patient.ChronicDiseases = updatedPatient.ChronicDiseases;
            patient.ClosePerson = updatedPatient.ClosePerson;
            patient.PhoneNumber = updatedPatient.PhoneNumber;
            patient.Age = updatedPatient.Age;
            patient.City = updatedPatient.City;
            patient.Neighborhood = updatedPatient.Neighborhood;


            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم تحديث البيانات بنجاح";
            return RedirectToAction("Profile");
        }


        [HttpPost]
        public IActionResult CancelSchedule(int Id, string CancelReason)
        {
            var appointment = _context.Appointments.FirstOrDefault(a => a.Id == Id);

            if (appointment == null)
            {
                TempData["Error"] = "لم يتم العثور على الحجز.";
                return RedirectToAction("MyAppointments");
            }

            appointment.Status = AppointmentStatus.Cancelled;
            appointment.CancelReason = CancelReason;
            appointment.TimeCancel = DateTime.Now;

            _context.SaveChanges();

            TempData["SuccessMessage"] = "تم إلغاء الحجز بنجاح.";
            return RedirectToAction("MyAppointments");
        }

    }
}
