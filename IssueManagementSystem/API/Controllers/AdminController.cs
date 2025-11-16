using IssueManagementSystem.Application.DTOs;
using IssueManagementSystem.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static IssueManagementSystem.Application.DTOs.PostDtos;

namespace IssueManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly PostService _postService;
        private readonly UserService _userService;
        public AdminController(UserService userService) => _userService = userService;
        public AdminController(PostService postService) => _postService = postService;

        [HttpPost("user/promote")]
        [Authorize(Roles = "SUPERADMIN")]
        public async Task<IActionResult> Promote([FromBody] Promote request)
        {
            var superAdminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _userService.PromoteToAdminAsync(request.id, superAdminId);
            return Ok(new { user.Id, user.UserName, user.Role });
        }

        [HttpPost("post/approve")]
        [Authorize(Roles = "ADMIN,SUPERADMIN")]
        public async Task<IActionResult> Approve(ChangePostStatusRequest request)
        {
            int adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var post = await _postService.ApprovePostAsync(request, adminId);
            return Ok(post);
        }

        [HttpPost("post/reject")]
        [Authorize(Roles = "ADMIN,SUPERADMIN")]
        public async Task<IActionResult> Reject(ChangePostStatusRequest request)
        {
            int adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var post = await _postService.RejectPostAsync(request, adminId);
            return Ok(post);
        }

        // GET: api/Post/submitted/all
        // Admin/SuperAdmin sees all submitted posts
        [HttpGet("post/submitted/all")]
        [Authorize(Roles = "ADMIN,SUPERADMIN")]
        public async Task<IActionResult> GetAllSubmittedPosts()
        {
            var posts = await _postService.GetAllSubmittedPostsAsync();
            return Ok(posts);
        }

        // GET: api/Post/submitted/user/{userId}
        // Admin/SuperAdmin sees submitted posts of a specific user
        [HttpGet("post/submitted/user/{userId}")]
        [Authorize(Roles = "ADMIN,SUPERADMIN")]
        public async Task<IActionResult> GetSubmittedPostsByUser(int userId)
        {
            var posts = await _postService.GetSubmittedPostsByUserAsync(userId);
            return Ok(posts);
        }
    }
}
