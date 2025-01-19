using VmesteGO;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class RepositoriesConfiguration
{
    public static IServiceCollection AddDbContextWithRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddApplicationDbContext(configuration)
            .AddScoped(typeof(IRepository<>), typeof(Repository<>));
    }
}