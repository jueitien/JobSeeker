namespace JobSeeker.Models
{
    public static class UserRoles
    {
        public const string JobSeeker = "Job Seeker";
        public const string Employer = "Employer";
        public const string CareerCounsellor = "Career Counsellor";
        public const string Administrator = "Administrator";

        // Roles available for public self-registration
        public static readonly string[] Registerable =
        [
            JobSeeker,
            Employer,
            CareerCounsellor
        ];

        // All roles including Administrator (used for seeding only)
        public static readonly string[] All =
        [
            JobSeeker,
            Employer,
            CareerCounsellor,
            Administrator
        ];
    }
}