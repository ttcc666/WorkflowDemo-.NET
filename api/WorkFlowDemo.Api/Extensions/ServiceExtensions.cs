using SqlSugar;
using WorkFlowDemo.BLL.Services;
using WorkFlowDemo.DAL.Repositories;

namespace WorkFlowDemo.Api.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddSqlSugar(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ISqlSugarClient>(s =>
            {
                var connectionString = configuration.GetSection("ConnectionConfigs:0:ConnectionString").Value
                    ?? "Server=.;Database=WorkFlowDemo;Trusted_Connection=True;TrustServerCertificate=True;";

                var config = new ConnectionConfig()
                {
                    ConnectionString = connectionString,
                    DbType = DbType.SqlServer,
                    IsAutoCloseConnection = true,
                };
                return new SqlSugarClient(config);
            });
            return services;
        }

        public static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            services.AddScoped<UserDal>();
            services.AddScoped<UserBll>();

            services.AddScoped<MaterialDal>();
            services.AddScoped<MaterialTemporaryScanDal>();
            services.AddScoped<MaterialInventoryDal>();
            services.AddScoped<MaterialBll>();
            return services;
        }

        public static void InitializeDatabase(this IServiceProvider serviceProvider)
        {
            var db = serviceProvider.GetRequiredService<ISqlSugarClient>();

            // 创建数据库（如果不存在）
            db.DbMaintenance.CreateDatabase();

            // 创建表（如果不存在）
            db.CodeFirst.InitTables(
                typeof(WorkFlowDemo.Models.Entities.User),
                typeof(WorkFlowDemo.Models.Entities.Material),
                typeof(WorkFlowDemo.Models.Entities.MaterialInventory),
                typeof(WorkFlowDemo.Models.Entities.MaterialTemporaryScan),
                typeof(WorkFlowDemo.Models.Entities.MaterialHistory)
            );

            // 初始化种子数据
            SeedData(db);
        }

        private static void SeedData(ISqlSugarClient db)
        {
            // 清空并重新插入 Material 数据
            db.Deleteable<WorkFlowDemo.Models.Entities.Material>().ExecuteCommand();
            var materials = new[]
            {
                new WorkFlowDemo.Models.Entities.Material { Id = Guid.NewGuid().ToString(), MaterialCode = "M001", Name = "Material 1" },
                new WorkFlowDemo.Models.Entities.Material { Id = Guid.NewGuid().ToString(), MaterialCode = "M002", Name = "Material 2" },
                new WorkFlowDemo.Models.Entities.Material { Id = Guid.NewGuid().ToString(), MaterialCode = "M003", Name = "Material 3" },
                new WorkFlowDemo.Models.Entities.Material { Id = Guid.NewGuid().ToString(), MaterialCode = "M004", Name = "Material 4" },
                new WorkFlowDemo.Models.Entities.Material { Id = Guid.NewGuid().ToString(), MaterialCode = "M005", Name = "Material 5" }
            };
            db.Insertable(materials).ExecuteCommand();

            // 清空并重新插入 MaterialInventory 数据
            db.Deleteable<WorkFlowDemo.Models.Entities.MaterialInventory>().ExecuteCommand();
            var inventories = materials.Select(m => new WorkFlowDemo.Models.Entities.MaterialInventory
            {
                Id = Guid.NewGuid().ToString(),
                MaterialCode = m.MaterialCode,
                Qty = 100,
                CreatedTime = DateTime.Now,
                UpdatedTime = DateTime.Now
            }).ToArray();
            db.Insertable(inventories).ExecuteCommand();
        }
    }
}
