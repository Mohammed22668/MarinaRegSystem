using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using MarinaRegSystem.Models;

public class EmailHelper
{
    public static async Task SendAppointmentNotificationEmailAsync(Appointment appointment, string departmentName, string subDepartmentName, string doctorName)
    {
        var subject = "تنبيه حجز موعد جديد";
        var body = $@"
            <p>تم إنشاء حجز جديد في النظام.</p>
            <p><strong>اسم المريض:</strong> {appointment.PatientName}</p>
            <p><strong>التاريخ:</strong> {appointment.AppointmentDate:yyyy-MM-dd}</p>
            <p><strong>الوقت:</strong> {appointment.AppointmentTime}</p>
            <p><strong>القسم:</strong> {departmentName}</p>
            <p><strong>القسم الفرعي:</strong> {subDepartmentName}</p>
            <p><strong>الطبيب:</strong> {doctorName}</p>
        ";

        var toEmails = new[] {
            "prog_dev@marinagroupiq.com",
            "ead@marinahospital-iq.com"
            // "ezio.ahmed12@gmail.com",
        };

        var smtpClient = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential("eo.ghost12@gmail.com", "leik rtcy opud ynlr"),
            EnableSsl = true,
        };

        foreach (var to in toEmails)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress("eo.ghost12@gmail.com", "نظام المواعيد"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(to);
            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
