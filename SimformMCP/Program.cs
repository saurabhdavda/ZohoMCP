﻿var basePath = AppContext.BaseDirectory;

// If launched by VS Code via stdio
if (args.Contains("--stdio"))
{
    var stdioBuilder = Host.CreateApplicationBuilder(args);

    stdioBuilder.Logging.AddConsole(o =>
        o.LogToStandardErrorThreshold = LogLevel.Trace);

    stdioBuilder.Configuration.AddJsonFile(
    Path.Combine(basePath, "appsettings.json"), optional: true)
    .AddEnvironmentVariables();

    stdioBuilder.Services
        .AddHttpClient()
        .AddSingleton<ZohoAuthService>()
        .AddSingleton<ZohoService>();

    stdioBuilder.Services
        .AddMcpServer()
        .WithStdioServerTransport()
        .WithToolsFromAssembly();

    await stdioBuilder.Build().RunAsync();
    return;
}

// HTTP/SSE mode for Power Automate & Teams
var webBuilder = WebApplication.CreateBuilder(args);

webBuilder.Logging.AddConsole(o =>
    o.LogToStandardErrorThreshold = LogLevel.Trace);

webBuilder.Configuration.AddJsonFile(Path.Combine(basePath, "appsettings.json"), optional: true)
.AddEnvironmentVariables();

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
webBuilder.WebHost.UseUrls($"http://0.0.0.0:{port}");

webBuilder.Services
    .AddHttpClient()
    .AddSingleton<ZohoAuthService>()
    .AddSingleton<ZohoService>();

webBuilder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = webBuilder.Build();

app.MapGet("/", () => "Zoho MCP Server Running");

app.MapMcp("/mcp");
// app.MapMcp();

await app.RunAsync();