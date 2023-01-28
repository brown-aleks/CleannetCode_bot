using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CleannetCode_bot.Features.Forwards
{
    internal class ForwardsHandler : IForwardHandler
    {
        private Dictionary<long, List<long>> _restrictedTopics;

        private readonly IStorageService _storageService;
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<ForwardsHandler> _logger;
        private readonly string _directoryName = "forwards";
        private readonly long _chatIdToForward = 1;

        public ForwardsHandler(IStorageService storageService, ITelegramBotClient botClient, ILogger<ForwardsHandler> logger)
        {
            _restrictedTopics = new Dictionary<long, List<long>>()
            {
                {1, new List<long>() { 1 } }
            };

            _storageService = storageService;
            _botClient = botClient;
            _logger = logger;
        }

        public async Task HandleAsync(Message message, CancellationToken ct)
        {
            var logPrefix = $"message: {message.MessageId}, chat: {message.Chat.Id}:";

            _logger.LogDebug($"{logPrefix} {nameof(ForwardsHandler)} called");

            if (!_restrictedTopics.TryGetValue(message.Chat.Id, out var allowedUsers))
            {
                // чат не в списке запрещенных
                _logger.LogDebug($"{logPrefix} Chat is not restricted");
                return;
            }

            // Проверяем пользователь в списке разрешенных
            if (allowedUsers.Any(u => u == message.From.Id))
            {
                // сообщения от пользователя разрешены
                _logger.LogDebug($"{logPrefix} The user is in the allowed list in the chat");
                return;
            }

            _logger.LogInformation($"{logPrefix} Message is restricted, strting handling restricted message");

            // сохраняем сообщение
            _logger.LogInformation($"{logPrefix} Saving message");
            await _storageService.AddObject(message, typeof(Message), _directoryName, ct);

            // пересылаем сообщение в генерал
            _logger.LogInformation($"{logPrefix} Forwarding message");
            await _botClient.ForwardMessageAsync(_chatIdToForward, message.Chat.Id, message.MessageId, true, true, ct);

            // удаляем сообщение
            _logger.LogInformation($"{logPrefix} Deleting message");
            await _botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId, ct);
        }
    }
}
