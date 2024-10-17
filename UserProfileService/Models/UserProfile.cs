namespace UserProfileService.Models
{
    public class UserProfile
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;

        // List of user IDs this user is following
        public List<Guid> Following { get; set; } = [];

        // List of user IDs who are following this user
        public List<Guid> Followers { get; set; } = [];
    }
}