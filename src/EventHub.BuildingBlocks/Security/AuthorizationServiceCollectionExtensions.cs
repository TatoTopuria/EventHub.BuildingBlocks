using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Security;

/// <summary>
/// Registers the shared EventHub authorization policies in a service collection.
/// </summary>
public static class AuthorizationServiceCollectionExtensions
{
    /// <summary>
    /// Adds the four EventHub policies: <see cref="EventHubPolicies.CustomerOrAbove"/>,
    /// <see cref="EventHubPolicies.OrganizerOrAbove"/>, <see cref="EventHubPolicies.AdminOnly"/>,
    /// and <see cref="EventHubPolicies.BookingOwnerOrAdmin"/>.
    /// </summary>
    /// <remarks>
    /// The resource-based <see cref="EventHubPolicies.BookingOwnerOrAdmin"/> policy is wired here
    /// but the corresponding <see cref="BookingOwnerAuthorizationHandler"/> needs an
    /// <see cref="IBookingOwnershipResolver"/> implementation. Services that intend to enforce
    /// this policy must additionally call <see cref="AddBookingOwnerAuthorization{TResolver}"/>
    /// to register both the resolver and the handler.
    /// </remarks>
    public static IServiceCollection AddEventHubAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(EventHubPolicies.CustomerOrAbove, policy => policy.RequireRole(
                EventHubRoles.Customer,
                EventHubRoles.Organizer,
                EventHubRoles.Admin));

            options.AddPolicy(EventHubPolicies.OrganizerOrAbove, policy => policy.RequireRole(
                EventHubRoles.Organizer,
                EventHubRoles.Admin));

            options.AddPolicy(EventHubPolicies.AdminOnly, policy => policy.RequireRole(
                EventHubRoles.Admin));

            options.AddPolicy(EventHubPolicies.BookingOwnerOrAdmin, policy =>
            {
                // The role gate is intentionally permissive ("anyone authenticated"). The real
                // check is the resource-based requirement below, which the handler evaluates by
                // loading the booking and comparing the owning customer id with the caller.
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new BookingOwnerOrAdminRequirement());
            });
        });

        return services;
    }

    /// <summary>
    /// Registers the <see cref="BookingOwnerAuthorizationHandler"/> together with the service's
    /// own <typeparamref name="TResolver"/> implementation of <see cref="IBookingOwnershipResolver"/>.
    /// </summary>
    /// <remarks>
    /// Scoped so the handler can pull in scoped deps (EF DbContext in the in-process case, gRPC
    /// client + channel in the cross-service case). Returns the service collection to keep the
    /// fluent registration story consistent with the rest of the EventHub composition root.
    /// </remarks>
    public static IServiceCollection AddBookingOwnerAuthorization<TResolver>(this IServiceCollection services)
        where TResolver : class, IBookingOwnershipResolver
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IBookingOwnershipResolver, TResolver>();
        services.AddScoped<IAuthorizationHandler, BookingOwnerAuthorizationHandler>();
        return services;
    }
}
