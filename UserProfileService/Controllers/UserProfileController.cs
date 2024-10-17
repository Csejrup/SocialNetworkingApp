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

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetUserProfile(Guid id)
        {
            var user = await userProfileService.GetUserProfileAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPost("{userId:guid}/follow/{userIdToFollow:guid}")]
        public async Task<IActionResult> FollowUser(Guid userId, Guid userIdToFollow)
        {
            await userProfileService.FollowUserAsync(userId, userIdToFollow);
            return Ok("Followed successfully.");
        }

        [HttpPost("{userId:guid}/unfollow/{userIdToUnfollow}")]
        public async Task<IActionResult> UnfollowUser(Guid userId, Guid userIdToUnfollow)
        {
            await userProfileService.UnfollowUserAsync(userId, userIdToUnfollow);
            return Ok("Unfollowed successfully.");
        }

        [HttpGet("{userId:guid}/followers")]
        public async Task<IActionResult> GetFollowers(Guid userId)
        {
            var followers = await userProfileService.GetFollowersAsync(userId);
            return Ok(followers);
        }
    }
}