using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PetCare.Domain.Interfaces;
using Resend;

namespace PetCare.Infrastructure.Services;

public class ResendEmailService : IEmailService
{
    private readonly IResend _resend;
    private readonly string _fromEmail;
    private readonly ILogger<ResendEmailService> _logger;

    public ResendEmailService(IResend resend, IConfiguration configuration, ILogger<ResendEmailService> logger)
    {
        _resend = resend;
        _logger = logger;
        _fromEmail = configuration["Resend:FromEmail"]
            ?? Environment.GetEnvironmentVariable("RESEND_FROM_EMAIL")
            ?? "noreply@pettsuba.live";
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        try
        {
            var message = new EmailMessage
            {
                From = _fromEmail,
                Subject = subject,
                HtmlBody = htmlBody
            };
            message.To.Add(to);

            await _resend.EmailSendAsync(message);
            _logger.LogInformation("Email sent to {To} with subject: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            throw;
        }
    }

    public async Task SendWelcomeEmailAsync(string to, string userName)
    {
        var subject = "Welcome to PetCare!";
        var html = $"""
            <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
                <div style="background-color: #4f9d69; padding: 30px; text-align: center; border-radius: 8px 8px 0 0;">
                    <h1 style="color: white; margin: 0;">Welcome to PetCare! 🐾</h1>
                </div>
                <div style="padding: 30px; background-color: #f9f9f9; border-radius: 0 0 8px 8px;">
                    <p style="font-size: 16px;">Hi <strong>{userName}</strong>,</p>
                    <p style="font-size: 16px;">Thank you for joining PetCare! We're thrilled to have you and your furry friends on board.</p>
                    <p style="font-size: 16px;">You can now:</p>
                    <ul style="font-size: 15px; line-height: 1.8;">
                        <li>Manage your pet profiles</li>
                        <li>Track health records</li>
                        <li>Book services</li>
                        <li>Shop our products</li>
                    </ul>
                    <div style="text-align: center; margin: 30px 0;">
                        <a href="https://pettsuba.live" style="background-color: #4f9d69; color: white; padding: 12px 30px; text-decoration: none; border-radius: 6px; font-size: 16px;">Visit PetCare</a>
                    </div>
                    <p style="color: #888; font-size: 13px; text-align: center;">If you have any questions, reply to this email anytime.</p>
                </div>
            </div>
            """;

        await SendEmailAsync(to, subject, html);
    }

    public async Task SendSubscriptionConfirmationAsync(string to, string userName, string planName, DateTime expiryDate)
    {
        var subject = "Your PetCare Subscription is Active!";
        var html = $"""
            <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
                <div style="background-color: #4f9d69; padding: 30px; text-align: center; border-radius: 8px 8px 0 0;">
                    <h1 style="color: white; margin: 0;">Subscription Confirmed! ✅</h1>
                </div>
                <div style="padding: 30px; background-color: #f9f9f9; border-radius: 0 0 8px 8px;">
                    <p style="font-size: 16px;">Hi <strong>{userName}</strong>,</p>
                    <p style="font-size: 16px;">Your <strong>{planName}</strong> subscription has been activated successfully.</p>
                    <div style="background-color: #fff; border: 1px solid #e0e0e0; border-radius: 6px; padding: 20px; margin: 20px 0;">
                        <p style="margin: 0; font-size: 15px;"><strong>Plan:</strong> {planName}</p>
                        <p style="margin: 8px 0 0; font-size: 15px;"><strong>Valid until:</strong> {expiryDate:MMMM dd, yyyy}</p>
                    </div>
                    <div style="text-align: center; margin: 30px 0;">
                        <a href="https://pettsuba.live" style="background-color: #4f9d69; color: white; padding: 12px 30px; text-decoration: none; border-radius: 6px; font-size: 16px;">Go to Dashboard</a>
                    </div>
                    <p style="color: #888; font-size: 13px; text-align: center;">Thank you for supporting PetCare!</p>
                </div>
            </div>
            """;

        await SendEmailAsync(to, subject, html);
    }
}
