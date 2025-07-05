namespace Http.Request;

public class Request
{
    public HttpMethod Method { get; private set; } = HttpMethod.Get;
    public string Path { get; private set; } = null!;
    public string HttpVersion { get; private set; } = null!;
    public string Body { get; private set; } = string.Empty;
    public Dictionary<string, string> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);

    public static Request Parse(string rawRequest)
    {
        var request = new Request();
        var requestLines = rawRequest.Split("\r\n");

        var requestLineParts = requestLines[0].Split(' ');
        if (requestLineParts.Length >= 3)
        {
            try
            {
                var validMethod = Enum.Parse<HttpMethod>(requestLineParts[0], true);
                request.Method = validMethod;
            }
            catch
            {
                throw new InvalidHttpRequestException("Invalid Http Request, Missing Method, Path or Http Version");
            }
           
            request.Path = requestLineParts[1];
            request.HttpVersion = requestLineParts[2];
        }
        else
        {
            throw new InvalidHttpRequestException("Invalid Http Request, Missing Method, Path or Http Version");
        }

        var lineIndex = 1;

        while (lineIndex < requestLines.Length && !string.IsNullOrWhiteSpace(requestLines[lineIndex]))
        {
            var header = requestLines[lineIndex];
            var seperator = header.IndexOf(':');
            if (seperator > 0)
            {
                var key = header[..seperator].Trim();
                var value = header[(seperator + 1)..].Trim();
                request.Headers[key] = value;
            }
            lineIndex++;
        }

        if (lineIndex < requestLines.Length)
        {
            request.Body = string.Join("\r\n", requestLines.Skip(lineIndex + 1));
        }
        
        return request;
    }
}