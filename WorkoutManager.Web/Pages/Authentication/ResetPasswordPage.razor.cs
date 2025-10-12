using Microsoft.AspNetCore.Components;
using MudBlazor;
using WorkoutManager.Web.Services;

namespace WorkoutManager.Web.Pages.Authentication;

public partial class ResetPasswordPage
{
    [Inject]
    private IAuthService AuthService { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private MudForm _form = default!;
    private bool _success;
    private ResetPasswordModel _model = new();

    private async Task Submit()
    {
        await _form.Validate();
        if (!_success) return;

        var result = await AuthService.ResetPasswordRequestAsync(_model.Email);
        if (result)
        {
            Snackbar.Add("Password reset email sent! Please check your inbox.", Severity.Success);
        }
        else
        {
            Snackbar.Add("Failed to send reset email. Please try again.", Severity.Error);
        }
    }

    private class ResetPasswordModel
    {
        public string Email { get; set; } = string.Empty;
    }
}
