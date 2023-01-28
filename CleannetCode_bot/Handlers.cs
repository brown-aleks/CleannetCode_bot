using CleannetCode_bot.Features.Welcome;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.Payments;

namespace CleannetCode_bot
{
    public class Handlers
    {
        private readonly IStorageService storage;
        private readonly WelcomeHandler welcomeHandler;

        public Handlers(IStorageService storage, WelcomeHandler welcomeHandler)
        {
            this.storage = storage;
            this.welcomeHandler = welcomeHandler;
        }
        public Task CallbackQueryAsync(CallbackQuery? callbackQuery, CancellationToken cts)
        {
            if (callbackQuery is null) { return Task.CompletedTask; }
            storage.AddObject(callbackQuery, typeof(CallbackQuery), "callbackQuery", cts);
            return Task.CompletedTask;
        }

        public Task ChannelPostAsync(Message? channelPost, CancellationToken cts)
        {
            if (channelPost is null) { return Task.CompletedTask; }
            storage.AddObject(channelPost, typeof(Message), "channelPost", cts);
            return Task.CompletedTask;
        }

        public Task ChatJoinRequestAsync(ChatJoinRequest? chatJoinRequest, CancellationToken cts)
        {
            if (chatJoinRequest is null) { return Task.CompletedTask; }
            storage.AddObject(chatJoinRequest, typeof(ChatJoinRequest), "chatJoinRequest", cts);
            return Task.CompletedTask;
        }

        public async Task ChatMemberAsync(ChatMemberUpdated? chatMember, CancellationToken cts)
        {
            if (chatMember is null) { return; }
            await storage.AddObject(chatMember, typeof(ChatMemberUpdated), "chatMember", cts);
            if (chatMember.NewChatMember.Status is ChatMemberStatus.Member)
                await welcomeHandler.HandleChatMember(chatMember.NewChatMember.User, chatMember.Chat.Id);
        }

        public Task ChosenInlineResultAsync(ChosenInlineResult? chosenInlineResult, CancellationToken cts)
        {
            if (chosenInlineResult is null) { return Task.CompletedTask; }
            storage.AddObject(chosenInlineResult, typeof(ChosenInlineResult), "chosenInlineResult", cts);
            return Task.CompletedTask;
        }

        public Task EditedChannelPostAsync(Message? editedChannelPost, CancellationToken cts)
        {
            if (editedChannelPost is null) { return Task.CompletedTask; }
            storage.AddObject(editedChannelPost, typeof(Message), "editedChannelPost", cts);
            return Task.CompletedTask;
        }

        public Task EditedMessageAsync(Message? editedMessage, CancellationToken cts)
        {
            if (editedMessage is null) { return Task.CompletedTask; }
            storage.AddObject(editedMessage, typeof(Message), "editedMessage", cts);
            return Task.CompletedTask;
        }

        public Task InlineQueryAsync(InlineQuery? inlineQuery, CancellationToken cts)
        {
            if (inlineQuery is null) { return Task.CompletedTask; }
            storage.AddObject(inlineQuery, typeof(InlineQuery), "inlineQuery", cts);
            return Task.CompletedTask;
        }

        public async Task MessageAsync(Message? message, CancellationToken cts)
        {
            if (message is null) { return; }
            await storage.AddObject(message, typeof(Message), "Message", cts);
            await welcomeHandler.HandleAnswersAsync(message);
 #region reminder
            //  Обнаружение команд к боту
            /*
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
            */

            // Эхо полученного текста сообщения
            /*
            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chat.Id,
                text: $"{message.ForwardSenderName} написал:\n" + messageText,  //  chat.Username
                cancellationToken: cancellationToken,
                replyToMessageId: message.MessageId);
            */
            #endregion
        }

        public Task MyChatMemberAsync(ChatMemberUpdated? myChatMember, CancellationToken cts)
        {
            if (myChatMember is null) { return Task.CompletedTask; }
            storage.AddObject(myChatMember, typeof(ChatMemberUpdated), "myChatMember", cts);
            return Task.CompletedTask;
        }

        public Task PollAnswerAsync(PollAnswer? pollAnswer, CancellationToken cts)
        {
            if (pollAnswer is null) { return Task.CompletedTask; }
            storage.AddObject(pollAnswer, typeof(PollAnswer), "pollAnswer", cts);
            return Task.CompletedTask;
        }

        public Task PollAsync(Poll? poll, CancellationToken cts)
        {
            if (poll is null) { return Task.CompletedTask; }
            storage.AddObject(poll, typeof(Poll), "poll", cts);
            return Task.CompletedTask;
        }

        public Task PreCheckoutQueryAsync(PreCheckoutQuery? preCheckoutQuery, CancellationToken cts)
        {
            return Task.CompletedTask;//throw new NotImplementedException();
        }

        public Task ShippingQueryAsync(ShippingQuery? shippingQuery, CancellationToken cts)
        {
            return Task.CompletedTask;//throw new NotImplementedException();
        }

        public Task UnknownAsync(Update update, CancellationToken cts)
        {
            if (update is null) { return Task.CompletedTask; }
            storage.AddObject(update, typeof(Update), "Unknown", cts);
            return Task.CompletedTask;
        }
    }
}