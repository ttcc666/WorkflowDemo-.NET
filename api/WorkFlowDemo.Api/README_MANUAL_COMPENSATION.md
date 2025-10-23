# 人工干预补偿机制说明

## 概述

[`OrderProcessingWithManualCompensationWorkflow`](../WorkFlowDemo.BLL/Workflows/OrderProcessingWithManualCompensationWorkflow.cs:1) 提供了带人工审批的补偿机制。当订单处理过程中发生错误时,工作流会暂停并等待人工审批,决定是否执行补偿操作。

## 工作流程

```
订单提交 → 库存预留 → 优惠券核销 → 支付扣款
                                    ↓
                            [检测到错误]
                                    ↓
                            ⏸️ 工作流暂停
                                    ↓
                        等待人工审批决策
                        ↙              ↘
                审批通过              审批拒绝
                    ↓                    ↓
            执行补偿操作          不执行补偿
            (逆序回滚)            (需手动处理)
```

## 使用步骤

### 步骤1: 启动工作流

发送订单处理请求(订单金额 > 1000 会触发失败):

```bash
POST https://localhost:5001/order-processing-manual
Content-Type: application/json

{
  "productId": "PROD-MANUAL",
  "quantity": 10,
  "couponCode": "DISCOUNT10",
  "orderAmount": 1500.00,
  "userId": "USER-MANUAL"
}
```

**响应示例**:
```
⏸️ 工作流已暂停,等待人工审批是否执行补偿...
WorkflowInstanceId: 3fa85f64-5717-4562-b3fc-2c963f66afa6
```

**重要**: 记录返回的 `WorkflowInstanceId`,后续审批需要使用。

### 步骤2: 查看工作流状态

通过日志查看当前工作流状态:

```
工作流状态更新 - 步骤: 库存预留, 状态: ✓ 库存预留完成
工作流状态更新 - 步骤: 优惠券核销, 状态: ✓ 优惠券核销完成, 最终金额: ¥1350.00
工作流状态更新 - 步骤: 支付扣款, 状态: ✓ 支付扣款完成, PaymentId: PAY-xxx
工作流状态更新 - 步骤: 模拟失败, 状态: ⚠️ 模拟发货单创建失败 - 等待人工审批补偿
工作流状态更新 - 步骤: 等待审批, 状态: ⏸️ 工作流已暂停,等待人工审批是否执行补偿...
```

### 步骤3: 人工审批

#### 选项A: 审批通过 - 执行补偿

```bash
POST https://localhost:5001/approve-compensation?workflowInstanceId=3fa85f64-5717-4562-b3fc-2c963f66afa6
Content-Type: application/json

true
```

**预期结果**:
```
✅ 审批通过 - 开始执行补偿
🔄 开始执行补偿流程...
↩️ 支付已退款: PAY-xxx, 金额: ¥1350.00
↩️ 库存已释放: ProductId=PROD-MANUAL, Quantity=10
✅ 所有补偿操作已完成
❌ 订单处理失败! 经人工审批后,所有操作已回滚
```

#### 选项B: 审批拒绝 - 不执行补偿

```bash
POST https://localhost:5001/approve-compensation?workflowInstanceId=3fa85f64-5717-4562-b3fc-2c963f66afa6
Content-Type: application/json

false
```

**预期结果**:
```
❌ 审批拒绝 - 不执行补偿
⚠️ 订单处理失败! 人工审批拒绝补偿,请手动处理
```

## 补偿操作详情

当审批通过时,系统会按以下顺序执行补偿:

### 1. 取消发货单
- **条件**: 如果发货单已创建 (`shipmentCreated = true`)
- **操作**: 调用 [`CancelShipmentActivity`](../WorkFlowDemo.BLL/Activities/CancelShipmentActivity.cs:1)
- **日志**: `↩️ 发货单已取消: SHIP-xxx`

### 2. 退款
- **条件**: 如果支付已处理 (`paymentProcessed = true`)
- **操作**: 调用 [`RefundPaymentActivity`](../WorkFlowDemo.BLL/Activities/RefundPaymentActivity.cs:1)
- **日志**: `↩️ 支付已退款: PAY-xxx, 金额: ¥1350.00`

