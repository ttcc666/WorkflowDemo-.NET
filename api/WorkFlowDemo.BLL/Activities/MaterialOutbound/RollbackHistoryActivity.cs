using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Microsoft.Extensions.Logging;
using WorkFlowDemo.BLL.Activities.Common;
using WorkFlowDemo.DAL.Repositories;

namespace WorkFlowDemo.BLL.Activities.MaterialOutbound
{
    [Activity("MaterialOutbound", "回滚履历", "删除已写入的履历记录")]
    public class RollbackHistoryActivity : BaseActivity<bool>
    {
        [Input(Description = "履历ID列表")]
        public Input<List<string>> HistoryIds { get; set; } = default!;

        protected override async ValueTask<bool> ExecuteActivityAsync(ActivityExecutionContext context, ILogger logger)
        {
            var historyIds = HistoryIds.Get(context);
            if (historyIds?.Any() != true) return true;

            var repository = context.GetRequiredService<IMaterialRepository>();
            return await repository.DeleteHistoryByIdsAsync(historyIds);
        }
    }
}