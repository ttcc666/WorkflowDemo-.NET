using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Microsoft.Extensions.Logging;
using WorkFlowDemo.BLL.Activities.Common;
using WorkFlowDemo.DAL.Repositories;
using WorkFlowDemo.Models.Dtos;

namespace WorkFlowDemo.BLL.Activities.MaterialOutbound
{
    [Activity("MaterialOutbound", "检验库存", "检查物料库存是否充足")]
    public class CheckInventoryActivity : BaseActivity<bool>
    {
        [Input(Description = "出库详细列表")]
        public Input<List<MaterialOutboundDetailDto>> Details { get; set; } = default!;

        protected override async ValueTask<bool> ExecuteActivityAsync(ActivityExecutionContext context, ILogger logger)
        {
            var details = Details.Get(context);
            if (details?.Any() != true)
            {
                logger.LogWarning("出库详细列表为空");
                return false;
            }

            var repository = context.GetRequiredService<IMaterialRepository>();
            foreach (var detail in details)
            {
                var inventory = await repository.GetInventoryByMaterialCodeAsync(detail.MaterialCode);
                if (inventory?.Qty < detail.Qty)
                {
                    logger.LogWarning("物料 {Code} 库存不足，需要: {Required}, 实际: {Actual}",
                        detail.MaterialCode, detail.Qty, inventory?.Qty ?? 0);
                    return false;
                }
            }

            return true;
        }
    }
}