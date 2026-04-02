namespace Scheduling.Api.Application.Dtos.Citas;

public class CitaDto
{
    public Guid Id { get; set; }
    public Guid PacienteId { get; set; }
    public string PacienteNombre { get; set; }
    public Guid MedicoId { get; set; }
    public string MedicoNombre { get; set; }
    public string Especialidad { get; set; }
    public DateTime FechaCita { get; set; }
    public TimeSpan HoraCita { get; set; }
    public string Estado { get; set; }
}
