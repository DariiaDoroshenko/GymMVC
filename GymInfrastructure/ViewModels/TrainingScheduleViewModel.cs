namespace GymInfrastructure.ViewModels
{
    using GymDomain.Model;

    public class TrainingScheduleViewModel
    {
        public string TrainingTitle { get; set; } = string.Empty;
        public List<DateOnly> Dates { get; set; } = new();
        public DateOnly SelectedDate { get; set; }
        public List<Training> Trainings { get; set; } = new();
    }
}
