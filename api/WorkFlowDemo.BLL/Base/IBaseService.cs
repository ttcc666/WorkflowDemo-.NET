using System.Linq.Expressions;

namespace WorkFlowDemo.BLL.Base
{
    public interface IBaseService<T> where T : class, new()
    {
        Task<List<T>> GetListAsync();
        Task<T> GetFirstAsync(Expression<Func<T, bool>> where);
        Task<T> GetByIdAsync(int id);
        Task<int> AddAsync(T entity);
        Task<bool> UpdateAsync(T entity);
        Task<bool> DeleteAsync(int id);
    }
}