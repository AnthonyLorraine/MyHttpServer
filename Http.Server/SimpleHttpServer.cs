using System.Net;
using System.Net.Sockets;
using System.Text;
using Http.Request;

namespace Http.Server;

public class SimpleHttpServer
{
    private readonly int _port;
    private readonly TcpListener _server;
    private readonly RequestDelegate _pipeline;
    public SimpleHttpServer(int port, RequestDelegate pipeline)
    {
        _pipeline = pipeline;
        _port = port;
        _server = new TcpListener(IPAddress.Any, _port);
    }

    public async Task RunAsync()
    {
        try
        {
            _server.Start();
            Console.WriteLine($"Server started. Listening on port {_port}...");
            Console.WriteLine("Waiting for connections...");
         

            while (true)
            {
                var client = await _server.AcceptTcpClientAsync();
                Console.WriteLine($"Client connected from: {((IPEndPoint)client.Client.RemoteEndPoint!).Address}");
                await HandleClientAsync(client, _pipeline); 
            }
        }
        catch (SocketException e)
        {
            Console.WriteLine($"SocketException: {e.Message}");
        }
        finally
        {
            _server.Stop();
            Console.WriteLine("Server stopped.");
        }
    }
    
    private static async Task HandleClientAsync(TcpClient client, RequestDelegate pipeline)
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
            var request = Request.Request.Parse(requestString);

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
}