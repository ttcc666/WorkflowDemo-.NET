using Elsa.Http;
using Elsa.Workflows;
using Elsa.Workflows.Activities;
using WorkFlowDemo.BLL.Activities.OrderProcessing;
using WorkFlowDemo.BLL.Activities.Common;
using WorkFlowDemo.Models.Dtos;

namespace WorkFlowDemo.BLL.Workflows.OrderProcessing
{
    /// <summary>
    /// 订单处理工作流
    /// </summary>
    public class OrderProcessingWorkflow : WorkflowBase
    {
        protected override void Build(IWorkflowBuilder builder)
        {
            var orderInput = builder.WithVariable<CreateOrderDto>();
            var discountedAmount = builder.WithVariable<decimal>();
            var paymentId = builder.WithVariable<string>();
            var shipmentId = builder.WithVariable<string>();
            var points = builder.WithVariable<int>();
            var resultMessage = builder.WithVariable<string>();

            builder.Root = new Sequence
            {
                Activities =
                {
                    new HttpEndpoint
                    {
                        Path = new("/order-processing"),
                        SupportedMethods = new(new[] { "POST" }),
                        CanStartWorkflow = true,
                        ParsedContent = new(orderInput)
                    },
                    new LogWorkflowStatusActivity
                    {
                        StepName = new("开始"),
                        StatusMessage = new("订单处理工作流已启动")
                    },
                    new ReserveInventoryActivity
                    {
                        ProductId = new(context => orderInput.Get(context)?.ProductId ?? string.Empty),
                        Quantity = new(context => orderInput.Get(context)?.Quantity ?? 0)
                    },
                    new LogWorkflowStatusActivity
                    {
                        StepName = new("库存预留"),
                        StatusMessage = new("库存预留完成")
                    },
                    new ApplyCouponActivity
                    {
                        CouponCode = new(context => orderInput.Get(context)?.CouponCode ?? string.Empty),
                        OrderAmount = new(context => orderInput.Get(context)?.OrderAmount ?? 0),
                        Result = new(discountedAmount)
                    },
                    new LogWorkflowStatusActivity
                    {
                        StepName = new("优惠券核销"),
                        StatusMessage = new(context => $"优惠券核销完成, 最终金额: {discountedAmount.Get(context):C}")
                    },
                    new ProcessPaymentActivity
                    {
                        OrderAmount = new(discountedAmount),
                        UserId = new(context => orderInput.Get(context)?.UserId ?? string.Empty),
                        Result = new(paymentId)
                    },
                    new LogWorkflowStatusActivity
                    {
                        StepName = new("支付扣款"),
                        StatusMessage = new(context => $"支付扣款完成, PaymentId: {paymentId.Get(context)}")
                    },
                    new CreateShipmentActivity
                    {
                        ProductId = new(context => orderInput.Get(context)?.ProductId ?? string.Empty),
                        Quantity = new(context => orderInput.Get(context)?.Quantity ?? 0),
                        UserId = new(context => orderInput.Get(context)?.UserId ?? string.Empty),
                        Result = new(shipmentId)
                    },
                    new LogWorkflowStatusActivity
                    {
                        StepName = new("创建发货单"),
                        StatusMessage = new(context => $"发货单创建完成, ShipmentId: {shipmentId.Get(context)}")
                    },
                    new AwardPointsActivity
                    {
                        OrderAmount = new(discountedAmount),
                        UserId = new(context => orderInput.Get(context)?.UserId ?? string.Empty),
                        Result = new(points)
                    },
                    new LogWorkflowStatusActivity
                    {
                        StepName = new("积分发放"),
                        StatusMessage = new(context => $"积分发放完成, Points: {points.Get(context)}")
                    },
                    new SetVariable
                    {
                        Variable = resultMessage,
                        Value = new(context =>
                        {
                            var payment = paymentId.Get(context);
                            var shipment = shipmentId.Get(context);
                            var pts = points.Get(context);
                            var amount = discountedAmount.Get(context);
                            return $"订单处理完成! PaymentId: {payment}, ShipmentId: {shipment}, Points: {pts}, FinalAmount: {amount:C}";
                        })
                    },
                    new WriteHttpResponse
                    {
                        Content = new(resultMessage)
                    }
                }
            };
        }
    }
}