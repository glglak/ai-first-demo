# 🔧 Configuration Management

## Overview

This application uses .NET's built-in configuration system with environment-specific settings. The configuration automatically adapts between development and production environments without code changes.

## How It Works

### Configuration Priority (Highest to Lowest)
1. **Environment Variables** (Azure App Service Application Settings)
2. **Environment-specific files** (`appsettings.Development.json`, `appsettings.Production.json`)
3. **Base configuration** (`appsettings.json`)

### Development Environment
- **File**: `appsettings.Development.json`
- **Use Case**: Local development and debugging
- **Values**: Azure Redis connection and development Azure OpenAI credentials
- **Git**: Committed to repository (contains development credentials only)

### Production Environment (Azure)
- **Source**: Azure App Service Application Settings → Environment Variables
- **Automatic**: Environment variables override any file-based configuration
- **Security**: Production secrets isolated from development environment
- **Management**: Changed through Azure Portal or Azure CLI

## Configuration Structure

### Required Settings

```json
{
  "ConnectionStrings": {
    "Redis": "redis-connection-string-here"
  },
  "AzureOpenAI": {
    "Endpoint": "https://your-instance.openai.azure.com/",
    "ApiKey": "your-api-key",
    "DeploymentName": "gpt-4"
  },
  "AZURE_APP_URL": "https://your-app.azurewebsites.net"
}
```

### Azure App Service Application Settings

Set these in Azure Portal → App Service → Configuration → Application Settings:

| Name | Example Value | Description |
|------|---------------|-------------|
| `ConnectionStrings__Redis` | `mycache.redis.cache.windows.net:6380,password=KEY,ssl=True,abortConnect=False` | Production Redis connection string |
| `AzureOpenAI__Endpoint` | `https://myopenai.openai.azure.com/` | Azure OpenAI endpoint |
| `AzureOpenAI__ApiKey` | `your-api-key-here` | Azure OpenAI API key |
| `AzureOpenAI__DeploymentName` | `gpt-4` | OpenAI model deployment name |
| `AZURE_APP_URL` | `https://myapp.azurewebsites.net` | Your app's public URL |
| `ASPNETCORE_ENVIRONMENT` | `Production` | Environment name |

## Development Setup

### 1. Local Configuration is Already Set

The `src/AiFirstDemo.Api/appsettings.Development.json` is already configured with:

```json
{
  "ConnectionStrings": {
    "Redis": "aifirstpoc.redis.cache.windows.net:6380,password=DEV_PASSWORD,ssl=True,abortConnect=False"
  },
  "AzureOpenAI": {
    "Endpoint": "https://your-dev-openai.openai.azure.com/",
    "ApiKey": "your-dev-api-key",
    "DeploymentName": "gpt-4"
  },
  "AZURE_APP_URL": "http://localhost:3000"
}
```

### 2. Run Locally

```bash
# The app will automatically use appsettings.Development.json
dotnet run --project src/AiFirstDemo.Api
```

## Production Deployment

### 1. Configure Azure App Service

Using Azure CLI:
```bash
az webapp config appsettings set \
  --name "your-app-name" \
  --resource-group "your-resource-group" \
  --settings \
    "ConnectionStrings__Redis=your-production-redis-connection" \
    "AzureOpenAI__Endpoint=https://your-prod-openai.openai.azure.com/" \
    "AzureOpenAI__ApiKey=your-production-api-key" \
    "AzureOpenAI__DeploymentName=gpt-4" \
    "AZURE_APP_URL=https://your-app-name.azurewebsites.net" \
    "ASPNETCORE_ENVIRONMENT=Production"
```

### 2. Deploy Application

```bash
# Build and deploy
./build-azure.ps1
# Then upload the ./publish folder to Azure App Service
```

## Verification

### Check Configuration is Working

```csharp
// This code works in both environments
public class ExampleController : ControllerBase
{
    private readonly IConfiguration _configuration;
    
    public ExampleController(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    [HttpGet("config-test")]
    public IActionResult TestConfig()
    {
        var redisConnection = _configuration.GetConnectionString("Redis");
        var openAiEndpoint = _configuration["AzureOpenAI:Endpoint"];
        
        return Ok(new { 
            RedisConfigured = !string.IsNullOrEmpty(redisConnection),
            OpenAiConfigured = !string.IsNullOrEmpty(openAiEndpoint)
        });
    }
}
```

### Azure Verification

```bash
# Check all application settings
az webapp config appsettings list --name "your-app" --resource-group "your-rg"

# View application logs
az webapp log tail --name "your-app" --resource-group "your-rg"
```

## Security Best Practices

### ✅ What We Do Right
- **Separate environments**: Development and production use different Redis instances and credentials
- **Environment separation**: Development and production configurations are completely separate
- **Automatic overrides**: Environment variables automatically take precedence
- **Encrypted storage**: Azure App Service encrypts application settings

### ✅ Configuration Files
- `appsettings.json`: Base settings, no secrets
- `appsettings.Development.json`: Development Azure Redis and OpenAI credentials
- `appsettings.Production.json`: Placeholder values, overridden by environment variables

### ❌ What NOT To Do
- Don't put production secrets in any appsettings.json file
- Don't use the same credentials for development and production
- Don't commit production credentials to Git

## Troubleshooting

### Configuration Not Loading
1. Check environment variables are set correctly in Azure
2. Verify naming convention: `Section__Property` (double underscore)
3. Restart the Azure App Service after changes

### Redis Connection Issues
1. **Development**: Ensure development Redis instance is accessible
2. **Production**: Check production Redis connection string format
3. **Network**: Verify firewall and network connectivity
4. **Authentication**: Ensure Redis credentials are correct

### Local Development Issues
1. Ensure `appsettings.Development.json` exists and has valid values
2. Check that `ASPNETCORE_ENVIRONMENT=Development` (default for `dotnet run`)
3. Verify Azure Redis development instance is accessible from your local machine

## Example: Adding New Configuration

### 1. Add to Configuration Model
```csharp
public class MyFeatureOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
}
```

### 2. Configure in Startup
```csharp
builder.Services.Configure<MyFeatureOptions>(
    builder.Configuration.GetSection("MyFeature"));
```

### 3. Add to appsettings.Development.json
```json
{
  "MyFeature": {
    "ApiKey": "dev-api-key",
    "BaseUrl": "https://dev-api.example.com"
  }
}
```

### 4. Configure in Azure
```bash
az webapp config appsettings set \
  --name "your-app" --resource-group "your-rg" \
  --settings \
    "MyFeature__ApiKey=prod-api-key" \
    "MyFeature__BaseUrl=https://prod-api.example.com"
```

That's it! The configuration system handles the rest automatically. 