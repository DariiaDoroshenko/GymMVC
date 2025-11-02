using GymDomain.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
using GymInfrastructure.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace GymInfrastructure.Controllers
{

    public class ScheduleController : Controller
    {
        private readonly SportsClubDbContext _context;
        private const int DaysInWeek = 7;

        public ScheduleController(SportsClubDbContext context)
        {
            _context = context;
        }

        // =================== РОЗКЛАД ===================
        public async Task<IActionResult> Index(string? date)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var dates = Enumerable.Range(0, DaysInWeek)
                .Select(i => today.AddDays(i))
                .ToList();

            var selected = !string.IsNullOrEmpty(date) && DateOnly.TryParse(date, out var parsed)
                ? parsed
                : today;

            var trainings = await GetTrainingsByDate(selected);

            var vm = new ScheduleViewModel
            {
                Dates = dates,
                SelectedDate = selected,
                Trainings = trainings
            };

            return View(vm);
        }

        // AJAX-завантаження тренувань за датою
        public async Task<IActionResult> ByDate(string date)
        {
            if (!DateOnly.TryParse(date, out var d))
                return BadRequest("Invalid date");

            var trainings = await GetTrainingsByDate(d);
            return PartialView("_TrainingCards", trainings);
        }

        private async Task<List<Training>> GetTrainingsByDate(DateOnly date)
        {
            return await _context.Trainings
                .Include(t => t.Trainer)
                .Where(t => t.Date == date && (t.IsCanceled == null || t.IsCanceled == false))
                .OrderBy(t => t.StartTime)
                .ToListAsync();
        }

        // =================== СТВОРЕННЯ ===================
        [HttpGet]
        public IActionResult Create(string? date)
        {
            var model = new Training
            {
                Date = !string.IsNullOrEmpty(date) && DateOnly.TryParse(date, out var parsed)
                    ? parsed
                    : DateOnly.FromDateTime(DateTime.Today),
                StartTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(9))
            };

            ViewBag.Trainers = new SelectList(_context.Trainers, "Id", "LastName");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormCollection form)
        {
            // Отримуємо дані з форми як рядки
            var title = form["Title"];
            var trainerId = int.Parse(form["TrainerId"]);
            var date = DateOnly.FromDateTime(DateTime.Parse(form["Date"]));
            var startTime = TimeOnly.FromDateTime(DateTime.Parse(form["StartTime"]));
            var duration = int.TryParse(form["DurationMinutes"], out var d) ? d : 60;
            var price = decimal.TryParse(form["Price"], out var p) ? p : 0m;
            var maxClients = int.TryParse(form["MaxClients"], out var m) ? m : 10;
            var description = form["Description"];

            var training = new Training
            {
                Title = title,
                TrainerId = trainerId,
                Date = date,
                StartTime = startTime,
                DurationMinutes = duration,
                Price = price,
                MaxClients = maxClients,
                Description = description
            };

            _context.Trainings.Add(training);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { date = date.ToString("yyyy-MM-dd") });
        }




        // =================== РЕДАГУВАННЯ ===================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var training = await _context.Trainings.FindAsync(id);
            if (training == null) return NotFound();

            ViewBag.Trainers = new SelectList(_context.Trainers, "Id", "LastName", training.TrainerId);
            return View(training);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IFormCollection form)
        {
            var existing = await _context.Trainings.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.Title = form["Title"];
            existing.TrainerId = int.Parse(form["TrainerId"]);
            existing.Date = DateOnly.FromDateTime(DateTime.Parse(form["Date"]));
            existing.StartTime = TimeOnly.FromDateTime(DateTime.Parse(form["StartTime"]));
            existing.DurationMinutes = int.TryParse(form["DurationMinutes"], out var d) ? d : 60;
            existing.Price = decimal.TryParse(form["Price"], out var p) ? p : 0m;
            existing.MaxClients = int.TryParse(form["MaxClients"], out var m) ? m : 10;
            existing.Description = form["Description"];

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { date = existing.Date.ToString("yyyy-MM-dd") });
        }



        // =================== ВИДАЛЕННЯ ===================
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var training = await _context.Trainings
                .Include(t => t.Trainer)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (training == null) return NotFound();

            return View(training);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var training = await _context.Trainings.FindAsync(id);
            if (training == null) return NotFound();

            var date = training.Date;
            _context.Trainings.Remove(training);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { date = date.ToString("yyyy-MM-dd") });
        }

        // =================== ДЕТАЛІ КОНКРЕТНОГО ЗАНЯТТЯ ===================
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var training = await _context.Trainings
                .Include(t => t.Trainer)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (training == null)
                return NotFound();

            return View(training);
        }

    }
}
