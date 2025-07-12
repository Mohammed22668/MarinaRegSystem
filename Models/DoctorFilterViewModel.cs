using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MarinaRegSystem.Models
{

    // ViewModels/DoctorFilterViewModel.cs
    public class DoctorFilterViewModel
    {
        public string Name { get; set; }
        public string Speciality { get; set; }
        public int? DepartmentId { get; set; }
        public int? SubDepartmentId { get; set; }
        public ShiftType? Shift { get; set; }

        public List<Doctor> Results { get; set; }

        public List<SelectListItem> Departments { get; set; }
        public List<SelectListItem> SubDepartments { get; set; }
    }






}