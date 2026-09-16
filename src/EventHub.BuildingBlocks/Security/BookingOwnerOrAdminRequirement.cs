using Microsoft.AspNetCore.Authorization;

namespace BuildingBlocks.Security;

/// <summary>
/// Marker requirement consumed by <see cref="BookingOwnerAuthorizationHandler"/>. Has no payload
/// — the handler reads the <c>bookingId</c> route value from the active <c>HttpContext</c> and
/// the authenticated user's identifier from the supplied <see cref="AuthorizationHandlerContext.User"/>.
/// </summary>
public sealed class BookingOwnerOrAdminRequirement : IAuthorizationRequirement;
