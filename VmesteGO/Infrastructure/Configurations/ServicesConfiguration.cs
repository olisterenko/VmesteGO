using VmesteGO;
using VmesteGO.Services;
using VmesteGO.Services.Interfaces;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServicesConfiguration
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IJwtService, JwtService>();
        
        services.AddScoped<IFriendService, FriendService>();
        services.AddScoped<IUserContext, UserContext>();
        services.AddAutoMapper(typeof(MappingProfile));
        
        services.AddScoped<IEventService, EventService>();
        
        services.AddScoped<ICommentService, CommentService>();
        
        services.AddScoped<IEventInvitationService, EventInvitationService>();
        
        return services;
    }
}