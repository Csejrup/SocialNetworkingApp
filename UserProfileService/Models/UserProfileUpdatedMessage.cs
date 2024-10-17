namespace UserProfileService.Models;

public class UserProfileUpdatedMessage
{
    public Guid UserId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}