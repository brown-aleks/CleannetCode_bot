using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using File = System.IO.File;

namespace CleannetCode_bot.Features.Welcome;

public class WelcomeBotClient : IWelcomeBotClient
{
    private readonly IOptionsMonitor<WelcomeBotClientOptions> _optionsMonitor;
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly IWelcomeStickersBotClient _welcomeStickersBotClient;

    public WelcomeBotClient(
        ITelegramBotClient telegramBotClient,
        IOptionsMonitor<WelcomeBotClientOptions> optionsMonitor,
        IWelcomeStickersBotClient welcomeStickersBotClient)
    {
        _telegramBotClient = telegramBotClient;
        _optionsMonitor = optionsMonitor;
        _welcomeStickersBotClient = welcomeStickersBotClient;
    }

    private WelcomeBotClientOptions Options => _optionsMonitor.CurrentValue;

    public async Task SendWelcomeMessageInCommonChatAsync(
        string username,
        long userId,
        long chatId,
        CancellationToken cancellationToken = default)
    {
        await _welcomeStickersBotClient.SendRandomWelcomeStickerFromSetAsync(chatId: chatId, cancellationToken: cancellationToken);

        var me = await _telegramBotClient.GetMeAsync(cancellationToken: cancellationToken);
        var urlToBot = $"https://t.me/{me.Username}";
        var markup = new InlineKeyboardMarkup(InlineKeyboardButton.WithUrl(text: "Ну давай, переходи", url: urlToBot));

        await _telegramBotClient.SendTextMessageAsync(
            chatId: chatId,
            text: $@"Привет {GetUserLink(username: username, userId: userId)}\. Добро пожаловать в коммьюнити\.
Чтобы удобно ориентироваться в сообществе добавь меня \(бота\) к себе\. Я тебе в деталях все расскажу\.",
            parseMode: ParseMode.MarkdownV2,
            messageThreadId: Options.WelcomeThreadIdByChatId.GetValueOrDefault(chatId),
            replyMarkup: markup,
            cancellationToken: cancellationToken);
    }

    public async Task SendYoutubePromptAsync(
        long chatId,
        CancellationToken cancellationToken = default)
    {
        await _telegramBotClient.SendTextMessageAsync(
            chatId: chatId,
            text: @"Твое никнейм на Youtube\. Служит для идентификации тебя на стримах: ",
            parseMode: ParseMode.MarkdownV2,
            cancellationToken: cancellationToken);
    }

    public async Task SendYoutubeConfirmedAsync(
        long chatId,
        CancellationToken cancellationToken = default)
    {
        await _telegramBotClient.SendTextMessageAsync(
            chatId: chatId,
            text: @"Успешно заполнен Youtube\.",
            parseMode: ParseMode.MarkdownV2,
            cancellationToken: cancellationToken);
    }

    public async Task SendGithubPromptAsync(
        long chatId,
        CancellationToken cancellationToken = default)
    {
        await using var githubNicknameExample = File.OpenRead("github_nickname.png");
        await _telegramBotClient.SendPhotoAsync(
            chatId: chatId,
            new InputFile(githubNicknameExample),
            caption: @"Твой никнейм на Github \(профиль должен быть доступ через https://github\.com/\<твой ник\>\)\. Будет использоваться для домашек: ",
            parseMode: ParseMode.MarkdownV2,
            cancellationToken: cancellationToken);
    }

    public async Task SendGithubConfirmedAsync(
        long chatId,
        CancellationToken cancellationToken = default)
    {
        await _telegramBotClient.SendTextMessageAsync(
            chatId: chatId,
            text: @"Успешно заполнен Github",
            parseMode: ParseMode.MarkdownV2,
            cancellationToken: cancellationToken);
    }

    public async Task SendGithubInvalidProfileAsync(
        long chatId,
        CancellationToken cancellationToken = default)
    {
        await _telegramBotClient.SendTextMessageAsync(
            chatId: chatId,
            text: @"Профиль не найден на Github",
            parseMode: ParseMode.MarkdownV2,
            cancellationToken: cancellationToken);
    }

    public async Task SendInformationAsync(
        long chatId,
        string? username,
        string? githubNick,
        string? youtubeName,
        CancellationToken cancellationToken = default)
    {
        await _telegramBotClient.SendTextMessageAsync(
            chatId: chatId,
            text: $@"Что я знаю о тебе
Имя профиля: {username ?? "Не знаю"},
Github: {githubNick ?? "Не знаю"},
Youtube: {youtubeName ?? "Не знаю"}\.",
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken);
    }

    public async Task SendInformationRemovedSuccessfulAsync(
        long chatId,
        CancellationToken cancellationToken = default)
    {
        var markup = new ReplyKeyboardMarkup(
            new[]
            {
                new KeyboardButton(WelcomeBotCommandNames.StartCommand)
            });
        await _telegramBotClient.SendTextMessageAsync(
            chatId: chatId,
            text: @"Информация удалена",
            replyMarkup: markup,
            cancellationToken: cancellationToken);
    }

    public async Task SendWelcomeMessageInPersonalChatAsync(
        string username,
        long userId,
        long chatId,
        CancellationToken cancellationToken = default)
    {
        await _welcomeStickersBotClient.SendRandomWelcomeStickerFromSetAsync(chatId: chatId, cancellationToken: cancellationToken);
        await _telegramBotClient.SendTextMessageAsync(
            chatId: chatId,
            text: $@"Снова привет {GetUserLink(username: username, userId: userId)}

Прежде чем продолжить общение, давайте убедимся, что вы на правильном пути\.
Разработка для вас \– это серьезное увлечение, вы хотите разобраться и научиться, или уже обладаете навыками и хотите проверить свои умения\.
В нашем комьюнити каждый участник найдет свое дело :\)

Новичкам мы полезны тем, что предоставляем поддержку в изучении программирования\.
Опытные разработчики могут улучшить свои soft skills, преодолеть синдром самозванца и получить удовлетворение от оказания помощи другим участникам\.


В нашем комьюнити ты сможешь найти много интересного, ниже представлены все основные ссылки\.
[Наш основной чат](https://t.me/cleannetcode)
[Стримы тут](https://www.youtube.com/@Cleannetcode)
[Гитхаб нашего комьюнити](https://github.com/cleannetcode)
[LinkedIn группа](https://www.linkedin.com/groups/9315319/)
Занятия и домашние задания пока доступны в виде видео и стримов на YouTube, но мы разрабатываем отдельную платформу для этого\.
Мониторинг прогресса и система достижений также в процессе разработки\.

Чтобы максимально активно участвовать в сообществе и получать ревью домашних заданий и проектов, а также отслеживать свой прогресс, нам нужна информация о ваших аккаунтах в Telegram, Github и YouTube \(личная персональная информация не требуется\)\.
Информацию о вашем аккаунте в Telegram мы уже получили \- хе\-хе\.
Осталось заполнить данные о YouTube и Github\.

{WelcomeBotCommandNames.ChangeGithubInfoCommand} \- показать собранную информацию
{WelcomeBotCommandNames.ChangeYoutubeInfoCommand} \- удалить ваши данные
{WelcomeBotCommandNames.ClearMyInfoCommand} \- указать или изменить никнейм пользователя на Youtube
{WelcomeBotCommandNames.GetMyInfoCommand} \- указать или изменить Github никнейм",
            parseMode: ParseMode.MarkdownV2,
            cancellationToken: cancellationToken);
    }

    private static string GetUserLink(string username, long userId)
    {
        return $"[@{username}](tg://user?id={userId})";
    }
}