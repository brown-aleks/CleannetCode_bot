using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        private readonly IConfiguration config;
        private readonly Handlers handlers;

        public BotService(ILogger<BotService> logger, IConfiguration config, Handlers handlers)
        {
            this.logger = logger;
            this.config = config;
            this.handlers = handlers;
        }
        public async Task RunAsync()
        {
            string? accessToken = config.GetValue<string>("AccessToken");
            if (accessToken == null)
            {
                logger.LogError("{DateTime:dd.MM.yyyy HH:mm:ss:ffff}\tAccessToken is null", DateTime.Now);
                return;
            }
            logger.LogInformation("{DateTime:dd.MM.yyyy HH:mm:ss:ffff}\tCleannetCode_bot is start", DateTime.Now);

            using CancellationTokenSource cts = new();

            var botClient = new TelegramBotClient(accessToken);

            // Начать получение не блокирует поток вызывающего абонента. Получение выполняется в пуле потоков.
            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = Array.Empty<UpdateType>() // получать все типы обновлений
            };

            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );

            var me = await botClient.GetMeAsync();

            logger.LogInformation("{DateTime:dd.MM.yyyy HH:mm:ss:ffff}\tHey! I am {BotName}", DateTime.Now, me.Username);

            Console.ReadKey();

            // Отправить запрос на отмену, чтобы остановить бота
            cts.Cancel();
        }

        public Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cts)
        {
            // Типы обновлений смотри тут: https://core.telegram.org/bots/api#update

            //--------------------------------------------------
            // Разобратся с типом getUpdates. Попытаться запросить историю уже прочитанных сообщений: https://core.telegram.org/bots/api#getupdates
            //--------------------------------------------------

            return update.Type switch
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
            };
        }

        static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

    }
}