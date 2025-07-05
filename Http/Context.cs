using System.Net.Sockets;
using System.Text;

namespace Http.Request;

public delegate Task RequestDelegate(Context context);
public class Context
{
    public Request Request { get; }
    public Response Response { get; }
    public NetworkStream NetworkStream { get; }
    public bool ResponseSent { get; set; }
    
    public Context(Request request, NetworkStream networkStream)
    {
        Request = request;
        Response = new Response();
        NetworkStream = networkStream;
        ResponseSent = false;
    }
    
    public async Task SendResponseAsync()
    {
        if (ResponseSent) return;

        Response.Headers["Connection"] = "close";

        var headerBytes = Encoding.ASCII.GetBytes(Response.GetHeaderString());
        await NetworkStream.WriteAsync(headerBytes);

        if (Response.BodyBytes.Length > 0)
        {
            await NetworkStream.WriteAsync(Response.BodyBytes.AsMemory(0, Response.BodyBytes.Length));
        }
        ResponseSent = true;
    }
}

