using WorkFlowDemo.BLL.Base;
using WorkFlowDemo.DAL.Repositories;
using WorkFlowDemo.Models.Entities;

namespace WorkFlowDemo.BLL.Services
{
    public class UserBll : BaseService<User>
    {
        public UserBll(UserDal userDal) : base(userDal)
        {
        }
    }
}