namespace MoviesDashboard.Data
{
    public class ApplicationUserOTP
    {
        public string Id { get; set; }

        public string ApplicationUserId { get; set; }
        public AppUser ApplicationUser { get; set; }

        public string OTP { get; set; }
        public DateTime CreateAt { get; set; }
        public bool IsValid { get; set; }
        public DateTime ValidTo { get; set; }
    }
}
