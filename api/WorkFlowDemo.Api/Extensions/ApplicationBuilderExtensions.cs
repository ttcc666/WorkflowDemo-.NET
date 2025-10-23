using Elsa.Extensions;
using WorkFlowDemo.Api.Middlewares;

namespace WorkFlowDemo.Api.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static WebApplication ConfigureElsaMiddleware(this WebApplication app)
        {
            // Configure the HTTP request pipeline.
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.UseWorkflows();

            return app;
        }

        public static WebApplication ConfigureSwagger(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("v1/swagger.json", "Elsa Api V1");
                });
            }

            app.UseSwagger();
            app.UseSwaggerUI();

            return app;
        }

        public static WebApplication ConfigureMiddleware(this WebApplication app)
        {
            app.UseMiddleware<GlobalExceptionMiddleware>();
            app.UseHttpsRedirection();
            app.UseCors();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            return app;
        }

        public static WebApplication ConfigureEndpoints(this WebApplication app)
        {
            app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
            app.MapControllers();

            return app;
        }
    }
}