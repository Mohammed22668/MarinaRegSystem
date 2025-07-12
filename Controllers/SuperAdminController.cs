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
    [Authorize(Roles = "SuperAdmin")]
    public class SuperAdminController : BaseController


    {



        public SuperAdminController(ApplicationDbContext context) : base(context) { }

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
            var totalUsers = await _context.cUsers.CountAsync();
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


        [HttpGet]
        public IActionResult Users(string search, string phone, bool? isActive)
        {
            var users = _context.cUsers.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                users = users.Where(u => u.Username.Contains(search));

            if (!string.IsNullOrEmpty(phone))
                users = users.Where(u => u.PhoneNumber.Contains(phone));

            if (isActive.HasValue)
                users = users.Where(u => u.IsActive == isActive);

            var result = users.OrderByDescending(u => u.CreatedAt).ToList();

            return View(result);
        }



        [HttpGet]
        public IActionResult EditUser(long id)
        {
            var user = _context.cUsers.FirstOrDefault(u => u.Id == id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditUser(cUsers model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _context.cUsers.FirstOrDefault(u => u.Id == model.Id);
            if (user == null) return NotFound();

            user.Username = model.Username;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.IsActive = model.IsActive;
            user.UpdatedAt = DateTime.Now;
            user.Role = model.Role;

            _context.SaveChanges();

            TempData["Success"] = "تم تحديث البيانات بنجاح";
            return RedirectToAction("Users");
        }


        [HttpGet]
        public IActionResult ResetPassword(long id)
        {
            var user = _context.cUsers.FirstOrDefault(u => u.Id == id);
            if (user == null) return NotFound();

            var model = new ResetPasswordViewModel
            {
                UserId = user.Id,
                Username = user.Username
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _context.cUsers.FirstOrDefault(u => u.Id == model.UserId);
            if (user == null) return NotFound();

            user.Password = Functions.Encrypt256(model.NewPassword);
            user.UpdatedAt = DateTime.Now;
            _context.SaveChanges();

            TempData["Success"] = "تم تغيير كلمة المرور بنجاح";
            return RedirectToAction("Users");
        }








    }
}