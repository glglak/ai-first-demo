# 🚀 Azure Deployment Guide - AI First Demo

## 📋 **Overview**

This guide will walk you through deploying the AI First Demo to Azure using:
- **Azure App Service** for hosting the .NET API and React frontend
- **Azure Cache for Redis** for data storage
- **Azure OpenAI** for AI features
- **Azure Application Insights** for monitoring

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

### **Phase 2: Configure Application Settings**

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

#### **2.2 Configure App Settings**
```bash
az webapp config appsettings set \
  --name "ai-first-demo-app" \
  --resource-group "ai-first-demo-rg" \
  --settings \
    "ConnectionStrings__Redis=YOUR_REDIS_CONNECTION_STRING" \
    "AzureOpenAI__Endpoint=https://ai-first-demo-openai.openai.azure.com/" \
    "AzureOpenAI__ApiKey=YOUR_OPENAI_API_KEY" \
    "AzureOpenAI__DeploymentName=gpt-4" \
    "ASPNETCORE_ENVIRONMENT=Production"
```

### **Phase 3: Prepare Application for Deployment**

#### **3.1 Update Production Configuration**

Create `src/AiFirstDemo.Api/appsettings.Production.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "Redis": "#{ConnectionStrings__Redis}#"
  },
  "AzureOpenAI": {
    "Endpoint": "#{AzureOpenAI__Endpoint}#",
    "ApiKey": "#{AzureOpenAI__ApiKey}#",
    "DeploymentName": "#{AzureOpenAI__DeploymentName}#"
  }
}
```

#### **3.2 Update Frontend for Production**

Update `src/AiFirstDemo.Web/vite.config.ts`:
```typescript
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  build: {
    outDir: '../AiFirstDemo.Api/wwwroot',
    emptyOutDir: true,
  },
  server: {
    proxy: {
      '/api': {
        target: process.env.VITE_API_URL || 'http://localhost:5001',
        changeOrigin: true,
        secure: false,
      }
    }
  }
})
```

#### **3.3 Build Frontend for Production**
```bash
# Navigate to frontend
cd src/AiFirstDemo.Web

# Install dependencies
npm install

# Build for production (outputs to API wwwroot)
npm run build
```

#### **3.4 Configure API to Serve Static Files**

Update `src/AiFirstDemo.Api/Program.cs`:
```csharp
// Add after var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();

// Add before app.Run();
app.MapFallbackToFile("index.html");
```

### **Phase 4: Deploy to Azure**

#### **4.1 Deploy Using Azure CLI**
```bash
# Navigate to API project
cd src/AiFirstDemo.Api

# Create deployment package
dotnet publish -c Release -o ./publish

# Deploy to Azure
az webapp deployment source config-zip \
  --name "ai-first-demo-app" \
  --resource-group "ai-first-demo-rg" \
  --src "./publish.zip"
```

#### **4.2 Alternative: Deploy Using Visual Studio**
1. Right-click on `AiFirstDemo.Api` project
2. Select "Publish"
3. Choose "Azure App Service"
4. Select your subscription and app service
5. Click "Publish"

### **Phase 5: Configure Custom Domain (Optional)**

#### **5.1 Add Custom Domain**
```bash
az webapp config hostname add \
  --webapp-name "ai-first-demo-app" \
  --resource-group "ai-first-demo-rg" \
  --hostname "yourdomain.com"
```

#### **5.2 Enable SSL**
```bash
az webapp config ssl bind \
  --name "ai-first-demo-app" \
  --resource-group "ai-first-demo-rg" \
  --certificate-thumbprint YOUR_CERT_THUMBPRINT \
  --ssl-type SNI
```

## 🔧 **Post-Deployment Configuration**

### **1. Enable Application Insights**
```bash
az monitor app-insights component create \
  --app "ai-first-demo-insights" \
  --location "East US" \
  --resource-group "ai-first-demo-rg"

# Get instrumentation key
az monitor app-insights component show \
  --app "ai-first-demo-insights" \
  --resource-group "ai-first-demo-rg"
```

### **2. Configure Monitoring**
Add to app settings:
```bash
az webapp config appsettings set \
  --name "ai-first-demo-app" \
  --resource-group "ai-first-demo-rg" \
  --settings \
    "APPLICATIONINSIGHTS_CONNECTION_STRING=YOUR_CONNECTION_STRING"
```

### **3. Enable Always On**
```bash
az webapp config set \
  --name "ai-first-demo-app" \
  --resource-group "ai-first-demo-rg" \
  --always-on true
```

### **4. Configure Auto-Scaling**
```bash
az monitor autoscale create \
  --resource-group "ai-first-demo-rg" \
  --resource "ai-first-demo-plan" \
  --resource-type Microsoft.Web/serverfarms \
  --name "ai-first-demo-autoscale" \
  --min-count 1 \
  --max-count 3 \
  --count 1
```

## 🔍 **Verification & Testing**

### **1. Health Checks**
- Visit: `https://your-app.azurewebsites.net/swagger`
- Check: API documentation loads
- Test: Create a session via API

### **2. Frontend Verification**
- Visit: `https://your-app.azurewebsites.net`
- Check: React app loads correctly
- Test: Navigation between features

