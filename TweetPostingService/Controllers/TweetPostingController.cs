using Microsoft.AspNetCore.Mvc;
using TweetPostingService.Dtos;
using TweetPostingService.Services;

namespace TweetPostingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TweetController(ITweetService tweetService) : ControllerBase
    {
        [HttpPost("post")]
        public async Task<IActionResult> PostTweet([FromBody] TweetDto tweetDto)
        {
            if (tweetDto == null || string.IsNullOrEmpty(tweetDto.Content))
            {
                return BadRequest("Tweet content cannot be empty.");
            }

            await tweetService.PostTweetAsync(tweetDto);

            return Ok("Tweet posted successfully.");
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetTweetsByUser(Guid userId)
        {
            if (userId == null)
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
        [HttpDelete("{tweetId}")]
        public async Task<IActionResult> DeleteTweet(Guid tweetId, Guid userId)
        {
            await tweetService.DeleteTweetAsync(tweetId, userId);
            return Ok("Tweet deleted successfully.");
        }
    }
}