### 3. 释放库存
- **条件**: 如果库存已预留 (`inventoryReserved = true`)
- **操作**: 调用 [`RollbackInventoryActivity`](../WorkFlowDemo.BLL/Activities/RollbackInventoryActivity.cs:1)
- **日志**: `↩️ 库存已释放: ProductId=PROD-MANUAL, Quantity=10`

## 状态跟踪

工作流使用以下标志跟踪每个步骤的完成状态:

| 变量 | 说明 | 用途 |
|------|------|------|
| `inventoryReserved` | 库存是否已预留 | 决定是否需要释放库存 |
| `paymentProcessed` | 支付是否已处理 | 决定是否需要退款 |
| `shipmentCreated` | 发货单是否已创建 | 决定是否需要取消发货单 |
| `hasError` | 是否发生错误 | 触发人工审批流程 |
| `manualApproval` | 人工审批结果 | 决定是否执行补偿 |

## 使用场景

### 场景1: 高价值订单需要人工确认

对于高价值订单,如果处理失败,可能需要人工评估是否立即退款,还是先联系客户确认。

### 场景2: 复杂业务规则

某些业务场景下,补偿操作可能涉及多个系统或需要特殊处理,需要人工介入决策。

### 场景3: 审计和合规要求

某些行业(如金融)可能要求关键操作必须经过人工审批才能执行。

## 测试文件

使用 [`ManualCompensation.http`](ManualCompensation.http:1) 文件进行测试:

1. 发送订单处理请求
2. 记录返回的 WorkflowInstanceId
3. 使用该ID发送审批请求

## 与自动补偿的对比

| 特性 | 自动补偿 | 人工干预补偿 |
|------|----------|--------------|
| 端点 | `/order-processing-safe` | `/order-processing-manual` |
| 失败处理 | 自动执行补偿 | 暂停等待审批 |
| 响应时间 | 快速 | 取决于审批速度 |
| 适用场景 | 标准流程 | 需要人工决策 |
| 工作流文件 | [`OrderProcessingWithCompensationWorkflow.cs`](../WorkFlowDemo.BLL/Workflows/OrderProcessingWithCompensationWorkflow.cs:1) | [`OrderProcessingWithManualCompensationWorkflow.cs`](../WorkFlowDemo.BLL/Workflows/OrderProcessingWithManualCompensationWorkflow.cs:1) |

## 注意事项

1. **WorkflowInstanceId 必须准确**: 审批请求必须使用正确的工作流实例ID
2. **工作流会一直等待**: 如果不发送审批请求,工作流会一直处于暂停状态
3. **审批超时**: 建议在生产环境中设置审批超时机制
4. **日志记录**: 所有审批决策都应该被记录以便审计
5. **并发处理**: 确保同一个工作流实例不会被多次审批

## 扩展建议

### 1. 添加审批超时

```csharp
new Delay
{
    Duration = new(TimeSpan.FromMinutes(30))
},
new If
{
    Condition = new(context => !manualApproval.IsSet),
    Then = new Sequence
    {
        Activities =
        {
            // 超时自动拒绝或执行默认操作
        }
    }
}
```

### 2. 添加审批人信息

扩展审批请求,包含审批人信息:

```json
{
  "approved": true,
  "approver": "admin@example.com",
  "reason": "客户已确认退款",
  "timestamp": "2024-01-01T10:00:00Z"
}
```

### 3. 多级审批

对于高价值订单,可以实现多级审批机制:

```
一级审批(主管) → 二级审批(经理) → 执行补偿
```

## 相关文件

- **工作流**: [`OrderProcessingWithManualCompensationWorkflow.cs`](../WorkFlowDemo.BLL/Workflows/OrderProcessingWithManualCompensationWorkflow.cs:1)
- **测试文件**: [`ManualCompensation.http`](ManualCompensation.http:1)
- **补偿活动**:
  - [`RollbackInventoryActivity.cs`](../WorkFlowDemo.BLL/Activities/RollbackInventoryActivity.cs:1)
  - [`RefundPaymentActivity.cs`](../WorkFlowDemo.BLL/Activities/RefundPaymentActivity.cs:1)
  - [`CancelShipmentActivity.cs`](../WorkFlowDemo.BLL/Activities/CancelShipmentActivity.cs:1)
- **自动补偿版本**: [`README_ORDER_WORKFLOW.md`](README_ORDER_WORKFLOW.md:1)