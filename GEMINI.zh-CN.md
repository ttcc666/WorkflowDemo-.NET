
# 项目概述

本项目是一个基于 .NET 9.0 的物料管理系统 Web API。它利用 Elsa 工作流库来管理和执行工作流，特别是物料出库和审批流程。后端使用 ASP.NET Core 构建，并使用 SqlSugarCore 作为 ORM 与数据库进行交互。该项目还包含一个使用 HTML、JavaScript 和 Bootstrap 构建的简单前端。

项目结构分为四个主要部分：

*   **WorkFlowDemo.Api:** 主 Web API 项目，提供与系统交互和触发工作流的端点。
*   **WorkFlowDemo.BLL:** 业务逻辑层，包含工作流定义和业务服务。
*   **WorkFlowDemo.DAL:** 数据访问层，负责数据库操作。
*   **WorkFlowDemo.Models:** 包含整个应用程序使用的数据模型（实体和 DTO）。

# 构建和运行

要构建和运行此项目，您需要安装 .NET 9.0 SDK。

1.  **还原依赖项：**
    ```bash
    dotnet restore
    ```

2.  **构建解决方案：**
    ```bash
    dotnet build
    ```

3.  **运行 API：**
    导航到 `api/WorkFlowDemo.Api` 目录并运行以下命令：
    ```bash
    dotnet run
    ```
    API 将在 `http://localhost:5085` 上可用。

# 开发约定

*   **编码风格：** 项目遵循标准的 C# 和 .NET 编码约定。
*   **工作流：** 工作流在 `WorkFlowDemo.BLL/Workflows` 目录中使用 Elsa 工作流库进行定义。
*   **数据库：** 项目使用名为 `elsa.db` 的 SQLite 数据库进行工作流持久化，并使用另一个数据库（可能在 `appsettings.json` 中配置）存储业务数据。
*   **API 文档：** API 使用 Swagger 进行文档记录，在应用程序运行时可在 `http://localhost:5085/swagger` 上访问。
*   **前端：** 前端位于 `WorkFlowDemo.Api/wwwroot` 目录中。它是一个简单的 HTML/JS/CSS 应用程序。
