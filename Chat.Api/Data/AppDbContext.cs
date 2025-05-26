using Microsoft.EntityFrameworkCore;
using ChatApp.API.Models;

namespace ChatApp.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Message> Messages => Set<Message>();
        public DbSet<Group> Groups { get; set; }
    }
}
