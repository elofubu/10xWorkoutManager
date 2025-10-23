using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.ComponentModel.DataAnnotations;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.Web.Services;

namespace WorkoutManager.Web.Components
{
    public partial class CreateExerciseDialog
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; } = default!;

        [Inject]
        private IExerciseService ExerciseService { get; set; } = default!;

        [Inject]
        private IMuscleGroupService MuscleGroupService { get; set; } = default!;

        private bool _success;
        private CreateExerciseModel _model = new();
        private List<MuscleGroupDto> _muscleGroups = new();

        protected override async Task OnInitializedAsync()
        {
            var result = await MuscleGroupService.GetMuscleGroupsAsync();
            _muscleGroups = result.ToList();
        }

        private async Task Submit()
        {
            var newExerciseDto = new CreateExerciseDto
            {
                Name = _model.Name,
                MuscleGroupId = _model.MuscleGroupId
            };

            var exercise = await ExerciseService.CreateExerciseAsync(newExerciseDto);
            MudDialog.Close(MudBlazor.DialogResult.Ok(exercise));
        }

        private void Cancel() => MudDialog.Cancel();

        public class CreateExerciseModel
        {
            [Required]
            public string Name { get; set; } = default!;

            [Required]
            public long MuscleGroupId { get; set; }
        }
    }
}
