using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;

namespace CleannetCode_bot
{
    public class BotService : IBotService
    {
        private readonly ILogger<BotService> logger;
        private readonly Handlers handlers;
        private readonly ITelegramBotClient client;
        private readonly ConcurrentBag<Task> awaitedTasks = new();
        public BotService(ILogger<BotService> logger, Handlers handlers, ITelegramBotClient client)
        {
            this.logger = logger;
            this.handlers = handlers;
            this.client = client;
        }

        public async Task RunAsync()
        {
            // Начать получение не блокирует поток вызывающего абонента. Получение выполняется в пуле потоков.
            var receiverOptions = new ReceiverOptions()
            {
                AllowedUpdates = new[]
                {
                    UpdateType.Message,
                    UpdateType.InlineQuery,
                    UpdateType.ChosenInlineResult,
                    UpdateType.CallbackQuery,
                    UpdateType.EditedMessage,
                    UpdateType.ChannelPost,
                    UpdateType.EditedChannelPost,
                    UpdateType.ShippingQuery,
                    UpdateType.PreCheckoutQuery,
                    UpdateType.Poll,
                    UpdateType.PollAnswer,
                    UpdateType.MyChatMember,
                    UpdateType.ChatMember,
                    UpdateType.ChatJoinRequest
                }
            };

            using CancellationTokenSource cts = new();
            client.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );

            var me = await client.GetMeAsync(cts.Token);

            logger.LogInformation("{DateTime:dd.MM.yyyy HH:mm:ss:ffff}\tHey! I am {BotName}", DateTime.Now, me.Username);

            Console.ReadKey();
            Task.WaitAll(awaitedTasks.ToArray());
            // Отправить запрос на отмену, чтобы остановить бота
            cts.Cancel();
        }

        private Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cts)
        {
            // Типы обновлений смотри тут: https://core.telegram.org/bots/api#update
            //--------------------------------------------------
            // Разобратся с типом getUpdates. Попытаться запросить историю уже прочитанных сообщений: https://core.telegram.org/bots/api#getupdates
            //--------------------------------------------------

            awaitedTasks.Add(update.Type switch
            {
                UpdateType.Message => handlers.MessageAsync(update.Message, cts),
                UpdateType.InlineQuery => handlers.InlineQueryAsync(update.InlineQuery, cts),
                UpdateType.ChosenInlineResult => handlers.ChosenInlineResultAsync(update.ChosenInlineResult, cts),
                UpdateType.CallbackQuery => handlers.CallbackQueryAsync(update.CallbackQuery, cts),
                UpdateType.EditedMessage => handlers.EditedMessageAsync(update.EditedMessage, cts),
                UpdateType.ChannelPost => handlers.ChannelPostAsync(update.ChannelPost, cts),
                UpdateType.EditedChannelPost => handlers.EditedChannelPostAsync(update.EditedChannelPost, cts),
                UpdateType.ShippingQuery => handlers.ShippingQueryAsync(update.ShippingQuery, cts),
                UpdateType.PreCheckoutQuery => handlers.PreCheckoutQueryAsync(update.PreCheckoutQuery, cts),
                UpdateType.Poll => handlers.PollAsync(update.Poll, cts),
                UpdateType.PollAnswer => handlers.PollAnswerAsync(update.PollAnswer, cts),
                UpdateType.MyChatMember => handlers.MyChatMemberAsync(update.MyChatMember, cts),
                UpdateType.ChatMember => handlers.ChatMemberAsync(update.ChatMember, cts),
                UpdateType.ChatJoinRequest => handlers.ChatJoinRequestAsync(update.ChatJoinRequest, cts),
                _ => handlers.UnknownAsync(update, cts)
            });
            return Task.CompletedTask;
        }

        private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            logger.LogError(exception, errorMessage);
            return Task.CompletedTask;
        }

    }
}