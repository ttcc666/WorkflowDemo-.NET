using SqlSugar;
using WorkFlowDemo.DAL.Base;
using WorkFlowDemo.Models.Entities;

namespace WorkFlowDemo.DAL.Repositories
{
    public class MaterialInventoryRepository : BaseRepository<MaterialInventory>, IMaterialInventoryRepository
    {
        public MaterialInventoryRepository(ISqlSugarClient db) : base(db)
        {
        }
    }
}