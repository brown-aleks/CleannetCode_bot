using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CleannetCode_bot
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var botClient = new TelegramBotClient(args[0]);

            using CancellationTokenSource cts = new();

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

            var b = await botClient.GetChatAsync(new ChatId(-1001414886435));

            Console.WriteLine("Hey! I am CleannetCode_bot.");
            Console.ReadKey();

            // Отправить запрос на отмену, чтобы остановить бота
            cts.Cancel();
        }
        static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var client = botClient;

            // Обрабатывать только обновления сообщений: https://core.telegram.org/bots/api#message
            if (update.Message is not { } message)
                return;
            // Обрабатывать только текстовые сообщения
            if (message.Text is not { } messageText)
                return;

            var chat = message.Chat;

            Console.WriteLine($"{message.Date.ToLocalTime()} - {message.From?.Username} - {message.From?.FirstName} {message.From?.LastName} - message in chat {chat.Id}.\n Received a '{messageText}'");

            // Эхо полученного текста сообщения
            /*
            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chat.Id,
                text: $"{message.ForwardSenderName} написал:\n" + messageText,  //  chat.Username
                cancellationToken: cancellationToken,
                replyToMessageId: message.MessageId);
            */
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