using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using MoviesDashboard.Data;
using MoviesDashboard.ViewModels;
using System.Security.Claims;

namespace MoviesDashboard.Services
{
    public class AccountService : IAccountService
    {

        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;

        public AccountService(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public AuthenticationProperties ConfigureExternalLogin(string provider, string? redirectUrl)
        {
            // The ConfigureExternalAuthenticationProperties method sets up parameters needed for the external provider,
            // such as the login provider name (e.g., Google, Facebook) and the redirect URL to be used after login.
            return _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        }

        public async Task<ExternalLoginInfo?> GetExternalLoginInfoAsync()
        {
            // Retrieve login information about the user from the external login provider (e.g., Google, Facebook).
            // This includes details like the provider's name and the user's identifier within that provider.
            return await _signInManager.GetExternalLoginInfoAsync();
        }

        public async Task<SignInResult> ExternalLoginSignInAsync(string loginProvider, string providerKey, bool isPersistent)
        {
            // Attempt to sign in the user using their external login details.
            // If a corresponding record exists in the UserLogins table, the user will be logged in.
            return await _signInManager.ExternalLoginSignInAsync(
                loginProvider,    // The name of the external login provider (e.g., Google, Facebook).
                providerKey,      // The unique identifier of the user within the external provider.
                isPersistent: isPersistent,   // Indicates whether the login session should persist across browser restarts.
                bypassTwoFactor: true  // Bypass two-factor authentication if enabled.
            );
        }

        public async Task<IdentityResult> CreateExternalUserAsync(ExternalLoginInfo info)
        {
            // Extract email claim (mandatory for identifying the user)
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(email))
                return IdentityResult.Failed(new IdentityError { Description = "Email not received from external provider." });

            // Check if user with this email already exists
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                // If user already exists, link this external login to the existing account
                var loginResult = await _userManager.AddLoginAsync(existingUser, info);

                if (loginResult.Succeeded)
                {
                    // Update last login time
                    //existingUser.LastLogin = DateTime.UtcNow;
                    await _userManager.UpdateAsync(existingUser);

                    // Sign in the existing user
                    await _signInManager.SignInAsync(existingUser, isPersistent: false);
                }

                return loginResult;
            }

            // Extract optional claims
            var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty;
            var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname);

            // Create new ApplicationUser instance
            var user = new AppUser
            {
                UserName = email,                     // Use email as username (unique in system)
                Email = email,                             // Primary email from Google
                //FirstName = firstName,              // From Google claim (or blank if missing)
                //LastName = lastName,               // From Google claim (nullable)
                EmailConfirmed = true,              // External providers already confirm email
                //IsActive = true,
                //CreatedOn = DateTime.UtcNow,
                //LastLogin = DateTime.UtcNow
            };

            // Create the user in Identity DB (Users table)
            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
                return result;

            // Link external login info (UserLogins table)
            result = await _userManager.AddLoginAsync(user, info);

            // Sign in immediately if successful
            if (result.Succeeded)
                await _signInManager.SignInAsync(user, isPersistent: false);

            return result;
        }

        public Task<IdentityResult> RegisterUserAsync(RegisterVM model)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> ConfirmEmailAsync(Guid userId, string token)
        {
            throw new NotImplementedException();
        }

        public Task<SignInResult> LoginUserAsync(LoginVM model)
        {
            throw new NotImplementedException();
        }

        public Task LogoutUserAsync()
        {
            throw new NotImplementedException();
        }

        public Task SendEmailConfirmationAsync(string email)
        {
            throw new NotImplementedException();
        }
    }
}
