using SqlSugar;
using WorkFlowDemo.DAL.Base;
using WorkFlowDemo.Models.Entities;

namespace WorkFlowDemo.DAL.Repositories
{
    public class MaterialInventoryDal : BaseRepository<MaterialInventory>
    {
        public MaterialInventoryDal(ISqlSugarClient db) : base(db)
        {
        }
    }
}