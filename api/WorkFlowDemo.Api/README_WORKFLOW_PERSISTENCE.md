# Elsa工作流持久化配置说明

## 概述

本项目已配置Elsa工作流持久化,使用SQLite数据库存储工作流定义、实例和执行状态。这确保了工作流在应用重启后仍能继续执行,并支持长时间运行的工作流(如人工审批流程)。

## 持久化架构

### 数据库
- **类型**: SQLite
- **文件**: `elsa.db` (位于项目根目录)
- **连接字符串**: 在 `appsettings.json` 中配置

### 持久化内容

1. **工作流定义 (Workflow Definitions)**
   - 工作流的结构和配置
   - 活动及其连接关系
   - 输入/输出变量定义

2. **工作流实例 (Workflow Instances)**
   - 正在执行或已完成的工作流实例
   - 实例状态(运行中、已完成、已暂停、失败等)
   - 实例变量值

3. **工作流执行日志 (Workflow Execution Logs)**
   - 活动执行记录
   - 错误和异常信息
   - 状态转换历史

4. **书签 (Bookmarks)**
   - 暂停点信息(如等待HTTP请求、等待人工审批)
   - 恢复执行所需的上下文

## 配置文件

### appsettings.json

```json
{
  "ConnectionStrings": {
    "Elsa": "Data Source=elsa.db;Cache=Shared"
  }
}
```

### ElsaServiceExtensions.cs

```csharp
public static IServiceCollection AddElsaWorkflow(this IServiceCollection services, IConfiguration configuration)
{
    services.AddElsa(elsa =>
    {
        // 注册工作流...
        
        // 配置工作流定义持久化
        elsa.UseWorkflowManagement(management =>
        {
            management.UseEntityFrameworkCore(ef =>
            {
                ef.UseSqlite(configuration.GetConnectionString("Elsa") 
                    ?? "Data Source=elsa.db;Cache=Shared");
            });
        });
        
        // 配置工作流实例运行时持久化
        elsa.UseWorkflowRuntime(runtime =>
        {
            runtime.UseEntityFrameworkCore(ef =>
            {
                ef.UseSqlite(configuration.GetConnectionString("Elsa") 
                    ?? "Data Source=elsa.db;Cache=Shared");
            });
        });
    });
    
    return services;
}
```

## 持久化的好处

### 1. 应用重启后恢复

工作流实例在应用重启后自动恢复:

```
应用启动 → 加载暂停的工作流实例 → 从书签位置继续执行
```

### 2. 长时间运行的工作流

支持需要长时间等待的工作流:
- **人工审批**: 工作流暂停等待审批,审批后继续
- **定时任务**: 等待特定时间后继续执行
- **外部事件**: 等待外部系统回调

### 3. 工作流历史追踪

可以查询历史工作流实例:
- 查看执行状态
- 分析失败原因
- 审计和合规

### 4. 分布式执行

支持多个应用实例共享工作流状态:
- 负载均衡
- 高可用性
- 横向扩展

## 使用示例

### 人工审批工作流

[`OrderProcessingWithManualCompensationWorkflow`](../WorkFlowDemo.BLL/Workflows/OrderProcessing/OrderProcessingWithManualCompensationWorkflow.cs:1) 演示了持久化的实际应用:

**步骤1**: 启动工作流
```bash
POST /order-processing-manual
{
  "orderAmount": 1500.00,
  ...
}
```

**响应**: 返回WorkflowInstanceId
```
WorkflowInstanceId: abc-123-def
```

**步骤2**: 工作流暂停并持久化到数据库

**步骤3**: 应用可以重启,工作流状态保持

**步骤4**: 发送审批请求恢复工作流
```bash
POST /approve-compensation?workflowInstanceId=abc-123-def
true
```

**步骤5**: 工作流从暂停点继续执行

## 数据库管理

### 查看数据库

使用SQLite工具查看数据库内容:

```bash
# 使用sqlite3命令行工具
sqlite3 elsa.db

# 查看表
.tables

# 查看工作流实例
SELECT * FROM WorkflowInstances;

# 查看书签
SELECT * FROM Bookmarks;
```

### 常用表

| 表名 | 说明 |
|------|------|
| `WorkflowDefinitions` | 工作流定义 |
| `WorkflowInstances` | 工作流实例 |
| `ActivityExecutionRecords` | 活动执行记录 |
| `Bookmarks` | 暂停点/书签 |
| `WorkflowExecutionLogRecords` | 执行日志 |

