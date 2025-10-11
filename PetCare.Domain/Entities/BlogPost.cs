namespace PetCare.Domain.Entities;

using PetCare.Domain.Common;

public class BlogPost : AuditableEntity
{
    public Guid? AuthorId { get; set; }
    public Guid? CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? FeaturedImageUrl { get; set; }
    public string? Excerpt { get; set; }
    public string Status { get; set; } = "draft";
    public int ViewCount { get; set; } = 0;
    public DateTime? PublishedAt { get; set; }

    // Navigation properties
    public virtual User? Author { get; set; }
    public virtual BlogCategory? Category { get; set; }
    public virtual ICollection<BlogPostTag> BlogPostTags { get; set; } = new List<BlogPostTag>();
    public virtual ICollection<BlogComment> Comments { get; set; } = new List<BlogComment>();
    public virtual ICollection<BlogLike> Likes { get; set; } = new List<BlogLike>();
}
