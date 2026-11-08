using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace WorkoutManager.Web.Pages.Settings;

public partial class SettingsPage
{
    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private bool _isDeletingAccount = false;

    // TODO: Account deletion needs a server-side implementation. Users live in the Entra External ID
    // tenant, so deleting one requires Microsoft Graph calls (User.ReadWrite.All or similar) from
    // WorkoutManager.Api — it cannot be done from the browser with MSAL. That endpoint should also
    // delete the user's rows from the database. Until it exists, say so instead of failing silently.
    private Task DeleteAccount()
    {
        Snackbar.Add("Account deletion isn't available yet.", Severity.Info);
        return Task.CompletedTask;
    }
}
