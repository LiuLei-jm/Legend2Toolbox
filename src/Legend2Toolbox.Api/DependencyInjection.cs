using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace Legend2Toolbox.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        });

        services.AddAuthentication(IdentityConstants.BearerScheme);
        services.AddAuthorization();
        services.AddIdentityApiEndpoints<ApplicationUser>(options =>
        {
            options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            options.User.RequireUniqueEmail = true;
            options.Lockout.AllowedForNewUsers = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "LegendToolBox API", Version = "v1" });
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "请输入有效令牌。格式：Bearer {你的AccessToken}",
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        var rateLimitSettings = configuration.GetSection("RateLimiting");
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, ct) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                if (!context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    retryAfter = TimeSpan.FromSeconds(60);
                }
                await context.HttpContext.Response.WriteAsync($"请求过于频繁，请 {retryAfter.TotalSeconds} 秒后重试.", ct);
            };

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                if (!int.TryParse(rateLimitSettings["GlobalRequestsPerMinute"], out var globalRequestsPerMinute))
                    globalRequestsPerMinute = 100;
                return RateLimitPartition.GetFixedWindowLimiter(partitionKey: ipAddress, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = globalRequestsPerMinute,
                    Window = TimeSpan.FromMinutes(1)
                });
            });

            options.AddFixedWindowLimiter("login-policy", options =>
            {
                if (!int.TryParse(rateLimitSettings["LoginAttemptsPerMinute"], out var loginAttemptsMinute))
                    loginAttemptsMinute = 5;
                if (!int.TryParse(rateLimitSettings["LoginQueueLimit"], out var loginQueueLimit))
                    loginQueueLimit = 2;
                options.PermitLimit = loginAttemptsMinute;
                options.Window = TimeSpan.FromMinutes(1);
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                options.QueueLimit = loginQueueLimit;
            });

            options.AddFixedWindowLimiter("register-policy", options =>
            {
                if (!int.TryParse(rateLimitSettings["RegistrationAttemptsPerHour"], out var registrationAttemptsPerHour))
                    registrationAttemptsPerHour = 3;
                options.PermitLimit = registrationAttemptsPerHour;
                options.Window = TimeSpan.FromHours(1);
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                options.QueueLimit = 0;
            });
        });

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
            });
        });

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        services.AddSignalR();
        services.AddSingleton<IConnectionManager, ConnectionManager>();
        services.AddHostedService<ExpiredCardNumberProcessor>();
        return services;
    }
}
