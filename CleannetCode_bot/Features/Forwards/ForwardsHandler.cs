using Telegram.Bot;
using Telegram.Bot.Types;

namespace CleannetCode_bot.Features.Forwards
{
    internal class ForwardsHandler : IForwardHandler
    {
        private Dictionary<long, List<long>> _chatUsers;

        private readonly IStorageService _storageService;
        private readonly ITelegramBotClient _botClient;
        private readonly string _directoryName = "forwards";
        private readonly long _chatIdToForward = 1;

        public ForwardsHandler(IStorageService storageService, ITelegramBotClient botClient)
        {
            _chatUsers = new Dictionary<long, List<long>>()
            {
                {1, new List<long>() { 1 } }
            };

            _storageService = storageService;
            _botClient = botClient;
        }

        public async Task HandleAsync(Message message, CancellationToken ct)
        {
            if (!_chatUsers.TryGetValue(message.Chat.Id, out var allowedUsers))
            {
                // топик не в списке запрещенных
                return;
            }

            // Проверяем пользователь в списке разрешенных?
            if (allowedUsers.Any(u => u == message.From.Id))
            {
                // сообщения от пользователя разрешены
                return;
            }

            // сохраняем сообщение
            await _storageService.AddObject(message, typeof(Message), _directoryName, ct);

            // пересылаем сообщение в генерал
            await _botClient.ForwardMessageAsync(_chatIdToForward, message.Chat.Id, message.MessageId, true, true, ct);

            // удаляем сообщение
            await _botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
        }
    }
}
