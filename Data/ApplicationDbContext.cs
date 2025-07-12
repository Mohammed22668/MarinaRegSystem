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

        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<DoctorSchedule> DoctorSchedules { get; set; }

        public DbSet<Appointment> Appointments { get; set; }

        public DbSet<Patient> Patients { get; set; }

        public DbSet<SubDepartment> SubDepartments { get; set; }

        public DbSet<LabTest> LabTests { get; set; }

        public DbSet<LabInvoice> LabInvoices { get; set; }
        public DbSet<LabInvoiceTest> LabInvoiceTests { get; set; }

        public DbSet<LabInventory> LabInventory { get; set; }
        public DbSet<LabStock> LabStocks { get; set; }




        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // تكوين العلاقات والقيود


            builder.Entity<DoctorSchedule>()
                .HasOne(ds => ds.Doctor)
                .WithMany(d => d.DoctorSchedules)
                .HasForeignKey(ds => ds.DoctorId)
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


            builder.Entity<Doctor>()
                .Property(d => d.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.Entity<DoctorSchedule>()
                .Property(ds => ds.CreatedAt)
                .HasDefaultValueSql("GETDATE()");


            builder.Entity<Appointment>()
            .HasOne(a => a.Patient)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);// أو Cascade حسب ما تراه مناسبًا


            builder.Entity<SubDepartment>()
    .HasOne(sd => sd.Department)
    .WithMany(d => d.SubDepartments)
    .HasForeignKey(sd => sd.DepartmentId)
    .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SubDepartment>()
                .Property(sd => sd.CreatedAt)
                .HasDefaultValueSql("GETDATE()");



            builder.Entity<Doctor>()
          .HasOne(d => d.SubDepartment)
          .WithMany(sd => sd.Doctors)
          .HasForeignKey(d => d.SubDepartmentId)
          .OnDelete(DeleteBehavior.SetNull);


            builder.Entity<LabInvoiceTest>()
                  .HasOne(lit => lit.LabInvoice)
                  .WithMany(li => li.LabInvoiceTests)
                  .HasForeignKey(lit => lit.LabInvoiceId)
                  .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<LabInvoiceTest>()
                .HasOne(lit => lit.LabTest)
                .WithMany()
                .HasForeignKey(lit => lit.LabTestId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.Entity<LabInvoiceTest>()
.Property(p => p.Price)
.HasPrecision(18, 2);

            builder.Entity<LabInvoiceTest>()
                .Property(p => p.QuantityUsed)
                .HasPrecision(18, 2);

            builder.Entity<LabInvoiceTest>()
                .Property(p => p.ResultValue)
                .HasPrecision(18, 2);





        }
    }
}
