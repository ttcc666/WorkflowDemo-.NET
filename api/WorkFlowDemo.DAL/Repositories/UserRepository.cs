using SqlSugar;
using WorkFlowDemo.DAL.Base;
using WorkFlowDemo.Models.Entities;

namespace WorkFlowDemo.DAL.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(ISqlSugarClient db) : base(db)
        {
        }
    }
}