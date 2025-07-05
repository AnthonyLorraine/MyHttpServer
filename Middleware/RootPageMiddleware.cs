using Http.Request;
using HttpMethod = Http.Request.HttpMethod;

namespace Middleware;

public class RootPageMiddleware : IMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _webRootPath;
    public RootPageMiddleware(RequestDelegate next, string webRootPath)
    {
        _next = next;
        _webRootPath = webRootPath;
    }
    public async Task InvokeAsync(Context context)
    {
        if (context.Request.Method != HttpMethod.Get)
        {
            await _next(context);
            return;
        }

        if (context.Request.Path != "/")
        {
            await _next(context);
            return;
        }

        if (!File.Exists($"{_webRootPath}/index.html"))
        {
            await _next(context);
            return;
        }

        var html = await File.ReadAllTextAsync($"{_webRootPath}/index.html");
        context.Response.Ok();
        context.Response.SetHtmlContent(html);
        await context.SendResponseAsync();
        return;
    }
}