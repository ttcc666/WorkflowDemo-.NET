using Elsa.Workflows;
using Elsa.Workflows.Activities;
using Elsa.Workflows.Memory;
using Elsa.Workflows.Models;
using WorkFlowDemo.BLL.Activities.MaterialOutbound;
using WorkFlowDemo.BLL.Activities.Common;
using WorkFlowDemo.Models.Dtos;

namespace WorkFlowDemo.BLL.Services.Demo
{
    public class MaterialOutBoundWorkflowService : IMaterialOutBoundWorkflowService
    {
        private readonly IWorkflowRunner _workflowRunner;
        private const string WorkflowName = "MaterialOutBoundWorkflow";
        public MaterialOutBoundWorkflowService(IWorkflowRunner workflowRunner)
        {
            _workflowRunner = workflowRunner;
        }
        public async Task<RunWorkflowResult> StartMaterialOutBoundWorkflowAsync(string materialOutBatchNo)
        {

            // 定义工作流变量
            var materialOutBatchNoVar = new Variable<string>("MaterialOutBatchNo", materialOutBatchNo);

            // 出库详情
            var MaterialOutboundDetails = new Variable<List<MaterialOutboundDetailDto>>("MaterialOutboundDetails", new List<MaterialOutboundDetailDto>());

            // 库存校验
            var IsInventorySufficient = new Variable<KeyValuePair<bool, string>>("IsInventorySufficient", new KeyValuePair<bool, string>(false, string.Empty));

            // 库存更新结果
            var InventoryUpdateResult = new Variable<bool>("InventoryUpdateResult", false);

            // 履历记录ID列表
            var HistoryIds = new Variable<List<string>>("HistoryIds", new List<string>());

            // 扫描记录删除结果
            var ScanRecordsDeleteResult = new Variable<bool>("ScanRecordsDeleteResult", false);

            // 操作人
            var Operator = new Variable<string>("Operator", "System");

            // 创建工作流实例
            var workflow = new Workflow
            {
                Name = WorkflowName,
                // 定义工作流变量
                Variables = { materialOutBatchNoVar, MaterialOutboundDetails, IsInventorySufficient, InventoryUpdateResult, HistoryIds, ScanRecordsDeleteResult, Operator },
                Root = new Sequence
                {
                    Activities =
                    {
                        // 1 获取单据明细
                        new GetOutboundDetailsActivity
                        {
                            BatchNumber = new Input<string>(materialOutBatchNoVar),
                            Result = new Output<List<MaterialOutboundDetailDto>>(MaterialOutboundDetails)
                        },
                        // 记录明细
                        new LogWorkflowStatusActivity
                        {
                            StepName = new Input<string>("获取出库明细"),
                            StatusMessage = new Input<string>(Context=> $"批次号 {materialOutBatchNoVar.Get(Context)} 的出库明细：{string.Join(", ", MaterialOutboundDetails.Get(Context).Select(d=> $"物料编码: {d.MaterialCode}, 数量: {d.Qty}"))}")
                        },

                        // 2 校验库存
                        new CheckInventoryActivity
                        {
                            Details = new Input<List<MaterialOutboundDetailDto>>(MaterialOutboundDetails),
                            Result = new Output<bool>(InventoryUpdateResult)
                        },
                        new If
                        {
                            Condition = new Input<bool>(Context => InventoryUpdateResult.Get(Context)),
                            Then = new Sequence
                            {
                                Activities =
                                {
                                    new LogWorkflowStatusActivity
                                    {
                                        StepName = new Input<string>("库存校验"),
                                        StatusMessage = new Input<string>("库存充足，继续执行出库流程。")
                                    },
                                    
                                    // 3. 更新库存
                                    new UpdateInventoryActivity
                                    {
                                        Details = new Input<List<MaterialOutboundDetailDto>>(MaterialOutboundDetails),
                                        Result = new Output<bool>(InventoryUpdateResult)
                                    },

                                    new LogWorkflowStatusActivity
                                    {
                                        StepName = new Input<string>("更新库存"),
                                        StatusMessage = new Input<string>(Context=> $"库存更新结果：{InventoryUpdateResult.Get(Context)}")
                                    },
                                    
                                    // 检查库存更新是否成功
                                    new If
                                    {
                                        Condition = new Input<bool>(Context => InventoryUpdateResult.Get(Context)),
                                        Then = new Sequence
                                        {
                                            Activities =
                                            {
                                                new LogWorkflowStatusActivity
                                                {
                                                    StepName = new Input<string>("更新库存"),
                                                    StatusMessage = new Input<string>("库存更新成功，正在写入履历记录。")
                                                },
                                                
                                                // 4. 写入履历
                                                new WriteHistoryActivity
                                                {
                                                    BatchNumber = new Input<string>(materialOutBatchNoVar),
                                                    Details = new Input<List<MaterialOutboundDetailDto>>(MaterialOutboundDetails),
                                                    Operator = new Input<string>(Operator),
                                                    Result = new Output<List<string>>(HistoryIds)
                                                },

                                                new LogWorkflowStatusActivity
                                                {
                                                    StepName = new Input<string>("写入履历"),
                                                    StatusMessage = new Input<string>(Context=> $"履历记录已创建：{string.Join(", ", HistoryIds.Get(Context))}")
                                                },
                                                
                                                // 检查履历是否创建成功
                                                new If
                                                {
                                                    Condition = new Input<bool>(Context => HistoryIds.Get(Context) != null && HistoryIds.Get(Context).Count > 0),
                                                    Then = new Sequence
                                                    {
                                                        Activities =
                                                        {
                                                            new LogWorkflowStatusActivity
                                                            {
                                                                StepName = new Input<string>("写入履历"),
                                                                StatusMessage = new Input<string>("履历记录创建成功，正在删除扫描记录。")
                                                            },
                                                            
                                                            // 5. 删除扫描记录
                                                            new DeleteScanRecordsActivity
                                                            {
                                                                BatchNumber = new Input<string>(materialOutBatchNoVar),
                                                                Result = new Output<bool>(ScanRecordsDeleteResult)
                                                            },

                                                            new LogWorkflowStatusActivity
                                                            {
                                                                StepName = new Input<string>("删除扫描记录"),
                                                                StatusMessage = new Input<string>(Context=> $"扫描记录删除结果：{ScanRecordsDeleteResult.Get(Context)}")
                                                            },
                                                            
                                                            // 检查扫描记录删除是否成功
                                                            new If
                                                            {
                                                                Condition = new Input<bool>(Context => ScanRecordsDeleteResult.Get(Context)),
                                                                Then = new LogWorkflowStatusActivity
                                                                {
                                                                    StepName = new Input<string>("流程完成"),
                                                                    StatusMessage = new Input<string>("物料出库流程已成功完成。")
                                                                },
                                                                Else = new Sequence
                                                                {
                                                                    Activities =
                                                                    {
                                                                        new LogWorkflowStatusActivity
                                                                        {
                                                                            StepName = new Input<string>("删除扫描记录"),
                                                                            StatusMessage = new Input<string>("扫描记录删除失败，但出库流程已完成。")
                                                                        },
                                                                        new LogWorkflowStatusActivity
                                                                        {
                                                                            StepName = new Input<string>("删除扫描记录"),
                                                                            StatusMessage = new Input<string>("可能需要手动清理扫描记录。")
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    },
                                                    Else = new Sequence
                                                    {
                                                        Activities =
                                                        {
                                                            new LogWorkflowStatusActivity
                                                            {
                                                                StepName = new Input<string>("写入履历"),
                                                                StatusMessage = new Input<string>("履历记录创建失败，正在回滚库存变更。")
                                                            },
                                                            
                                                            // 回滚库存
                                                            new RollbackInventoryActivity
                                                            {
                                                                Details = new Input<List<MaterialOutboundDetailDto>>(MaterialOutboundDetails)
                                                            },

                                                            new LogWorkflowStatusActivity
                                                            {
                                                                StepName = new Input<string>("回滚库存"),
                                                                StatusMessage = new Input<string>("库存回滚完成。由于履历创建失败，流程已终止。")
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        },
                                        Else = new Sequence
                                        {
                                            Activities =
                                            {
                                                new LogWorkflowStatusActivity
                                                {
                                                    StepName = new Input<string>("更新库存"),
                                                    StatusMessage = new Input<string>("库存更新失败，流程已终止。")
                                                },
                                                new LogWorkflowStatusActivity
                                                {
                                                    StepName = new Input<string>("更新库存"),
                                                    StatusMessage = new Input<string>("无需回滚，因为库存未被修改。")
                                                }
                                            }
                                        }
                                    }
                                }
                            },
                            Else = new Sequence
                            {
                                Activities =
                                {
                                    new LogWorkflowStatusActivity
                                    {
                                        StepName = new Input<string>("校验库存"),
                                        StatusMessage = new Input<string>("库存不足，流程已终止。")
                                    },
                                    new LogWorkflowStatusActivity
                                    {
                                        StepName = new Input<string>("校验库存"),
                                        StatusMessage = new Input<string>("需要人工介入解决库存问题。")
                                    },
                                    new LogWorkflowStatusActivity
                                    {
                                        StepName = new Input<string>("校验库存"),
                                        StatusMessage = new Input<string>("流程结束，未对库存或履历进行任何更改。")
                                    }
                                }
                            }
                        },
                        
                    
                    // 最终步骤：记录工作流完成状态
                    new LogWorkflowStatusActivity
                    {
                        StepName = new Input<string>("工作流完成"),
                        StatusMessage = new Input<string>(Context=> $"批次 {materialOutBatchNoVar.Get(Context)} 的物料出库工作流已处理完成。")
                    },
                    new LogWorkflowStatusActivity
                    {
                        StepName = new Input<string>("工作流完成"),
                        StatusMessage = new Input<string>(Context=> $"工作流执行完成时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                    }
                    },


                }
            };
            return await _workflowRunner.RunAsync(workflow);
        }
    }
}