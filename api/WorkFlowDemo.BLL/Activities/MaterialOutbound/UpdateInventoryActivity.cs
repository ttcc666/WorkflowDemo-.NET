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
    /// 更新库存活动
    /// </summary>
    [Activity("MaterialOutbound", "更新库存", "扣减物料库存")]
    public class UpdateInventoryActivity : CodeActivity<bool>
    {
        [Input(Description = "出库详细列表")]
        public Input<List<MaterialOutboundDetailDto>> Details { get; set; } = default!;

        protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var materialDal = context.GetRequiredService<MaterialDal>();
            var logger = context.GetRequiredService<ILogger<UpdateInventoryActivity>>();
            var details = Details.Get(context);
            
            logger.LogInformation("开始更新库存，物料数量: {Count}", details?.Count ?? 0);

            try
            {
                foreach (var detail in details ?? new List<MaterialOutboundDetailDto>())
                {
                    var success = await materialDal.UpdateInventoryAsync(detail.MaterialCode, detail.Qty);
                    
                    if (!success)
                    {
                        logger.LogError("更新库存失败，物料: {MaterialCode}", detail.MaterialCode);
                        context.Set(Result, false);
                        return;
                    }
                }

                logger.LogInformation("成功更新库存");
                context.Set(Result, true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "更新库存失败");
                throw;
            }
        }
    }
}