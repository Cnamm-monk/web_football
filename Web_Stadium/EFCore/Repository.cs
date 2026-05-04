using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Web_Stadium.End;

namespace Web_Stadium.EFCore
{
    // Lớp thực thi IRepository - dùng EF Core để truy vấn database
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly SanBongContext _context;
        protected readonly DbSet<T> _dbSet;

        //DI injecct SanBongContext vào Repository
        public Repository(SanBongContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }
        public async Task<T?> GetByIdAsync(object id)
            => await _dbSet.FindAsync(id);
        public async Task<IEnumerable<T>> GetAllAsync()
            => await _dbSet.ToListAsync();
        public async Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>> predicate)
            => await _dbSet.Where(predicate).ToListAsync();
        public async Task<T?> FirstOrDefaultAsync(
            Expression<Func<T, bool>> predicate)
            => await _dbSet.FirstOrDefaultAsync(predicate);
        public async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        public async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(object id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(int page, int pageSize,
            Expression<Func<T, bool>>? filter = null,
            Expression<Func<T, object>>? orderBy = null,
            bool descending = false)
        {
            IQueryable<T> query = _dbSet;
            if (filter != null)
                query = query.Where(filter);
            var  totalCount = await query.CountAsync();
            if (orderBy != null)
                query = descending 
                    ? query.OrderByDescending(orderBy) 
                    : query.OrderBy(orderBy);
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (items, totalCount);
        }

    }
}
