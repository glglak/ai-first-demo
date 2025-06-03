# Solution Architecture

This document outlines the backend solution architecture for the AI-First Demo application.

## Overall Solution Architecture

```mermaid
graph TB
    subgraph "Client Layer"
        Browser[Web Browser<br/>React SPA]
        Mobile[Mobile Browser<br/>Responsive UI]
    end
    
    subgraph "API Gateway Layer"
        API[ASP.NET Core API<br/>Web API Controllers]
        SignalR[SignalR Hub<br/>Real-time Communication]
    end
    
    subgraph "Service Layer (Vertical Slices)"
        QuizService[Quiz Service<br/>Questions & Hints]
        GameService[Game Service<br/>Spaceship Scores]
        TipsService[Tips Service<br/>User Content & Likes]
        AnalyticsService[Analytics Service<br/>Data Aggregation]
        UserSessionService[User Session Service<br/>Session Management]
    end
    
    subgraph "Infrastructure Layer"
        Redis[(Redis<br/>Primary Database<br/>& Cache)]
        OpenAI[Azure OpenAI<br/>Hint Generation]
    end
    
    subgraph "Cross-Cutting Concerns"
        Logging[Structured Logging<br/>Serilog]
        Caching[Response Caching<br/>Memory + Redis]
        Compression[Response Compression<br/>Brotli/Gzip]
    end
    
    Browser --> API
    Mobile --> API
    Browser --> SignalR
    Mobile --> SignalR
    
    API --> QuizService
    API --> GameService
    API --> TipsService
    API --> AnalyticsService
    API --> UserSessionService
    
    SignalR --> AnalyticsService
    
    QuizService --> Redis
    QuizService --> OpenAI
    GameService --> Redis
    TipsService --> Redis
    AnalyticsService --> Redis
    UserSessionService --> Redis
    
    API --> Logging
    API --> Caching
    API --> Compression
```

## Service Layer Architecture (Vertical Slices)

```mermaid
graph TD
    subgraph "Quiz Slice"
        QC[Quiz Controller<br/>API Endpoints]
        QS[Quiz Service<br/>Business Logic]
        QM[Quiz Models<br/>DTOs & Entities]
        
        QC --> QS
        QS --> QM
    end
    
    subgraph "Game Slice"
        GC[Game Controller]
        GS[Game Service]
        GM[Game Models]
        
        GC --> GS
        GS --> GM
    end
    
    subgraph "Tips Slice"
        TC[Tips Controller]
        TS[Tips Service]
        TM[Tips Models]
        
        TC --> TS
        TS --> TM
    end
    
    subgraph "Analytics Slice"
        AC[Analytics Controller]
        AS[Analytics Service]
        AM[Analytics Models]
        
        AC --> AS
        AS --> AM
    end
    
    subgraph "Shared Infrastructure"
        RS[Redis Service<br/>IRedisService]
        OS[OpenAI Service<br/>IOpenAIService]
        UHS[SignalR Hub<br/>Analytics Updates]
    end
    
    QS --> RS
    QS --> OS
    GS --> RS
    TS --> RS
    AS --> RS
    AS --> UHS
```

## Data Flow Architecture

```mermaid
flowchart TD
    subgraph "Request Flow"
        Client[Client Request]
        Controller[API Controller]
        Service[Domain Service]
        Redis[Redis Store]
        Response[JSON Response]
    end
    
    subgraph "Real-time Flow"
        Action[User Action]
        Hub[SignalR Hub]
        Broadcast[Real-time Broadcast]
        Update[UI Update]
    end
    
    subgraph "AI Integration Flow"
        HintReq[Hint Request]
        Cache{Cache Hit?}
        OpenAI[Azure OpenAI API]
        CacheStore[Store in Cache<br/>24h TTL]
        HintResp[Hint Response]
    end
    
    Client --> Controller
    Controller --> Service
    Service --> Redis
    Redis --> Service
    Service --> Controller
    Controller --> Response
    
    Action --> Hub
    Hub --> Service
    Service --> Broadcast
    Broadcast --> Update
    
    HintReq --> Cache
    Cache -->|Hit| HintResp
    Cache -->|Miss| OpenAI
    OpenAI --> CacheStore
    CacheStore --> HintResp
```

## Redis Data Patterns

