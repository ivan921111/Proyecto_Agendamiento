namespace Scheduling.Api.Domain;

public class DisponibilidadMedica
{
    public Guid Id { get; set; }
    public Guid MedicoId { get; set; }
    public Medico Medico { get; set; }
    public string DiaSemana { get; set; }
    public TimeSpan HoraInicio { get; set; }
    public TimeSpan HoraFin { get; set; }
    public int DuracionCitaMinutos { get; set; }
}