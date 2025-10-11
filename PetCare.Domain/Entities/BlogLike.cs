namespace PetCare.Domain.Entities;

using PetCare.Domain.Common;

public class BlogLike : BaseEntity
{
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }

    // Navigation properties
    public virtual BlogPost Post { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
