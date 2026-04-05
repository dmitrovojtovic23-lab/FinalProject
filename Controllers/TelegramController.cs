using Microsoft.AspNetCore.Mvc;
using FinalProject.BLL.DTOs;
using FinalProject.BLL.Interfaces;
using FinalProject.API.Services;
using Telegram.Bot.Types;

namespace FinalProject.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelegramController : ControllerBase
    {
        private readonly TelegramBotService _botService;
        private readonly ILogger<TelegramController> _logger;

        public TelegramController(TelegramBotService botService, ILogger<TelegramController> logger)
        {
            _botService = botService;
            _logger = logger;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook([FromBody] Update update)
        {
            try
            {
                _logger.LogInformation("Received webhook update: {UpdateId}", update.Id);
                await _botService.HandleUpdateAsync(update);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook update");
                return BadRequest();
            }
        }

        [HttpGet("info")]
        public IActionResult GetBotInfo()
        {
            return Ok(new
            {
                status = "running",
                description = "Telegram Bot for Task Management",
                version = "1.0.0",
                endpoints = new
                {
                    webhook = "/api/telegram/webhook",
                    commands = new[]
                    {
                        "/start - Start bot",
                        "/tasks - View tasks",
                        "/addtask - Add task",
                        "/completetask - Complete task",
                        "/categories - View categories",
                        "/addcategory - Add category",
                        "/reminders - View reminders",
                        "/addreminder - Add reminder",
                        "/stats - View statistics"
                    }
                }
            });
        }
    }
}
