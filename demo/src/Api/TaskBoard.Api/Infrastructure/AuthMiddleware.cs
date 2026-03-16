namespace TaskBoard.Api.Infrastructure;

public class AuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public AuthMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
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
            await context.Response.WriteAsync("Authentication required.");
            return;
        }

        var configuredKey = _configuration["Authentication:ApiKey"];
        if (apiKey != configuredKey)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Authentication required.");
            return;
        }

        await _next(context);
    }
}
