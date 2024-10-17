using TweetPostingService.Models;

namespace TweetPostingService.Repositories;

public interface IUserCacheRepository
{
    Task UpdateUserProfileAsync(UserProfileCache userProfile);
    Task<UserProfileCache?> GetUserProfileAsync(Guid userId);
}
