using Microsoft.OpenApi.Models;
using System.CommandLine;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace WorkFlowDemo.Api.Extensions
{
    public static class ApiServiceExtensions
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services)
        {
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                });
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.CustomSchemaIds(type => type.ToString());
                c.SwaggerDoc("v1", new() { Title = "WorkFlowDemo.Api", Version = "v1" });
            });
            services.AddHealthChecks();
            services.AddAutoMapper(typeof(Program));

            return services;
        }

        public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
        {
            services.AddCors(cors => cors
                .AddDefaultPolicy(policy => policy
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithExposedHeaders("x-elsa-workflow-instance-id")));

            return services;
        }
    }
}