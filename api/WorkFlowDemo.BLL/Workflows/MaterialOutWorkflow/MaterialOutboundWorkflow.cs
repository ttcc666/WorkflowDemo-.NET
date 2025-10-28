using Elsa.Http;
using Elsa.Workflows;
using Elsa.Workflows.Activities;
using WorkFlowDemo.BLL.Activities.Common;
using WorkFlowDemo.BLL.Activities.MaterialOutbound;
using WorkFlowDemo.Models.Common;
using WorkFlowDemo.Models.Dtos;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace WorkFlowDemo.BLL.Workflows.MaterialOutWorkflow
{
    /// <summary>
    /// 物料出库工作流 - HTTP 端点触发版本
    /// </summary>
    public class MaterialOutboundWorkflow : WorkflowBase
    {
        protected override void Build(IWorkflowBuilder builder)
        {
            var requestVar = builder.WithVariable<MaterialOutboundRequest>();
            var batchNoVar = builder.WithVariable<string>();
            var operatorVar = builder.WithVariable<string>();
            var detailsVar = builder.WithVariable<List<MaterialOutboundDetailDto>>();
            var checkResultVar = builder.WithVariable<bool>();
            var updateResultVar = builder.WithVariable<bool>();
            var historyIdsVar = builder.WithVariable<List<string>>();
            var deleteResultVar = builder.WithVariable<bool>();
            var resultVar = builder.WithVariable<string>();
            var workflowIdVar = builder.WithVariable<string>();
            var approvalDecisionVar = builder.WithVariable<ApprovalDecision>();

            var jsonOptions = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            builder.Root = new Sequence
            {
                Activities =
                {
                    new HttpEndpoint
                    {
                        Path = new("/material/outbound/start"),
                        SupportedMethods = new(new[] { "POST" }),
                        CanStartWorkflow = true,
                        ParsedContent = new(requestVar)
                    },

                    new SetVariable
                    {
                        Variable = batchNoVar,
                        Value = new(context => requestVar.Get(context)?.BatchNumber ?? "")
                    },

                    new SetVariable
                    {
                        Variable = operatorVar,
                        Value = new(context => requestVar.Get(context)?.Operator ?? "System")
                    },

                    new GetWorkflowIdActivity
                    {
                        Result = new(workflowIdVar)
                    },

                    new LogWorkflowStatusActivity
                    {
                        StepName = new("工作流启动"),
                        StatusMessage = new(context => $"物料出库流程启动 - 批次号: {batchNoVar.Get(context)}"),
                        StepOrder = new(0),
                        ExecutionStatus = new("Started"),
                        WorkflowName = new("MaterialOutboundWorkflow"),
                        BatchNumber = new(batchNoVar),
                        Operator = new(operatorVar)
                    },

                    // 提前获取明细和校验库存
                    new GetOutboundDetailsActivity
                    {
                        BatchNumber = new(batchNoVar),
                        Result = new(detailsVar)
                    },

                    new LogWorkflowStatusActivity
                    {
                        StepName = new("获取出库明细"),
                        StatusMessage = new(context => $"获取到 {detailsVar.Get(context)?.Count ?? 0} 条出库明细"),
                        StepOrder = new(1),
                        ExecutionStatus = new("Completed"),
                        WorkflowName = new("MaterialOutboundWorkflow"),
                        BatchNumber = new(batchNoVar),
                        Operator = new(operatorVar)
                    },

                    new CheckInventoryActivity
                    {
                        Details = new(detailsVar),
                        Result = new(checkResultVar)
                    },

                    // 根据库存校验结果设置响应消息
                    new SetVariable
                    {
                        Variable = resultVar,
                        Value = new(context => JsonSerializer.Serialize(
                            ApiResponse.Success(new {
                                message = "物料出库流程已启动",
                                batchNumber = batchNoVar.Get(context),
                                workflowInstanceId = workflowIdVar.Get(context)
                            }), jsonOptions))
                    },

                    new WriteHttpResponse
                    {
                        Content = new(resultVar),
                        ContentType = new("application/json")
                    },

                    // 根据库存情况决定流程
                    new If(context => checkResultVar.Get(context))
                    {
                        // 库存充足 - 直接执行出库
                        Then = new Sequence
                        {
                            Activities =
                            {
                                new LogWorkflowStatusActivity
                                {
                                    StepName = new("库存校验"),
                                    StatusMessage = new("库存充足"),
                                    StepOrder = new(2),
                                    ExecutionStatus = new("Completed"),
                                    WorkflowName = new("MaterialOutboundWorkflow"),
                                    BatchNumber = new(batchNoVar),
                                    Operator = new(operatorVar)
                                },

                                new UpdateInventoryActivity
                                {
                                    Details = new(detailsVar),
                                    Result = new(updateResultVar)
                                },

                                new LogWorkflowStatusActivity
                                {
                                    StepName = new("更新库存"),
                                    StatusMessage = new("库存更新成功"),
                                    StepOrder = new(3),
                                    ExecutionStatus = new("Completed"),
                                    WorkflowName = new("MaterialOutboundWorkflow"),
                                    BatchNumber = new(batchNoVar),
                                    Operator = new(operatorVar)
                                },

                                new WriteHistoryActivity
                                {
                                    BatchNumber = new(batchNoVar),
                                    Details = new(detailsVar),
                                    Operator = new(operatorVar),
                                    Result = new(historyIdsVar)
                                },

                                new LogWorkflowStatusActivity
                                {
                                    StepName = new("写入履历"),
                                    StatusMessage = new(context => $"创建了 {historyIdsVar.Get(context)?.Count ?? 0} 条履历记录"),
                                    StepOrder = new(4),
                                    ExecutionStatus = new("Completed"),
                                    WorkflowName = new("MaterialOutboundWorkflow"),
                                    BatchNumber = new(batchNoVar),
                                    Operator = new(operatorVar)
                                },

                                new DeleteScanRecordsActivity
                                {
                                    BatchNumber = new(batchNoVar),
                                    Result = new(deleteResultVar)
                                },

                                new LogWorkflowStatusActivity
                                {
                                    StepName = new("删除扫描记录"),
                                    StatusMessage = new("扫描记录删除成功"),
                                    StepOrder = new(5),
                                    ExecutionStatus = new("Completed"),
                                    WorkflowName = new("MaterialOutboundWorkflow"),
                                    BatchNumber = new(batchNoVar),
                                    Operator = new(operatorVar)
                                }
                            }
                        },
                        // 库存不足 - 等待审批
                        Else = new Sequence
                        {
                            Activities =
                            {
                                new LogWorkflowStatusActivity
                                {
                                    StepName = new("库存校验"),
                                    StatusMessage = new("库存不足，等待审批"),
                                    StepOrder = new(2),
                                    ExecutionStatus = new("Pending"),
                                    WorkflowName = new("MaterialOutboundWorkflow"),
                                    BatchNumber = new(batchNoVar),
                                    Operator = new(operatorVar),
                                    ErrorMessage = new("库存不足"),
                                    RequiresApproval = new(true)
                                },

                                // 审批端点 - 必须紧跟在第一个响应之后
                                new HttpEndpoint
                                {
                                    Path = new(context => $"/material/outbound/approve/{workflowIdVar.Get(context)}"),
                                    SupportedMethods = new(new[] { "POST" }),
                                    CanStartWorkflow = false,
                                    ParsedContent = new(approvalDecisionVar)
                                },

                                new LogWorkflowStatusActivity
                                {
                                    StepName = new("收到审批"),
                                    StatusMessage = new(context => $"收到审批决策: {approvalDecisionVar.Get(context)?.Decision}"),
                                    StepOrder = new(3),
                                    ExecutionStatus = new("Completed"),
                                    WorkflowName = new("MaterialOutboundWorkflow"),
                                    BatchNumber = new(batchNoVar),
                                    Operator = new(operatorVar)
                                },

                                new If(context => approvalDecisionVar.Get(context)?.Decision == "approved")
                                {
                                    Then = new Sequence
                                    {
                                        Activities =
                                        {
                                            new UpdateLogApprovalStatusActivity
                                            {
                                                BatchNumber = new(batchNoVar)
                                            },

                                            new LogWorkflowStatusActivity
                                            {
                                                StepName = new("审批通过"),
                                                StatusMessage = new("审批通过，继续执行出库流程"),
                                                StepOrder = new(4),
                                                ExecutionStatus = new("Completed"),
                                                WorkflowName = new("MaterialOutboundWorkflow"),
                                                BatchNumber = new(batchNoVar),
                                                Operator = new(operatorVar)
                                            },

                                            new UpdateInventoryActivity
                                            {
                                                Details = new(detailsVar),
                                                Result = new(updateResultVar)
                                            },

                                            new LogWorkflowStatusActivity
                                            {
                                                StepName = new("更新库存"),
                                                StatusMessage = new("库存更新成功"),
                                                StepOrder = new(5),
                                                ExecutionStatus = new("Completed"),
                                                WorkflowName = new("MaterialOutboundWorkflow"),
                                                BatchNumber = new(batchNoVar),
                                                Operator = new(operatorVar)
                                            },

                                            new WriteHistoryActivity
                                            {
                                                BatchNumber = new(batchNoVar),
                                                Details = new(detailsVar),
                                                Operator = new(operatorVar),
                                                Result = new(historyIdsVar)
                                            },

                                            new LogWorkflowStatusActivity
                                            {
                                                StepName = new("写入履历"),
                                                StatusMessage = new(context => $"创建了 {historyIdsVar.Get(context)?.Count ?? 0} 条履历记录"),
                                                StepOrder = new(6),
                                                ExecutionStatus = new("Completed"),
                                                WorkflowName = new("MaterialOutboundWorkflow"),
                                                BatchNumber = new(batchNoVar),
                                                Operator = new(operatorVar)
                                            },

                                            new DeleteScanRecordsActivity
                                            {
                                                BatchNumber = new(batchNoVar),
                                                Result = new(deleteResultVar)
                                            },

                                            new LogWorkflowStatusActivity
                                            {
                                                StepName = new("删除扫描记录"),
                                                StatusMessage = new("扫描记录删除成功"),
                                                StepOrder = new(7),
                                                ExecutionStatus = new("Completed"),
                                                WorkflowName = new("MaterialOutboundWorkflow"),
                                                BatchNumber = new(batchNoVar),
                                                Operator = new(operatorVar)
                                            },

                                            new SetVariable
                                            {
                                                Variable = resultVar,
                                                Value = new(JsonSerializer.Serialize(
                                                    ApiResponse.Success("审批通过，出库完成"), jsonOptions))
                                            },

                                            new WriteHttpResponse
                                            {
                                                Content = new(resultVar),
                                                ContentType = new("application/json")
                                            }
                                        }
                                    },
                                    Else = new Sequence
                                    {
                                        Activities =
                                        {
                                            new LogWorkflowStatusActivity
                                            {
                                                StepName = new("审批拒绝"),
                                                StatusMessage = new("审批被拒绝，流程终止"),
                                                StepOrder = new(4),
                                                ExecutionStatus = new("Failed"),
                                                WorkflowName = new("MaterialOutboundWorkflow"),
                                                BatchNumber = new(batchNoVar),
                                                Operator = new(operatorVar),
                                                ErrorMessage = new("审批被拒绝")
                                            },

                                            new SetVariable
                                            {
                                                Variable = resultVar,
                                                Value = new(JsonSerializer.Serialize(
                                                    ApiResponse.Fail("审批已拒绝", 400), jsonOptions))
                                            },

                                            new WriteHttpResponse
                                            {
                                                Content = new(resultVar),
                                                ContentType = new("application/json")
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },

                    new LogWorkflowStatusActivity
                    {
                        StepName = new("工作流完成"),
                        StatusMessage = new(context => $"批次 {batchNoVar.Get(context)} 物料出库工作流执行完成"),
                        StepOrder = new(99),
                        ExecutionStatus = new("Completed"),
                        WorkflowName = new("MaterialOutboundWorkflow"),
                        BatchNumber = new(batchNoVar),
                        Operator = new(operatorVar)
                    }
                }
            };
        }
    }

    public class MaterialOutboundRequest
    {
        public string BatchNumber { get; set; } = string.Empty;
        public string? Operator { get; set; }
    }

    public class ApprovalDecision
    {
        public string Decision { get; set; } = string.Empty;
    }
}