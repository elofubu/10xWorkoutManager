using Microsoft.AspNetCore.Components;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.Web.Services;

namespace WorkoutManager.Web.Pages.History
{
    public partial class HistoryPage
    {
        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        private ISessionService SessionService { get; set; } = default!;

        private IEnumerable<SessionSummaryDto> _sessions = new List<SessionSummaryDto>();
        private PaginationInfo _pagination = new();
        
        private int PageCount => _pagination.PageSize > 0 ? (int)Math.Ceiling((double)_pagination.TotalCount / _pagination.PageSize) : 0;

        protected override async Task OnInitializedAsync()
        {
            var result = await SessionService.GetSessionHistoryAsync();
            _sessions = result.Data;
            _pagination = result.Pagination;
        }

        private void NavigateToSession(int sessionId)
        {
            NavigationManager.NavigateTo($"/history/{sessionId}");
        }
    }
}
