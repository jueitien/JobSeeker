using JobSeeker.Models;
using JobSeeker.Models.ViewModels;
using JobSeeker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace JobSeeker.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly S3StorageService _s3Storage;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            S3StorageService s3Storage,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _s3Storage = s3Storage;
            _logger = logger;
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
            if (!UserRoles.Registerable.Contains(model.Role))
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

            // Check account status BEFORE attempting sign-in.
            // This way we never call SignOutAsync mid-request (which would
            // invalidate the antiforgery cookie and cause HTTP 400).
            if (user.AccountStatus == "SUSPENDED")
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Your account has been suspended. Please contact support.");
                return View(model);
            }

            if (user.AccountStatus == "DEACTIVATED")
            {
                ModelState.AddModelError(
                    string.Empty,
                    "This account has been deactivated.");
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

            ViewBag.HasProfileImage = !string.IsNullOrWhiteSpace(user.ProfileImageS3Key);

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
        public async Task<IActionResult> EditAccount(EditAccountViewModel model, IFormFile? profileImage)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            ViewBag.HasProfileImage = !string.IsNullOrWhiteSpace(user.ProfileImageS3Key);

            if (!ModelState.IsValid)
                return View(model);

            var normalizedEmail = model.Email.Trim();
            var existing = await _userManager.FindByEmailAsync(normalizedEmail);
            if (existing != null && existing.Id != user.Id)
            {
                ModelState.AddModelError(nameof(model.Email), "This email address is already in use.");
                return View(model);
            }

            string? newlyUploadedProfileImageKey = null;
            var oldProfileImageKey = user.ProfileImageS3Key;

            if (profileImage != null && profileImage.Length > 0)
            {
                const long maxProfileImageBytes = 5 * 1024 * 1024;
                var allowedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(profileImage.FileName).ToLowerInvariant();

                if (!allowedImageExtensions.Contains(extension))
                {
                    ModelState.AddModelError("profileImage", "Profile image must be JPG, JPEG, PNG, or WEBP.");
                    return View(model);
                }

                if (profileImage.Length > maxProfileImageBytes)
                {
                    ModelState.AddModelError("profileImage", "Profile image must be 5 MB or smaller.");
                    return View(model);
                }

                try
                {
                    newlyUploadedProfileImageKey = await _s3Storage.UploadAsync(
                        profileImage,
                        "profile-images",
                        user.Id);

                    user.ProfileImageS3Key = newlyUploadedProfileImageKey;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to upload profile image to Amazon S3 for user {UserId}.", user.Id);
                    ModelState.AddModelError("profileImage", "Profile image upload failed. Please try again.");
                    return View(model);
                }
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
                if (!string.IsNullOrWhiteSpace(newlyUploadedProfileImageKey))
                {
                    try
                    {
                        await _s3Storage.DeleteAsync(newlyUploadedProfileImageKey);
                    }
                    catch (Exception cleanupEx)
                    {
                        _logger.LogWarning(cleanupEx, "Could not clean up newly uploaded profile image {Key}.", newlyUploadedProfileImageKey);
                    }

                    user.ProfileImageS3Key = oldProfileImageKey;
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                ViewBag.HasProfileImage = !string.IsNullOrWhiteSpace(user.ProfileImageS3Key);
                return View(model);
            }

            if (!string.IsNullOrWhiteSpace(newlyUploadedProfileImageKey)
                && !string.IsNullOrWhiteSpace(oldProfileImageKey)
                && !string.Equals(newlyUploadedProfileImageKey, oldProfileImageKey, StringComparison.Ordinal))
            {
                try
                {
                    await _s3Storage.DeleteAsync(oldProfileImageKey);
                }
                catch (Exception cleanupEx)
                {
                    _logger.LogWarning(cleanupEx, "Could not delete previous profile image {Key}.", oldProfileImageKey);
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "Account details updated.";

            return RedirectToAction(nameof(EditAccount));
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ProfileImage()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || string.IsNullOrWhiteSpace(user.ProfileImageS3Key))
                return NotFound();

            try
            {
                var presignedUrl = await _s3Storage.GetPresignedUrlAsync(
                    user.ProfileImageS3Key, TimeSpan.FromMinutes(30));
                return Redirect(presignedUrl);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not create presigned profile image URL for {Key}.", user.ProfileImageS3Key);
                return NotFound();
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveProfileImage()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            var key = user.ProfileImageS3Key;
            if (string.IsNullOrWhiteSpace(key))
                return RedirectToAction(nameof(EditAccount));

            user.ProfileImageS3Key = null;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = "The profile image record could not be updated.";
                return RedirectToAction(nameof(EditAccount));
            }

            try
            {
                await _s3Storage.DeleteAsync(key);
            }
            catch (Exception ex)
            {
                // The account no longer references the object, so a failed cleanup
                // should not stop the user from removing their profile image.
                _logger.LogWarning(ex, "Could not clean up old profile image {Key} from S3.", key);
            }

            TempData["SuccessMessage"] = "Profile image removed.";
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