using MarinaRegSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MarinaRegSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<cUsers> cUsers { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<DoctorSchedule> DoctorSchedules { get; set; }
        public DbSet<DoctorService> DoctorServices { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // تكوين العلاقات والقيود
            builder.Entity<Service>()
                .HasOne(s => s.Department)
                .WithMany(d => d.Services)
                .HasForeignKey(s => s.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<DoctorSchedule>()
                .HasOne(ds => ds.Doctor)
                .WithMany(d => d.DoctorSchedules)
                .HasForeignKey(ds => ds.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<DoctorService>()
                .HasOne(ds => ds.Doctor)
                .WithMany(d => d.DoctorServices)
                .HasForeignKey(ds => ds.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<DoctorService>()
                .HasOne(ds => ds.Service)
                .WithMany(s => s.DoctorServices)
                .HasForeignKey(ds => ds.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Appointment>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Appointment>()
                .HasOne(a => a.Department)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Appointment>()
                .HasOne(a => a.Service)
                .WithMany(s => s.Appointments)
                .HasForeignKey(a => a.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // تكوين القيم الافتراضية
            builder.Entity<Appointment>()
                .Property(a => a.Status)
                .HasDefaultValue(AppointmentStatus.Pending);

            builder.Entity<Appointment>()
                .Property(a => a.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.Entity<Department>()
                .Property(d => d.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.Entity<Service>()
                .Property(s => s.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.Entity<Doctor>()
                .Property(d => d.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.Entity<DoctorSchedule>()
                .Property(ds => ds.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.Entity<DoctorService>()
                .Property(ds => ds.CreatedAt)
                .HasDefaultValueSql("GETDATE()");
        }
    }
}
