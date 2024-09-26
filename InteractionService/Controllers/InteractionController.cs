using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using InteractionService.Dtos;
using InteractionService.Services;
using Monitoring;

namespace InteractionService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InteractionController(IInteractionService interactionService) : ControllerBase
    {
        [HttpPost("like")]
        public async Task<IActionResult> LikeTweet([FromBody] LikeDto likeDto)
        {
            var parentContext = ActivityHelper.ExtractPropagationContextFromHttpRequest(Request);
            using var activity = LoggingService.activitySource.StartActivity("POST Interaction/like", ActivityKind.Consumer, parentContext.ActivityContext);

            if (likeDto == null || likeDto.TweetId <= 0 || likeDto.UserId <= 0)
            {
                return BadRequest("Invalid like data.");
            }

            await interactionService.LikeTweetAsync(likeDto);

            return Ok("Tweet liked successfully.");
        }

        [HttpPost("comment")]
        public async Task<IActionResult> CommentOnTweet([FromBody] CommentDto commentDto)
        {
            var parentContext = ActivityHelper.ExtractPropagationContextFromHttpRequest(Request);
            using var activity = LoggingService.activitySource.StartActivity("POST Interaction/comment", ActivityKind.Consumer, parentContext.ActivityContext);
            
            if (commentDto == null || string.IsNullOrEmpty(commentDto.Content))
            {
                return BadRequest("Comment content cannot be empty.");
            }

            await interactionService.CommentOnTweetAsync(commentDto);

            return Ok("Comment added successfully.");
        }

        [HttpGet("comments/{tweetId}")]
        public async Task<IActionResult> GetCommentsForTweet(int tweetId)
        {
            var parentContext = ActivityHelper.ExtractPropagationContextFromHttpRequest(Request);
            using var activity = LoggingService.activitySource.StartActivity("GET Interaction/comments/tweetId", ActivityKind.Consumer, parentContext.ActivityContext);

            if (tweetId <= 0)
                return BadRequest("Invalid tweet ID.");
            

            var comments = await interactionService.GetCommentsForTweetAsync(tweetId);

            if (comments == null || comments.Count == 0)
                return NotFound($"No comments found for tweet with ID {tweetId}.");
            

            return Ok(comments);
        }
    }
}