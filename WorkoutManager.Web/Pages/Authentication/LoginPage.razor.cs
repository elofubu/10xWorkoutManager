using Microsoft.AspNetCore.Components;
using MudBlazor;
using WorkoutManager.Web.Services;

namespace WorkoutManager.Web.Pages.Authentication;

public partial class LoginPage
{
    [Inject]
    private IAuthService AuthService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private MudForm _form = default!;
    private bool _success;
    private LoginModel _model = new();

    private async Task Submit()
    {
        await _form.Validate();
        if (!_success) return;

        var result = await AuthService.LoginAsync(_model.Email, _model.Password);
        if (result)
        {
            Snackbar.Add("Login successful!", Severity.Success);
            NavigationManager.NavigateTo("/");
        }
        else
        {
            Snackbar.Add("Invalid email or password.", Severity.Error);
        }
    }

    private class LoginModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
