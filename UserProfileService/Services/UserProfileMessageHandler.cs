using Shared.Messaging;
using UserProfileService.Models;

namespace UserProfileService.Services;

public class UserProfileMessageHandler : IMessageHandler<UserProfile>
{
    public void Handle(UserProfile message)
    {
        throw new NotImplementedException();
    }
}