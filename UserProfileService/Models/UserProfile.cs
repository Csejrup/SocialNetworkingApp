namespace UserProfileService.Models
{
    public class UserProfile
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;

        // List of user IDs this user is following
        public List<int> Following { get; set; } = new List<int>();

        // List of user IDs who are following this user
        public List<int> Followers { get; set; } = new List<int>();
    }
}