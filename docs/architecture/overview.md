# Architecture Overview

## Design Principles

### 1. Vertical Slice Architecture
Each feature is self-contained with all layers:
- Controller/Endpoint
- Business Logic
- Data Access
- Models/DTOs
- Validation
- Tests

### 2. Modular Monolith
- Clear module boundaries
- Minimal inter-module dependencies
- Potential for future microservice extraction

### 3. Clean Architecture Concepts
- Dependency Inversion
- Single Responsibility
- Interface Segregation

## Technology Stack

### Backend (.NET 8)
```csharp
// Core packages
- ASP.NET Core 8
- MediatR (CQRS pattern)
- FluentValidation
- StackExchange.Redis
- Azure.AI.OpenAI
- SignalR
- Serilog
```

### Frontend (React)
```javascript
// Core packages
- React 18
- TypeScript
- Vite
- React Router
- React Query
- Tailwind CSS
- SignalR Client
```

## Data Architecture

### Redis Key Patterns
```
user:session:{ip_hash}           # User session data
quiz:attempt:{session_id}        # Quiz attempts
game:score:{game_id}             # Game scores
game:leaderboard:daily           # Daily leaderboard
tips:category:{category}         # Tips by category
analytics:visits:{date}          # Daily analytics
cache:openai:{hash}              # AI response cache
```

### Data Models
```csharp
public record UserSession(
    string SessionId,
    string Name,
    string IpHash,
    DateTime CreatedAt,
    DateTime LastActivity,
    bool HasCompletedQuiz
);

public record QuizAttempt(
    string SessionId,
    List<QuizAnswer> Answers,
    int Score,
    DateTime CompletedAt,
    TimeSpan Duration
);

public record GameScore(
    string SessionId,
    string PlayerName,
    int Score,
    DateTime AchievedAt,
    TimeSpan GameDuration
);
```

## API Design

### RESTful Endpoints
```
GET    /api/sessions/{sessionId}           # Get session
POST   /api/sessions                       # Create session

GET    /api/quiz/questions                 # Get quiz questions
POST   /api/quiz/submit                    # Submit quiz

GET    /api/game/leaderboard              # Get leaderboard
POST   /api/game/score                    # Submit score

GET    /api/tips                          # Get tips
POST   /api/tips/generate                 # Generate AI tips

GET    /api/analytics/dashboard           # Analytics data
```

### SignalR Hubs
```
/hubs/game        # Real-time game updates
/hubs/analytics   # Live analytics
/hubs/quiz        # Quiz participation
```

## Security Considerations

1. **Rate Limiting**: Prevent API abuse
2. **Input Validation**: Comprehensive validation
3. **IP Tracking**: Hash IPs for privacy
4. **CORS**: Properly configured
5. **API Keys**: Secure Azure OpenAI integration

## Performance Optimizations

1. **Redis Connection Pooling**
2. **AI Response Caching**
3. **Async/Await Patterns**
4. **Efficient React Rendering**
5. **Code Splitting**

## Scalability Patterns

1. **Stateless API Design**
2. **Redis Clustering Support**
3. **Horizontal Scaling Ready**
4. **CDN for Static Assets**
5. **Background Job Processing**