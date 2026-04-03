namespace Scheduling.Api.Application.Dtos.Disponibilidades;

public class DisponibilidadMedicaDto
{
    public DateTime FechaDisponibilidad { get; set; }
    public string HoraInicio { get; set; }
    public string HoraFin { get; set; }
    public int DuracionCitaMinutos { get; set; }
}
