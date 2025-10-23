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
    /// 检验库存活动
    /// </summary>
    [Activity("MaterialOutbound", "检验库存", "检查物料库存是否充足")]
    public class CheckInventoryActivity : CodeActivity<bool>
    {
        [Input(Description = "出库详细列表")]
        public Input<List<MaterialOutboundDetailDto>> Details { get; set; } = default!;

        protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var materialRepository = context.GetRequiredService<IMaterialRepository>();
            var logger = context.GetRequiredService<ILogger<CheckInventoryActivity>>();
            var details = Details.Get(context);

            logger.LogInformation("开始检验库存，物料数量: {Count}", details?.Count ?? 0);

            try
            {
                if (details == null || !details.Any())
                {
                    logger.LogWarning("出库详细列表为空");
                    context.Set(Result, false);
                    return;
                }

                // 检查每个物料的库存
                foreach (var detail in details)
                {
                    var inventory = await materialRepository.GetInventoryByMaterialCodeAsync(detail.MaterialCode);

                    if (inventory == null || inventory.Qty < detail.Qty)
                    {
                        logger.LogWarning("物料 {MaterialCode} 库存不足，需要: {Required}, 实际: {Actual}",
                            detail.MaterialCode, detail.Qty, inventory?.Qty ?? 0);
                        context.Set(Result, false);
                        return;
                    }
                }

                logger.LogInformation("库存检验通过");
                context.Set(Result, true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "检验库存失败");
                throw;
            }
        }
    }
}