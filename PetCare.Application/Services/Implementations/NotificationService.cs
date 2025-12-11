using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetCare.Application.Common;
using PetCare.Application.Services.Interfaces;
using PetCare.Domain.Entities;
using PetCare.Infrastructure.Data;

namespace PetCare.Application.Services.Implementations;

public class NotificationService : INotificationService
{
    private readonly PetCareDbContext _context;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(PetCareDbContext context, ILogger<NotificationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ServiceResult<Notification>> CreateNotificationAsync(Guid userId, string title, string message, string? type = null, string? relatedEntityId = null)
    {
        try
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
                return ServiceResult<Notification>.FailureResult("User not found");

            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                NotificationType = type ?? "General",
                LinkUrl = relatedEntityId,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return ServiceResult<Notification>.SuccessResult(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating notification for user: {UserId}", userId);
            return ServiceResult<Notification>.FailureResult($"Error creating notification: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<Notification>>> GetUserNotificationsAsync(Guid userId, bool unreadOnly = false)
    {
        try
        {
            var query = _context.Notifications
                .Where(n => n.UserId == userId);

            if (unreadOnly)
                query = query.Where(n => !n.IsRead);

            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return ServiceResult<List<Notification>>.SuccessResult(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notifications for user: {UserId}", userId);
            return ServiceResult<List<Notification>>.FailureResult($"Error retrieving notifications: {ex.Message}");
        }
    }

    public async Task<ServiceResult<Notification>> MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        try
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null)
                return ServiceResult<Notification>.FailureResult("Notification not found");

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return ServiceResult<Notification>.SuccessResult(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification as read: {NotificationId}", notificationId);
            return ServiceResult<Notification>.FailureResult($"Error updating notification: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> MarkAllAsReadAsync(Guid userId)
    {
        try
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();

            return ServiceResult<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read for user: {UserId}", userId);
            return ServiceResult<bool>.FailureResult($"Error updating notifications: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> DeleteNotificationAsync(Guid notificationId, Guid userId)
    {
        try
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null)
                return ServiceResult<bool>.FailureResult("Notification not found");

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return ServiceResult<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting notification: {NotificationId}", notificationId);
            return ServiceResult<bool>.FailureResult($"Error deleting notification: {ex.Message}");
        }
    }

    public async Task<ServiceResult<int>> GetUnreadCountAsync(Guid userId)
    {
        try
        {
            var count = await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);

            return ServiceResult<int>.SuccessResult(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread count for user: {UserId}", userId);
            return ServiceResult<int>.FailureResult($"Error getting unread count: {ex.Message}");
        }
    }
}


