using Elsa.Workflows;
using Elsa.Workflows.Activities;
using Elsa.Workflows.Memory;

namespace WorkFlowDemo.BLL.Services.Demo
{
    public class WorkflowService : IWorkflowService
    {
        private readonly IWorkflowRunner workflowRunner;
       
        public WorkflowService(IWorkflowRunner workflowRunner)
        {
            this.workflowRunner = workflowRunner;
        }
        public async Task DemoWorkflowAsync(int a,int b)
        {
            // 定义工作流变量变量
            var numA = new Variable("A",a);
            var numB = new Variable("B", b);

            // 创建工作流实例
            var workflow = new Workflow
            {
                // 定义工作流变量
                Variables = { numA, numB },
                Root = new Sequence
                {
                    Activities =
                    {
                        new WriteLine($"number A is: {{A}}"),
                        new WriteLine($"number B is: {{B}}"),
                    }
                }
            };
           await workflowRunner.RunAsync(workflow);
        }
    }

}