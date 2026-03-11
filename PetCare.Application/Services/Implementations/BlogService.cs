using PetCare.Application.Common;
using PetCare.Application.DTOs.Blog;
using PetCare.Application.Services.Interfaces;
using PetCare.Domain.Entities;
using PetCare.Infrastructure.Repositories.Interfaces;
using System.Text.RegularExpressions;

namespace PetCare.Application.Services.Implementations;

public class BlogService : IBlogService
{
    private readonly IUnitOfWork _unitOfWork;

    public BlogService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    private static string GenerateSlug(string text)
    {
        var slug = text.ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        return slug.Trim('-');
    }

    private static BlogPostDto MapToDto(BlogPost post, int likeCount = 0)
    {
        return new BlogPostDto
        {
            Id = post.Id,
            Title = post.Title,
            Slug = post.Slug,
            Content = post.Content,
            FeaturedImageUrl = post.FeaturedImageUrl,
            Excerpt = post.Excerpt,
            Status = post.Status,
            AuthorId = post.AuthorId,
            AuthorName = post.Author?.FullName,
            CategoryId = post.CategoryId,
            CategoryName = post.Category?.CategoryName,
            ViewCount = post.ViewCount,
            LikeCount = likeCount,
            PublishedAt = post.PublishedAt,
            CreatedAt = post.CreatedAt,
            Tags = post.BlogPostTags?.Select(t => t.Tag.TagName).ToList() ?? new()
        };
    }

    private static BlogCommentDto MapCommentToDto(BlogComment comment)
    {
        return new BlogCommentDto
        {
            Id = comment.Id,
            PostId = comment.PostId,
            UserId = comment.UserId,
            UserName = comment.User?.FullName,
            ParentCommentId = comment.ParentCommentId,
            CommentText = comment.CommentText,
            IsApproved = comment.IsApproved,
            CreatedAt = comment.CreatedAt,
            Replies = comment.Replies?
                .Where(r => r.IsApproved)
                .Select(MapCommentToDto)
                .ToList() ?? new()
        };
    }

