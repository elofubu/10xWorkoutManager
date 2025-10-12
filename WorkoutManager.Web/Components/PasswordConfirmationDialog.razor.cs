using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace WorkoutManager.Web.Components;

public partial class PasswordConfirmationDialog
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = default!;

    [Parameter]
    public string Title { get; set; } = "Confirm Action";

    [Parameter]
    public string ContentText { get; set; } = string.Empty;

    [Parameter]
    public string ConfirmButtonText { get; set; } = "Confirm";

    private string _password = string.Empty;

    private void Cancel()
    {
        MudDialog.Cancel();
    }

    private void Confirm()
    {
        MudDialog.Close(DialogResult.Ok(_password));
    }
}

