using Amazon.S3;
using VmesteGO.Options;
using VmesteGO.Services;
using VmesteGO.Services.Interfaces;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class S3Configuration
{
    public static IServiceCollection AddS3(this IServiceCollection services, IConfiguration configuration)
    {
        var s3Options = configuration.GetSection("S3");
        services.Configure<S3Options>(configuration.GetSection("S3"));

        services.AddSingleton<IAmazonS3>(_ =>
        {
            var s3Config = new AmazonS3Config
            {
                ServiceURL = s3Options["EndpointUrl"],
                ForcePathStyle = true
            };

            return new AmazonS3Client(s3Options["AccessKey"], s3Options["SecretKey"], s3Config);
        });
        
        services.AddSingleton<IS3StorageService, S3StorageService>();
        
        return services;
    }
}