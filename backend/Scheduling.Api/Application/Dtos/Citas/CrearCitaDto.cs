namespace Scheduling.Api.Application.Dtos.Citas;

public class CrearCitaDto
{
    public Guid IdPaciente { get; set; }
    public Guid IdMedico { get; set; }
    public DateTime FechaCita { get; set; }
    public TimeSpan HoraCita { get; set; }
}