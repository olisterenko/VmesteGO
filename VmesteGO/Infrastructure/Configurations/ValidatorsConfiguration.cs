using FluentValidation;
using VmesteGO.Dto.Requests;
using VmesteGO.Validators;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ValidatorsConfiguration
{
    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddScoped<IValidator<CreateEventRequest>, CreateEventRequestValidator>();
        services.AddScoped<IValidator<UserRegisterRequest>, UserRegisterRequestValidator>();
        services.AddScoped<IValidator<UserLoginRequest>, UserLoginRequestValidator>();
        services.AddScoped<IValidator<UserUpdateRequest>, UserUpdateRequestValidator>();
        return services;
    }
}