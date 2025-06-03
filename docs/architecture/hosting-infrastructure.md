# Hosting & Infrastructure Architecture

This document outlines the hosting and infrastructure architecture for the AI-First Demo application deployed on Microsoft Azure.

## Azure Infrastructure Overview

```mermaid
graph TB
    subgraph "Internet"
        Users[Users<br/>Global Internet Access]
        CDN[Azure CDN<br/>(Optional)<br/>Static Asset Delivery]
    end
    
    subgraph "Azure Canada Central Region"
        subgraph "App Service Plan"
            AppService[Azure App Service<br/>Linux B1 Basic<br/>Single Instance]
        end
        
        subgraph "Managed Services"
            Redis[Azure Cache for Redis<br/>C0 Basic 250MB<br/>Non-persistent]
            OpenAI[Azure OpenAI Service<br/>GPT-3.5-turbo<br/>Pay-per-use]
        end
        
        subgraph "Monitoring & Logging"
            AppInsights[Application Insights<br/>Performance Monitoring]
            LogAnalytics[Log Analytics Workspace<br/>Centralized Logging]
        end
    end
    
    subgraph "Development & Deployment"
        GitHub[GitHub Repository<br/>Source Control]
        Actions[GitHub Actions<br/>CI/CD Pipeline]
        LocalDev[Local Development<br/>Docker Redis + .NET]
    end
    
    Users --> CDN
    CDN --> AppService
    Users --> AppService
    
    AppService --> Redis
    AppService --> OpenAI
    AppService --> AppInsights
    AppInsights --> LogAnalytics
    
    GitHub --> Actions
    Actions --> AppService
    LocalDev --> GitHub
```

## Application Deployment Architecture

```mermaid
graph TD
    subgraph "Single Azure App Service"
        subgraph "Process Container"
            ASPCore[ASP.NET Core 8<br/>Kestrel Web Server<br/>Port 80/443]
            
            subgraph "Static Files"
                ReactBuild[React Production Build<br/>wwwroot/assets/]
                StaticMiddleware[Static File Middleware<br/>Compression & Caching]
            end
            
            subgraph "API Services"
                Controllers[Web API Controllers<br/>RESTful Endpoints]
                SignalRHub[SignalR Hub<br/>Real-time Updates]
                BackgroundServices[Background Services<br/>Analytics Updates]
            end
        end
        
        subgraph "Configuration"
            AppSettings[appsettings.json<br/>Base Config]
            EnvVars[Environment Variables<br/>Secrets & Connection Strings]
        end
    end
    
    ASPCore --> StaticMiddleware
    ASPCore --> Controllers  
    ASPCore --> SignalRHub
    ASPCore --> BackgroundServices
    
    Controllers --> AppSettings
    Controllers --> EnvVars
    SignalRHub --> EnvVars
```

## CI/CD Pipeline Architecture

```mermaid
graph LR
    subgraph "Source Control"
        Dev[Developer<br/>Local Changes]
        PR[Pull Request<br/>Code Review]
        Main[Main Branch<br/>Production Ready]
    end
    
    subgraph "GitHub Actions Workflow"
        Trigger[Push to Main<br/>Workflow Trigger]
        
        subgraph "Build Stage"
            DotNetBuild[.NET Build<br/>dotnet restore<br/>dotnet build]
            NodeBuild[Node.js Build<br/>npm install<br/>npm run build]
            TestRun[Run Tests<br/>dotnet test]
        end
        
        subgraph "Publish Stage"
            DotNetPublish[.NET Publish<br/>dotnet publish<br/>Include React Build]
            CreateArtifact[Create Deployment<br/>Artifact Package]
        end
        
        subgraph "Deploy Stage"
            AzureDeploy[Azure Web Deploy<br/>publish-profile<br/>Zero Downtime]
        end
    end
    
    subgraph "Azure App Service"
        LiveApp[Production Application<br/>aifirstsession-cshcfrh3h5g6f5ea<br/>.canadacentral-01.azurewebsites.net]
    end
    
    Dev --> PR
    PR --> Main
    Main --> Trigger
    
    Trigger --> DotNetBuild
    Trigger --> NodeBuild
    DotNetBuild --> TestRun
    NodeBuild --> DotNetPublish
    TestRun --> DotNetPublish
    
    DotNetPublish --> CreateArtifact
    CreateArtifact --> AzureDeploy
    AzureDeploy --> LiveApp
```

