using System.Net.Mail;
using System.Net;

namespace WebApplication2.Controllers
{
    public class EmailService
    {
        private readonly string _smtpServer = "smtp.office365.com";
        private readonly int _smtpPort = 587;
        private readonly string _smtpUser = "starb4nkdev1@outlook.com";
        private readonly string _smtpPass = "starbank123";

        public void SendEmail(string toAddress, string subject, string body, decimal monto, string numeroCuenta)
        {
            var smtpClient = new SmtpClient(_smtpServer)
            {
                Port = _smtpPort,
                Credentials = new NetworkCredential(_smtpUser, _smtpPass),
                EnableSsl = true,
            };

            var fullBody = $"{body} Monto: {monto:C}, Número de Cuenta: {numeroCuenta}";

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpUser),
                Subject = subject,
                Body = fullBody,
                IsBodyHtml = true
            };
            mailMessage.To.Add(toAddress);

            try
            {
                smtpClient.Send(mailMessage);
                Console.WriteLine("Email sent successfully to {0}", toAddress);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending email. Exception: {0}", ex.Message);
                throw; // Optionally re-throw if you want to handle this higher up
            }
        }
    }
}

