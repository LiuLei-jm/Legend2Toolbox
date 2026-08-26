using Legend2Toolbox.Api.Endpoints.Admin;
using Legend2Toolbox.Api.Endpoints.CardNumber;
using Legend2Toolbox.Api.Endpoints.ConnectionKey;

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

    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();
    }
    else
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            db.Database.Migrate();
        }
        catch (Exception ex)
        {
            Directory.CreateDirectory(@"D:\Logs\ToolboxAPI");
            File.WriteAllText(@"D:\Logs\ToolboxAPI\db-migration-error.txt", ex.ToString());
            throw;
        }
    }
    // Configure the HTTP request pipeline.
    app.UseExceptionHandler();
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseCors();
    app.UseHttpsRedirection();
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapCustomIdentityEndpoints();
    app.MapAdminUserEndpoints();
    app.MapConnectionKeyEndpoints();
    app.MapCardNumberEndpoints();

    app.MapHub<ResourceSyncHub>("/sync");

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

public partial class Program
{
}