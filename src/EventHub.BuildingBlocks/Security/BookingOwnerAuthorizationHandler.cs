using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Security;

/// <summary>
/// Authorization handler for <see cref="BookingOwnerOrAdminRequirement"/>. Succeeds when the
/// authenticated user is the owner of the booking referenced in the <c>bookingId</c> route value,
/// or has the Admin role. Otherwise calls <see cref="AuthorizationHandlerContext.Fail()"/> which
/// the ASP.NET pipeline turns into HTTP 403.
/// </summary>
/// <remarks>
/// Defense-in-depth on top of the existing role gates: a role policy alone would still allow any
/// authenticated Customer to confirm or cancel another Customer's booking by guessing its id.
/// </remarks>
public sealed class BookingOwnerAuthorizationHandler : AuthorizationHandler<BookingOwnerOrAdminRequirement>
{
    public const string BookingIdRouteValue = "bookingId";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IBookingOwnershipResolver _resolver;
    private readonly ILogger<BookingOwnerAuthorizationHandler> _logger;

    public BookingOwnerAuthorizationHandler(
        IHttpContextAccessor httpContextAccessor,
        IBookingOwnershipResolver resolver,
        ILogger<BookingOwnerAuthorizationHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _resolver = resolver;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        BookingOwnerOrAdminRequirement requirement)
    {
        // Admin short-circuit: skip the booking lookup entirely. Admins are the operational
        // backstop and need to be able to confirm/cancel any booking; the lookup would still
        // succeed but it would be wasted DB / gRPC work.
        if (context.User.IsInRole(EventHubRoles.Admin))
        {
            context.Succeed(requirement);
            return;
        }

        var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? context.User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("BookingOwnerAuthFailed reason={Reason}", "MissingOrInvalidUserId");
            context.Fail();
            return;
        }

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            _logger.LogWarning("BookingOwnerAuthFailed reason={Reason}", "NoHttpContext");
            context.Fail();
            return;
        }

        if (!httpContext.Request.RouteValues.TryGetValue(BookingIdRouteValue, out var rawBookingId)
            || !Guid.TryParse(rawBookingId?.ToString(), out var bookingId))
        {
            _logger.LogWarning("BookingOwnerAuthFailed reason={Reason}", "MissingOrInvalidBookingIdRouteValue");
            context.Fail();
            return;
        }

        Guid? ownerId;
        try
        {
            ownerId = await _resolver.GetOwnerCustomerIdAsync(bookingId, httpContext.RequestAborted);
        }
        catch (Exception exception)
        {
            // Resolver fault (DB outage, gRPC channel error). Fail closed so a transient backend
            // problem cannot turn into a privilege escalation. Logged at error so ops sees the
            // distinction between "not authorized" and "could not authorize".
            _logger.LogError(
                exception,
                "BookingOwnerAuthFailed reason={Reason} bookingId={BookingId}",
                "ResolverFault",
                bookingId);
            context.Fail();
            return;
        }

        if (ownerId is null)
        {
            // Treat "not found" identically to "not owner" so attackers cannot probe booking
            // existence via 403-vs-404 differences.
            _logger.LogInformation(
                "BookingOwnerAuthFailed reason={Reason} bookingId={BookingId}",
                "BookingNotFound",
                bookingId);
            context.Fail();
            return;
        }

        if (ownerId.Value == userId)
        {
            context.Succeed(requirement);
            return;
        }

        _logger.LogInformation(
            "BookingOwnerAuthFailed reason={Reason} bookingId={BookingId} callerId={CallerId}",
            "NotOwner",
            bookingId,
            userId);
        context.Fail();
    }
}
