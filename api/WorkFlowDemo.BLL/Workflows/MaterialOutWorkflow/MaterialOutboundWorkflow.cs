using Elsa.Http;
using Elsa.Workflows;
using Elsa.Workflows.Activities;
using WorkFlowDemo.BLL.Activities.Common;
using WorkFlowDemo.BLL.Activities.MaterialOutbound;
using WorkFlowDemo.Models.Common;
using WorkFlowDemo.Models.Dtos;
using System.Text.Encodings.Web;
using System.Text.Json;
using Elsa.Workflows.Memory;

namespace WorkFlowDemo.BLL.Workflows.MaterialOutWorkflow
{
    /// <summary>
    /// 物料出库工作流 - HTTP 端点触发版本
    /// </summary>
    public class MaterialOutboundWorkflow : WorkflowBase
    {
        private const string WORKFLOW_NAME = "MaterialOutboundWorkflow";
        private const string APPROVED_DECISION = "approved";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// 构建物料出库工作流
        /// 流程：接收请求 -> 获取明细 -> 检查库存 -> 库存充足直接出库 / 库存不足等待审批 -> 执行出库操作
        /// </summary>
        protected override void Build(IWorkflowBuilder builder)
        {
            // 定义工作流变量
            var requestVar = builder.WithVariable<MaterialOutboundRequest>();
            var batchNoVar = builder.WithVariable<string>();
            var operatorVar = builder.WithVariable<string>();
            var detailsVar = builder.WithVariable<List<MaterialOutboundDetailDto>>();
            var checkResultVar = builder.WithVariable<bool>();
            var resultVar = builder.WithVariable<string>();
            var workflowIdVar = builder.WithVariable<string>();
            var approvalDecisionVar = builder.WithVariable<ApprovalDecision>();

            builder.Root = new Sequence
            {
                Activities =
                {
                    // 1. HTTP 端点：接收出库请求
                    new HttpEndpoint
                    {
                        Path = new("/material/outbound/start"),
                        SupportedMethods = new(new[] { "POST" }),
                        CanStartWorkflow = true,
                        ParsedContent = new(requestVar)
                    },

                    // 2. 提取批次号
                    new SetVariable
                    {
                        Variable = batchNoVar,
                        Value = new(context => requestVar.Get(context)?.BatchNumber ?? "")
                    },

                    // 3. 提取操作员
                    new SetVariable
                    {
                        Variable = operatorVar,
                        Value = new(context => requestVar.Get(context)?.Operator ?? "System")
                    },

                    // 4. 获取工作流实例 ID
                    new GetWorkflowIdActivity
                    {
                        Result = new(workflowIdVar)
                    },

                    // 5. 记录工作流启动日志
                    new LogWorkflowStatusActivity
                    {
                        StepName = new("工作流启动"),
                        StatusMessage = new(context => $"物料出库流程启动 - 批次号: {batchNoVar.Get(context)}"),
                        StepOrder = new(0),
                        ExecutionStatus = new("Started"),
                        WorkflowName = new(WORKFLOW_NAME),
                        BatchNumber = new(batchNoVar),
                        Operator = new(operatorVar)
                    },

                    // 6. 获取出库明细
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
                        WorkflowName = new(WORKFLOW_NAME),
                        BatchNumber = new(batchNoVar),
                        Operator = new(operatorVar)
                    },

                    // 7. 检查库存是否充足
                    new CheckInventoryActivity
                    {
                        Details = new(detailsVar),
                        Result = new(checkResultVar)
                    },

                    // 8. 设置初始响应消息
                    new SetVariable
                    {
                        Variable = resultVar,
                        Value = new(context => JsonSerializer.Serialize(
                            ApiResponse.Success(new {
                                message = "物料出库流程已启动",
                                batchNumber = batchNoVar.Get(context),
                                workflowInstanceId = workflowIdVar.Get(context)
                            }), JsonOptions))
                    },

                    // 9. 返回初始响应
                    new WriteHttpResponse
                    {
                        Content = new(resultVar),
                        ContentType = new("application/json")
                    },

                    // 10. 根据库存检查结果分支
                    new If(context => checkResultVar.Get(context))
                    {
                        // 分支 A：库存充足 - 直接执行出库
                        Then = new Sequence
                        {
                            Activities =
                            {
                                CreateLogActivity("库存校验", "库存充足", 2, "Completed", batchNoVar, operatorVar),

                                CreateOutboundProcessSequence(batchNoVar, detailsVar, operatorVar, 3)
                            }
                        },
                        // 分支 B：库存不足 - 等待审批
                        Else = new Sequence
                        {
                            Activities =
                            {
                                CreateLogActivity("库存校验", "库存不足，等待审批", 2, "Pending", batchNoVar, operatorVar, "库存不足", true),

                                // 审批端点：等待审批决策
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
                                    WorkflowName = new(WORKFLOW_NAME),
                                    BatchNumber = new(batchNoVar),
                                    Operator = new(operatorVar)
                                },

                                // 根据审批决策分支
                                new If(context => approvalDecisionVar.Get(context)?.Decision == APPROVED_DECISION)
                                {
                                    // 审批通过：执行出库
                                    Then = new Sequence
                                    {
                                        Activities =
                                        {
                                            new UpdateLogApprovalStatusActivity
                                            {
                                                BatchNumber = new(batchNoVar)
                                            },

                                            CreateLogActivity("审批通过", "审批通过，继续执行出库流程", 4, "Completed", batchNoVar, operatorVar),

                                            CreateOutboundProcessSequence(batchNoVar, detailsVar, operatorVar, 5),

                                            new SetVariable
                                            {
                                                Variable = resultVar,
                                                Value = new(JsonSerializer.Serialize(
                                                    ApiResponse.Success("审批通过，出库完成"), JsonOptions))
                                            },

                                            new WriteHttpResponse
                                            {
                                                Content = new(resultVar),
                                                ContentType = new("application/json")
                                            }
                                        }
                                    },
                                    // 审批拒绝：终止流程
                                    Else = new Sequence
                                    {
                                        Activities =
                                        {
                                            new UpdateLogApprovalStatusActivity
                                            {
                                                BatchNumber = new(batchNoVar)
                                            },
                                            CreateLogActivity("审批拒绝", "审批被拒绝，流程终止", 4, "Failed", batchNoVar, operatorVar, "审批被拒绝"),

                                            new SetVariable
                                            {
                                                Variable = resultVar,
                                                Value = new(JsonSerializer.Serialize(
                                                    ApiResponse.Fail("审批已拒绝", 400), JsonOptions))
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
                        WorkflowName = new(WORKFLOW_NAME),
                        BatchNumber = new(batchNoVar),
                        Operator = new(operatorVar)
                    }
                }
            };
        }

        /// <summary>
        /// 创建日志记录活动
        /// </summary>
        private static LogWorkflowStatusActivity CreateLogActivity(
            string stepName,
            string statusMessage,
            int stepOrder,
            string executionStatus,
            Variable<string> batchNoVar,
            Variable<string> operatorVar,
            string errorMessage = null,
            bool requiresApproval = false)
        {
            var activity = new LogWorkflowStatusActivity
            {
                StepName = new(stepName),
                StatusMessage = new(statusMessage),
                StepOrder = new(stepOrder),
                ExecutionStatus = new(executionStatus),
                WorkflowName = new(WORKFLOW_NAME),
                BatchNumber = new(batchNoVar),
                Operator = new(operatorVar)
            };

            if (!string.IsNullOrEmpty(errorMessage))
                activity.ErrorMessage = new(errorMessage);

            if (requiresApproval)
                activity.RequiresApproval = new(true);

            return activity;
        }

        /// <summary>
        /// 创建出库处理序列：更新库存 -> 写入履历 -> 删除扫描记录
        /// </summary>
        private static Sequence CreateOutboundProcessSequence(
            Variable<string> batchNoVar,
            Variable<List<MaterialOutboundDetailDto>> detailsVar,
            Variable<string> operatorVar,
            int stepOffset)
        {
            return new Sequence
            {
                Activities =
                {
                    // 更新库存
                    new UpdateInventoryActivity
                    {
                        Details = new(detailsVar)
                    },

                    CreateLogActivity("更新库存", "库存更新成功", stepOffset, "Completed", batchNoVar, operatorVar),

                    // 写入履历
                    new WriteHistoryActivity
                    {
                        BatchNumber = new(batchNoVar),
                        Details = new(detailsVar),
                        Operator = new(operatorVar)
                    },

                    CreateLogActivity("写入履历", "履历记录创建成功", stepOffset + 1, "Completed", batchNoVar, operatorVar),

                    // 删除扫描记录
                    new DeleteScanRecordsActivity
                    {
                        BatchNumber = new(batchNoVar)
                    },

                    CreateLogActivity("删除扫描记录", "扫描记录删除成功", stepOffset + 2, "Completed", batchNoVar, operatorVar)
                }
            };
        }
    }

    /// <summary>
    /// 物料出库请求模型
    /// </summary>
    public class MaterialOutboundRequest
    {
        public string BatchNumber { get; set; } = string.Empty;
        public string? Operator { get; set; }
    }

    /// <summary>
    /// 审批决策模型
    /// </summary>
    public class ApprovalDecision
    {
        public string Decision { get; set; } = string.Empty;
    }
}