using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Monitoring;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Context.Propagation;
using UserProfileService.Dtos;
using UserProfileService.Services;

namespace UserProfileService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserProfileController(IUserProfileService userProfileService) : ControllerBase
    {
        
        
        [HttpPost("database")]
        public async Task<IActionResult> Database()
        {

            var parentContext = ActivityHelper.ExtractPropagationContextFromHttpRequest(Request);
            using var activity = LoggingService.activitySource.StartActivity("Post UserProfile/database", ActivityKind.Consumer, parentContext.ActivityContext);

            LoggingService.Log.AddContext().Information($"Post UserProfile/database endpoint called");

            await userProfileService.CreateDb();
            return Ok();
        }
        
        
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] UserProfileDto userProfile)
        {

            var parentContext = ActivityHelper.ExtractPropagationContextFromHttpRequest(Request);
            using var activity = LoggingService.activitySource.StartActivity("POST UserProfile/register", ActivityKind.Consumer, parentContext.ActivityContext);

            LoggingService.Log.AddContext().Information($"POST UserProfile/register endpoint received request: {JsonSerializer.Serialize(userProfile)}");
            
            await userProfileService.RegisterUserAsync(userProfile);
          
            LoggingService.Log.AddContext().Information("POST UserProfile/register endpoint request finished successfully");

            return Ok("User registered successfully");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserProfile(int id)
        {

            var parentContext = ActivityHelper.ExtractPropagationContextFromHttpRequest(Request);
            using var activity = LoggingService.activitySource.StartActivity("GET UserProfile/id", ActivityKind.Consumer, parentContext.ActivityContext);

            LoggingService.Log.AddContext().Information($"GET UserProfile/id endpoint called with id: {id}");

            var user = await userProfileService.GetUserProfileAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPost("{userId}/follow/{userIdToFollow}")]
        public async Task<IActionResult> FollowUser(int userId, int userIdToFollow)
        {
            var parentContext = ActivityHelper.ExtractPropagationContextFromHttpRequest(Request);
            using var activity = LoggingService.activitySource.StartActivity("GET UserProfile/{userId}/follow/{userIdToFollow}", ActivityKind.Consumer, parentContext.ActivityContext);

            LoggingService.Log.AddContext().Information($"GET UserProfile/userId/follow/userIdToFollow endpoint called with userId: {userId} and userIdToFollow: {userIdToFollow}");

            await userProfileService.FollowUserAsync(userId, userIdToFollow);
            LoggingService.Log.AddContext().Information($"GET UserProfile/userId/follow/userIdToFollow endpoint request finished successfully");

            return Ok("Followed successfully.");
        }

        [HttpPost("{userId}/unfollow/{userIdToUnfollow}")]
        public async Task<IActionResult> UnfollowUser(int userId, int userIdToUnfollow)
        {
            var parentContext = ActivityHelper.ExtractPropagationContextFromHttpRequest(Request);
            using var activity = LoggingService.activitySource.StartActivity("GET UserProfile/{userId}/follow/{userIdToUnfollow}", ActivityKind.Consumer, parentContext.ActivityContext);

            LoggingService.Log.AddContext().Information($"GET UserProfile/userId/follow/userIdToUnfollow endpoint called with userId: {userId} and userIdToFollow: {userIdToUnfollow}");

            await userProfileService.UnfollowUserAsync(userId, userIdToUnfollow);
            
            LoggingService.Log.AddContext().Information($"GET UserProfile/userId/follow/userIdToUnfollow request finished successfully");

            return Ok("Unfollowed successfully.");
        }

        // New endpoint to get followers
        [HttpGet("{userId}/followers")]
        public async Task<IActionResult> GetFollowers(int userId)
        {
            var parentContext = ActivityHelper.ExtractPropagationContextFromHttpRequest(Request);
            using var activity = LoggingService.activitySource.StartActivity("GET UserProfile/{userId}/followers", ActivityKind.Consumer, parentContext.ActivityContext);
            LoggingService.Log.AddContext().Information($"GET UserProfile/userId/followers endpoint called with userId: {userId}");

            var followers = await userProfileService.GetFollowersAsync(userId);
            
            LoggingService.Log.AddContext().Information("GET UserProfile/userId/followers request finished successfully");

            return Ok(followers);
        }
    }
}