using AiFirstDemo.Infrastructure;
using AiFirstDemo.Features;
using AiFirstDemo.Features.Shared.Hubs;
using Serilog;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add SignalR
builder.Services.AddSignalR();

// Add Response Compression for better performance
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
    {
        "application/json",
        "text/json",
        "text/css",
        "text/javascript",
        "application/javascript",
        "text/html",
        "text/xml",
        "text/plain"
    });
});

// Configure compression levels
builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

// Add Memory Cache for better performance
builder.Services.AddMemoryCache();

// Add CORS with Azure support
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        var allowedOrigins = new List<string>
        {
            "http://localhost:3000", 
            "http://localhost:5173"
        };

        // Add Azure App Service URL if configured
        var azureUrl = builder.Configuration["AZURE_APP_URL"];
        if (!string.IsNullOrEmpty(azureUrl))
        {
            allowedOrigins.Add(azureUrl);
            allowedOrigins.Add($"https://{azureUrl}");
        }

        policy.WithOrigins(allowedOrigins.ToArray())
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add Infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

// Add Feature services
builder.Services.AddFeatures();

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AiFirstDemo.Features.AssemblyReference).Assembly));

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

// Use Response Compression (should be early in pipeline)
app.UseResponseCompression();

// Serve static files for React app (for Azure single app deployment)
var staticFilesPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
if (Directory.Exists(staticFilesPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        OnPrepareResponse = ctx =>
        {
            // Cache static assets for 1 year
            if (ctx.File.Name.EndsWith(".js") || ctx.File.Name.EndsWith(".css") || 
                ctx.File.Name.EndsWith(".woff") || ctx.File.Name.EndsWith(".woff2") ||
                ctx.File.Name.EndsWith(".png") || ctx.File.Name.EndsWith(".jpg") ||
                ctx.File.Name.EndsWith(".svg") || ctx.File.Name.EndsWith(".ico"))
            {
                ctx.Context.Response.Headers.CacheControl = "public,max-age=31536000,immutable";
            }
            // Cache HTML files for 1 hour
            else if (ctx.File.Name.EndsWith(".html"))
            {
                ctx.Context.Response.Headers.CacheControl = "public,max-age=3600";
            }
        }
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowReactApp");

app.UseAuthorization();

app.MapControllers();

// Map SignalR hubs
app.MapHub<GameHub>("/hubs/game");
app.MapHub<AnalyticsHub>("/hubs/analytics");

// Fallback to index.html for React Router (SPA support)
app.MapFallbackToFile("index.html");

app.Run();