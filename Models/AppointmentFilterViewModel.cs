using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MarinaRegSystem.Models
{
    public class AppointmentFilterViewModel
    {
        // البحث
        public string PatientName { get; set; }
        public string Username { get; set; }
        public string DoctorName { get; set; }

        // الفلاتر
        public int? DepartmentId { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public int? Shift { get; set; }
        public AppointmentStatus? Status { get; set; }

        // النتائج
        public IEnumerable<AppointmentViewModel> Results { get; set; }

        // القوائم المنسدلة
        public IEnumerable<SelectListItem> Departments { get; set; }
        public IEnumerable<SelectListItem> Statuses { get; set; }
        public IEnumerable<SelectListItem> Shifts { get; set; }
    }
}
