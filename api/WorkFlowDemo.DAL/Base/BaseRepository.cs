using SqlSugar;
using System.Linq.Expressions;

namespace WorkFlowDemo.DAL.Base
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class, new()
    {
        protected readonly ISqlSugarClient _db;

        public BaseRepository(ISqlSugarClient db)
        {
            _db = db;
        }

        public virtual async Task<List<T>> GetListAsync()
        {
            return await _db.Queryable<T>().ToListAsync();
        }

        public virtual async Task<List<T>> GetListAsync(Expression<Func<T, bool>> where)
        {
            return await _db.Queryable<T>().Where(where).ToListAsync();
        }

        public virtual async Task<T> GetFirstAsync(Expression<Func<T, bool>> where)
        {
            return await _db.Queryable<T>().FirstAsync(where);
        }

        public virtual async Task<T> GetByIdAsync(int id)
        {
            return await _db.Queryable<T>().InSingleAsync(id);
        }

        public virtual async Task<int> AddAsync(T entity)
        {
            return await _db.Insertable(entity).ExecuteReturnIdentityAsync();
        }

        public virtual async Task<bool> UpdateAsync(T entity)
        {
            return await _db.Updateable(entity).ExecuteCommandHasChangeAsync();
        }

        public virtual async Task<bool> DeleteAsync(int id)
        {
            return await _db.Deleteable<T>().In(id).ExecuteCommandHasChangeAsync();
        }
    }
}