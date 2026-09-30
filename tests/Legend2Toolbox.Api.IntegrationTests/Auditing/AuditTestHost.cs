using Legend2Toolbox.Api;
using Legend2Toolbox.Api.Endpoints.Admin;
using Legend2Toolbox.Api.Endpoints.Audit;
using Legend2Toolbox.Api.Endpoints.CardNumber;
using Legend2Toolbox.Api.Endpoints.ConnectionKey;
using Legend2Toolbox.Api.Endpoints.Script;
using Legend2Toolbox.Application;
using Legend2Toolbox.Application.Common.Interfaces;
using Legend2Toolbox.Domain.Entities.Audit;
using Legend2Toolbox.Domain.Entities.Membership;
using Legend2Toolbox.Infrastructure;
using Legend2Toolbox.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Legend2Toolbox.Api.IntegrationTests.Auditing;

internal sealed class AuditTestHost : IAsyncDisposable
{
    public const string Password = "AuditTest!123";
    public string Root { get; } = Path.Combine(Path.GetTempPath(), "Legend2AuditTests", Guid.NewGuid().ToString("N"));
    public string SpoolDirectory => Path.Combine(Root, "pending");
    public WebApplication App { get; private set; } = null!;
    public HttpClient Client { get; private set; } = null!;

    public static async Task<AuditTestHost> CreateAsync()
    {
        var host = new AuditTestHost();
        Directory.CreateDirectory(host.Root);
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Testing", ContentRootPath = host.Root,
            ApplicationName = typeof(Program).Assembly.FullName
        });
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = $"Data Source={Path.Combine(host.Root, "test.db")};Pooling=False",
            ["JwtSettings:Secret"] = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"),
            ["JwtSettings:Issuer"] = "audit-tests",
            ["JwtSettings:Audience"] = "audit-tests",
            ["Audit:SpoolDirectory"] = host.SpoolDirectory,
            ["Audit:WriteTimeoutSeconds"] = "1",
            ["RateLimiting:GlobalRequestsPerMinute"] = "10000",
            ["RateLimiting:LoginAttemptsPerMinute"] = "10000",
            ["RateLimiting:RegistrationAttemptsPerHour"] = "10000"
        });
        builder.Services.AddApplicationService();
        builder.Services.AddInfrastructureService(builder.Configuration);
        builder.Services.AddApiServices(builder.Configuration);
        host.App = builder.Build();
        host.App.UseExceptionHandler();
        host.App.Use(async (context, next) =>
        {
            context.Connection.RemoteIpAddress = IPAddress.Parse("203.0.113.25");
            await next();
        });
        host.App.UseRateLimiter();
        host.App.UseAuthentication();
        host.App.UseAuthorization();
        host.App.MapCustomIdentityEndpoints();
        host.App.MapAdminUserEndpoints();
        host.App.MapAuditEndpoints();
        host.App.MapScriptEndpoints();
        host.App.MapCardNumberEndpoints();
        host.App.MapConnectionKeyEndpoints();
        await using (var scope = host.App.Services.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreatedAsync();
            var roles = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            foreach (var role in Enum.GetNames<Roles>())
                (await roles.CreateAsync(new ApplicationRole { Name = role })).Succeeded.Should().BeTrue();
        }
        await host.App.StartAsync();
        host.Client = host.App.GetTestClient();
        return host;
    }

    public async Task<ApplicationUser> CreateUserAsync(string name, string role = "Member")
    {
        await using var scope = App.Services.CreateAsyncScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser { UserName = name, Email = name + "@example.test" };
        (await manager.CreateAsync(user, Password)).Succeeded.Should().BeTrue();
        (await manager.AddToRoleAsync(user, role)).Succeeded.Should().BeTrue();
        if (role == "Member")
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.UserMemberships.Add(UserMembership.Create(user.Id, 30, MembershipSource.Admin));
            await db.SaveChangesAsync();
        }
        return user;
    }

    public async Task<HttpClient> AsUserAsync(ApplicationUser user)
    {
        await using var scope = App.Services.CreateAsyncScope();
        var tokens = scope.ServiceProvider.GetRequiredService<ITokenService>();
        var (token, _) = await tokens.GenerateAccessTokenAsync(user);
        var client = App.GetTestClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public async Task<List<AuditLog>> LogsAsync()
    {
        await using var scope = App.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().AuditLogs
            .AsNoTracking().Include(x => x.Details).OrderBy(x => x.OccurredAtUnixMs).ToListAsync();
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await App.DisposeAsync();
        var root = Path.GetFullPath(Root);
        var parent = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "Legend2AuditTests")) + Path.DirectorySeparatorChar;
        if (root.StartsWith(parent, StringComparison.OrdinalIgnoreCase) && Directory.Exists(root))
            Directory.Delete(root, recursive: true);
    }
}
