using Microsoft.AspNetCore.Components;
using MudBlazor;
using WorkoutManager.Web.Services;
using WorkoutManager.Web.Components;

namespace WorkoutManager.Web.Pages.Settings;

public partial class SettingsPage
{
    [Inject]
    private IAuthService AuthService { get; set; } = default!;

    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private async Task DeleteAccount()
    {
        var parameters = new DialogParameters
        {
            { nameof(PasswordConfirmationDialog.Title), "Delete Account" },
            { nameof(PasswordConfirmationDialog.ContentText), "This action is irreversible. All your data will be permanently deleted. Please enter your password to confirm." },
            { nameof(PasswordConfirmationDialog.ConfirmButtonText), "Delete Account" }
        };

        var dialog = await DialogService.ShowAsync<PasswordConfirmationDialog>("Delete Account", parameters);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled && result.Data is string password)
        {
            var success = await AuthService.DeleteAccountAsync(password);
            if (success)
            {
                Snackbar.Add("Account deleted successfully.", Severity.Success);
            }
            else
            {
                Snackbar.Add("Failed to delete account. Please check your password.", Severity.Error);
            }
        }
    }
}
