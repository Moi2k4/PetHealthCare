using Microsoft.EntityFrameworkCore;
using PetCare.Domain.Entities;
using PetCare.Domain.Interfaces;
using PetCare.Infrastructure.Data;

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

            var reminderLogs = new List<HealthReminder>();

            foreach (var vaccination in overdueVaccinations)
            {
                var key = BuildReminderKey(vaccination.Id);
                if (sentTodayKeys.Contains(key))
                {
                    continue;
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
                    overdueDays);

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

    private static string BuildOverdueEmailBody(
        string userName,
        string petName,
        string vaccineName,
        DateTime dueDate,
        int overdueDays)
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
                <p style=\"margin-top:24px\">PetCare Team</p>
            </div>
            """;
    }
}
