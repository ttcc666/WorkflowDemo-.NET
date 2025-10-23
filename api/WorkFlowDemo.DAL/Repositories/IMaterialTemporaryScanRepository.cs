using WorkFlowDemo.DAL.Base;
using WorkFlowDemo.Models.Entities;

namespace WorkFlowDemo.DAL.Repositories
{
    public interface IMaterialTemporaryScanRepository : IBaseRepository<MaterialTemporaryScan>
    {
        Task<List<MaterialTemporaryScan>> GetByBatchNumberAsync(string batchNumber);
        Task<bool> DeleteByBatchNumberAsync(string batchNumber);
    }
}