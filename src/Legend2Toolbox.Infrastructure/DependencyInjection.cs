using Legend2Toolbox.Application.Common.Interfaces;
using Legend2Toolbox.Infrastructure.Files;
using Legend2Toolbox.Infrastructure.Services;
using Legend2Toolbox.Infrastructure.SignalR;
using Microsoft.Extensions.DependencyInjection;

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
}
