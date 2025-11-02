using GymDomain.Model;

namespace GymInfrastructure.ViewModels
{
    public class TrainerScheduleViewModel
    {
        public Trainer Trainer { get; set; } = null!;
        public List<DateOnly> Dates { get; set; } = new();
        public DateOnly SelectedDate { get; set; }
        public List<Training> Trainings { get; set; } = new();
    }
}
