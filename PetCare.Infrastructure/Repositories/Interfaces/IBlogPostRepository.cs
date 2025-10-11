using PetCare.Domain.Entities;

namespace PetCare.Infrastructure.Repositories.Interfaces;

public interface IBlogPostRepository : IGenericRepository<BlogPost>
{
    Task<IEnumerable<BlogPost>> GetPublishedPostsAsync();
    Task<BlogPost?> GetPostBySlugAsync(string slug);
    Task<BlogPost?> GetPostWithDetailsAsync(Guid postId);
    Task<IEnumerable<BlogPost>> GetPostsByCategoryAsync(Guid categoryId);
    Task IncrementViewCountAsync(Guid postId);
}
