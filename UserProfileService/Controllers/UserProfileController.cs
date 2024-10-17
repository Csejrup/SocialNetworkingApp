using Microsoft.AspNetCore.Mvc;
using UserProfileService.Dtos;
using UserProfileService.Services;
using System.Threading.Tasks;

namespace UserProfileService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserProfileController(IUserProfileService userProfileService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] UserProfileDto userProfile)
        {
            await userProfileService.RegisterUserAsync(userProfile);
            return Ok("User registered successfully");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserProfile(Guid id)
        {
            var user = await userProfileService.GetUserProfileAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPost("{userId}/follow/{userIdToFollow}")]
        public async Task<IActionResult> FollowUser(Guid userId, Guid userIdToFollow)
        {
            await userProfileService.FollowUserAsync(userId, userIdToFollow);
            return Ok("Followed successfully.");
        }

        [HttpPost("{userId}/unfollow/{userIdToUnfollow}")]
        public async Task<IActionResult> UnfollowUser(Guid userId, Guid userIdToUnfollow)
        {
            await userProfileService.UnfollowUserAsync(userId, userIdToUnfollow);
            return Ok("Unfollowed successfully.");
        }

        // New endpoint to get followers
        [HttpGet("{userId}/followers")]
        public async Task<IActionResult> GetFollowers(Guid userId)
        {
            var followers = await userProfileService.GetFollowersAsync(userId);
            return Ok(followers);
        }
    }
}