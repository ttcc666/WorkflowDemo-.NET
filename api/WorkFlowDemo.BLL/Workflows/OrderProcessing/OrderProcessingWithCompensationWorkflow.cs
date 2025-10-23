using Elsa.Http;
using Elsa.Workflows;
using Elsa.Workflows.Activities;
using WorkFlowDemo.BLL.Activities;
using WorkFlowDemo.Models.Dtos;

namespace WorkFlowDemo.BLL.Workflows
{
    /// <summary>
    /// 带补偿机制的订单处理工作流
    /// 使用Fault处理和补偿逻辑
    /// 当订单金额>1000时会模拟失败并触发补偿
    /// </summary>
    public class OrderProcessingWithCompensationWorkflow : WorkflowBase
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
            var resultMessage = builder.WithVariable<string>();

            builder.Root = new Sequence
            {
                Activities =
                {
                    new HttpEndpoint
                    {
                        Path = new("/order-processing-safe"),
                        SupportedMethods = new(new[] { "POST" }),
                        CanStartWorkflow = true,
                        ParsedContent = new(orderInput)
                    },
                    new LogWorkflowStatusActivity
                    {
                        StepName = new("开始"),
                        StatusMessage = new("订单处理工作流已启动(带补偿机制)")
                    },
                    
                    // 检查是否需要模拟失败(订单金额>1000时模拟失败)
                    new SetVariable
                    {
                        Variable = shouldSimulateFailure,
                        Value = new(context => orderInput.Get(context).OrderAmount > 1000)
                    },
                    
                    // 步骤1: 库存预留
                    new ReserveInventoryActivity
                    {
                        ProductId = new(context => orderInput.Get(context).ProductId),
                        Quantity = new(context => orderInput.Get(context).Quantity)
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
                        CouponCode = new(context => orderInput.Get(context).CouponCode),
                        OrderAmount = new(context => orderInput.Get(context).OrderAmount),
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
                        UserId = new(context => orderInput.Get(context).UserId),
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
                    
                    // 模拟失败点 - 在创建发货单之前检查
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
                                    StatusMessage = new("⚠️ 模拟发货单创建失败 - 触发补偿机制")
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
                                    ProductId = new(context => orderInput.Get(context).ProductId),
                                    Quantity = new(context => orderInput.Get(context).Quantity),
                                    UserId = new(context => orderInput.Get(context).UserId),
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
                                    UserId = new(context => orderInput.Get(context).UserId),
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
                    
                    // 根据是否有错误执行不同的逻辑
                    new If
                    {
                        Condition = new(hasError),
                        Then = new Sequence
                        {
                            Activities =
                            {
                                new LogWorkflowStatusActivity
                                {
                                    StepName = new("开始补偿"),
                                    StatusMessage = new("🔄 开始执行补偿流程...")
                                },
                                
                                // 补偿1: 取消发货单(如果已创建)
                                new If
                                {
                                    Condition = new(context => shipmentCreated.Get(context)),
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
                                
                                // 补偿2: 退款(如果支付已处理)
                                new If
                                {
                                    Condition = new(context => paymentProcessed.Get(context)),
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
                                
                                // 补偿3: 释放库存(如果库存已预留)
                                new If
                                {
                                    Condition = new(context => inventoryReserved.Get(context)),
                                    Then = new Sequence
                                    {
                                        Activities =
                                        {
                                            new RollbackInventoryActivity
                                            {
                                                ProductId = new(context => orderInput.Get(context).ProductId),
                                                Quantity = new(context => orderInput.Get(context).Quantity)
                                            },
                                            new LogWorkflowStatusActivity
                                            {
                                                StepName = new("补偿-释放库存"),
                                                StatusMessage = new(context => 
                                                {
                                                    var input = orderInput.Get(context);
                                                    return $"↩️ 库存已释放: ProductId={input.ProductId}, Quantity={input.Quantity}";
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
                                
                                // 错误响应
                                new SetVariable
                                {
                                    Variable = resultMessage,
                                    Value = new("❌ 订单处理失败! 所有操作已回滚。原因: 发货单创建失败(订单金额>1000触发模拟失败)")
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