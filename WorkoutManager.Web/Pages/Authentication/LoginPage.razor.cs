using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using WorkoutManager.Web.Services;

namespace WorkoutManager.Web.Pages.Authentication;

public partial class LoginPage
{
    private class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    private readonly LoginModel _model = new();
    private MudForm _form;
    private bool _success;
    private bool _isSubmitting;
    private string _errorMessage;

    [Inject]
    private IAuthService AuthService { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; }

    [Inject]
    private AuthenticationStateProvider AuthStateProvider { get; set; }

    private async Task Submit()
    {
        await _form.Validate();
        if (!_success) { _errorMessage = "Wrong user name or bad password"; return; }

        _isSubmitting = true;
        _errorMessage = null;

        try
        {
            var loggedIn = await AuthService.LoginAsync(_model.Email, _model.Password);
            if (loggedIn)
            {
                ((SupabaseAuthenticationStateProvider)AuthStateProvider).NotifyUserAuthenticationStateChanged();
                NavigationManager.NavigateTo("/");
            }
            else
            {
                _errorMessage = "Wrong user name or bad password";
            }
        }
        catch (Exception ex)
        {
            _errorMessage = "Wrong user name or bad password";
        }
        finally
        {
            _isSubmitting = false;
            StateHasChanged();
        }
    }

    private async Task OnKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            // Trigger the filter or search logic here
            await Submit();
        }
    }
}
