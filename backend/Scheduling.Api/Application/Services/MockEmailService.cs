using Microsoft.Extensions.Configuration;

namespace Scheduling.Api.Application.Services;

public class MockEmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public MockEmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task SendAppointmentConfirmationAsync(string toEmail, string patientName, string doctorName, string specialty, DateTime date, TimeSpan time)
    {
        // Simula el envío de correo sin conectarse a SMTP
        var useMock = bool.TryParse(_configuration["Email:UseMock"], out var value) && value;
        if (!useMock)
        {
            throw new InvalidOperationException("MockEmailService used but Email:UseMock is false");
        }

        Console.WriteLine("[MOCK EMAIL] Enviando confirmación de cita:");
        Console.WriteLine($"Para: {toEmail}");
        Console.WriteLine($"Paciente: {patientName}");
        Console.WriteLine($"Médico: {doctorName}");
        Console.WriteLine($"Especialidad: {specialty}");
        Console.WriteLine($"Fecha: {date:dd/MM/yyyy}");
        Console.WriteLine($"Hora: {time.ToString("hh\\:mm")}");

        return Task.CompletedTask;
    }
}
