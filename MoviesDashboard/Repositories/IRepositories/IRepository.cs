using System.Linq.Expressions;

namespace MoviesDashboard.Repositories.IRepositories
{
    public interface IRepository<T> where T : class
    {

        Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);
        void Update(T entity);
        void Delete(T entity);

        Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, bool>>? expression = null,
            Expression<Func<T, object>>[]? includes = null,
            bool tracker = true,
            CancellationToken cancellationToken = default);

        Task<T?> GetOneAsync(
            Expression<Func<T, bool>>? expression = null,
            Expression<Func<T, object>>[]? includes = null,
            bool tracker = true,
            CancellationToken cancellationToken = default);

        Task<int> CommitAsync(CancellationToken cancellationToken = default);
    }
}
