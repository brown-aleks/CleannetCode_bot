using Telegram.Bot;

namespace CleannetCodeBot.Features.Forwards;

public interface IForwardHandler
{
    Task HandleAsync(
        long fromChatId,
        int messageId,
        bool isTopicMessage,
        int topicId,
        long senderId,
        ITelegramBotClient botClient,
        CancellationToken ct);
}