using System.Linq.Expressions;

namespace Web_Stadium.EFCore
{
    // Interface định nghĩa các phương thức CRUD cơ bản cho repository
    // T là kiểu dữ liệu bất kỳ (ví dụ: SanBong, DatSan, etc.)
    public  interface IRepository<T> where T : class
    {
        // lấy một bản ghi theo id
        Task<T?> GetByIdAsync(object id);

        // lấy tất cả bản ghi
        Task<IEnumerable<T>> GetAllAsync();
        // tìm kiếm bản ghi theo điều kiện
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        // lấy bản ghi đầu tiên thỏa mãn điều kiện
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        // thêm mới bản ghi
        Task<T> AddAsync(T entity);
        // cập nhật bản ghi
        Task UpdateAsync(T entity);
        // xóa bản ghi theo id
        Task DeleteAsync(object id);
        // lấy dữ liệu phân trang
        Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
            int page, 
            int pageSize,
            Expression<Func<T, bool>>? predicate = null,
            Expression<Func<T, object>>? orderBy = null,
            bool descending = false);
    }
}
