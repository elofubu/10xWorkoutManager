using MudBlazor;

namespace WorkoutManager.Web.Helpers
{
    // The app is dark-only; this maps MudBlazor's palette to the tokens in wwwroot/css/app.css so
    // icon buttons, menus and dialogs stop rendering with the default light (black-on-dark) colors.
    public static class AppTheme
    {
        public static readonly MudTheme Theme = new()
        {
            PaletteDark = new PaletteDark
            {
                Primary = "#84cc16",
                PrimaryContrastText = "#0f172a",
                Secondary = "#94a3b8",
                SecondaryContrastText = "#0f172a",
                Background = "#020617",
                Surface = "#0f172a",
                AppbarBackground = "#0f172a",
                AppbarText = "#84cc16",
                DrawerBackground = "#0f172a",
                TextPrimary = "#f1f5f9",
                TextSecondary = "#94a3b8",
                ActionDefault = "#94a3b8",
                LinesDefault = "#334155",
                LinesInputs = "#334155",
                Divider = "#1e293b",
                OverlayLight = "rgba(2,6,23,.6)"
            }
        };
    }
}
