using Elsa.Http;
using Elsa.Workflows;
using Elsa.Workflows.Activities;
using WorkFlowDemo.BLL.Activities.OrderProcessing;
using WorkFlowDemo.BLL.Activities.Common;
using WorkFlowDemo.BLL.Activities.Compensation;
using WorkFlowDemo.Models.Dtos;

namespace WorkFlowDemo.BLL.Workflows.OrderProcessing
{
    /// <summary>
    /// 带人工干预补偿机制的订单处理工作流
    /// 当发生错误时,需要人工审批是否执行补偿
    /// </summary>
    public class OrderProcessingWithManualCompensationWorkflow : WorkflowBase
    {
        protected override void Build(IWorkflowBuilder builder)
        {
            var orderInput = builder.WithVariable<CreateOrderDto>();
            var discountedAmount = builder.WithVariable<decimal>();
            var paymentId = builder.WithVariable<string>();
            var shipmentId = builder.WithVariable<string>();
            var points = builder.WithVariable<int>();
            var inventoryReserved = builder.WithVariable<bool>();
            var paymentProcessed = builder.WithVariable<bool>();
            var shipmentCreated = builder.WithVariable<bool>();
            var shouldSimulateFailure = builder.WithVariable<bool>();
            var hasError = builder.WithVariable<bool>();
            var manualApproval = builder.WithVariable<bool>();
            var resultMessage = builder.WithVariable<string>();

            builder.Root = new Sequence
            {
                Activities =
                {
                    new HttpEndpoint
                    {
                        Path = new("/order-processing-manual"),
                        SupportedMethods = new(new[] { "POST" }),
                        CanStartWorkflow = true,
                        ParsedContent = new(orderInput)
                    },
                    new LogWorkflowStatusActivity
                    {
                        StepName = new("开始"),
                        StatusMessage = new("订单处理工作流已启动(带人工干预补偿)")
                    },
                    
                    // 检查是否需要模拟失败
                    new SetVariable
                    {
                        Variable = shouldSimulateFailure,
                        Value = new(context => (orderInput.Get(context)?.OrderAmount ?? 0) > 1000)
                    },
                    
                    // 步骤1: 库存预留
                    new ReserveInventoryActivity
                    {
                        ProductId = new(context => orderInput.Get(context)?.ProductId ?? string.Empty),
                        Quantity = new(context => orderInput.Get(context)?.Quantity ?? 0)
                    },
                    new SetVariable
                    {
                        Variable = inventoryReserved,
                        Value = new(true)
                    },
                    new LogWorkflowStatusActivity
                    {
                        StepName = new("库存预留"),
                        StatusMessage = new("✓ 库存预留完成")
                    },
                    
                    // 步骤2: 优惠券核销
                    new ApplyCouponActivity
                    {
                        CouponCode = new(context => orderInput.Get(context)?.CouponCode ?? string.Empty),
                        OrderAmount = new(context => orderInput.Get(context)?.OrderAmount ?? 0),
                        Result = new(discountedAmount)
                    },
                    new LogWorkflowStatusActivity
                    {
                        StepName = new("优惠券核销"),
                        StatusMessage = new(context => $"✓ 优惠券核销完成, 最终金额: {discountedAmount.Get(context):C}")
                    },
                    
                    // 步骤3: 支付扣款
                    new ProcessPaymentActivity
                    {
                        OrderAmount = new(discountedAmount),
                        UserId = new(context => orderInput.Get(context)?.UserId ?? string.Empty),
                        Result = new(paymentId)
                    },
                    new SetVariable
                    {
                        Variable = paymentProcessed,
                        Value = new(true)
                    },
                    new LogWorkflowStatusActivity
                    {
                        StepName = new("支付扣款"),
                        StatusMessage = new(context => $"✓ 支付扣款完成, PaymentId: {paymentId.Get(context)}")
                    },
                    
                    // 模拟失败点
                    new If
                    {
                        Condition = new(shouldSimulateFailure),
                        Then = new Sequence
                        {
                            Activities =
                            {
                                new SetVariable
                                {
                                    Variable = hasError,
                                    Value = new(true)
                                },
                                new LogWorkflowStatusActivity
                                {
                                    StepName = new("模拟失败"),
                                    StatusMessage = new("⚠️ 模拟发货单创建失败 - 等待人工审批补偿")
                                }
                            }
                        },
                        Else = new Sequence
                        {
                            Activities =
                            {
                                // 步骤4: 创建发货单
                                new CreateShipmentActivity
                                {
                                    ProductId = new(context => orderInput.Get(context)?.ProductId ?? string.Empty),
                                    Quantity = new(context => orderInput.Get(context)?.Quantity ?? 0),
                                    UserId = new(context => orderInput.Get(context)?.UserId ?? string.Empty),
                                    Result = new(shipmentId)
                                },
                                new SetVariable
                                {
                                    Variable = shipmentCreated,
                                    Value = new(true)
                                },
                                new LogWorkflowStatusActivity
                                {
                                    StepName = new("创建发货单"),
                                    StatusMessage = new(context => $"✓ 发货单创建完成, ShipmentId: {shipmentId.Get(context)}")
                                },
                                
                                // 步骤5: 积分发放
                                new AwardPointsActivity
                                {
                                    OrderAmount = new(discountedAmount),
                                    UserId = new(context => orderInput.Get(context)?.UserId ?? string.Empty),
                                    Result = new(points)
                                },
                                new LogWorkflowStatusActivity
                                {
                                    StepName = new("积分发放"),
                                    StatusMessage = new(context => $"✓ 积分发放完成, Points: {points.Get(context)}")
                                }
                            }
                        }
                    },
                    
                    // 如果有错误,等待人工审批
                    new If
                    {
                        Condition = new(hasError),
                        Then = new Sequence
                        {
                            Activities =
                            {
                                new LogWorkflowStatusActivity
                                {
                                    StepName = new("等待审批"),
                                    StatusMessage = new("⏸️ 工作流已暂停,等待人工审批是否执行补偿...")
                                },
                                
                                // 人工审批端点 - 等待外部调用
                                // 使用固定路径,工作流ID通过查询参数传递
                                new HttpEndpoint
                                {
                                    Path = new("/approve-compensation"),
                                    SupportedMethods = new(new[] { "POST" }),
                                    CanStartWorkflow = false,
                                    ParsedContent = new(manualApproval)
                                },
                                
                                new LogWorkflowStatusActivity
                                {
                                    StepName = new("审批结果"),
                                    StatusMessage = new(context => 
                                        manualApproval.Get(context) 
                                            ? "✅ 审批通过 - 开始执行补偿" 
                                            : "❌ 审批拒绝 - 不执行补偿")
                                },
                                
                                // 根据审批结果决定是否补偿
                                new If
                                {
                                    Condition = new(manualApproval),
                                    Then = new Sequence
                                    {
                                        Activities =
                                        {
                                            new LogWorkflowStatusActivity
                                            {
                                                StepName = new("开始补偿"),
                                                StatusMessage = new("🔄 开始执行补偿流程...")
                                            },
                                            
                                            // 补偿1: 取消发货单
                                            new If
                                            {
                                                Condition = new(shipmentCreated),
                                                Then = new Sequence
                                                {
                                                    Activities =
                                                    {
                                                        new CancelShipmentActivity
                                                        {
                                                            ShipmentId = new(shipmentId)
                                                        },
                                                        new LogWorkflowStatusActivity
                                                        {
                                                            StepName = new("补偿-取消发货单"),
                                                            StatusMessage = new(context => $"↩️ 发货单已取消: {shipmentId.Get(context)}")
                                                        }
                                                    }
                                                }
                                            },
                                            
                                            // 补偿2: 退款
                                            new If
                                            {
                                                Condition = new(paymentProcessed),
                                                Then = new Sequence
                                                {
                                                    Activities =
                                                    {
                                                        new RefundPaymentActivity
                                                        {
                                                            PaymentId = new(paymentId),
                                                            Amount = new(discountedAmount)
                                                        },
                                                        new LogWorkflowStatusActivity
                                                        {
                                                            StepName = new("补偿-退款"),
                                                            StatusMessage = new(context => $"↩️ 支付已退款: {paymentId.Get(context)}, 金额: {discountedAmount.Get(context):C}")
                                                        }
                                                    }
                                                }
                                            },
                                            
                                            // 补偿3: 释放库存
                                            new If
                                            {
                                                Condition = new(inventoryReserved),
                                                Then = new Sequence
                                                {
                                                    Activities =
                                                    {
                                                        new RollbackInventoryActivity
                                                        {
                                                            ProductId = new(context => orderInput.Get(context)?.ProductId ?? string.Empty),
                                                            Quantity = new(context => orderInput.Get(context)?.Quantity ?? 0)
                                                        },
                                                        new LogWorkflowStatusActivity
                                                        {
                                                            StepName = new("补偿-释放库存"),
                                                            StatusMessage = new(context =>
                                                            {
                                                                var input = orderInput.Get(context);
                                                                return $"↩️ 库存已释放: ProductId={input?.ProductId ?? "N/A"}, Quantity={input?.Quantity ?? 0}";
                                                            })
                                                        }
                                                    }
                                                }
                                            },
                                            
                                            new LogWorkflowStatusActivity
                                            {
                                                StepName = new("补偿完成"),
                                                StatusMessage = new("✅ 所有补偿操作已完成")
                                            },
                                            
                                            new SetVariable
                                            {
                                                Variable = resultMessage,
                                                Value = new("❌ 订单处理失败! 经人工审批后,所有操作已回滚")
                                            }
                                        }
                                    },
                                    Else = new Sequence
                                    {
                                        Activities =
                                        {
                                            new SetVariable
                                            {
                                                Variable = resultMessage,
                                                Value = new("⚠️ 订单处理失败! 人工审批拒绝补偿,请手动处理")
                                            }
                                        }
                                    }
                                },
                                
                                new WriteHttpResponse
                                {
                                    Content = new(resultMessage)
                                }
                            }
                        },
                        Else = new Sequence
                        {
                            Activities =
                            {
                                // 成功响应
                                new SetVariable
                                {
                                    Variable = resultMessage,
                                    Value = new(context =>
                                    {
                                        var payment = paymentId.Get(context);
                                        var shipment = shipmentId.Get(context);
                                        var pts = points.Get(context);
                                        var amount = discountedAmount.Get(context);
                                        return $"✅ 订单处理完成! PaymentId: {payment}, ShipmentId: {shipment}, Points: {pts}, FinalAmount: {amount:C}";
                                    })
                                },
                                new WriteHttpResponse
                                {
                                    Content = new(resultMessage)
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}