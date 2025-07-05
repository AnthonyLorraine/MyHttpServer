using Http.Request;

namespace Middleware;

public interface IMiddleware
{
    Task InvokeAsync(Context context);
}