using System.Net;

namespace Legend2Toolbox.Infrastructure.Auditing;

public sealed class AuditOptions
{
    public string SpoolDirectory { get; set; } = Path.Combine(AppContext.BaseDirectory, "audit-pending");
    public string[] TrustedProxies { get; set; } = [];
    public int ForwardLimit { get; set; } = 3;
    public int WriteTimeoutSeconds { get; set; } = 3;
}

public static class AuditIpResolver
{
    public static (string? ClientIp, string? PeerIp) Resolve(HttpContext? context, AuditOptions options)
    {
        var peer = Normalize(context?.Connection.RemoteIpAddress);
        var client = peer;
        if (context is null || peer is null) return (client, peer);
        var trusted = options.TrustedProxies.Select(x =>
            IPAddress.TryParse(x, out var address) ? Normalize(address) : null).ToHashSet();
        // Walk from the immediate peer towards the client; never trust an arbitrary first header.
        var forwarded = context.Request.Headers["X-Forwarded-For"].ToString();
        if (forwarded.Length > 2048) return (client, peer);
        var chain = forwarded.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var limit = Math.Clamp(options.ForwardLimit, 0, 10);
        for (var i = chain.Length - 1; i >= 0 && chain.Length - i <= limit; i--)
        {
            if (!trusted.Contains(client) || !IPAddress.TryParse(chain[i], out var address)) break;
            client = Normalize(address);
        }
        return (client, peer);
    }

    private static string? Normalize(IPAddress? address) =>
        address?.IsIPv4MappedToIPv6 == true ? address.MapToIPv4().ToString() : address?.ToString();
}
