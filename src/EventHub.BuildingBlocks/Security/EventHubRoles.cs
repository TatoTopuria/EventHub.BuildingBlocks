namespace BuildingBlocks.Security;

/// <summary>
/// Canonical role names emitted by the Identity service and recognised by every downstream API.
/// Values must match the <c>Identity.Service.Domain.Enums.Role</c> string representations exactly,
/// because role claim comparisons are case-sensitive in <see cref="System.Security.Claims.ClaimsPrincipal.IsInRole"/>.
/// </summary>
public static class EventHubRoles
{
    public const string Customer = "Customer";
    public const string Organizer = "Organizer";
    public const string Admin = "Admin";
}
