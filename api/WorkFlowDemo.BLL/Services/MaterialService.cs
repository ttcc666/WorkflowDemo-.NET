using WorkFlowDemo.BLL.Base;
using WorkFlowDemo.DAL.Repositories;
using WorkFlowDemo.Models.Dtos;
using WorkFlowDemo.Models.Entities;

namespace WorkFlowDemo.BLL.Services
{
    public class MaterialService : BaseService<Material>, IMaterialService
    {
        private readonly IMaterialRepository _materialRepository;
        private readonly IMaterialTemporaryScanRepository _materialTemporaryScanRepository;
        private readonly IMaterialInventoryRepository _materialInventoryRepository;

        public MaterialService(IMaterialRepository materialRepository, IMaterialTemporaryScanRepository materialTemporaryScanRepository, IMaterialInventoryRepository materialInventoryRepository) : base(materialRepository)
        {
            _materialRepository = materialRepository;
            _materialTemporaryScanRepository = materialTemporaryScanRepository;
            _materialInventoryRepository = materialInventoryRepository;
        }

        public async Task<ValueTuple<bool, string>> ScanAndSaveAsync(MaterialTemporaryScan scan)
        {
            var material = await _materialRepository.GetFirstAsync(it => it.MaterialCode == scan.MaterialCode);
            if (material == null)
            {
                return (false, "Material code not found.");
            }

            scan.Id = Guid.NewGuid().ToString();
            scan.OperationTime = DateTime.Now;
            scan.BatchNumber = string.IsNullOrEmpty(scan.BatchNumber) ? Guid.NewGuid().ToString().Substring(0, 8).ToUpper() : scan.BatchNumber;
            await _materialTemporaryScanRepository.AddAsync(scan);

            return (true, scan.BatchNumber);
        }

        public async Task<ValueTuple<bool, string>> CompleteScanAsync(MaterialTemporaryScanComplete complete)
        {
            var scans = await _materialTemporaryScanRepository.GetListAsync(it => it.BatchNumber == complete.BatchNo);
            if (scans == null || !scans.Any())
            {
                return (false, "No scans found for the provided batch number.");
            }

            // Here you can add logic to mark the scans as completed or move them to another table if needed.

            return (true, "Scan completed successfully.");
        }

        public async Task<List<Material>> GetMaterialsAsync()
        {
            return await _materialRepository.GetListAsync();
        }

        public async Task<bool> AddMaterialAsync(Material material)
        {
            material.Id = Guid.NewGuid().ToString();
            await _materialRepository.AddAsync(material);
            return true;
        }

        public async Task<List<MaterialInventory>> GetInventoriesAsync()
        {
            return await _materialInventoryRepository.GetListAsync();
        }

        public async Task<bool> AddInventoryAsync(MaterialInventory inventory)
        {
            var existing = await _materialInventoryRepository.GetFirstAsync(it => it.MaterialCode == inventory.MaterialCode);
            if (existing != null)
            {
                existing.Qty += inventory.Qty;
                existing.UpdatedTime = DateTime.Now;
                return await _materialInventoryRepository.UpdateAsync(existing);
            }

            inventory.Id = Guid.NewGuid().ToString();
            inventory.CreatedTime = DateTime.Now;
            inventory.UpdatedTime = DateTime.Now;
            await _materialInventoryRepository.AddAsync(inventory);
            return true;
        }

        public Task<string> GenerateBatchNumberAsync()
        {
            return Task.FromResult(DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
        }

        public async Task<ValueTuple<bool, string>> ScanItemAsync(MaterialTemporaryScan scan)
        {
            var material = await _materialRepository.GetFirstAsync(it => it.MaterialCode == scan.MaterialCode);
            if (material == null)
            {
                return (false, "物料代码不存在");
            }

            // 检查同一批次中是否已存在相同物料代码的记录
            var existing = await _materialTemporaryScanRepository.GetFirstAsync(it =>
                it.BatchNumber == scan.BatchNumber &&
                it.MaterialCode == scan.MaterialCode);

            if (existing != null)
            {
                // 如果存在，累加数量
                existing.Qty += scan.Qty;
                existing.OperationTime = DateTime.Now;
                await _materialTemporaryScanRepository.UpdateAsync(existing);
                return (true, $"扫描成功，已累加数量至 {existing.Qty}");
            }

            // 如果不存在，创建新记录
            scan.Id = Guid.NewGuid().ToString();
            scan.OperationTime = DateTime.Now;
            scan.Operator = "System";
            await _materialTemporaryScanRepository.AddAsync(scan);

            return (true, "扫描成功");
        }
    }
}