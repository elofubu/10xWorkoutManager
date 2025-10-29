using Microsoft.AspNetCore.Components;
using MudBlazor;
using WorkoutManager.Web.Services;

namespace WorkoutManager.Web.Pages.Authentication
{
    public partial class ResetPasswordPage
    {
        private class ResetPasswordModel
        {
            public string Email { get; set; }
        }

        private readonly ResetPasswordModel _model = new();
        private MudForm _form;
        private bool _success;
        private bool _isSubmitting;
        private string _message;
        private Severity _messageSeverity = Severity.Info;

        [Inject]
        private IAuthService AuthService { get; set; }

        private async Task Submit()
        {
            await _form.Validate();
            if (!_success) return;

            _isSubmitting = true;
            _message = null;

            try
            {
                await AuthService.ResetPasswordAsync(_model.Email);
                _message = "If an account with this email exists, a password reset link has been sent. Please check your inbox.";
                _messageSeverity = Severity.Success;
            }
            catch (Exception)
            {
                _message = "If an account with this email exists, a password reset link has been sent. Please check your inbox.";
                _messageSeverity = Severity.Success;
            }
            finally
            {
                _isSubmitting = false;
                StateHasChanged();
            }
        }
    }
}
