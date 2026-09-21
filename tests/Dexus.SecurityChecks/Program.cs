using ClientProfile;
using OAuthService;
using Microsoft.Extensions.Configuration;

static void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

const string firstTokens = """{"access_token":"test-a","refresh_token":"refresh-a","expires_in":3600,"refresh_expires_in":7200}""";
const string secondTokens = """{"access_token":"test-b","refresh_token":"refresh-b","expires_in":3600,"refresh_expires_in":7200}""";
var first = new Profile();
var second = new Profile();
Check(!first.Create() && !second.Create(), "A new session must not restore shared tokens.");
await Task.WhenAll(Task.Run(() => Check(first.StoreUserTokens(firstTokens), "First session failed.")),
    Task.Run(() => Check(second.StoreUserTokens(secondTokens), "Second session failed.")));
Check(first.AccessToken == "test-a" && second.AccessToken == "test-b", "Sessions leaked tokens.");
Check(!first.StoreUserTokens("{}") && !first.StoreUserTokens("invalid"), "Malformed tokens accepted.");
Check(first.AccessToken == "test-a", "Invalid response replaced valid tokens.");
Check(first.ThrowOut() && !first.Create(), "Logout retained tokens.");
Check(second.AccessToken == "test-b", "Logout affected another session.");
Check(!new Profile().Create(), "A new circuit inherited another circuit's tokens.");
var values = new Dictionary<string, string?>
{
    ["BungieAPIKeys:API_Key"] = "test-key",
    ["BungieAPIKeys:Client_ID"] = "test-id",
    ["BungieAPIKeys:Client_Secret"] = "test-secret",
    ["Dexus:AllowCredentialEntry"] = "true",
};
var disabled = new OAuthProfile(new ConfigurationBuilder().Build());
Check(!disabled.HasValidAPIKeys(), "Unconfigured server accepted credentials.");
// Credentials enable sign-in directly; the previous demo gate must not disable OAuth.
values["Dexus:EnableLiveBungie"] = "false";
var enabled = new OAuthProfile(new ConfigurationBuilder().AddInMemoryCollection(values).Build());
Check(enabled.HasValidAPIKeys(), "Server credentials did not enable Bungie sign-in.");
string state = enabled.State;
Check(state.Length == 64 && enabled.ValidateState(state), "Invalid state generation.");
enabled.RefreshState();
Check(!enabled.ValidateState(state) && !enabled.ValidateState(null), "Stale state accepted.");
Console.WriteLine("Security checks passed: session isolation, logout, invalid tokens, server credential configuration, state generation.");

await OAuthFlowChecks.RunAsync();
