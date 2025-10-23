using SqlSugar;
using WorkFlowDemo.DAL.Base;
using WorkFlowDemo.Models.Entities;

namespace WorkFlowDemo.DAL.Repositories
{
    public class MaterialTemporaryScanRepository : BaseRepository<MaterialTemporaryScan>, IMaterialTemporaryScanRepository
    {
        public MaterialTemporaryScanRepository(ISqlSugarClient db) : base(db)
        {
        }

        public async Task<List<MaterialTemporaryScan>> GetByBatchNumberAsync(string batchNumber)
        {
            return await _db.Queryable<MaterialTemporaryScan>()
                .Where(x => x.BatchNumber == batchNumber)
                .ToListAsync();
        }

        public async Task<bool> DeleteByBatchNumberAsync(string batchNumber)
        {
            return await _db.Deleteable<MaterialTemporaryScan>()
                .Where(x => x.BatchNumber == batchNumber)
                .ExecuteCommandAsync() > 0;
        }
    }
}