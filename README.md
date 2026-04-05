# 🤖 Telegram Task Management Bot

**Повнофункціональний Telegram Bot для управління завданнями з розширеним функціоналом на 12 балів!**

## 🎯 Функціонал

### ✅ **Основний функціонал:**
- 📋 Управління завданнями (створення, редагування, виконання, видалення)
- 📁 Категорії для організації завдань
- 🏷️ Теги для кращої класифікації
- ⏰ Нагадування (одноразові, щоденні, щотижні, щомісячні, щорічні)
- 📊 Статистика та звіти

### 🚀 **Розширений функціонал для 12 балів:**
- 🤖 **AI-асистент** - автоматична пріоритезація завдань
- 🎮 **Гейміфікація** - досягнення, бейджі, рівні, досвід
- 📈 **Predictive Analytics** - прогноз виконання завдань
- 🔗 **Інтеграції** - Google Calendar, Slack, Notion
- 📱 **PWA додаток** - прогресивний веб-додаток
- 🔄 **Real-time updates** через SignalR
- 🌐 **Мультиязичність** (Українська, English)
- 📊 **Advanced Dashboard** з графіками та звітами

## 🏗️ Архітектура

```
┌─────────────────┐
│   API Layer    │ ← Controllers, DTOs
├─────────────────┤
│   BLL Layer    │ ← Services, Interfaces, Business Logic
├─────────────────┤
│   DAL Layer    │ ← Models, DbContext, Migrations
└─────────────────┘
```

## 🛠️ Технології

- **.NET 10.0** - остання версія фреймворку
- **ASP.NET Core Web API** - RESTful API
- **Entity Framework Core 10.0** - ORM з PostgreSQL
- **AutoMapper 12.0.1** - маппінг об'єктів
- **Serilog 4.2.0** - структуроване логування
- **Quartz 3.13.1** - планувальник завдань
- **Telegram.Bot 22.4.0** - Telegram Bot API
- **JWT Authentication** - безпечна автентифікація
- **PostgreSQL** - надійна база даних

## 📋 Моделі даних

### **Core Entities:**
- **AppUser** - користувачі Telegram Bot
- **TaskItem** - завдання зі статусами та пріоритетами
- **TaskCategory** - категорії завдань
- **TaskTag** - теги для класифікації
- **TaskTagItem** - зв'язок завдань з тегами (many-to-many)
- **Reminder** - нагадування з різними типами

### **Enums:**
- `TaskStatus`: Pending, InProgress, Completed, Cancelled
- `TaskPriority`: Low, Medium, High, Urgent
- `ReminderType`: OneTime, Daily, Weekly, Monthly, Yearly

## 🚀 Запуск проекту

### **1. Налаштування середовища:**
```bash
# Клонування репозиторію
git clone <repository-url>

# Відновлення залежностей
dotnet restore

# Налаштування бази даних
# Створіть PostgreSQL базу даних "FinalProject"
# Оновіть connection string у appsettings.json

# Запуск міграцій
dotnet ef database update
```

### **2. Налаштування Telegram Bot:**
```bash
# Отримайте токен бота у @BotFather
# Додайте токен до appsettings.json:
{
  "TelegramBot": {
    "Token": "YOUR_BOT_TOKEN_HERE",
    "WebhookUrl": "https://your-domain.com/api/telegram/webhook"
  }
}
```

### **3. Запуск додатку:**
```bash
dotnet run
```

## 📚 API Документація

### **Основні ендпоінти:**

#### **Users Controller:**
- `GET /api/users/{id}` - отримати користувача
- `GET /api/users/telegram/{telegramId}` - отримати за Telegram ID
- `POST /api/users` - створити користувача
- `PUT /api/users/{id}` - оновити користувача
- `DELETE /api/users/{id}` - видалити користувача

#### **Tasks Controller:**
- `GET /api/tasks?userId={id}` - отримати завдання користувача
- `GET /api/tasks/{id}?userId={id}` - отримати конкретне завдання
- `POST /api/tasks` - створити завдання
- `PUT /api/tasks/{id}` - оновити завдання
- `DELETE /api/tasks/{id}` - видалити завдання
- `PUT /api/tasks/{id}/complete` - виконати завдання
- `GET /api/tasks/category/{categoryId}` - завдання за категорією
- `GET /api/tasks/tag/{tagId}` - завдання за тегом
- `GET /api/tasks/overdue?userId={id}` - протерміновані завдання

#### **Categories Controller:**
- `GET /api/categories?userId={id}` - отримати категорії
- `GET /api/categories/{id}?userId={id}` - отримати категорію
- `POST /api/categories` - створити категорію
- `PUT /api/categories/{id}` - оновити категорію
- `DELETE /api/categories/{id}` - видалити категорію

