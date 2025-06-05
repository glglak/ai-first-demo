using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AiFirstDemo.Infrastructure.Redis;
using AiFirstDemo.Infrastructure.OpenAI;
using StackExchange.Redis;
using Microsoft.Extensions.Logging;
using Azure.AI.OpenAI;
using Azure;

namespace AiFirstDemo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Redis connection with proper error handling
        services.AddSingleton<IConnectionMultiplexer>(provider =>
        {
            var logger = provider.GetService<ILogger<ConnectionMultiplexer>>();
            var redisConnectionString = configuration.GetConnectionString("Redis");
            
            if (string.IsNullOrEmpty(redisConnectionString))
            {
                logger?.LogWarning("No Redis connection string found. Running in mock mode - some features may not work properly.");
                return null!;
            }
            
            try
            {
                logger?.LogInformation("Attempting to connect to Azure Redis...");
                var options = ConfigurationOptions.Parse(redisConnectionString);
                
                // Set very aggressive timeouts to prevent hanging
                options.ConnectTimeout = 3000;      // 3 seconds to connect
                options.SyncTimeout = 1000;         // 1 second for sync operations
                options.AsyncTimeout = 2000;        // 2 seconds for async operations
                options.AbortOnConnectFail = false; // Don't fail startup if Redis is unavailable
                options.ConnectRetry = 1;           // Only 1 retry attempt for faster failover
                options.ReconnectRetryPolicy = new ExponentialRetry(250); // Faster retry policy
                
                // Add additional resilience settings
                options.KeepAlive = 60;         // Keep connection alive
                options.DefaultDatabase = 0;
                options.AllowAdmin = false;     // Security best practice
                
                // Add a task timeout wrapper to prevent indefinite hanging
                var connectionTask = Task.Run(() => ConnectionMultiplexer.Connect(options));
                
                if (connectionTask.Wait(TimeSpan.FromSeconds(5))) // 5 second total timeout
                {
                    var connection = connectionTask.Result;
                    logger?.LogInformation("Redis connection established successfully");
                    return connection;
                }
                else
                {
                    logger?.LogWarning("Redis connection timed out after 5 seconds. Running in mock mode - some features may not work properly.");
                    return null!;
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Failed to connect to Redis. Running in mock mode - some features may not work properly.");
                return null!;
            }
        });
        
        // Add Redis services
        services.AddScoped<IRedisService, RedisService>();
        
        // Add Azure OpenAI client
        services.AddSingleton<OpenAIClient>(provider =>
        {
            var logger = provider.GetService<ILogger<OpenAIClient>>();
            var endpoint = configuration["AzureOpenAI:Endpoint"];
            var apiKey = configuration["AzureOpenAI:ApiKey"];
            
            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
            {
                logger?.LogWarning("Azure OpenAI configuration is missing. Some AI features may not work.");
                throw new InvalidOperationException("Azure OpenAI configuration is required. Please check your appsettings.json file.");
            }
            
            try
            {
                var client = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
                logger?.LogInformation("Azure OpenAI client initialized successfully");
                return client;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Failed to initialize Azure OpenAI client");
                throw;
            }
        });
        
        // Add OpenAI services
        services.AddScoped<IOpenAIService, OpenAIService>();
        
        return services;
    }
} 