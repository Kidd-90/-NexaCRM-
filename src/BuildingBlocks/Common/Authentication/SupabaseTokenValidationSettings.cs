using System;
using System.Collections.Generic;

namespace BuildingBlocks.Common.Authentication;

/// <summary>
/// Configurable settings controlling how Supabase JWT tokens are validated.
/// These options can be bound from configuration (e.g. <c>Supabase:TokenValidation</c>).
/// </summary>
public sealed class SupabaseTokenValidationSettings
{
    /// <summary>
    /// Whether the validator should enforce the issuer contained in the token.
    /// Defaults to <c>true</c> because Supabase tokens include an issuer that can be derived from the Supabase URL.
    /// </summary>
    public bool ValidateIssuer { get; set; } = true;

    /// <summary>
    /// Whether the validator should enforce audience checks.
    /// Disabled by default because Supabase tokens are typically scoped to the "authenticated" audience
    /// but some service-level flows (e.g. service role key usage) might omit or change the audience claim.
    /// </summary>
    public bool ValidateAudience { get; set; }

    /// <summary>
    /// Audiences considered valid when <see cref="ValidateAudience"/> is enabled.
    /// A sensible default of <c>"authenticated"</c> is provided because it is the default audience emitted by Supabase Auth.
    /// </summary>
    public IList<string> ValidAudiences { get; set; } = new List<string> { "authenticated" };

    /// <summary>
    /// Amount of clock skew tolerated when validating expiration and not-before claims.
    /// Defaults to two minutes to account for small differences between Supabase and server clocks.
    /// </summary>
    public TimeSpan ClockSkew { get; set; } = TimeSpan.FromMinutes(2);
}
