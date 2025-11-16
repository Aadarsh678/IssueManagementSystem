using IssueManagementSystem.Domain.Entities;

namespace IssueManagementSystem.Domain.Interface
{
    public interface ICommentRepository
    {
        Task AddAsync(Comment comment);
        Task<Comment?> GetByIdAsync(int id);
        IQueryable<Comment> GetAllForPost(int postId);
        Task DeleteAsync(int id);
        Task UpdateAsync(Comment comment);
    }
}
