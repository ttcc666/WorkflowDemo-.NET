using Elsa.Http;
using Elsa.Scheduling.Activities;
using Elsa.Workflows;
using Elsa.Workflows.Activities;
using WorkFlowDemo.BLL.Activities;
using WorkFlowDemo.Models.Dtos;

namespace ECommerceWorkflows;

/// <summary>
/// 复杂的电商订单处理工作流
/// 场景：用户下单 -> 库存预留 -> 优惠券核销 -> 支付扣款 -> 发货 -> 积分发放
/// 任何环节失败都会触发补偿机制，回滚之前的所有操作
/// </summary>
public class ComplexOrderWorkflow : WorkflowBase
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
                    Path = new("/complex-order-workflow"),
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
                        Value = new("你是Elsa")
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


