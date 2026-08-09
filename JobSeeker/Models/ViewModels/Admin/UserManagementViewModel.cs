namespace JobSeeker.Models.ViewModels.Admin
{
    public class UserManagementViewModel
    {
        public List<UserListItem> Users { get; set; } = new();
        public string? SearchKeyword { get; set; }
        public string? FilterRole { get; set; }
        public string? FilterStatus { get; set; }
        public int TotalCount { get; set; }
    }

    public class UserListItem
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string Role { get; set; } = string.Empty;
        public string AccountStatus { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
