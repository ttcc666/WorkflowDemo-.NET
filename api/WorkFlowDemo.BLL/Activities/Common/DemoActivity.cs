using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Activities.Flowchart.Attributes;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Microsoft.Extensions.DependencyInjection;
using WorkFlowDemo.BLL.Services;
using WorkFlowDemo.Models.Dtos;

namespace WorkFlowDemo.BLL.Activities
{
    [FlowNode("true", "false")]
    public class DemoActivity : CodeActivity
    {
        [Input(Description = "The condition to evaluate.")]
        public Input<CreatePostWorkflowDto> Condition { get; set; } = default!;
        public IActivity? Then { get; set; }
        public IActivity? Else { get; set; }
        protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var userBll = context.GetRequiredService<UserBll>();
            var condition = Condition.Get<CreatePostWorkflowDto>(context);
            var user = await userBll.GetFirstAsync(u => u.Name == condition.Name);
            var result = user != null;

            var nextActivity = result ? Then : Else;

            if (nextActivity != null)
            {
                await context.ScheduleActivityAsync(nextActivity);
            }
        }
    }
}