namespace PetCare.Domain.Entities;

using PetCare.Domain.Common;

public class BlogPostTag : BaseEntity
{
    public Guid PostId { get; set; }
    public Guid TagId { get; set; }

    // Navigation properties
    public virtual BlogPost Post { get; set; } = null!;
    public virtual Tag Tag { get; set; } = null!;
}
