using Microsoft.AspNetCore.Identity;
using MoviesDashboard.ViewModels.Identity;

namespace MoviesDashboard.Interfaces
{
    public interface IAccountService
    {
        Task<IdentityResult> RegisterAsync(RegisterVM model, string origin, CancellationToken cancellationToken = default);

        Task<IdentityResult> LoginAsync(LoginVM model, CancellationToken cancellationToken = default);

        Task<IdentityResult> RegisterConfirmationAsync(string userId, string token, CancellationToken cancellationToken = default);
        Task<IdentityResult> ResendConfirmEmailAsync(string nameOrEmail,string origin, CancellationToken cancellationToken = default);


    }
}
