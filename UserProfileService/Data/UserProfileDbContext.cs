using Microsoft.EntityFrameworkCore;
using UserProfileService.Models;

namespace UserProfileService.Data
{
    public class UserProfileDbContext(DbContextOptions<UserProfileDbContext> options) : DbContext(options)
    {
        public DbSet<UserProfile> UserProfiles { get; set; }
    }
}