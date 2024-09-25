using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Monitoring;
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
            var parentContext = LoggingService.ExtractPropagationContextFromHttpRequest(Request);
            using var activity = LoggingService.activitySource.StartActivity("POST Tweet/post", ActivityKind.Consumer, parentContext.ActivityContext);

            LoggingService.Log.AddContext().Information($"POST Tweet/post endpoint received request: {JsonSerializer.Serialize(tweetDto)}");

            
            if (tweetDto == null || string.IsNullOrEmpty(tweetDto.Content))
            {
                LoggingService.Log.Warning("Tweet content cannot be empty.");
                return BadRequest("Tweet content cannot be empty.");
            }

            await tweetService.PostTweetAsync(tweetDto);

            return Ok("Tweet posted successfully.");
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetTweetsByUser(int userId)
        {
            
            LoggingService.Log.AddContext().Information($"GET Tweet/user/userid endpoint received request: {JsonSerializer.Serialize(userId)}");

            if (userId <= 0)
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
        public async Task<IActionResult> DeleteTweet(int tweetId, int userId)
        {
            
            LoggingService.Log.AddContext().Information($"DELETE Tweet/id endpoint received request: {JsonSerializer.Serialize(tweetId)}");

            await tweetService.DeleteTweetAsync(tweetId, userId);
            return Ok("Tweet deleted successfully.");
        }
    }
}