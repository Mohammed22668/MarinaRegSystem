using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MarinaRegSystem.Data;
using MarinaRegSystem.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using MarinaRegSystem.Helpers;

namespace MarinaRegSystem.Controllers
{
    [RoleAuthorize("Admin")]
    public class AdminController : BaseController
    {
        public AdminController(ApplicationDbContext context) : base(context) { }

        public async Task<IActionResult> Index()
        {
            var user = HttpContext.Items["CurrentUser"] as cUsers;
            ViewBag.Username = user?.FullName;

            ViewBag.DepartmentsCount = await _context.Departments.CountAsync();
            ViewBag.DoctorsCount = await _context.Doctors.CountAsync();
            ViewBag.ServicesCount = await _context.Services.CountAsync();
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

        public IActionResult Services()
        {
            var services = _context.Services.Include(s => s.Department).ToList();
            return View(services);
        }

        [HttpGet]
        public async Task<IActionResult> AddService()
        {
            var departments = await _context.Departments.ToListAsync();
            ViewBag.Departments = new SelectList(departments, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddService(Service service)
        {
            if (ModelState.IsValid)
            {
                _context.Services.Add(service);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Services));
            }
            var departments = await _context.Departments.ToListAsync();
            ViewBag.Departments = new SelectList(departments, "Id", "Name");
            return View(service);
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
        public async Task<IActionResult> AddDoctor(Doctor doctor)
        {
            if (ModelState.IsValid)
            {
                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Doctors));
            }
            var departments = await _context.Departments.ToListAsync();
            ViewBag.Departments = new SelectList(departments, "Id", "Name");
            return View(doctor);
        }

        public IActionResult DoctorSchedules()
        {
            var schedules = _context.DoctorSchedules
                .Include(s => s.Doctor)
                .ToList();
            return View(schedules);
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
    }
}
