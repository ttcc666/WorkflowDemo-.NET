using System.Linq.Expressions;
using WorkFlowDemo.DAL.Base;

namespace WorkFlowDemo.BLL.Base
{
    public class BaseService<T> : IBaseService<T> where T : class, new()
    {
        protected readonly IBaseRepository<T> _repository;

        public BaseService(IBaseRepository<T> repository)
        {
            _repository = repository;
        }

        public virtual async Task<List<T>> GetListAsync()
        {
            return await _repository.GetListAsync();
        }

        public virtual async Task<T> GetFirstAsync(Expression<Func<T, bool>> where)
        {
            return await _repository.GetFirstAsync(where);
        }

        public virtual async Task<T> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public virtual async Task<int> AddAsync(T entity)
        {
            return await _repository.AddAsync(entity);
        }

        public virtual async Task<bool> UpdateAsync(T entity)
        {
            return await _repository.UpdateAsync(entity);
        }

        public virtual async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }
    }
}