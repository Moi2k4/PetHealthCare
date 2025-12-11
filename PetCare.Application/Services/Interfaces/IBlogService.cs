using PetCare.Application.Common;
using PetCare.Domain.Entities;

namespace PetCare.Application.Services.Interfaces;

public interface IBlogService
{
    Task<ServiceResult<BlogPost>> CreateBlogPostAsync(string title, string content, Guid authorId, Guid? categoryId = null, List<string>? tags = null);
    Task<ServiceResult<BlogPost>> UpdateBlogPostAsync(Guid id, string? title = null, string? content = null, Guid? categoryId = null);
    Task<ServiceResult<bool>> DeleteBlogPostAsync(Guid id, Guid authorId);
    Task<ServiceResult<BlogPost>> GetBlogPostByIdAsync(Guid id);
    Task<ServiceResult<BlogPost>> GetBlogPostBySlugAsync(string slug);
    Task<ServiceResult<PagedResult<BlogPost>>> GetAllBlogPostsAsync(int page = 1, int pageSize = 10, bool publishedOnly = true);
    Task<ServiceResult<PagedResult<BlogPost>>> GetBlogPostsByCategoryAsync(Guid categoryId, int page = 1, int pageSize = 10);
    Task<ServiceResult<PagedResult<BlogPost>>> GetBlogPostsByTagAsync(string tag, int page = 1, int pageSize = 10);
    Task<ServiceResult<BlogPost>> PublishBlogPostAsync(Guid id, Guid authorId);
    Task<ServiceResult<BlogPost>> UnpublishBlogPostAsync(Guid id, Guid authorId);
    Task<ServiceResult<BlogComment>> AddCommentAsync(Guid postId, Guid userId, string content, Guid? parentCommentId = null);
    Task<ServiceResult<bool>> DeleteCommentAsync(Guid commentId, Guid userId);
    Task<ServiceResult<BlogLike>> LikeBlogPostAsync(Guid postId, Guid userId);
    Task<ServiceResult<bool>> UnlikeBlogPostAsync(Guid postId, Guid userId);
    Task<ServiceResult<List<BlogCategory>>> GetAllCategoriesAsync();
    Task<ServiceResult<List<Tag>>> GetAllTagsAsync();
}
