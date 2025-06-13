using System;

namespace MarinaRegSystem.Models
{
    public class RecentAppointmentViewModel
    {
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public DateTime AppointmentDate { get; set; }
        public AppointmentStatus Status { get; set; }
    }
} 