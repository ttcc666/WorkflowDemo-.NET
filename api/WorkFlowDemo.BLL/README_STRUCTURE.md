# WorkFlowDemo.BLL 项目结构说明

## 目录结构

```
WorkFlowDemo.BLL/
├── Activities/                    # 自定义活动
│   ├── Common/                   # 通用活动
│   │   ├── DemoActivity.cs      # 演示活动
│   │   └── LogWorkflowStatusActivity.cs  # 日志记录活动
│   ├── OrderProcessing/          # 订单处理活动
│   │   ├── ReserveInventoryActivity.cs   # 库存预留
│   │   ├── ApplyCouponActivity.cs        # 优惠券核销
│   │   ├── ProcessPaymentActivity.cs     # 支付扣款
│   │   ├── CreateShipmentActivity.cs     # 创建发货单
│   │   └── AwardPointsActivity.cs        # 积分发放
│   └── Compensation/             # 补偿活动
│       ├── RollbackInventoryActivity.cs  # 回滚库存
│       ├── RefundPaymentActivity.cs      # 退款
│       ├── CancelShipmentActivity.cs     # 取消发货单
│       └── SimulateFailureActivity.cs    # 模拟失败(测试用)
├── Workflows/                     # 工作流定义
│   ├── Demo/                     # 演示工作流
│   │   ├── DemoWorkflow.cs      # 基础演示
│   │   ├── PostDemoWorkflow.cs  # POST请求演示
│   │   └── ComplexOrderWorkflow.cs  # 复杂订单演示
│   └── OrderProcessing/          # 订单处理工作流
│       ├── OrderProcessingWorkflow.cs                    # 基础版本
│       ├── OrderProcessingWithCompensationWorkflow.cs    # 自动补偿版本
│       └── OrderProcessingWithManualCompensationWorkflow.cs  # 人工干预版本
├── Services/                      # 业务逻辑服务
│   ├── UserBll.cs
│   └── MaterialBll.cs
└── Base/                          # 基础类
    ├── IBaseService.cs
    └── BaseService.cs
```

## 活动分类说明

### 1. Common (通用活动)
包含可在多个工作流中复用的通用活动:
- **DemoActivity**: 演示用的条件判断活动
- **LogWorkflowStatusActivity**: 记录工作流状态的日志活动

### 2. OrderProcessing (订单处理活动)
订单处理流程中的核心业务活动:
- **ReserveInventoryActivity**: 预留商品库存
- **ApplyCouponActivity**: 应用优惠券并计算折扣
- **ProcessPaymentActivity**: 处理支付并生成支付ID
- **CreateShipmentActivity**: 创建发货单
- **AwardPointsActivity**: 发放积分奖励

### 3. Compensation (补偿活动)
用于错误恢复和事务回滚的补偿活动:
- **RollbackInventoryActivity**: 释放已预留的库存
- **RefundPaymentActivity**: 退还已扣除的款项
- **CancelShipmentActivity**: 取消已创建的发货单
- **SimulateFailureActivity**: 模拟失败场景(用于测试补偿机制)

## 工作流分类说明

### 1. Demo (演示工作流)
用于学习和演示Elsa Workflows基本功能:
- **DemoWorkflow**: GET请求处理演示
- **PostDemoWorkflow**: POST请求处理演示
- **ComplexOrderWorkflow**: 复杂业务流程演示

### 2. OrderProcessing (订单处理工作流)
生产级订单处理工作流,包含三个版本:

#### OrderProcessingWorkflow (基础版本)
- **端点**: `/order-processing`
- **特点**: 标准的订单处理流程,无错误处理
- **适用**: 演示和测试

#### OrderProcessingWithCompensationWorkflow (自动补偿版本)
- **端点**: `/order-processing-safe`
- **特点**: 自动检测错误并执行补偿
- **触发条件**: 订单金额 > 1000 时模拟失败
- **补偿顺序**: 取消发货单 → 退款 → 释放库存
- **适用**: 标准业务流程

#### OrderProcessingWithManualCompensationWorkflow (人工干预版本)
- **端点**: `/order-processing-manual`
- **特点**: 发生错误时暂停,等待人工审批
- **审批端点**: `/approve-compensation?workflowInstanceId={id}`
- **补偿决策**: 由人工审批决定是否执行补偿
- **适用**: 需要人工决策的场景

## 使用指南

### 添加新活动

1. 确定活动类型(通用/订单处理/补偿)
2. 在对应文件夹创建活动类
3. 继承 `CodeActivity` 或 `CodeActivity<T>`
4. 实现 `ExecuteAsync` 方法

示例:
```csharp
namespace WorkFlowDemo.BLL.Activities.OrderProcessing
{
    public class NewActivity : CodeActivity<string>
    {
        [Input(Description = "输入参数")]
        public Input<string> InputParam { get; set; } = default!;

        protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var logger = context.GetRequiredService<ILogger<NewActivity>>();
            var input = InputParam.Get(context);
            
            // 业务逻辑
            await Task.Delay(100);
            
            context.Set(Result, "result");
        }
    }
}
```

### 添加新工作流

1. 确定工作流类型(演示/订单处理/其他)
2. 在对应文件夹创建工作流类
3. 继承 `WorkflowBase`
4. 实现 `Build` 方法

示例:
```csharp
namespace WorkFlowDemo.BLL.Workflows.OrderProcessing
{
    public class NewWorkflow : WorkflowBase
    {
        protected override void Build(IWorkflowBuilder builder)
        {
            var input = builder.WithVariable<InputDto>();
            
            builder.Root = new Sequence
            {
                Activities =
                {
                    new HttpEndpoint
                    {
                        Path = new("/new-workflow"),
                        SupportedMethods = new(new[] { "POST" }),
                        CanStartWorkflow = true,
                        ParsedContent = new(input)
                    },
                    // 添加更多活动...
                }
            };
        }
    }
}
```

## 相关文档

- [订单处理工作流说明](../WorkFlowDemo.Api/README_ORDER_WORKFLOW.md)
- [人工干预补偿机制说明](../WorkFlowDemo.Api/README_MANUAL_COMPENSATION.md)
- [Elsa配置说明](../ELSA_CONFIGURATION.md)

## 最佳实践

1. **活动设计**
   - 单一职责:每个活动只做一件事
   - 可复用:通用逻辑放在Common文件夹
   - 日志记录:使用ILogger记录关键操作

2. **工作流设计**
   - 清晰的命名:工作流名称应反映其功能
   - 错误处理:生产环境使用带补偿的版本
   - 状态跟踪:使用变量跟踪执行状态

3. **补偿机制**
   - 逆序执行:补偿操作按相反顺序执行
   - 条件检查:只补偿已执行的操作
   - 幂等性:补偿操作应该是幂等的

4. **文件组织**
   - 按功能分类:相关的活动和工作流放在一起
   - 命名规范:使用清晰的命名约定
   - 文档完善:为复杂逻辑添加注释