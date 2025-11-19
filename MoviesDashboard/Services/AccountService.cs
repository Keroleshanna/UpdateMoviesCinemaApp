using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using MoviesDashboard.Interfaces;
using MoviesDashboard.Models.Identity;
using MoviesDashboard.ViewModels.Identity;
using System.Net;
using System.Text.Encodings.Web;

namespace MoviesDashboard.Services
{
    public class AccountService : IAccountService
    {
        /// <You_should_inject_In_DI>
        /// services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        ///  {
        ///      options.SignIn.RequireConfirmedEmail = true;
        ///  })
        ///          .AddEntityFrameworkStores<ApplicationDbContext>()
        ///          .AddDefaultTokenProviders();
        /// </summary>
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _singInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<AccountService> _logger;

        public AccountService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> singInManager, IEmailSender emailSender, ILogger<AccountService> logger)
        {
            _userManager = userManager;
            _singInManager = singInManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task<IdentityResult> RegisterAsync(RegisterVM model, string origin, CancellationToken cancellationToken = default)
        {
            // 1. Business Validation (unique UserName and Email)
            if (await _userManager.FindByEmailAsync(model.Email) != null)
                return IdentityResult.Failed(new IdentityError { Code = "Email Exists", Description = "Email already exists." });

            if (await _userManager.FindByNameAsync(model.UserName) != null)
                return IdentityResult.Failed(new IdentityError { Code = "User Name Exists", Description = "User Name already exists" });

            // 2. Create User entity
            var user = new ApplicationUser
            {
                Name = model.Name,
                UserName = model.UserName,
                Email = model.Email
            };

            // 3. Create New User (this will validate password against Identity password policy)
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return result;

            /// 4. Create new Role (this code must be not here [seeder data])
            ///  var newRole = await _roleManager.CreateAsync(new IdentityRole("User"));
            ///  if (!newRole.Succeeded)
            ///    return IdentityResult.Failed(new IdentityError());

            // 5. Add default role (optional)
            var roleResult = await _userManager.AddToRoleAsync(user, "User"); // Every new user must be a User
            if (!roleResult.Succeeded)
                return roleResult;


            // 5. Send confirmation email (recommended)
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmUrl = $"{origin}/Identity/Account/ConfirmEmail?userId={user.Id}&token={WebUtility.UrlEncode(token)}";
            await _emailSender.SendEmailAsync(user.Email, "Movies - Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(confirmUrl)}'>clicking here</a>.");




            _logger.LogInformation("New user registered: {UserId}", user.Id);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> LoginAsync(LoginVM model, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByNameAsync(model.UserNameOrEmail) ?? await _userManager.FindByEmailAsync(model.UserNameOrEmail);

            if (user == null)
                return IdentityResult.Failed(new IdentityError { Code = "UserNameOrEmailIsNotExists", Description = "This Email or User Name is Not Exists" });

            var result = await _singInManager.PasswordSignInAsync(user, model.Password, true, true);
            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                    return IdentityResult.Failed(new IdentityError { Code = "You are locked for 5 minutes", Description = "You tried to login more than 5 terms" });

                if (result.IsNotAllowed) // you should inject options.SignIn.RequireConfirmedEmail = true; 
                    return IdentityResult.Failed(new IdentityError { Code = "Please confirm your Email First (Check your Email)!!!", Description = "You tried Login to place not allowed for you" });

                if (result.RequiresTwoFactor)
                    return IdentityResult.Failed(new IdentityError { Code = "You should confirm", Description = "Please confirm this email" });


                return IdentityResult.Failed(new IdentityError { Code = "Invalid UserNaem or Email / Password", Description = "Please check for your input!!" });
            }


            return IdentityResult.Success;
        }

        public async Task<IdentityResult> RegisterConfirmationAsync(string userId, string token, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return IdentityResult.Failed(new IdentityError { Code = "You are not user", Description = "There is one change thing!!" });

            return await _userManager.ConfirmEmailAsync(user, token);

        }

        public async Task<IdentityResult> ResendConfirmEmailAsync(string nameOrEmail, string origin, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByNameAsync(nameOrEmail) ?? await _userManager.FindByEmailAsync(nameOrEmail);
            if (user is null || user.Email == null)
                return IdentityResult.Failed(new IdentityError { Code = "Invalid your Email or Name!", Description = "There is no user with this name or Email" });

            if (user.EmailConfirmed)
                return IdentityResult.Failed(new IdentityError {Code ="This Email is already Confirmed!!!!", Description = "Every thing is okay this Email is already Confirmed"});

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmUrl = $"{origin}/Identity/Account/ConfirmEmail?userId={user.Id}&token={WebUtility.UrlEncode(token)}";
            await _emailSender.SendEmailAsync(user.Email, "Movies - Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(confirmUrl)}'>clicking here</a>.");

            return IdentityResult.Success;
        }
    }
}
