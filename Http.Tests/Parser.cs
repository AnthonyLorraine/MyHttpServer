using HttpMethod = Http.Request.HttpMethod;

namespace Http.Tests;

public class Parser
{
    [Fact]
    public void ParsesGetRawRequest()
    {
        const string rawRequest = """
                                  GET /favicon.ico HTTP/1.1
                                  Host: localhost
                                  Connection: keep-alive
                                  sec-ch-ua-platform: "Windows"
                                  User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/137.0.0.0 Safari/537.36
                                  sec-ch-ua: "Google Chrome";v="137", "Chromium";v="137", "Not/A)Brand";v="24"
                                  sec-ch-ua-mobile: ?0
                                  Accept: image/avif,image/webp,image/apng,image/svg+xml,image/*,*/*;q=0.8
                                  Sec-Fetch-Site: same-origin
                                  Sec-Fetch-Mode: no-cors
                                  Sec-Fetch-Dest: image
                                  Referer: http://localhost/
                                  Accept-Encoding: gzip, deflate, br, zstd
                                  Accept-Language: en-AU,en;q=0.9,pl;q=0.8

                                  """;
        var request = Request.Request.Parse(rawRequest);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal("/favicon.ico", request.Path);
        Assert.Equal("HTTP/1.1", request.HttpVersion);
        Assert.Empty(request.Body);
        Assert.Equal(13, request.Headers.Count);
        
        Assert.True(request.Headers.ContainsKey("Host"));
        Assert.Equal("localhost", request.Headers["Host"]);

        Assert.True(request.Headers.ContainsKey("Connection"));
        Assert.Equal("keep-alive", request.Headers["Connection"]);

        Assert.True(request.Headers.ContainsKey("User-Agent"));
        Assert.Equal("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/137.0.0.0 Safari/537.36", request.Headers["User-Agent"]);

        Assert.True(request.Headers.ContainsKey("Accept"));
        Assert.Equal("image/avif,image/webp,image/apng,image/svg+xml,image/*,*/*;q=0.8", request.Headers["Accept"]);

        Assert.True(request.Headers.ContainsKey("Referer"));
        Assert.Equal("http://localhost/", request.Headers["Referer"]);

        Assert.True(request.Headers.ContainsKey("Accept-Encoding"));
        Assert.Equal("gzip, deflate, br, zstd", request.Headers["Accept-Encoding"]);

        Assert.True(request.Headers.ContainsKey("Accept-Language"));
        Assert.Equal("en-AU,en;q=0.9,pl;q=0.8", request.Headers["Accept-Language"]);

        Assert.True(request.Headers.ContainsKey("sec-ch-ua-platform"));
        Assert.Equal("\"Windows\"", request.Headers["sec-ch-ua-platform"]);

        Assert.True(request.Headers.ContainsKey("sec-ch-ua"));
        Assert.Equal("\"Google Chrome\";v=\"137\", \"Chromium\";v=\"137\", \"Not/A)Brand\";v=\"24\"", request.Headers["sec-ch-ua"]);

        Assert.True(request.Headers.ContainsKey("sec-ch-ua-mobile"));
        Assert.Equal("?0", request.Headers["sec-ch-ua-mobile"]);

        Assert.True(request.Headers.ContainsKey("Sec-Fetch-Site"));
        Assert.Equal("same-origin", request.Headers["Sec-Fetch-Site"]);

        Assert.True(request.Headers.ContainsKey("Sec-Fetch-Mode"));
        Assert.Equal("no-cors", request.Headers["Sec-Fetch-Mode"]);

        Assert.True(request.Headers.ContainsKey("Sec-Fetch-Dest"));
        Assert.Equal("image", request.Headers["Sec-Fetch-Dest"]);
    }
    
    [Fact]
    public void ParsesPostRawRequest()
    {
        const string rawRequest = """
                                  POST / HTTP/1.1
                                  Content-Type: application/json
                                  Content-Length: 37
                                  User-Agent: IntelliJ HTTP Client/JetBrains Rider 2025.1.3
                                  Accept-Encoding: br, deflate, gzip, x-gzip
                                  Accept: */*
                                  host: localhost
                                  
                                  {
                                    "id": 999,
                                    "value": "content"
                                  }
                                  
                                  """;
        var request = Request.Request.Parse(rawRequest);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("/", request.Path);
        Assert.Equal("HTTP/1.1", request.HttpVersion);
        Assert.Equal("{\r\n  \"id\": 999,\r\n  \"value\": \"content\"\r\n}\r\n", request.Body);
        Assert.Equal(6, request.Headers.Count);
        
        Assert.True(request.Headers.ContainsKey("Content-Type"));
        Assert.Equal("application/json", request.Headers["Content-Type"]);

        Assert.True(request.Headers.ContainsKey("Content-Length"));
        Assert.Equal("37", request.Headers["Content-Length"]);

        Assert.True(request.Headers.ContainsKey("User-Agent"));
        Assert.Equal("IntelliJ HTTP Client/JetBrains Rider 2025.1.3", request.Headers["User-Agent"]);

        Assert.True(request.Headers.ContainsKey("Accept"));
        Assert.Equal("*/*", request.Headers["Accept"]);

        Assert.True(request.Headers.ContainsKey("Host"));
        Assert.Equal("localhost", request.Headers["Host"]);
        
        Assert.True(request.Headers.ContainsKey("Accept-Encoding"));
        Assert.Equal("br, deflate, gzip, x-gzip", request.Headers["Accept-Encoding"]);
    }
}