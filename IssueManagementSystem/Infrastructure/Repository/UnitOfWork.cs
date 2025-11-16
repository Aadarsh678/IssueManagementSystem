using IssueManagementSystem.Domain.Interface;
using IssueManagementSystem.Infrastructure.Data;

namespace IssueManagementSystem.Infrastructure.Repository
{
    public class UnitOfWork : IUnitofWork
    {
        private readonly AppDbContext _context;

        // Backing field for lazy initialization
        private IPostRepository? _postRepository;

        private IUserRepository? _userRepository;

        private ICommentRepository? _commentRepository;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        // Expose PostRepository
        public IPostRepository PostRepository =>
            _postRepository ??= new PostRepository(_context);

        public IUserRepository UserRepository =>
            _userRepository ??= new UserRepository(_context);

        public ICommentRepository CommentRepository =>
            _commentRepository ??= new CommentRepository(_context);

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
