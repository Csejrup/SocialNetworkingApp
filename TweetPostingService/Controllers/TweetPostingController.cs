using Microsoft.AspNetCore.Mvc;
using TweetPostingService.Services;
using Shared.Dtos;

namespace TweetPostingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TweetPostingController(ITweetService tweetService) : ControllerBase
    {
        [HttpPost("post")]
        public async Task<IActionResult> PostTweet([FromBody] TweetDto tweetDto)
        {
            if (tweetDto == null || string.IsNullOrEmpty(tweetDto.Content))
            {
                return BadRequest("Tweet content cannot be empty.");
            }

            // Error handling for missing UserId
            if (tweetDto.UserId == Guid.Empty)
            {
                return BadRequest("User ID cannot be empty.");
            }

            await tweetService.PostTweetAsync(tweetDto);
            return Ok("Tweet posted successfully.");
        }

        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetTweetsByUser(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest("Invalid user ID.");
            }

            var tweets = await tweetService.GetTweetsByUserAsync(userId);

            if (tweets == null || tweets.Count == 0)
            {
                return NotFound($"No tweets found for user with ID {userId}.");
            }

            return Ok(tweets);
        }

        [HttpDelete("{tweetId:guid}")]
        public async Task<IActionResult> DeleteTweet(Guid tweetId, [FromQuery] Guid userId)
        {
            if (tweetId == Guid.Empty || userId == Guid.Empty)
            {
                return BadRequest("Invalid tweet ID or user ID.");
            }

            try
            {
                await tweetService.DeleteTweetAsync(tweetId, userId);
                return Ok("Tweet deleted successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); 
            }
        }
    }
}
