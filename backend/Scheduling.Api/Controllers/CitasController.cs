using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scheduling.Api.Application.Dtos.Citas;
using Scheduling.Api.Application.Dtos.Disponibilidades;
using Scheduling.Api.Application.Services;
using Scheduling.Api.Domain;
using Scheduling.Api.Infrastructure.Data;
using System.Security.Claims;

namespace Scheduling.Api.Controllers;

[Authorize]
[ApiController]
[Route("citas")]
public class CitasController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IPdfService _pdfService;

    public CitasController(ApplicationDbContext context, IEmailService emailService, IPdfService pdfService)
    {
        _context = context;
        _emailService = emailService;
        _pdfService = pdfService;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerCitas()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized("Token inválido o expirado.");

        var citas = await _context.Citas
            .Where(c => c.PacienteId == userId)
            .Select(c => new CitaDto
            {
                Id = c.Id,
                PacienteId = c.PacienteId,
                PacienteNombre = c.Paciente != null ? c.Paciente.Username : "",
                MedicoId = c.MedicoId,
                MedicoNombre = c.Medico != null ? $"{c.Medico.Nombre} {c.Medico.Apellido}" : "",
                Especialidad = c.Medico != null && c.Medico.Especialidad != null ? c.Medico.Especialidad.Nombre : "",
                FechaCita = c.FechaCita,
                HoraCita = c.HoraCita,
                Estado = c.Estado
            })
            .ToListAsync();

        return Ok(citas);
    }

    [HttpGet("especialidades")]
    public async Task<IActionResult> ObtenerEspecialidades()
    {
        var especialidades = await _context.Especialidades
            .Select(e => new
            {
                e.Id,
                e.Nombre
            })
            .ToListAsync();

        return Ok(especialidades);
    }

    [HttpGet("medicos-por-especialidad")]
    public async Task<IActionResult> ObtenerMedicosPorEspecialidad([FromQuery] Guid especialidadId)
    {
        var medicos = await _context.Medicos
            .Where(m => m.EspecialidadId == especialidadId)
            .Include(m => m.Especialidad)
            .Select(m => new
            {
                m.Id,
                m.Nombre,
                m.Apellido,
                Especialidad = m.Especialidad != null ? m.Especialidad.Nombre : "",
                m.Email,
                m.Telefono
            })
            .ToListAsync();

        return Ok(medicos);
    }

    [HttpGet("disponibilidad")]
    public async Task<IActionResult> ObtenerDisponibilidad([FromQuery] Guid medicoId)
    {
        var disponibilidad = await _context.DisponibilidadesMedicas
            .Where(d => d.MedicoId == medicoId)
            .Select(d => new
            {
                d.Id,
                d.FechaDisponibilidad,
                d.HoraInicio,
                d.HoraFin,
                d.DuracionCitaMinutos
            })
            .ToListAsync();

        return Ok(disponibilidad);
    }

    [HttpGet("citas-ocupadas")]
    public async Task<IActionResult> ObtenerCitasOcupadasPorMedico([FromQuery] Guid medicoId)
    {
        var citasOcupadas = await _context.Citas
            .Where(c => c.MedicoId == medicoId && c.Estado != "Cancelada")
            .Select(c => new
            {
                c.FechaCita,
                c.HoraCita
            })
            .ToListAsync();

        return Ok(citasOcupadas);
    }


    [HttpPost]
    public async Task<IActionResult> CrearCita([FromBody] CrearCitaDto dto)
    {
        var medico = await _context.Medicos.Include(m => m.Especialidad).FirstOrDefaultAsync(m => m.Id == dto.IdMedico);
        if (medico == null)
            return BadRequest("Médico no encontrado");

        var paciente = await _context.Users.FindAsync(dto.IdPaciente);
        if (paciente == null)
            return BadRequest("Paciente no encontrado");

        // Verificar disponibilidad médica
        var disponibilidadesDelDia = await _context.DisponibilidadesMedicas
            .Where(d => d.MedicoId == dto.IdMedico && d.FechaDisponibilidad.Date == dto.FechaCita.Date)
            .ToListAsync(); // Traer los horarios a memoria para evaluación en cliente

        // Ahora que los datos están en memoria, podemos usar la lógica de C# sin problemas
        var horarioValido = disponibilidadesDelDia
            .Any(d => d.HoraInicio <= dto.HoraCita && dto.HoraCita.Add(TimeSpan.FromMinutes(d.DuracionCitaMinutos)) <= d.HoraFin);

        if (!horarioValido)
            return BadRequest("El médico no tiene disponibilidad en la fecha/hora solicitada");

        // Verificar conflicto de citas existentes
        var conflicto = await _context.Citas.AnyAsync(c => c.MedicoId == dto.IdMedico && c.FechaCita.Date == dto.FechaCita.Date && c.HoraCita == dto.HoraCita && c.Estado != "Cancelada");
        if (conflicto)
            return BadRequest("Ya existe otra cita para ese médico en el mismo horario");

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
            // Log error but don't fail the appointment creation
            Console.WriteLine($"Error sending confirmation email: {ex.Message}");
        }

        return CreatedAtAction(nameof(ObtenerCitas), new { id = cita.Id }, new { cita.Id, cita.MedicoId, cita.PacienteId, cita.FechaCita, cita.HoraCita, cita.Estado, Especialidad = medico.Especialidad?.Nombre });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> ReprogramarCita(Guid id, [FromBody] CrearCitaDto dto)
    {
        var citaExistente = await _context.Citas.Include(c => c.Medico).ThenInclude(m => m.Especialidad).FirstOrDefaultAsync(c => c.Id == id);
        if (citaExistente == null)
            return NotFound("Cita a reprogramar no encontrada");

        if (citaExistente.Estado == "Cancelada")
            return BadRequest("No se puede reprogramar una cita cancelada");

        var medico = await _context.Medicos.FindAsync(dto.IdMedico);
        if (medico == null)
            return BadRequest("Médico no encontrado");

        var paciente = await _context.Users.FindAsync(dto.IdPaciente);
        if (paciente == null)
            return BadRequest("Paciente no encontrado");

        // Verificar disponibilidad médica para nueva fecha/hora
        var disponibilidadesDelDia = await _context.DisponibilidadesMedicas
            .Where(d => d.MedicoId == dto.IdMedico && d.FechaDisponibilidad.Date == dto.FechaCita.Date)
            .ToListAsync();

        var horarioValido = disponibilidadesDelDia
            .Any(d => d.HoraInicio <= dto.HoraCita && dto.HoraCita.Add(TimeSpan.FromMinutes(d.DuracionCitaMinutos)) <= d.HoraFin);

        if (!horarioValido)
            return BadRequest("El médico no tiene disponibilidad en la fecha/hora solicitada");

        // No conflicto con otras citas que no sean ella misma
        var conflicto = await _context.Citas.AnyAsync(c => c.Id != id && c.MedicoId == dto.IdMedico && c.FechaCita.Date == dto.FechaCita.Date && c.HoraCita == dto.HoraCita && c.Estado != "Cancelada");
        if (conflicto)
            return BadRequest("Ya existe otra cita para ese médico en el mismo horario");

        citaExistente.MedicoId = dto.IdMedico;
        citaExistente.PacienteId = dto.IdPaciente;
        citaExistente.FechaCita = dto.FechaCita.Date;
        citaExistente.HoraCita = dto.HoraCita;

        await _context.SaveChangesAsync();

        return Ok(new { citaExistente.Id, citaExistente.MedicoId, citaExistente.PacienteId, citaExistente.FechaCita, citaExistente.HoraCita, citaExistente.Estado, Especialidad = medico.Especialidad?.Nombre });
    }

    [HttpGet("{id}/pdf")]
    public async Task<IActionResult> DescargarCitaPdf(Guid id)
    {
        var cita = await _context.Citas
             .Include(c => c.Medico)
             .ThenInclude(m => m.Especialidad)
             .Include(c => c.Paciente)
             .FirstOrDefaultAsync(c => c.Id == id);

        if (cita == null) return NotFound("Cita no encontrada");

        var doctorName = cita.Medico != null ? $"{cita.Medico.Nombre} {cita.Medico.Apellido}" : "Desconocido";
        var patientName = cita.Paciente != null ? cita.Paciente.Username : "Desconocido";
        var specialty = cita.Medico?.Especialidad?.Nombre ?? "Sin especialidad";

        var pdfBytes = _pdfService.GenerateAppointmentPdf(patientName, doctorName, specialty, cita.FechaCita, cita.HoraCita);

        return File(pdfBytes, "application/pdf", $"Cita_{id}.pdf");
    }

    [HttpPut("{id}/cancelar")]
    public async Task<IActionResult> CancelarCita(Guid id)
    {
        var cita = await _context.Citas.FindAsync(id);
        if (cita == null) return NotFound("Cita no encontrada");

        if (cita.Estado == "Cancelada") return BadRequest("La cita ya está cancelada");

        cita.Estado = "Cancelada";
        await _context.SaveChangesAsync();

        return Ok(new { cita.Id, cita.Estado });
    }

    // ENDPOINTS PARA MÉDICOS

    [HttpGet("medico/me")]
    public async Task<IActionResult> ObtenerMedicoActual()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized("Token inválido or expirado");

        var medico = await _context.Medicos
            .Include(m => m.Especialidad)
            .FirstOrDefaultAsync(m => m.UsuarioId == userId);

        if (medico == null)
            return NotFound("No es un médico registrado");

        return Ok(new
        {
            medico.Id,
            medico.Nombre,
            medico.Apellido,
            medico.EspecialidadId,
            Especialidad = medico.Especialidad?.Nombre ?? "",
            medico.Email,
            medico.Telefono
        });
    }

    [HttpGet("medico/disponibilidades")]
    public async Task<IActionResult> ObtenerDisponibilidadesMedicoActual()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized("Token inválido o expirado");

        var medico = await _context.Medicos.FirstOrDefaultAsync(m => m.UsuarioId == userId);
        if (medico == null)
            return NotFound("No es un médico registrado");

        var disponibilidades = await _context.DisponibilidadesMedicas
            .Where(d => d.MedicoId == medico.Id)
            .Select(d => new
            {
                d.Id,
                d.FechaDisponibilidad,
                d.HoraInicio,
                d.HoraFin,
                d.DuracionCitaMinutos
            })
            .ToListAsync();

        return Ok(disponibilidades);
    }

    [HttpPost("medico/disponibilidades")]
    public async Task<IActionResult> CrearDisponibilidadMedicoActual([FromBody] DisponibilidadMedicaDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized("Token inválido o expirado");

        var medico = await _context.Medicos.FirstOrDefaultAsync(m => m.UsuarioId == userId);
        if (medico == null)
            return NotFound("No es un médico registrado");

        // Verificar si ya existe disponibilidad para esa fecha
        var existente = await _context.DisponibilidadesMedicas
            .FirstOrDefaultAsync(d => d.MedicoId == medico.Id && d.FechaDisponibilidad.Date == dto.FechaDisponibilidad.Date);

        if (existente != null)
            return BadRequest("Ya existe disponibilidad registrada para esa fecha");

        // Convertir strings a TimeSpan
        if (!TimeSpan.TryParse(dto.HoraInicio, out var horaInicio) || !TimeSpan.TryParse(dto.HoraFin, out var horaFin))
            return BadRequest("Formato de hora inválido");


        var disponibilidad = new DisponibilidadMedica
        {
            Id = Guid.NewGuid(),
            MedicoId = medico.Id,
            FechaDisponibilidad = dto.FechaDisponibilidad.Date,
            HoraInicio = horaInicio,
            HoraFin = horaFin,
            DuracionCitaMinutos = dto.DuracionCitaMinutos
        };

        _context.DisponibilidadesMedicas.Add(disponibilidad);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerDisponibilidadesMedicoActual), new { id = disponibilidad.Id }, new
        {
            disponibilidad.Id,
            disponibilidad.FechaDisponibilidad,
            disponibilidad.HoraInicio,
            disponibilidad.HoraFin,
            disponibilidad.DuracionCitaMinutos
        });
    }

    [HttpPut("medico/disponibilidades/{id}")]
    public async Task<IActionResult> ActualizarDisponibilidadMedicosActual(Guid id, [FromBody] DisponibilidadMedicaDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized("Token inválido o expirado");

        var medico = await _context.Medicos.FirstOrDefaultAsync(m => m.UsuarioId == userId);
        if (medico == null)
            return NotFound("No es un médico registrado");

        var disponibilidad = await _context.DisponibilidadesMedicas
            .FirstOrDefaultAsync(d => d.Id == id && d.MedicoId == medico.Id);

        if (disponibilidad == null)
            return NotFound("Disponibilidad no encontrada");

        // Convertir strings a TimeSpan
        if (!TimeSpan.TryParse(dto.HoraInicio, out var horaInicio) || !TimeSpan.TryParse(dto.HoraFin, out var horaFin))
            return BadRequest("Formato de hora inválido");

        disponibilidad.FechaDisponibilidad = dto.FechaDisponibilidad.Date;
        disponibilidad.HoraInicio = horaInicio;
        disponibilidad.HoraFin = horaFin;
        disponibilidad.DuracionCitaMinutos = dto.DuracionCitaMinutos;

        await _context.SaveChangesAsync();

        return Ok(disponibilidad);
    }

    [HttpDelete("medico/disponibilidades/{id}")]
    public async Task<IActionResult> EliminarDisponibilidadMedicoActual(Guid id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized("Token inválido o expirado");

        var medico = await _context.Medicos.FirstOrDefaultAsync(m => m.UsuarioId == userId);
        if (medico == null)
            return NotFound("No es un médico registrado");

        var disponibilidad = await _context.DisponibilidadesMedicas
            .FirstOrDefaultAsync(d => d.Id == id && d.MedicoId == medico.Id);

        if (disponibilidad == null)
            return NotFound("Disponibilidad no encontrada");

        _context.DisponibilidadesMedicas.Remove(disponibilidad);
        await _context.SaveChangesAsync();

        return Ok("Disponibilidad eliminada");
    }

    [HttpGet("medico/reporte")]
    public async Task<IActionResult> ObtenerReporteCitasMedicoActual(
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin,
        [FromQuery] string? paciente,
        [FromQuery] string estado)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized("Token inválido o expirado");

        var medico = await _context.Medicos
            .Include(m => m.Especialidad)
            .FirstOrDefaultAsync(m => m.UsuarioId == userId);

        if (medico == null)
            return NotFound("No es un médico registrado");

        var query = _context.Citas
            .Include(c => c.Paciente)
            .Where(c => c.MedicoId == medico.Id);

        if (fechaInicio.HasValue)
            query = query.Where(c => c.FechaCita >= fechaInicio.Value);

        if (fechaFin.HasValue)
            query = query.Where(c => c.FechaCita <= fechaFin.Value);

        if (!string.IsNullOrWhiteSpace(paciente))
            query = query.Where(c => c.Paciente != null && c.Paciente.Username.Contains(paciente));

        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(c => c.Estado.Contains(estado));

        var citas = await query
            .Select(c => new ReporteCitasDto
            {
                Id = c.Id,
                PacienteNombre = c.Paciente != null ? c.Paciente.Username : "",
                PacienteEmail = c.Paciente != null ? c.Paciente.Email : "",
                FechaCita = c.FechaCita,
                HoraCita = c.HoraCita,
                Estado = c.Estado,
                Especialidad = medico.Especialidad != null ? medico.Especialidad.Nombre : ""
            })
            .OrderByDescending(c => c.FechaCita)
            .ToListAsync();

        return Ok(citas);
    }

    [HttpGet("medico/reporte/pdf")]
    public async Task<IActionResult> DescargarReporteCitasMedicoActualPdf(
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin,
        [FromQuery] string? paciente,
        [FromQuery] string? estado)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized("Token inválido o expirado");

        var medico = await _context.Medicos
            .Include(m => m.Especialidad)
            .FirstOrDefaultAsync(m => m.UsuarioId == userId);

        if (medico == null)
            return NotFound("No es un médico registrado");

        var query = _context.Citas
            .Include(c => c.Paciente)
            .Where(c => c.MedicoId == medico.Id);

        if (fechaInicio.HasValue)
            query = query.Where(c => c.FechaCita >= fechaInicio.Value);

        if (fechaFin.HasValue)
            query = query.Where(c => c.FechaCita <= fechaFin.Value);

        if (!string.IsNullOrWhiteSpace(paciente))
            query = query.Where(c => c.Paciente != null && c.Paciente.Username.Contains(paciente));

        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(c => c.Estado.Contains(estado));

        var citas = await query
            .OrderByDescending(c => c.FechaCita)
            .ToListAsync();

        // Generar PDF del reporte
        var pdfBytes = GenerarReportePdf(medico, citas);

        return File(pdfBytes, "application/pdf", $"Reporte_Citas_{medico.Nombre}_{medico.Apellido}_{DateTime.Now:yyyyMMdd}.pdf");
    }

    private byte[] GenerarReportePdf(Medico medico, List<Cita> citas)
    {
        // Aquí importas QuestPDF para generar el PDF del reporte
        return _pdfService.GenerateMedicAppointmentReportPdf(
            medico.Nombre,
            medico.Apellido,
            medico.Especialidad?.Nombre ?? "",
            citas.Select(c => new ReporteCitasDto
            {
                Id = c.Id,
                PacienteNombre = c.Paciente != null ? c.Paciente.Username : "",
                PacienteEmail = c.Paciente != null ? c.Paciente.Email : "",
                FechaCita = c.FechaCita,
                HoraCita = c.HoraCita,
                Estado = c.Estado,
                Especialidad = medico.Especialidad != null ? medico.Especialidad.Nombre : ""
            }).ToList()
        );
    }
}