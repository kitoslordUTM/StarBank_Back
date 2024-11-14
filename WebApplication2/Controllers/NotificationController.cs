using Microsoft.AspNetCore.Mvc;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly EmailService _emailService;

        public NotificationController(EmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("sendNotification1")]
        public IActionResult SendNotification1([FromBody] NotificationCorreoDto correoDto)
        {
            try
            {
                _emailService.SendEmail(correoDto.ToEmail, correoDto.Subject, correoDto.Message, correoDto.Monto, correoDto.NumeroCuenta);
                return Ok("Notification sent successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Failed to send notification. Error: " + ex.Message);
            }
        }

        // Renombrada la clase para evitar conflicto con ClienteController
        public class NotificationCorreoDto
        {
            public string ToEmail { get; set; }
            public string Subject { get; set; }
            public string Message { get; set; }
            public decimal Monto { get; set; }
            public string NumeroCuenta { get; set; }
        }
    }
}
