using IssueManagementSystem.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static IssueManagementSystem.Application.DTOs.CommentDtos;

namespace IssueManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly CommentService _commentService;

        public CommentController(CommentService commentService)
        {
            _commentService = commentService;
        }

        // POST: api/Comment
        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] CreateCommentRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var comment = await _commentService.AddCommentAsync(request, userId);
            return Ok(comment);
        }

        // GET: api/Comment/post/{postId}
        [HttpGet("post/{postId}")]
        public async Task<IActionResult> GetCommentsByPost(int postId)
        {
            var comments = await _commentService.GetCommentsByPostAsync(postId);
            return Ok(comments);
        }

        // DELETE: api/Comment/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _commentService.DeleteCommentAsync(id, userId);
            return NoContent();
        }
    }
}
