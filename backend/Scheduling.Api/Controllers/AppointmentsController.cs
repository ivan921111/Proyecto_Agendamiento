using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scheduling.Api.Application.Dtos.Appointment;
using Scheduling.Api.Domain;
using Scheduling.Api.Infrastructure.Data;

namespace Scheduling.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AppointmentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAppointments()
    {
        var appointments = await _context.Appointments
            .Select(a => new AppointmentDto
            {
                Id = a.Id,
                Title = a.Title,
                Date = a.Date,
                UserId = a.UserId
            })
            .ToListAsync();

        return Ok(appointments);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentDto createAppointmentDto)
    {
        var appointment = new Appointment
        {
            Title = createAppointmentDto.Title,
            Date = createAppointmentDto.Date,
            UserId = createAppointmentDto.UserId
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        var appointmentDto = new AppointmentDto
        {
            Id = appointment.Id,
            Title = appointment.Title,
            Date = appointment.Date,
            UserId = appointment.UserId
        };

        return CreatedAtAction(nameof(CreateAppointment), new { id = appointment.Id }, appointmentDto);
    }
}
