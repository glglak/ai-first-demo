using Microsoft.Extensions.DependencyInjection;
using AiFirstDemo.Features.UserSessions;
using AiFirstDemo.Features.Quiz;
using AiFirstDemo.Features.Quiz.Models;
using AiFirstDemo.Features.SpaceshipGame;
using AiFirstDemo.Features.SpaceshipGame.Models;
using AiFirstDemo.Features.TipsAndTricks;
using AiFirstDemo.Features.Analytics;

namespace AiFirstDemo.Features;

public static class DependencyInjection
{
    public static IServiceCollection AddFeatures(this IServiceCollection services)
    {
        // Add feature-specific services
        services.AddScoped<IUserSessionService, UserSessionService>();
        services.AddScoped<IQuizService, QuizService>();
        services.AddScoped<IGameService, GameService>();
        services.AddScoped<ITipsService, TipsService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        
        return services;
    }
}