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
    private string? _errorMessage;
    private bool _isSubmitting;
    private bool _registrationSuccess = false;

    private async Task Submit()
    {
        await _form.Validate();
        if (!_success)
        {
            return;
        }

        if (_model.Password != _model.ConfirmPassword)
        {
            _errorMessage = "Passwords do not match.";
            return;
        }

        _isSubmitting = true;
        _errorMessage = null;

        try
        {
            await AuthService.RegisterAsync(_model.Email, _model.Password);
            _registrationSuccess = true;
        }
        catch (Exception ex)
        {
            _errorMessage = ex.Message;
        }
        finally
        {
            _isSubmitting = false;
            StateHasChanged();
        }
    }

    private class RegisterModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
