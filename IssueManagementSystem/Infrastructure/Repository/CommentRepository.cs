using IssueManagementSystem.Domain.Entities;
using IssueManagementSystem.Domain.Interface;
using IssueManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IssueManagementSystem.Infrastructure.Repository
{
    public class CommentRepository : ICommentRepository
    {
        private readonly AppDbContext _context;
        public CommentRepository(AppDbContext context) => _context = context;

        public async Task AddAsync(Comment comment)
        {
            await _context.Comments.AddAsync(comment);
        }

        public async Task<Comment?> GetByIdAsync(int id)
        {
            return await _context.Comments
                .Include(c => c.Author)
                .Include(c => c.Post)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public IQueryable<Comment> GetAllForPost(int postId)
        {
            return _context.Comments
                .Where(c => c.PostId == postId)
                .Include(c => c.Author)
                .Include(c => c.Post)
                .AsQueryable();
        }

        public async Task DeleteAsync(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment != null)
                _context.Comments.Remove(comment);
        }

        public async Task UpdateAsync(Comment comment)
        {
            _context.Comments.Update(comment);
        }
    }
}
