using Http.Server;

var builder = new WebHostBuilder();

builder.BasicWebHost();


var app = builder.Build();
await app.RunAsync();