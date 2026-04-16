using Microsoft.EntityFrameworkCore;
using PetCare.Domain.Entities;
using PetCare.Domain.Interfaces;
using PetCare.Infrastructure.Data;
using System.Text.Json;

namespace PetCare.API.Services;

public class VaccinationReminderHostedService : BackgroundService
{
    private const string ReminderType = "vaccination_email_overdue";

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<VaccinationReminderHostedService> _logger;
    private readonly IConfiguration _configuration;

    public VaccinationReminderHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<VaccinationReminderHostedService> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalMinutes = _configuration.GetValue<int?>("VaccinationReminder:IntervalMinutes") ?? 60;
        if (intervalMinutes < 10)
        {
            intervalMinutes = 10;
        }

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(intervalMinutes));

        await ProcessOnceAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ProcessOnceAsync(stoppingToken);
        }
    }

    private async Task ProcessOnceAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<PetCareDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var utcToday = DateTime.UtcNow.Date;
            var utcTomorrow = utcToday.AddDays(1);
            var frontendBaseUrl = (_configuration["Frontend:BaseUrl"]
                ?? Environment.GetEnvironmentVariable("FRONTEND_BASE_URL")
                ?? "https://pettsuba.live").TrimEnd('/');

            var overdueVaccinations = await db.Vaccinations
                .AsNoTracking()
                .Include(v => v.Pet)
                    .ThenInclude(p => p.User)
                .Where(v => v.NextDueDate.HasValue
                            && v.NextDueDate.Value.Date <= utcToday
                            && v.Pet.IsActive
                            && v.Pet.User.IsActive
                            && !string.IsNullOrWhiteSpace(v.Pet.User.Email))
                .ToListAsync(cancellationToken);

            if (overdueVaccinations.Count == 0)
            {
                return;
            }

            var sentTodayKeysList = await db.HealthReminders
                .AsNoTracking()
                .Where(r => r.ReminderType == ReminderType
                            && r.ReminderDate >= utcToday
                            && r.ReminderDate < utcTomorrow
                            && r.Notes != null)
                .Select(r => r.Notes!)
                .ToListAsync(cancellationToken);

            var sentTodayKeys = sentTodayKeysList.ToHashSet();

            var actionLogs = await db.HealthReminders
                .AsNoTracking()
                .Where(r => r.ReminderType == "vaccination_user_action"
                            && r.Notes != null
                            && r.ReminderDate >= utcToday.AddDays(-30))
                .OrderByDescending(r => r.ReminderDate)
                .Select(r => new { r.ReminderDate, r.Notes })
                .ToListAsync(cancellationToken);

            var latestActionByVaccination = new Dictionary<Guid, (string Status, DateTime At)>();
            foreach (var log in actionLogs)
            {
                if (!TryExtractVaccinationAction(log.Notes!, out var vaccinationId, out var actionStatus))
                {
                    continue;
                }

                if (!latestActionByVaccination.ContainsKey(vaccinationId))
                {
                    latestActionByVaccination[vaccinationId] = (actionStatus, log.ReminderDate);
                }
            }

            var reminderLogs = new List<HealthReminder>();

            foreach (var vaccination in overdueVaccinations)
            {
                var key = BuildReminderKey(vaccination.Id);
                if (sentTodayKeys.Contains(key))
                {
                    continue;
                }

                if (latestActionByVaccination.TryGetValue(vaccination.Id, out var latestAction))
                {
                    if (latestAction.Status == "done" || latestAction.Status == "booked")
                    {
                        continue;
                    }

                    if (latestAction.Status == "remind_later" && latestAction.At >= utcToday.AddDays(-2))
                    {
                        continue;
                    }
                }

                var user = vaccination.Pet.User;
                var dueDate = vaccination.NextDueDate!.Value.Date;
                var overdueDays = (utcToday - dueDate).Days;

                var subject = $"[PetCare] Nhac nho tiem phong qua han cho {vaccination.Pet.PetName}";
                var body = BuildOverdueEmailBody(
                    user.FullName,
                    vaccination.Pet.PetName,
                    vaccination.VaccineName,
                    dueDate,
                    overdueDays,
                    BuildBookingDeepLink(frontendBaseUrl, vaccination.PetId, vaccination.Id, vaccination.VaccineName));

                try
                {
                    await emailService.SendEmailAsync(user.Email, subject, body);

                    reminderLogs.Add(new HealthReminder
                    {
                        Id = Guid.NewGuid(),
                        PetId = vaccination.PetId,
                        ReminderType = ReminderType,
                        ReminderTitle = $"Overdue vaccine email: {vaccination.VaccineName}",
                        ReminderDate = DateTime.UtcNow,
                        IsCompleted = true,
                        Notes = key
                    });

                    sentTodayKeys.Add(key);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Failed sending overdue vaccine reminder email to {Email} for vaccination {VaccinationId}",
                        user.Email,
                        vaccination.Id);
                }
            }

            if (reminderLogs.Count > 0)
            {
                await db.HealthReminders.AddRangeAsync(reminderLogs, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Sent {Count} overdue vaccination reminder email(s)", reminderLogs.Count);
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore cancellation during shutdown.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while processing vaccination reminder emails");
        }
    }

    private static string BuildReminderKey(Guid vaccinationId) => $"vaccination:{vaccinationId}";

    private static bool TryExtractVaccinationAction(string notes, out Guid vaccinationId, out string status)
    {
        vaccinationId = Guid.Empty;
        status = string.Empty;

        try
        {
            using var doc = JsonDocument.Parse(notes);
            var root = doc.RootElement;

            var vaccinationRaw = root.TryGetProperty("vaccinationId", out var idElement)
                ? idElement.GetString()
                : null;

            var statusRaw = root.TryGetProperty("status", out var statusElement)
                ? statusElement.GetString()
                : null;

            if (!Guid.TryParse(vaccinationRaw, out vaccinationId) || string.IsNullOrWhiteSpace(statusRaw))
            {
                return false;
            }

            status = statusRaw.Trim().ToLowerInvariant();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string BuildBookingDeepLink(string frontendBaseUrl, Guid petId, Guid vaccinationId, string vaccineName)
    {
        var encodedVaccine = Uri.EscapeDataString(vaccineName);
        return $"{frontendBaseUrl}/dat-lich?source=vaccination_reminder&petId={petId}&vaccinationId={vaccinationId}&vaccine={encodedVaccine}";
    }

    private static string BuildOverdueEmailBody(
        string userName,
        string petName,
        string vaccineName,
        DateTime dueDate,
        int overdueDays,
        string bookingUrl)
    {
        return $"""
            <div style=\"font-family:Arial,sans-serif;line-height:1.6;color:#222\">
                <h2 style=\"margin-bottom:8px\">Nhac nho tiem phong qua han</h2>
                <p>Xin chao {userName},</p>
                <p>
                    Vaccine <strong>{vaccineName}</strong> cua be <strong>{petName}</strong>
                    da qua han tu ngay <strong>{dueDate:dd/MM/yyyy}</strong>
                    ({overdueDays} ngay).
                </p>
                <p>
                    Ban nen dat lich tiem nhac lai som de dam bao suc khoe cho be.
                </p>
                <p style=\"margin-top:16px\">
                    <a href=\"{bookingUrl}\" style=\"display:inline-block;background:#0f766e;color:#fff;text-decoration:none;padding:10px 16px;border-radius:8px;font-weight:600\">
                        Dat lich tiem ngay
                    </a>
                </p>
                <p style=\"font-size:13px;color:#666\">
                    Khi mo trang dat lich, ban co the danh dau: da dat lich, da tiem xong, hoac nhac lai sau.
                </p>
                <p style=\"margin-top:24px\">PetCare Team</p>
            </div>
            """;
    }
}
