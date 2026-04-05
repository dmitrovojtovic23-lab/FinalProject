using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FinalProject.API.Services
{
    public class TelegramBotHostedService : BackgroundService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IServiceProvider _services;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TelegramBotHostedService> _logger;

        public TelegramBotHostedService(
            ITelegramBotClient botClient,
            IServiceProvider services,
            IConfiguration configuration,
            ILogger<TelegramBotHostedService> logger)
        {
            _botClient = botClient;
            _services = services;
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var webhookUrl = _configuration["TelegramBot:WebhookUrl"];

            if (!string.IsNullOrEmpty(webhookUrl))
            {
                await _botClient.SetWebhookAsync(webhookUrl, cancellationToken: stoppingToken);
                _logger.LogInformation("Telegram bot webhook set to: {WebhookUrl}", webhookUrl);
                try
                {
                    await Task.Delay(Timeout.Infinite, stoppingToken);
                }
                catch (TaskCanceledException) { }

                return;
            }

            try
            {
                await _botClient.DeleteWebhookAsync(cancellationToken: stoppingToken);
                _logger.LogInformation("Deleted existing Telegram webhook (if any) before starting long polling");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete existing Telegram webhook before long polling");
            }

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<Telegram.Bot.Types.Enums.UpdateType>()
            };

            _botClient.StartReceiving(
                async (botClient, update, token) => await HandleUpdateAsync(update, token),
                async (botClient, exception, token) => await HandlePollingErrorAsync(exception),
                receiverOptions,
                cancellationToken: stoppingToken);

            _logger.LogInformation("Telegram bot started long polling");

            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (TaskCanceledException) { }
        }

        private async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _services.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<TelegramBotService>();
                await handler.HandleUpdateAsync(update);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling Telegram update");
            }
        }

        private Task HandlePollingErrorAsync(Exception exception)
        {
            var err = exception switch
            {
                ApiRequestException apiEx => $"Telegram API Error: [{apiEx.ErrorCode}] {apiEx.Message}",
                _ => exception.ToString()
            };

            _logger.LogError("Polling error: {Error}", err);
            return Task.CompletedTask;
        }
    }
}
