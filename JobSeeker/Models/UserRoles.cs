namespace JobSeeker.Models
{
    public static class UserRoles
    {
        public const string JobSeeker = "Job Seeker";
        public const string Employer = "Employer";
        public const string CareerCounsellor = "Career Counsellor";
        public const string Administrator = "Administrator";

        public static readonly string[] All =
        [
            JobSeeker,
            Employer,
            CareerCounsellor,
            Administrator
        ];
    }
}