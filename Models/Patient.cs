using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace MarinaRegSystem.Models
{
    [Table("Patients")]
    public class Patient
    {
        public long Id { get; set; }
        public long UserId { get; set; }

        public string FullName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public string NationalNumber { get; set; }
        public string Country { get; set; }
        public string Province { get; set; }

        public string BloodType { get; set; }

        public string Allergies { get; set; }
        public string ChronicDiseases { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;



        // العلاقة
        public virtual cUsers User { get; set; }

        public virtual ICollection<Appointment> Appointments { get; set; }

        public Patient()
        {
            Appointments = new HashSet<Appointment>();
        }


    }
}