using AiFirstDemo.Features.TipsAndTricks.Models;
using AiFirstDemo.Infrastructure.Redis;
using AiFirstDemo.Infrastructure.OpenAI;
using AiFirstDemo.Features.UserSessions;
using AiFirstDemo.Features.Shared.Models;
using Microsoft.Extensions.Logging;

namespace AiFirstDemo.Features.TipsAndTricks;

public class TipsService : ITipsService
{
    private readonly IRedisService _redis;
    private readonly IOpenAIService _openAI;
    private readonly IUserSessionService _userSession;
    private readonly ILogger<TipsService> _logger;
    
    private const string TIPS_PREFIX = "tips:";
    private const string CATEGORY_PREFIX = "tips:category:";
    private const string USER_LIKES_PREFIX = "tips:likes:user:";
    private const string TIP_LIKES_PREFIX = "tips:likes:tip:";
    private const string CATEGORIES_KEY = "tips:categories";
    private const string SEEDED_KEY = "tips:seeded";

    public TipsService(
        IRedisService redis,
        IOpenAIService openAI,
        IUserSessionService userSession,
        ILogger<TipsService> logger)
    {
        _redis = redis;
        _openAI = openAI;
        _userSession = userSession;
        _logger = logger;
        
        // Seed predefined tips on startup (fire and forget, but with better error handling)
        _ = Task.Run(async () =>
        {
            try
            {
                await SeedPredefinedTipsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to seed predefined tips during service initialization");
            }
        });
    }

    /// <summary>
    /// Ensures predefined tips are seeded. Can be called multiple times safely.
    /// </summary>
    public async Task EnsureTipsSeededAsync()
    {
        await SeedPredefinedTipsAsync();
    }

