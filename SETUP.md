# AI-First Demo Setup Guide

## Overview

This guide will help you set up the AI-First Demo application, which showcases modern development practices using AI-assisted tools like **Cursor** and **Windsurf**. The application demonstrates full-stack development with .NET 8, React, Redis, and Azure OpenAI integration.

## ✅ Current Features

- **AI-Powered Quiz System** - Interactive quiz with OpenAI-generated hints
- **HTML5 Spaceship Game** - Classic Asteroids-style game with real-time updates  
- **Comprehensive Tips & Tricks** - 60+ curated tips for AI editor mastery (Cursor & Windsurf)
- **Real-time Analytics Dashboard** - Live data visualization with advanced data grids
- **Performance-Optimized Architecture** - Both frontend and backend optimizations
- **Azure-Ready Deployment** - Production deployment configuration

## Prerequisites

### Required Software
- **Node.js 20 or higher** - [Download from nodejs.org](https://nodejs.org/)
- **.NET 8 SDK** - [Download from Microsoft](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Redis** - For caching and session storage
- **Git** - For version control

### Redis Setup Options

#### Option 1: Redis via Windows (Recommended for Development)
1. **Download Redis for Windows**: [GitHub Releases](https://github.com/microsoftarchive/redis/releases)
2. **Install and Start Redis**:
   ```powershell
   # Extract Redis files to C:\Redis
   # Open PowerShell as Administrator
   cd C:\Redis
   .\redis-server.exe
   
   # In another terminal, test Redis:
   .\redis-cli.exe
   ping  # Should return "PONG"
   ```

#### Option 2: Redis via WSL (Alternative)
```bash
# In WSL terminal
sudo apt update
sudo apt install redis-server
sudo service redis-server start

# Test Redis
redis-cli ping  # Should return "PONG"
```

#### Option 3: Azure Cache for Redis (Production)
- Use Azure Cache for Redis for production deployments
- Update connection string in `appsettings.Production.json`

### Azure OpenAI Setup (Optional for AI Features)
1. **Create Azure OpenAI Resource** in Azure Portal
2. **Deploy GPT-4 Model** in Azure OpenAI Studio
3. **Get API Key and Endpoint** from Azure Portal
4. **Update Configuration** (see Configuration section below)

## Configuration

### Backend Configuration

#### 1. appsettings.json
Create or update `src/AiFirstDemo.Api/appsettings.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "OpenAI": {
    "ApiKey": "your-azure-openai-api-key",
    "Endpoint": "https://your-resource.openai.azure.com/",
    "DeploymentName": "gpt-4"
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:5173",
      "https://localhost:5173",
      "http://localhost:3000",
      "https://localhost:3000"
    ]
  }
}
```

#### 2. User Secrets (Recommended for Development)
```powershell
# Navigate to API project
cd src/AiFirstDemo.Api

# Initialize user secrets
dotnet user-secrets init

# Set Azure OpenAI configuration
dotnet user-secrets set "OpenAI:ApiKey" "your-actual-api-key"
dotnet user-secrets set "OpenAI:Endpoint" "https://your-resource.openai.azure.com/"
dotnet user-secrets set "OpenAI:DeploymentName" "gpt-4"

# Set Redis connection (if different from localhost)
dotnet user-secrets set "Redis:ConnectionString" "your-redis-connection"
```

### Frontend Configuration

#### 1. API Base URL
The frontend automatically detects the API URL:
- **Development**: `http://localhost:5002` or `https://localhost:5003`
- **Production**: Same origin as frontend

#### 2. Environment Variables (Optional)
Create `src/AiFirstDemo.Web/.env.local`:
```env
VITE_API_BASE_URL=http://localhost:5002
```

## Installation & Setup

### 1. Clone the Repository
```bash
git clone https://github.com/your-repo/ai-first-demo.git
cd ai-first-demo
```

### 2. Backend Setup
```powershell
# Navigate to API project
cd src/AiFirstDemo.Api

# Restore NuGet packages
dotnet restore

# Build the project
dotnet build

# Run database migrations (if using EF Core in future)
# dotnet ef database update
```

### 3. Frontend Setup  
```powershell
# Navigate to React project
cd src/AiFirstDemo.Web

# Install npm packages
npm install

# Build the project (optional for development)
npm run build
```

### 4. Start Redis
Ensure Redis is running on `localhost:6379` (default port)

## Running the Application

### Development Mode

#### Option 1: Using PowerShell Script (Recommended)
```powershell
# From project root
.\start-dev.ps1
```

This script will:
- Start the .NET API server on ports 5002 (HTTP) and 5003 (HTTPS)
- Start the React development server on port 5173
- Open the application in your default browser

#### Option 2: Manual Start
```powershell
# Terminal 1: Start Backend
cd src/AiFirstDemo.Api
dotnet run

# Terminal 2: Start Frontend  
cd src/AiFirstDemo.Web
npm run dev
```

### Access URLs
- **Frontend**: http://localhost:5173
- **Backend API**: http://localhost:5002
- **API Documentation**: http://localhost:5002/swagger (if Swagger is enabled)

## Feature Testing

### 1. Tips & Tricks System ✅
- Navigate to "Tips & Tricks" section
- Test category filtering (cursor-basics, ai-first-dev, dotnet-react, etc.)
- Try liking/unliking tips
- Search for specific content

**Categories Available**:
- **cursor-basics**: Essential Cursor shortcuts and features
- **cursor-advanced**: Advanced Cursor techniques  
- **ai-first-dev**: AI-first development practices (including Windsurf)
- **dotnet-react**: .NET + React specific tips
- **best-practices**: General development best practices
- **productivity**: Productivity enhancement tips
- **troubleshooting**: Debugging and problem-solving

### 2. AI-Powered Quiz ✅
- Complete user session creation
- Take the interactive quiz
- Test AI hint functionality (Easy/Medium questions only)
- Verify Redis caching (hints should load instantly on second request)

### 3. Spaceship Game ✅
- Play the HTML5 Canvas game
- Test keyboard controls (Arrow keys, Spacebar)
- Verify real-time score updates
- Check leaderboard functionality

### 4. Analytics Dashboard ✅
- View real-time analytics
- Test data grid sorting, filtering, and pagination
- Verify SignalR live updates
- Check performance with large datasets

## Performance Testing

### Backend Performance ✅
- **Response Compression**: Verify Brotli/Gzip compression in browser dev tools
- **Caching**: Check Redis for cached quiz hints and analytics data
- **Static Files**: Verify proper cache headers for static assets

### Frontend Performance ✅
- **React Optimization**: Check React DevTools for unnecessary re-renders
- **Query Caching**: Verify React Query caching behavior
- **Loading States**: Test skeleton loading vs spinner performance

## Troubleshooting

### Common Issues

#### 1. Redis Connection Failed
```
Error: "Redis connection failed"
```
**Solution**: 
- Ensure Redis is running on port 6379
- Check Windows Firewall settings
- Verify Redis is accepting connections: `redis-cli ping`

#### 2. OpenAI API Errors
```
Error: "OpenAI API call failed"
```
**Solution**:
- Verify API key and endpoint in configuration
- Check Azure OpenAI resource deployment status
- Ensure model deployment name matches configuration

#### 3. CORS Errors
```
Error: "CORS policy blocked the request"
```
**Solution**:
- Verify frontend URL is in `appsettings.json` CORS configuration
- Check that frontend is running on expected port (5173)
- Restart backend after CORS configuration changes

#### 4. Frontend Build Errors
```
Error: TypeScript compilation errors
```
**Solution**:
- Run `npm install` to ensure all dependencies are installed
- Check Node.js version (requires 20+)
- Clear node_modules and reinstall: `rm -rf node_modules && npm install`

#### 5. Port Conflicts
```
Error: "Port already in use"
```
**Solution**:
- Check if other applications are using ports 5002, 5003, or 5173
- Kill conflicting processes or use different ports
- Update CORS configuration if changing backend ports

### Debug Endpoints

#### Tips System Debug
```
POST /api/tips/debug/force-reseed  # Force reload all tips
GET /api/tips/debug/check/cursor-1 # Check specific tip exists
```

#### Health Checks
```
GET /api/health      # Application health status
GET /api/tips        # Verify tips are loaded
GET /api/analytics   # Verify analytics data
```

### Logging

#### Backend Logging
- Check console output for structured logs
- Logs include Redis operations, OpenAI calls, and performance metrics

#### Frontend Logging  
- Open browser dev tools for React Query and SignalR logs
- Network tab shows API calls and caching behavior

## Production Deployment

### Azure App Service Deployment ✅

#### 1. Build for Production
```powershell
# Run the build script
.\build-azure.ps1
```

#### 2. Deploy to Azure
- Upload the `publish` folder to Azure App Service
- Configure environment variables in Azure Portal
- Set up Azure Cache for Redis
- Configure custom domain and SSL

#### 3. Production Configuration
Update `appsettings.Production.json` with:
- Azure Cache for Redis connection string
- Azure OpenAI production endpoint
- Production CORS origins
- Application Insights connection string

## AI Development Tools Setup

### Cursor IDE Setup
1. **Install Cursor**: [Download from cursor.sh](https://cursor.sh/)
2. **Open Project**: File → Open Folder → Select ai-first-demo
3. **Configure .cursorrules**: The project includes comprehensive AI development rules
4. **Test AI Features**: Use @ file references, Cmd+K inline editing, Cmd+L chat

### Windsurf IDE Setup  
1. **Install Windsurf**: Available from Codeium
2. **Open Project**: Import the ai-first-demo repository
3. **Try Cascade Mode**: Ask Windsurf to "analyze this project and suggest improvements"
4. **Use Flow Mode**: Collaborative editing for complex refactoring tasks

### Development Workflow
1. **Use Windsurf for**: Architecture planning, complex feature development
2. **Use Cursor for**: Rapid coding, debugging, code review
3. **Leverage Tips System**: Reference the built-in tips for optimal AI editor usage

## Next Steps

### Learning Path
1. **Explore the Tips System**: Master AI editor workflows with 60+ curated tips
2. **Study the Architecture**: Understand Service Layer patterns and performance optimizations  
3. **Extend Features**: Add new quiz questions, game features, or analytics views
4. **Deploy to Azure**: Set up production environment with full Azure integration

### Extension Ideas
- **Authentication**: Add Azure AD B2C or Auth0 integration
- **Database**: Replace Redis with SQL Server + Entity Framework
- **Testing**: Add comprehensive unit and integration tests
- **Mobile**: Enhance responsive design for mobile devices
- **Monitoring**: Integrate Application Insights for production monitoring

This setup provides a complete foundation for exploring AI-assisted development with modern tools and practices! 