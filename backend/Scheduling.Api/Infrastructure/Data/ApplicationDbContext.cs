using Microsoft.EntityFrameworkCore;
using Scheduling.Api.Domain;

namespace Scheduling.Api.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Medico> Medicos { get; set; }
    public DbSet<Especialidad> Especialidades { get; set; }
    public DbSet<DisponibilidadMedica> DisponibilidadesMedicas { get; set; }
    public DbSet<Cita> Citas { get; set; }
}
