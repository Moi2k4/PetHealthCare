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
    public string? AuthorName { get; set; }
    public string? CategoryName { get; set; }
    public int ViewCount { get; set; }
    public DateTime? PublishedAt { get; set; }
    public List<string> Tags { get; set; } = new();
}
