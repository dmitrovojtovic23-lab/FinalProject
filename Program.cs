using FinalProject.DAL;
using FinalProject.BLL.Interfaces;
using FinalProject.BLL.Services;
using FinalProject.BLL.Mapping;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Telegram.Bot;
using FinalProject.API.Services;
using Quartz;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IReminderService, ReminderService>();

builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

builder.Services.AddSingleton<ITelegramBotClient>(sp =>
{
    var botToken = builder.Configuration["TelegramBot:Token"];
    if (string.IsNullOrEmpty(botToken))
    {
        throw new InvalidOperationException("Telegram bot token is not configured");
    }
    return new TelegramBotClient(botToken);
});

builder.Services.AddScoped<TelegramBotService>();

builder.Services.AddHostedService<TelegramBotHostedService>();

builder.Services.AddTransient<ReminderJobService>();

builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT secret key is not configured")))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
}

{
    var schedulerFactory = app.Services.GetRequiredService<ISchedulerFactory>();
    var scheduler = await schedulerFactory.GetScheduler(System.Threading.CancellationToken.None);

    var reminderJobKey = new JobKey("reminder-job");
    var reminderJob = JobBuilder.Create<ReminderJobService>()
        .WithIdentity(reminderJobKey)
        .Build();

    var reminderTrigger = TriggerBuilder.Create()
        .WithIdentity("reminder-trigger")
        .StartNow()
        .WithSimpleSchedule(x => x
            .WithIntervalInMinutes(1)
            .RepeatForever())
        .Build();

    await scheduler.ScheduleJob(reminderJob, reminderTrigger);
}

app.Logger.LogInformation("Application started successfully!");

app.Run();
