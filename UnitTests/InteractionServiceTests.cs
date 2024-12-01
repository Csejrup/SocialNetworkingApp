using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InteractionService.Dtos;
using InteractionService.Models;
using InteractionService.Repositories;
using Shared.Events;
using Shared.Messaging;

namespace UnitTests;


public class InteractionServiceTests
{
    private readonly Mock<ILikeRepository> _likeRepositoryMock;
    private readonly Mock<ICommentRepository> _commentRepositoryMock;
    private readonly Mock<IMessageClient> _messageClientMock;
    private readonly InteractionService.Services.InteractionService _interactionService;

    public InteractionServiceTests()
    {
        _likeRepositoryMock = new Mock<ILikeRepository>();
        _commentRepositoryMock = new Mock<ICommentRepository>();
        _messageClientMock = new Mock<IMessageClient>();

        _interactionService = new InteractionService.Services.InteractionService(
            _likeRepositoryMock.Object,
            _commentRepositoryMock.Object,
            _messageClientMock.Object
        );
    }

    
    [Fact]
    public async Task LikeTweetAsync_Should_Add_Like_And_Send_TweetLiked_Event()
    {
        // Arrange
        var likeDto = new LikeDto
        {
            UserId = Guid.NewGuid(),
            TweetId = Guid.NewGuid()
        };

        _likeRepositoryMock.Setup(x => x.AddLikeAsync(It.IsAny<Like>())).Returns(Task.CompletedTask);

        // Act
        await _interactionService.LikeTweetAsync(likeDto);

        // Assert
        _likeRepositoryMock.Verify(x => x.AddLikeAsync(It.IsAny<Like>()), Times.Once);
        _messageClientMock.Verify(x => x.Send(It.IsAny<TweetEvent>(), "TweetLiked"), Times.Once);
    }

    
    [Fact]
    public async Task CommentOnTweetAsync_Should_Add_Comment_And_Send_TweetCommented_Event()
    {
        // Arrange
        var commentDto = new CommentDto
        {
            UserId = Guid.NewGuid(),
            TweetId = Guid.NewGuid(),
            Content = "This is a comment"
        };

        _commentRepositoryMock.Setup(x => x.AddCommentAsync(It.IsAny<Comment>())).Returns(Task.CompletedTask);

        // Act
        await _interactionService.CommentOnTweetAsync(commentDto);

        // Assert
        _commentRepositoryMock.Verify(x => x.AddCommentAsync(It.IsAny<Comment>()), Times.Once);
        _messageClientMock.Verify(x => x.Send(It.IsAny<TweetEvent>(), "TweetCommented"), Times.Once);
    }

    [Fact]
    public async Task GetCommentsForTweetAsync_Should_Return_Comments_For_Given_TweetId()
    {
        // Arrange
        var tweetId = Guid.NewGuid();
        var comments = new List<Comment>
        {
            new Comment { UserId = Guid.NewGuid(), TweetId = tweetId, Content = "Comment 1" },
            new Comment { UserId = Guid.NewGuid(), TweetId = tweetId, Content = "Comment 2" }
        };

        _commentRepositoryMock.Setup(x => x.GetCommentsByTweetIdAsync(tweetId))
            .ReturnsAsync(comments);

        // Act
        var result = await _interactionService.GetCommentsForTweetAsync(tweetId);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Comment 1", result[0].Content);
        Assert.Equal("Comment 2", result[1].Content);
    }
}
