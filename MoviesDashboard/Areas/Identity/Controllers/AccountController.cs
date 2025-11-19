using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MoviesDashboard.Interfaces;
using MoviesDashboard.ViewModels.Identity;
using System.Reflection;
using System.Threading.Tasks;

namespace MoviesDashboard.Areas.Identity.Controllers
{
    [Area("Identity")]
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet]
        public IActionResult Register() => View(new RegisterVM());
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return View(model);

            var origin = $"{Request.Scheme}://{Request.Host}";
            var result = await _accountService.RegisterAsync(model, origin, cancellationToken);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Code);

                return View(model);
            }

            return RedirectToAction(nameof(Login));
        }

        public IActionResult Login() => View();
        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _accountService.LoginAsync(model, cancellationToken);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Code);
                }
                return View(model);
            }

            return RedirectToAction(controllerName: "Home", actionName: "Index", routeValues: new { area = "Customer" });
        }


        public async Task<IActionResult> ConfirmEmail(string userId, string token, CancellationToken cancellationToken)
        {
            var confirmResult = await _accountService.RegisterConfirmationAsync(userId, token, cancellationToken);
            if (!confirmResult.Succeeded)
            {
                foreach (var error in confirmResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Code);
                return NotFound();
            }
            return RedirectToAction(nameof(Login));
        }


        public IActionResult ResendConfirmEmail() => View();
        [HttpPost]
        public async Task<IActionResult> ResendConfirmEmail(ResendEmailConfirmationVM model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return View(model);

            var origin = $"{Request.Scheme}://{Request.Host}";

            var reselt = await _accountService.ResendConfirmEmailAsync(model.UserNameOREmail, origin, cancellationToken);
            if (!reselt.Succeeded)
            {
                foreach (var error in reselt.Errors)
                    ModelState.AddModelError(string.Empty, error.Code);
                return View(model);
            }

            TempData["Success"] = "We send to you a new confirm Message";
            return RedirectToAction(nameof(Login));
        }
    }

}
