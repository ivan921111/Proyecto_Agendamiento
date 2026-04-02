namespace Scheduling.Api.Application.Dtos.Appointment;

public class AppointmentDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public DateTime Date { get; set; }
    public Guid UserId { get; set; }
}
