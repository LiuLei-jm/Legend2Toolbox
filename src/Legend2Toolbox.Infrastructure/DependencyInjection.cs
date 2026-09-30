namespace Legend2Toolbox.Infrastructure;

using Legend2Toolbox.Infrastructure.Auditing;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureService(this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");
        services.AddHttpContextAccessor();
        services.Configure<AuditOptions>(configuration.GetSection("Audit"));
        services.AddScoped<AuditSession>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<AuditSaveChangesInterceptor>();
        services.AddScoped<AuditTransactionInterceptor>();
        services.AddSingleton<AuditStore>();
        services.AddHostedService<AuditRetryService>();
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>(),
                sp.GetRequiredService<AuditTransactionInterceptor>())
            .UseSqlite(connectionString,
            sqlOptions => sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
            ));

        //var connectionString = configuration.GetConnectionString("PostgresqlConnection");
        //services.AddDbContext<ApplicationDbContext>(options => { options.UseNpgsql(connectionString); });
        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<ApplicationDbContext>()
        );
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IMembershipService, MembershipService>();
        services.AddScoped<IMembershipPaymentService, MembershipPaymentService>();
        services.AddScoped<IPaymentGateway, AlipayGateway>();
        services.AddScoped<IPaymentGateway, WechatGateway>();
        services.Configure<AlipayGatewayOptions>(configuration.GetSection("Payments:Alipay"));
        services.Configure<WechatGatewayOptions>(configuration.GetSection("Payments:Wechat"));
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddTransient<IEmailSender, SmtpEmailSender>();
        return services;
    }
}
