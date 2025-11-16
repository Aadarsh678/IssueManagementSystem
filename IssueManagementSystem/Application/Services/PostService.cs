using IssueManagementSystem.Domain.Entities;
using IssueManagementSystem.Domain.Enums;
using IssueManagementSystem.Domain.Interface;
using static IssueManagementSystem.Application.DTOs.PostDtos;
using Microsoft.EntityFrameworkCore;

namespace IssueManagementSystem.Application.Services
{
    public class PostService
    {
        private readonly IUnitofWork _unitOfWork;

        public PostService(IUnitofWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Create Post (Draft)
        public async Task<PostResponse> CreatePostAsync(CreatePostRequest request, int creatorId)
        {
            var post = new Post
            {
                Title = request.Title,
                Description = request.Description,
                CreatedBy = creatorId,
                Status = PostStatus.DRAFT,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.PostRepository.AddAsync(post);
            await _unitOfWork.SaveAsync();
            var postWithCreator = await _unitOfWork.PostRepository.GetByIdAsync(post.Id);

            return MapToResponse(post);
        }

        // Submit for approval
        public async Task<PostResponse> SubmitForApprovalAsync(int postId, int userId)
        {
            var post = await _unitOfWork.PostRepository.GetByIdAsync(postId);
            if (post == null) throw new Exception("Post not found");
            if (post.CreatedBy != userId) throw new Exception("Cannot submit others' post");

            if (post.Status != PostStatus.DRAFT)
                throw new Exception("Only drafts can be submitted for approval");

            post.Status = PostStatus.PENDING_APPROVAL;

            await _unitOfWork.PostRepository.AddUpdateAsync(new PostUpdate
            {
                PostId = post.Id,
                ActorId = userId,
                Action = PostStatus.PENDING_APPROVAL.ToString(),
                UpdatedAt = DateTime.UtcNow
            });

            await _unitOfWork.SaveAsync();
            return MapToResponse(post);
        }

        // Approve Post
        public async Task<PostResponse> ApprovePostAsync(ChangePostStatusRequest request, int adminId)
        {
            var post = await _unitOfWork.PostRepository.GetByIdAsync(request.Id);
            if (post == null) throw new Exception("Post not found");

            var actor = await _unitOfWork.UserRepository.GetByIdAsync(adminId);
            if (actor == null || (actor.Role != UserRole.ADMIN && actor.Role != UserRole.SUPERADMIN))
                throw new Exception("Only ADMIN or SUPERADMIN can approve posts");

            if (post.CreatedBy == adminId)
                throw new Exception("Creator cannot approve their own post");

            if (post.Status != PostStatus.PENDING_APPROVAL)
                throw new Exception("Post must be submitted for approval before it can be approved");

            post.Status = PostStatus.APPROVED;

            await _unitOfWork.PostRepository.AddUpdateAsync(new PostUpdate
            {
                PostId = post.Id,
                ActorId = adminId,
                Action = PostStatus.APPROVED.ToString(),
                UpdatedAt = DateTime.UtcNow
            });

            await _unitOfWork.SaveAsync();
            return MapToResponse(post);
        }

        // Reject Post
        public async Task<PostResponse> RejectPostAsync(ChangePostStatusRequest request, int adminId)
        {
            var post = await _unitOfWork.PostRepository.GetByIdAsync(request.Id);
            if (post == null) throw new Exception("Post not found");

            var actor = await _unitOfWork.UserRepository.GetByIdAsync(adminId);
            if (actor == null || (actor.Role != UserRole.ADMIN && actor.Role != UserRole.SUPERADMIN))
                throw new Exception("Only ADMIN or SUPERADMIN can reject posts");

            if (post.CreatedBy == adminId)
                throw new Exception("Creator cannot reject their own post");


            if (post.Status != PostStatus.PENDING_APPROVAL)
                throw new Exception("Post must be submitted for approval before it can be rejected");

            post.Status = PostStatus.REJECTED;

            await _unitOfWork.PostRepository.AddUpdateAsync(new PostUpdate
            {
                PostId = post.Id,
                ActorId = adminId,
                Action = PostStatus.REJECTED.ToString(),
                UpdatedAt = DateTime.UtcNow
            });

            await _unitOfWork.SaveAsync();
            return MapToResponse(post);
        }

        // Close Post
        public async Task<PostResponse> ClosePostAsync(ChangePostStatusRequest request, int userId)
        {
            var post = await _unitOfWork.PostRepository.GetByIdAsync(request.Id);
            if (post == null) throw new Exception("Post not found");

            var actor = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (actor == null)
                throw new Exception("User not found");

            if (post.CreatedBy != userId && actor.Role != UserRole.ADMIN && actor.Role != UserRole.SUPERADMIN)
                throw new Exception("Only the creator, ADMIN or SUPERADMIN can close this post");

            if (post.Status != PostStatus.APPROVED)
                throw new Exception("Post must be be approved before it can be closed");

            post.Status = PostStatus.CLOSED;

            await _unitOfWork.PostRepository.AddUpdateAsync(new PostUpdate
            {
                PostId = post.Id,
                ActorId = userId,
                Action = PostStatus.CLOSED.ToString(),
                UpdatedAt = DateTime.UtcNow
            });

            await _unitOfWork.SaveAsync();
            return MapToResponse(post);
        }

        // Delete Post
        public async Task DeletePostAsync(int postId, int userId)
        {
            var post = await _unitOfWork.PostRepository.GetByIdAsync(postId);
            if (post == null) throw new Exception("Post not found");
            if (post.CreatedBy != userId) throw new Exception("Cannot delete others' post");

            await _unitOfWork.PostRepository.DeleteAsync(postId);
            await _unitOfWork.SaveAsync();
        }

        // Get all approved posts (for all users)
        public async Task<IEnumerable<PostResponse>> GetAllApprovedPostsAsync()
        {
            var posts = await _unitOfWork.PostRepository.GetAll()
                .Where(p => p.Status == PostStatus.APPROVED)
                .Include(p => p.Creator)
                .Include(p => p.Assignee)
                .ToListAsync();

            return posts.Select(MapToResponse);
        }

        // Get all posts for a specific user (all statuses)
        public async Task<IEnumerable<PostResponse>> GetAllUserPostsAsync(int userId)
        {
            var posts = await _unitOfWork.PostRepository.GetAll()
                .Where(p => p.CreatedBy == userId)
                .Include(p => p.Creator)
                .Include(p => p.Assignee)
                .ToListAsync();

            return posts.Select(MapToResponse);
        }

        public async Task<IEnumerable<PostResponse>> GetAllSubmittedPostsAsync()
        {
            var posts = await _unitOfWork.PostRepository.GetAll()
                .Where(p => p.Status == PostStatus.PENDING_APPROVAL)
                .Include(p => p.Creator)
                .Include(p => p.Assignee)
                .ToListAsync();

            return posts.Select(MapToResponse);
        }
        public async Task<IEnumerable<PostResponse>> GetSubmittedPostsByUserAsync(int userId)
        {
            var posts = await _unitOfWork.PostRepository.GetAll()
                .Where(p => p.Status == PostStatus.PENDING_APPROVAL && p.CreatedBy == userId)
                .Include(p => p.Creator)
                .Include(p => p.Assignee)
                .ToListAsync();

            return posts.Select(MapToResponse);
        }

        private PostResponse MapToResponse(Post post) => new PostResponse
        {
            Id = post.Id,
            Title = post.Title,
            Description = post.Description,
            Status = post.Status.ToString(),
            CreatorId = post.CreatedBy,
            Creator = post.Creator.UserName,
            Assignee = post.Assignee?.UserName,
            CreatedAt = post.CreatedAt
        };
    }
}
