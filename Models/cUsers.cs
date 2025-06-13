using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MarinaRegSystem.Models
{
    [Table("cUsers")]
    public class cUsers
    {
        public Int64 Id { get; set; }
        public string Uid { get; set; }
        public string FullName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; }
        public string NationalNumber { get; set; }
        public string Gender { get; set; } 
        public string Country { get; set; }
        public string Province { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }

         public string Role { get; set; }
    }
}
