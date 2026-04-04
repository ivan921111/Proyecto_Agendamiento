using Microsoft.EntityFrameworkCore;
using Scheduling.Api.Application.Interfaces;
using System.Linq.Expressions;

namespace Scheduling.Api.Infrastructure.Data;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id) => await _dbSet.FindAsync(id);

    public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression) => await _dbSet.Where(expression).ToListAsync();

    public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

    public async Task RemoveAsync(T entity) => _dbSet.Remove(entity);

    // Nota: SaveChanges se manejará a través de una Unidad de Trabajo (Unit of Work) o al final del request.
    // Para este refactor, el SaveChanges() del controlador original lo manejará.
}