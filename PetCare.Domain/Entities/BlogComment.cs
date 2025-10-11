namespace PetCare.Domain.Entities;

using PetCare.Domain.Common;

public class BlogComment : AuditableEntity
{
    public Guid PostId { get; set; }
    public Guid? UserId { get; set; }
    public Guid? ParentCommentId { get; set; }
    public string CommentText { get; set; } = string.Empty;
    public bool IsApproved { get; set; } = false;

    // Navigation properties
    public virtual BlogPost Post { get; set; } = null!;
    public virtual User? User { get; set; }
    public virtual BlogComment? ParentComment { get; set; }
    public virtual ICollection<BlogComment> Replies { get; set; } = new List<BlogComment>();
}
