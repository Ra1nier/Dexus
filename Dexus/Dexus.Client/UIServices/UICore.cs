namespace Dexus.Client.UIServices;

using DexusCore;
using Microsoft.AspNetCore.Components;

/// <summary>
/// Extends the DexusCore's functionality into the UI space by allowing some integration between the Core and UI space. Should only be used in special circumstances when integration between the Backend and UI Space is necessary.
/// </summary>
public class UICore
{
    /// <summary>
    /// Singleton instance of UI Libs and Core used by DI.
    /// </summary>
    private readonly Core Core;
    private readonly DialogUIService Dialog;
    private readonly NavigationManager Navigation;

    /// <summary>
    /// Constructor for UICore.
    /// </summary>
    /// <param name="core">DI Core instance.</param>
    /// <param name="dialog">DI DialogUIService instance.</param>
    /// <param name="navigation">DI NavigationManager instance.</param>
    public UICore(Core core, DialogUIService dialog, NavigationManager navigation)
    {
        this.Core = core;
        this.Dialog = dialog;
        this.Navigation = navigation;
    }

    /// <summary>
    /// Verifies that Dexus is currently able to process API requests.
    /// </summary>
    public async Task VerifyAPIAvaliablity()
    {
        switch (Core.OAuthStatus)
        {
            case 3:
                Dialog.SendDialog("No API Tokens.",
                    "Dexus does not have any saved API Tokens for Bungie. Would you like to be redirected to a page where they can be inputted?",
                    () => { Navigation.NavigateTo("/DexusAccount?signInRedirect=false&inputTokenRedirect=true"); });
                return;
            case 2:
                Dialog.SendDialog("Not Authorized with Bungie.",
                    "You have not authorized Dexus to access your Bungie information or authorization has expired. Would you like to be redirected to the sign in page?",
                    () => { Navigation.NavigateTo("/DexusAccount?signInRedirect=true&inputTokenRedirect=false"); });
                return;
            case 1:
                {
                    if (!await Core.ConfirmOAuthStatus())
                        Dialog.SendDialog("Bungie Authorization Error.",
                            "Your authorization with Bungie has expired and Dexus was unable to refresh it. You really should not be seeing this error but resigning in should fix it. Would you like to be redirected to the sign in page?",
                            () =>
                            {
                                Navigation.NavigateTo("/DexusAccount?signInRedirect=true&inputTokenRedirect=false");
                            });
                    break;
                }
        }
    }
}