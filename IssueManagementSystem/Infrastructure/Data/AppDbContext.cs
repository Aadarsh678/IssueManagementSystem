using IssueManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace IssueManagementSystem.Infrastructure.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options) { 
        }
        public DbSet<User> Users => Set<User>();
        public DbSet<Post> Posts => Set<Post>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<PostUpdate> PostUpdates => Set<PostUpdate>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                 .Property(u => u.Role)
                 .HasConversion<string>(); // stores UserRole as string in DB

            modelBuilder.Entity<Post>()
                .Property(p => p.Status)
                .HasConversion<string>();

            // Post
            modelBuilder.Entity<Post>()
                .HasOne(p => p.Creator)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Post>()
                .HasOne(p => p.Assignee)
                .WithMany()
                .HasForeignKey(p => p.AssignedTo)
                .OnDelete(DeleteBehavior.SetNull);

            // Comment
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Author)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            // PostUpdate
            modelBuilder.Entity<PostUpdate>()
                .HasOne(u => u.Post)
                .WithMany(p => p.Updates)
                .HasForeignKey(u => u.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PostUpdate>()
                .HasOne(u => u.Actor)
                .WithMany(a => a.PostUpdates)
                .HasForeignKey(u => u.ActorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
