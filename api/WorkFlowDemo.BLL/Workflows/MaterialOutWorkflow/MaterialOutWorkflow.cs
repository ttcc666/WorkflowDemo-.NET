using Elsa.Http;
using Elsa.Workflows;
using Elsa.Workflows.Activities;
using WorkFlowDemo.BLL.Activities.Common;
using WorkFlowDemo.BLL.Activities.MaterialOutbound;
using WorkFlowDemo.Models.Dtos;

namespace WorkFlowDemo.BLL.Workflows.MaterialOutWorkflow
{
    /// <summary>
    /// 物料出库工作流 - 包含补偿机制
    /// </summary>
    public class MaterialOutWorkflow : WorkflowBase
    {
        protected override void Build(IWorkflowBuilder builder)
        {
            var inputVar = builder.WithVariable<MaterialOutboundDto>();
            var detailsVar = builder.WithVariable<List<MaterialOutboundDetailDto>>();
            var historyIdsVar = builder.WithVariable<List<string>>();
            var checkResultVar = builder.WithVariable<bool>();
            var updateResultVar = builder.WithVariable<bool>();
            var deleteResultVar = builder.WithVariable<bool>();
            var resultVar = builder.WithVariable<string>();

            builder.Root = new Sequence
            {
                Activities =
                {
                    new HttpEndpoint
                    {
                        Path = new("/material-outbound"),
                        SupportedMethods = new(new[] { "POST" }),
                        CanStartWorkflow = true,
                        ParsedContent = new(inputVar)
                    },
                    
                    new LogWorkflowStatusActivity
                    {
                        StepName = new("开始"),
                        StatusMessage = new("物料出库流程启动")
                    },

                    // 步骤1: 获取出库详细
                    new GetOutboundDetailsActivity
                    {
                        BatchNumber = new(context => inputVar.Get(context)?.BatchNumber ?? string.Empty),
                        Result = new(detailsVar)
                    },

                    // 步骤2: 检验库存
                    new CheckInventoryActivity
                    {
                        Details = new(detailsVar),
                        Result = new(checkResultVar)
                    },

                    new If(context => !checkResultVar.Get(context))
                    {
                        Then = new Sequence
                        {
                            Activities =
                            {
                                new LogWorkflowStatusActivity
                                {
                                    StepName = new("失败"),
                                    StatusMessage = new("库存不足，流程终止")
                                },
                                new SetVariable { Variable = resultVar, Value = new("❌ 库存不足") },
                                new WriteHttpResponse { Content = new(resultVar) }
                            }
                        }
                    },

                    // 步骤3: 写入履历
                    new WriteHistoryActivity
                    {
                        BatchNumber = new(context => inputVar.Get(context)?.BatchNumber ?? string.Empty),
                        Details = new(detailsVar),
                        Operator = new(context => inputVar.Get(context)?.Operator ?? string.Empty),
                        Result = new(historyIdsVar)
                    },

                    new LogWorkflowStatusActivity
                    {
                        StepName = new("履历"),
                        StatusMessage = new("✓ 履历写入成功")
                    },

                    // 步骤4: 更新库存
                    new UpdateInventoryActivity
                    {
                        Details = new(detailsVar),
                        Result = new(updateResultVar)
                    },

                    // 如果更新库存失败，执行补偿：回滚履历
                    new If(context => !updateResultVar.Get(context))
                    {
                        Then = new Sequence
                        {
                            Activities =
                            {
                                new LogWorkflowStatusActivity
                                {
                                    StepName = new("补偿"),
                                    StatusMessage = new("⚠️ 更新库存失败，开始回滚履历")
                                },
                                new RollbackHistoryActivity
                                {
                                    HistoryIds = new(historyIdsVar)
                                },
                                new LogWorkflowStatusActivity
                                {
                                    StepName = new("补偿完成"),
                                    StatusMessage = new("↩️ 履历已回滚")
                                },
                                new SetVariable { Variable = resultVar, Value = new("❌ 更新库存失败，已回滚履历") },
                                new WriteHttpResponse { Content = new(resultVar) }
                            }
                        }
                    },

                    new LogWorkflowStatusActivity
                    {
                        StepName = new("库存"),
                        StatusMessage = new("✓ 库存更新成功")
                    },

                    // 步骤5: 删除扫描记录
                    new DeleteScanRecordsActivity
                    {
                        BatchNumber = new(context => inputVar.Get(context)?.BatchNumber ?? string.Empty),
                        Result = new(deleteResultVar)
                    },

                    // 如果删除扫描记录失败，执行补偿：回滚库存和履历
                    new If(context => !deleteResultVar.Get(context))
                    {
                        Then = new Sequence
                        {
                            Activities =
                            {
                                new LogWorkflowStatusActivity
                                {
                                    StepName = new("补偿"),
                                    StatusMessage = new("⚠️ 删除扫描记录失败，开始回滚库存和履历")
                                },
                                new RollbackInventoryActivity
                                {
                                    Details = new(detailsVar)
                                },
                                new RollbackHistoryActivity
                                {
                                    HistoryIds = new(historyIdsVar)
                                },
                                new LogWorkflowStatusActivity
                                {
                                    StepName = new("补偿完成"),
                                    StatusMessage = new("↩️ 库存和履历已回滚")
                                },
                                new SetVariable { Variable = resultVar, Value = new("❌ 删除扫描记录失败，已回滚所有操作") },
                                new WriteHttpResponse { Content = new(resultVar) }
                            }
                        }
                    },

                    // 成功完成
                    new LogWorkflowStatusActivity
                    {
                        StepName = new("完成"),
                        StatusMessage = new("✅ 物料出库流程成功完成")
                    },

                    new SetVariable { Variable = resultVar, Value = new("✅ 物料出库成功") },
                    new WriteHttpResponse { Content = new(resultVar) }
                }
            };
        }
    }
}