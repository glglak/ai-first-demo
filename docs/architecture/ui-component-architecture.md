# UI Component Architecture

This document outlines the user interface component architecture for the AI-First Demo application.

## Frontend Architecture Overview

```mermaid
graph TB
    subgraph "React App Structure"
        App[App.tsx<br/>Main Router & Context]
        
        subgraph "Pages Layer"
            Home[Home.tsx<br/>Landing Page]
        end
        
        subgraph "Feature Components"
            Quiz[Quiz Components<br/>Question Flow & Results]
            Game[Spaceship Game<br/>WebGL Canvas Game]
            Tips[Tips & Tricks<br/>User-Generated Content]
            Analytics[Analytics Dashboard<br/>Real-time Data Tables]
            UserSessions[User Sessions<br/>Session Management]
        end
        
        subgraph "Shared Components"
            DataTable[DataTable.tsx<br/>Reusable Table Component]
            UI[UI Components<br/>Buttons, Forms, etc.]
        end
        
        subgraph "Shared Services"
            API[api.ts<br/>HTTP Client & Auth]
            Contexts[React Contexts<br/>Session & SignalR]
        end
    end
    
    App --> Home
    App --> Quiz
    App --> Game
    App --> Tips
    App --> Analytics
    App --> UserSessions
    
    Quiz --> DataTable
    Analytics --> DataTable
    Tips --> UI
    
    Quiz --> API
    Game --> API
    Tips --> API
    Analytics --> API
    UserSessions --> API
    
    App --> Contexts
    Analytics --> Contexts
```

## Component Interaction Flow

```mermaid
sequenceDiagram
    participant U as User
    participant S as Session Context
    participant Q as Quiz Component
    participant A as API Service
    participant B as Backend
    participant R as Redis
    participant AI as OpenAI

    U->>S: Create Session
    S->>A: POST /api/user-sessions
    A->>B: Create User Session
    B->>R: Store Session Data
    
    U->>Q: Start Quiz
    Q->>A: GET /api/quiz/questions
    A->>B: Get Quiz Questions
    B-->>Q: Return Questions
    
    U->>Q: Request Hint
    Q->>A: GET /api/quiz/hint/{id}
    A->>B: Get Question Hint
    B->>R: Check Hint Cache
    
    alt Hint Cached
        R-->>B: Return Cached Hint
    else No Cache
        B->>AI: Generate Hint
        AI-->>B: Return AI Hint
        B->>R: Cache Hint (24h TTL)
    end
    
    B-->>Q: Return Hint
    Q-->>U: Display Hint
    
    U->>Q: Submit Quiz
    Q->>A: POST /api/quiz/submit
    A->>B: Calculate Results
    B->>R: Store Results
    B-->>Q: Return Results & Stats
```

## State Management Pattern

```mermaid
graph LR
    subgraph "Component State"
        LS[Local State<br/>useState]
        ES[Effect State<br/>useEffect]
        MS[Memoized State<br/>useMemo]
    end
    
    subgraph "Global State"
        SC[Session Context<br/>User Session Data]
        SRC[SignalR Context<br/>Real-time Updates]
    end
    
    subgraph "Server State"
        RQ[React Query<br/>API Data & Caching]
        RM[Mutations<br/>API Updates]
    end
    
    LS --> MS
    ES --> LS
    SC --> LS
    SRC --> ES
    RQ --> LS
    RM --> RQ
```

## Key Design Principles

### 1. Feature-Based Organization
- Components organized by feature domain (quiz, game, tips, analytics)
- Each feature has its own components, services, and types
- Shared components for common UI patterns

### 2. React Query for Server State
- Intelligent caching with appropriate stale times
- Optimistic updates for better UX
- Error handling and retry logic

### 3. Context for Global State
- Session context for user authentication state
- SignalR context for real-time updates
- Minimal global state, prefer local component state

### 4. Performance Optimizations
- `React.memo` for expensive components
- `useMemo` for computed values
- `useCallback` for stable function references
- Strategic lazy loading where appropriate

## Component Responsibilities

| Component | Responsibility |
|-----------|----------------|
| **App.tsx** | Main router, context providers, global layout |
| **Quiz Components** | Question flow, hint system, results display |
| **Game Component** | WebGL spaceship game with scoring |
| **Tips Components** | User-generated tips with like/unlike functionality |
| **Analytics Components** | Real-time data tables with sorting/filtering |
| **DataTable** | Reusable table with TanStack Table integration |
| **Session Context** | User session management and persistence |
| **SignalR Context** | Real-time updates from backend |
| **API Service** | HTTP client with authentication and error handling |

## Real-Time Updates Architecture

```mermaid
graph TD
    subgraph "Frontend"
        AC[Analytics Component]
        SRC[SignalR Context]
        RQ[React Query]
    end
    
    subgraph "Backend"
        SH[SignalR Hub]
        AS[Analytics Service]
        RS[Redis Store]
    end
    
    AC --> SRC
    SRC --> SH
    SH --> AS
    AS --> RS
    
    SH -->|Push Updates| SRC
    SRC -->|Trigger Refetch| AC
    AC -->|Update UI| RQ
```

This architecture ensures efficient real-time updates while maintaining good separation of concerns and performance. 