using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MarinaRegSystem.Models
{

    public class SubDepartmentFilterViewModel
    {
        public string SubDepartmentName { get; set; }
        public int? DepartmentId { get; set; }

        public List<SubDepartment> Results { get; set; }

        public List<SelectListItem> Departments { get; set; }
    }




}