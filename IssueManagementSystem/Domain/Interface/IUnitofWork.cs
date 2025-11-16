namespace IssueManagementSystem.Domain.Interface
{
    public interface IUnitofWork
    {
        IPostRepository PostRepository { get; }
        IUserRepository UserRepository { get; }
        ICommentRepository CommentRepository { get; }
        Task<int> SaveAsync();
    }
}
