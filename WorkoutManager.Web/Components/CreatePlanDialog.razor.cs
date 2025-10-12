using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.Web.Services;

namespace WorkoutManager.Web.Components
{
    public partial class CreatePlanDialog
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; } = default!;

        [Inject]
        private IWorkoutPlanService WorkoutPlanService { get; set; } = default!;

        private string _planName = string.Empty;
        private string _newDayName = string.Empty;
        private List<CreateTrainingDayDto> _trainingDays = new();
        private int _orderCounter = 1;

        private void Submit()
        {
            if (string.IsNullOrWhiteSpace(_planName) || !_trainingDays.Any())
            {
                // TODO: Show validation error to user
                return;
            }

            var newPlan = new CreateWorkoutPlanDto
            {
                Name = _planName,
                TrainingDays = _trainingDays.Select(d => new CreateTrainingDayDto { Name = d.Name, Order = d.Order }).ToList()
            };
            MudDialog.Close(DialogResult.Ok(newPlan));
        }

        void Cancel() => MudDialog.Cancel();

        private void AddTrainingDay()
        {
            if (!string.IsNullOrWhiteSpace(_newDayName))
            {
                _trainingDays.Add(new CreateTrainingDayDto { Name = _newDayName, Order = _orderCounter++ });
                _newDayName = string.Empty;
            }
        }
        
        private void RemoveTrainingDay(CreateTrainingDayDto day)
        {
            _trainingDays.Remove(day);
        }

        private void HandleDayNameKeyup(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                AddTrainingDay();
            }
        }
    }
}
