    using GymDomain.Models;
using GymDomain.Model;
using GymInfrastructure;
    using GymInfrastructure.ViewModels;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;




namespace GymInfrastructure.Controllers
{

    public class TrainersController : Controller
    {
        private readonly SportsClubDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public TrainersController(
            SportsClubDbContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // 🔹 Список усіх тренерів
        public async Task<IActionResult> Index()
        {
            var trainers = await _context.Trainers.ToListAsync();
            return View(trainers);
        }

        // 🔹 Деталі тренера
        public async Task<IActionResult> Details(int id)
        {
            var trainer = await _context.Trainers.FirstOrDefaultAsync(t => t.Id == id);
            if (trainer == null)
                return NotFound();

            return View(trainer);
        }
       
    
            // 🔹 Форма створення
            [HttpGet]
            public IActionResult Create()
            {
                return View();
            }

            // 🔹 Обробка створення тренера + автоматичне створення користувача
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Create(Trainer trainer)
            {
                if (!ModelState.IsValid)
                    return View(trainer);

                // ✅ Перевірити, чи існує користувач з таким email
                var existingUser = await _userManager.FindByEmailAsync(trainer.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("", "Користувач із таким email уже існує.");
                    return View(trainer);
                }

                // ✅ Створюємо нового користувача
                var newUser = new User
                {
                    UserName = trainer.Email,
                    Email = trainer.Email
                };

                // Можна задати дефолтний пароль або згенерувати
                string defaultPassword = "Trainer123!"; // ти можеш потім змінити

                var createResult = await _userManager.CreateAsync(newUser, defaultPassword);

                if (!createResult.Succeeded)
                {
                    foreach (var error in createResult.Errors)
                        ModelState.AddModelError("", error.Description);
                    return View(trainer);
                }

                // ✅ Перевірити, чи є роль "trainer" і створити її, якщо немає
                if (!await _roleManager.RoleExistsAsync("trainer"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("trainer"));
                }

                // ✅ Додати користувачу роль "trainer"
                await _userManager.AddToRoleAsync(newUser, "trainer");

                // ✅ Зберегти тренера з посиланням на користувача
                trainer.IdentityUserId = newUser.Id;
                _context.Trainers.Add(trainer);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Тренера успішно створено. Він може увійти в систему.";
                return RedirectToAction(nameof(Index));
            }
        public async Task<IActionResult> Schedule(int id, DateOnly? date)
        {
            // Знайдемо тренера
            var trainer = await _context.Trainers
                .Include(t => t.Training)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trainer == null)
                return NotFound();

            // Якщо дата не передана — поточна дата
            var selectedDate = date ?? DateOnly.FromDateTime(DateTime.Today);

            // Починаємо з сьогоднішньої дати
            var today = DateOnly.FromDateTime(DateTime.Today);

            // Показуємо 7 днів починаючи з сьогодні
            var dates = Enumerable.Range(0, 7)
                .Select(i => today.AddDays(i))
                .ToList();


            // Тренування цього тренера на вибрану дату
            var trainings = await _context.Trainings
                .Where(t => t.TrainerId == id && t.Date == selectedDate)
                .OrderBy(t => t.StartTime)
                .ToListAsync();

            // ViewModel
            var viewModel = new GymInfrastructure.ViewModels.TrainerScheduleViewModel
            {
                Trainer = trainer,
                Trainings = trainings,
                Dates = dates,
                SelectedDate = selectedDate
            };

            return View(viewModel);
        }
    }
    }
