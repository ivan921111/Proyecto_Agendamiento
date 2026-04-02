namespace Scheduling.Api.Domain;

public class Especialidad
{
    public Guid Id { get; set; }
    public string Nombre { get; set; }
    public ICollection<Medico> Medicos { get; set; }
}