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
    /// 回滚库存活动
    /// </summary>
    [Activity("MaterialOutbound", "回滚库存", "恢复已扣减的库存")]
    public class RollbackInventoryActivity : CodeActivity<bool>
    {
        [Input(Description = "出库详细列表")]
        public Input<List<MaterialOutboundDetailDto>> Details { get; set; } = default!;

        protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var materialDal = context.GetRequiredService<MaterialDal>();
            var logger = context.GetRequiredService<ILogger<RollbackInventoryActivity>>();
            var details = Details.Get(context);
            
            logger.LogInformation("开始回滚库存，物料数量: {Count}", details?.Count ?? 0);

            try
            {
                if (details == null || !details.Any())
                {
                    logger.LogWarning("出库详细列表为空，无需回滚");
                    context.Set(Result, true);
                    return;
                }

                foreach (var detail in details)
                {
                    var success = await materialDal.RollbackInventoryAsync(detail.MaterialCode, detail.Qty);
                    
                    if (!success)
                    {
                        logger.LogError("回滚库存失败，物料: {MaterialCode}", detail.MaterialCode);
                        context.Set(Result, false);
                        return;
                    }
                }

                logger.LogInformation("成功回滚库存");
                context.Set(Result, true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "回滚库存失败");
                throw;
            }
        }
    }
}