## Network & Security Architecture

```mermaid
graph TB
    subgraph "Internet Boundary"
        Internet[Public Internet<br/>Global Users]
        Firewall[Azure Firewall<br/>DDoS Protection]
    end
    
    subgraph "Azure App Service"
        subgraph "Application Security"
            HTTPS[HTTPS Termination<br/>TLS 1.2+ Only]
            CORS[CORS Policy<br/>Dynamic Origin Validation]
            RateLimit[Rate Limiting<br/>IP-based Quiz Limits]
        end
        
        subgraph "Authentication"
            SessionAuth[Session-based Auth<br/>Redis Session Store]
            IPTracking[IP Address Tracking<br/>SHA-256 Hashed]
        end
    end
    
    subgraph "Data Security"
        subgraph "Secrets Management"
            EnvVars[Environment Variables<br/>Azure App Service Config]
            KeyVault[Azure Key Vault<br/>(Future Enhancement)]
        end
        
        subgraph "Data Protection"
            RedisSSL[Redis SSL/TLS<br/>Encrypted in Transit]
            OpenAIHTTPS[OpenAI HTTPS<br/>API Key Authentication]
        end
    end
    
    Internet --> Firewall
    Firewall --> HTTPS
    HTTPS --> CORS
    CORS --> RateLimit
    RateLimit --> SessionAuth
    
    SessionAuth --> EnvVars
    SessionAuth --> RedisSSL
    SessionAuth --> OpenAIHTTPS
```

## Performance & Scalability Architecture

```mermaid
graph TD
    subgraph "Performance Optimization"
        subgraph "Caching Strategy"
            ResponseCache[HTTP Response Caching<br/>30s - 5min TTL]
            MemoryCache[In-Memory Caching<br/>Frequently Accessed Data]
            RedisCache[Redis Distributed Cache<br/>Session & Application Data]
        end
        
        subgraph "Compression"
            Brotli[Brotli Compression<br/>Modern Browsers]
            Gzip[Gzip Fallback<br/>Legacy Support]
        end
        
        subgraph "Static Asset Optimization"
            StaticCaching[Static File Caching<br/>1 Year Cache Headers]
            Minification[CSS/JS Minification<br/>React Production Build]
        end
    end
    
    subgraph "Scalability Options"
        subgraph "Current (Demo)"
            SingleInstance[Single B1 Instance<br/>1 Core, 1.75GB RAM]
        end
        
        subgraph "Scale-Out Options"
            ScaleOut[Horizontal Scaling<br/>Multiple Instances]
            LoadBalancer[Azure Load Balancer<br/>Auto-scaling Rules]
            Premium[Premium App Service<br/>Auto-scale Triggers]
        end
    end
    
    ResponseCache --> Brotli
    Brotli --> Gzip
    MemoryCache --> RedisCache
    StaticCaching --> Minification
    
    SingleInstance -.->|Future| ScaleOut
    ScaleOut --> LoadBalancer
    LoadBalancer --> Premium
```

## Monitoring & Observability

```mermaid
graph TB
    subgraph "Application Telemetry"
        AppCode[Application Code<br/>Structured Logging]
        Metrics[Custom Metrics<br/>Business KPIs]
        Traces[Distributed Tracing<br/>Request Correlation]
    end
    
    subgraph "Azure Monitoring Stack"
        AppInsights[Application Insights<br/>APM & Error Tracking]
        LogAnalytics[Log Analytics<br/>Query & Analysis]
        AzureMonitor[Azure Monitor<br/>Alerting & Dashboards]
    end
    
    subgraph "Observability Features"
        Performance[Performance Monitoring<br/>Response Times & Throughput]
        Errors[Error Tracking<br/>Exception Analysis]
        Usage[Usage Analytics<br/>Feature Adoption]
        Availability[Availability Monitoring<br/>Uptime Checks]
    end
    
    AppCode --> AppInsights
    Metrics --> AppInsights
    Traces --> AppInsights
    
    AppInsights --> LogAnalytics
    LogAnalytics --> AzureMonitor
    
    AppInsights --> Performance
    AppInsights --> Errors
    AppInsights --> Usage
    AppInsights --> Availability
```

