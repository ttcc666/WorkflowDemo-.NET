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
    [Activity("MaterialOutbound", "回滚库存", "恢复已扣减的库存")]
    public class RollbackInventoryActivity : BaseActivity<bool>
    {
        [Input(Description = "出库详细列表")]
        public Input<List<MaterialOutboundDetailDto>> Details { get; set; } = default!;

        protected override async ValueTask<bool> ExecuteActivityAsync(ActivityExecutionContext context, ILogger logger)
        {
            var details = Details.Get(context);
            if (details?.Any() != true) return true;

            var repository = context.GetRequiredService<IMaterialRepository>();
            foreach (var detail in details)
            {
                if (!await repository.RollbackInventoryAsync(detail.MaterialCode, detail.Qty))
                {
                    logger.LogError("回滚库存失败，物料: {Code}", detail.MaterialCode);
                    return false;
                }
            }
            return true;
        }
    }
}