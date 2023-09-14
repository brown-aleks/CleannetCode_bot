using Telegram.Bot.Types;

namespace CleannetCodeBot.Infrastructure;
public class TelegramRequest
{
    public TelegramRequest(Update update)
    {
        Update = update;
    }

    public Update Update { get; }
}