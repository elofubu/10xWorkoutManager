using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace WorkoutManager.Web.Components
{
    public partial class ConfirmationDialog
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; } = default!;

        [Parameter]
        public string Title { get; set; } = default!;

        [Parameter]
        public string ContentText { get; set; } = default!;

        [Parameter]
        public string ConfirmButtonText { get; set; } = "Confirm";

        private void Confirm() => MudDialog.Close(MudBlazor.DialogResult.Ok(true));
        private void Cancel() => MudDialog.Cancel();
    }
}
