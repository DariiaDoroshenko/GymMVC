using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using GymDomain.Model;

namespace GymInfrastructure.Controllers
{
  
    public class TrainingRegistrationController : Controller
    {
        private readonly SportsClubDbContext _context;

        public TrainingRegistrationController(SportsClubDbContext context)
        {
            _context = context;
        }

        // ------------------- ЗАПИСАТИСЬ -------------------
        public async Task<IActionResult> Register(int trainingId)
        {
            // Отримуємо поточного користувача з Identity
            var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Шукаємо відповідного клієнта в таблиці Clients
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.IdentityUserId == identityUserId);
            if (client == null)
            {
                TempData["Error"] = "Профіль клієнта не знайдено.";
                return RedirectToAction("Index", "Home");
            }

            // Перевіряємо, чи вже записаний користувач
            bool alreadyRegistered = await _context.TrainingRegistrations
                .AnyAsync(r => r.ClientId == client.Id && r.TrainingId == trainingId);

            if (alreadyRegistered)
                return RedirectToAction("MyTrainings", new { message = "Ви вже записані на це тренування." });

            var registration = new TrainingRegistration
            {
                ClientId = client.Id,
                TrainingId = trainingId,
                RegistrationDate = DateTime.Now,
                Status = "Active"
            };

            _context.TrainingRegistrations.Add(registration);
            await _context.SaveChangesAsync();

            return RedirectToAction("MyTrainings", "TrainingRegistration");

        }

        // ------------------- МОЇ ТРЕНУВАННЯ -------------------
        public async Task<IActionResult> MyTrainings(string? message = null)
        {
            var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.IdentityUserId == identityUserId);

            if (client == null)
            {
                TempData["Error"] = "Профіль клієнта не знайдено.";
                return RedirectToAction("Index", "Home");
            }

            var registrations = await _context.TrainingRegistrations
                .Include(r => r.Training)
                .ThenInclude(t => t.Trainer)
                .Where(r => r.ClientId == client.Id)
                .ToListAsync();

            var now = DateTime.Now;

            var upcoming = registrations
                .Where(r => r.Training.Date.ToDateTime(r.Training.StartTime) > now && r.Status == "Active")
                .OrderBy(r => r.Training.Date)
                .ToList();

            var archived = registrations
                .Where(r => r.Training.Date.ToDateTime(r.Training.StartTime) <= now || r.Status == "Canceled")
                .OrderByDescending(r => r.Training.Date)
                .ToList();

            ViewBag.Message = message;
            ViewBag.Archived = archived;

            return View(upcoming);
        }

        // ------------------- СКАСУВАТИ ЗАПИС -------------------
        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var registration = await _context.TrainingRegistrations.FindAsync(id);
            if (registration == null) return NotFound();

            registration.Status = "Canceled";
            await _context.SaveChangesAsync();

            return RedirectToAction("MyTrainings", "TrainingRegistration", new { message = "Запис скасовано." });
        }
    }
}
