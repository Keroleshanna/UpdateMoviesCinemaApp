using Microsoft.AspNetCore.Identity;

namespace MoviesDashboard.Models.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }

    }
}
