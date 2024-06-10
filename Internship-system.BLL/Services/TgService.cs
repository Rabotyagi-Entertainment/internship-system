using Internship_system.DAL.Configuration;
using Internship_system.DAL.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Internship_system.BLL.Services;

public class TgService {
    private readonly IServiceProvider _serviceProvider;

    public TgService(IServiceProvider serviceProvider) {
        _serviceProvider = serviceProvider;
    }

    public async Task Execute() {
        var botClient = new TelegramBotClient("6895417555:AAE3G2dH6xbic7a66l5eCrnkBnH0ulqfidM");
        while (true)
        {
            // Check for new messages
            var updates = botClient.GetUpdatesAsync().Result;

            foreach (var update in updates)
            {
                // Handle the message
                await HandleUpdateAsync(update);
            }
        }
    }
    private async Task HandleUpdateAsync(Update update)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InterDbContext>();

        if (update.Type == UpdateType.Message && update.Message?.Text != null)
        {
            var message = update.Message;
            if (message.Text == "/start") {
                var existedUser = await dbContext.StudentTelegrams
                    .FirstOrDefaultAsync(st => st.ChatId == message.Chat.Id.ToString());
                if (existedUser == null) {
                    var newUser = new StudentTelegram {
                        TgName = message.Chat.Username ?? "",
                        ChatId = message.Chat.Id.ToString()
                    };
                    dbContext.Add(newUser);
                    await dbContext.SaveChangesAsync();
                }

            }
            /*await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Вы ",
                cancellationToken: cancellationToken);*/
        }
    }
}