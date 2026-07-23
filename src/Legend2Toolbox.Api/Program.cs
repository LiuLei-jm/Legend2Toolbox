
using Legend2Toolbox.Api.Endpoints.SecurityKey;

try
{
    Log.Information("LegendToolBox API 正在启动...");

    var builder = WebApplication.CreateBuilder(args);

    Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
    builder.Host.UseSerilog();


    builder.Services.AddApplicationService();
    builder.Services.AddInfrastructureService(builder.Configuration);
    builder.Services.AddApiServices(builder.Configuration);

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    app.UseExceptionHandler();
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapCustomIdentityEndpoints();
    app.MapAdminUserEndpoints();
    app.MapSecurityKeyEndpoints();

    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        try
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

            logger.LogInformation("正在检查并初始化系统基础数据...");
            await IdentityDataSeeder.SeedAsync(userManager, roleManager, logger);
            logger.LogInformation("系统基础数据初始化完毕！");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "在初始化系统默认数据时发生致命错误.");
        }
    }

    app.Run();

}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "主机意外终止,启动失败!");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