### 清理数据

```sql
-- 删除已完成的工作流实例(保留最近30天)
DELETE FROM WorkflowInstances 
WHERE Status = 'Finished' 
AND FinishedAt < datetime('now', '-30 days');

-- 删除失败的工作流实例
DELETE FROM WorkflowInstances 
WHERE Status = 'Faulted';
```

## 迁移到其他数据库

### 使用SQL Server

1. 安装NuGet包:
```bash
dotnet add package Elsa.EntityFrameworkCore.SqlServer
```

2. 更新配置:
```csharp
ef.UseSqlServer(configuration.GetConnectionString("Elsa"));
```

3. 更新连接字符串:
```json
{
  "ConnectionStrings": {
    "Elsa": "Server=.;Database=ElsaWorkflows;Trusted_Connection=True;"
  }
}
```

### 使用PostgreSQL

1. 安装NuGet包:
```bash
dotnet add package Elsa.EntityFrameworkCore.PostgreSql
```

2. 更新配置:
```csharp
ef.UsePostgreSql(configuration.GetConnectionString("Elsa"));
```

3. 更新连接字符串:
```json
{
  "ConnectionStrings": {
    "Elsa": "Host=localhost;Database=elsa;Username=postgres;Password=password"
  }
}
```

## 性能优化

### 1. 索引优化

Elsa自动创建必要的索引,但可以根据查询模式添加额外索引:

```sql
-- 为常用查询添加索引
CREATE INDEX IX_WorkflowInstances_Status_CreatedAt 
ON WorkflowInstances(Status, CreatedAt);
```

### 2. 定期清理

设置定时任务清理旧数据:

```csharp
// 在后台服务中定期清理
public class WorkflowCleanupService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // 清理30天前已完成的工作流
            await CleanupOldWorkflowsAsync();
            
            // 每天执行一次
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }
}
```

### 3. 连接池配置

```json
{
  "ConnectionStrings": {
    "Elsa": "Data Source=elsa.db;Cache=Shared;Pooling=True;Max Pool Size=100"
  }
}
```

## 监控和诊断

### 查询工作流状态

```csharp
// 注入IWorkflowInstanceStore
public class WorkflowMonitorService
{
    private readonly IWorkflowInstanceStore _workflowInstanceStore;
    
    public async Task<WorkflowInstance?> GetWorkflowInstanceAsync(string instanceId)
    {
        return await _workflowInstanceStore.FindAsync(instanceId);
    }
    
    public async Task<IEnumerable<WorkflowInstance>> GetRunningWorkflowsAsync()
    {
        return await _workflowInstanceStore.FindManyAsync(
            new WorkflowInstanceFilter { Status = WorkflowStatus.Running }
        );
    }
}
```

### 日志记录

Elsa自动记录工作流执行日志,可以通过日志系统查看:

```json
{
  "Logging": {
    "LogLevel": {
      "Elsa": "Information",
      "Elsa.Workflows.Runtime": "Debug"
    }
  }
}
```

## 故障排除

### 问题1: 数据库锁定

**症状**: `database is locked` 错误

**解决方案**:
```json
{
  "ConnectionStrings": {
    "Elsa": "Data Source=elsa.db;Cache=Shared;Journal Mode=WAL"
  }
}
```

### 问题2: 工作流无法恢复

**症状**: 应用重启后工作流没有继续执行

**检查**:
1. 确认数据库文件存在
2. 检查书签是否正确保存
3. 查看日志中的错误信息

### 问题3: 性能下降

**症状**: 工作流执行变慢

**解决方案**:
1. 清理旧的工作流实例
2. 优化数据库索引
3. 考虑迁移到更强大的数据库(SQL Server/PostgreSQL)

## 最佳实践

1. **定期备份**: 定期备份 `elsa.db` 文件
2. **监控大小**: 监控数据库文件大小,及时清理
3. **测试恢复**: 定期测试工作流恢复功能
4. **日志保留**: 设置合理的日志保留策略
5. **错误处理**: 为长时间运行的工作流添加超时和错误处理

## 相关文档

- [订单处理工作流](README_ORDER_WORKFLOW.md)
- [人工干预补偿机制](README_MANUAL_COMPENSATION.md)
- [Elsa官方文档](https://v3.elsaworkflows.io/)