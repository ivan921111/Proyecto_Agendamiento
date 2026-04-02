namespace Scheduling.Api.Domain;

public class Medico
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public User Usuario { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public Guid EspecialidadId { get; set; }
    public Especialidad Especialidad { get; set; }
    public string Email { get; set; }
    public string Telefono { get; set; }
    public ICollection<DisponibilidadMedica> Disponibilidades { get; set; }
    public ICollection<Cita> Citas { get; set; }
}