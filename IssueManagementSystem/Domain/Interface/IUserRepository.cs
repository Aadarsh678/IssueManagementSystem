using IssueManagementSystem.Domain.Entities;

namespace IssueManagementSystem.Domain.Interface
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);
        Task AddAsync(User user);
    }
}
