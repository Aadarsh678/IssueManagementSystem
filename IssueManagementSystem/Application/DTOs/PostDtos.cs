namespace IssueManagementSystem.Application.DTOs
{
    public class PostDtos
    {
        public class CreatePostRequest
        {
            public string Title { get; set; } = null!;
            public string Description { get; set; } = null!;
        }

        public class UpdatePostRequest
        {
            public int Id { get; set; }
            public string? Title { get; set; }
            public string? Description { get; set; }
        }

        public class ChangePostStatusRequest
        {
            public int Id { get; set; }
        }

        public class PostResponse
        {
            public int Id { get; set; }
            public string Title { get; set; } = null!;
            public string Description { get; set; } = null!;
            public string Status { get; set; } = null!;
            public int CreatorId { get; set; }
            public string Creator { get; set; } = null!;
            public string? Assignee { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
