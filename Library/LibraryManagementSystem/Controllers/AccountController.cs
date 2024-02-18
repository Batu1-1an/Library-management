using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using LibraryManagementSystem.Models;
using System.Security.Claims;

namespace LibraryManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole<int>> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Check if this is the first user (make them admin)
                    if (!await _roleManager.RoleExistsAsync("Admin"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole<int>("Admin"));
                        await _userManager.AddToRoleAsync(user, "Admin");
                    }

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.GivenName, user.FirstName),
                        new Claim(ClaimTypes.Surname, user.LastName),
                        new Claim(ClaimTypes.Name, user.Email),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, await _userManager.IsInRoleAsync(user, "Admin") ? "Admin" : "User")
                    };

                    await _signInManager.SignInWithClaimsAsync(user, isPersistent: false, claims);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(
                        model.Email,
                        model.Password,
                        model.RememberMe,
                        lockoutOnFailure: false);

                    if (result.Succeeded)
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.GivenName, user.FirstName),
                            new Claim(ClaimTypes.Surname, user.LastName),
                            new Claim(ClaimTypes.Name, user.Email),
                            new Claim(ClaimTypes.Email, user.Email),
                            new Claim(ClaimTypes.Role, await _userManager.IsInRoleAsync(user, "Admin") ? "Admin" : "User")
                        };

                        await _signInManager.SignInWithClaimsAsync(user, model.RememberMe, claims);

                        if (!string.IsNullOrEmpty(returnUrl))
                        {
                            return LocalRedirect(returnUrl);
                        }
                        return RedirectToAction("Index", "Home");
                    }
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var model = new UpdateProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(UpdateProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.UserName = model.Email;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                // Update claims
                var existingClaims = await _userManager.GetClaimsAsync(user);
                await _userManager.RemoveClaimsAsync(user, existingClaims);
                
                var newClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.GivenName, user.FirstName),
                    new Claim(ClaimTypes.Surname, user.LastName),
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, await _userManager.IsInRoleAsync(user, "Admin") ? "Admin" : "User")
                };
                
                await _userManager.AddClaimsAsync(user, newClaims);
                await _signInManager.RefreshSignInAsync(user);

                if (!string.IsNullOrEmpty(model.NewPassword))
                {
                    if (await _userManager.CheckPasswordAsync(user, model.CurrentPassword))
                    {
                        var changePasswordResult = await _userManager.ChangePasswordAsync(
                            user,
                            model.CurrentPassword,
                            model.NewPassword);

                        if (!changePasswordResult.Succeeded)
                        {
                            foreach (var error in changePasswordResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            return View(model);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                        return View(model);
                    }
                }

                TempData["StatusMessage"] = "Your profile has been updated";
                return RedirectToAction(nameof(Profile));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
} 