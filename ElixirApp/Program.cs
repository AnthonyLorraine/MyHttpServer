using System.Net;
using System.Net.Sockets;
using System.Text;
using Http.Request;
using Middleware;

Console.WriteLine("Starting TCP Echo Server (Asynchronous)...");
const string webRootPath = "wwwroot";
var server = new TcpListener(IPAddress.Any, 80);
if (!Directory.Exists(webRootPath))
{
    Directory.CreateDirectory(webRootPath);
    Console.WriteLine($"Created '{webRootPath}' directory. Please place your static files (e.g., favicon.ico) inside.");
}

try
{
    server.Start();
    Console.WriteLine("Server started. Listening on port 5000...");
    Console.WriteLine("Waiting for connections...");
    RequestDelegate app = new NotFoundMiddleware().InvokeAsync;
    app = new RootPageMiddleware(app, webRootPath).InvokeAsync;
    app = new StaticFileMiddleware(app, webRootPath).InvokeAsync;

    while (true)
    {
        var client = await server.AcceptTcpClientAsync();
        Console.WriteLine($"Client connected from: {((IPEndPoint)client.Client.RemoteEndPoint!).Address}");
        _ = HandleClientAsync(client, app); 
    }
}
catch (SocketException e)
{
    Console.WriteLine($"SocketException: {e.Message}");
}
finally
{
    server.Stop();
    Console.WriteLine("Server stopped.");
}

return;

static async Task HandleClientAsync(TcpClient client, RequestDelegate pipeline)
{
        var ns = client.GetStream();
        var remoteEndPoint = (IPEndPoint?)client.Client.RemoteEndPoint;

        try
        {
            var requestBuffer = new byte[1024]; 
            var bytesRead = await ns.ReadAsync(requestBuffer);
            if (bytesRead == 0)
            {
                return;
            }
            
            var requestString = Encoding.ASCII.GetString(requestBuffer, 0, bytesRead);
            var request = Request.Parse(requestString);

            var context = new Context(request, ns);
            await pipeline(context);

            if (!context.ResponseSent)
            {
                context.Response.ServerError();
                await context.SendResponseAsync();
            }
            
        }
        catch (IOException ex) when (ex.InnerException is SocketException se && se.SocketErrorCode == SocketError.ConnectionReset)
        {
            Console.WriteLine($"Client {remoteEndPoint.Address} forcibly disconnected (Connection Reset).");
        }
        catch (SocketException e)
        {
            Console.WriteLine($"SocketException for client {remoteEndPoint.Address}: {e.Message}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"An unexpected error occurred with client {remoteEndPoint.Address}: {e.Message}");
        }
        finally
        {
            if (ns != null) ns.Close();
            if (client != null) client.Close();
            Console.WriteLine($"Client connection closed for: {remoteEndPoint.Address}");
        }
}