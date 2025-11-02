using GymDomain.Model;
using GymInfrastructure.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymInfrastructure.Controllers
{
  
    public class TrainingsController : Controller
    {
        private readonly SportsClubDbContext _context;

        public TrainingsController(SportsClubDbContext context)
        {
            _context = context;
        }

        // 🔹 Список типів тренувань
        public async Task<IActionResult> Index()
        {
            var trainings = await _context.Trainings
                .GroupBy(t => t.Title)
                .Select(g => g.First()) // унікальні типи
                .ToListAsync();

            return View(trainings);
        }

        // 🔹 Деталі тренування (опис, ціна, складність)
        public async Task<IActionResult> Details(string title)
        {
            if (string.IsNullOrEmpty(title))
                return NotFound();

            var training = await _context.Trainings
                .Where(t => t.Title == title)
                .OrderByDescending(t => t.Date)
                .FirstOrDefaultAsync();

            if (training == null)
                return NotFound();

            return View(training);
        }

        // 🔹 Записатись — показує розклад тільки цього виду тренування
        [HttpGet]
        public async Task<IActionResult> ScheduleByType(string title, string? date)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var dates = Enumerable.Range(0, 7).Select(i => today.AddDays(i)).ToList();

            var selected = !string.IsNullOrEmpty(date) && DateOnly.TryParse(date, out var parsed)
                ? parsed
                : today;

            var trainings = await _context.Trainings
                .Include(t => t.Trainer)
                .Where(t => t.Title == title && t.Date == selected)
                .OrderBy(t => t.StartTime)
                .ToListAsync();

            var vm = new TrainingScheduleViewModel
            {
                TrainingTitle = title,
                Dates = dates,
                SelectedDate = selected,
                Trainings = trainings
            };

            return View(vm);
        }

    }
}

