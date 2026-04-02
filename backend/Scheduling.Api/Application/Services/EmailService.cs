using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Scheduling.Api.Application.Services;

public interface IEmailService
{
    Task SendAppointmentConfirmationAsync(string toEmail, string patientName, string doctorName, string specialty, DateTime date, TimeSpan time);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly IPdfService _pdfService;

    public EmailService(IConfiguration configuration, IPdfService pdfService)
    {
        _configuration = configuration;
        _pdfService = pdfService;
    }

    public async Task SendAppointmentConfirmationAsync(string toEmail, string patientName, string doctorName, string specialty, DateTime date, TimeSpan time)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Sistema de Agendamiento", _configuration["Email:From"] ?? "noreply@agendamiento.com"));
            message.To.Add(new MailboxAddress(patientName, toEmail));
            message.Subject = "Confirmación de Cita Médica";

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = $@"
                <html>
                <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #333;'>Confirmación de Cita Médica</h2>
                    <p>Hola <strong>{patientName}</strong>,</p>
                    <p>Tu cita ha sido confirmada con los siguientes detalles:</p>
                    <div style='background-color: #f5f5f5; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                        <p><strong>Especialidad:</strong> {specialty}</p>
                        <p><strong>Médico:</strong> {doctorName}</p>
                        <p><strong>Fecha:</strong> {date.ToString("dd/MM/yyyy")}</p>
                        <p><strong>Hora:</strong> {time.ToString(@"hh\:mm")}</p>
                    </div>
                    <p>Por favor, llega 15 minutos antes de tu cita.</p>
                    <p>Si necesitas cancelar o reprogramar, puedes hacerlo desde tu cuenta.</p>
                    <br>
                    <p>Atentamente,<br>Equipo de Agendamiento Médico</p>
                </body>
                </html>";

            var pdfBytes = _pdfService.GenerateAppointmentPdf(patientName, doctorName, specialty, date, time);
            bodyBuilder.Attachments.Add("confirmacion_cita.pdf", pdfBytes, new ContentType("application", "pdf"));
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_configuration["Email:Smtp:Host"] ?? "smtp.gmail.com",
                                    int.Parse(_configuration["Email:Smtp:Port"] ?? "587"),
                                    SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_configuration["Email:Smtp:Username"],
                                         _configuration["Email:Smtp:Password"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            // Log the error but don't throw to avoid breaking appointment creation
            Console.WriteLine($"Error sending email: {ex.Message}");
        }
    }
}