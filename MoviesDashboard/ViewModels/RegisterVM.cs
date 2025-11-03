using System.ComponentModel.DataAnnotations;

namespace MoviesDashboard.ViewModels
{
    public class RegisterVM
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
        [Required, DataType(DataType.Password), Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }

    }
}
