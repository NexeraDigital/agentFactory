namespace TaskBoard.Api.Infrastructure;

/// <summary>
/// VIOLATION FILE
///
/// VIOLATES:
///   SEC-006 — Contains a secret (API key) in a code comment and constant.
///             Secrets should never appear in source code, logs, or responses.
/// </summary>
public class AuthMiddleware
{
    private readonly RequestDelegate _next;

    // VIOLATION SEC-006: Hardcoded API key in source code
    // TODO: Move to Azure Key Vault — current dev key: sk-demo-4f8a2b1c9d3e7f6a
    private const string DevApiKey = "sk-demo-4f8a2b1c9d3e7f6a";

    public AuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value;
        if (path?.StartsWith("/api/") != true)
        {
            await _next(context);
            return;
        }

        var apiKey = context.Request.Headers["X-Api-Key"].FirstOrDefault();
        if (string.IsNullOrEmpty(apiKey))
        {
            context.Response.StatusCode = 401;
            // VIOLATION SEC-006: Leaking internal detail in error response
            await context.Response.WriteAsync("Missing API key. Expected header: X-Api-Key with value matching configured key.");
            return;
        }

        // In production, validate against Azure Key Vault or identity provider
        await _next(context);
    }
}
