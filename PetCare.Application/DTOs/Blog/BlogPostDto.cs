namespace PetCare.Application.DTOs.Blog;

public class BlogPostDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? FeaturedImageUrl { get; set; }
    public string? Excerpt { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? AuthorId { get; set; }
    public string? AuthorName { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> Tags { get; set; } = new();
}

public class BlogPostDetailDto : BlogPostDto
{
    public bool IsLikedByCurrentUser { get; set; }
    public List<BlogCommentDto> Comments { get; set; } = new();
}

public class CreateBlogPostDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid? CategoryId { get; set; }
    public string? Excerpt { get; set; }
    public string? FeaturedImageUrl { get; set; }
    public string Status { get; set; } = "draft";
    public List<string> Tags { get; set; } = new();
}

public class UpdateBlogPostDto
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public Guid? CategoryId { get; set; }
    public string? Excerpt { get; set; }
    public string? FeaturedImageUrl { get; set; }
    public string? Status { get; set; }
    public List<string>? Tags { get; set; }
}

public class BlogCategoryDto
{
    public Guid Id { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int PostCount { get; set; }
}

public class CreateBlogCategoryDto
{
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class BlogCommentDto
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public Guid? ParentCommentId { get; set; }
    public string CommentText { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<BlogCommentDto> Replies { get; set; } = new();
}

public class CreateCommentDto
{
    public string CommentText { get; set; } = string.Empty;
    public Guid? ParentCommentId { get; set; }
}
