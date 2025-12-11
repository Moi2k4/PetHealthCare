using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetCare.Application.Common;
using PetCare.Application.Services.Interfaces;
using PetCare.Domain.Entities;
using PetCare.Infrastructure.Data;

namespace PetCare.Application.Services.Implementations;

public class BlogService : IBlogService
{
    private readonly PetCareDbContext _context;
    private readonly ILogger<BlogService> _logger;

    public BlogService(PetCareDbContext context, ILogger<BlogService> logger)
    {
        _context = context;
        _logger = logger;
    }

    private string GenerateSlug(string title)
    {
        return title.ToLower().Replace(" ", "-").Replace(",", "").Replace(".", "");
    }

    public async Task<ServiceResult<BlogPost>> CreateBlogPostAsync(string title, string content, Guid authorId, Guid? categoryId = null, List<string>? tags = null)
    {
        try
        {
            var slug = GenerateSlug(title);
            var blogPost = new BlogPost
            {
                Title = title,
                Slug = slug,
                Content = content,
                AuthorId = authorId,
                CategoryId = categoryId,
                Status = "draft",
                ViewCount = 0
            };

            _context.BlogPosts.Add(blogPost);
            await _context.SaveChangesAsync();

            if (tags != null && tags.Any())
            {
                foreach (var tagName in tags)
                {
                    var tag = await _context.Tags.FirstOrDefaultAsync(t => t.TagName == tagName);
                    if (tag == null)
                    {
                        tag = new Tag { TagName = tagName, Slug = GenerateSlug(tagName) };
                        _context.Tags.Add(tag);
                        await _context.SaveChangesAsync();
                    }

                    var blogPostTag = new BlogPostTag { PostId = blogPost.Id, TagId = tag.Id };
                    _context.BlogPostTags.Add(blogPostTag);
                }
                await _context.SaveChangesAsync();
            }

            return ServiceResult<BlogPost>.SuccessResult(blogPost, "Blog post created successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating blog post");
            return ServiceResult<BlogPost>.FailureResult("Failed to create blog post.");
        }
    }

