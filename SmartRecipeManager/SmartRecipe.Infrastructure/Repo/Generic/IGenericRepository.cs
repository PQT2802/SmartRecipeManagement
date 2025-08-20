using System.Linq.Expressions;

namespace SmartRecipe.Infrastructure.Repo.Generic
{
    public interface IGenericRepository<T> where T : class
    {
        List<T> GetAll();
        Task<List<T>> GetAllAsync();
        void Create(T entity);
        Task<int> CreateAsync(T entity);
        void Update(T entity);
        Task<int> UpdateAsync(T entity);
        bool Remove(T entity);
        Task<bool> RemoveAsync(T entity);
        Task<bool> RemoveRangeAsync(IEnumerable<T> entities);
        Task<bool> IsExistAsync(Guid? id);
        T GetById(int id);
        Task<T> GetByIdAsync(int id);
        T GetById(string code);
        Task<T> GetByIdAsync(string code);
        T GetById(Guid code);
        Task<T> GetByIdAsync(Guid code);

        Task<T> GetByAsync(string type, string value);

        Task<T> GetByIdIncludedAsync(Guid id, params Expression<Func<T, object>>[] includes);
        Task<T?> GetAsync(
    Expression<Func<T, bool>> predicate,
    Func<IQueryable<T>, IQueryable<T>>? include = null);
        Task<List<T>?> GetManyAsync(
    Expression<Func<T, bool>> predicate,
    Func<IQueryable<T>, IQueryable<T>>? include = null,
    bool asNoTracking = false,
    Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
    int? take = null);
        int Save();
        Task UpdateRangeAsync(IEnumerable<T> e);
        Task<int> SaveAsync();
        Task CreateRangeAsync(IEnumerable<T> entities);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    }
}
