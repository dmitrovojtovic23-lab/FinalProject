using FinalProject.BLL.DTOs;
using FinalProject.BLL.Interfaces;
using FinalProject.DAL;
using TaskStatus = FinalProject.DAL.TaskStatus;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace FinalProject.API.Services
{
    public class TelegramBotService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IUserService _userService;
        private readonly ITaskService _taskService;
        private readonly ICategoryService _categoryService;
        private readonly ITagService _tagService;
        private readonly IReminderService _reminderService;
        private readonly ILogger<TelegramBotService> _logger;

        public TelegramBotService(
            ITelegramBotClient botClient,
            IUserService userService,
            ITaskService taskService,
            ICategoryService categoryService,
            ITagService tagService,
            IReminderService reminderService,
            ILogger<TelegramBotService> logger)
        {
            _botClient = botClient;
            _userService = userService;
            _taskService = taskService;
            _categoryService = categoryService;
            _tagService = tagService;
            _reminderService = reminderService;
            _logger = logger;
        }

        public async Task HandleUpdateAsync(Update update)
        {
            if (update.Message != null)
            {
                await HandleMessageAsync(update.Message);
            }
            else if (update.CallbackQuery != null)
            {
                await HandleCallbackAsync(update.CallbackQuery);
            }
        }

        private async Task HandleMessageAsync(Message message)
        {
            var user = await GetOrCreateUserAsync(message.From);
            if (user == null) return;

            if (message.Text?.StartsWith('/') == true)
            {
                await HandleCommandAsync(message, user);
            }
            else
            {
                await SendMainMenuAsync(user.TelegramId);
            }
        }

        private async Task HandleCommandAsync(Message message, UserDto user)
        {
            var command = message.Text?.Split(' ')[0]?.ToLower();
            var args = message.Text?.Split(' ').Skip(1).ToArray();

            switch (command)
            {
                case "/start":
                    await SendWelcomeMessageAsync(user.TelegramId);
                    break;

                case "/tasks":
                    await SendTasksMenuAsync(user.Id, user.TelegramId);
                    break;

                case "/addtask":
                    await AddTaskAsync(args, user);
                    break;

                case "/completetask":
                    await CompleteTaskAsync(args, user);
                    break;

                case "/categories":
                    await SendCategoriesMenuAsync(user.Id, user.TelegramId);
                    break;

                case "/addcategory":
                    await AddCategoryAsync(args, user);
                    break;

                case "/reminders":
                    await SendRemindersMenuAsync(user.Id, user.TelegramId);
                    break;

                case "/addreminder":
                    await AddReminderAsync(args, user);
                    break;

                case "/stats":
                    await SendStatsAsync(user);
                    break;

                default:
                    await SendUnknownCommandAsync(user.TelegramId);
                    break;
            }
        }

        private async Task HandleCallbackAsync(CallbackQuery callbackQuery)
        {
            var user = await _userService.GetUserByTelegramIdAsync(callbackQuery.From.Id.ToString());
            _logger.LogInformation("Received callback from {TelegramId}: {Data}", callbackQuery.From.Id, callbackQuery.Data);
            if (user == null) return;

            var data = callbackQuery.Data;
            var parts = data.Split(':');

            if (parts.Length >= 2)
            {
                var action = parts[0];
                var value = parts[1];

                switch (action)
                {
                    case "complete_task":
                        await CompleteTaskByIdAsync(int.Parse(value), user);
                        break;
                    case "delete_task":
                        await DeleteTaskByIdAsync(int.Parse(value), user);
                        break;
                    case "view_tasks":
                        await ViewTasksByCategoryAsync(int.Parse(value), user);
                        break;
                    case "menu":
                        
                        switch (value)
                        {
                            case "tasks":
                                await SendTasksMenuAsync(user.Id, user.TelegramId);
                                break;
                            case "categories":
                                await SendCategoriesMenuAsync(user.Id, user.TelegramId);
                                break;
                            case "reminders":
                                await SendRemindersMenuAsync(user.Id, user.TelegramId);
                                break;
                            case "stats":
                                await SendStatsAsync(user);
                                break;
                        }
                        break;
                    case "action":

                        switch (value)
                        {
                            case "add_task":
                                await _botClient.SendTextMessageAsync(user.TelegramId, "📝 Щоб додати завдання використайте команду: /addtask Назва завдання");
                                break;
                        }
                        break;
                }
            }

            await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
        }

        private async Task<UserDto?> GetOrCreateUserAsync(User fromUser)
        {
            var user = await _userService.GetUserByTelegramIdAsync(fromUser.Id.ToString());
            if (user == null)
            {
                var createUserDto = new CreateUserDto
                {
                    FirstName = fromUser.FirstName,
                    LastName = fromUser.LastName,
                    Username = fromUser.Username ?? fromUser.FirstName,
                    TelegramId = fromUser.Id.ToString()
                };

                user = await _userService.CreateUserAsync(createUserDto);
                _logger.LogInformation($"Created new user: {user.FirstName} {user.LastName}");
            }

            return user;
        }

        private async Task SendWelcomeMessageAsync(string telegramId)
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("📋 Мої завдання", "menu:tasks") },
                new[] { InlineKeyboardButton.WithCallbackData("📁 Категорії", "menu:categories") },
                new[] { InlineKeyboardButton.WithCallbackData("⏰ Нагадування", "menu:reminders") },
                new[] { InlineKeyboardButton.WithCallbackData("📊 Статистика", "menu:stats") }
            });

            await _botClient.SendTextMessageAsync(
                chatId: telegramId,
                text: "👋 Вітаю! Я ваш персональний помічник для управління завданнями.\n\nОберіть опцію нижче:",
                replyMarkup: keyboard
            );
        }

        private async Task SendMainMenuAsync(string telegramId)
        {
            await SendWelcomeMessageAsync(telegramId);
        }

        private async Task SendTasksMenuAsync(int userId, string telegramId)
        {
            var tasks = await _taskService.GetUserTasksAsync(userId);
            var activeTasks = tasks.Where(t => t.Status != TaskStatus.Completed).ToList();
            
            if (!activeTasks.Any())
            {
                await _botClient.SendTextMessageAsync(telegramId, "🎉 У вас немає активних завдань!");
                return;
            }

            var message = "📋 **Ваші активні завдання:**\n\n";
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new InlineKeyboardButton("➕ Додати завдання") { CallbackData = "action:add_task" }
            });

            for (int i = 0; i < Math.Min(activeTasks.Count, 10); i++)
            {
                var task = activeTasks[i];
                var status = task.Status == TaskStatus.Completed ? "✅" : 
                           task.Status == TaskStatus.InProgress ? "🔄" : "⏳";
                
                message += $"{status} *{task.Title}*\n";
                message += $"📅 {task.DueDate:dd.MM.yyyy}\n";
                message += $"🏷️ {task.Category?.Name ?? "Без категорії"}\n\n";
            }

            if (activeTasks.Count > 10)
            {
                message += $"... і ще {activeTasks.Count - 10} завдань\n";
            }

            await _botClient.SendTextMessageAsync(telegramId, message, replyMarkup: keyboard);
        }

        private async Task SendCategoriesMenuAsync(int userId, string telegramId)
        {
            var categories = await _categoryService.GetUserCategoriesAsync(userId);
            
            if (!categories.Any())
            {
                await _botClient.SendTextMessageAsync(telegramId, "📁 У вас немає категорій. Створіть нову за допомогою /addcategory");
                return;
            }

            var message = "📁 **Ваші категорії:**\n\n";
            foreach (var category in categories)
            {
                message += $"🏷️ {category.Name} ({category.TaskCount} завдань)\n";
            }

            await _botClient.SendTextMessageAsync(telegramId, message);
        }

        private async Task SendRemindersMenuAsync(int userId, string telegramId)
        {
            var reminders = await _reminderService.GetUserRemindersAsync(userId);
            
            if (!reminders.Any())
            {
                await _botClient.SendTextMessageAsync(telegramId, "⏰ У вас немає нагадувань. Створіть нове за допомогою /addreminder");
                return;
            }

            var message = "⏰ **Ваші нагадування:**\n\n";
            foreach (var reminder in reminders)
            {
                var type = reminder.Type switch
                {
                    ReminderType.Daily => "Щоденно",
                    ReminderType.Weekly => "Щотижня",
                    ReminderType.Monthly => "Щомісяця",
                    _ => "Разове"
                };
                
                message += $"🔔 {reminder.Message}\n";
                message += $"📅 {reminder.ReminderTime:dd.MM.yyyy HH:mm}\n";
                message += $"🔄 {type}\n\n";
            }

            await _botClient.SendTextMessageAsync(telegramId, message);
        }

        private async Task SendStatsAsync(UserDto user)
        {
            var tasks = await _taskService.GetUserTasksAsync(user.Id);
            var completed = tasks.Count(t => t.Status == TaskStatus.Completed);
            var inProgress = tasks.Count(t => t.Status == TaskStatus.InProgress);
            var pending = tasks.Count(t => t.Status == TaskStatus.Pending);
            var overdue = (await _taskService.GetOverdueTasksAsync(user.Id)).Count();

            var message = "📊 **Ваша статистика:**\n\n";
            message += $"✅ Виконано: {completed}\n";
            message += $"🔄 В процесі: {inProgress}\n";
            message += $"⏳ Очікує: {pending}\n";
            message += $"⚠️ Протерміновано: {overdue}\n\n";
            message += $"📈 Всього завдань: {tasks.Count()}";

            await _botClient.SendTextMessageAsync(user.TelegramId, message);
        }

        private async Task AddTaskAsync(string[] args, UserDto user)
        {
            if (args.Length == 0)
            {
                await _botClient.SendTextMessageAsync(user.TelegramId, "📝 Введіть назву завдання після /addtask");
                return;
            }

            var title = string.Join(" ", args);
            var createTaskDto = new CreateTaskDto
            {
                Title = title,
                Priority = TaskPriority.Medium
            };

            var task = await _taskService.CreateTaskAsync(createTaskDto);
            await _botClient.SendTextMessageAsync(user.TelegramId, $"✅ Завдання \"{task.Title}\" створено!");
        }

        private async Task CompleteTaskAsync(string[] args, UserDto user)
        {
            if (args.Length == 0)
            {
                await _botClient.SendTextMessageAsync(user.TelegramId, "🔢 Введіть ID завдання після /completetask");
                return;
            }

            if (!int.TryParse(args[0], out var taskId))
            {
                await _botClient.SendTextMessageAsync(user.TelegramId, "❌ Невірний ID завдання");
                return;
            }

            try
            {
                var task = await _taskService.CompleteTaskAsync(taskId, user.Id);
                await _botClient.SendTextMessageAsync(user.TelegramId, $"🎉 Завдання \"{task.Title}\" виконано!");
            }
            catch (KeyNotFoundException)
            {
                await _botClient.SendTextMessageAsync(user.TelegramId, "❌ Завдання не знайдено");
            }
        }

        private async Task AddCategoryAsync(string[] args, UserDto user)
        {
            if (args.Length == 0)
            {
                await _botClient.SendTextMessageAsync(user.TelegramId, "📁 Введіть назву категорії після /addcategory");
                return;
            }

            var name = string.Join(" ", args);
            var createCategoryDto = new CreateCategoryDto { Name = name };
            
            var category = await _categoryService.CreateCategoryAsync(createCategoryDto, user.Id);
            await _botClient.SendTextMessageAsync(user.TelegramId, $"✅ Категорія \"{category.Name}\" створена!");
        }

        private async Task AddReminderAsync(string[] args, UserDto user)
        {
            if (args.Length == 0)
            {
                await _botClient.SendTextMessageAsync(user.TelegramId, "⏰ Введіть повідомлення нагадування після /addreminder");
                return;
            }

            var message = string.Join(" ", args);
            var createReminderDto = new CreateReminderDto
            {
                Message = message,
                ReminderTime = DateTime.UtcNow.AddMinutes(30),
                Type = ReminderType.OneTime
            };

            var reminder = await _reminderService.CreateReminderAsync(createReminderDto, user.Id);
            await _botClient.SendTextMessageAsync(user.TelegramId, $"✅ Нагадування створено на {reminder.ReminderTime:dd.MM.yyyy HH:mm}");
        }

        private async Task CompleteTaskByIdAsync(int taskId, UserDto user)
        {
            try
            {
                var task = await _taskService.CompleteTaskAsync(taskId, user.Id);
                await _botClient.SendTextMessageAsync(user.TelegramId, $"🎉 Завдання \"{task.Title}\" виконано!");
            }
            catch (KeyNotFoundException)
            {
                await _botClient.SendTextMessageAsync(user.TelegramId, "❌ Завдання не знайдено");
            }
        }

        private async Task DeleteTaskByIdAsync(int taskId, UserDto user)
        {
            var result = await _taskService.DeleteTaskAsync(taskId, user.Id);
            if (result)
            {
                await _botClient.SendTextMessageAsync(user.TelegramId, "🗑️ Завдання видалено");
            }
            else
            {
                await _botClient.SendTextMessageAsync(user.TelegramId, "❌ Завдання не знайдено");
            }
        }

        private async Task ViewTasksByCategoryAsync(int categoryId, UserDto user)
        {
            var tasks = await _taskService.GetTasksByCategoryAsync(user.Id, categoryId);
            var category = await _categoryService.GetCategoryByIdAsync(categoryId, user.Id);
            
            if (!tasks.Any())
            {
                await _botClient.SendTextMessageAsync(user.TelegramId, "📁 У цій категорії немає завдань");
                return;
            }

            var message = $"📋 **Завдання в категорії \"{category?.Name}\":**\n\n";
            foreach (var task in tasks)
            {
                var status = task.Status == TaskStatus.Completed ? "✅" : 
                           task.Status == TaskStatus.InProgress ? "🔄" : "⏳";
                message += $"{status} {task.Title}\n";
            }

            await _botClient.SendTextMessageAsync(user.TelegramId, message);
        }

        private async Task SendUnknownCommandAsync(string telegramId)
        {
            await _botClient.SendTextMessageAsync(
                telegramId,
                "❌ Невідома команда. Використовуйте:\n" +
                "/start - Початок роботи\n" +
                "/tasks - Мої завдання\n" +
                "/addtask - Додати завдання\n" +
                "/completetask - Виконати завдання\n" +
                "/categories - Мої категорії\n" +
                "/addcategory - Додати категорію\n" +
                "/reminders - Мої нагадування\n" +
                "/addreminder - Додати нагадування\n" +
                "/stats - Статистика"
            );
        }

        public async Task SendNotificationAsync(string telegramId, string message)
        {
            try
            {
                await _botClient.SendTextMessageAsync(telegramId, message);
                _logger.LogInformation("Sent notification to {TelegramId}: {Message}", telegramId, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to {TelegramId}", telegramId);
            }
        }
    }
}
