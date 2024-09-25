using Microsoft.EntityFrameworkCore;
using Monitoring;
using UserProfileService.Models;
using UserProfileService.Data;

namespace UserProfileService.Repositories
{
    public class UserProfileRepository(UserProfileDbContext context) : IUserProfileRepository
    {
        public async Task AddUserProfileAsync(UserProfile userProfile)
        {
            using var activity = LoggingService.activitySource.StartActivity();

            await context.UserProfiles.AddAsync(userProfile);
            await context.SaveChangesAsync();
        }

        public async Task<UserProfile?> GetUserProfileByIdAsync(int id)
        {
            return await context.UserProfiles.FindAsync(id);
        }

        public async Task<UserProfile?> GetUserProfileByUsernameAsync(string username)
        {
            using var activity = LoggingService.activitySource.StartActivity();
            return await context.UserProfiles.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task UpdateUserProfileAsync(UserProfile userProfile)
        {
            using var activity = LoggingService.activitySource.StartActivity();
            context.UserProfiles.Update(userProfile);
            await context.SaveChangesAsync();
        }

        public async Task<List<UserProfile>> GetAllUserProfilesAsync()
        {
            using var activity = LoggingService.activitySource.StartActivity();
            return await context.UserProfiles.ToListAsync();
        }

        public async Task FollowUserAsync(int userId, int userIdToFollow)
        {
            using var activity = LoggingService.activitySource.StartActivity();
            var user = await context.UserProfiles.FindAsync(userId);
            var userToFollow = await context.UserProfiles.FindAsync(userIdToFollow);

            if (user == null || userToFollow == null)
                throw new Exception("Either the user or the user to follow does not exist.");

            if (!user.Following.Contains(userIdToFollow))
            {
                user.Following.Add(userIdToFollow);
                userToFollow.Followers.Add(userId);
            }

            await context.SaveChangesAsync();
        }

        public async Task UnfollowUserAsync(int userId, int userIdToUnfollow)
        {
            using var activity = LoggingService.activitySource.StartActivity();
            var user = await context.UserProfiles.FindAsync(userId);
            var userToUnfollow = await context.UserProfiles.FindAsync(userIdToUnfollow);

            if (user == null || userToUnfollow == null)
                throw new Exception("Either the user or the user to unfollow does not exist.");

            if (user.Following.Contains(userIdToUnfollow))
            {
                user.Following.Remove(userIdToUnfollow);
                userToUnfollow.Followers.Remove(userId);
            }

            await context.SaveChangesAsync();
        }

        // New method to get the list of followers
        public async Task<List<int>> GetFollowersAsync(int userId)
        {
            using var activity = LoggingService.activitySource.StartActivity();
            var user = await context.UserProfiles.FindAsync(userId);
            if (user == null) throw new Exception("User not found.");

            return user.Followers;
        }

    }
}