    public async Task<ServiceResult<IEnumerable<BlogPostDto>>> GetPublishedPostsAsync()
    {
        try
        {
            var posts = await _unitOfWork.BlogPosts.GetPublishedPostsAsync();
            var likeRepo = _unitOfWork.Repository<BlogLike>();
            var dtos = new List<BlogPostDto>();
            foreach (var post in posts)
            {
                var likeCount = await likeRepo.CountAsync(l => l.PostId == post.Id);
                dtos.Add(MapToDto(post, likeCount));
            }
            return ServiceResult<IEnumerable<BlogPostDto>>.SuccessResult(dtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<BlogPostDto>>.FailureResult($"Error retrieving posts: {ex.Message}");
        }
    }

    public async Task<ServiceResult<IEnumerable<BlogPostDto>>> GetAllPostsAsync()
    {
        try
        {
            var postRepo = _unitOfWork.Repository<BlogPost>();
            var posts = await postRepo.GetAllAsync();
            return ServiceResult<IEnumerable<BlogPostDto>>.SuccessResult(posts.Select(p => MapToDto(p)));
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<BlogPostDto>>.FailureResult($"Error retrieving posts: {ex.Message}");
        }
    }

    public async Task<ServiceResult<BlogPostDetailDto>> GetPostBySlugAsync(string slug, Guid? currentUserId = null)
    {
        try
        {
            var post = await _unitOfWork.BlogPosts.GetPostBySlugAsync(slug);
            if (post == null)
                return ServiceResult<BlogPostDetailDto>.FailureResult("Post not found");

            await _unitOfWork.BlogPosts.IncrementViewCountAsync(post.Id);
            return await BuildDetailDtoAsync(post, currentUserId);
        }
        catch (Exception ex)
        {
            return ServiceResult<BlogPostDetailDto>.FailureResult($"Error retrieving post: {ex.Message}");
        }
    }

    public async Task<ServiceResult<BlogPostDetailDto>> GetPostByIdAsync(Guid postId, Guid? currentUserId = null)
    {
        try
        {
            var post = await _unitOfWork.BlogPosts.GetPostWithDetailsAsync(postId);
            if (post == null)
                return ServiceResult<BlogPostDetailDto>.FailureResult("Post not found");

            await _unitOfWork.BlogPosts.IncrementViewCountAsync(postId);
            return await BuildDetailDtoAsync(post, currentUserId);
        }
        catch (Exception ex)
        {
            return ServiceResult<BlogPostDetailDto>.FailureResult($"Error retrieving post: {ex.Message}");
        }
    }

    private async Task<ServiceResult<BlogPostDetailDto>> BuildDetailDtoAsync(BlogPost post, Guid? currentUserId)
    {
        var likeRepo = _unitOfWork.Repository<BlogLike>();
        var likeCount = await likeRepo.CountAsync(l => l.PostId == post.Id);
        var isLiked = currentUserId.HasValue
            && await likeRepo.AnyAsync(l => l.PostId == post.Id && l.UserId == currentUserId.Value);

        var dto = new BlogPostDetailDto
        {
            Id = post.Id,
            Title = post.Title,
            Slug = post.Slug,
            Content = post.Content,
            FeaturedImageUrl = post.FeaturedImageUrl,
            Excerpt = post.Excerpt,
            Status = post.Status,
            AuthorId = post.AuthorId,
            AuthorName = post.Author?.FullName,
            CategoryId = post.CategoryId,
            CategoryName = post.Category?.CategoryName,
            ViewCount = post.ViewCount,
            LikeCount = likeCount,
            PublishedAt = post.PublishedAt,
            CreatedAt = post.CreatedAt,
            Tags = post.BlogPostTags?.Select(t => t.Tag.TagName).ToList() ?? new(),
            IsLikedByCurrentUser = isLiked,
            Comments = post.Comments?
                .Where(c => c.IsApproved && c.ParentCommentId == null)
                .Select(MapCommentToDto)
                .ToList() ?? new()
        };

        return ServiceResult<BlogPostDetailDto>.SuccessResult(dto);
    }

    public async Task<ServiceResult<IEnumerable<BlogPostDto>>> GetPostsByCategoryAsync(Guid categoryId)
    {
        try
        {
            var posts = await _unitOfWork.BlogPosts.GetPostsByCategoryAsync(categoryId);
            return ServiceResult<IEnumerable<BlogPostDto>>.SuccessResult(posts.Select(p => MapToDto(p)));
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<BlogPostDto>>.FailureResult($"Error retrieving posts: {ex.Message}");
        }
    }

    public async Task<ServiceResult<BlogPostDto>> CreatePostAsync(Guid authorId, CreateBlogPostDto dto)
    {
        try
        {
            var slug = GenerateSlug(dto.Title);
            var existing = await _unitOfWork.BlogPosts.GetPostBySlugAsync(slug);
            if (existing != null)
                slug = $"{slug}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

            var post = new BlogPost
            {
                AuthorId = authorId,
                CategoryId = dto.CategoryId,
                Title = dto.Title,
                Slug = slug,
                Content = dto.Content,
                Excerpt = dto.Excerpt,
                FeaturedImageUrl = dto.FeaturedImageUrl,
                Status = dto.Status,
                PublishedAt = dto.Status == "published" ? DateTime.UtcNow : null
            };

            await _unitOfWork.Repository<BlogPost>().AddAsync(post);
            await _unitOfWork.SaveChangesAsync();

            if (dto.Tags?.Any() == true)
                await AttachTagsAsync(post.Id, dto.Tags);

            await _unitOfWork.SaveChangesAsync();

            var created = await _unitOfWork.BlogPosts.GetPostWithDetailsAsync(post.Id);
            return ServiceResult<BlogPostDto>.SuccessResult(MapToDto(created!), "Blog post created");
        }
        catch (Exception ex)
        {
            return ServiceResult<BlogPostDto>.FailureResult($"Error creating post: {ex.Message}");
        }
    }

    public async Task<ServiceResult<BlogPostDto>> UpdatePostAsync(Guid postId, UpdateBlogPostDto dto)
    {
        try
        {
            var post = await _unitOfWork.BlogPosts.GetPostWithDetailsAsync(postId);
            if (post == null)
                return ServiceResult<BlogPostDto>.FailureResult("Post not found");

            if (dto.Title != null)
            {
                post.Title = dto.Title;
                post.Slug = GenerateSlug(dto.Title);
            }
            if (dto.Content != null) post.Content = dto.Content;
            if (dto.CategoryId.HasValue) post.CategoryId = dto.CategoryId;
            if (dto.Excerpt != null) post.Excerpt = dto.Excerpt;
            if (dto.FeaturedImageUrl != null) post.FeaturedImageUrl = dto.FeaturedImageUrl;
            if (dto.Status != null)
            {
                if (dto.Status == "published" && post.Status != "published")
                    post.PublishedAt = DateTime.UtcNow;
                post.Status = dto.Status;
            }

            await _unitOfWork.Repository<BlogPost>().UpdateAsync(post);

            if (dto.Tags != null)
            {
                var tagRelRepo = _unitOfWork.Repository<BlogPostTag>();
                var oldTags = await tagRelRepo.FindAsync(t => t.PostId == postId);
                await tagRelRepo.DeleteRangeAsync(oldTags);
                await AttachTagsAsync(postId, dto.Tags);
            }

            await _unitOfWork.SaveChangesAsync();

            var updated = await _unitOfWork.BlogPosts.GetPostWithDetailsAsync(postId);
            return ServiceResult<BlogPostDto>.SuccessResult(MapToDto(updated!), "Blog post updated");
        }
        catch (Exception ex)
        {
            return ServiceResult<BlogPostDto>.FailureResult($"Error updating post: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> DeletePostAsync(Guid postId)
    {
        try
        {
            var post = await _unitOfWork.Repository<BlogPost>().GetByIdAsync(postId);
            if (post == null)
                return ServiceResult<bool>.FailureResult("Post not found");

            await _unitOfWork.Repository<BlogPost>().DeleteAsync(post);
            await _unitOfWork.SaveChangesAsync();
            return ServiceResult<bool>.SuccessResult(true, "Post deleted");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult($"Error deleting post: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> PublishPostAsync(Guid postId)
    {
        try
        {
            var post = await _unitOfWork.Repository<BlogPost>().GetByIdAsync(postId);
            if (post == null)
                return ServiceResult<bool>.FailureResult("Post not found");

            post.Status = "published";
            post.PublishedAt = DateTime.UtcNow;
            await _unitOfWork.Repository<BlogPost>().UpdateAsync(post);
            await _unitOfWork.SaveChangesAsync();
            return ServiceResult<bool>.SuccessResult(true, "Post published");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult($"Error publishing post: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> UnpublishPostAsync(Guid postId)
    {
        try
        {
            var post = await _unitOfWork.Repository<BlogPost>().GetByIdAsync(postId);
            if (post == null)
                return ServiceResult<bool>.FailureResult("Post not found");

            post.Status = "draft";
            await _unitOfWork.Repository<BlogPost>().UpdateAsync(post);
            await _unitOfWork.SaveChangesAsync();
            return ServiceResult<bool>.SuccessResult(true, "Post unpublished");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult($"Error unpublishing post: {ex.Message}");
        }
    }

    public async Task<ServiceResult<BlogCommentDto>> AddCommentAsync(Guid userId, Guid postId, CreateCommentDto dto)
    {
        try
        {
            var postExists = await _unitOfWork.Repository<BlogPost>()
                .AnyAsync(p => p.Id == postId && p.Status == "published");
            if (!postExists)
                return ServiceResult<BlogCommentDto>.FailureResult("Post not found");

            var comment = new BlogComment
            {
                PostId = postId,
                UserId = userId,
                ParentCommentId = dto.ParentCommentId,
                CommentText = dto.CommentText,
                IsApproved = false
            };

            await _unitOfWork.Repository<BlogComment>().AddAsync(comment);
            await _unitOfWork.SaveChangesAsync();

            var resultDto = new BlogCommentDto
            {
                Id = comment.Id,
                PostId = comment.PostId,
                UserId = comment.UserId,
                ParentCommentId = comment.ParentCommentId,
                CommentText = comment.CommentText,
                IsApproved = comment.IsApproved,
                CreatedAt = comment.CreatedAt
            };

            return ServiceResult<BlogCommentDto>.SuccessResult(resultDto, "Comment submitted for approval");
        }
        catch (Exception ex)
        {
            return ServiceResult<BlogCommentDto>.FailureResult($"Error adding comment: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> ApproveCommentAsync(Guid commentId)
    {
        try
        {
            var commentRepo = _unitOfWork.Repository<BlogComment>();
            var comment = await commentRepo.GetByIdAsync(commentId);
            if (comment == null)
                return ServiceResult<bool>.FailureResult("Comment not found");

            comment.IsApproved = true;
            await commentRepo.UpdateAsync(comment);
            await _unitOfWork.SaveChangesAsync();
            return ServiceResult<bool>.SuccessResult(true, "Comment approved");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult($"Error approving comment: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> DeleteCommentAsync(Guid commentId)
    {
        try
        {
            var commentRepo = _unitOfWork.Repository<BlogComment>();
            var comment = await commentRepo.GetByIdAsync(commentId);
            if (comment == null)
                return ServiceResult<bool>.FailureResult("Comment not found");

            await commentRepo.DeleteAsync(comment);
            await _unitOfWork.SaveChangesAsync();
            return ServiceResult<bool>.SuccessResult(true, "Comment deleted");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult($"Error deleting comment: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> ToggleLikeAsync(Guid userId, Guid postId)
    {
        try
        {
            var likeRepo = _unitOfWork.Repository<BlogLike>();
            var existing = await likeRepo.FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
            if (existing != null)
            {
                await likeRepo.DeleteAsync(existing);
                await _unitOfWork.SaveChangesAsync();
                return ServiceResult<bool>.SuccessResult(false, "Post unliked");
            }

            await likeRepo.AddAsync(new BlogLike { PostId = postId, UserId = userId });
            await _unitOfWork.SaveChangesAsync();
            return ServiceResult<bool>.SuccessResult(true, "Post liked");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult($"Error toggling like: {ex.Message}");
        }
    }

    public async Task<ServiceResult<IEnumerable<BlogCategoryDto>>> GetCategoriesAsync()
    {
        try
        {
            var categories = await _unitOfWork.Repository<BlogCategory>().GetAllAsync();
            var dtos = categories.Select(c => new BlogCategoryDto
            {
                Id = c.Id,
                CategoryName = c.CategoryName,
                Slug = c.Slug,
                Description = c.Description,
                PostCount = c.BlogPosts?.Count(p => p.Status == "published") ?? 0
            });
            return ServiceResult<IEnumerable<BlogCategoryDto>>.SuccessResult(dtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<BlogCategoryDto>>.FailureResult($"Error retrieving categories: {ex.Message}");
        }
    }

    public async Task<ServiceResult<BlogCategoryDto>> CreateCategoryAsync(CreateBlogCategoryDto dto)
    {
        try
        {
            var category = new BlogCategory
            {
                CategoryName = dto.CategoryName,
                Slug = GenerateSlug(dto.CategoryName),
                Description = dto.Description
            };

            await _unitOfWork.Repository<BlogCategory>().AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return ServiceResult<BlogCategoryDto>.SuccessResult(new BlogCategoryDto
            {
                Id = category.Id,
                CategoryName = category.CategoryName,
                Slug = category.Slug,
                Description = category.Description
            }, "Category created");
        }
        catch (Exception ex)
        {
            return ServiceResult<BlogCategoryDto>.FailureResult($"Error creating category: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> DeleteCategoryAsync(Guid categoryId)
    {
        try
        {
            var category = await _unitOfWork.Repository<BlogCategory>().GetByIdAsync(categoryId);
            if (category == null)
                return ServiceResult<bool>.FailureResult("Category not found");

            await _unitOfWork.Repository<BlogCategory>().DeleteAsync(category);
            await _unitOfWork.SaveChangesAsync();
            return ServiceResult<bool>.SuccessResult(true, "Category deleted");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult($"Error deleting category: {ex.Message}");
        }
    }

    private async Task AttachTagsAsync(Guid postId, List<string> tagNames)
    {
        var tagRepo = _unitOfWork.Repository<Tag>();
        var tagRelRepo = _unitOfWork.Repository<BlogPostTag>();

        foreach (var tagName in tagNames.Where(t => !string.IsNullOrWhiteSpace(t)))
        {
            var slug = GenerateSlug(tagName);
            var tag = await tagRepo.FirstOrDefaultAsync(t => t.Slug == slug);
            if (tag == null)
            {
                tag = new Tag { TagName = tagName.Trim(), Slug = slug };
                await tagRepo.AddAsync(tag);
                await _unitOfWork.SaveChangesAsync();
            }
            await tagRelRepo.AddAsync(new BlogPostTag { PostId = postId, TagId = tag.Id });
        }
    }
}
