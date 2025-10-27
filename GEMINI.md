
# Project Overview

This project is a .NET 9.0 Web API for a material management system. It utilizes the Elsa Workflow library to manage and execute workflows, specifically for material outbound and approval processes. The backend is built with ASP.NET Core, and it uses SqlSugarCore as an ORM for database interactions. The project also includes a simple frontend built with HTML, JavaScript, and Bootstrap.

The project is structured into four main parts:

*   **WorkFlowDemo.Api:** The main web API project, which exposes endpoints for interacting with the system and triggering workflows.
*   **WorkFlowDemo.BLL:** The business logic layer, which contains the workflow definitions and business services.
*   **WorkFlowDemo.DAL:** The data access layer, responsible for database operations.
*   **WorkFlowDemo.Models:** Contains the data models (entities and DTOs) used throughout the application.

# Building and Running

To build and run this project, you will need the .NET 9.0 SDK installed.

1.  **Restore dependencies:**
    ```bash
    dotnet restore
    ```

2.  **Build the solution:**
    ```bash
    dotnet build
    ```

3.  **Run the API:**
    Navigate to the `api/WorkFlowDemo.Api` directory and run the following command:
    ```bash
    dotnet run
    ```
    The API will be available at `http://localhost:5085`.

# Development Conventions

*   **Coding Style:** The project follows standard C# and .NET coding conventions.
*   **Workflows:** Workflows are defined in the `WorkFlowDemo.BLL/Workflows` directory using the Elsa Workflow library.
*   **Database:** The project uses a SQLite database named `elsa.db` for workflow persistence and another database (presumably configured in `appsettings.json`) for business data.
*   **API Documentation:** The API is documented using Swagger, which is available at `http://localhost:5085/swagger` when the application is running.
*   **Frontend:** The frontend is located in the `WorkFlowDemo.Api/wwwroot` directory. It's a simple HTML/JS/CSS application.
