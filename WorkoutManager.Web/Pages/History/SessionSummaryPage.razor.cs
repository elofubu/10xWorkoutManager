using Microsoft.AspNetCore.Components;
using WorkoutManager.BusinessLogic.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkoutManager.Web.Services;

namespace WorkoutManager.Web.Pages.History
{
    public partial class SessionSummaryPage
    {
        [Parameter]
        public int Id { get; set; }

        [Inject]
        private ISessionService SessionService { get; set; } = default!;

        private SessionDetailsDto _session = new();

        protected override async Task OnInitializedAsync()
        {
            _session = await SessionService.GetSessionDetailsAsync(Id);
        }

        private string GetExerciseName(int exerciseId)
        {
            // TODO: Replace with a call to a service that can resolve exercise names
            return $"Exercise {exerciseId}";
        }
    }
}
