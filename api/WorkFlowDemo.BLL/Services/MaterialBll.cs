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

        public MaterialBll(MaterialDal materialDal, MaterialTemporaryScanDal materialTemporaryScanDal) : base(materialDal)
        {
            _materialDal = materialDal;
            _materialTemporaryScanDal = materialTemporaryScanDal;
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
    }
}