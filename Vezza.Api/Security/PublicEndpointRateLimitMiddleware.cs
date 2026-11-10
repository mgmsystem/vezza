using System.Collections.Concurrent;

namespace Vezza.Api.Security;

public class PublicEndpointRateLimitMiddleware
{
    private static readonly HashSet<string> LimitedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/v1/usuarios/autenticar",
        "/v1/usuarios/registrar",
        "/v1/usuarios/autorizacao",
        "/v1/usuarios/alterar-senha"
    };

    private readonly RequestDelegate _next;
    private readonly ConcurrentDictionary<string, RateLimitEntry> _entries = new();
    private readonly int _permitLimit;
    private readonly TimeSpan _window;

    public PublicEndpointRateLimitMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _permitLimit = configuration.GetValue<int?>("Security:PublicEndpointRateLimit:PermitLimit") ?? 5;
        var windowSeconds = configuration.GetValue<int?>("Security:PublicEndpointRateLimit:WindowSeconds") ?? 60;
        _window = TimeSpan.FromSeconds(windowSeconds);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!HttpMethods.IsPost(context.Request.Method) ||
            !LimitedPaths.Contains(context.Request.Path.Value ?? string.Empty))
        {
            await _next(context);
            return;
        }

        var clientAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var key = $"{clientAddress}:{context.Request.Path.Value}";
        var entry = _entries.GetOrAdd(key, _ => new RateLimitEntry(DateTime.UtcNow));

        if (!entry.TryAcquire(DateTime.UtcNow, _window, _permitLimit))
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                "{\"statusCode\":429,\"errors\":[\"Muitas tentativas. Tente novamente mais tarde.\"]}");
            return;
        }

        await _next(context);
    }

    private sealed class RateLimitEntry
    {
        private readonly object _lock = new();
        private DateTime _windowStartedAt;
        private int _requests;

        public RateLimitEntry(DateTime windowStartedAt)
        {
            _windowStartedAt = windowStartedAt;
        }

        public bool TryAcquire(DateTime now, TimeSpan window, int permitLimit)
        {
            lock (_lock)
            {
                if (now - _windowStartedAt >= window)
                {
                    _windowStartedAt = now;
                    _requests = 0;
                }

                if (_requests >= permitLimit)
                    return false;

                _requests++;
                return true;
            }
        }
    }
}
