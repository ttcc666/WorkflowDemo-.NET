# 订单处理工作流说明

## 概述

这是一个完整的订单处理工作流系统,演示了如何使用 Elsa Workflows 构建复杂的业务流程。系统提供了两个版本的工作流:

1. **基础版本** (`/order-processing`) - 标准的订单处理流程
2. **带补偿机制版本** (`/order-processing-safe`) - 包含错误处理和补偿逻辑的安全版本

## 工作流程

```
输入数据 (HTTP Request)
    ↓
变量初始化
    ↓
库存预留 → productId, quantity
    ↓
优惠券核销 → couponCode, orderAmount (更新)
    ↓
支付扣款 → orderAmount → paymentId (生成)
    ↓
创建发货单 → shipmentId (生成)
    ↓
积分发放 → orderAmount → points (计算)
    ↓
返回结果 (HTTP Response)
```

## 自定义活动

### 1. ReserveInventoryActivity (库存预留)
- **输入**: ProductId, Quantity
- **输出**: bool (成功/失败)
- **功能**: 预留指定产品的库存

### 2. ApplyCouponActivity (优惠券核销)
- **输入**: CouponCode, OrderAmount
- **输出**: decimal (折扣后金额)
- **功能**: 应用优惠券并计算折扣后的订单金额

### 3. ProcessPaymentActivity (支付扣款)
- **输入**: OrderAmount, UserId
- **输出**: string (PaymentId)
- **功能**: 处理支付并生成支付ID

### 4. CreateShipmentActivity (创建发货单)
- **输入**: ProductId, Quantity, UserId
- **输出**: string (ShipmentId)
- **功能**: 创建发货单并生成发货ID

### 5. AwardPointsActivity (积分发放)
- **输入**: OrderAmount, UserId
- **输出**: int (Points)
- **功能**: 根据订单金额计算并发放积分 (10%返还)

### 6. LogWorkflowStatusActivity (状态日志)
- **输入**: StepName, StatusMessage
- **功能**: 记录工作流每个步骤的执行状态

## 输入数据模型

```json
{
  "productId": "PROD-001",
  "quantity": 2,
  "couponCode": "DISCOUNT10",
  "orderAmount": 100.00,
  "userId": "USER-123"
}
```

### 字段说明

- `productId`: 产品ID (必填)
- `quantity`: 购买数量 (必填)
- `couponCode`: 优惠券代码 (可选)
- `orderAmount`: 订单金额 (必填)
- `userId`: 用户ID (必填)

## 测试工作流

### 使用 HTTP 文件测试

1. 打开 `OrderProcessing.http` 文件
2. 点击 "Send Request" 按钮
3. 查看响应结果

### 使用 curl 测试

```bash
curl -X POST https://localhost:5001/order-processing \
  -H "Content-Type: application/json" \
  -d '{
    "productId": "PROD-001",
    "quantity": 2,
    "couponCode": "DISCOUNT10",
    "orderAmount": 100.00,
    "userId": "USER-123"
  }'
```

## 查看工作流状态

工作流的每个步骤都会记录日志,您可以通过以下方式查看:

1. **控制台日志**: 运行应用程序时,控制台会显示每个步骤的执行状态
2. **日志文件**: 检查应用程序的日志文件
3. **Elsa Studio**: 使用 Elsa Studio 可视化工具查看工作流实例

### 日志示例

```
工作流状态更新 [WorkflowId: abc123] - 步骤: 开始, 状态: 订单处理工作流已启动
工作流状态更新 [WorkflowId: abc123] - 步骤: 库存预留, 状态: 库存预留完成
工作流状态更新 [WorkflowId: abc123] - 步骤: 优惠券核销, 状态: 优惠券核销完成, 最终金额: ¥90.00
工作流状态更新 [WorkflowId: abc123] - 步骤: 支付扣款, 状态: 支付扣款完成, PaymentId: PAY-xxx
工作流状态更新 [WorkflowId: abc123] - 步骤: 创建发货单, 状态: 发货单创建完成, ShipmentId: SHIP-xxx
工作流状态更新 [WorkflowId: abc123] - 步骤: 积分发放, 状态: 积分发放完成, Points: 9
```

## 响应示例

```
订单处理完成! PaymentId: PAY-abc123def456, ShipmentId: SHIP-xyz789, Points: 9, FinalAmount: ¥90.00
```

## 扩展功能

### 添加错误处理

您可以为每个活动添加错误处理逻辑:

```csharp
new ReserveInventoryActivity
{
    ProductId = new(context => orderInput.Get(context).ProductId),
    Quantity = new(context => orderInput.Get(context).Quantity)
}
```

