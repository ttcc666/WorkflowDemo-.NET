using WorkFlowDemo.DAL.Base;
using WorkFlowDemo.Models.Entities;

namespace WorkFlowDemo.DAL.Repositories
{
    public interface IMaterialRepository : IBaseRepository<Material>
    {
        Task<MaterialInventory?> GetInventoryByMaterialCodeAsync(string materialCode);
        Task<bool> InsertHistoryAsync(MaterialHistory history);
        Task<bool> UpdateInventoryAsync(string materialCode, decimal qty);
        Task<bool> DeleteHistoryByIdsAsync(List<string> historyIds);
        Task<bool> RollbackInventoryAsync(string materialCode, decimal qty);
    }
}