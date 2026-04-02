namespace Scheduling.Api.Application.Dtos.Appointment;

public class CreateAppointmentDto
{
    public string Title { get; set; }
    public DateTime Date { get; set; }
    public Guid UserId { get; set; }
}
