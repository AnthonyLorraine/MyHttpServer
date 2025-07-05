using Http.Request;
using HttpMethod = Http.Request.HttpMethod;

namespace Middleware;

public class StaticFileMiddleware : IMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _webRootPath;

    public StaticFileMiddleware(RequestDelegate next, string webRootPath)
    {
        _next = next;
        _webRootPath = webRootPath;
    }
    
    public async Task InvokeAsync(Context context)
    {
        if (context.Request.Method != HttpMethod.Get)
        {
            return;
        }

        var relativePath = context.Request.Path.TrimStart('/');
        var filePath = Path.Combine(_webRootPath, relativePath);

        if (!filePath.StartsWith(_webRootPath, StringComparison.OrdinalIgnoreCase) || !File.Exists(filePath))
        {
            await _next(context);
            return;
        }

        try
        {
            var fileBytes = await File.ReadAllBytesAsync(filePath);
            var contentType = GetContentType(filePath);
            context.Response.Ok();
            context.Response.SetBinaryContent(fileBytes, contentType);
            await context.SendResponseAsync();
        }
        catch (Exception ex)
        {
            context.Response.ServerError();
            await context.SendResponseAsync();
        }
    }
    
    private static string GetContentType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".html" => "text/html",
            ".css" => "text/css",
            ".js" => "application/javascript",
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".ico" => "image/x-icon",
            ".svg" => "image/svg+xml",
            _ => "application/octet-stream"
        };
    }
}