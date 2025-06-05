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
        // Add Redis connection - NEVER use mock mode, always connect to real Redis
        services.AddSingleton<IConnectionMultiplexer>(provider =>
        {
            var logger = provider.GetService<ILogger<ConnectionMultiplexer>>();
            var redisConnectionString = configuration.GetConnectionString("Redis");
            
            if (string.IsNullOrEmpty(redisConnectionString))
            {
                logger?.LogError("Redis connection string is required but not found in configuration!");
                throw new InvalidOperationException("Redis connection string is required. Please check appsettings.json");
            }
            
            logger?.LogInformation("Connecting to Redis: {RedisHost}", redisConnectionString.Split(',')[0]);
            
            try
            {
                var options = ConfigurationOptions.Parse(redisConnectionString);
                
                // Enhanced connection settings for Azure Redis
                options.ConnectTimeout = 10000; // 10 seconds - more time for Azure Redis
                options.SyncTimeout = 5000; // 5 seconds for sync operations
                options.AsyncTimeout = 10000; // 10 seconds for async operations
                options.ConnectRetry = 3; // More retries for reliability
                options.ReconnectRetryPolicy = new ExponentialRetry(1000, 30000); // 1s to 30s retry
                options.KeepAlive = 60; // Keep connection alive
                options.DefaultDatabase = 0;
                options.AllowAdmin = false; // Security best practice
                
                var connection = ConnectionMultiplexer.Connect(options);
                
                // Test the connection immediately
                var database = connection.GetDatabase();
                var testKey = "connection:test:" + DateTime.UtcNow.Ticks;
                database.StringSet(testKey, "connected", TimeSpan.FromSeconds(10));
                var testValue = database.StringGet(testKey);
                database.KeyDelete(testKey);
                
                if (testValue != "connected")
                {
                    throw new InvalidOperationException("Redis connection test failed - could not read/write test data");
                }
                
                logger?.LogInformation("✅ Successfully connected to Redis and verified read/write operations");
                return connection;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "❌ FAILED to connect to Redis. Application cannot start without Redis connection.");
                throw new InvalidOperationException($"Redis connection failed: {ex.Message}", ex);
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