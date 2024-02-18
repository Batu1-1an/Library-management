using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Services
{
    public class ReminderService : BackgroundService
    {
        private readonly ILogger<ReminderService> _logger;
        private readonly IServiceProvider _services;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(12); // Check twice per day

        public ReminderService(
            ILogger<ReminderService> logger,
            IServiceProvider services)
        {
            _logger = logger;
            _services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndSendReminders();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while sending reminders");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private decimal CalculatePenalty(int daysLate)
        {
            if (daysLate <= 7)
                return daysLate * 0.50m;  // $0.50 per day for first week
            else if (daysLate <= 14)
                return (7 * 0.50m) + ((daysLate - 7) * 1.00m);  // $1.00 per day for second week
            else
                return (7 * 0.50m) + (7 * 1.00m) + ((daysLate - 14) * 2.00m);  // $2.00 per day after two weeks
        }

        private async Task SendReminder(BorrowRecord borrow, IEmailService emailService, string reminderType)
        {
            try
            {
                await emailService.SendBorrowingDeadlineReminderAsync(
                    borrow.Member.Email,
                    borrow.Book.Title,
                    borrow.DueDate,
                    reminderType
                );

                borrow.Notes = $"{reminderType} reminder sent on {DateTime.Now:g}";
                
                _logger.LogInformation(
                    "Sent {ReminderType} reminder for book {BookTitle} to {MemberEmail}",
                    reminderType,
                    borrow.Book.Title,
                    borrow.Member.Email
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send {ReminderType} reminder for book {BookTitle} to {MemberEmail}",
                    reminderType,
                    borrow.Book.Title,
                    borrow.Member.Email
                );
            }
        }

        private async Task CheckAndSendReminders()
        {
            using var scope = _services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<LibraryContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var today = DateTime.Now.Date;
            var oneWeekFromNow = today.AddDays(7);
            var threeDaysFromNow = today.AddDays(3);
            var oneDayFromNow = today.AddDays(1);

            // Get all active borrows that need reminders
            var activeLoans = await context.BorrowRecords
                .Include(b => b.Book)
                .Include(b => b.Member)
                .Where(b => !b.IsReturned)
                .ToListAsync();

            foreach (var borrow in activeLoans)
            {
                if (borrow.DueDate.Date == oneWeekFromNow)
                {
                    await SendReminder(borrow, emailService, "One Week");
                }
                else if (borrow.DueDate.Date == threeDaysFromNow)
                {
                    await SendReminder(borrow, emailService, "Three Days");
                }
                else if (borrow.DueDate.Date == oneDayFromNow)
                {
                    await SendReminder(borrow, emailService, "One Day");
                }
                else if (borrow.DueDate.Date < today && 
                        (!borrow.Notes?.Contains("Penalty notification") ?? true))
                {
                    try
                    {
                        var daysLate = (today - borrow.DueDate.Date).Days;
                        var penaltyAmount = CalculatePenalty(daysLate);

                        await emailService.SendPenaltyNotificationAsync(
                            borrow.Member.Email,
                            borrow.Book.Title,
                            daysLate,
                            penaltyAmount
                        );

                        borrow.Notes = $"Penalty notification sent on {DateTime.Now:g}";

                        _logger.LogInformation(
                            "Sent penalty notification for book {BookTitle} to {MemberEmail}",
                            borrow.Book.Title,
                            borrow.Member.Email
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Failed to send penalty notification for book {BookTitle} to {MemberEmail}",
                            borrow.Book.Title,
                            borrow.Member.Email
                        );
                    }
                }
            }

            await context.SaveChangesAsync();
        }
    }
} 