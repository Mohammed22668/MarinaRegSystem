using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MarinaRegSystem.Models
{
    public class RegisterViewModel
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string NationalNumber { get; set; }

        [Required]
        public string Gender { get; set; } // "Male" or "Female"

        [Required]
        public string Country { get; set; }

        [Required]
        public string Province { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password"), DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}
