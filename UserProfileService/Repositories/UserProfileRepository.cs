using System.Data;
using System.Data.Common;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Monitoring;
using UserProfileService.Models;
using UserProfileService.Data;

namespace UserProfileService.Repositories
{
    public class UserProfileRepository(UserProfileDbContext context) : IUserProfileRepository
    {
        private SqlConnection? _connection;

        private DbConnection GetDbConnection()
        {
            if (_connection != null) 
                return _connection;
            
            LoggingService.Log.Debug("Get coon ection");
            var  connection = new SqlConnection($"Server=userprofile-db;User Id=sa;Password=SuperSecret7!;Encrypt=false;");
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
                    Execute(connection, "DROP TABLE IF EXISTS UserProfiles");

                    Execute(connection, "CREATE TABLE UserProfiles(id INTEGER PRIMARY KEY, userName VARCHAR(500), email VARCHAR(500), bio VARCHAR(500) )");
                    LoggingService.Log.Debug("Tables recreated successfully.");
                }
                catch (Exception ex)
                {
                    LoggingService.Log.Error("Error while recreating database", ex);
                    throw;
                }
        }
        
        
        
        public async Task AddUserProfileAsync(UserProfile userProfile)
        {

            using var activity = LoggingService.activitySource.StartActivity();
            
            var connection = GetDbConnection();
            
            var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = "INSERT INTO UserProfiles(id, userName, email, bio) VALUES(@id,@userName,@email,@bio)";

            var pBio = new SqlParameter("bio", userProfile.Bio);
            var pId = new SqlParameter("id", userProfile.Id);
            var pUserName= new SqlParameter("userName", userProfile.Username);
            var pEmail = new SqlParameter("email", userProfile.Id);

            insertCmd.Parameters.Add(pBio);
            insertCmd.Parameters.Add(pEmail);
            insertCmd.Parameters.Add(pUserName);
            insertCmd.Parameters.Add(pId);

           await insertCmd.ExecuteNonQueryAsync();
           
        }

        public async Task<UserProfile?> GetUserProfileByIdAsync(int id)
        {
            using var activity = LoggingService.activitySource.StartActivity();
            var connection = GetDbConnection();
            
            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM UserProfiles WHERE id = @id";
            
            var pId = new SqlParameter("id", id);
            selectCmd.Parameters.Add(pId);
            
            UserProfile? user = null;

            await using var reader =  await selectCmd.ExecuteReaderAsync();

            await reader.ReadAsync();
                user = new UserProfile()
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Email = reader.GetString(2),
                    Bio = reader.GetString(3)
                };

            return user;
        }

        public async Task<UserProfile?> GetUserProfileByUsernameAsync(string username)
        {
            using var activity = LoggingService.activitySource.StartActivity();
            return await context.UserProfiles.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task UpdateUserProfileAsync(UserProfile userProfile)
        {
            using var activity = LoggingService.activitySource.StartActivity();
            context.UserProfiles.Update(userProfile);
            await context.SaveChangesAsync();
        }

        public async Task<List<UserProfile>> GetAllUserProfilesAsync()
        {
            using var activity = LoggingService.activitySource.StartActivity();
            return await context.UserProfiles.ToListAsync();
        }

        public async Task FollowUserAsync(int userId, int userIdToFollow)
        {
            using var activity = LoggingService.activitySource.StartActivity();
            var user = await context.UserProfiles.FindAsync(userId);
            var userToFollow = await context.UserProfiles.FindAsync(userIdToFollow);

            if (user == null || userToFollow == null)
                throw new Exception("Either the user or the user to follow does not exist.");

            if (!user.Following.Contains(userIdToFollow))
            {
                user.Following.Add(userIdToFollow);
                userToFollow.Followers.Add(userId);
            }

            await context.SaveChangesAsync();
        }

        public async Task UnfollowUserAsync(int userId, int userIdToUnfollow)
        {
            using var activity = LoggingService.activitySource.StartActivity();
            var user = await context.UserProfiles.FindAsync(userId);
            var userToUnfollow = await context.UserProfiles.FindAsync(userIdToUnfollow);

            if (user == null || userToUnfollow == null)
                throw new Exception("Either the user or the user to unfollow does not exist.");

            if (user.Following.Contains(userIdToUnfollow))
            {
                user.Following.Remove(userIdToUnfollow);
                userToUnfollow.Followers.Remove(userId);
            }

            await context.SaveChangesAsync();
        }

        // New method to get the list of followers
        public async Task<List<int>> GetFollowersAsync(int userId)
        {
            using var activity = LoggingService.activitySource.StartActivity();
            var user = await context.UserProfiles.FindAsync(userId);
            if (user == null) throw new Exception("User not found.");

            return user.Followers;
        }

    }
}
