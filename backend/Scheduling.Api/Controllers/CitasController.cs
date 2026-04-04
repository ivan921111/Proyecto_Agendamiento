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
    private readonly ICitaService _citaService;
    private readonly IPdfService _pdfService;

    public CitasController(ApplicationDbContext context, IEmailService emailService, IPdfService pdfService, ICitaService citaService)
    {
        _context = context;
        _pdfService = pdfService;
        _citaService = citaService;
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
        var (cita, error) = await _citaService.CreateCitaAsync(dto);

        if (error != null)
            return BadRequest(error);

        var medico = await _context.Medicos.Include(m => m.Especialidad).FirstOrDefaultAsync(m => m.Id == cita.MedicoId);

        return CreatedAtAction(nameof(ObtenerCitas), new { id = cita.Id }, new { cita.Id, cita.MedicoId, cita.PacienteId, cita.FechaCita, cita.HoraCita, cita.Estado, Especialidad = medico?.Especialidad?.Nombre });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> ReprogramarCita(Guid id, [FromBody] CrearCitaDto dto)
    {
        var (cita, error) = await _citaService.ReprogramarCitaAsync(id, dto);

        if (error != null)
        {
            // Distinguir entre no encontrado y otras solicitudes incorrectas
            if (error.Contains("no encontrada")) return NotFound(error);
            return BadRequest(error);
        }

        return Ok(new { cita.Id, cita.MedicoId, cita.PacienteId, cita.FechaCita, cita.HoraCita, cita.Estado, Especialidad = cita.Medico?.Especialidad?.Nombre });
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