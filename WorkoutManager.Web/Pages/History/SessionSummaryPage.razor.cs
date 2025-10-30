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
        public long Id { get; set; }

        [Inject]
        private ISessionService SessionService { get; set; } = default!;

        private SessionDetailsDto? _session = null;
        private bool _isLoading = true;

        protected override async Task OnInitializedAsync()
        {
            _isLoading = true;
            try
            {
                _session = await SessionService.GetSessionDetailsAsync(Id);
            }
            finally
            {
                _isLoading = false;
            }
        }

        private string GetExerciseName(long exerciseId)
        {
            // TODO: Replace with a call to a service that can resolve exercise names
            return $"Exercise {exerciseId}";
        }
    }
}
