using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCare.Application.DTOs.Blog;
using PetCare.Application.Services.Interfaces;

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

    private Guid? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }

    // ──────────────────────── Posts (public) ────────────────────────

    /// <summary>Get all published blog posts</summary>
    [HttpGet]
    public async Task<IActionResult> GetPublishedPosts()
    {
        var result = await _blogService.GetPublishedPostsAsync();
        return Ok(result);
    }

    /// <summary>Get a published post by slug (increments view count)</summary>
    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var userId = GetCurrentUserId();
        var result = await _blogService.GetPostBySlugAsync(slug, userId);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    /// <summary>Get a post by ID (increments view count)</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = GetCurrentUserId();
        var result = await _blogService.GetPostByIdAsync(id, userId);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    /// <summary>Get published posts by category</summary>
    [HttpGet("category/{categoryId:guid}")]
    public async Task<IActionResult> GetByCategory(Guid categoryId)
    {
        var result = await _blogService.GetPostsByCategoryAsync(categoryId);
        return Ok(result);
    }

    // ──────────────────────── Posts (admin) ────────────────────────

    /// <summary>Get all posts (admin)</summary>
    [HttpGet("all")]
    [Authorize]
    public async Task<IActionResult> GetAllPosts()
    {
        var result = await _blogService.GetAllPostsAsync();
        return Ok(result);
    }

    /// <summary>Create a new blog post (authenticated)</summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreatePost([FromBody] CreateBlogPostDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await _blogService.CreatePostAsync(userId.Value, dto);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>Update a blog post (authenticated)</summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> UpdatePost(Guid id, [FromBody] UpdateBlogPostDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _blogService.UpdatePostAsync(id, dto);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>Delete a blog post (authenticated)</summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> DeletePost(Guid id)
    {
        var result = await _blogService.DeletePostAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>Publish a blog post (authenticated)</summary>
    [HttpPatch("{id:guid}/publish")]
    [Authorize]
    public async Task<IActionResult> PublishPost(Guid id)
    {
        var result = await _blogService.PublishPostAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>Unpublish a blog post (authenticated)</summary>
    [HttpPatch("{id:guid}/unpublish")]
    [Authorize]
    public async Task<IActionResult> UnpublishPost(Guid id)
    {
        var result = await _blogService.UnpublishPostAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    // ──────────────────────── Comments ────────────────────────

    /// <summary>Add a comment to a post (authenticated)</summary>
    [HttpPost("{postId:guid}/comments")]
    [Authorize]
    public async Task<IActionResult> AddComment(Guid postId, [FromBody] CreateCommentDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await _blogService.AddCommentAsync(userId.Value, postId, dto);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>Approve a comment (authenticated)</summary>
    [HttpPatch("comments/{commentId:guid}/approve")]
    [Authorize]
    public async Task<IActionResult> ApproveComment(Guid commentId)
    {
        var result = await _blogService.ApproveCommentAsync(commentId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>Delete a comment (authenticated)</summary>
    [HttpDelete("comments/{commentId:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteComment(Guid commentId)
    {
        var result = await _blogService.DeleteCommentAsync(commentId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    // ──────────────────────── Likes ────────────────────────

    /// <summary>Toggle like on a post (authenticated)</summary>
    [HttpPost("{postId:guid}/like")]
    [Authorize]
    public async Task<IActionResult> ToggleLike(Guid postId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await _blogService.ToggleLikeAsync(userId.Value, postId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    // ──────────────────────── Categories ────────────────────────

    /// <summary>Get all blog categories</summary>
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var result = await _blogService.GetCategoriesAsync();
        return Ok(result);
    }

    /// <summary>Create a blog category (authenticated)</summary>
    [HttpPost("categories")]
    [Authorize]
    public async Task<IActionResult> CreateCategory([FromBody] CreateBlogCategoryDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _blogService.CreateCategoryAsync(dto);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>Delete a blog category (authenticated)</summary>
    [HttpDelete("categories/{categoryId:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteCategory(Guid categoryId)
    {
        var result = await _blogService.DeleteCategoryAsync(categoryId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}
