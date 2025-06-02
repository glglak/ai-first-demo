# Development Setup Guide

## Prerequisites

### Required Software
- **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Node.js 18+** - [Download](https://nodejs.org/)
- **Git** - [Download](https://git-scm.com/)
- **Visual Studio Code** or **Visual Studio 2022**
- **Cursor** or **Windsurf** (for AI-assisted development)

### Azure Services
- **Azure Redis Cache** - For primary data storage
- **Azure OpenAI Service** - For AI integrations

## Local Development Setup

### 1. Clone the Repository
```bash
git clone https://github.com/glglak/ai-first-demo.git
cd ai-first-demo
```

### 2. Backend Setup (.NET 8)

#### Install Dependencies
```bash
cd src/AiFirstDemo.Api
dotnet restore
```

#### Configure Environment
Create `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "Redis": "YOUR_REDIS_CONNECTION_STRING"
  },
  "AzureOpenAI": {
    "Endpoint": "YOUR_AZURE_OPENAI_ENDPOINT",
    "ApiKey": "YOUR_AZURE_OPENAI_API_KEY",
    "DeploymentName": "gpt-4"
  }
}
```

#### Run the API
```bash
dotnet run
# API will be available at https://localhost:5001
```

### 3. Frontend Setup (React)

#### Install Dependencies
```bash
cd src/AiFirstDemo.Web
npm install
```

#### Run Development Server
```bash
npm run dev
# Frontend will be available at http://localhost:3000
```

## Using Cursor/Windsurf for Development

### Project Structure for AI Assistance
The project is organized to maximize AI productivity:

1. **Clear Module Boundaries** - Each feature is self-contained
2. **Consistent Naming** - Follows .NET and React conventions
3. **Comprehensive Types** - TypeScript definitions for all models
4. **Documentation** - Inline comments and README files

### Effective AI Prompts for This Project

#### Adding New Features
```
@src/AiFirstDemo.Features/Quiz Create a new feature called "Polls" following the same vertical slice pattern as the Quiz feature. Include:
- Models for Poll, PollOption, PollVote
- Service interface and implementation
- Controller with CRUD operations
- Redis storage using the same patterns
```

#### Frontend Development
```
@src/AiFirstDemo.Web/src/components Create a reusable Modal component that:
- Uses React portals
- Supports custom content
- Has close button and backdrop click
- Follows our Tailwind CSS patterns
```

#### Bug Fixes
```
@src/AiFirstDemo.Features/SpaceshipGame/GameService.cs The score validation is too strict. Update ValidateScoreAsync to be more reasonable for casual players while still preventing obvious cheating.
```

### Context Management Best Practices

1. **Reference Specific Files**: Use `@filename` to give AI context
2. **Include Related Types**: Reference type definitions when working with data
3. **Follow Patterns**: Point to existing similar implementations
4. **Provide Examples**: Show expected input/output when relevant

### Model Selection Guidelines

- **GPT-4**: Complex logic, architecture decisions, debugging
- **GPT-3.5**: Simple CRUD operations, styling, documentation
- **Claude**: Code reviews, refactoring, testing strategies

## Testing

### Backend Tests
```bash
cd tests/AiFirstDemo.Tests
dotnet test
```

### Frontend Tests
```bash
cd src/AiFirstDemo.Web
npm run test
```

## Docker Development

### Build and Run
```bash
docker-compose up --build
```

### Services
- **API**: http://localhost:5000
- **Web**: http://localhost:3000
- **Redis**: localhost:6379

## Troubleshooting

### Common Issues

1. **Redis Connection Failed**
   - Verify connection string
   - Check firewall settings
   - Ensure Redis is running

2. **Azure OpenAI Rate Limits**
   - Check API quota
   - Implement retry logic
   - Cache responses

3. **CORS Issues**
   - Verify CORS policy in Program.cs
   - Check frontend proxy configuration

### Performance Tips

1. **Redis Optimization**
   - Use connection pooling
   - Implement key expiration
   - Monitor memory usage

2. **API Optimization**
   - Enable response caching
   - Use async/await properly
   - Implement pagination

3. **Frontend Optimization**
   - Code splitting
   - Image optimization
   - Bundle analysis

## IDE Configuration

### VS Code Extensions
- C# Dev Kit
- ES7+ React/Redux/React-Native snippets
- Tailwind CSS IntelliSense
- Thunder Client (API testing)
- GitLens

### Cursor/Windsurf Tips
- Use the project's `.cursorrules` file
- Enable TypeScript strict mode
- Configure auto-imports
- Set up code formatting on save

## Deployment

See [deployment guide](./deployment.md) for production setup instructions.
