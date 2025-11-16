using IssueManagementSystem.Domain.Entities;
using IssueManagementSystem.Domain.Interface;
using Microsoft.EntityFrameworkCore;
using static IssueManagementSystem.Application.DTOs.CommentDtos;

namespace IssueManagementSystem.Application.Services
{
    public class CommentService
    {
        private readonly IUnitofWork _unitOfWork;

        public CommentService(IUnitofWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Add a comment
        public async Task<CommentResponse> AddCommentAsync(CreateCommentRequest request, int authorId)
        {
            // Check post exists
            var post = await _unitOfWork.PostRepository.GetByIdAsync(request.PostId);
            if (post == null)
                throw new Exception("Post not found");

            var comment = new Comment
            {
                PostId = request.PostId,
                AuthorId = authorId,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.CommentRepository.AddAsync(comment);
            await _unitOfWork.SaveAsync();

            // Load navigation property Author for response
            var createdComment = await _unitOfWork.CommentRepository.GetByIdAsync(comment.Id);

            return MapToResponse(createdComment!);
        }

        // Get all comments for a post
        public async Task<IEnumerable<CommentResponse>> GetCommentsByPostAsync(int postId)
        {
            var comments = await _unitOfWork.CommentRepository.GetAllForPost(postId)
                .Where(c => c.PostId == postId)
                .Include(c => c.Author)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            return comments.Select(MapToResponse);
        }

        // Delete comment (author or admin/superadmin)
        public async Task DeleteCommentAsync(int commentId, int userId)
        {
            var comment = await _unitOfWork.CommentRepository.GetByIdAsync(commentId);
            if (comment == null)
                throw new Exception("Comment not found");

            var actor = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (actor == null)
                throw new Exception("User not found");

            if (comment.AuthorId != userId && actor.Role != Domain.Enums.UserRole.ADMIN && actor.Role != Domain.Enums.UserRole.SUPERADMIN)
                throw new Exception("Only the author, ADMIN, or SUPERADMIN can delete this comment");

            await _unitOfWork.CommentRepository.DeleteAsync(commentId);
            await _unitOfWork.SaveAsync();
        }

        private CommentResponse MapToResponse(Comment comment) => new CommentResponse
        {
            Id = comment.Id,
            PostId = comment.PostId,
            AuthorId = comment.AuthorId,
            Author = comment.Author.UserName,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt
        };
    }
}
