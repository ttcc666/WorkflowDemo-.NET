# 审批工作流示例

这是一个使用 Elsa Workflow 实现的简单审批工作流示例，演示了工作流的暂停和恢复功能。

## 工作流说明

### 功能特点

- ✅ 工作流暂停：在审批节点暂停，等待外部决策
- ✅ 条件分支：根据审批结果（批准/拒绝）执行不同的逻辑
- ✅ 持久化支持：工作流状态保存到数据库，应用重启后可恢复
- ✅ HTTP 触发：通过 HTTP 端点启动和恢复工作流

### 工作流程

1. **提交审批请求** → POST `/workflows/approval/start`
   - 输入：申请人、申请内容
   - 工作流启动并立即返回响应
   - 工作流在审批节点暂停

2. **等待审批决策** → 工作流暂停状态
   - 工作流实例保存在数据库中
   - 等待外部系统或用户提交审批决策

3. **提交审批决策** → POST `/workflows/approval/decision/{workflowInstanceId}`
   - 输入：`{ "decision": "approved" }` 或 `{ "decision": "rejected" }`
   - 工作流实例 ID 作为 URL 路径参数

4. **执行结果分支**
   - **批准**：执行批准后的业务逻辑，流程成功完成
   - **拒绝**：执行拒绝后的清理逻辑，流程终止

## 使用方法

### 1. 启动应用

```bash
cd api/WorkFlowDemo.Api
dotnet run
```

### 2. 访问测试页面

打开浏览器访问：`http://localhost:5085/approval-test.html`

### 3. 使用 API 测试

#### 提交审批请求

```bash
curl -X POST http://localhost:5085/workflows/approval/start \
  -H "Content-Type: application/json" \
  -d '{
    "applicant": "张三",
    "content": "请假申请"
  }' \
  -i
```

**响应示例：**

```json
{
  "success": true,
  "data": "审批请求已提交，等待审批"
}
```

**重要：** 从响应头 `x-elsa-workflow-instance-id` 中获取工作流实例 ID

#### 批准申请

```bash
curl -X POST http://localhost:5085/workflows/approval/decision/<工作流实例ID> \
  -H "Content-Type: application/json" \
  -d '{"decision": "approved"}'
```

#### 拒绝申请

```bash
curl -X POST http://localhost:5085/workflows/approval/decision/<工作流实例ID> \
  -H "Content-Type: application/json" \
  -d '{"decision": "rejected"}'
```

## 技术实现

### 核心组件

1. **SimpleApprovalWorkflow.cs** - 工作流定义
   - 使用 `HttpEndpoint` 接收请求
   - 使用 `If` 活动实现条件分支
   - 使用 `LogWorkflowStatusActivity` 记录状态

2. **持久化配置** - ElsaServiceExtensions.cs
   - 使用 SQLite 存储工作流定义和实例
   - 支持工作流暂停后恢复

3. **测试页面** - approval-test.html
   - 提供可视化的测试界面
   - 演示完整的审批流程

### 关键代码

```csharp
// 第一个 HttpEndpoint：启动工作流
new HttpEndpoint
{
    Path = new("/approval/start"),
    SupportedMethods = new(new[] { "POST" }),
    CanStartWorkflow = true,
    ParsedContent = new(requestVar)
}

// 第二个 HttpEndpoint：恢复工作流（暂停点）
new HttpEndpoint
{
    Path = new(context => $"/approval/decision/{workflowIdVar.Get(context)}"),
    SupportedMethods = new(new[] { "POST" }),
    CanStartWorkflow = false,  // 不能启动新实例
    ParsedContent = new(approvalDecisionVar)
}

// 条件分支
new If(context => approvalDecisionVar.Get(context)?.Decision == "approved")
{
    Then = new Sequence { /* 批准逻辑 */ },
    Else = new Sequence { /* 拒绝逻辑 */ }
}
```

## 扩展建议

1. **添加超时机制**：如果长时间未审批，自动拒绝
2. **多级审批**：支持多个审批节点
3. **审批历史**：记录审批过程和决策人
4. **通知功能**：审批状态变更时发送通知
5. **权限控制**：只有特定角色可以审批

## 注意事项

- 确保已启用工作流持久化（ElsaServiceExtensions.cs）
- 工作流实例 ID 必须作为 URL 路径参数正确传递才能恢复暂停的工作流
- 生产环境建议使用更可靠的数据库（如 SQL Server、PostgreSQL）
