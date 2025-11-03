using System.ComponentModel.DataAnnotations;

namespace MoviesDashboard.ViewModels
{
    public class LoginVM
    {
        public int Id { get; set; }

        [Required]
        public string UserNameOeEmail { get; set; } = string.Empty;
        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }
}
