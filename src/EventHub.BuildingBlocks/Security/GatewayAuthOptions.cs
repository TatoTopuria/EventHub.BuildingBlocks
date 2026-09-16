namespace BuildingBlocks.Security;

/// <summary>
/// Options binding for the HMAC-signed claim-forwarding contract emitted by the API gateway.
/// </summary>
public sealed class GatewayAuthOptions
{
    public const string SectionName = "GatewayAuth";

    /// <summary>Shared secret used by both gateway and downstream to sign/verify the HMAC.</summary>
    public string SharedSecret { get; set; } = string.Empty;

    public string UserIdHeader { get; set; } = "X-Forwarded-UserId";
    public string RolesHeader { get; set; } = "X-Forwarded-Roles";
    public string SignatureHeader { get; set; } = "X-Gateway-Signature";
}
