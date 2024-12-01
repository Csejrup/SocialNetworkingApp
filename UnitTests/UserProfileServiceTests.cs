using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shared.Dtos;
using Shared.Messaging;
using UserProfileService.Dtos;
using UserProfileService.Models;
using UserProfileService.Repositories;

namespace UnitTests;

public class UserProfileServiceTests
{
    private readonly Mock<IUserProfileRepository> _userProfileRepositoryMock;
    private readonly Mock<IMessageClient> _messageClientMock;
    private readonly UserProfileService.Services.UserProfileService _userProfileService;

    public UserProfileServiceTests()
    {
        _userProfileRepositoryMock = new Mock<IUserProfileRepository>();
        _messageClientMock = new Mock<IMessageClient>();
        _userProfileService = new UserProfileService.Services.UserProfileService(_userProfileRepositoryMock.Object, _messageClientMock.Object);
    }

    [Fact]
    public async Task RegisterUserAsync_ShouldRegisterUserAndSendMessage()
    {
        // Arrange
        var userProfileDto = new UserProfileDto
        {
            Username = "testuser",
            Email = "test@example.com",
            Bio = "Test Bio"
        };

        _userProfileRepositoryMock
            .Setup(repo => repo.AddUserProfileAsync(It.IsAny<UserProfile>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _userProfileService.RegisterUserAsync(userProfileDto);

        // Assert
        _userProfileRepositoryMock.Verify(repo => repo.AddUserProfileAsync(It.Is<UserProfile>(up => 
            up.Username == userProfileDto.Username &&
            up.Email == userProfileDto.Email &&
            up.Bio == userProfileDto.Bio
        )), Times.Once);

        _messageClientMock.Verify(client => client.Send(It.IsAny<UserProfileUpdatedMessage>(), "UserProfileUpdatedMessage"), Times.Once);
        Assert.NotEqual(Guid.Empty, result); // Ensure a GUID was returned
    }

    [Fact]
    public async Task GetUserProfileWithTweetsAsync_ShouldReturnUserProfileWithTweets()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userProfile = new UserProfile
        {
            Id = userId,
            Username = "testuser",
            Bio = "Test Bio"
        };
        
        var tweets = new List<TweetDto>
        {
            new TweetDto { Content = "Test Tweet" }
        };

        _userProfileRepositoryMock
            .Setup(repo => repo.GetUserProfileByIdAsync(userId))
            .ReturnsAsync(userProfile);

        _messageClientMock
            .Setup(client => client.Listen<TweetResponseMessage>(It.IsAny<Action<TweetResponseMessage>>(), "UserTweetsFetched"))
            .Callback<Action<TweetResponseMessage>, string>((callback, _) =>
            {
                var tweetResponse = new TweetResponseMessage { UserId = userId, Tweets = tweets };
                callback(tweetResponse);
            });

        // Act
        var result = await _userProfileService.GetUserProfileWithTweetsAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal("testuser", result.Username);
        Assert.Equal("Test Bio", result.Bio);
        Assert.Single(result.Tweets);
        Assert.Equal("Test Tweet", result.Tweets[0].Content);
    }

    [Fact]
    public async Task GetUserProfileWithTweetsAsync_ShouldReturnEmptyTweets_WhenNoTweetsReceived()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userProfile = new UserProfile
        {
            Id = userId,
            Username = "testuser",
            Bio = "Test Bio"
        };

        _userProfileRepositoryMock
            .Setup(repo => repo.GetUserProfileByIdAsync(userId))
            .ReturnsAsync(userProfile);

        _messageClientMock
            .Setup(client => client.Listen<TweetResponseMessage>(It.IsAny<Action<TweetResponseMessage>>(), "UserTweetsFetched"))
            .Callback<Action<TweetResponseMessage>, string>((callback, _) =>
            {
                // No tweets received, simulate a timeout scenario
            });

        // Act
        var result = await _userProfileService.GetUserProfileWithTweetsAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal("testuser", result.Username);
        Assert.Empty(result.Tweets); // Ensure empty tweets are returned
    }
}
