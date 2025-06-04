# 🚀 Azure Deployment Guide - AI First Demo

## 📋 **Overview**

This guide will walk you through deploying the AI First Demo to Azure using:
- **Azure App Service** for hosting the .NET API and React frontend
- **Azure Cache for Redis** for data storage
- **Azure OpenAI** for AI features
- **Configuration via Environment Variables** in Azure App Service

## 🔧 **Configuration Strategy**

### **Local Development**
- Configuration values are read from `appsettings.Development.json`
- Contains localhost Redis connection and development secrets
- Perfect for local debugging and development

### **Azure Production**
- Configuration values are **automatically overridden** by Azure App Service Application Settings
- Azure App Service converts Application Settings to environment variables
- .NET Configuration system automatically uses environment variables over appsettings.json values
- **No secrets stored in source code** - everything is configured through Azure Portal

### **How It Works**
1. **Local**: .NET reads from `appsettings.Development.json`
2. **Azure**: .NET reads from environment variables (set via Azure App Service Application Settings)
3. **Environment variables always take precedence** over appsettings.json values

## 🏗️ **Azure Resources Needed**

### 1. **Resource Group**
- Container for all your resources
- Choose a region close to your users

### 2. **Azure App Service Plan**
- **Recommended**: Standard S1 or higher
- Supports custom domains and SSL
- Auto-scaling capabilities

### 3. **Azure App Service (Web App)**
- Hosts both API and React frontend
- .NET 8 runtime
- Always On enabled

### 4. **Azure Cache for Redis**
- **Recommended**: Standard C1 or higher
- Primary data store for the application
- High availability and persistence

### 5. **Azure OpenAI Service**
- GPT-4 deployment for AI features
- Content filtering and safety features
- Usage monitoring and quotas

## 🚀 **Step-by-Step Deployment**

### **Phase 1: Create Azure Resources**

#### **1.1 Create Resource Group**
```bash
# Using Azure CLI
az group create --name "ai-first-demo-rg" --location "East US"
```

#### **1.2 Create App Service Plan**
```bash
az appservice plan create \
  --name "ai-first-demo-plan" \
  --resource-group "ai-first-demo-rg" \
  --sku S1 \
  --is-linux
```

#### **1.3 Create Web App**
```bash
az webapp create \
  --name "ai-first-demo-app" \
  --resource-group "ai-first-demo-rg" \
  --plan "ai-first-demo-plan" \
  --runtime "DOTNETCORE:8.0"
```

#### **1.4 Create Redis Cache**
```bash
az redis create \
  --name "ai-first-demo-redis" \
  --resource-group "ai-first-demo-rg" \
  --location "East US" \
  --sku Standard \
  --vm-size C1
```

#### **1.5 Create Azure OpenAI**
```bash
az cognitiveservices account create \
  --name "ai-first-demo-openai" \
  --resource-group "ai-first-demo-rg" \
  --location "East US" \
  --kind OpenAI \
  --sku S0
```

### **Phase 2: Configure Application Settings (Environment Variables)**

#### **2.1 Get Connection Strings**

**Redis Connection String:**
```bash
az redis list-keys --name "ai-first-demo-redis" --resource-group "ai-first-demo-rg"
```

**OpenAI Endpoint and Key:**
```bash
az cognitiveservices account show --name "ai-first-demo-openai" --resource-group "ai-first-demo-rg"
az cognitiveservices account keys list --name "ai-first-demo-openai" --resource-group "ai-first-demo-rg"
```

#### **2.2 Configure App Settings (THE IMPORTANT PART)**

**Using Azure CLI:**
```bash
az webapp config appsettings set \
  --name "ai-first-demo-app" \
  --resource-group "ai-first-demo-rg" \
  --settings \
    "ConnectionStrings__Redis=YOUR_REDIS_CONNECTION_STRING_HERE" \
    "AzureOpenAI__Endpoint=https://your-openai-instance.openai.azure.com/" \
    "AzureOpenAI__ApiKey=YOUR_OPENAI_API_KEY_HERE" \
    "AzureOpenAI__DeploymentName=gpt-4" \
    "AZURE_APP_URL=https://your-app-name.azurewebsites.net" \
    "ASPNETCORE_ENVIRONMENT=Production"
```

**Using Azure Portal:**
1. Go to your Azure App Service
2. Navigate to **Configuration** → **Application settings**
3. Add the following settings:

| Name | Value |
|------|-------|
| `ConnectionStrings__Redis` | `your-redis-cache.redis.cache.windows.net:6380,password=YOUR_KEY,ssl=True,abortConnect=False` |
| `AzureOpenAI__Endpoint` | `https://your-openai-instance.openai.azure.com/` |
| `AzureOpenAI__ApiKey` | `your-openai-api-key` |
| `AzureOpenAI__DeploymentName` | `gpt-4` |
| `AZURE_APP_URL` | `https://your-app-name.azurewebsites.net` |
| `ASPNETCORE_ENVIRONMENT` | `Production` |

