using Http.Request;

namespace Middleware;

public class NotFoundMiddleware : IMiddleware
{
    private readonly RequestDelegate? _next;
    public NotFoundMiddleware(RequestDelegate? next = null)
    {
        _next = next;
    }
    public async Task InvokeAsync(Context context)
    {
        context.Response.NotFound();
        await context.SendResponseAsync();
        return;
    }
}