using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Microsoft.Extensions.Logging;
using WorkFlowDemo.BLL.Activities.Common;
using WorkFlowDemo.DAL.Repositories;

namespace WorkFlowDemo.BLL.Activities.MaterialOutbound
{
    [Activity("MaterialOutbound", "删除扫描记录", "删除临时扫描记录")]
    public class DeleteScanRecordsActivity : BaseActivity<bool>
    {
        [Input(Description = "批次号")]
        public Input<string> BatchNumber { get; set; } = default!;

        protected override async ValueTask<bool> ExecuteActivityAsync(ActivityExecutionContext context, ILogger logger)
        {
            var batchNumber = BatchNumber.Get(context);
            var repository = context.GetRequiredService<IMaterialTemporaryScanRepository>();
            return await repository.DeleteByBatchNumberAsync(batchNumber);
        }
    }
}