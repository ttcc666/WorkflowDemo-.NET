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

        public async Task<MaterialInventory?> GetInventoryByMaterialCodeAsync(string materialCode)
        {
            return await _db.Queryable<MaterialInventory>()
                .Where(x => x.MaterialCode == materialCode)
                .FirstAsync();
        }

        public async Task<bool> InsertHistoryAsync(MaterialHistory history)
        {
            return await _db.Insertable(history).ExecuteCommandAsync() > 0;
        }

        public async Task<bool> UpdateInventoryAsync(string materialCode, decimal qty)
        {
            return await _db.Updateable<MaterialInventory>()
                .SetColumns(x => x.Qty == x.Qty - qty)
                .SetColumns(x => x.UpdatedTime == DateTime.Now)
                .Where(x => x.MaterialCode == materialCode)
                .ExecuteCommandAsync() > 0;
        }

        public async Task<bool> DeleteHistoryByIdsAsync(List<string> historyIds)
        {
            return await _db.Deleteable<MaterialHistory>()
                .Where(x => historyIds.Contains(x.Id))
                .ExecuteCommandAsync() > 0;
        }

        public async Task<bool> RollbackInventoryAsync(string materialCode, decimal qty)
        {
            return await _db.Updateable<MaterialInventory>()
                .SetColumns(x => x.Qty == x.Qty + qty)
                .SetColumns(x => x.UpdatedTime == DateTime.Now)
                .Where(x => x.MaterialCode == materialCode)
                .ExecuteCommandAsync() > 0;
        }
    }
}