using PetCare.Application.Common;
using PetCare.Domain.Entities;

namespace PetCare.Application.Services.Interfaces;

public interface INotificationService
{
    Task<ServiceResult<Notification>> CreateNotificationAsync(Guid userId, string title, string message, string? type = null, string? relatedEntityId = null);
    Task<ServiceResult<List<Notification>>> GetUserNotificationsAsync(Guid userId, bool unreadOnly = false);
    Task<ServiceResult<Notification>> MarkAsReadAsync(Guid notificationId, Guid userId);
    Task<ServiceResult<bool>> MarkAllAsReadAsync(Guid userId);
    Task<ServiceResult<bool>> DeleteNotificationAsync(Guid notificationId, Guid userId);
    Task<ServiceResult<int>> GetUnreadCountAsync(Guid userId);
}
