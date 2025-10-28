using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using WorkFlowDemo.BLL.Services;

namespace WorkFlowDemo.BLL.Activities.Common
{
    [Activity("WorkflowDemo", "Common", "更新日志审批状态")]
    public class UpdateLogApprovalStatusActivity : CodeActivity
    {
        [Input(Description = "批次号")]
        public Input<string> BatchNumber { get; set; } = default!;

        protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var logService = context.GetRequiredService<IWorkflowExecutionLogService>();
            var batchNumber = BatchNumber.Get(context);

            if (string.IsNullOrEmpty(batchNumber))
                return;

            var logs = await logService.GetByBatchNumberAsync(batchNumber);
            var approvalLog = logs.FirstOrDefault(l => l.RequiresApproval && l.ExecutionStatus == "Pending");

            if (approvalLog != null)
            {
                approvalLog.RequiresApproval = false;
                approvalLog.ExecutionStatus = "Completed";
                await logService.UpdateAsync(approvalLog);
            }
        }
    }
}