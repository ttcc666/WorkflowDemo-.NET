using System.Linq.Expressions;

namespace WorkFlowDemo.DAL.Base
{
    public interface IBaseRepository<T> where T : class, new()
    {
        Task<List<T>> GetListAsync();
        Task<List<T>> GetListAsync(Expression<Func<T, bool>> where);
        Task<T> GetFirstAsync(Expression<Func<T, bool>> where);
        Task<T> GetByIdAsync(int id);
        Task<int> AddAsync(T entity);
        Task<bool> UpdateAsync(T entity);
        Task<bool> DeleteAsync(int id);
    }
}