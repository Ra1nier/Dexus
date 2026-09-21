// <copyright file="Profile.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ClientProfile;

using System.Text.Json;

/// <summary>
/// Server-side session credentials shared only by circuits authenticated to that session.
/// </summary>
public class Profile
{
    private SessionState state = new SessionState();

    /// <summary>Gets the current access token, or empty after logout/session expiry.</summary>
    public string AccessToken => this.IsActive ? this.state.Data.AccessToken : string.Empty;

    /// <summary>Gets the current refresh token, or empty after logout/session expiry.</summary>
    public string RefreshToken => this.IsActive ? this.state.Data.RefreshToken : string.Empty;

    /// <summary>Gets the Bungie membership ID returned by the token endpoint.</summary>
    public string MembershipID => this.IsActive ? this.state.Data.MembershipID : string.Empty;

    /// <summary>Gets the selected Destiny membership ID.</summary>
    public string? DestinyMembershipID => this.IsActive ? this.state.Data.DestinyID : null;

    /// <summary>Gets the selected Destiny membership type.</summary>
    public string? MembershipType => this.IsActive ? this.state.Data.MembershipType : null;

    /// <summary>Gets the UTC access token expiry.</summary>
    public DateTime AccessTokenExpireDate => this.state.Data.AccessExpiry;

    /// <summary>Gets the UTC refresh token expiry.</summary>
    public DateTime RefreshTokenExpireDate => this.state.Data.RefreshExpiry;

    /// <summary>Gets the lock coordinating refreshes across tabs of the same session.</summary>
    public SemaphoreSlim RefreshLock => this.state.RefreshLock;

    /// <summary>Gets a value indicating whether this session has not expired or been revoked.</summary>
    public bool IsActive => !this.state.Revoked && DateTimeOffset.UtcNow < this.state.ExpiresUtc;

    /// <summary>Binds a circuit to a session resolved by the host from an authenticated principal.</summary>
    /// <param name="session">Server-resolved session profile, never a browser-provided owner.</param>
    public void BindTo(Profile session)
    {
        this.state = session.state;
    }

    /// <summary>Sets the absolute server-session expiration.</summary>
    /// <param name="expiresUtc">UTC expiration matching the authentication cookie.</param>
    public void SetSessionExpiry(DateTimeOffset expiresUtc)
    {
        this.state.ExpiresUtc = expiresUtc;
    }

    /// <summary>Checks whether this session contains token values.</summary>
    /// <returns>True when both tokens exist in an active session.</returns>
    public bool Create() => !string.IsNullOrWhiteSpace(this.AccessToken) && !string.IsNullOrWhiteSpace(this.RefreshToken);

    /// <summary>Revokes this session, including all circuits bound to it.</summary>
    /// <returns>True after the credentials are cleared.</returns>
    public bool ThrowOut()
    {
        lock (this.state)
        {
            this.state.Revoked = true;
            this.state.Data = new TokenData();
        }

        return true;
    }

    /// <summary>Checks whether this session has a Destiny membership.</summary>
    /// <returns>True if the membership and type are present.</returns>
    public bool LoadDestinyID() => !string.IsNullOrWhiteSpace(this.DestinyMembershipID) && !string.IsNullOrWhiteSpace(this.MembershipType);

    /// <summary>Checks whether this session contains token values.</summary>
    /// <returns>True if token values exist.</returns>
    public bool StoreUserTokens() => this.Create();

    /// <summary>Validates a token response and atomically updates credentials and expiry dates.</summary>
    /// <param name="json">Token response JSON from Bungie.</param>
    /// <returns>False for malformed responses or revoked sessions.</returns>
    public bool StoreUserTokens(string json)
    {
        try
        {
            Tokens? candidate = JsonSerializer.Deserialize<Tokens>(json);
            if (candidate is null || string.IsNullOrWhiteSpace(candidate.AccessToken)
                || string.IsNullOrWhiteSpace(candidate.RefreshToken)
                || candidate.ExpiresIn <= 0 || candidate.RefreshExpiresIn <= 0)
            {
                return false;
            }

            DateTime now = DateTime.UtcNow;
            lock (this.state)
            {
                if (!this.IsActive)
                {
                    return false;
                }

                this.state.Data = new TokenData
                {
                    DestinyID = this.state.Data.DestinyID,
                    MembershipType = this.state.Data.MembershipType,
                    AccessToken = candidate.AccessToken,
                    RefreshToken = candidate.RefreshToken,
                    MembershipID = candidate.MembershipId ?? this.state.Data.MembershipID,
                    AccessExpiry = now.AddSeconds(candidate.ExpiresIn),
                    RefreshExpiry = now.AddSeconds(candidate.RefreshExpiresIn),
                };
            }

            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>Stores the Destiny identity returned by Bungie for this session.</summary>
    /// <param name="destinyMembershipID">Provider-returned Destiny membership ID.</param>
    /// <param name="membershipType">Provider-returned membership type.</param>
    /// <returns>True if this active session was updated.</returns>
    public bool StoreDestinyMembershipID(string destinyMembershipID, string membershipType)
    {
        if (string.IsNullOrWhiteSpace(destinyMembershipID) || string.IsNullOrWhiteSpace(membershipType))
        {
            return false;
        }

        lock (this.state)
        {
            if (!this.IsActive)
            {
                return false;
            }

            this.state.Data = new TokenData
            {
                AccessToken = this.state.Data.AccessToken,
                RefreshToken = this.state.Data.RefreshToken,
                AccessExpiry = this.state.Data.AccessExpiry,
                RefreshExpiry = this.state.Data.RefreshExpiry,
                MembershipID = this.state.Data.MembershipID,
                DestinyID = destinyMembershipID,
                MembershipType = membershipType,
            };
        }

        return true;
    }

    private sealed class SessionState
    {
        public TokenData Data { get; set; } = new TokenData();

        public bool Revoked { get; set; }

        public DateTimeOffset ExpiresUtc { get; set; } = DateTimeOffset.MaxValue;

        public SemaphoreSlim RefreshLock { get; } = new SemaphoreSlim(1, 1);
    }

    private sealed class TokenData
    {
        public string AccessToken { get; set; } = string.Empty;

        public string RefreshToken { get; set; } = string.Empty;

        public string MembershipID { get; set; } = string.Empty;

        public string? DestinyID { get; set; }

        public string? MembershipType { get; set; }

        public DateTime AccessExpiry { get; set; }

        public DateTime RefreshExpiry { get; set; }
    }
}