    public async Task<ServiceResult<BlogPost>> UpdateBlogPostAsync(Guid id, string? title = null, string? content = null, Guid? categoryId = null)
    {
        try
        {
            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost == null) return ServiceResult<BlogPost>.FailureResult("Blog post not found.");

            if (!string.IsNullOrEmpty(title))
            {
                blogPost.Title = title;
                blogPost.Slug = GenerateSlug(title);
            }
            if (!string.IsNullOrEmpty(content)) blogPost.Content = content;
            if (categoryId.HasValue) blogPost.CategoryId = categoryId;

            await _context.SaveChangesAsync();
            return ServiceResult<BlogPost>.SuccessResult(blogPost, "Blog post updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating blog post {Id}", id);
            return ServiceResult<BlogPost>.FailureResult("Failed to update blog post.");
        }
    }

    public async Task<ServiceResult<bool>> DeleteBlogPostAsync(Guid id, Guid authorId)
    {
        try
        {
            var blogPost = await _context.BlogPosts.FirstOrDefaultAsync(b => b.Id == id && b.AuthorId == authorId);
            if (blogPost == null) return ServiceResult<bool>.FailureResult("Blog post not found or unauthorized.");

            _context.BlogPosts.Remove(blogPost);
            await _context.SaveChangesAsync();
            return ServiceResult<bool>.SuccessResult(true, "Blog post deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting blog post {Id}", id);
            return ServiceResult<bool>.FailureResult("Failed to delete blog post.");
        }
    }

    public async Task<ServiceResult<BlogPost>> GetBlogPostByIdAsync(Guid id)
    {
        try
        {
            var blogPost = await _context.BlogPosts
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Include(b => b.Comments).ThenInclude(c => c.User)
                .Include(b => b.Likes)
                .Include(b => b.BlogPostTags).ThenInclude(bt => bt.Tag)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (blogPost == null) return ServiceResult<BlogPost>.FailureResult("Blog post not found.");

            blogPost.ViewCount++;
            await _context.SaveChangesAsync();

            return ServiceResult<BlogPost>.SuccessResult(blogPost);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting blog post {Id}", id);
            return ServiceResult<BlogPost>.FailureResult("Failed to retrieve blog post.");
        }
    }

    public async Task<ServiceResult<BlogPost>> GetBlogPostBySlugAsync(string slug)
    {
        try
        {
            var blogPost = await _context.BlogPosts
                .Include(b => b.Author)
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Slug == slug);

            if (blogPost == null) return ServiceResult<BlogPost>.FailureResult("Blog post not found.");

            blogPost.ViewCount++;
            await _context.SaveChangesAsync();

            return ServiceResult<BlogPost>.SuccessResult(blogPost);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting blog post by slug");
            return ServiceResult<BlogPost>.FailureResult("Failed to retrieve blog post.");
        }
    }

    public async Task<ServiceResult<PagedResult<BlogPost>>> GetAllBlogPostsAsync(int page = 1, int pageSize = 10, bool publishedOnly = true)
    {
        try
        {
            var query = _context.BlogPosts.Include(b => b.Author).Include(b => b.Category).AsQueryable();
            if (publishedOnly) query = query.Where(b => b.Status == "published");

            query = query.OrderByDescending(b => b.PublishedAt ?? b.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var pagedResult = new PagedResult<BlogPost> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
            return ServiceResult<PagedResult<BlogPost>>.SuccessResult(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all blog posts");
            return ServiceResult<PagedResult<BlogPost>>.FailureResult("Failed to retrieve blog posts.");
        }
    }

    public async Task<ServiceResult<PagedResult<BlogPost>>> GetBlogPostsByCategoryAsync(Guid categoryId, int page = 1, int pageSize = 10)
    {
        try
        {
            var query = _context.BlogPosts.Where(b => b.CategoryId == categoryId && b.Status == "published")
                .OrderByDescending(b => b.PublishedAt);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return ServiceResult<PagedResult<BlogPost>>.SuccessResult(new PagedResult<BlogPost> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting blog posts by category");
            return ServiceResult<PagedResult<BlogPost>>.FailureResult("Failed to retrieve blog posts.");
        }
    }

    public async Task<ServiceResult<PagedResult<BlogPost>>> GetBlogPostsByTagAsync(string tag, int page = 1, int pageSize = 10)
    {
        try
        {
            var query = _context.BlogPosts
                .Include(b => b.BlogPostTags).ThenInclude(bt => bt.Tag)
                .Where(b => b.BlogPostTags.Any(t => t.Tag.TagName == tag) && b.Status == "published")
                .OrderByDescending(b => b.PublishedAt);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return ServiceResult<PagedResult<BlogPost>>.SuccessResult(new PagedResult<BlogPost> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting blog posts by tag");
            return ServiceResult<PagedResult<BlogPost>>.FailureResult("Failed to retrieve blog posts.");
        }
    }

    public async Task<ServiceResult<BlogPost>> PublishBlogPostAsync(Guid id, Guid authorId)
    {
        try
        {
            var blogPost = await _context.BlogPosts.FirstOrDefaultAsync(b => b.Id == id && b.AuthorId == authorId);
            if (blogPost == null) return ServiceResult<BlogPost>.FailureResult("Blog post not found or unauthorized.");

            blogPost.Status = "published";
            blogPost.PublishedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ServiceResult<BlogPost>.SuccessResult(blogPost, "Blog post published successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing blog post");
            return ServiceResult<BlogPost>.FailureResult("Failed to publish blog post.");
        }
    }

    public async Task<ServiceResult<BlogPost>> UnpublishBlogPostAsync(Guid id, Guid authorId)
    {
        try
        {
            var blogPost = await _context.BlogPosts.FirstOrDefaultAsync(b => b.Id == id && b.AuthorId == authorId);
            if (blogPost == null) return ServiceResult<BlogPost>.FailureResult("Blog post not found or unauthorized.");

            blogPost.Status = "draft";
            await _context.SaveChangesAsync();

            return ServiceResult<BlogPost>.SuccessResult(blogPost, "Blog post unpublished successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unpublishing blog post");
            return ServiceResult<BlogPost>.FailureResult("Failed to unpublish blog post.");
        }
    }

    public async Task<ServiceResult<BlogComment>> AddCommentAsync(Guid postId, Guid userId, string content, Guid? parentCommentId = null)
    {
        try
        {
            var blogPost = await _context.BlogPosts.FindAsync(postId);
            if (blogPost == null) return ServiceResult<BlogComment>.FailureResult("Blog post not found.");

            var comment = new BlogComment
            {
                PostId = postId,
                UserId = userId,
                CommentText = content,
                ParentCommentId = parentCommentId
            };

            _context.BlogComments.Add(comment);
            await _context.SaveChangesAsync();

            return ServiceResult<BlogComment>.SuccessResult(comment, "Comment added successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comment");
            return ServiceResult<BlogComment>.FailureResult("Failed to add comment.");
        }
    }

    public async Task<ServiceResult<bool>> DeleteCommentAsync(Guid commentId, Guid userId)
    {
        try
        {
            var comment = await _context.BlogComments.FirstOrDefaultAsync(c => c.Id == commentId && c.UserId == userId);
            if (comment == null) return ServiceResult<bool>.FailureResult("Comment not found or unauthorized.");

            _context.BlogComments.Remove(comment);
            await _context.SaveChangesAsync();

            return ServiceResult<bool>.SuccessResult(true, "Comment deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting comment");
            return ServiceResult<bool>.FailureResult("Failed to delete comment.");
        }
    }

    public async Task<ServiceResult<BlogLike>> LikeBlogPostAsync(Guid postId, Guid userId)
    {
        try
        {
            var existingLike = await _context.BlogLikes.FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
            if (existingLike != null) return ServiceResult<BlogLike>.FailureResult("You already liked this post.");

            var like = new BlogLike { PostId = postId, UserId = userId };
            _context.BlogLikes.Add(like);
            await _context.SaveChangesAsync();

            return ServiceResult<BlogLike>.SuccessResult(like, "Blog post liked successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error liking blog post");
            return ServiceResult<BlogLike>.FailureResult("Failed to like blog post.");
        }
    }

    public async Task<ServiceResult<bool>> UnlikeBlogPostAsync(Guid postId, Guid userId)
    {
        try
        {
            var like = await _context.BlogLikes.FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
            if (like == null) return ServiceResult<bool>.FailureResult("You haven't liked this post.");

            _context.BlogLikes.Remove(like);
            await _context.SaveChangesAsync();

            return ServiceResult<bool>.SuccessResult(true, "Blog post unliked successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unliking blog post");
            return ServiceResult<bool>.FailureResult("Failed to unlike blog post.");
        }
    }

    public async Task<ServiceResult<List<BlogCategory>>> GetAllCategoriesAsync()
    {
        try
        {
            var categories = await _context.BlogCategories.ToListAsync();
            return ServiceResult<List<BlogCategory>>.SuccessResult(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting blog categories");
            return ServiceResult<List<BlogCategory>>.FailureResult("Failed to retrieve categories.");
        }
    }

    public async Task<ServiceResult<List<Tag>>> GetAllTagsAsync()
    {
        try
        {
            var tags = await _context.Tags.ToListAsync();
            return ServiceResult<List<Tag>>.SuccessResult(tags);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tags");
            return ServiceResult<List<Tag>>.FailureResult("Failed to retrieve tags.");
        }
    }
}

