using WorkFlowDemo.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 添加 Elsa 工作流服务（带持久化）
builder.Services.AddElsaWorkflow(builder.Configuration);

// 添加 CORS 策略
builder.Services.AddCorsPolicy();

// 添加 API 服务（Controllers、Swagger、HealthChecks、AutoMapper）
builder.Services.AddApiServices();

// 添加 SqlSugar 配置（用于业务数据访问）
builder.Services.AddSqlSugar(builder.Configuration);

// 添加业务服务
builder.Services.AddBusinessServices();

var app = builder.Build();

// 初始化数据库
app.Services.InitializeDatabase();

// 配置 Swagger
app.ConfigureSwagger();

// 配置中间件管道
app.ConfigureMiddleware();

// 配置 Elsa 中间件
app.ConfigureElsaMiddleware();

// 配置端点
app.ConfigureEndpoints();

app.Run();
