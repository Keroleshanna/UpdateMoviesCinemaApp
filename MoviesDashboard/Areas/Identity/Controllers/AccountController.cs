using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using MoviesDashboard.Data;
using MoviesDashboard.Repositories.IRepositories;
using MoviesDashboard.ViewModels;
using System.Security.Claims;

namespace MoviesDashboard.Areas.Identity.Controllers
{
    [Area("Identity")]
    public class AccountController : Controller
    {
        //private readonly UserManager<AppUser> _userManager;
        //private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IRepository<ApplicationUserOTP> _applicationUserOTPRepository;

        public AccountController(
            UserManager<AppUser> userManager, 
            SignInManager<AppUser> signInManager, 
            IEmailSender emailSender, IRepository<ApplicationUserOTP> applicationUserOTPRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _applicationUserOTPRepository = applicationUserOTPRepository;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var result = await _userManager.CreateAsync(new()
            {
                Name = vm.Name,
                Email = vm.Email,
                UserName = vm.UserName
            }, password: vm.Password);

            if (!result.Succeeded)
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, item.Code);
                }
                return View(vm);
            }
            TempData["Success-notification"] = "Register Successfully";
            return RedirectToAction("Login");
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var user = await _userManager.FindByNameAsync(vm.UserNameOeEmail) ??
                await _userManager.FindByEmailAsync(vm.UserNameOeEmail);

            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid User_Name/Email or Password");
                return View(vm);
            }


            var pass = await _signInManager.PasswordSignInAsync(user, vm.Password, vm.RememberMe, true);

            if (!pass.Succeeded)
            {
                if (pass.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Too Many Attempt Please try again after 5 min");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid User_Name/Email or Password");
                }
                return View(vm);
            }
            TempData["Success-notification"] = "Login Successfully";
            return RedirectToAction(actionName: "Index", controllerName: "Home", new { area = "Admin" });

        }
        // ✅ زرار تسجيل الدخول بجوجل
        [HttpGet]
        public IActionResult GoogleLogin()
        {
            var redirectUrl = Url.Action("GoogleResponse", "Account", new { area = "Identity" });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
            return Challenge(properties, "Google");
        }

        // ✅ بعد ما جوجل ترجع المستخدم هنا
        [HttpGet]
        public async Task<IActionResult> GoogleResponse()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return RedirectToAction("Login");

            // هل المستخدم موجود أصلاً؟
            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (signInResult.Succeeded)
                return RedirectToAction("Index", "Home", new { area = "Admin" });

            // لو أول مرة يسجل، نضيفه كـ AppUser جديد
            var email = info.Principal.FindFirstValue(System.Security.Claims.ClaimTypes.Email);
            var name = info.Principal.FindFirstValue(System.Security.Claims.ClaimTypes.Name);

            var user = new AppUser
            {
                UserName = email,
                Email = email,
                Name = name ?? email ?? string.Empty
            };

            var result = await _userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                await _userManager.AddLoginAsync(user, info);
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }

            TempData["Error-notification"] = "خطأ أثناء التسجيل بجوجل";
            return RedirectToAction("Login");
        }

        public async Task<IActionResult> ConfirmEmail(string id, string token)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
            {
                TempData["error-notification"] = "User Not Found";

                return RedirectToAction("Index", "Home", new { area = "Customer" });
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
                TempData["error-notification"] = "Invalid Or Expired Token";
            else
                TempData["success-notification"] = "Confirm Your Email Successfully";

            return RedirectToAction("Index", "Home", new { area = "Customer" });
        }
        public IActionResult ResendEmailConfirmation()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResendEmailConfirmation(ResendEmailConfirmationVM resendEmailConfirmationVM)
        {
            if (!ModelState.IsValid)
                return View(resendEmailConfirmationVM);

            var user = await _userManager.FindByNameAsync(resendEmailConfirmationVM.UserNameOREmail) ?? await _userManager.FindByEmailAsync(resendEmailConfirmationVM.UserNameOREmail);

            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid User Name / Email");
                return View(resendEmailConfirmationVM);
            }

            if (user.EmailConfirmed)
            {
                ModelState.AddModelError(string.Empty, "Already Confirmed!!!");
                return View(resendEmailConfirmationVM);
            }

            // Send Mail Confirmation
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var link = Url.Action(nameof(ConfirmEmail), "Account", new { area = "Identity", token = token, user.Id }, Request.Scheme);

            await _emailSender.SendEmailAsync(user.Email!
                , "Ecommerce 518 - Resend Confirm Your Email!"
                , $"<h1>Please Confirm Your Email By Clicking <a href='{link}'>Here</a></h1>");

            TempData["success-notification"] = "Send Email Successfully";
            return RedirectToAction("Login");
        }

        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordVM forgetPasswordVM, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return View(forgetPasswordVM);

            var user = await _userManager.FindByNameAsync(forgetPasswordVM.UserNameOREmail) ?? await _userManager.FindByEmailAsync(forgetPasswordVM.UserNameOREmail);

            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid User Name / Email");
                return View(forgetPasswordVM);
            }

            var otp = new Random().Next(1000, 9999).ToString();

            var userOTPs = await _applicationUserOTPRepository.GetAllAsync(e => e.ApplicationUserId == user.Id);

            var totalCount = userOTPs.Count(e => (DateTime.UtcNow - e.CreateAt).TotalHours < 24);

            if (totalCount > 5)
            {
                ModelState.AddModelError(string.Empty, "Pleas Try Again Later. Too Many At temps");
                return View(forgetPasswordVM);
            }
            else
            {
                await _applicationUserOTPRepository.CreateAsync(new()
                {
                    ApplicationUserId = user.Id,
                    CreateAt = DateTime.UtcNow,
                    IsValid = true,
                    Id = Guid.NewGuid().ToString(),
                    OTP = otp,
                    ValidTo = DateTime.UtcNow.AddMinutes(30)
                }, cancellationToken: cancellationToken);
                await _applicationUserOTPRepository.CommitAsync(cancellationToken);

                await _emailSender.SendEmailAsync(user.Email!
                    , "Ecommerce 518 - Forget Password!"
                    , $"<h1>Use this OTP: {otp} To Validate Your Account. Don't share it.</h1>");

                TempData["success-notification"] = "Send OTP Your Email";
            }

            TempData["From-ForgetPassword"] = Guid.NewGuid().ToString();
            return RedirectToAction("ValidateOTP", new { userId = user.Id });
        }

        public IActionResult ValidateOTP(string userId)
        {
            if (TempData["From-ForgetPassword"] is null)
                return NotFound();

            return View(new ValidateOTP()
            {
                UserId = userId
            });
        }

        [HttpPost]
        public async Task<IActionResult> ValidateOTP(ValidateOTP validateOTP)
        {
            if (!ModelState.IsValid)
                return View(validateOTP);


            var validOTP = await _applicationUserOTPRepository.GetOneAsync(e => e.ApplicationUserId == validateOTP.UserId && e.IsValid && e.ValidTo > DateTime.UtcNow);

            if (validOTP is null)
            {
                TempData["error-notification"] = "Invalid OTP";
                return RedirectToAction(nameof(ValidateOTP), new { userId = validateOTP.UserId });
            }

            TempData["From-ValidateOTP"] = Guid.NewGuid().ToString();

            return RedirectToAction("NewPassword", new { userId = validateOTP.UserId });
        }

        public IActionResult NewPassword(string userId)
        {
            if (TempData["From-ValidateOTP"] is null)
                return NotFound();

            return View(new NewPasswordVM()
            {
                UserId = userId
            });
        }

        [HttpPost]
        public async Task<IActionResult> NewPassword(NewPasswordVM newPasswordVM)
        {
            if (!ModelState.IsValid)
                return View(newPasswordVM);

            var user = await _userManager.FindByIdAsync(newPasswordVM.UserId);

            if (user is null)
            {
                TempData["error-notification"] = "User Not Found";
                return RedirectToAction(nameof(NewPassword), new { userId = newPasswordVM.UserId });
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _userManager.ResetPasswordAsync(user, token, newPasswordVM.Password);

            TempData["success-notification"] = "Change Password Successfully";

            return RedirectToAction("Login");
        }


    }
}
