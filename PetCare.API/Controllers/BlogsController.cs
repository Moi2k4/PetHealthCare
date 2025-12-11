using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCare.Application.Services.Interfaces;
using System.Security.Claims;

namespace PetCare.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BlogsController : ControllerBase
{
    private readonly IBlogService _blogService;

    public BlogsController(IBlogService blogService)
    {
        _blogService = blogService;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    [HttpPost]
    [Authorize(Roles = "admin,service_provider")]
    public async Task<IActionResult> CreateBlogPost([FromBody] CreateBlogPostRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _blogService.CreateBlogPostAsync(
            request.Title, 
            request.Content, 
            userId, 
            request.CategoryId, 
            request.Tags);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetBlogPostById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin,service_provider")]
    public async Task<IActionResult> UpdateBlogPost(Guid id, [FromBody] UpdateBlogPostRequest request)
    {
        var result = await _blogService.UpdateBlogPostAsync(id, request.Title, request.Content, request.CategoryId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin,service_provider")]
    public async Task<IActionResult> DeleteBlogPost(Guid id)
    {
        var userId = GetCurrentUserId();
        var result = await _blogService.DeleteBlogPostAsync(id, userId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBlogPostById(Guid id)
    {
        var result = await _blogService.GetBlogPostByIdAsync(id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet("slug/{slug}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBlogPostBySlug(string slug)
    {
        var result = await _blogService.GetBlogPostBySlugAsync(slug);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllBlogPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] bool publishedOnly = true)
    {
        var result = await _blogService.GetAllBlogPostsAsync(page, pageSize, publishedOnly);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("category/{categoryId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBlogPostsByCategory(Guid categoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _blogService.GetBlogPostsByCategoryAsync(categoryId, page, pageSize);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("tag/{tag}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBlogPostsByTag(string tag, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _blogService.GetBlogPostsByTagAsync(tag, page, pageSize);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("{id}/publish")]
    [Authorize(Roles = "admin,service_provider")]
    public async Task<IActionResult> PublishBlogPost(Guid id)
    {
        var userId = GetCurrentUserId();
        var result = await _blogService.PublishBlogPostAsync(id, userId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("{id}/unpublish")]
    [Authorize(Roles = "admin,service_provider")]
    public async Task<IActionResult> UnpublishBlogPost(Guid id)
    {
        var userId = GetCurrentUserId();
        var result = await _blogService.UnpublishBlogPostAsync(id, userId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("{postId}/comments")]
    [Authorize]
    public async Task<IActionResult> AddComment(Guid postId, [FromBody] AddCommentRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _blogService.AddCommentAsync(postId, userId, request.Content, request.ParentCommentId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("comments/{commentId}")]
    [Authorize]
    public async Task<IActionResult> DeleteComment(Guid commentId)
    {
        var userId = GetCurrentUserId();
        var result = await _blogService.DeleteCommentAsync(commentId, userId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("{postId}/like")]
    [Authorize]
    public async Task<IActionResult> LikeBlogPost(Guid postId)
    {
        var userId = GetCurrentUserId();
        var result = await _blogService.LikeBlogPostAsync(postId, userId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("{postId}/like")]
    [Authorize]
    public async Task<IActionResult> UnlikeBlogPost(Guid postId)
    {
        var userId = GetCurrentUserId();
        var result = await _blogService.UnlikeBlogPostAsync(postId, userId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("categories")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllCategories()
    {
        var result = await _blogService.GetAllCategoriesAsync();
        return Ok(result);
    }

    [HttpGet("tags")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllTags()
    {
        var result = await _blogService.GetAllTagsAsync();
        return Ok(result);
    }
}

public record CreateBlogPostRequest(string Title, string Content, Guid? CategoryId, List<string>? Tags);
public record UpdateBlogPostRequest(string? Title, string? Content, Guid? CategoryId);
public record AddCommentRequest(string Content, Guid? ParentCommentId);
