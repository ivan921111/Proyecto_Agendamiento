namespace Scheduling.Api.Application.Dtos.Citas;

public class ReporteCitasDto
{
    public Guid Id { get; set; }
    public string PacienteNombre { get; set; }
    public string PacienteEmail { get; set; }
    public DateTime FechaCita { get; set; }
    public TimeSpan HoraCita { get; set; }
    public string Estado { get; set; }
    public string Especialidad { get; set; }
}
