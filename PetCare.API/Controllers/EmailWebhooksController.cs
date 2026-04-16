using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCare.Domain.Entities;
using PetCare.Infrastructure.Data;

namespace PetCare.API.Controllers;

[ApiController]
[Route("api/webhooks/email")]
public class EmailWebhooksController : ControllerBase
{
    private readonly PetCareDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailWebhooksController> _logger;

    public EmailWebhooksController(
        PetCareDbContext dbContext,
        IConfiguration configuration,
        ILogger<EmailWebhooksController> logger)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("resend")]
    [AllowAnonymous]
    public async Task<IActionResult> ReceiveResendWebhook([FromBody] JsonElement payload)
    {
        if (!IsAuthorizedWebhookRequest())
        {
            return Unauthorized(new { success = false, message = "Invalid webhook authorization" });
        }

        try
        {
            var eventType = TryGetString(payload, "type") ?? "unknown";

            var data = payload.TryGetProperty("data", out var dataNode)
                ? dataNode
                : default;

            var emailId = TryGetString(data, "email_id")
                ?? TryGetString(data, "id");

            var recipient = ExtractRecipient(data);
            var clickedUrl = ExtractClickedUrl(data);
            var eventTimestamp = ParseDateTime(TryGetString(payload, "created_at"))
                ?? ParseDateTime(TryGetString(data, "created_at"));

            var trackingEvent = new EmailTrackingEvent
            {
                Id = Guid.NewGuid(),
                EventType = eventType,
                EmailId = emailId,
                Recipient = recipient,
                ClickedUrl = clickedUrl,
                EventTimestamp = eventTimestamp,
                PayloadJson = payload.GetRawText(),
                CreatedAt = DateTime.UtcNow
            };

            await _dbContext.EmailTrackingEvents.AddAsync(trackingEvent);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "Captured Resend webhook event {EventType} for {Recipient} (EmailId={EmailId})",
                eventType,
                recipient,
                emailId);

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process Resend webhook payload");
            return BadRequest(new { success = false, message = "Invalid webhook payload" });
        }
    }

    private bool IsAuthorizedWebhookRequest()
    {
        var secret = Environment.GetEnvironmentVariable("RESEND_WEBHOOK_SECRET")
            ?? _configuration["Resend:WebhookSecret"];

        if (string.IsNullOrWhiteSpace(secret))
        {
            // If not configured, accept payloads to avoid blocking non-prod environments.
            return true;
        }

        var auth = Request.Headers.Authorization.ToString();
        if (!string.IsNullOrWhiteSpace(auth) && auth.Equals($"Bearer {secret}", StringComparison.Ordinal))
        {
            return true;
        }

        var resendSignature = Request.Headers["Resend-Signature"].ToString();
        if (!string.IsNullOrWhiteSpace(resendSignature) && resendSignature.Equals(secret, StringComparison.Ordinal))
        {
            return true;
        }

        return false;
    }

    private static string? ExtractRecipient(JsonElement data)
    {
        if (!data.TryGetProperty("to", out var toNode))
        {
            return null;
        }

        if (toNode.ValueKind == JsonValueKind.String)
        {
            return toNode.GetString();
        }

        if (toNode.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in toNode.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                {
                    return item.GetString();
                }
            }
        }

        return null;
    }

    private static string? ExtractClickedUrl(JsonElement data)
    {
        if (data.TryGetProperty("click", out var clickNode))
        {
            var fromLink = TryGetString(clickNode, "link") ?? TryGetString(clickNode, "url");
            if (!string.IsNullOrWhiteSpace(fromLink))
            {
                return fromLink;
            }
        }

        return TryGetString(data, "url");
    }

    private static DateTime? ParseDateTime(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        if (DateTime.TryParse(raw, out var parsed))
        {
            return parsed;
        }

        return null;
    }

    private static string? TryGetString(JsonElement node, string propertyName)
    {
        if (node.ValueKind == JsonValueKind.Undefined || node.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (!node.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : property.GetRawText();
    }
}
