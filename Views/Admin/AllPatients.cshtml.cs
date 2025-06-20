using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace MarinaRegSystem.Views.Admin
{
    public class AllPatients : PageModel
    {
        private readonly ILogger<AllPatients> _logger;

        public AllPatients(ILogger<AllPatients> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}