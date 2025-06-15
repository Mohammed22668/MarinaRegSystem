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
        public string Username { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }

        public string Role { get; set; }

        public virtual Patient Patient { get; set; }
    }
}
