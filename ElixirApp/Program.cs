using Http.Server;

var builder = new WebHostBuilder();

var app = builder.Build();

await app.RunAsync();