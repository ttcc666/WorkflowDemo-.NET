using SqlSugar;
using WorkFlowDemo.DAL.Base;
using WorkFlowDemo.Models.Entities;

namespace WorkFlowDemo.DAL.Repositories
{
    public class MaterialDal : BaseRepository<Material>
    {
        public MaterialDal(ISqlSugarClient db) : base(db)
        {
        }
    }
}