# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a .NET 9 workflow demonstration project built with Elsa Workflow framework. It implements approval workflows and material outbound workflows with HTTP endpoints, persistence, and custom activities.

## Architecture

### Project Structure
- **WorkFlowDemo.Api** - Web API layer with controllers and configuration
- **WorkFlowDemo.BLL** - Business logic layer containing workflows and custom activities
- **WorkFlowDemo.DAL** - Data access layer
- **WorkFlowDemo.Models** - Shared models and DTOs

### Key Technologies
- **Elsa Workflow 3.5.1** - Core workflow engine with HTTP triggers and persistence
- **SqlSugar 5.1.4** - ORM for business data (SQL Server)
- **SQLite** - Elsa workflow persistence storage
- **AutoMapper** - Object mapping
- **Swagger** - API documentation

## Development Commands

### Build and Run
```bash
# Build the solution
dotnet build

# Run the API project
cd WorkFlowDemo.Api
dotnet run

# The API will be available at http://localhost:5085
```

### Database Setup
The project uses dual database configuration:
- **Elsa workflows**: SQLite (`elsa.db`) - automatically created
- **Business data**: SQL Server (configured in `appsettings.json`)

## Workflow Architecture

### Core Workflows
1. **SimpleApprovalWorkflow** (`WorkFlowDemo.BLL.Workflows.ApprovalWorkflow`)
   - HTTP-triggered approval process with pause/resume capability
   - Endpoints: `/workflows/approval/start` and `/workflows/approval/decision/{id}`

2. **MaterialOutboundWorkflow** (`WorkFlowDemo.BLL.Workflows.MaterialOutWorkflow`)
   - Material inventory management workflow

### Custom Activities
Located in `WorkFlowDemo.BLL.Activities/`:
- **Common**: `LogWorkflowStatusActivity`, `GetWorkflowIdActivity`, `UpdateLogApprovalStatusActivity`
- **MaterialOutbound**: Inventory management activities

### Workflow Configuration
- Workflows are registered in `ElsaServiceExtensions.AddElsaWorkflow()`
- HTTP base URL: `http://localhost:5085/workflows`
- Persistence: SQLite with Entity Framework Core

## Key Configuration Files

### appsettings.json
```json
{
  "ConnectionStrings": {
    "Elsa": "Data Source=elsa.db;Cache=Shared"
  },
  "ConnectionConfigs": [
    {
      "ConfigId": "Default",
      "ConnectionString": "Server=.;Database=WorkFlowDemo;Trusted_Connection=True;TrustServerCertificate=True;",
      "DbType": "SqlServer"
    }
  ]
}
```

## Extension Methods Pattern
The project uses extension methods for service registration:
- `ElsaServiceExtensions` - Workflow configuration
- `ApiServiceExtensions` - API services (Controllers, Swagger, etc.)
- `DependencyInjectionExtensions` - SqlSugar and business services
- `ApplicationBuilderExtensions` - Middleware pipeline configuration

## Testing Workflows

### Approval Workflow Test
```bash
# Start approval
curl -X POST http://localhost:5085/workflows/approval/start \
  -H "Content-Type: application/json" \
  -d '{"applicant": "张三", "content": "请假申请"}'

# Get workflow instance ID from response header: x-elsa-workflow-instance-id

# Approve/Reject
curl -X POST http://localhost:5085/workflows/approval/decision/{workflowInstanceId} \
  -H "Content-Type: application/json" \
  -d '{"decision": "approved"}'
```

## Development Notes

### Workflow Development
- Inherit from `WorkflowBase` for new workflows
- Register workflows in `ElsaServiceExtensions.AddElsaWorkflow()`
- Use `HttpEndpoint` activities for HTTP triggers
- Custom activities should inherit from `BaseActivity`

### Database Considerations
- Elsa uses SQLite for workflow persistence (development)
- Business data uses SQL Server
- Database initialization happens in `Program.cs` via `InitializeDatabase()`

### Service Registration
- Business services use auto-registration via Scrutor
- Workflows require explicit registration
- SqlSugar configuration supports multiple connection strings