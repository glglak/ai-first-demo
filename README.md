# AI-First Demo

A comprehensive demonstration of **AI-assisted full-stack development** using modern tools like **Cursor** and **Windsurf**. This project showcases how AI can accelerate development while maintaining enterprise-grade architecture, performance, and code quality.

## 🌐 **Live Demo**
**🚀 [Try the Live App](https://aifirstsession-cshcfrh3h5g6f5ea.canadacentral-01.azurewebsites.net)**

### ✅ Complete Feature Set
- **🧠 AI-Powered Quiz System** - Interactive quiz with OpenAI-generated hints and intelligent caching
- **🎮 HTML5 Spaceship Game** - Classic Asteroids-style game with real-time score updates
- **💡 Comprehensive Tips & Tricks** - 60+ curated tips for mastering AI editors (Cursor & Windsurf)
- **📊 Real-time Analytics Dashboard** - Live data visualization with advanced sortable data grids
- **⚡ Performance-Optimized Architecture** - Both frontend and backend performance enhancements
- **☁️ Azure-Ready Deployment** - Production deployment configuration for Azure App Service

## 🛠 **Technology Stack**

### Backend (.NET 8)
- **ASP.NET Core 8** - High-performance Web API
- **Redis** - Primary data storage and caching  
- **SignalR** - Real-time communication for live updates
- **Azure OpenAI** - AI integration for hint generation
- **Service Layer Pattern** - Clean business logic separation

### Frontend (React 18)
- **React 18 + TypeScript** - Type-safe, modern component architecture
- **TanStack Table** - Advanced data grids with sorting, filtering, pagination
- **React Query** - API state management with intelligent caching
- **Tailwind CSS** - Utility-first styling with custom themes
- **SignalR Client** - Real-time updates and live data synchronization

## 🏗 **Simple Architecture**

### What We Actually Use ✅
- **Service Layer Pattern** - Direct service injection (not CQRS)
- **Vertical Slice Organization** - Features as self-contained modules
- **Redis as Primary Database** - Simple key-value storage with TTL caching
- **Minimal SignalR Usage** - Only for game updates and analytics
- **Azure OpenAI Integration** - Quiz hints with cost-optimized caching
- **React with Standard Patterns** - useState, useEffect, React Query

### Project Structure
```
src/
├── AiFirstDemo.Api/           # .NET Web API + React hosting
├── AiFirstDemo.Features/      # Business logic by feature
│   ├── Quiz/                  # Quiz system with AI hints
│   ├── SpaceshipGame/         # HTML5 canvas game
│   ├── TipsAndTricks/         # Tips knowledge base
│   ├── Analytics/             # Real-time dashboard
│   └── Shared/                # Common models, SignalR hubs
├── AiFirstDemo.Infrastructure/# External services (Redis, OpenAI)
└── AiFirstDemo.Web/          # React frontend (builds to API/wwwroot)
```

## 🚀 **Quick Start**

### Prerequisites
- **Node.js 20+** - [Download](https://nodejs.org/)
- **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Redis** - Local installation or Azure Cache for Redis

### One-Command Setup
```powershell
# Clone and start the application
git clone https://github.com/glglak/ai-first-demo.git
cd ai-first-demo
.\start-dev.ps1
```

This will:
- Start .NET API server (ports 5002/5003)
- Start React development server (port 5173)  
- Open the application in your browser

### Manual Setup
```powershell
# Backend
cd src/AiFirstDemo.Api
dotnet restore && dotnet run

# Frontend (new terminal)
cd src/AiFirstDemo.Web
npm install && npm run dev
```

### Access Points
- **Frontend**: http://localhost:5173
- **Backend API**: http://localhost:5002
- **Live Demo**: https://aifirstsession-cshcfrh3h5g6f5ea.canadacentral-01.azurewebsites.net

## 🎯 **AI Development Showcase**

### Cursor & Windsurf Tips (60+ Built-in)
- **Cursor Essentials**: @ file references, Cmd+K editing, Cmd+L chat
- **Windsurf Features**: Cascade autonomous mode, Flow collaborative editing
- **.NET + React**: API design patterns, React integration, TypeScript generation
- **Best Practices**: Context building, specific prompts, code review workflows

### Production AI Integration
```csharp
// Smart caching for cost optimization
public async Task<string> GetQuestionHintAsync(int questionId)
{
    var cacheKey = $"hint:{questionId}";
    var cached = await _redis.GetAsync<string>(cacheKey);
    if (cached != null) return cached;

    var hint = await _openAI.GenerateHintAsync(question);
    await _redis.SetAsync(cacheKey, hint, TimeSpan.FromHours(24));
    return hint;
}
```

## 🎮 **Features Overview**

### 1. AI-Powered Quiz System
- Interactive multiple choice questions with varying difficulty
- OpenAI-generated hints for Easy/Medium questions (Hard excluded for scoring integrity)
- 24-hour Redis caching to optimize AI costs
- Real-time scoring and session tracking

### 2. HTML5 Spaceship Game  
- Classic Asteroids-style gameplay with smooth 60fps animation
- Real-time score updates via SignalR
- Advanced collision detection and physics
- Responsive controls for keyboard and touch

### 3. Tips & Tricks Knowledge Base
- 60+ professional tips across 7 categories
- AI editor focus with specific Cursor and Windsurf workflows
- Interactive features: likes, category filtering, search
- Community contribution system

### 4. Real-time Analytics Dashboard
- Live data visualization with SignalR updates
- Advanced data grids using TanStack Table
- Performance optimized with React.memo and intelligent caching
- Multiple data views: quiz, game, and user analytics

## 📖 **Documentation**

- **[Setup Guide](SETUP.md)** - Installation and configuration
- **[Azure Deployment](AZURE-DEPLOYMENT.md)** - Production deployment guide  
- **[Architecture Docs](docs/architecture/)** - System design and patterns
- **[.cursorrules](.cursorrules)** - AI development guidelines

## 🔧 **Performance Features**

### Backend Optimizations ✅
- **Response Compression** - Brotli/Gzip with optimal levels
- **Intelligent Caching** - 24h hints, 30s analytics, 1h sessions
- **Static File Serving** - Proper cache headers and compression
- **Async Patterns** - All I/O operations properly awaited

### Frontend Optimizations ✅
- **React Performance** - Strategic memo, useMemo, useCallback
- **Query Management** - React Query with intelligent staleTime
- **Loading Experience** - Skeleton loading for better UX
- **Bundle Optimization** - Code splitting and optimized builds

## 🚀 **Deployment**

### Azure App Service (Live)
- **Single App Service** - Hosts both .NET API and React frontend
- **Azure Cache for Redis** - Primary data storage
- **Azure OpenAI** - AI hint generation
- **GitHub Actions CI/CD** - Automated deployment pipeline

### Build & Deploy
```powershell
# Build for Azure deployment
.\build-azure.ps1

# Deploy the publish/ directory to Azure App Service
# Or use GitHub Actions for automated deployment
```

## 🤝 **Contributing**

This project demonstrates AI-first development patterns. Key principles:
- **Feature-based organization** over layered architecture
- **Performance by default** in every feature
- **AI-friendly code structure** for easy understanding and modification
- **Practical over complex** - choose simple solutions that work

## 📝 **License**

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

**Built with AI assistance using Cursor and Windsurf IDEs** 🤖✨