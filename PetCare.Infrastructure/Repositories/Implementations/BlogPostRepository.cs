using Microsoft.EntityFrameworkCore;
using PetCare.Domain.Entities;
using PetCare.Infrastructure.Data;
using PetCare.Infrastructure.Repositories.Interfaces;

namespace PetCare.Infrastructure.Repositories.Implementations;

public class BlogPostRepository : GenericRepository<BlogPost>, IBlogPostRepository
{
    public BlogPostRepository(PetCareDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<BlogPost>> GetPublishedPostsAsync()
    {
        return await _dbSet
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Include(p => p.BlogPostTags)
                .ThenInclude(pt => pt.Tag)
            .Where(p => p.Status == "published")
            .OrderByDescending(p => p.PublishedAt)
            .ToListAsync();
    }

    public async Task<BlogPost?> GetPostBySlugAsync(string slug)
    {
        return await _dbSet
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Include(p => p.BlogPostTags)
                .ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Slug == slug);
    }

    public async Task<BlogPost?> GetPostWithDetailsAsync(Guid postId)
    {
        return await _dbSet
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Include(p => p.BlogPostTags)
                .ThenInclude(pt => pt.Tag)
            .Include(p => p.Comments.Where(c => c.IsApproved))
                .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(p => p.Id == postId);
    }

    public async Task<IEnumerable<BlogPost>> GetPostsByCategoryAsync(Guid categoryId)
    {
        return await _dbSet
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Where(p => p.CategoryId == categoryId && p.Status == "published")
            .OrderByDescending(p => p.PublishedAt)
            .ToListAsync();
    }

    public async Task IncrementViewCountAsync(Guid postId)
    {
        var post = await _dbSet.FindAsync(postId);
        if (post != null)
        {
            post.ViewCount++;
            await _context.SaveChangesAsync();
        }
    }
}
