using SqlSugar;
using WorkFlowDemo.DAL.Base;
using WorkFlowDemo.Models.Entities;

namespace WorkFlowDemo.DAL.Repositories
{
    public class UserDal : BaseRepository<User>
    {
        public UserDal(ISqlSugarClient db) : base(db)
        {
        }
    }
}