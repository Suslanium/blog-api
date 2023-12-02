using blog_api.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace blog_api.Data;

public class BlogDbContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public DbSet<Community> Communities { get; set; }

    public DbSet<Subscription> Subscriptions { get; set; }

    public DbSet<Post> Posts { get; set; }

    public DbSet<Tag> Tags { get; set; }
    
    public DbSet<InvalidTokenInfo> InvalidatedTokens { get; set; }

    public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(user => user.Email);
        modelBuilder.Entity<User>().HasMany(user => user.SubscribedCommunities)
            .WithMany(community => community.Subscribers).UsingEntity<Subscription>();
        modelBuilder.Entity<Post>().HasOne(post => post.Author).WithMany(user => user.Posts)
            .HasForeignKey(post => post.AuthorId).IsRequired();
        modelBuilder.Entity<Post>().HasOne(post => post.Community).WithMany(community => community.Posts)
            .HasForeignKey(post => post.CommunityId).IsRequired(false);
        modelBuilder.Entity<Post>().Property(post => post.LikeCount).HasDefaultValue(0);
        modelBuilder.Entity<Community>().Property(community => community.SubscribersCount).HasDefaultValue(1);
        modelBuilder.Entity<Post>().HasMany(post => post.Tags).WithMany(tag => tag.Posts);
        modelBuilder.Entity<Post>().HasMany<User>().WithMany().UsingEntity<LikedPosts>(
            configureLeft: builder =>
                builder.HasOne<Post>().WithMany(post => post.Likes).HasForeignKey(likedPost => likedPost.PostId),
            configureRight: builder =>
                builder.HasOne<User>().WithMany(user => user.LikedPosts).HasForeignKey(likedPost => likedPost.UserId));
        modelBuilder.Entity<Tag>().HasIndex(tag => tag.Name);
        modelBuilder.Entity<InvalidTokenInfo>().HasOne<User>().WithMany().HasForeignKey(info => info.UserId);
        modelBuilder.Entity<InvalidTokenInfo>().HasKey(info => new { info.UserId, info.IssuedTime });
        base.OnModelCreating(modelBuilder);
    }
}