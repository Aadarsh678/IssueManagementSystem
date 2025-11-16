using IssueManagementSystem.Domain.Enums;

namespace IssueManagementSystem.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public UserRole Role { get; set; }

        // Navigation properties
        public ICollection<Post> Posts { get; set; } = new List<Post>(); // Created posts
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<PostUpdate> PostUpdates { get; set; } = new List<PostUpdate>();
    }

}