## Cost Optimization Strategy

```mermaid
graph LR
    subgraph "Current Cost Structure"
        AppServiceCost[App Service B1<br/>~$13/month<br/>1 Core, 1.75GB RAM]
        RedisCost[Redis C0 Basic<br/>~$16/month<br/>250MB Cache]
        OpenAICost[Azure OpenAI<br/>Pay-per-use<br/>~$0.01 per hint]
    end
    
    subgraph "Cost Optimizations"
        CacheStrategy[Aggressive Caching<br/>24h Hint Cache<br/>Reduce API Calls]
        
        ResourceSizing[Right-sizing<br/>B1 for Demo<br/>Scale as Needed]
        
        MonthlyBudget[Monthly Budget<br/>~$50 total<br/>Alert at 80%]
    end
    
    subgraph "Scaling Economics"
        DemoPhase[Demo Phase<br/>B1 + C0 Basic<br/>Cost: ~$30/month]
        
        ProductionPhase[Production Phase<br/>S1 + C1 Standard<br/>Cost: ~$100/month]
        
        ScalePhase[Scale Phase<br/>P1v2 + C2 Premium<br/>Cost: ~$300/month]
    end
    
    AppServiceCost --> CacheStrategy
    RedisCost --> ResourceSizing
    OpenAICost --> MonthlyBudget
    
    DemoPhase --> ProductionPhase
    ProductionPhase --> ScalePhase
```

## Disaster Recovery & Business Continuity

```mermaid
graph TD
    subgraph "Current Setup (Demo)"
        SingleRegion[Single Region<br/>Canada Central<br/>Basic Redundancy]
        BackupStrategy[Code in GitHub<br/>No Data Backup<br/>Redis Non-persistent]
    end
    
    subgraph "Production Recommendations"
        MultiRegion[Multi-Region Setup<br/>Primary: Canada Central<br/>Secondary: East US 2]
        
        DataBackup[Redis Persistence<br/>Daily Snapshots<br/>Point-in-time Recovery]
        
        TrafficManager[Azure Traffic Manager<br/>DNS-based Failover<br/>Health Check Probes]
    end
    
    subgraph "Recovery Procedures"
        AutoFailover[Automatic Failover<br/>Traffic Manager<br/>RTO: 5 minutes]
        
        ManualFailover[Manual Failover<br/>GitHub Actions<br/>RTO: 15 minutes]
        
        DataRecovery[Data Recovery<br/>Redis Backup Restore<br/>RPO: 24 hours]
    end
    
    SingleRegion -.->|Upgrade Path| MultiRegion
    BackupStrategy -.->|Upgrade Path| DataBackup
    
    MultiRegion --> TrafficManager
    DataBackup --> AutoFailover
    TrafficManager --> ManualFailover
    AutoFailover --> DataRecovery
```

## Key Infrastructure Decisions

### 1. Single App Service Deployment
- React SPA and .NET API in one container
- Simplified deployment and management
- Cost-effective for demo scenarios
- Easy to scale out when needed

### 2. Azure-Native Services
- Azure Cache for Redis for simplicity
- Azure OpenAI for AI capabilities
- Application Insights for monitoring
- Leverages Azure's managed service benefits

### 3. GitHub Actions CI/CD
- Simple workflow with build, test, and deploy
- Secrets managed in GitHub repository
- Zero-downtime deployments
- Easy to trigger and monitor

### 4. Cost-Optimized Configuration
- B1 App Service plan for demos
- C0 Redis for development workloads
- Pay-per-use OpenAI for variable costs
- ~$30/month total running cost

### 5. Security-First Approach
- HTTPS-only communication
- Environment variables for secrets
- CORS policies for API protection
- Rate limiting for abuse prevention

This infrastructure provides a solid foundation for the demo while maintaining clear upgrade paths for production scenarios. 