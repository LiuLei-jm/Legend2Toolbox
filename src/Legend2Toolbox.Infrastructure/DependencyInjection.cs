using Legend2Toolbox.Application.Common.Interfaces;
using Legend2Toolbox.Infrastructure.Files;
using Legend2Toolbox.Infrastructure.Identity;
using Legend2Toolbox.Infrastructure.Persistence;
using Legend2Toolbox.Infrastructure.Services;
using Legend2Toolbox.Infrastructure.SignalR;

namespace Legend2Toolbox.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IClientConfigurationService, ClientConfigurationService>();
        services.AddSingleton<IClientFileOperationService, ClientFileOperationService>();
        services.AddSingleton<ISignalRClientService, SignalRClientService>();
        return services;
    }
    public static IServiceCollection AddInfrastructureToApi(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddTransient<IEmailSender, EmailSender>();
        return services;
    }
}
