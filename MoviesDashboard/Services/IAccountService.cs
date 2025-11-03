using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using MoviesDashboard.ViewModels;

namespace MoviesDashboard.Services
{
    public interface IAccountService
    {
        //Existing Methods
        Task<IdentityResult> RegisterUserAsync(RegisterVM model);
        Task<IdentityResult> ConfirmEmailAsync(Guid userId, string token);
        Task<SignInResult> LoginUserAsync(LoginVM model);
        Task LogoutUserAsync();
        Task SendEmailConfirmationAsync(string email);
        //Task<ProfileViewModel> GetUserProfileByEmailAsync(string email);

        //New Methods for External Login
        AuthenticationProperties ConfigureExternalLogin(string provider, string? redirectUrl);
        Task<ExternalLoginInfo?> GetExternalLoginInfoAsync();
        Task<SignInResult> ExternalLoginSignInAsync(string loginProvider, string providerKey, bool isPersistent);
        Task<IdentityResult> CreateExternalUserAsync(ExternalLoginInfo info);
    }
}