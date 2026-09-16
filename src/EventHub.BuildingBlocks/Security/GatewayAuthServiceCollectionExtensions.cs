using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Security;

/// <summary>
/// Registers the gateway-forwarded-claims authentication scheme used by every
/// service that sits behind the API gateway.
/// </summary>
public static class GatewayAuthServiceCollectionExtensions
{
    /// <summary>
    /// Reads the <c>GatewayAuth</c> configuration section, validates that <see cref="GatewayAuthOptions.SharedSecret"/>
    /// is set, and registers the <see cref="GatewayForwardedClaimsHandler"/> authentication scheme.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the <c>GatewayAuth</c> section is missing or <c>SharedSecret</c> is empty. Failing
    /// fast at startup prevents the service from booting in a permissive (or silently broken) state.
    /// </exception>
    public static IServiceCollection AddGatewayForwardedAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration.GetSection(GatewayAuthOptions.SectionName).Get<GatewayAuthOptions>()
            ?? throw new InvalidOperationException(
                $"Configuration section '{GatewayAuthOptions.SectionName}' is required for gateway authentication.");

        if (string.IsNullOrWhiteSpace(options.SharedSecret))
        {
            throw new InvalidOperationException(
                $"{GatewayAuthOptions.SectionName}:SharedSecret is required for gateway authentication.");
        }

        services.Configure<GatewayAuthOptions>(configuration.GetSection(GatewayAuthOptions.SectionName));
        services.AddSingleton(options);

        services.AddAuthentication(GatewayForwardedClaimsHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, GatewayForwardedClaimsHandler>(
                GatewayForwardedClaimsHandler.SchemeName,
                _ => { });

        return services;
    }
}
