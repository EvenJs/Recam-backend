using Microsoft.EntityFrameworkCore;
using Remp.DataAccess.Data;
using Remp.Repository.Interfaces;

namespace Remp.Repository.Repositories;

public class BaseRepository<T> : IBaseRepository<T> where T : class
{
  protected readonly AppDbContext _context;
  protected readonly DbSet<T> _dbSet;

  public BaseRepository(AppDbContext context)
  {
    _context = context;
    _dbSet = context.Set<T>();
  }

  public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

  public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

  public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

  public Task UpdateAsync(T entity){
    _dbSet.Update(entity);
    return Task.CompletedTask;
  }

  public Task DeleteAsync(T entity)
  {
    _dbSet.Remove(entity);
    return Task.CompletedTask;
  } 
}