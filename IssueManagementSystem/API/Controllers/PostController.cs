using IssueManagementSystem.Application.DTOs;
using IssueManagementSystem.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static IssueManagementSystem.Application.DTOs.PostDtos;

[ApiController]
[Route("api/[controller]")]
public class PostController : ControllerBase
{
    private readonly PostService _postService;
    public PostController(PostService postService) => _postService = postService;

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(CreatePostRequest request)
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var post = await _postService.CreatePostAsync(request, userId);
        return Ok(post);
    }

    [HttpPost("submit")]
    [Authorize]
    public async Task<IActionResult> Submit(ChangePostStatusRequest request)
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var post = await _postService.SubmitForApprovalAsync(request.Id, userId);
        return Ok(post);
    }

    [HttpPost("close")]
    [Authorize]
    public async Task<IActionResult> Close(ChangePostStatusRequest request)
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var post = await _postService.ClosePostAsync(request, userId);
        return Ok(post);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        await _postService.DeletePostAsync(id, userId);
        return NoContent();
    }

    [HttpGet("approved")]
    [Authorize]
    public async Task<IActionResult> GetAllApproved()
    {
        var posts = await _postService.GetAllApprovedPostsAsync();
        return Ok(posts);
    }

    [HttpGet("user")]
    [Authorize]
    public async Task<IActionResult> GetUserPosts()
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var posts = await _postService.GetAllUserPostsAsync(userId);
        return Ok(posts);
    }
}
