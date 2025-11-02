using System;
using GymDomain.Model;
using GymInfrastructure;

namespace GymInfrastructure.ViewModels
{
    public class ScheduleViewModel
    {
        public List<DateOnly> Dates { get; set; } = new();
        public DateOnly SelectedDate { get; set; }
        public IEnumerable<Training> Trainings { get; set; } = Enumerable.Empty<Training>();
    }
}
