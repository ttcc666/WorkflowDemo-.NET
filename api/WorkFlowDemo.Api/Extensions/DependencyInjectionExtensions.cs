using WorkFlowDemo.BLL.Services;
using WorkFlowDemo.DAL.Repositories;

namespace WorkFlowDemo.Api.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddAutoRegistration(this IServiceCollection services)
        {
            services.Scan(scan => scan
                .FromAssemblyOf<IUserService>()
                .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Service")))
                .AsMatchingInterface((type, _) => type.GetInterfaces().FirstOrDefault(i => i.Name == $"I{type.Name}"))
                .WithScopedLifetime());

            services.Scan(scan => scan
                .FromAssemblyOf<IUserRepository>()
                .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Repository")))
                .AsMatchingInterface((type, _) => type.GetInterfaces().FirstOrDefault(i => i.Name == $"I{type.Name}"))
                .WithScopedLifetime());

            return services;
        }
    }
}