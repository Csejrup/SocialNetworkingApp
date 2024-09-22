using Microsoft.EntityFrameworkCore;
using InteractionService.Models;

namespace InteractionService.Data
{
    public class InteractionDbContext(DbContextOptions<InteractionDbContext> options) : DbContext(options)
    {
        public DbSet<Like> Likes { get; set; }
        public DbSet<Comment> Comments { get; set; }
    }
}