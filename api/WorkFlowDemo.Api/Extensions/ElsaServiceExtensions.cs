using Elsa.EntityFrameworkCore.Extensions;
using Elsa.EntityFrameworkCore.Modules.Management;
using Elsa.EntityFrameworkCore.Modules.Runtime;
using Elsa.Extensions;
using WorkFlowDemo.BLL.Workflows;

namespace WorkFlowDemo.Api.Extensions
{
    public static class ElsaServiceExtensions
    {
        public static IServiceCollection AddElsaWorkflow(this IServiceCollection services)
        {
            services.AddElsa(elsa =>
            {
                elsa.AddWorkflow<DemoWorkflow>();
                elsa.AddWorkflow<PostDemoWorkflow>();
                elsa.AddWorkflow<OrderProcessingWorkflow>();
                elsa.AddWorkflow<OrderProcessingWithCompensationWorkflow>();
                elsa.AddWorkflow<OrderProcessingWithManualCompensationWorkflow>();
                elsa.UseHttp(http => http.ConfigureHttpOptions = options =>
                {
                    options.BaseUrl = new Uri("https://localhost:5085");
                    options.BasePath = "/workflows";
                });
            });

            return services;
        }
    }
}