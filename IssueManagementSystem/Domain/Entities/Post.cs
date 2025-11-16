using IssueManagementSystem.Domain.Enums;


namespace IssueManagementSystem.Domain.Entities
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int CreatedBy { get; set; } // FK to User
        public int? AssignedTo { get; set; } // Nullable FK
        public PostStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User Creator { get; set; } = null!;     // CreatedBy
        public User? Assignee { get; set; }            // AssignedTo
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<PostUpdate> Updates { get; set; } = new List<PostUpdate>();
    }

}
