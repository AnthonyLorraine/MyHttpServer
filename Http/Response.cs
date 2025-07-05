using System.Text;

namespace Http.Request;

public class Response
{
    public int StatusCode { get; set; }
    public string StatusText { get; set; }
    public Dictionary<string, string> Headers { get; private set; }
    public byte[] BodyBytes { get; set; } // Body as bytes for any content type

    public Response()
    {
        Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        StatusCode = 200; // Default to OK
        StatusText = "OK";
        BodyBytes = [];
    }

    /// <summary>
    /// Sets the content of the response with HTML.
    /// </summary>
    public void SetHtmlContent(string html)
    {
        BodyBytes = Encoding.UTF8.GetBytes(html);
        Headers["Content-Type"] = "text/html; charset=UTF-8";
        Headers["Content-Length"] = BodyBytes.Length.ToString();
    }

    /// <summary>
    /// Sets the content of the response with binary data (e.g., images).
    /// </summary>
    public void SetBinaryContent(byte[] data, string contentType)
    {
        BodyBytes = data;
        Headers["Content-Type"] = contentType;
        Headers["Content-Length"] = BodyBytes.Length.ToString();
    }

    /// <summary>
    /// Builds the full HTTP response string (headers only).
    /// </summary>
    public string GetHeaderString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"HTTP/1.1 {StatusCode} {StatusText}");
        foreach (var header in Headers)
        {
            sb.AppendLine($"{header.Key}: {header.Value}");
        }

        sb.AppendLine();
        return sb.ToString();
    }
}

public static class HttpResponse
{
    public static void BadRequest(this Response response)
    {
        response.StatusCode = 400;
        response.StatusText = "Bad Request";
        response.BodyBytes = [];
    }

    public static void NotFound(this Response response)
    {
        response.StatusCode = 404;
        response.StatusText = "Not Found";
        response.BodyBytes = [];
    }
    public static void ServerError(this Response response)
    {
        response.StatusCode = 500;
        response.StatusText = "Internal Server Error";
        response.BodyBytes = [];
    }

    public static void Ok(this Response response)
    {
        response.StatusCode = 200;
        response.StatusText = "OK";
        response.BodyBytes = [];
    }
    
}