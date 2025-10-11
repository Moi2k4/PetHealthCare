namespace PetCare.Domain.Entities;

using PetCare.Domain.Common;

public class Tag : BaseEntity
{
    public string TagName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    // Navigation properties
    public virtual ICollection<BlogPostTag> BlogPostTags { get; set; } = new List<BlogPostTag>();
}
