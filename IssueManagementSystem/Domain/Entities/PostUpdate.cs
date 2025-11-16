namespace IssueManagementSystem.Domain.Entities
{
    public class PostUpdate
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public int ActorId { get; set; }  // User who performed the update
        public string Action { get; set; } = null!;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Post Post { get; set; } = null!;
        public User Actor { get; set; } = null!;
    }

}
