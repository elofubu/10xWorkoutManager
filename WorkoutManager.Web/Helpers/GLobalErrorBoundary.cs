using Microsoft.AspNetCore.Components.Web;
using WorkoutManager.Web.Services;

namespace WorkoutManager.Web.Helpers
{
    public class GLobalErrorBoundary : ErrorBoundary
    {
        public GLobalErrorBoundary()
        {
            
        }

        protected override async Task OnErrorAsync(Exception exception)
        {
             await base.OnErrorAsync(exception);
        }
    }
}
