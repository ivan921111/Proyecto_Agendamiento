using Scheduling.Api.Application.Dtos.Citas;
using Scheduling.Api.Domain;
using Scheduling.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Scheduling.Api.Application.Services;

public interface ICitaService
{
    Task<(Cita? Cita, string? Error)> CreateCitaAsync(CrearCitaDto dto);
    Task<(Cita? Cita, string? Error)> ReprogramarCitaAsync(Guid id, CrearCitaDto dto);
}

public class CitaService : ICitaService
{
    private readonly ApplicationDbContext _context; // Usaremos DbContext para la Unidad de Trabajo (SaveChanges)
    private readonly IEmailService _emailService;

    public CitaService(ApplicationDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task<(Cita? Cita, string? Error)> CreateCitaAsync(CrearCitaDto dto)
    {
        var medico = await _context.Medicos.Include(m => m.Especialidad).FirstOrDefaultAsync(m => m.Id == dto.IdMedico);
        if (medico == null) return (null, "Médico no encontrado");

        var paciente = await _context.Users.FindAsync(dto.IdPaciente);
        if (paciente == null) return (null, "Paciente no encontrado");

        var (horarioValido, errorHorario) = await IsHorarioValido(dto.IdMedico, dto.FechaCita, dto.HoraCita);
        if (!horarioValido) return (null, errorHorario);

        var cita = new Cita
        {
            Id = Guid.NewGuid(),
            MedicoId = dto.IdMedico,
            PacienteId = dto.IdPaciente,
            FechaCita = dto.FechaCita.Date,
            HoraCita = dto.HoraCita,
            Estado = "Pendiente"
        };

        _context.Citas.Add(cita);
        await _context.SaveChangesAsync();

        // Enviar email de confirmación
        try
        {
            await _emailService.SendAppointmentConfirmationAsync(
                paciente.Email,
                paciente.Username,
                $"{medico.Nombre} {medico.Apellido}",
                medico.Especialidad?.Nombre ?? "Sin especialidad",
                cita.FechaCita,
                cita.HoraCita
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending confirmation email: {ex.Message}");
        }

        return (cita, null);
    }

    public async Task<(Cita? Cita, string? Error)> ReprogramarCitaAsync(Guid id, CrearCitaDto dto)
    {
        var citaExistente = await _context.Citas.Include(c => c.Medico).ThenInclude(m => m.Especialidad).FirstOrDefaultAsync(c => c.Id == id);
        if (citaExistente == null) return (null, "Cita a reprogramar no encontrada");

        if (citaExistente.Estado == "Cancelada") return (null, "No se puede reprogramar una cita cancelada");

        var (horarioValido, errorHorario) = await IsHorarioValido(dto.IdMedico, dto.FechaCita, dto.HoraCita, id);
        if (!horarioValido) return (null, errorHorario);

        citaExistente.MedicoId = dto.IdMedico;
        citaExistente.PacienteId = dto.IdPaciente;
        citaExistente.FechaCita = dto.FechaCita.Date;
        citaExistente.HoraCita = dto.HoraCita;

        await _context.SaveChangesAsync();

        return (citaExistente, null);
    }

    private async Task<(bool, string?)> IsHorarioValido(Guid medicoId, DateTime fechaCita, TimeSpan horaCita, Guid? excludingCitaId = null)
    {
        var disponibilidadesDelDia = await _context.DisponibilidadesMedicas
            .Where(d => d.MedicoId == medicoId && d.FechaDisponibilidad.Date == fechaCita.Date)
            .ToListAsync();

        var horarioValido = disponibilidadesDelDia
            .Any(d => d.HoraInicio <= horaCita && horaCita.Add(TimeSpan.FromMinutes(d.DuracionCitaMinutos)) <= d.HoraFin);

        if (!horarioValido) return (false, "El médico no tiene disponibilidad en la fecha/hora solicitada");

        var conflictoQuery = _context.Citas.Where(c => c.MedicoId == medicoId && c.FechaCita.Date == fechaCita.Date && c.HoraCita == horaCita && c.Estado != "Cancelada");
        if (excludingCitaId.HasValue) conflictoQuery = conflictoQuery.Where(c => c.Id != excludingCitaId.Value);
        if (await conflictoQuery.AnyAsync()) return (false, "Ya existe otra cita para ese médico en el mismo horario");

        return (true, null);
    }
}