### **Phase 3: Deploy Application**

#### **3.1 Build and Deploy Using PowerShell Script**
```powershell
# Run the build script from project root
./build-azure.ps1

# This will create a ./publish folder ready for deployment
```

#### **3.2 Deploy Using Azure CLI**
```bash
# Create ZIP file from publish folder
Compress-Archive -Path "./publish/*" -DestinationPath "./deployment.zip"

# Deploy to Azure
az webapp deployment source config-zip \
  --name "ai-first-demo-app" \
  --resource-group "ai-first-demo-rg" \
  --src "./deployment.zip"
```

#### **3.3 Alternative: Deploy Using Azure Portal**
1. Run `./build-azure.ps1` to create publish folder
2. Zip the `./publish` folder
3. Go to Azure App Service → **Deployment Center** → **ZIP Deploy**
4. Upload the ZIP file

## ✅ **Configuration Verification**

### **How .NET Configuration Works:**

1. **Priority Order** (highest to lowest):
   - Environment Variables (Azure App Service Application Settings)
   - appsettings.Production.json
   - appsettings.json

2. **In Development:**
   - Reads from `appsettings.Development.json`
   - Contains localhost values for local testing

3. **In Azure:**
   - Azure App Service converts Application Settings to environment variables
   - Environment variables **automatically override** appsettings.json values
   - No code changes needed!

### **Example:**
```csharp
// This code works in both environments:
var redisConnection = configuration.GetConnectionString("Redis");

// Locally: reads from appsettings.Development.json
// Azure: reads from ConnectionStrings__Redis environment variable
```

## 🔍 **Verification & Testing**

### **1. Verify Configuration**
```bash
# Check app settings are configured correctly
az webapp config appsettings list --name "ai-first-demo-app" --resource-group "ai-first-demo-rg"
```

### **2. Health Checks**
- Visit: `https://your-app.azurewebsites.net/swagger`
- Check: API documentation loads
- Test: Create a session via API

### **3. Frontend Verification**
- Visit: `https://your-app.azurewebsites.net`
- Check: React app loads correctly
- Test: Navigation between features

### **4. Feature Testing**
- Test: Create user session
- Test: Take quiz with AI hints
- Test: Play spaceship game
- Test: View analytics

## 🚨 **Troubleshooting**

### **Common Configuration Issues:**

#### **1. "Redis connection timeout"**
```bash
# Check your Redis connection string format:
# Correct: "cachename.redis.cache.windows.net:6380,password=KEY,ssl=True,abortConnect=False"
# Wrong: Missing ssl=True or wrong port
```

#### **2. "OpenAI API errors"**
```bash
# Verify settings match exactly:
az webapp config appsettings list --name "your-app" --resource-group "your-rg" | grep -i openai
```

#### **3. "Application failed to start"**
```bash
# View real-time logs
az webapp log tail --name "your-app" --resource-group "your-rg"
```

### **Debugging Commands:**
```bash
# Check all environment variables
az webapp config appsettings list --name "ai-first-demo-app" --resource-group "ai-first-demo-rg"

# View application logs
az webapp log tail --name "ai-first-demo-app" --resource-group "ai-first-demo-rg"

# Restart application
az webapp restart --name "ai-first-demo-app" --resource-group "ai-first-demo-rg"
```

## 🔐 **Security Best Practices**

### **✅ What We Do Right:**
- **No secrets in source code** - everything in Azure App Service settings
- **Environment variables override** configuration files
- **Production secrets isolated** from development environment
- **Connection strings encrypted** in Azure App Service

### **✅ Configuration Files:**
- `appsettings.Development.json` - Contains localhost settings for development
- `appsettings.Production.json` - Contains placeholder values (overridden by Azure)
- **Real secrets only exist in Azure App Service Application Settings**

## 💰 **Cost Optimization**

### **Development Environment:**
- App Service: Basic B1 ($~13/month)
- Redis: Basic C0 ($~16/month)
- OpenAI: Pay-per-use (varies)

### **Production Environment:**
- App Service: Standard S1 ($~56/month)
- Redis: Standard C1 ($~25/month)
- OpenAI: Pay-per-use (varies)

## 🎉 **Success!**

Your AI First Demo is now running on Azure with proper configuration management!

**Key Benefits:**
- ✅ **Secure**: No secrets in source code
- ✅ **Simple**: Environment variables automatically override settings  
- ✅ **Flexible**: Easy to change configuration without code changes
- ✅ **Best Practice**: Follows .NET configuration conventions

**What happens:**
1. **Development**: Uses `appsettings.Development.json` (localhost values)
2. **Azure**: Uses Azure App Service Application Settings (production values)
3. **Automatic**: No code changes needed between environments! 