```mermaid
graph LR
    subgraph "Redis Key Patterns"
        subgraph "User Sessions"
            US[session:{sessionId}<br/>User session data]
            UIP[ip:{ipHash}<br/>IP tracking]
        end
        
        subgraph "Quiz Data"
            QA[quiz:attempts:{ipHash}<br/>Quiz attempts count]
            QR[quiz:results:{sessionId}<br/>Quiz results]
            QH[quiz:hints:{questionId}<br/>Cached AI hints]
        end
        
        subgraph "Game Data"
            GS[game:scores<br/>Sorted set of scores]
            GP[game:player:{sessionId}<br/>Player game data]
        end
        
        subgraph "Tips Data"
            T[tips:{tipId}<br/>Tip content]
            TC[tips:category:{category}<br/>Tips by category]
            TL[tips:likes:{tipId}<br/>Tip like count]
            UL[user:likes:{sessionId}<br/>User's liked tips]
        end
        
        subgraph "Analytics Cache"
            AQ[analytics:quiz<br/>Cached quiz stats]
            AG[analytics:game<br/>Cached game stats]
            AT[analytics:tips<br/>Cached tips stats]
        end
    end
```

## Performance & Caching Strategy

```mermaid
graph TD
    subgraph "Caching Layers"
        subgraph "Level 1: Memory Cache"
            MC[In-Memory Cache<br/>Fast access<br/>5-30 second TTL]
        end
        
        subgraph "Level 2: Redis Cache"
            RC[Redis Cache<br/>Distributed<br/>5min - 24h TTL]
        end
        
        subgraph "Level 3: Computed Cache"
            CC[Response Caching<br/>HTTP Headers<br/>30s - 5min TTL]
        end
    end
    
    subgraph "Cache Usage"
        Analytics[Analytics Queries<br/>30s cache]
        Hints[AI Hints<br/>24h cache]
        Sessions[User Sessions<br/>1h cache]
        Tips[Tips Data<br/>5min cache]
    end
    
    Analytics --> MC
    Analytics --> RC
    Analytics --> CC
    
    Hints --> RC
    Sessions --> RC
    Tips --> RC
```

## Security & Configuration

```mermaid
graph TB
    subgraph "Configuration Management"
        AppSettings[appsettings.json<br/>Base Configuration]
        DevSettings[appsettings.Development.json<br/>Local Development]
        ProdSettings[appsettings.Production.json<br/>Production Secrets]
        EnvVars[Environment Variables<br/>Azure App Service]
    end
    
    subgraph "Security Measures"
        CORS[CORS Policy<br/>Dynamic Origins]
        RateLimit[Rate Limiting<br/>IP-based Quiz Limits]
        InputValid[Input Validation<br/>Data Annotations]
        SecretMgmt[Secret Management<br/>.gitignore Protection]
    end
    
    AppSettings --> DevSettings
    AppSettings --> ProdSettings
    ProdSettings --> EnvVars
    
    EnvVars --> CORS
    EnvVars --> RateLimit
    EnvVars --> InputValid
    EnvVars --> SecretMgmt
```

## Error Handling & Monitoring

```mermaid
graph TD
    subgraph "Error Handling Pipeline"
        Request[Incoming Request]
        Validation[Input Validation]
        BusinessLogic[Business Logic]
        ExceptionFilter[Global Exception Filter]
        ErrorResponse[Structured Error Response]
    end
    
    subgraph "Logging & Monitoring"
        StructuredLog[Structured Logging<br/>Serilog]
        LogLevels[Log Levels<br/>Info/Warn/Error]
        LogSinks[Log Sinks<br/>Console/File/Azure]
    end
    
    Request --> Validation
    Validation --> BusinessLogic
    BusinessLogic --> ExceptionFilter
    ExceptionFilter --> ErrorResponse
    
    Validation --> StructuredLog
    BusinessLogic --> StructuredLog
    ExceptionFilter --> StructuredLog
    
    StructuredLog --> LogLevels
    LogLevels --> LogSinks
```

## Key Architecture Decisions

### 1. Service Layer Pattern (Not CQRS)
- Direct service injection for simplicity
- Business logic encapsulated in domain services
- Clean separation between controllers and business logic

### 2. Redis as Primary Database
- Single data store for simplicity
- Built-in caching capabilities
- Excellent performance for demo scenarios

### 3. Vertical Slice Architecture
- Features organized as self-contained slices
- Each slice owns its controllers, services, and models
- Shared infrastructure services for cross-cutting concerns

### 4. Performance-First Design
- Multiple caching layers (memory, Redis, HTTP)
- Response compression (Brotli/Gzip)
- Optimized query patterns and data structures

### 5. AI Integration Strategy
- Cost-optimized with 24-hour hint caching
- Fallback mechanisms for AI service failures
- Smart hint exclusion for difficult questions

This architecture balances simplicity, performance, and maintainability while demonstrating modern .NET development practices. 