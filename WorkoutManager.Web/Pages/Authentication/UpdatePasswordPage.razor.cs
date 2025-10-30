using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;
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

        [Inject]
        private ISnackbar Snackbar { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            if (!await AuthService.IsAuthenticatedAsync())
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

                Snackbar.Add("Your password has been updated successfully.", Severity.Success);

                await AuthService.LogoutAsync();
                NavigationManager.NavigateTo("/");
            }
            catch (Supabase.Gotrue.Exceptions.GotrueException ex)
            {
                _messageSeverity = Severity.Error;

                try
                {
                    var detailedError = JsonSerializer.Deserialize<GotureMessageDetails>(ex.Message);

                    _message = detailedError.Message;
                }
                catch (Exception innerEx)
                {
                    _message = $"An error occurred: {ex.Message}";
                }
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

        private class GotureMessageDetails
        {
            [JsonPropertyName("code")]
            public int Code { get; set; }
            [JsonPropertyName("error_code")]
            public string ErrorCode { get; set; }
            [JsonPropertyName("msg")]
            public string Message { get; set; }
        }
    }
}
