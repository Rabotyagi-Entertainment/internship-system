using Internship_system.DAL.Configuration;
using Internship_system.DAL.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Polling;

namespace Internship_system.BLL.Services;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

public class TelegramBotBackgroundService : BackgroundService
{
    private readonly ITelegramBotClient _botClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TelegramBotBackgroundService> _logger;

    public TelegramBotBackgroundService(ITelegramBotClient botClient, ILogger<TelegramBotBackgroundService> logger, IServiceProvider serviceProvider) {
        _botClient = botClient;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>() // Receive all update types
        };

        _botClient.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            cancellationToken: stoppingToken);

        return Task.CompletedTask;  
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InterDbContext>();

        if (update.Type == UpdateType.Message && update.Message?.Text != null)
        {
            var message = update.Message;
            if (message.Text == "/start") {
                var existedUser = await dbContext.StudentTelegrams
                    .FirstOrDefaultAsync(st => st.ChatId == message.Chat.Id.ToString(),
                        cancellationToken: cancellationToken);
                if (existedUser == null) {
                    var newUser = new StudentTelegram {
                        TgName = message.Chat.Username ?? "",
                        ChatId = message.Chat.Id.ToString()
                    };
                    dbContext.Add(newUser);
                    await dbContext.SaveChangesAsync(cancellationToken);
                }

            }
            /*await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Вы ",
                cancellationToken: cancellationToken);*/
        }
    }

    private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception.Message);
        return Task.CompletedTask;
    }
}
