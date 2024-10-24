using Moq;
using Shared.Dtos;
using Shared.Events;
using Shared.Messaging;
using TweetPostingService.Models;
using TweetPostingService.Repositories;
using TweetPostingService.Services;

namespace UnitTests;

public class TweetServiceTests
{
    private readonly Mock<ITweetRepository> _tweetRepositoryMock;
    private readonly Mock<IMessageClient> _messageClientMock;
    private readonly Mock<IUserCacheRepository> _userCacheRepositoryMock;
    private readonly TweetService _tweetService;

    public TweetServiceTests()
    {
        _tweetRepositoryMock = new Mock<ITweetRepository>();
        _messageClientMock = new Mock<IMessageClient>();
        _userCacheRepositoryMock = new Mock<IUserCacheRepository>();
        _tweetService = new TweetService(
            _tweetRepositoryMock.Object, 
            _messageClientMock.Object, 
            _userCacheRepositoryMock.Object
        );
    }

    [Fact]
    public async Task PostTweetAsync_ShouldPostTweetAndPublishEvent()
    {
        // Arrange
        var tweetDto = new TweetDto
        {
            UserId = Guid.NewGuid(),
            Content = "Test tweet"
        };

        _userCacheRepositoryMock
            .Setup(repo => repo.GetUserProfileAsync(tweetDto.UserId))
            .ReturnsAsync(new UserProfileCache { UserId = tweetDto.UserId });

        _tweetRepositoryMock
            .Setup(repo => repo.AddTweetAsync(It.IsAny<Tweet>()))
            .Returns(Task.CompletedTask);

        // Act
        await _tweetService.PostTweetAsync(tweetDto);

        // Assert
        _tweetRepositoryMock.Verify(repo => repo.AddTweetAsync(It.Is<Tweet>(t => 
            t.UserId == tweetDto.UserId && t.Content == tweetDto.Content)), Times.Once);

        _messageClientMock.Verify(client => client.Send(It.IsAny<TweetEvent>(), "TweetPosted"), Times.Once);
    }

    [Fact]
    public async Task PostTweetAsync_ShouldThrowException_WhenUserProfileNotFound()
    {
        // Arrange
        var tweetDto = new TweetDto
        {
            UserId = Guid.NewGuid(),
            Content = "Test tweet"
        };

        _userCacheRepositoryMock
            .Setup(repo => repo.GetUserProfileAsync(tweetDto.UserId))
            .ReturnsAsync((UserProfileCache)null);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _tweetService.PostTweetAsync(tweetDto));
    }

    [Fact]
    public async Task GetTweetsByUserAsync_ShouldReturnTweets()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tweets = new List<Tweet>
        {
            new Tweet { Id = Guid.NewGuid(), UserId = userId, Content = "Test tweet", CreatedAt = DateTime.UtcNow }
        };

        _tweetRepositoryMock
            .Setup(repo => repo.GetTweetsByUserIdAsync(userId))
            .ReturnsAsync(tweets);

        // Act
        var result = await _tweetService.GetTweetsByUserAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Test tweet", result[0].Content);
    }

    [Fact]
    public async Task DeleteTweetAsync_ShouldDeleteTweetAndPublishEvent()
    {
        // Arrange
        var tweetId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var tweet = new Tweet { Id = tweetId, UserId = userId, Content = "Test tweet" };

        _tweetRepositoryMock
            .Setup(repo => repo.GetTweetByIdAsync(tweetId))
            .ReturnsAsync(tweet);

        _tweetRepositoryMock
            .Setup(repo => repo.DeleteTweetAsync(It.IsAny<Tweet>()))
            .Returns(Task.CompletedTask);

        // Act
        await _tweetService.DeleteTweetAsync(tweetId, userId);

        // Assert
        _tweetRepositoryMock.Verify(repo => repo.DeleteTweetAsync(It.Is<Tweet>(t => 
            t.Id == tweetId && t.UserId == userId)), Times.Once);

        _messageClientMock.Verify(client => client.Send(It.IsAny<TweetEvent>(), "TweetDeleted"), Times.Once);
    }

    [Fact]
    public async Task DeleteTweetAsync_ShouldThrowException_WhenTweetNotFoundOrUserIdMismatch()
    {
        // Arrange
        var tweetId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _tweetRepositoryMock
            .Setup(repo => repo.GetTweetByIdAsync(tweetId))
            .ReturnsAsync((Tweet)null);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _tweetService.DeleteTweetAsync(tweetId, userId));
    }
    
   
}
