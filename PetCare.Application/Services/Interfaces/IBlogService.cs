using PetCare.Application.Common;
using PetCare.Application.DTOs.Blog;

namespace PetCare.Application.Services.Interfaces;

public interface IBlogService
{
    // Posts - public
    Task<ServiceResult<IEnumerable<BlogPostDto>>> GetPublishedPostsAsync();
    Task<ServiceResult<BlogPostDetailDto>> GetPostBySlugAsync(string slug, Guid? currentUserId = null);
    Task<ServiceResult<BlogPostDetailDto>> GetPostByIdAsync(Guid postId, Guid? currentUserId = null);
    Task<ServiceResult<IEnumerable<BlogPostDto>>> GetPostsByCategoryAsync(Guid categoryId);

    // Posts - admin/author
    Task<ServiceResult<IEnumerable<BlogPostDto>>> GetAllPostsAsync();
    Task<ServiceResult<BlogPostDto>> CreatePostAsync(Guid authorId, CreateBlogPostDto dto);
    Task<ServiceResult<BlogPostDto>> UpdatePostAsync(Guid postId, UpdateBlogPostDto dto);
    Task<ServiceResult<bool>> DeletePostAsync(Guid postId);
    Task<ServiceResult<bool>> PublishPostAsync(Guid postId);
    Task<ServiceResult<bool>> UnpublishPostAsync(Guid postId);

    // Comments
    Task<ServiceResult<BlogCommentDto>> AddCommentAsync(Guid userId, Guid postId, CreateCommentDto dto);
    Task<ServiceResult<bool>> ApproveCommentAsync(Guid commentId);
    Task<ServiceResult<bool>> DeleteCommentAsync(Guid commentId);

    // Likes
    Task<ServiceResult<bool>> ToggleLikeAsync(Guid userId, Guid postId);

    // Categories
    Task<ServiceResult<IEnumerable<BlogCategoryDto>>> GetCategoriesAsync();
    Task<ServiceResult<BlogCategoryDto>> CreateCategoryAsync(CreateBlogCategoryDto dto);
    Task<ServiceResult<bool>> DeleteCategoryAsync(Guid categoryId);
}
