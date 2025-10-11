namespace PetCare.Domain.Entities;

using PetCare.Domain.Common;

public class BlogCategory : BaseEntity
{
    public string CategoryName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation properties
    public virtual ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
}