### 添加补偿机制

如果某个步骤失败,可以添加补偿活动来回滚之前的操作。

### 添加条件分支

使用 `If` 活动根据条件执行不同的分支:

```csharp
new If
{
    Condition = new(context => discountedAmount.Get(context) > 1000),
    Then = new LogWorkflowStatusActivity { ... },
    Else = new LogWorkflowStatusActivity { ... }
}
```

## 错误处理和补偿机制

### 补偿工作流 (`/order-processing-safe`)

当订单处理过程中任何步骤失败时,系统会自动执行补偿操作,回滚已完成的步骤:

#### 补偿顺序(逆序执行)

1. **取消发货单** - 如果发货单已创建,则取消
2. **退款** - 如果支付已完成,则执行退款
3. **释放库存** - 如果库存已预留,则释放

#### 补偿活动

- [`RollbackInventoryActivity`](../WorkFlowDemo.BLL/Activities/RollbackInventoryActivity.cs:1) - 释放已预留的库存
- [`RefundPaymentActivity`](../WorkFlowDemo.BLL/Activities/RefundPaymentActivity.cs:1) - 退还已扣款的金额
- [`CancelShipmentActivity`](../WorkFlowDemo.BLL/Activities/CancelShipmentActivity.cs:1) - 取消已创建的发货单

#### 状态跟踪

每个步骤完成后都会设置相应的状态标志:
- `inventoryReserved` - 库存是否已预留
- `paymentProcessed` - 支付是否已处理
- `shipmentCreated` - 发货单是否已创建

这些标志用于确定在发生错误时需要回滚哪些操作。

### 测试补偿机制

#### 方法1: 使用模拟失败触发器

工作流内置了失败模拟机制,**当订单金额 > 1000 时会自动触发失败**:

```bash
# 测试补偿机制 - 订单金额 > 1000
curl -X POST https://localhost:5001/order-processing-safe \
  -H "Content-Type: application/json" \
  -d '{
    "productId": "PROD-001",
    "quantity": 2,
    "couponCode": "DISCOUNT10",
    "orderAmount": 1500.00,
    "userId": "USER-123"
  }'
```

预期结果:
```
⚠️ 模拟发货单创建失败 - 触发补偿机制
🔄 开始执行补偿流程...
↩️ 支付已退款: PAY-xxx, 金额: ¥1350.00
↩️ 库存已释放: ProductId=PROD-001, Quantity=2
✅ 所有补偿操作已完成
❌ 订单处理失败! 所有操作已回滚。原因: 发货单创建失败(订单金额>1000触发模拟失败)
```

#### 方法2: 手动抛出异常

也可以修改某个活动使其抛出异常,例如在 `CreateShipmentActivity` 中添加:

```csharp
throw new Exception("模拟发货单创建失败");
```

这将触发补偿流程,系统会自动:
1. 取消已创建的发货单(如果已创建)
2. 退还已扣除的款项
3. 释放已预留的库存

## 注意事项

1. 确保应用程序已正确配置 Elsa Workflows
2. 所有自定义活动都已注册到依赖注入容器
3. 日志级别设置为 Information 或更详细,以查看工作流状态
4. 测试时使用有效的数据格式
5. 生产环境建议使用带补偿机制的版本 (`/order-processing-safe`)

## 相关文件

- **基础工作流**: [`WorkFlowDemo.BLL/Workflows/OrderProcessingWorkflow.cs`](../WorkFlowDemo.BLL/Workflows/OrderProcessingWorkflow.cs:1)
- **补偿工作流**: [`WorkFlowDemo.BLL/Workflows/OrderProcessingWithCompensationWorkflow.cs`](../WorkFlowDemo.BLL/Workflows/OrderProcessingWithCompensationWorkflow.cs:1)
- **自定义活动**: `WorkFlowDemo.BLL/Activities/`
- **补偿活动**:
  - [`RollbackInventoryActivity.cs`](../WorkFlowDemo.BLL/Activities/RollbackInventoryActivity.cs:1)
  - [`RefundPaymentActivity.cs`](../WorkFlowDemo.BLL/Activities/RefundPaymentActivity.cs:1)
  - [`CancelShipmentActivity.cs`](../WorkFlowDemo.BLL/Activities/CancelShipmentActivity.cs:1)
- **DTO模型**: [`WorkFlowDemo.Models/Dtos/CreateOrderDto.cs`](../WorkFlowDemo.Models/Dtos/CreateOrderDto.cs:1)
- **测试文件**: [`WorkFlowDemo.Api/OrderProcessing.http`](OrderProcessing.http:1)