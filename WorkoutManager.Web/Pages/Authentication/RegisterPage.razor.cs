using Microsoft.AspNetCore.Components;
using MudBlazor;
using WorkoutManager.Web.Services;

namespace WorkoutManager.Web.Pages.Authentication;

public partial class RegisterPage
{
    [Inject]
    private IAuthService AuthService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private MudForm _form = default!;
    private bool _success;
    private RegisterModel _model = new();

    private async Task Submit()
    {
        await _form.Validate();
        if (!_success) return;

        if (_model.Password != _model.ConfirmPassword)
        {
            Snackbar.Add("Passwords do not match.", Severity.Error);
            return;
        }

        var result = await AuthService.RegisterAsync(_model.Email, _model.Password);
        if (result)
        {
            Snackbar.Add("Registration successful! Please check your email to verify your account.", Severity.Success);
            NavigationManager.NavigateTo("/");
        }
        else
        {
            Snackbar.Add("Registration failed. Email may already be in use.", Severity.Error);
        }
    }

    private class RegisterModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
