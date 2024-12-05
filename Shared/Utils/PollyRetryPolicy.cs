namespace Shared.Utils;
using Polly;
using Polly.Retry;

public static class PollyRetryPolicy
{
  public static AsyncRetryPolicy CreateRetryPolicy()
  {
    return Policy
      .Handle<Exception>() 
      .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(500 * retryAttempt), // Exponential backoff
        onRetry: (exception, timespan, retryCount, context) =>
        {
          Console.WriteLine($"Retry {retryCount} encountered an error: {exception.Message}. Retrying in {timespan}...");
        });
  }
}