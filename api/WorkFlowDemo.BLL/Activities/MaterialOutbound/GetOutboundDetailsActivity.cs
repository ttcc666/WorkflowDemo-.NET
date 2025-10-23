using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Microsoft.Extensions.Logging;
using WorkFlowDemo.DAL.Repositories;
using WorkFlowDemo.Models.Dtos;

namespace WorkFlowDemo.BLL.Activities.MaterialOutbound
{
    /// <summary>
    /// 获取出库详细信息活动
    /// </summary>
    [Activity("MaterialOutbound", "获取出库详细", "根据批次号获取出库详细信息")]
    public class GetOutboundDetailsActivity : CodeActivity<List<MaterialOutboundDetailDto>>
    {
        [Input(Description = "批次号")]
        public Input<string> BatchNumber { get; set; } = default!;

        protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var scanRepository = context.GetRequiredService<IMaterialTemporaryScanRepository>();
            var logger = context.GetRequiredService<ILogger<GetOutboundDetailsActivity>>();
            var batchNumber = BatchNumber.Get(context);

            logger.LogInformation("开始获取出库详细信息，批次号: {BatchNumber}", batchNumber);

            try
            {
                // 从临时扫描表获取出库详细
                var scanRecords = await scanRepository.GetByBatchNumberAsync(batchNumber);

                if (scanRecords == null || !scanRecords.Any())
                {
                    logger.LogWarning("未找到批次号 {BatchNumber} 的扫描记录", batchNumber);
                    context.Set(Result, new List<MaterialOutboundDetailDto>());
                    return;
                }

                var details = scanRecords.Select(s => new MaterialOutboundDetailDto
                {
                    MaterialCode = s.MaterialCode,
                    Qty = s.Qty
                }).ToList();

                logger.LogInformation("成功获取出库详细信息，批次号: {BatchNumber}, 记录数: {Count}",
                    batchNumber, details.Count);

                context.Set(Result, details);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "获取出库详细信息失败，批次号: {BatchNumber}", batchNumber);
                throw;
            }
        }
    }
}