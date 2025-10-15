using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace WorkoutManager.Web.Components
{
    public partial class ActiveSessionDialog
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; } = default!;

        private void Continue() => MudDialog.Close(DialogResult.Ok("continue"));
        private void FinishAndStartNew() => MudDialog.Close(DialogResult.Ok("finish_and_start_new"));
        private void Cancel() => MudDialog.Cancel();
    }
}
