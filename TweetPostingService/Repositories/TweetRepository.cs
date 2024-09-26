using Microsoft.EntityFrameworkCore;
using TweetPostingService.Models;
using TweetPostingService.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Monitoring;

namespace TweetPostingService.Repositories
{
    public class TweetRepository(TweetDbContext context) : ITweetRepository
    {
        
        private SqlConnection? _connection;

        private DbConnection GetDbConnection()
        {
            if (_connection != null) 
                return _connection;
            
            var  connection = new SqlConnection($"Server=tweets-db;User Id=sa;Password=SuperSecret7!;Encrypt=false;");
            connection.Open();

            _connection = connection;
            return connection;
        }
        
        
        private static void Execute(IDbConnection connection, string sql)
        {
            try
            {
                using var trans = connection.BeginTransaction();
                var cmd = connection.CreateCommand();
                cmd.Transaction = trans;
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
                trans.Commit();
                LoggingService.Log.Information($"SQL executed successfully: {sql}");
            }
            catch (SqlException ex)
            {
                LoggingService.Log.Information($"SQL Execution failed: {sql}", ex);
                throw;
            }
           
        }
        public async Task RecreateDatabase()
        {
            using var trace = LoggingService.activitySource.StartActivity();
            var connection = GetDbConnection();
                try
                {
                    LoggingService.Log.Information("Recreating tables in database.");
                    Execute(connection, "DROP TABLE IF EXISTS Tweets");

                    Execute(connection, "CREATE TABLE Tweets(id INTEGER PRIMARY KEY, userId INTEGER, content VARCHAR(500), createdAt DATE)");
                    LoggingService.Log.Debug("Tables recreated successfully.");
                }
                catch (Exception ex)
                {
                    LoggingService.Log.Error("Error while recreating database", ex);
                    throw;
                }
        }
        
        public async Task AddTweetAsync(Tweet tweet)
        {
            using var _ = LoggingService.activitySource.StartActivity();
            var connection = GetDbConnection();
            
            var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = "INSERT INTO Tweets(id, userId, content, createdAt) VALUES(@id,@userId,@content,@createdAt)";

            var pId = new SqlParameter("id", tweet.Id);
            var pContent= new SqlParameter("content", tweet.Content);
            var pUserId = new SqlParameter("userId", tweet.UserId);
            var pCreatedAt = new SqlParameter("userId", tweet.CreatedAt);

            insertCmd.Parameters.Add(pId);
            insertCmd.Parameters.Add(pContent);
            insertCmd.Parameters.Add(pUserId);
            insertCmd.Parameters.Add(pCreatedAt);

            await insertCmd.ExecuteNonQueryAsync();
        }

        
        
        public async Task<Tweet?> GetTweetByIdAsync(int tweetId)
        {
            using var _ = LoggingService.activitySource.StartActivity();

            var connection = GetDbConnection();
         
            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM UserProfiles WHERE id = @id";
            
            var pId = new SqlParameter("id", tweetId);
            selectCmd.Parameters.Add(pId);
            
            Tweet? tweet = null;

            await using var reader =  await selectCmd.ExecuteReaderAsync();

            await reader.ReadAsync();
            tweet = new Tweet()
            {
                Id = reader.GetInt32(0),
                UserId = reader.GetInt32(1),
                Content = reader.GetString(2),
                CreatedAt = reader.GetDateTime(3)
            };

            return tweet;
        }

        public async Task DeleteTweetAsync(Tweet tweet)
        {
            using var _ = LoggingService.activitySource.StartActivity();

            context.Tweets.Remove(tweet);
            await context.SaveChangesAsync();
        }

        public async Task<List<Tweet>> GetTweetsByUserIdAsync(int userId)
        {
            using var _ = LoggingService.activitySource.StartActivity();

            return await context.Tweets.Where(t => t.UserId == userId).ToListAsync();
        }
    }
}