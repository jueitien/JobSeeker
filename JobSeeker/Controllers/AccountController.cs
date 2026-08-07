using JobSeeker.Models;
using JobSeeker.Models.ViewModels;
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

            if (await _userManager.IsInRoleAsync(user, UserRoles.JobSeeker))
                return RedirectToAction("JobSeeker", "Home");

            if (await _userManager.IsInRoleAsync(user, UserRoles.Employer))
                return RedirectToAction("Employer", "Home");

            if (await _userManager.IsInRoleAsync(
                    user, UserRoles.CareerCounsellor))
                return RedirectToAction("CareerCounsellor", "Home");

            if (await _userManager.IsInRoleAsync(
                    user, UserRoles.Administrator))
                return RedirectToAction("Administrator", "Home");

            await _signInManager.SignOutAsync();

            ModelState.AddModelError(
                string.Empty,
                "No valid role is assigned to this account.");

            return View(model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditAccount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            return View(new EditAccountViewModel
            {
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber
            });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAccount(EditAccountViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            var normalizedEmail = model.Email.Trim();
            var existing = await _userManager.FindByEmailAsync(normalizedEmail);
            if (existing != null && existing.Id != user.Id)
            {
                ModelState.AddModelError(nameof(model.Email), "This email address is already in use.");
                return View(model);
            }

            user.FullName = model.FullName.Trim();
            user.Email = normalizedEmail;
            user.UserName = normalizedEmail;
            user.PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber)
                ? null
                : model.PhoneNumber.Trim();

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "Account details updated.";

            return RedirectToAction(nameof(EditAccount));
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