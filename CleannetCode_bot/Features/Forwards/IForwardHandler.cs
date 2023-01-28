using Telegram.Bot.Types;

namespace CleannetCode_bot.Features.Forwards
{
    internal interface IForwardHandler
    {
        Task HandleAsync(Message message, CancellationToken ct);
    }
}
