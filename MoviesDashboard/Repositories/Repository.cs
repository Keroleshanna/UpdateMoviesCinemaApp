using Microsoft.EntityFrameworkCore;
using MoviesDashboard.Persistence.Context;
using MoviesDashboard.Repositories.IRepositories;
using System.Linq.Expressions;

namespace MoviesDashboard.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;
        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default)
        {
            var entityCreated = await _dbSet.AddAsync(entity, cancellationToken);
            return entityCreated.Entity;
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? expression = null, Expression<Func<T, object>>[]? includes = null, bool tracked = true, CancellationToken cancellationToken = default)
        {
            var entities = _dbSet.AsQueryable();

            if (!tracked)
                entities = entities.AsNoTracking();

            if (includes?.Length > 0)
            {
                foreach (var item in includes)
                    entities = entities.Include(item);
            }

            if (expression is not null)
                entities = entities.Where(expression);


            return await entities.ToListAsync(cancellationToken);
        }

        public async Task<T?> GetOneAsync(Expression<Func<T, bool>>? expression = null, Expression<Func<T, object>>[]? includes = null, bool tracked = true, CancellationToken cancellationToken = default)
        {
            return (await GetAllAsync(expression, includes, tracked, cancellationToken)).FirstOrDefault();
        }

        public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                // TODO: inject ILogger<Repository<T>> and log it
                Console.Error.WriteLine($"Commit failed: {ex.Message}");
                throw; // عشان اللي بينادي يعرف إن فيه مشكلة
            }
        }
    }
}