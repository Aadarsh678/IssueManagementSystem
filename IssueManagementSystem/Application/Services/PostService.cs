using IssueManagementSystem.Domain.Entities;
using IssueManagementSystem.Domain.Enums;
using IssueManagementSystem.Domain.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using static IssueManagementSystem.Application.DTOs.PostDtos;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
                PostType = request.PostType,
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

        //get post by id
        public async Task<PostResponse> GetPostByIdAsync(int postId)
        {
            // Get post with related entities
            var post = await _unitOfWork.PostRepository.GetByIdAsync(postId);

            if (post == null)
                throw new Exception("Post not found");

            // Map to DTO using your helper
            return MapToResponse(post);
        }


        // Get all approved posts (for all users)
        public async Task<IEnumerable<PostResponse>> GetAllApprovedPostsAsync(PostType? typeFilter)
        {
            var query = _unitOfWork.PostRepository
                .GetAll()
                .Include(p => p.Creator)
                .Include(p => p.Assignee)
                .Where(p => p.Status == PostStatus.APPROVED)
                .AsQueryable();

            if (typeFilter.HasValue)
            {
                query = query.Where(p => p.PostType == typeFilter.Value);
            }

            var posts = await query.ToListAsync();
            return posts.Select(MapToResponse);
        }



        // Get all posts for a specific user (all statuses)
        public async Task<IEnumerable<PostResponse>> GetAllUserPostsAsync(
        int userId,
        PostType? typeFilter,
        PostStatus? statusFilter)
            {
                var query = _unitOfWork.PostRepository
                    .GetAll()
                    .Include(p => p.Creator)
                    .Include(p => p.Assignee)
                    .Where(p => p.CreatedBy == userId)
                    .AsQueryable();

                if (typeFilter.HasValue)
                    query = query.Where(p => p.PostType == typeFilter.Value);

                if (statusFilter.HasValue)
                    query = query.Where(p => p.Status == statusFilter.Value);

                var posts = await query.ToListAsync();
                return posts.Select(MapToResponse);
            }


        public async Task<IEnumerable<PostResponse>> GetAllSubmittedPostsAsync(PostType? typeFilter)
        {
            // Build query 
            var query = _unitOfWork.PostRepository
                .GetAll()
                .Include(p => p.Creator)
                .Include(p => p.Assignee)
                .Where(p => p.Status == PostStatus.PENDING_APPROVAL)
                .AsQueryable();

            // Apply optional filter
            if (typeFilter.HasValue)
            {
                query = query.Where(p => p.PostType == typeFilter.Value);
            }

            // Execute query
            var posts = await query.ToListAsync();

            // Map to DTO
            return posts.Select(MapToResponse);
        }

        public async Task<IEnumerable<PostResponse>> GetSubmittedPostsByUserAsync(int userId, PostType? typeFilter)
        {
            // Build query first
            var query = _unitOfWork.PostRepository
                .GetAll()
                .Include(p => p.Creator)
                .Include(p => p.Assignee)
                .Where(p => p.Status == PostStatus.PENDING_APPROVAL && p.CreatedBy == userId)
                .AsQueryable();

            // Apply optional filter
            if (typeFilter.HasValue)
            {
                query = query.Where(p => p.PostType == typeFilter.Value);
            }

            // Execute query
            var posts = await query.ToListAsync();

            // Map to DTO
            return posts.Select(MapToResponse);
        }


        private PostResponse MapToResponse(Post post) => new PostResponse
        {
            Id = post.Id,
            Title = post.Title,
            Description = post.Description,
            PostType =post.PostType.ToString(),
            Status = post.Status.ToString(),
            CreatorId = post.CreatedBy,
            Creator = post.Creator.UserName,
            Assignee = post.Assignee?.UserName,
            CreatedAt = post.CreatedAt
        };
    }
}
