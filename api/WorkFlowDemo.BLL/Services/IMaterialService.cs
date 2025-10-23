using WorkFlowDemo.BLL.Base;
using WorkFlowDemo.Models.Dtos;
using WorkFlowDemo.Models.Entities;

namespace WorkFlowDemo.BLL.Services
{
    public interface IMaterialService : IBaseService<Material>
    {
        Task<ValueTuple<bool, string>> ScanAndSaveAsync(MaterialTemporaryScan scan);
        Task<ValueTuple<bool, string>> CompleteScanAsync(MaterialTemporaryScanComplete complete);
        Task<List<Material>> GetMaterialsAsync();
        Task<bool> AddMaterialAsync(Material material);
        Task<List<MaterialInventory>> GetInventoriesAsync();
        Task<bool> AddInventoryAsync(MaterialInventory inventory);
        Task<string> GenerateBatchNumberAsync();
        Task<ValueTuple<bool, string>> ScanItemAsync(MaterialTemporaryScan scan);
    }
}