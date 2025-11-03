using Microsoft.AspNetCore.Identity;

namespace MoviesDashboard.Data
{
    public class AppUser : IdentityUser
    {
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
    }
}
