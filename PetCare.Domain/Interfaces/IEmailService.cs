namespace PetCare.Domain.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlBody);
    Task SendWelcomeEmailAsync(string to, string userName);
    Task SendSubscriptionConfirmationAsync(string to, string userName, string planName, DateTime expiryDate);
    Task SendAIAnalysisSummaryAsync(
        string to,
        string userName,
        string petName,
        string analysisType,
        string aiSummary,
        decimal? confidenceScore,
        DateTime createdAt);
}
