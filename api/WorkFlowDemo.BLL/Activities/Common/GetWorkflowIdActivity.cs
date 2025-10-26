using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;

namespace WorkFlowDemo.BLL.Activities.Common
{
    [Activity("WorkflowDemo", "Common", "获取当前工作流实例ID")]
    public class GetWorkflowIdActivity : CodeActivity<string>
    {
        protected override void Execute(ActivityExecutionContext context)
        {
            var workflowInstanceId = context.WorkflowExecutionContext.Id;
            context.SetResult(workflowInstanceId);
        }
    }
}