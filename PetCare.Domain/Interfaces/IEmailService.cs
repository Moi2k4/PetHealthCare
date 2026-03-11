namespace PetCare.Domain.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlBody);
    Task SendWelcomeEmailAsync(string to, string userName);
    Task SendSubscriptionConfirmationAsync(string to, string userName, string planName, DateTime expiryDate);
}