### **3. Redis Connection**
- Check: Application Insights for Redis connection logs
- Test: Create and retrieve data

### **4. OpenAI Integration**
- Test: Generate quiz questions
- Check: AI responses are working

## 🚨 **Troubleshooting**

### **Common Issues:**

#### **1. "Application failed to start"**
- Check Application Insights logs
- Verify connection strings are correct
- Ensure .NET 8 runtime is selected

#### **2. "Redis connection timeout"**
- Verify Redis cache is running
- Check firewall settings
- Validate connection string format

#### **3. "OpenAI API errors"**
- Verify API key is correct
- Check deployment name matches
- Ensure quota limits aren't exceeded

#### **4. "Static files not served"**
- Verify frontend build completed
- Check wwwroot folder contains files
- Ensure UseStaticFiles() is configured

### **Debugging Commands:**
```bash
# View application logs
az webapp log tail --name "ai-first-demo-app" --resource-group "ai-first-demo-rg"

# Check app settings
az webapp config appsettings list --name "ai-first-demo-app" --resource-group "ai-first-demo-rg"

# Restart application
az webapp restart --name "ai-first-demo-app" --resource-group "ai-first-demo-rg"
```

## 💰 **Cost Optimization**

### **Development Environment:**
- App Service: Basic B1 ($~13/month)
- Redis: Basic C0 ($~16/month)
- OpenAI: Pay-per-use (varies)

### **Production Environment:**
- App Service: Standard S1 ($~56/month)
- Redis: Standard C1 ($~25/month)
- OpenAI: Pay-per-use (varies)

### **Cost-Saving Tips:**
1. Use **Dev/Test pricing** for non-production
2. Enable **auto-shutdown** for development environments
3. Monitor **OpenAI usage** to avoid unexpected costs
4. Use **Azure Cost Management** for budget alerts

## 🔄 **CI/CD Pipeline (Optional)**

### **GitHub Actions Workflow:**
Create `.github/workflows/azure-deploy.yml`:
```yaml
name: Deploy to Azure

on:
  push:
    branches: [ main ]

jobs:
  deploy:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
        
    - name: Setup Node.js
      uses: actions/setup-node@v3
      with:
        node-version: '18'
        
    - name: Build Frontend
      run: |
        cd src/AiFirstDemo.Web
        npm install
        npm run build
        
    - name: Build Backend
      run: |
        cd src/AiFirstDemo.Api
        dotnet publish -c Release -o ./publish
        
    - name: Deploy to Azure
      uses: azure/webapps-deploy@v2
      with:
        app-name: 'ai-first-demo-app'
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: './src/AiFirstDemo.Api/publish'
```

## 🎉 **Success!**

Your AI First Demo is now running on Azure! 

**Next Steps:**
1. Set up monitoring and alerts
2. Configure backup strategies
3. Implement security best practices
4. Plan for scaling and performance optimization

**Useful Links:**
- [Azure App Service Documentation](https://docs.microsoft.com/en-us/azure/app-service/)
- [Azure Cache for Redis Documentation](https://docs.microsoft.com/en-us/azure/azure-cache-for-redis/)
- [Azure OpenAI Documentation](https://docs.microsoft.com/en-us/azure/cognitive-services/openai/)

## Quick Deployment Package

To create a deployment package for Azure App Service, run this single command from the project root:

```bash
dotnet publish src/AiFirstDemo.Api/AiFirstDemo.Api.csproj -c Release -o ./publish --self-contained false
```

This will create a `./publish` folder containing everything needed for Azure App Service deployment.

## Deployment Steps

1. **Package the application** (run the command above)
2. **Zip the publish folder**: 
   - Right-click the `./publish` folder → "Send to" → "Compressed folder"
   - Or use: `Compress-Archive -Path ./publish/* -DestinationPath deployment.zip`
3. **Upload to Azure App Service**:
   - Go to your Azure App Service in the portal
   - Navigate to "Deployment Center" → "ZIP Deploy"
   - Upload the zip file

## Configuration

✅ **Production secrets are configured** in `appsettings.Production.json`
✅ **Secrets are protected** from Git commits via .gitignore
✅ **Static files are included** (React build is copied to wwwroot)

## Azure App Service Settings

Make sure your Azure App Service has these settings:

- **Runtime**: .NET 8
- **Platform**: 64-bit
- **Always On**: Enabled (for production)
- **ARR Affinity**: Disabled (for better performance)

## Environment Variables (Optional)

If you prefer environment variables over appsettings.json, you can set these in Azure:

```
ConnectionStrings__Redis=your-redis-connection-string
OpenAI__ApiKey=your-openai-api-key
OpenAI__BaseUrl=your-azure-openai-endpoint
```

## Verification

After deployment, test these endpoints:
- `https://your-app.azurewebsites.net/` - Should show the React app
- `https://your-app.azurewebsites.net/api/health` - Should return 200 OK
- `https://your-app.azurewebsites.net/api/analytics/dashboard` - Should return analytics data

## Notes

- The React app is built and included in the .NET publish output
- Static files are served with proper caching headers
- SignalR is configured for Azure App Service
- Logs are written to the file system (viewable in Kudu console) 