using IssueManagementSystem.Domain.Entities;
using IssueManagementSystem.Domain.Interface;
using IssueManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IssueManagementSystem.Infrastructure.Repository
{
    public class PostRepository : IPostRepository
    {
        private readonly AppDbContext _context;
        public PostRepository(AppDbContext context) => _context = context;

        public async Task AddAsync(Post post) => await _context.Posts.AddAsync(post);

        public async Task<Post?> GetByIdAsync(int id) =>
            await _context.Posts.Include(p => p.Creator)
                                .Include(p => p.Assignee)
                                .FirstOrDefaultAsync(p => p.Id == id);

        public IQueryable<Post> GetAll() => _context.Posts.AsQueryable();

        public async Task AddUpdateAsync(PostUpdate update) =>
            await _context.PostUpdates.AddAsync(update);

        public async Task DeleteAsync(int postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post != null) _context.Posts.Remove(post);
        }
    }
}
