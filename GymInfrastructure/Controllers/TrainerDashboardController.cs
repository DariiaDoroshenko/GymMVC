using GymInfrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GymInfrastructure.Controllers
{
   
    public class TrainerDashboardController : Controller
    {
        private readonly SportsClubDbContext _context;

        public TrainerDashboardController(SportsClubDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Отримуємо Identity ID тренера
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Знаходимо тренера у базі
            var trainer = await _context.Trainers
                .FirstOrDefaultAsync(t => t.IdentityUserId == userId);

            if (trainer == null)
                return Unauthorized();

            // Витягуємо всі тренування цього тренера
            var trainings = await _context.Trainings
                .Include(t => t.TrainingRegistrations)
                    .ThenInclude(r => r.Client)
                .Where(t => t.TrainerId == trainer.Id)
                .OrderByDescending(t => t.Date)
                .ThenBy(t => t.StartTime)
                .ToListAsync();

            return View(trainings);
        }
    }
}
