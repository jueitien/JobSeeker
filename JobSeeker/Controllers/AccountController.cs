using JobSeeker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace JobSeeker.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View(new RegisterModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            RegisterModel model)
        {
            if (!UserRoles.All.Contains(model.Role))
            {
                ModelState.AddModelError(
                    nameof(model.Role),
                    "Please select a valid role.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existingUser =
                await _userManager.FindByEmailAsync(model.Email.Trim());

            if (existingUser != null)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "An account already uses this email.");

                return View(model);
            }

            var user = new ApplicationUser
            {
                FullName = model.FullName.Trim(),
                Email = model.Email.Trim(),
                UserName = model.Email.Trim()
            };

            var creationResult =
                await _userManager.CreateAsync(user, model.Password);

            if (!creationResult.Succeeded)
            {
                foreach (var error in creationResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View(model);
            }

            var roleResult =
                await _userManager.AddToRoleAsync(user, model.Role);

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);

                ModelState.AddModelError(
                    string.Empty,
                    "The selected role could not be assigned.");

                return View(model);
            }

            TempData["SuccessMessage"] =
                "Registration successful. You can now log in.";

            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginModel
            {
                ReturnUrl = returnUrl
            });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user =
                await _userManager.FindByEmailAsync(model.Email.Trim());

            if (user == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid email or password.");

                return View(model);
            }

            var result =
                await _signInManager.PasswordSignInAsync(
                    user,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: true);

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Your account is temporarily locked.");

                return View(model);
            }

            if (!result.Succeeded)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid email or password.");

                return View(model);
            }

            if (!string.IsNullOrWhiteSpace(model.ReturnUrl)
                && Url.IsLocalUrl(model.ReturnUrl))
            {
                return LocalRedirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}