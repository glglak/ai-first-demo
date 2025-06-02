# AI First Demo

A comprehensive demo showcasing AI-First development practices using Cursor/Windsurf with .NET 8, React, Redis, and Azure OpenAI.

## 🚀 Architecture

### Tech Stack
- **Backend**: ASP.NET Core 8 Web API
- **Frontend**: React SPA with TypeScript
- **Database**: Azure Redis Cache (Primary Storage)
- **AI**: Azure OpenAI Integration
- **Architecture**: Vertical Slices + Modular Monolith

### Features
1. **AI Quiz** - Interactive quiz about Cursor/AI development
2. **Spaceship Game** - Simple browser game with leaderboard
3. **Tips & Tricks** - AI-curated development tips
4. **Analytics Dashboard** - Real-time usage metrics

## 🏗️ Project Structure

```
AiFirstDemo/
├── src/
│   ├── AiFirstDemo.Api/                 # Web API Host
│   ├── AiFirstDemo.Web/                 # React SPA
│   ├── AiFirstDemo.Infrastructure/      # External services
│   └── AiFirstDemo.Features/            # Feature modules
│       ├── Quiz/
│       ├── SpaceshipGame/
│       ├── TipsAndTricks/
│       ├── UserSessions/
│       └── Analytics/
├── tests/
├── docker/
└── docs/
```

## 🛠️ Development Setup

### Prerequisites
- .NET 8 SDK
- Node.js 18+
- Azure Redis Cache
- Azure OpenAI Service

### Environment Variables
```bash
Redis__ConnectionString="your-redis-connection-string"
AzureOpenAI__Endpoint="https://your-endpoint.openai.azure.com/"
AzureOpenAI__ApiKey="your-api-key"
```

### Quick Start
```bash
# Clone repository
git clone https://github.com/glglak/ai-first-demo.git
cd ai-first-demo

# Setup backend
cd src/AiFirstDemo.Api
dotnet restore
dotnet run

# Setup frontend (new terminal)
cd src/AiFirstDemo.Web
npm install
npm start
```

## 📖 Documentation

- [Architecture Decision Records](./docs/architecture/)
- [Feature Documentation](./docs/features/)
- [API Documentation](./docs/api/)
- [Development Guide](./docs/development/)

## 🎯 Demo Flow

1. **Live Coding**: Demonstrate Cursor/Windsurf capabilities
2. **Interactive Quiz**: Audience participation
3. **Game Competition**: Live leaderboard
4. **AI Tips**: Real-time content generation

## 📊 Key Demo Points

- ✅ Vertical Slice Architecture
- ✅ Redis as Primary Database
- ✅ Azure OpenAI Integration
- ✅ Real-time Features (SignalR)
- ✅ Clean Code Structure
- ✅ Modern React Patterns
- ✅ Comprehensive Testing

## 🔧 Cursor/Windsurf Integration

This project is designed to showcase:
- **Context Management**: Proper file organization for AI assistance
- **Model Selection**: When to use different AI models
- **Prompt Engineering**: Effective AI collaboration patterns
- **Code Generation**: Leveraging AI for rapid development

---

**Live Demo URL**: TBD
**Presentation Date**: TBD