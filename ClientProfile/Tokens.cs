namespace ClientProfile;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Object that stores user tokens.
/// </summary>
internal class Tokens
{
    private string accessToken;
    private DateTime accessTokenExpireDate;
    private string refreshToken;
    private DateTime refreshTokenExpireDate;
    private string membershipID;
    private string destinyMembershipID;
    private string membershipType = "1";

    /// <summary>
    /// Gets or sets the Access Token for the client.
    /// </summary>
    public string AccessToken { get => this.accessToken; set => this.accessToken = value; }

    /// <summary>
    /// Gets or sets the Access Token Expire Date for the client.
    /// </summary>
    public DateTime AccessTokenExpireDate { get => this.accessTokenExpireDate; set => this.accessTokenExpireDate = value; }

    /// <summary>
    /// Gets or sets the Refresh Token for the client.
    /// </summary>
    public string RefreshToken { get => this.refreshToken; set => this.refreshToken = value; }

    /// <summary>
    /// Gets or sets the Refresh Token Expire Date for the client.
    /// </summary>
    public DateTime RefreshTokenExpireDate { get => this.refreshTokenExpireDate; set => this.refreshTokenExpireDate = value; }

    /// <summary>
    /// Gets or sets the Membership ID for the client.
    /// </summary>
    public string MembershipID { get => this.membershipID; set => this.membershipID = value; }

    /// <summary>
    /// Gets or sets the Destiny Membership ID for the client.
    /// </summary>
    public string DestinyMembershipID { get => this.destinyMembershipID; set => this.destinyMembershipID = value; }

    /// <summary>
    /// Gets or sets the Membership Type for the client.
    /// </summary>
    public string MembershipType { get => this.membershipType; set => this.membershipType = value; }
}
