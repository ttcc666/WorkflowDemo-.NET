using Elsa.Http;
using Elsa.Workflows;
using Elsa.Workflows.Activities;
using WorkFlowDemo.BLL.Activities.Common;
using WorkFlowDemo.Models.Common;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace WorkFlowDemo.BLL.Workflows.ApprovalWorkflow
{
    /// <summary>
    /// 简单审批工作流 - 演示工作流暂停和恢复
    /// </summary>
    public class SimpleApprovalWorkflow : WorkflowBase
    {
        protected override void Build(IWorkflowBuilder builder)
        {
            var requestVar = builder.WithVariable<ApprovalRequest>();
            var approvalDecisionVar = builder.WithVariable<ApprovalDecision>();
            var resultVar = builder.WithVariable<string>();
            var workflowIdVar = builder.WithVariable<string>();

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
                        Path = new("/approval/start"),
                        SupportedMethods = new(new[] { "POST" }),
                        CanStartWorkflow = true,
                        ParsedContent = new(requestVar)
                    },

                    new LogWorkflowStatusActivity
                    {
                        StepName = new("开始"),
                        StatusMessage = new(context => $"审批流程启动 - 申请人: {requestVar.Get(context)?.Applicant}, 内容: {requestVar.Get(context)?.Content}")
                    },

                    new GetWorkflowIdActivity
                    {
                        Result = new(workflowIdVar)
                    },

                    new SetVariable
                    {
                        Variable = resultVar,
                        Value = new(context => JsonSerializer.Serialize(
                            ApiResponse.Success(new {
                                message = "审批请求已提交，等待审批",
                                workflowInstanceId = workflowIdVar.Get(context)
                            }), jsonOptions))
                    },

                    new WriteHttpResponse
                    {
                        Content = new(resultVar),
                        ContentType = new("application/json")
                    },

                    new LogWorkflowStatusActivity
                    {
                        StepName = new("等待审批"),
                        StatusMessage = new("工作流已暂停，等待审批决策...")
                    },

                    // 使用 HttpEndpoint 接收审批决策，这会暂停工作流
                    new HttpEndpoint
                    {
                        Path = new(context => $"/approval/decision/{workflowIdVar.Get(context)}"),
                        SupportedMethods = new(new[] { "POST" }),
                        CanStartWorkflow = false,
                        ParsedContent = new(approvalDecisionVar)
                    },

                    new LogWorkflowStatusActivity
                    {
                        StepName = new("收到审批"),
                        StatusMessage = new(context => $"收到审批决策: {approvalDecisionVar.Get(context)?.Decision}")
                    },

                    // 根据审批结果执行不同分支
                    new If(context => approvalDecisionVar.Get(context)?.Decision == "approved")
                    {
                        Then = new Sequence
                        {
                            Activities =
                            {
                                new LogWorkflowStatusActivity
                                {
                                    StepName = new("批准"),
                                    StatusMessage = new("✅ 审批通过，流程继续执行")
                                },
                                new SetVariable
                                {
                                    Variable = resultVar,
                                    Value = new(JsonSerializer.Serialize(
                                        ApiResponse.Success("审批已通过"), jsonOptions))
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
                                    StepName = new("拒绝"),
                                    StatusMessage = new("❌ 审批被拒绝，流程终止")
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
                    },

                    new LogWorkflowStatusActivity
                    {
                        StepName = new("完成"),
                        StatusMessage = new(context =>
                            approvalDecisionVar.Get(context)?.Decision == "approved"
                                ? "✅ 审批流程成功完成"
                                : "❌ 审批流程已终止")
                    }
                }
            };
        }
    }

    public class ApprovalRequest
    {
        public string Applicant { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    public class ApprovalDecision
    {
        public string Decision { get; set; } = string.Empty;
    }
}