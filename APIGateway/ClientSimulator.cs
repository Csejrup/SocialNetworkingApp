using System.Text;
using System.Text.Json;
using Monitoring;
using UserProfileService.Dtos;

namespace APIGateway;

public class ClientSimulator
{
    private static HttpClient _httpClient = new HttpClient();
    public static async Task Run()
    {
        
        Thread.Sleep(22000);
        await CreateUserProfileDb();

        await RegisterUser(new UserProfileDto()
        {
            Id = 1,
            Bio = "I am a freelance tester",
            Email = "test@user.com",
            Username = "tester123"
        });

        var user = await GetUserById(1);
        
        // Create Tweet db
        
        // Add tweet 
        
        // Get tweet
        
        // more...
        

    }


    public static async Task<UserProfileDto?> GetUserById(int id)
    {
        try
        {
            // Start activity trace for zipkin
            using var activity = LoggingService.activitySource.StartActivity();

            var req = new HttpRequestMessage(HttpMethod.Get, $"http://userprofileservice/api/userprofile/{id}");
            
            // Add activity context to request
            req = ActivityHelper.AddActivityInfoToHttpRequest(req, activity);
            
            var res = await _httpClient.SendAsync(req);
            var content = res.Content.ReadAsStringAsync().Result;
            
            LoggingService.Log.Warning(content);
            
            return JsonSerializer.Deserialize<UserProfileDto>(content);
        }
        catch (Exception e)
        {
           LoggingService.Log.AddContext().Debug($"User not found: {id} ");
           return null;
        }

    }
    
    public static async Task CreateUserProfileDb()
    {
        try
        {
            // Start activity trace for zipkin
            using var activity = LoggingService.activitySource.StartActivity();

            var req = new HttpRequestMessage(HttpMethod.Post, $"http://userprofileservice/api/userprofile/database");
            
            // Add activity context to request
            req = ActivityHelper.AddActivityInfoToHttpRequest(req, activity);
            
            var res = await _httpClient.SendAsync(req);
            
        }
        catch (Exception e)
        {
            LoggingService.Log.AddContext().Information($"User db not created: " + e);
            
        }

    }
    
    
    public static async Task RegisterUser(UserProfileDto dto)
    {
        try
        {
            
            // Start activity trace for zipkin
            using var activity = LoggingService.activitySource.StartActivity();

            var req = new HttpRequestMessage(HttpMethod.Post, $"http://userprofileservice/api/userprofile/register");
            req.Content = new StringContent(
                JsonSerializer.Serialize(dto),
                Encoding.UTF8,
                "application/json");
            
            // Add activity context to request
            req = ActivityHelper.AddActivityInfoToHttpRequest(req, activity);
            
            await _httpClient.SendAsync(req);
            

        }
        catch (Exception e)
        {
            LoggingService.Log.AddContext().Debug($"Error registering user");
        }

    }
    
    
    
}