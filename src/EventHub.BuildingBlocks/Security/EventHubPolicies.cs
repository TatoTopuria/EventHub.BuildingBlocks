namespace BuildingBlocks.Security;

/// <summary>
/// Names of authorization policies registered by <c>AddEventHubAuthorization</c>.
/// Use these constants in <c>[Authorize(Policy = ...)]</c> attributes to avoid magic strings.
/// </summary>
/// <remarks>
/// The policies form a simple role hierarchy: Admin satisfies Organizer and Customer; Organizer satisfies Customer.
/// </remarks>
public static class EventHubPolicies
{
    /// <summary>Any user assigned at least the Customer role (i.e. every authenticated EventHub user).</summary>
    public const string CustomerOrAbove = "EventHub.CustomerOrAbove";

    /// <summary>Organizer or Admin. Used for event creation, event lifecycle changes, etc.</summary>
    public const string OrganizerOrAbove = "EventHub.OrganizerOrAbove";

    /// <summary>Admin only. Used for cross-tenant analytics and operational endpoints.</summary>
    public const string AdminOnly = "EventHub.AdminOnly";

    /// <summary>
    /// Resource-based policy: the authenticated user must be the owner of the booking identified
    /// by the <c>bookingId</c> route value, OR have the Admin role. Closes audit gap G4 — without
    /// this gate, any authenticated Customer could confirm or cancel another user's booking.
    /// </summary>
    /// <remarks>
    /// Requires a <c>IBookingOwnershipResolver</c> to be registered. The handler reads the
    /// resolver per-request, so each service can plug in its own lookup strategy: Booking uses the
    /// in-process EF repository, while a remote service would inject a gRPC-backed implementation.
    /// </remarks>
    public const string BookingOwnerOrAdmin = "EventHub.BookingOwnerOrAdmin";
}
