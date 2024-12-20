using Microsoft.AspNetCore.Mvc;
using InteractionService.Dtos;
using InteractionService.Services;

namespace InteractionService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InteractionController(IInteractionService interactionService) : ControllerBase
    {
        [HttpPost("like")]
        public async Task<IActionResult> LikeTweet([FromBody] LikeDto likeDto)
        {
            if (likeDto == null)
            {
                return BadRequest("Invalid like data.");
            }

            await interactionService.LikeTweetAsync(likeDto);

            return Ok("Tweet liked successfully.");
        }

        [HttpPost("comment")]
        public async Task<IActionResult> CommentOnTweet([FromBody] CommentDto commentDto)
        {
            if (commentDto == null || string.IsNullOrEmpty(commentDto.Content))
            {
                return BadRequest("Comment content cannot be empty.");
            }

            await interactionService.CommentOnTweetAsync(commentDto);

            return Ok("Comment added successfully.");
        }

        [HttpGet("comments/{tweetId:guid}")]
        public async Task<IActionResult> GetCommentsForTweet(Guid tweetId)
        {
            if (tweetId == Guid.Empty)
            {
                return BadRequest("Invalid tweet ID.");
            }

            var comments = await interactionService.GetCommentsForTweetAsync(tweetId);

            if (comments == null || comments.Count == 0)
            {
                return NotFound($"No comments found for tweet with ID {tweetId}.");
            }

            return Ok(comments);
        }
    }
}