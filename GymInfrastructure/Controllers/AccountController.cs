using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using GymDomain.Models;
using GymInfrastructure;
using GymDomain.Model;
using GymInfrastructure.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GymInfrastructure.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SportsClubDbContext _context;

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole> roleManager,
            SportsClubDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
        }

        // ------------------ LOGIN ------------------

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                user = await _userManager.FindByNameAsync(model.Email);

                if (user == null)
                {
                    user = await _userManager.Users
                        .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());
                }
            }

            if (user == null)
            {
                ModelState.AddModelError("", "Користувача не знайдено.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);

            if (result.Succeeded)
            {
                return RedirectToLocal(model.ReturnUrl);
            }

            ModelState.AddModelError("", "Невірний email або пароль.");
            return View(model);
        }


        // ------------------ REGISTER ------------------

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync("user"))
                        await _roleManager.CreateAsync(new IdentityRole("user"));

                    await _userManager.AddToRoleAsync(user, "user");

                    var client = new Client
                    {
                        FirstName = string.IsNullOrEmpty(model.Name) ? "Ім’я" : model.Name,
                        LastName = "—", // 👈 щоб уникнути NULL
                        Email = model.Email,
                        IdentityUserId = user.Id
                    };

                    _context.Clients.Add(client);
                    await _context.SaveChangesAsync();

                    await _signInManager.SignInAsync(user, false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        // ------------------ LOGOUT ------------------

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // ------------------ GOOGLE LOGIN ------------------

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            properties.Items["prompt"] = "select_account"; // змушує обирати акаунт
            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            returnUrl ??= Url.Content("~/");

            if (remoteError != null)
            {
                ModelState.AddModelError("", $"Помилка входу: {remoteError}");
                return RedirectToAction(nameof(Login));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return RedirectToAction(nameof(Login));

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);

            if (result.Succeeded)
                return RedirectToLocal(returnUrl);

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (email == null)
            {
                ModelState.AddModelError("", "Google не надав email.");
                return RedirectToAction(nameof(Login));
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new User { UserName = email, Email = email };
                var createResult = await _userManager.CreateAsync(user);
                if (createResult.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "user");

                    var client = new Client
                    {
                        FirstName = "Ім’я",
                        LastName = "Прізвище",
                        Email = email,
                        IdentityUserId = user.Id
                    };

                    _context.Clients.Add(client);
                    await _context.SaveChangesAsync();

                    await _userManager.AddLoginAsync(user, info);
                }
            }

            await _signInManager.SignInAsync(user, false);
            return RedirectToLocal(returnUrl);
        }

        // ------------------ HELPERS ------------------

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            else
                return RedirectToAction("Index", "Home");
        }
    }
}
