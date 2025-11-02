using GymInfrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using GymDomain.Model;


namespace GymInfrastructure.Controllers
{
  
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SportsClubDbContext _context;

        public HomeController(ILogger<HomeController> logger, SportsClubDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public async Task<IActionResult> GetTrainingStats()
        {
            var registrations = await _context.TrainingRegistrations
                .Include(r => r.Training)
                .Where(r => r.Training != null &&
                            (r.Status == "Confirmed" || r.Status == "Active"))
                .ToListAsync();

            var stats = registrations
                .AsEnumerable()
                .GroupBy(r => new
                {
                    Day = r.Training.Date.ToDateTime(TimeOnly.MinValue).DayOfWeek,
                    Hour = r.Training.StartTime.Hour
                })
                .Select(g => new
                {
                    day = g.Key.Day.ToString(),
                    hour = g.Key.Hour,
                    count = g.Count()
                })
                .OrderBy(g => g.day)
                .ThenBy(g => g.hour)
                .ToList();

            return Json(stats);
        }


    }
}

