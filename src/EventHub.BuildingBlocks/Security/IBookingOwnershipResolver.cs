namespace BuildingBlocks.Security;

/// <summary>
/// Resolves the owning customer for a booking. The booking-side service plugs in an EF-backed
/// implementation; downstream services that also need to authorize on booking ownership (e.g.
/// Payment, if it ever exposes a customer-facing manual-charge endpoint) can plug in a
/// gRPC-backed implementation that calls Booking.
/// </summary>
/// <remarks>
/// Returning <see langword="null"/> from <see cref="GetOwnerCustomerIdAsync"/> means the booking
/// does not exist (or is unreadable). The
/// <see cref="BookingOwnerAuthorizationHandler"/> treats both "not found" and "not owner" as
/// authorization failure — deliberately, so attackers cannot probe the existence of other users'
/// bookings via 403-vs-404 differences.
/// </remarks>
public interface IBookingOwnershipResolver
{
    Task<Guid?> GetOwnerCustomerIdAsync(Guid bookingId, CancellationToken cancellationToken);
}
