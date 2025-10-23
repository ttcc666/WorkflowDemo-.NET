using Elsa.Http;
using Elsa.Workflows;
using Elsa.Workflows.Activities;
using WorkFlowDemo.BLL.Activities.Common;
using WorkFlowDemo.Models.Dtos;

namespace WorkFlowDemo.BLL.Workflows.Demo
{
    public class PostDemoWorkflow : WorkflowBase
    {
        protected override void Build(IWorkflowBuilder builder)
        {
            var postedDataVariable = builder.WithVariable<CreatePostWorkflowDto>();
            var messageVariable = builder.WithVariable<string>();

            builder.Root = new Sequence
            {
                Activities =
                {
                    new HttpEndpoint
                    {
                        Path = new("/post-workflow"),
                        SupportedMethods = new(new[] { "POST" }),
                        CanStartWorkflow = true,
                        ParsedContent = new(postedDataVariable)
                    },
                    new DemoActivity
                    {
                        Condition = new(postedDataVariable),
                        Then = new SetVariable
                        {
                            Variable = messageVariable,
                            Value = new(context =>
                            {
                                var postedData = postedDataVariable.Get(context);
                                var name = postedData?.Name ?? "Anonymous";
                                var receivedMessage = postedData?.Message ?? "nothing";
                                return $"Hello, {name}! Your message was: '{receivedMessage}'";
                            })
                        },
                        Else = new SetVariable
                        {
                            Variable = messageVariable,
                            Value = new("你不是Elsa")
                        }
                    },
                    new WriteHttpResponse
                    {
                        Content = new(messageVariable)
                    }
                }
            };
        }
    }
}