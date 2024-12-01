using System.Collections.Concurrent;
using TweetPostingService.Models;

namespace TweetPostingService.Repositories;

public class UserCacheRepository : IUserCacheRepository
{
    private readonly ConcurrentDictionary<Guid, UserProfileCache> _userProfiles = new ();

    // Updates the user profile in the cache (add or update logic)
    public Task UpdateUserProfileAsync(UserProfileCache userProfile)
    {
        _userProfiles[userProfile.UserId] = userProfile;
        return Task.CompletedTask;
    }

    // Gets the user profile from the cache based on userId
    public Task<UserProfileCache?> GetUserProfileAsync(Guid userId)
    {
        _userProfiles.TryGetValue(userId, out var userProfile);
        return Task.FromResult(userProfile);
    }
}