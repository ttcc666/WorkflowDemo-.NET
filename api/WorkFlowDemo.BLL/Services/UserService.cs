using WorkFlowDemo.BLL.Base;
using WorkFlowDemo.DAL.Repositories;
using WorkFlowDemo.Models.Entities;

namespace WorkFlowDemo.BLL.Services
{
    public class UserService : BaseService<User>, IUserService
    {
        public UserService(IUserRepository userRepository) : base(userRepository)
        {
        }
    }
}