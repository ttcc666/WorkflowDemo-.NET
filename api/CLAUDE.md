# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 项目概述

这是一个基于 ASP.NET Core 9.0 和 Elsa Workflow 3.5.1 的工作流演示项目，展示了如何使用 Elsa 构建带补偿机制的业务工作流。项目包含订单处理和物料出库两个主要业务场景。

## 架构设计

项目采用经典的三层架构：

- **WorkFlowDemo.Models**: 实体模型和 DTO 定义层
- **WorkFlowDemo.DAL**: 数据访问层，使用 SqlSugar ORM
- **WorkFlowDemo.BLL**: 业务逻辑层，包含 Elsa 工作流定义和自定义 Activity
- **WorkFlowDemo.Api**: Web API 层，提供 HTTP 端点和前端页面

### 工作流架构

项目使用 Elsa Workflow 实现业务流程编排：

1. **工作流定义位置**: `WorkFlowDemo.BLL/Workflows/`
   - `Demo/`: 演示工作流
   - `OrderProcessing/`: 订单处理工作流（包含三种补偿策略）
   - `MaterialOutWorkflow/`: 物料出库工作流

2. **自定义 Activity 位置**: `WorkFlowDemo.BLL/Activities/`
   - `Common/`: 通用活动（如日志记录）
   - `OrderProcessing/`: 订单处理相关活动
   - `Compensation/`: 补偿活动
   - `MaterialOutbound/`: 物料出库相关活动

3. **工作流注册**: 所有工作流必须在 `WorkFlowDemo.Api/Extensions/ElsaServiceExtensions.cs` 中使用 `elsa.AddWorkflow<T>()` 注册

### 数据访问架构

- 使用 SqlSugar 作为 ORM，配置在 `ServiceExtensions.cs`
- 默认连接 SQL Server 本地实例 (`.`)，数据库名 `WorkFlowDemo`
- 数据库初始化在应用启动时自动执行（`InitializeDatabase` 方法）
- 包含种子数据自动填充（5 个物料，每个初始库存 100）

## 常用命令

### 构建和运行

```bash
# 构建整个解决方案
dotnet build WorkFlowDemo.slnx

# 运行 API 项目
cd WorkFlowDemo.Api
dotnet run

# 或使用 watch 模式（热重载）
dotnet watch run
```

### 数据库操作

数据库会在应用启动时自动创建和初始化。如需手动重置：

1. 删除 SQL Server 中的 `WorkFlowDemo` 数据库
2. 重新启动应用，数据库将自动重建

### 测试工作流

项目提供了前端测试页面（位于 `wwwroot/`）：

- `index.html`: 主页，包含工作流测试链接
- `material-query.html`: 物料查询页面
- `inventory-query.html`: 库存查询页面
- `scan.html`: 扫描出库页面

访问 `https://localhost:5085` 查看测试页面。

## 工作流端点

Elsa 工作流通过 HTTP 端点触发：

- `/workflows/demo`: GET 请求，演示工作流
- `/workflows/post-demo`: POST 请求，演示 POST 工作流
- `/workflows/order-processing`: POST 请求，基础订单处理
- `/workflows/order-processing-safe`: POST 请求，带自动补偿的订单处理（订单金额 >1000 触发失败）
- `/workflows/order-processing-manual`: POST 请求，带手动补偿的订单处理
- `/workflows/material-outbound`: POST 请求，物料出库工作流

## 补偿机制实现

项目展示了两种补偿模式：

### 1. 自动补偿模式 (OrderProcessingWithCompensationWorkflow)

使用 `If` 活动检测错误状态，自动执行补偿逻辑：
- 使用布尔变量跟踪每个步骤的完成状态
- 失败时按相反顺序执行补偿活动
- 补偿活动位于 `Activities/Compensation/`

### 2. 物料出库补偿模式 (MaterialOutWorkflow)

在每个关键步骤后立即检查结果，失败时触发补偿：
- 库存检查失败 → 终止流程
- 更新库存失败 → 回滚履历
- 删除扫描记录失败 → 回滚库存和履历

## 添加新工作流的步骤

1. 在 `WorkFlowDemo.BLL/Workflows/` 创建新工作流类，继承 `WorkflowBase`
2. 实现 `Build(IWorkflowBuilder builder)` 方法
3. 如需自定义活动，在 `WorkFlowDemo.BLL/Activities/` 创建活动类
4. 在 `ElsaServiceExtensions.cs` 中注册工作流：`elsa.AddWorkflow<YourWorkflow>()`
5. 在工作流中定义 `HttpEndpoint` 活动以暴露 HTTP 端点

## 关键配置文件

- `appsettings.json`: 数据库连接字符串配置
  - `ConnectionConfigs[0].ConnectionString`: SqlSugar 业务数据库
  - `ConnectionStrings.Elsa`: Elsa 持久化数据库（当前已注释，使用内存模式）

- `Program.cs`: 应用启动配置，使用扩展方法模式组织服务注册

## 依赖项版本

- .NET: 9.0
- Elsa Workflow: 3.5.1
- SqlSugar: 5.1.4.205
- AutoMapper: 12.0.1

## 注意事项

- Elsa 持久化功能当前已注释（`ElsaServiceExtensions.cs` 第 29-46 行），工作流定义和实例存储在内存中
- 如需启用持久化，取消注释相关代码并确保 SQLite 连接字符串正确
- 所有工作流日志使用 `LogWorkflowStatusActivity` 输出到控制台
- 前端页面使用原生 JavaScript，无需构建步骤
- 数据库种子数据在每次启动时会清空并重新插入（`SeedData` 方法）
