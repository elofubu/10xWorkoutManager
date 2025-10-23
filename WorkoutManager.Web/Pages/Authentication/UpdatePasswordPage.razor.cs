using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using WorkoutManager.Web.Services;

namespace WorkoutManager.Web.Pages.Authentication
{
    public partial class UpdatePasswordPage
    {
        private class UpdatePasswordModel
        {
            public string Password { get; set; }
            public string ConfirmPassword { get; set; }
        }

        private readonly UpdatePasswordModel _model = new();
        private MudForm _form;
        private bool _success;
        private bool _isSubmitting;
        private string _message;
        private Severity _messageSeverity = Severity.Info;
        private bool _isPasswordVisible;

        [Inject]
        private IAuthService AuthService { get; set; }

        [Inject]
        private NavigationManager NavigationManager { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationStateTask { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthenticationStateTask;
            if (authState.User?.Identity == null || !authState.User.Identity.IsAuthenticated)
            {
                NavigationManager.NavigateTo("/authentication/login");
            }
        }

        private async Task Submit()
        {
            await _form.Validate();
            if (!_success) return;

            _isSubmitting = true;
            _message = null;

            try
            {
                await AuthService.UpdatePasswordAsync(_model.Password);
                _message = "Your password has been updated successfully.";
                _messageSeverity = Severity.Success;
                await Task.Delay(2000); // Give user time to read the message
                NavigationManager.NavigateTo("/");
            }
            catch (Exception ex)
            {
                _message = $"An error occurred: {ex.Message}";
                _messageSeverity = Severity.Error;
            }
            finally
            {
                _isSubmitting = false;
                StateHasChanged();
            }
        }

        private string ValidateConfirmPassword(string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(confirmPassword))
            {
                return "Password confirmation is required!";
            }
            if (confirmPassword != _model.Password)
            {
                return "Passwords do not match.";
            }
            return null;
        }
    }
}
