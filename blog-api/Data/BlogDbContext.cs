using blog_api.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace blog_api.Data;

public class BlogDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    
    public DbSet<Community> Communities { get; set; }
    
    public DbSet<Subscription> Subscriptions { get; set; }

    public DbSet<TokenValidation> TokenValidation { get; set; }

    public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(user => user.Email);
        modelBuilder.Entity<User>().HasMany(user => user.SubscribedCommunities)
            .WithMany(community => community.Subscribers).UsingEntity<Subscription>();
        base.OnModelCreating(modelBuilder);
    }
}