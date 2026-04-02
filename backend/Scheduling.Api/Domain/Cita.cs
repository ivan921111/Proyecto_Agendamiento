namespace Scheduling.Api.Domain;

public class Cita
{
    public Guid Id { get; set; }
    public Guid PacienteId { get; set; }
    public User Paciente { get; set; }
    public Guid MedicoId { get; set; }
    public Medico Medico { get; set; }
    public DateTime FechaCita { get; set; }
    public TimeSpan HoraCita { get; set; }
    public string Estado { get; set; } // Pendiente, Confirmada, Cancelada, Completada
}