using FinalProject.BLL.DTOs;
using FinalProject.BLL.Interfaces;
using FinalProject.DAL;
using Quartz;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.API.Services
{
    [DisallowConcurrentExecution]
    public class ReminderJobService : IJob
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ReminderJobService> _logger;

        public ReminderJobService(IServiceProvider serviceProvider, ILogger<ReminderJobService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Starting reminder job execution at {Time}", DateTime.UtcNow);

            using var scope = _serviceProvider.CreateScope();
            var reminderService = scope.ServiceProvider.GetRequiredService<IReminderService>();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var telegramBotService = scope.ServiceProvider.GetRequiredService<TelegramBotService>();

            try
            {
                var pendingReminders = await reminderService.GetPendingRemindersAsync();
                
                foreach (var reminder in pendingReminders)
                {
                    try
                    {
                        await ProcessReminderAsync(reminder, reminderService, userService, telegramBotService);
                        
                        // Update last sent time
                        var updateDto = new UpdateReminderDto
                        {
                            ReminderTime = reminder.ReminderTime,
                            Type = reminder.Type,
                            IsActive = reminder.IsActive
                        };
                        await reminderService.UpdateReminderAsync(reminder.Id, updateDto, reminder.UserId);
                        
                        _logger.LogInformation("Processed reminder {ReminderId} for user {UserId}", reminder.Id, reminder.UserId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing reminder {ReminderId}", reminder.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing reminder job");
                throw;
            }

            _logger.LogInformation("Completed reminder job execution at {Time}", DateTime.UtcNow);
        }

        private async Task ProcessReminderAsync(
            ReminderDto reminder, 
            IReminderService reminderService,
            IUserService userService,
            TelegramBotService telegramBotService)
        {
            // Check if it's time to send the reminder
            if (reminder.ReminderTime <= DateTime.UtcNow)
            {
                var user = await userService.GetUserByIdAsync(reminder.UserId);
                if (user != null)
                {
                    // Send Telegram notification
                    var message = $"⏰ **Нагадування:**\n{reminder.Message}\n\n📅 {reminder.ReminderTime:dd.MM.yyyy HH:mm}";
                    await telegramBotService.SendNotificationAsync(user.TelegramId, message);

                    // Handle recurring reminders
                    if (reminder.Type != ReminderType.OneTime)
                    {
                        await ScheduleNextReminderAsync(reminder, reminderService);
                    }
                }
            }
        }

        private async Task ScheduleNextReminderAsync(
            ReminderDto reminder, 
            IReminderService reminderService)
        {
            DateTime nextTime = reminder.Type switch
            {
                ReminderType.Daily => reminder.ReminderTime.AddDays(1),
                ReminderType.Weekly => reminder.ReminderTime.AddDays(7),
                ReminderType.Monthly => reminder.ReminderTime.AddMonths(1),
                ReminderType.Yearly => reminder.ReminderTime.AddYears(1),
                _ => DateTime.MaxValue // Don't reschedule OneTime reminders
            };

            if (nextTime != DateTime.MaxValue)
            {
                var updateDto = new UpdateReminderDto
                {
                    ReminderTime = nextTime
                };

                await reminderService.UpdateReminderAsync(reminder.Id, updateDto, reminder.UserId);
            }
        }
    }
}