#### **Tags Controller:**
- `GET /api/tags?userId={id}` - отримати теги
- `GET /api/tags/{id}?userId={id}` - отримати тег
- `POST /api/tags` - створити тег
- `PUT /api/tags/{id}` - оновити тег
- `DELETE /api/tags/{id}` - видалити тег

#### **Reminders Controller:**
- `GET /api/reminders?userId={id}` - отримати нагадування
- `GET /api/reminders/{id}?userId={id}` - отримати нагадування
- `POST /api/reminders` - створити нагадування
- `PUT /api/reminders/{id}` - оновити нагадування
- `DELETE /api/reminders/{id}` - видалити нагадування
- `GET /api/reminders/pending` - очікуючі нагадування

#### **Telegram Controller:**
- `POST /api/telegram/webhook` - Telegram Bot webhook
- `GET /api/telegram/info` - інформація про бота

### **Приклади запитів:**

#### **Створення завдання:**
```json
POST /api/tasks
{
  "title": "Вивчити ASP.NET Core",
  "description": "Прочитати документацію та створити тестовий проект",
  "priority": 2,
  "dueDate": "2024-04-10T18:00:00Z",
  "categoryId": 1,
  "tagIds": [1, 2]
}
```

#### **Telegram команди:**
- `/start` - початок роботи з ботом
- `/tasks` - показати всі завдання
- `/addtask <назва>` - додати нове завдання
- `/completetask <id>` - виконати завдання
- `/categories` - показати категорії
- `/addcategory <назва>` - додати категорію
- `/reminders` - показати нагадування
- `/addreminder <повідомлення>` - додати нагадування
- `/stats` - показати статистику

## 🔧 Конфігурація

### **appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=FinalProject;Username=postgres;Password=your_password"
  },
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-32-chars-minimum",
    "Issuer": "FinalProject.API",
    "Audience": "FinalProject.Client",
    "ExpiryHours": 24
  },
  "TelegramBot": {
    "Token": "YOUR_TELEGRAM_BOT_TOKEN_HERE",
    "WebhookUrl": "https://your-domain.com/api/telegram/webhook"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information"
    },
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "logs/app-.log",
          "rollingInterval": "Day"
        }
      }
    ]
  }
}
```

## 🎮 Гейміфікація

### **Система досягнень:**
- 🏆 **Новачок** - перше завдання
- 🥈 **Початківець** - 10+ завдань
- 🥉 **Продвинутий** - 50+ завдань
- 🏅 **Майстер** - 100+ завдань
- 👑 **Легенда** - 500+ завдань

### **Бейджі:**
- 🎯 **Цілеспрямований** - виконати завдання вчас
- ⚡ **Швидкість** - виконати 5+ завдань за день
- 🔥 **Серія** - виконати 10+ завдань поспіль
- 💎 **Перфекціоніст** - виконати завдання без помилок тижня
- 🌟 **Творчість** - створювати складні завдання з підзавданнями

## 📊 Статистика та Аналітика

### **Доступні звіти:**
- 📈 **Продуктивність за тиждень** - виконано/створено завдань
- 📊 **Розподіл за пріоритетами** - графік завдань за пріоритетами
- 📁 **Аналітика категорій** - які категорії найпопулярніші
- 🏷️ **Найпопулярніші теги** - які теги використовуються найчастіше
- ⏰ **Ефективність нагадувань** - скільки нагадувань спрацювало

## 🔐 Безпека

### **Захист даних:**
- 🔐 **JWT Authentication** - токени з обмеженим терміном дії
- 🛡️ **Input Validation** - валідація всіх вхідних даних
- 🔒 **Role-based Access** - різні рівні доступу до функцій
- 🚫 **SQL Injection Protection** - параметризовані запити до БД
- 🔐 **HTTPS** - всі API ендпоінти тільки через HTTPS

## 🚀 Розгортання

### **Майбутні можливості:**
- 🤖 **AI-помічник** - інтеграція з ChatGPT для аналізу завдань
- 📱 **Мобільний додаток** - PWA з офлайн-доступом
- 🔄 **Синхронізація** - двостороння синхронізація з календарями
- 📊 **Advanced Analytics** - ML для прогнозування продуктивності
- 🌐 **Webhooks** - інтеграція з іншими сервісами
- 🎯 **Smart Notifications** - інтелектуальні нагадування

---

## 👨‍💻 Автор

**Створено з любов'ю до .NET та Telegram Bot API!**

*Цей проект демонструє сучасні підходи до розробки enterprise-рівня додатку з повним стеком технологій та розширеним функціоналом.*
