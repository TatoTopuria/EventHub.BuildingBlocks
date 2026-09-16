using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Security;

/// <summary>
/// Authentication handler that materializes a <see cref="ClaimsPrincipal"/> from claims forwarded
/// by the API gateway, after verifying an HMAC-SHA256 signature over the <c>{userId}|{roles}</c> payload.
/// </summary>
/// <remarks>
/// The gateway is the only authority that ever sees and validates the user's JWT. Downstream services
/// trust this handler — and only this handler — to reconstruct the principal, which is why the HMAC
/// signature is mandatory and validated in constant time.
/// </remarks>
public sealed class GatewayForwardedClaimsHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Gateway";

    private readonly GatewayAuthOptions _options;

    public GatewayForwardedClaimsHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IOptions<GatewayAuthOptions> gatewayOptions)
        : base(options, logger, encoder)
    {
        _options = gatewayOptions.Value;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var headers = Request.Headers;
        if (!headers.TryGetValue(_options.SignatureHeader, out var signatureValues) || signatureValues.Count == 0)
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing gateway signature."));
        }

        var signature = signatureValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(signature))
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing gateway signature."));
        }

        var userId = headers[_options.UserIdHeader].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing forwarded user identifier."));
        }

        var roles = headers[_options.RolesHeader].FirstOrDefault() ?? string.Empty;
        var payload = string.Join('|', userId, roles);
        var expectedSignature = CreateSignature(payload, _options.SharedSecret);

        byte[] signatureBytes;
        try
        {
            signatureBytes = Convert.FromBase64String(signature);
        }
        catch (FormatException)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid gateway signature format."));
        }

        if (!CryptographicOperations.FixedTimeEquals(
                signatureBytes,
                Convert.FromBase64String(expectedSignature)))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid gateway signature."));
        }

        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
        if (!string.IsNullOrWhiteSpace(roles))
        {
            claims.AddRange(roles
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(role => new Claim(ClaimTypes.Role, role.Trim())));
        }

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private static string CreateSignature(string payload, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        using var hmac = new HMACSHA256(keyBytes);
        return Convert.ToBase64String(hmac.ComputeHash(payloadBytes));
    }
}
