using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Microsoft.Extensions.Logging;
using WorkFlowDemo.BLL.Services;
using WorkFlowDemo.Models.Entities;

namespace WorkFlowDemo.BLL.Activities.Common
{
    [Activity("WorkflowDemo", "Common", "记录工作流状态")]
    public class LogWorkflowStatusActivity : CodeActivity
    {
        [Input(Description = "步骤名称")]
        public Input<string> StepName { get; set; } = default!;

        [Input(Description = "状态消息")]
        public Input<string> StatusMessage { get; set; } = default!;

        [Input(Description = "节点序号")]
        public Input<int> StepOrder { get; set; } = default!;

        [Input(Description = "工作流名称")]
        public Input<string> WorkflowName { get; set; } = default!;

        [Input(Description = "执行状态")]
        public Input<string> ExecutionStatus { get; set; } = new("Running");

        [Input(Description = "批次号")]
        public Input<string> BatchNumber { get; set; } = default!;

        [Input(Description = "操作人")]
        public Input<string> Operator { get; set; } = default!;

        [Input(Description = "错误信息")]
        public Input<string> ErrorMessage { get; set; } = default!;

        [Input(Description = "是否需要审批")]
        public Input<bool> RequiresApproval { get; set; } = default!;

        protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var logger = context.GetRequiredService<ILogger<LogWorkflowStatusActivity>>();
            var logService = context.GetRequiredService<IWorkflowExecutionLogService>();

            var stepName = StepName.GetOrDefault(context) ?? "未知";
            var statusMessage = StatusMessage.GetOrDefault(context) ?? "无";
            var workflowInstanceId = context.WorkflowExecutionContext.Id;

            // 控制台日志
            logger.LogInformation("工作流状态 [Id: {Id}] - 步骤: {Step}, 状态: {Status}",
                workflowInstanceId,
                stepName,
                statusMessage);

            // 数据库日志
            var workflowName = WorkflowName.GetOrDefault(context) ?? context.WorkflowExecutionContext.Workflow.GetType().Name;
            var activityTypeName = context.Activity.GetType().Name;
            
            var log = new WorkflowExecutionLog
            {
                Id = Guid.NewGuid().ToString(),
                WorkflowInstanceId = workflowInstanceId,
                WorkflowDefinitionName = workflowName,
                ActivityId = context.Activity.Id,
                ActivityName = activityTypeName,
                StepName = stepName,
                StepOrder = StepOrder.GetOrDefault(context),
                StatusMessage = statusMessage,
                ExecutionStatus = ExecutionStatus.GetOrDefault(context) ?? "Running",
                ErrorMessage = ErrorMessage.GetOrDefault(context),
                BatchNumber = BatchNumber.GetOrDefault(context),
                Operator = Operator.GetOrDefault(context),
                RequiresApproval = RequiresApproval.GetOrDefault(context),
                CreatedTime = DateTime.Now
            };

            await logService.LogAsync(log);
        }
    }
}