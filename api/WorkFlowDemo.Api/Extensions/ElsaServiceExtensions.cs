using Elsa.EntityFrameworkCore.Extensions;
using Elsa.EntityFrameworkCore.Modules.Management;
using Elsa.EntityFrameworkCore.Modules.Runtime;
using Elsa.Extensions;

namespace WorkFlowDemo.Api.Extensions
{
    public static class ElsaServiceExtensions
    {
        public static IServiceCollection AddElsaWorkflow(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddElsa(elsa =>
            {
                // 注册工作流 - 使用完整命名空间
                elsa.AddWorkflow<WorkFlowDemo.BLL.Workflows.MaterialOutWorkflow.MaterialOutWorkflow>();
                // 配置HTTP
                elsa.UseHttp(http => http.ConfigureHttpOptions = options =>
                {
                    options.BaseUrl = new Uri("https://localhost:5085");
                    options.BasePath = "/workflows";
                });

                // 配置工作流定义持久化 - 使用SQLite
                // elsa.UseWorkflowManagement(management =>
                // {
                //     management.UseEntityFrameworkCore(ef =>
                //     {
                //         ef.UseSqlite(configuration.GetConnectionString("Elsa")
                //             ?? "Data Source=elsa.db;Cache=Shared");
                //     });
                // });

                // // 配置工作流实例运行时持久化
                // elsa.UseWorkflowRuntime(runtime =>
                // {
                //     runtime.UseEntityFrameworkCore(ef =>
                //     {
                //         ef.UseSqlite(configuration.GetConnectionString("Elsa")
                //             ?? "Data Source=elsa.db;Cache=Shared");
                //     });
                // });
            });

            return services;
        }
    }
}