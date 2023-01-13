using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.Payments;

namespace CleannetCode_bot
{
    public class Program
    {
        private static List<Message> _messages = new();
        private static JsonSerializerOptions optionsJson = new();
        private static readonly string _fileName = "stat.json";
        private static readonly string _directory = "./StatDate/";

        public static async Task Main(string[] args)
        {
            var botClient = new TelegramBotClient(args[0]);
            
            using CancellationTokenSource cts = new();

            optionsJson = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true
            };

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

            Console.WriteLine($"Hey! I am CleannetCode_bot. {me.Username}");
            Console.ReadKey();

            // Отправить запрос на отмену, чтобы остановить бота
            cts.Cancel();
        }
        public static Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cts)
        {
            // Типы обновлений смотри тут: https://core.telegram.org/bots/api#update

            //--------------------------------------------------
            // Разобратся с типом getUpdates. Попытаться запросить историю уже прочитанных сообщений: https://core.telegram.org/bots/api#getupdates
            //--------------------------------------------------

            return update.Type switch
            {
                UpdateType.Message => HandleMessageAsync(update.Message, cts),
                UpdateType.InlineQuery => HandleInlineQueryAsync(update.InlineQuery, cts),
                UpdateType.ChosenInlineResult => HandleChosenInlineResultAsync(update.ChosenInlineResult, cts),
                UpdateType.CallbackQuery => HandleCallbackQueryAsync(update.CallbackQuery, cts),
                UpdateType.EditedMessage => HandleEditedMessageAsync(update.EditedMessage, cts),
                UpdateType.ChannelPost => HandleChannelPostAsync(update.ChannelPost, cts),
                UpdateType.EditedChannelPost => HandleEditedChannelPostAsync(update.EditedChannelPost, cts),
                UpdateType.ShippingQuery => HandleShippingQueryAsync(update.ShippingQuery, cts),
                UpdateType.PreCheckoutQuery => HandlePreCheckoutQueryAsync(update.PreCheckoutQuery, cts),
                UpdateType.Poll => HandlePollAsync(update.Poll, cts),
                UpdateType.PollAnswer => HandlePollAnswerAsync(update.PollAnswer, cts),
                UpdateType.MyChatMember => HandleMyChatMemberAsync(update.MyChatMember, cts),
                UpdateType.ChatMember => HandleChatMemberAsync(update.ChatMember, cts),
                UpdateType.ChatJoinRequest => HandleChatJoinRequestAsync(update.ChatJoinRequest, cts),
                _ => HandleUnknownAsync(update, cts)
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

        private static Task HandleMessageAsync(Message? message, CancellationToken cts)
        {
            if (message is null) { return Task.CompletedTask; }
            if (message.Text is { } messageText)
            {
                if (messageText == "/stat")
                {
                    var json = JsonSerializer.Serialize(_messages, optionsJson);

                    Directory.CreateDirectory(_directory);
                    string fileName = DateTime.Now.ToString("ddMMyyyy_HHmmss_ffff") + "-" + _fileName;
                    string _path = Path.Combine(_directory, fileName);
                    System.IO.File.WriteAllTextAsync(_path, json, cts);
                }
            }

            _messages.Add(message);

            var chat = message.Chat;

            Console.WriteLine($"message\t - {message.Date.ToLocalTime()}\t - {message.From?.Username}\t - {message.From?.FirstName}\t - {message.From?.LastName}\t - {message.Type}\t - {chat.Id}");


            // Эхо полученного текста сообщения
            /*
            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chat.Id,
                text: $"{message.ForwardSenderName} написал:\n" + messageText,  //  chat.Username
                cancellationToken: cancellationToken,
                replyToMessageId: message.MessageId);
            */
            return Task.CompletedTask;
        }

        private static Task HandleInlineQueryAsync(InlineQuery? inlineQuery, CancellationToken cts)
        {
            if (inlineQuery is null) { return Task.CompletedTask; }
            FileSaveAssistantAsync(inlineQuery, typeof(InlineQuery), "inlineQuery", cts);
            return Task.CompletedTask;
        }

        private static Task HandleChosenInlineResultAsync(ChosenInlineResult? chosenInlineResult, CancellationToken cts)
        {
            if (chosenInlineResult is null) { return Task.CompletedTask; }
            FileSaveAssistantAsync(chosenInlineResult, typeof(ChosenInlineResult), "chosenInlineResult", cts);
            return Task.CompletedTask;
        }

        private static Task HandleCallbackQueryAsync(CallbackQuery? callbackQuery, CancellationToken cts)
        {
            if (callbackQuery is null) { return Task.CompletedTask; }
            FileSaveAssistantAsync(callbackQuery, typeof(CallbackQuery), "callbackQuery", cts);
            return Task.CompletedTask;
        }

        private static Task HandleEditedMessageAsync(Message? editedMessage, CancellationToken cts)
        {
            if (editedMessage is null) { return Task.CompletedTask; }
            FileSaveAssistantAsync(editedMessage, typeof(Message), "editedMessage", cts);
            return Task.CompletedTask;
        }

        private static Task HandleChannelPostAsync(Message? channelPost, CancellationToken cts)
        {
            if (channelPost is null) { return Task.CompletedTask; }
            FileSaveAssistantAsync(channelPost, typeof(Message), "channelPost", cts);
            return Task.CompletedTask;
        }

        private static Task HandleEditedChannelPostAsync(Message? editedChannelPost, CancellationToken cts)
        {
            if (editedChannelPost is null) { return Task.CompletedTask; }
            FileSaveAssistantAsync(editedChannelPost, typeof(Message), "editedChannelPost", cts);
            return Task.CompletedTask;
        }

        private static Task HandleShippingQueryAsync(ShippingQuery? shippingQuery, CancellationToken cts)
        {
            return Task.CompletedTask;//throw new NotImplementedException();
        }

        private static Task HandlePreCheckoutQueryAsync(PreCheckoutQuery? preCheckoutQuery, CancellationToken cts)
        {
            return Task.CompletedTask;//throw new NotImplementedException();
        }

        private static Task HandlePollAsync(Poll? poll, CancellationToken cts)
        {
            if (poll is null) { return Task.CompletedTask; }
            FileSaveAssistantAsync(poll, typeof(Poll), "poll", cts);
            return Task.CompletedTask;
        }

        private static Task HandlePollAnswerAsync(PollAnswer? pollAnswer, CancellationToken cts)
        {
            if (pollAnswer is null) { return Task.CompletedTask; }
            FileSaveAssistantAsync(pollAnswer, typeof(PollAnswer), "pollAnswer", cts);
            return Task.CompletedTask;
        }

        private static Task HandleMyChatMemberAsync(ChatMemberUpdated? myChatMember, CancellationToken cts)
        {
            if (myChatMember is null) { return Task.CompletedTask; }
            FileSaveAssistantAsync(myChatMember, typeof(ChatMemberUpdated), "myChatMember", cts);
            return Task.CompletedTask;
        }

        private static Task HandleChatMemberAsync(ChatMemberUpdated? chatMember, CancellationToken cts)
        {
            if (chatMember is null) { return Task.CompletedTask; }
            FileSaveAssistantAsync(chatMember, typeof(ChatMemberUpdated), "chatMember", cts);
            return Task.CompletedTask;
        }

        private static Task HandleChatJoinRequestAsync(ChatJoinRequest? chatJoinRequest, CancellationToken cts)
        {
            if (chatJoinRequest is null) { return Task.CompletedTask; }
            FileSaveAssistantAsync(chatJoinRequest, typeof(ChatJoinRequest), "chatJoinRequest", cts);
            return Task.CompletedTask;
        }

        private static Task HandleUnknownAsync(Update update, CancellationToken cts)
        {
            if (update is null) { return Task.CompletedTask; }
            FileSaveAssistantAsync(update, typeof(Update), "Unknown", cts);
            return Task.CompletedTask;
        }

        private static Task FileSaveAssistantAsync(object obj, Type type, string metodName, CancellationToken cts)
        {
            if (obj is null)    return Task.CompletedTask;
            if (type is null)   return Task.CompletedTask;
            if (metodName is null)  return Task.CompletedTask;

            var json = JsonSerializer.Serialize(obj, type, optionsJson);

            string dir = $"{_directory}{metodName}/";
            Directory.CreateDirectory(dir);
            string fileName = DateTime.Now.ToString("ddMMyyyy_HHmmss_ffff") + "-" + _fileName;
            string path = Path.Combine(dir, fileName);
            System.IO.File.WriteAllTextAsync(path, json, cts);

            Console.WriteLine($"new file created\t {dir}{fileName}");

            return Task.CompletedTask;
        }
    }
}