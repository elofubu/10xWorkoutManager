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
        private bool _isLoading = true;
        private bool _isPaginationLoading = false;
        private int _currentPage = 1;

        private int PageCount => _pagination.PageSize > 0 ? (int)Math.Ceiling((double)_pagination.TotalCount / _pagination.PageSize) : 0;

        protected override async Task OnInitializedAsync()
        {
            _isLoading = true;
            try
            {
                var result = await SessionService.GetSessionHistoryAsync();
                _sessions = result.Data;
                _pagination = result.Pagination;
            }
            finally
            {
                _isLoading = false;
            }
        }

        private async Task PageChanged(int page)
        {
            _isPaginationLoading = true;
            _currentPage = page;
            try
            {
                //no paggination implemented yet
                //var result = await SessionService.GetSessionHistoryAsync(page);
                var result = await SessionService.GetSessionHistoryAsync();
                _sessions = result.Data;
                _pagination = result.Pagination;
                StateHasChanged();
            }
            finally
            {
                _isPaginationLoading = false;
            }
        }

        private void NavigateToSession(long sessionId)
        {
            NavigationManager.NavigateTo($"/history/{sessionId}");
        }

        private string HandleDecimalInWeight(decimal weight)
        {
            if((weight % 1) == 0)
            {
                return Math.Round(weight).ToString();
            }

            return weight.ToString();
        }
    }
}
