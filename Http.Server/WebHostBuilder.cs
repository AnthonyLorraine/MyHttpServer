using Http.Request;
using Middleware;

namespace Http.Server;

public class WebHostBuilder
{
    private string _webRootPath = "wwwroot";
    private int _port = 8000;
    private readonly List<Func<RequestDelegate, RequestDelegate>> _middlewareFactories = [];

    public SimpleHttpServer Build()
    {
        var pipeline = BuildMiddlewarePipeline();
        return new SimpleHttpServer(_port, pipeline);
    }

    public WebHostBuilder UseWebRoot(string path)
    {
        _webRootPath = path;
        return this;
    }

    public WebHostBuilder UsePort(int port)
    {
        _port = port;
        return this;
    }
    
    public WebHostBuilder AddMiddleware<TMiddleware>() where TMiddleware : class
    {
        _middlewareFactories.Add(next =>
        {
            try
            {
                var ctor = typeof(TMiddleware).GetConstructor([typeof(RequestDelegate)]);
                if (ctor is null)
                {
                    throw new InvalidOperationException(
                        $"Middleware '{typeof(TMiddleware).Name}' must have a public constructor that takes a single '{nameof(RequestDelegate)}'");
                }

                var instance = (TMiddleware)ctor.Invoke(new object[] { next });
                var invokeMethod = typeof(TMiddleware).GetMethod("InvokeAsync", [typeof(Context)]);
                if (invokeMethod == null || invokeMethod.ReturnType != typeof(Task))
                {
                    throw new InvalidOperationException(
                        $"Middleware '{typeof(TMiddleware).Name}' must have a public 'InvokeAsync(HttpContext context)' method returning Task."
                    );
                }

                return (RequestDelegate)invokeMethod.CreateDelegate(typeof(RequestDelegate), instance);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to create middleware instance for '{typeof(TMiddleware).Name}'. " +
                    $"Ensure it has a suitable constructor and InvokeAsync method.", ex
                );
            }
        });
        return this;
    }

    private RequestDelegate BuildMiddlewarePipeline()
    {
        if (!Directory.Exists(_webRootPath))
        {
            Directory.CreateDirectory(_webRootPath);
        }

        RequestDelegate app = new NotFoundMiddleware().InvokeAsync;
        app = new RootPageMiddleware(app, _webRootPath).InvokeAsync;
        app = new StaticFileMiddleware(app, _webRootPath).InvokeAsync;
        for (var i = _middlewareFactories.Count - 1; i >= 0 ; i--)
        {
            app = _middlewareFactories[i](app);
        }

        return app;
    }
    
}