    private async Task SeedPredefinedTipsAsync()
    {
        try
        {
            // Check if already seeded
            var alreadySeeded = await _redis.ExistsAsync(SEEDED_KEY);
            if (alreadySeeded)
            {
                _logger.LogInformation("Predefined tips already seeded, skipping seeding process");
                return;
            }

            _logger.LogInformation("Seeding predefined tips into Redis...");

            var predefinedTips = GetPredefinedTips();
            
            foreach (var tip in predefinedTips)
            {
                // Store the tip
                var success = await _redis.SetAsync($"{TIPS_PREFIX}{tip.Id}", tip, TimeSpan.FromDays(365));
                _logger.LogDebug("Stored tip {TipId} ({TipTitle}) in Redis: {Success}", tip.Id, tip.Title, success);
                
                // Add to category list
                await _redis.ListPushAsync($"{CATEGORY_PREFIX}{tip.Category}", tip.Id);
                _logger.LogDebug("Added tip {TipId} to category {Category}", tip.Id, tip.Category);
            }

            // Mark as seeded
            await _redis.SetAsync(SEEDED_KEY, "true", TimeSpan.FromDays(365));
            
            _logger.LogInformation("Successfully seeded {Count} predefined tips", predefinedTips.Count);
            
            // Verify seeding by checking if cursor-1 exists
            var testTip = await _redis.GetAsync<Tip>($"{TIPS_PREFIX}cursor-1");
            if (testTip != null)
            {
                _logger.LogInformation("Verification: cursor-1 tip found with title: {Title}", testTip.Title);
            }
            else
            {
                _logger.LogWarning("Verification failed: cursor-1 tip not found after seeding");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding predefined tips");
        }
    }

    private List<Tip> GetPredefinedTips()
    {
        return new List<Tip>
        {
            // Cursor Basics
            new Tip(
                Id: "cursor-1",
                Title: "Use @ to Reference Files",
                Content: "Type @ followed by a filename to give AI context about specific files. Example: \"@App.tsx can you add a new component?\" This helps AI understand your codebase structure.",
                Category: "cursor-basics",
                Difficulty: "beginner",
                Tags: new List<string> { "context", "files", "reference" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),
            new Tip(
                Id: "cursor-2",
                Title: "Cmd+K for Inline Editing",
                Content: "Use Cmd+K (Ctrl+K on Windows) to edit code inline. Select the code you want to modify, press Cmd+K, and describe what you want to change. Perfect for quick refactoring.",
                Category: "cursor-basics",
                Difficulty: "beginner",
                Tags: new List<string> { "shortcuts", "editing", "refactoring" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),
            new Tip(
                Id: "cursor-3",
                Title: "Cmd+L for Chat Mode",
                Content: "Press Cmd+L (Ctrl+L on Windows) to open the AI chat sidebar. This is perfect for asking questions, getting explanations, or planning your next steps without editing code directly.",
                Category: "cursor-basics",
                Difficulty: "beginner",
                Tags: new List<string> { "chat", "shortcuts", "planning" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),
            new Tip(
                Id: "cursor-4",
                Title: "Tab to Accept AI Suggestions",
                Content: "When AI shows inline suggestions (ghost text), press Tab to accept them. Press Escape to dismiss. You can also use Cmd+→ to accept word by word.",
                Category: "cursor-basics",
                Difficulty: "beginner",
                Tags: new List<string> { "autocomplete", "suggestions", "shortcuts" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),
            new Tip(
                Id: "cursor-5",
                Title: "Use .cursorrules for Project Context",
                Content: "Create a .cursorrules file in your project root to give AI persistent context about your coding standards, architecture, and preferences. This improves all AI suggestions.",
                Category: "cursor-basics",
                Difficulty: "intermediate",
                Tags: new List<string> { "configuration", "context", "standards" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),
            new Tip(
                Id: "cursor-6",
                Title: "Select Before Asking",
                Content: "Select the specific code you want to modify before using Cmd+K or asking questions. This gives AI precise context about what you're working on.",
                Category: "cursor-basics",
                Difficulty: "beginner",
                Tags: new List<string> { "selection", "context", "precision" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),
            new Tip(
                Id: "cursor-7",
                Title: "Use Natural Language Comments",
                Content: "Write comments in plain English describing what you want, then ask AI to implement it. Example: // Create a loading spinner component with animation",
                Category: "cursor-basics",
                Difficulty: "beginner",
                Tags: new List<string> { "comments", "natural-language", "implementation" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),

            // Advanced Cursor
            new Tip(
                Id: "cursor-adv-1",
                Title: "Multi-File Editing with @",
                Content: "Reference multiple files in one request: \"@components/Header.tsx @styles/globals.css update the header to use the new color scheme\". AI will edit multiple files simultaneously.",
                Category: "cursor-advanced",
                Difficulty: "advanced",
                Tags: new List<string> { "multi-file", "context", "batch-editing" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),
            new Tip(
                Id: "cursor-adv-2",
                Title: "Use Composer for Complex Features",
                Content: "Open Composer (Cmd+I) for building entire features. It can create multiple files, update existing ones, and handle complex architectural changes across your codebase.",
                Category: "cursor-advanced",
                Difficulty: "advanced",
                Tags: new List<string> { "composer", "features", "architecture" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),
            new Tip(
                Id: "cursor-adv-3",
                Title: "Context-Aware Refactoring",
                Content: "When refactoring, reference related files with @ to maintain consistency. Example: \"@types/User.ts @api/users.ts refactor this component to use the new User interface\"",
                Category: "cursor-advanced",
                Difficulty: "advanced",
                Tags: new List<string> { "refactoring", "context", "consistency" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),
            new Tip(
                Id: "cursor-adv-4",
                Title: "Use Cmd+Shift+L for Codebase Chat",
                Content: "Press Cmd+Shift+L to chat about your entire codebase. Great for architecture questions, debugging across files, or understanding complex interactions.",
                Category: "cursor-advanced",
                Difficulty: "intermediate",
                Tags: new List<string> { "codebase", "architecture", "debugging" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),
            new Tip(
                Id: "cursor-adv-5",
                Title: "Terminal Integration",
                Content: "Use Cmd+` to open terminal, then ask AI to explain terminal commands or help with CLI tasks. AI can help with git commands, package management, and more.",
                Category: "cursor-advanced",
                Difficulty: "intermediate",
                Tags: new List<string> { "terminal", "cli", "git" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),

            // AI-First Development
            new Tip(
                Id: "ai-first-1",
                Title: "Start with Intent, Not Implementation",
                Content: "Begin by describing what you want to achieve, not how to implement it. Let AI suggest the approach. Example: \"I need to cache API responses\" instead of \"Create a Redis cache layer\".",
                Category: "ai-first-dev",
                Difficulty: "intermediate",
                Tags: new List<string> { "intent", "planning", "approach" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),
            new Tip(
                Id: "ai-first-2",
                Title: "Iterative Development",
                Content: "Build features incrementally with AI. Start simple, get working code, then ask for improvements. This leads to better results than trying to build everything at once.",
                Category: "ai-first-dev",
                Difficulty: "beginner",
                Tags: new List<string> { "iterative", "incremental", "development" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),
            new Tip(
                Id: "ai-first-3",
                Title: "Use AI for Testing Strategy",
                Content: "Ask AI to create comprehensive test cases before implementing features. Example: \"What test cases should I write for a user authentication system?\"",
                Category: "ai-first-dev",
                Difficulty: "intermediate",
                Tags: new List<string> { "testing", "strategy", "quality" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),
            new Tip(
                Id: "ai-first-4",
                Title: "Architecture Planning with AI",
                Content: "Use AI to explore different architectural patterns. Ask: \"What are the trade-offs between microservices and modular monolith for my use case?\"",
                Category: "ai-first-dev",
                Difficulty: "advanced",
                Tags: new List<string> { "architecture", "planning", "patterns" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),
            new Tip(
                Id: "ai-first-5",
                Title: "Code Review with AI",
                Content: "Use AI as a code reviewer. Ask: \"Review this code for security issues, performance problems, and best practices.\" AI can catch issues you might miss.",
                Category: "ai-first-dev",
                Difficulty: "intermediate",
                Tags: new List<string> { "code-review", "security", "performance" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),

            // .NET & React
            new Tip(
                Id: "dotnet-1",
                Title: "AI-Driven API Design",
                Content: "Ask AI to design your .NET Web API endpoints: \"Create REST API endpoints for a blog system with posts, comments, and users.\" Include DTOs, controllers, and services.",
                Category: "dotnet-react",
                Difficulty: "intermediate",
                Tags: new List<string> { "api", "rest", "design", "architecture" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),
            new Tip(
                Id: "dotnet-2",
                Title: "React Hook Generation",
                Content: "Let AI create custom React hooks for your .NET API: \"Create a useApi hook for fetching data from my .NET backend with loading states and error handling.\"",
                Category: "dotnet-react",
                Difficulty: "intermediate",
                Tags: new List<string> { "react", "hooks", "api", "state" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),
            new Tip(
                Id: "dotnet-3",
                Title: "Entity Framework with AI",
                Content: "Use AI to design your EF Core models and relationships. Provide your requirements and let AI create the DbContext, entities, and configurations.",
                Category: "dotnet-react",
                Difficulty: "intermediate",
                Tags: new List<string> { "entityframework", "models", "database" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),
            new Tip(
                Id: "dotnet-4",
                Title: "TypeScript Types from C# Models",
                Content: "Ask AI to convert your C# DTOs to TypeScript interfaces: \"Convert these C# models to TypeScript interfaces for my React frontend.\"",
                Category: "dotnet-react",
                Difficulty: "beginner",
                Tags: new List<string> { "typescript", "models", "conversion" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),
            new Tip(
                Id: "dotnet-5",
                Title: "Validation with FluentValidation",
                Content: "Let AI create FluentValidation rules for your .NET models and corresponding client-side validation for React forms.",
                Category: "dotnet-react",
                Difficulty: "intermediate",
                Tags: new List<string> { "validation", "forms", "fluentvalidation" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),
            new Tip(
                Id: "dotnet-6",
                Title: "SignalR with React",
                Content: "Ask AI to set up real-time communication: \"Create a SignalR hub in .NET and React client for real-time notifications.\" Perfect for chat apps or live updates.",
                Category: "dotnet-react",
                Difficulty: "advanced",
                Tags: new List<string> { "signalr", "realtime", "websockets" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),
            new Tip(
                Id: "dotnet-7",
                Title: "React Query with .NET APIs",
                Content: "Use AI to create React Query configurations for your .NET endpoints: \"Set up React Query hooks for all my .NET API endpoints with proper caching.\"",
                Category: "dotnet-react",
                Difficulty: "intermediate",
                Tags: new List<string> { "react-query", "caching", "api" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),
            new Tip(
                Id: "dotnet-8",
                Title: "Authentication Flow",
                Content: "Let AI implement full authentication: \"Create JWT authentication in .NET API with React login/logout components and protected routes.\"",
                Category: "dotnet-react",
                Difficulty: "advanced",
                Tags: new List<string> { "authentication", "jwt", "security" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),
            new Tip(
                Id: "dotnet-9",
                Title: "Vertical Slice Architecture",
                Content: "Ask AI to organize your .NET project using Vertical Slice Architecture: \"Restructure this feature into a vertical slice with its own models, services, and controllers.\"",
                Category: "dotnet-react",
                Difficulty: "advanced",
                Tags: new List<string> { "architecture", "vertical-slice", "organization" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),
            new Tip(
                Id: "dotnet-10",
                Title: "React Component Libraries",
                Content: "Use AI to create reusable React component libraries: \"Create a design system with Button, Input, and Modal components that work with my .NET backend.\"",
                Category: "dotnet-react",
                Difficulty: "intermediate",
                Tags: new List<string> { "components", "design-system", "reusability" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),

            // Best Practices
            new Tip(
                Id: "best-1",
                Title: "Be Specific in Your Requests",
                Content: "Instead of \"make this better\", try \"refactor this function to use async/await and add error handling\". Specific requests get better results.",
                Category: "best-practices",
                Difficulty: "beginner",
                Tags: new List<string> { "communication", "specificity", "results" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),
            new Tip(
                Id: "best-2",
                Title: "Review AI Suggestions Carefully",
                Content: "Always review AI-generated code before accepting. Check for logic errors, security issues, and alignment with your project's patterns and standards.",
                Category: "best-practices",
                Difficulty: "beginner",
                Tags: new List<string> { "review", "security", "quality" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),
            new Tip(
                Id: "best-3",
                Title: "Provide Context Gradually",
                Content: "Start with basic context, then add more details as needed. Don't overwhelm AI with too much information initially - build up the context conversation by conversation.",
                Category: "best-practices",
                Difficulty: "intermediate",
                Tags: new List<string> { "context", "gradual", "conversation" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),
            new Tip(
                Id: "best-4",
                Title: "Use Examples in Requests",
                Content: "Include examples of what you want: \"Create a React component like this one @ExampleComponent.tsx but for user profiles instead of products.\"",
                Category: "best-practices",
                Difficulty: "beginner",
                Tags: new List<string> { "examples", "patterns", "clarity" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),
            new Tip(
                Id: "best-5",
                Title: "Test AI Suggestions",
                Content: "Always test AI-generated code. Run tests, check for compilation errors, and verify the functionality works as expected before committing.",
                Category: "best-practices",
                Difficulty: "beginner",
                Tags: new List<string> { "testing", "verification", "quality" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),

            // Productivity
            new Tip(
                Id: "prod-1",
                Title: "Use AI for Documentation",
                Content: "Ask AI to \"add JSDoc comments to this function\" or \"create a README for this component\". AI excels at generating clear, comprehensive documentation.",
                Category: "productivity",
                Difficulty: "beginner",
                Tags: new List<string> { "documentation", "comments", "readme" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),
            new Tip(
                Id: "prod-2",
                Title: "Automate Repetitive Tasks",
                Content: "Use AI to generate repetitive code patterns: \"Create CRUD operations for all my entities\" or \"Generate form components for all my DTOs\".",
                Category: "productivity",
                Difficulty: "intermediate",
                Tags: new List<string> { "automation", "crud", "forms" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),
            new Tip(
                Id: "prod-3",
                Title: "Quick Prototyping",
                Content: "Use AI to rapidly prototype ideas: \"Create a simple dashboard component with charts and tables\" - perfect for demonstrating concepts quickly.",
                Category: "productivity",
                Difficulty: "beginner",
                Tags: new List<string> { "prototyping", "rapid", "demo" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),
            new Tip(
                Id: "prod-4",
                Title: "Convert Between Formats",
                Content: "Use AI to convert data formats: \"Convert this JSON to C# classes\", \"Convert this SQL to LINQ\", or \"Convert this CSS to Tailwind classes\".",
                Category: "productivity",
                Difficulty: "beginner",
                Tags: new List<string> { "conversion", "formats", "transformation" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),
            new Tip(
                Id: "prod-5",
                Title: "Error Resolution",
                Content: "Copy error messages and ask AI: \"How do I fix this error: [paste error]\". AI can often provide step-by-step solutions.",
                Category: "productivity",
                Difficulty: "beginner",
                Tags: new List<string> { "debugging", "errors", "troubleshooting" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),

            // Troubleshooting
            new Tip(
                Id: "trouble-1",
                Title: "Debug with Context",
                Content: "When debugging, provide relevant context: \"This React component isn't updating when props change\" and include the component code and parent component.",
                Category: "troubleshooting",
                Difficulty: "intermediate",
                Tags: new List<string> { "debugging", "context", "react" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),
            new Tip(
                Id: "trouble-2",
                Title: "Performance Debugging",
                Content: "Ask AI to analyze performance issues: \"Why is this React component re-rendering too often?\" Include the code and explain the observed behavior.",
                Category: "troubleshooting",
                Difficulty: "intermediate",
                Tags: new List<string> { "performance", "re-rendering", "optimization" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),
            new Tip(
                Id: "trouble-3",
                Title: "API Integration Issues",
                Content: "When API calls fail, show AI both frontend and backend code: \"My React component can't fetch data from my .NET API\" - include both sides of the communication.",
                Category: "troubleshooting",
                Difficulty: "intermediate",
                Tags: new List<string> { "api", "integration", "debugging" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),
            new Tip(
                Id: "trouble-4",
                Title: "Dependency Conflicts",
                Content: "Share your package.json or .csproj with AI when facing dependency issues: \"I'm getting version conflicts with these packages\" - AI can suggest resolutions.",
                Category: "troubleshooting",  
                Difficulty: "beginner",
                Tags: new List<string> { "dependencies", "versions", "packages" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),
            new Tip(
                Id: "trouble-5",
                Title: "Build and Deployment Issues",
                Content: "When facing build errors, share the full error log with AI: \"My React build is failing with this error\" - include environment details and build configuration.",
                Category: "troubleshooting",
                Difficulty: "intermediate",
                Tags: new List<string> { "build", "deployment", "errors" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),

            // Windsurf-Specific Tips (Codeium's AI IDE)
            new Tip(
                Id: "windsurf-1",
                Title: "Cascade for Autonomous Development",
                Content: "Use Windsurf's Cascade feature for autonomous AI development. Ask it to \"build a complete feature\" and watch as it creates multiple files, handles dependencies, and implements the full workflow.",
                Category: "ai-first-dev",
                Difficulty: "intermediate",
                Tags: new List<string> { "windsurf", "cascade", "autonomous" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),
            new Tip(
                Id: "windsurf-2",
                Title: "Flow for Collaborative Editing",
                Content: "Windsurf's Flow mode enables collaborative editing with AI. Use it when you want to work together with AI on complex refactoring or feature development in real-time.",
                Category: "ai-first-dev",
                Difficulty: "intermediate",
                Tags: new List<string> { "windsurf", "flow", "collaborative" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),
            new Tip(
                Id: "windsurf-3",
                Title: "Project Analysis with Windsurf",
                Content: "Ask Windsurf to \"analyze my project structure and suggest improvements\". It can understand your entire codebase and provide architectural recommendations and refactoring suggestions.",
                Category: "ai-first-dev",
                Difficulty: "advanced",
                Tags: new List<string> { "windsurf", "analysis", "architecture" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),
            new Tip(
                Id: "windsurf-4",
                Title: "Task Planning in Windsurf",
                Content: "Use Windsurf for comprehensive task planning: \"Break down this feature into development tasks with implementation steps\". It excels at project management and development planning.",
                Category: "ai-first-dev",
                Difficulty: "intermediate",
                Tags: new List<string> { "windsurf", "planning", "tasks" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            ),
            new Tip(
                Id: "windsurf-5",
                Title: "Legacy Code Modernization",
                Content: "Windsurf excels at modernizing legacy code. Ask it to \"upgrade this old code to modern patterns\" and it will handle migrations, dependency updates, and pattern improvements systematically.",
                Category: "ai-first-dev",
                Difficulty: "advanced",
                Tags: new List<string> { "windsurf", "legacy", "modernization" },
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: false,
                CreatedBy: "system"
            )
        };
    }

    public async Task<TipsCollectionResponse> GetTipsAsync(string? category = null, string? sessionId = null)
    {
        List<Tip> tips;
        
        if (string.IsNullOrEmpty(category))
        {
            // Get all tips from default categories
            tips = await GetTipsFromMultipleCategoriesAsync();
        }
        else
        {
            // Get tips from specific category
            var tipIds = await _redis.ListRangeAsync($"{CATEGORY_PREFIX}{category}");
            tips = new List<Tip>();
            
            foreach (var tipId in tipIds)
            {
                var tip = await _redis.GetAsync<Tip>($"{TIPS_PREFIX}{tipId}");
                if (tip != null)
                {
                    tips.Add(tip);
                }
            }
        }

        var categories = await GetCategoriesAsync();
        var userLikes = new HashSet<string>();
        
        if (!string.IsNullOrEmpty(sessionId))
        {
            var likedTips = await _redis.ListRangeAsync($"{USER_LIKES_PREFIX}{sessionId}");
            userLikes = likedTips.ToHashSet();
        }

        var tipResponses = tips.Select(tip => new TipResponse(
            Id: tip.Id,
            Title: tip.Title,
            Content: tip.Content,
            Category: tip.Category,
            Difficulty: tip.Difficulty,
            Tags: tip.Tags,
            CreatedAt: tip.CreatedAt,
            Likes: tip.Likes,
            IsLikedByUser: userLikes.Contains(tip.Id),
            IsAiGenerated: tip.IsAiGenerated,
            CreatedBy: tip.CreatedBy
        )).OrderByDescending(t => t.Likes).ThenByDescending(t => t.CreatedAt).ToList();

        return new TipsCollectionResponse(
            Tips: tipResponses,
            TotalCount: tipResponses.Count,
            Category: category ?? "All",
            AvailableCategories: categories
        );
    }

    public async Task<Tip> CreateTipAsync(CreateTipRequest request, string sessionId)
    {
        _logger.LogInformation("🚫 TEMPORARY FIX: Skipping session validation for tips. SessionId: {SessionId}", sessionId);
        
        // Temporarily bypass session validation - same fix as GameService
        var session = new UserSession(
            SessionId: sessionId,
            Name: "Anonymous User",
            IpHash: "temp-hash",
            CreatedAt: DateTime.UtcNow,
            LastActivity: DateTime.UtcNow,
            HasCompletedQuiz: false
        );
        
        /*
        var session = await _userSession.GetSessionAsync(sessionId);
        if (session == null)
        {
            throw new InvalidOperationException("Invalid session");
        }
        */

        var tipId = Guid.NewGuid().ToString();
        var tip = new Tip(
            Id: tipId,
            Title: request.Title,
            Content: request.Content,
            Category: request.Category,
            Difficulty: request.Difficulty,
            Tags: request.Tags,
            CreatedAt: DateTime.UtcNow,
            Likes: 0,
            IsAiGenerated: false,
            CreatedBy: session.Name,
            CreatedByIpHash: session.IpHash
        );

        // Store the tip
        await _redis.SetAsync($"{TIPS_PREFIX}{tipId}", tip, TimeSpan.FromDays(365));
        
        // Add to category list
        await _redis.ListPushAsync($"{CATEGORY_PREFIX}{request.Category}", tipId);
        
        // Add category to categories list if not exists
        var categories = await GetCategoriesAsync();
        if (!categories.Contains(request.Category))
        {
            await _redis.ListPushAsync(CATEGORIES_KEY, request.Category);
        }

        _logger.LogInformation("Created tip {TipId} in category {Category} by user {UserName}", 
            tipId, request.Category, session.Name);
        return tip;
    }

    public async Task<List<Tip>> GenerateAiTipsAsync(GenerateTipsRequest request)
    {
        var aiTips = await _openAI.GenerateTipsAsync(request.Category, request.Count);
        var tips = new List<Tip>();

        foreach (var aiTip in aiTips)
        {
            var tipId = Guid.NewGuid().ToString();
            var tip = new Tip(
                Id: tipId,
                Title: ExtractTitleFromTip(aiTip),
                Content: aiTip,
                Category: request.Category,
                Difficulty: request.Difficulty ?? "Medium",
                Tags: ExtractTagsFromCategory(request.Category),
                CreatedAt: DateTime.UtcNow,
                Likes: 0,
                IsAiGenerated: true
            );

            // Store the tip
            await _redis.SetAsync($"{TIPS_PREFIX}{tipId}", tip, TimeSpan.FromDays(30));
            
            // Add to category list
            await _redis.ListPushAsync($"{CATEGORY_PREFIX}{request.Category}", tipId);
            
            tips.Add(tip);
        }

        _logger.LogInformation("Generated {Count} AI tips for category {Category}", tips.Count, request.Category);
        return tips;
    }

    public async Task<bool> LikeTipAsync(string tipId, string sessionId)
    {
        _logger.LogInformation("🚫 TEMPORARY FIX: Skipping session validation for liking tip {TipId}. SessionId: {SessionId}", tipId, sessionId);
        
        // Temporarily bypass session validation - same fix as GameService
        var session = new UserSession(
            SessionId: sessionId,
            Name: "Anonymous User",
            IpHash: "temp-hash",
            CreatedAt: DateTime.UtcNow,
            LastActivity: DateTime.UtcNow,
            HasCompletedQuiz: false
        );
        
        /*
        _logger.LogInformation("Attempting to like tip {TipId} for session {SessionId}", tipId, sessionId);
        
        var session = await _userSession.GetSessionAsync(sessionId);
        if (session == null)
        {
            _logger.LogWarning("Session {SessionId} not found when trying to like tip {TipId}", sessionId, tipId);
            return false;
        }
        */

        _logger.LogDebug("Session {SessionId} found: {SessionName}", sessionId, session.Name);

        var tip = await _redis.GetAsync<Tip>($"{TIPS_PREFIX}{tipId}");
        if (tip == null)
        {
            _logger.LogWarning("Tip {TipId} not found in Redis when trying to like", tipId);
            return false;
        }

        _logger.LogDebug("Tip {TipId} found: {TipTitle}", tipId, tip.Title);

        // Check if already liked
        var userLikes = await _redis.ListRangeAsync($"{USER_LIKES_PREFIX}{sessionId}");
        if (userLikes.Contains(tipId))
        {
            _logger.LogInformation("User {SessionId} has already liked tip {TipId}", sessionId, tipId);
            return false; // Already liked
        }

        _logger.LogDebug("User {SessionId} has not liked tip {TipId} yet, proceeding with like", sessionId, tipId);

        // Add like
        await _redis.ListPushAsync($"{USER_LIKES_PREFIX}{sessionId}", tipId);
        
        // Update tip likes count
        var updatedTip = tip with { Likes = tip.Likes + 1 };
        await _redis.SetAsync($"{TIPS_PREFIX}{tipId}", updatedTip, TimeSpan.FromDays(365));

        _logger.LogInformation("User {SessionId} successfully liked tip {TipId}. New like count: {LikeCount}", sessionId, tipId, updatedTip.Likes);
        return true;
    }

    public async Task<bool> UnlikeTipAsync(string tipId, string sessionId)
    {
        var session = await _userSession.GetSessionAsync(sessionId);
        if (session == null)
        {
            return false;
        }

        var tip = await _redis.GetAsync<Tip>($"{TIPS_PREFIX}{tipId}");
        if (tip == null)
        {
            return false;
        }

        // Remove from user likes (simplified - would need better implementation)
        // For demo purposes, we'll just decrement the count
        var updatedTip = tip with { Likes = Math.Max(0, tip.Likes - 1) };
        await _redis.SetAsync($"{TIPS_PREFIX}{tipId}", updatedTip, TimeSpan.FromDays(365));

        _logger.LogInformation("User {SessionId} unliked tip {TipId}", sessionId, tipId);
        return true;
    }

    public async Task<List<string>> GetCategoriesAsync()
    {
        var categories = await _redis.ListRangeAsync(CATEGORIES_KEY);
        if (!categories.Any())
        {
            // Initialize with all categories that match our predefined tips
            var defaultCategories = new List<string>
            {
                "cursor-basics",
                "cursor-advanced",
                "ai-first-dev", 
                "dotnet-react",
                "best-practices",
                "productivity",
                "troubleshooting"
            };

            foreach (var category in defaultCategories)
            {
                await _redis.ListPushAsync(CATEGORIES_KEY, category);
            }

            return defaultCategories;
        }

        return categories;
    }

    public async Task<Tip?> GetTipAsync(string tipId)
    {
        return await _redis.GetAsync<Tip>($"{TIPS_PREFIX}{tipId}");
    }

    private async Task<List<Tip>> GetTipsFromMultipleCategoriesAsync()
    {
        var categories = await GetCategoriesAsync();
        var allTips = new List<Tip>();

        foreach (var category in categories)
        {
            var tipIds = await _redis.ListRangeAsync($"{CATEGORY_PREFIX}{category}");
            foreach (var tipId in tipIds.Take(10)) // Increased from 3 to 10 per category
            {
                var tip = await _redis.GetAsync<Tip>($"{TIPS_PREFIX}{tipId}");
                if (tip != null)
                {
                    allTips.Add(tip);
                }
            }
        }

        return allTips;
    }

    private string ExtractTitleFromTip(string tipContent)
    {
        // Extract first sentence or first 50 characters as title
        var sentences = tipContent.Split('.', '!', '?');
        var title = sentences[0].Trim();
        
        if (title.Length > 50)
        {
            title = title.Substring(0, 47) + "...";
        }

        return title;
    }

    private List<string> ExtractTagsFromCategory(string category)
    {
        return category switch
        {
            "Cursor Basics" => new List<string> { "cursor", "basics", "getting-started" },
            "AI Integration" => new List<string> { "ai", "integration", "models" },
            "Best Practices" => new List<string> { "best-practices", "workflow", "efficiency" },
            "Advanced Features" => new List<string> { "advanced", "features", "power-user" },
            "Productivity Tips" => new List<string> { "productivity", "tips", "shortcuts" },
            "Debugging" => new List<string> { "debugging", "troubleshooting", "errors" },
            "Code Generation" => new List<string> { "code-generation", "ai-assistance", "automation" },
            _ => new List<string> { "general", "tips" }
        };
    }

    /// <summary>
    /// Forces reseeding of tips by clearing the seeded flag. Useful for development.
    /// </summary>
    public async Task ForceReseedAsync()
    {
        try
        {
            _logger.LogInformation("Force reseeding tips - clearing all existing tips and seeding predefined ones");
            
            // Clear the seeded flag to allow reseeding
            await _redis.DeleteAsync(SEEDED_KEY);
            
            // Clear all existing tips and categories
            await ClearAllTipsAsync();
            
            // Re-seed predefined tips
            await SeedPredefinedTipsAsync();
            
            _logger.LogInformation("Force reseeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during force reseed");
            throw;
        }
    }

    public async Task CleanSessionCreatedTipsAsync()
    {
        try
        {
            _logger.LogInformation("Cleaning tips created by user sessions...");
            
            var tipKeys = await _redis.GetKeysAsync($"{TIPS_PREFIX}*");
            var cleanedCount = 0;
            
            foreach (var key in tipKeys)
            {
                try
                {
                    var tip = await _redis.GetAsync<Tip>(key);
                    if (tip != null && !tip.IsAiGenerated && !string.IsNullOrEmpty(tip.CreatedByIpHash))
                    {
                        // This is a user-created tip, remove it
                        await _redis.DeleteAsync(key);
                        
                        // Note: Since we don't have ListRemoveAsync, we'll rebuild category lists during next reseed
                        // This is acceptable since user-created tips are being cleaned up
                        
                        cleanedCount++;
                        _logger.LogDebug("Removed user-created tip: {TipId} - {TipTitle}", tip.Id, tip.Title);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error processing tip {Key} during cleanup", key);
                }
            }
            
            // Clean up category lists that might contain removed tips by rebuilding them
            await RebuildCategoryListsAsync();
            
            _logger.LogInformation("Cleaned {Count} user-created tips from Redis", cleanedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning session-created tips");
            throw;
        }
    }

    private async Task RebuildCategoryListsAsync()
    {
        try
        {
            // Clear all category lists
            var categoryKeys = await _redis.GetKeysAsync($"{CATEGORY_PREFIX}*");
            foreach (var key in categoryKeys)
            {
                await _redis.DeleteAsync(key);
            }
            
            // Rebuild category lists from existing tips
            var tipKeys = await _redis.GetKeysAsync($"{TIPS_PREFIX}*");
            foreach (var key in tipKeys)
            {
                if (key.Contains("category:") || key.Contains("likes:") || key.Contains("categories")) continue;
                
                try
                {
                    var tip = await _redis.GetAsync<Tip>(key);
                    if (tip != null)
                    {
                        await _redis.ListPushAsync($"{CATEGORY_PREFIX}{tip.Category}", tip.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error rebuilding category for tip {Key}", key);
                }
            }
            
            _logger.LogInformation("Rebuilt category lists after cleanup");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rebuilding category lists");
        }
    }

    private async Task ClearAllTipsAsync()
    {
        try
        {
            // Clear all tip data
            var tipKeys = await _redis.GetKeysAsync($"{TIPS_PREFIX}*");
            foreach (var key in tipKeys)
            {
                await _redis.DeleteAsync(key);
            }
            
            // Clear all category lists
            var categoryKeys = await _redis.GetKeysAsync($"{CATEGORY_PREFIX}*");
            foreach (var key in categoryKeys)
            {
                await _redis.DeleteAsync(key);
            }
            
            // Clear user likes
            var likesKeys = await _redis.GetKeysAsync($"{USER_LIKES_PREFIX}*");
            foreach (var key in likesKeys)
            {
                await _redis.DeleteAsync(key);
            }
            
            // Clear tip likes
            var tipLikesKeys = await _redis.GetKeysAsync($"{TIP_LIKES_PREFIX}*");
            foreach (var key in tipLikesKeys)
            {
                await _redis.DeleteAsync(key);
            }
            
            // Clear categories
            await _redis.DeleteAsync(CATEGORIES_KEY);
            
            _logger.LogInformation("Cleared all tips and related data from Redis");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing all tips");
            throw;
        }
    }
}