
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarinaRegSystem.Models
{

public class AppointmentViewModel
{
    public Appointment Appointment { get; set; }
    public Patient Patient { get; set; }
}

}