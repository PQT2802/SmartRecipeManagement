using Microsoft.EntityFrameworkCore;
using SmartRecipe.Infrastructure.DB;
using System.Linq.Expressions;

namespace SmartRecipe.Infrastructure.Repo.Generic
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly SmartRecipeDbContext _context;

        public GenericRepository(SmartRecipeDbContext context)
        {
            _context = context; // Ensures that context is initialized
        }

        public List<T> GetAll()
        {
            return _context.Set<T>().ToList();
        }

        public async Task<List<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public void Create(T entity)
        {
            _context.Add(entity);
            //   _context.SaveChanges();
        }

        public async Task<int> CreateAsync(T entity)
        {
            _context.Add(entity);
            return await _context.SaveChangesAsync();
        }

        public void Update(T entity)
        {
            var tracker = _context.Attach(entity);
            tracker.State = EntityState.Modified;
            _context.SaveChanges();
        }

        public async Task<int> UpdateAsync(T entity)
        {
            var tracker = _context.Attach(entity);
            tracker.State = EntityState.Modified;
            return await _context.SaveChangesAsync();
        }

        public bool Remove(T entity)
        {
            _context.Remove(entity);
            _context.SaveChanges();
            return true;
        }

        public async Task<bool> RemoveAsync(T entity)
        {
            _context.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> RemoveRangeAsync(IEnumerable<T> entities)
        {
            _context.Set<T>().RemoveRange(entities);
            await _context.SaveChangesAsync();
            return true;
        }

        public T GetById(int id)
        {
            return _context.Set<T>().Find(id);
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public T GetById(string code)
        {
            return _context.Set<T>().Find(code);
        }

        public async Task<T> GetByIdAsync(string code)
        {
            return await _context.Set<T>().FindAsync(code);
        }

        public T GetById(Guid code)
        {
            return _context.Set<T>().Find(code);
        }

        public async Task<T> GetByIdAsync(Guid code)
        {
            return await _context.Set<T>().FindAsync(code);
        }
        public async Task<T> GetByIdIncludedAsync(Guid id, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _context.Set<T>().AsNoTracking();
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return await query.FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id);
        }
        public int Save()
        {
            return _context.SaveChanges();
        }

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<T> GetByAsync(string type, string value)
        {
            return await _context.Set<T>()
                .FirstOrDefaultAsync(x => EF.Property<string>(x, type) == value);
        }

        public async Task<bool> IsExistAsync(Guid? id)
        {
            if (!id.HasValue)
                return false;

            return await _context.Set<T>().AnyAsync(e => EF.Property<Guid>(e, "Id") == id.Value);
        }
        public async Task<T?> GetAsync(
    Expression<Func<T, bool>> predicate,
    Func<IQueryable<T>, IQueryable<T>>? include = null)
        {
            IQueryable<T> query = _context.Set<T>();

            if (include != null)
            {
                query = include(query);
            }

            return await query.FirstOrDefaultAsync(predicate);
        }
        public async Task<List<T>?> GetManyAsync(
    Expression<Func<T, bool>> predicate,
    Func<IQueryable<T>, IQueryable<T>>? include = null,
    bool asNoTracking = false,
    Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
    int? take = null)
        {
            IQueryable<T> query = _context.Set<T>();

            if (asNoTracking)
                query = query.AsNoTracking();

            if (include != null)
                query = include(query);

            query = query.Where(predicate);

            if (orderBy != null)
                query = orderBy(query);

            if (take.HasValue)
                query = query.Take(take.Value);

            return await query.ToListAsync();
        }
        public async Task UpdateRangeAsync(IEnumerable<T> e)
        {
            _context.Set<T>().UpdateRange(e);
            await _context.SaveChangesAsync();
        }
        public async Task CreateRangeAsync(IEnumerable<T> entities)
        {
            await _context.Set<T>().AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().AnyAsync(predicate);
        }
    }
}
