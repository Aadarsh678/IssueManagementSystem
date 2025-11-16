using IssueManagementSystem.Domain.Entities;

namespace IssueManagementSystem.Domain.Interface
{
    public interface IPostRepository
    {
        Task AddAsync(Post post);
        Task<Post?> GetByIdAsync(int id);
        IQueryable<Post> GetAll();
        Task AddUpdateAsync(PostUpdate update);
        Task DeleteAsync(int postId);
    }
}
