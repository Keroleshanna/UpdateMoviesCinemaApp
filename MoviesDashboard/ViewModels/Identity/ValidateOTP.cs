using System.ComponentModel.DataAnnotations;

namespace MoviesDashboard.ViewModels.Identity
{
    public class ValidateOTP
    {
        public int Id { get; set; }

        [Required]
        public string OTP { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }
}
