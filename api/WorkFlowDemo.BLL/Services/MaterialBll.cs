using WorkFlowDemo.BLL.Base;
using WorkFlowDemo.DAL.Repositories;
using WorkFlowDemo.Models.Dtos;
using WorkFlowDemo.Models.Entities;

namespace WorkFlowDemo.BLL.Services
{
    public class MaterialBll : BaseService<Material>
    {
        private readonly MaterialDal _materialDal;
        private readonly MaterialTemporaryScanDal _materialTemporaryScanDal;
        private readonly MaterialInventoryDal _materialInventoryDal;

        public MaterialBll(MaterialDal materialDal, MaterialTemporaryScanDal materialTemporaryScanDal, MaterialInventoryDal materialInventoryDal) : base(materialDal)
        {
            _materialDal = materialDal;
            _materialTemporaryScanDal = materialTemporaryScanDal;
            _materialInventoryDal = materialInventoryDal;
        }

        public async Task<ValueTuple<bool, string>> ScanAndSaveAsync(MaterialTemporaryScan scan)
        {
            var material = await _materialDal.GetFirstAsync(it => it.MaterialCode == scan.MaterialCode);
            if (material == null)
            {
                return (false, "Material code not found.");
            }

            scan.Id = Guid.NewGuid().ToString();
            scan.OperationTime = DateTime.Now;
            scan.BatchNumber = string.IsNullOrEmpty(scan.BatchNumber) ? Guid.NewGuid().ToString().Substring(0, 8).ToUpper() : scan.BatchNumber;
            await _materialTemporaryScanDal.AddAsync(scan);

            return (true, scan.BatchNumber);
        }

        public async Task<ValueTuple<bool, string>> CompleteScanAsync(MaterialTemporaryScanComplete complete)
        {
            var scans = await _materialTemporaryScanDal.GetListAsync(it => it.BatchNumber == complete.BatchNo);
            if (scans == null || !scans.Any())
            {
                return (false, "No scans found for the provided batch number.");
            }

            // Here you can add logic to mark the scans as completed or move them to another table if needed.

            return (true, "Scan completed successfully.");
        }

        public async Task<List<Material>> GetMaterialsAsync()
        {
            return await _materialDal.GetListAsync();
        }

        public async Task<bool> AddMaterialAsync(Material material)
        {
            material.Id = Guid.NewGuid().ToString();
            await _materialDal.AddAsync(material);
            return true;
        }

        public async Task<List<MaterialInventory>> GetInventoriesAsync()
        {
            return await _materialInventoryDal.GetListAsync();
        }

        public async Task<bool> AddInventoryAsync(MaterialInventory inventory)
        {
            var existing = await _materialInventoryDal.GetFirstAsync(it => it.MaterialCode == inventory.MaterialCode);
            if (existing != null)
            {
                existing.Qty += inventory.Qty;
                existing.UpdatedTime = DateTime.Now;
                return await _materialInventoryDal.UpdateAsync(existing);
            }
            
            inventory.Id = Guid.NewGuid().ToString();
            inventory.CreatedTime = DateTime.Now;
            inventory.UpdatedTime = DateTime.Now;
            await _materialInventoryDal.AddAsync(inventory);
            return true;
        }

        public Task<string> GenerateBatchNumberAsync()
        {
            return Task.FromResult(DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
        }

        public async Task<ValueTuple<bool, string>> ScanItemAsync(MaterialTemporaryScan scan)
        {
            var material = await _materialDal.GetFirstAsync(it => it.MaterialCode == scan.MaterialCode);
            if (material == null)
            {
                return (false, "物料代码不存在");
            }

            scan.Id = Guid.NewGuid().ToString();
            scan.OperationTime = DateTime.Now;
            scan.Operator = "System";
            await _materialTemporaryScanDal.AddAsync(scan);

            return (true, "扫描成功");
        }
    }
}