using backend_3_module.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend_3_module.Data;

public class BlogDbContext : DbContext
{
    public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Community> Communities { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<CommunityUser> CommunityUsers { get; set; }
    public DbSet<UserLikes> UserLikes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CommunityUser>(entity =>
        {
            entity.HasKey(cu => new { cu.CommunityId, cu.UserId });

            entity.HasOne(cu => cu.User)
                .WithMany(u => u.CommunityUsers)
                .HasForeignKey(cu => cu.UserId);

            entity.HasOne(cu => cu.Community)
                .WithMany(c => c.CommunityUsers)
                .HasForeignKey(cu => cu.CommunityId);
        });

        modelBuilder.Entity<UserLikes>(entity =>
        {
            entity.HasKey(ul => new { ul.PostId, ul.UserId });

            entity.HasOne(ul => ul.User)
                .WithMany(u => u.UserLikes)
                .HasForeignKey(ul => ul.UserId);

            entity.HasOne(ul => ul.Post)
                .WithMany(p => p.UserLikes)
                .HasForeignKey(ul => ul.PostId); //TODO каскадное удаление
        });

        modelBuilder.Entity<PostTags>(entity =>
        {
            entity.HasKey(pt => new { pt.PostId, pt.TagId });

            entity.HasOne(pt => pt.Tag)
                .WithMany(t => t.PostTags)
                .HasForeignKey(pt => pt.TagId);

            entity.HasOne(pt => pt.Post)
                .WithMany(p => p.PostTags)
                .HasForeignKey(pt => pt.PostId);
        });
    }
}