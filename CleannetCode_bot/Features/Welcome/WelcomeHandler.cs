using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using File = System.IO.File;

namespace CleannetCode_bot.Features.Welcome;

public record Position(int Id, string Name);

public enum State
{
    None,
    Github,
    Youtube,
    Position,
    End
}

public record WelcomeUserInfo(
    long Id,
    string Username,
    string FirstName = "",
    string LastName = "",
    int? YoutubeMessageId = null,
    string? YoutubeNick = null,
    int? GithubMessageId = null,
    string? GithubNick = null,
    int? PositionMessageId = null,
    int? PositionId = null,
    Position? Position = null,
    State State = State.None);

public class WelcomeHandler
{
    private readonly IConfiguration config;
    private readonly ITelegramBotClient client;

    private readonly ConcurrentDictionary<long, SemaphoreSlim> writeSemaphoreSlims = new();

    private static readonly Dictionary<int, Position> Positions = new()
    {
        { 1, new(1, "Учусь") },
        { 2, new(2, "Стажируюсь") },
        { 3, new(3, "Джуниор") },
        { 4, new(4, "Мидл") },
        { 5, new(5, "Сеньер") },
        { 6, new(6, "Лид") },
    };

    public WelcomeHandler(ITelegramBotClient client, IConfiguration config)
    {
        this.client = client;
        this.config = config;
    }

    public async Task HandleAnswersAsync(Message message)
    {
        if (message.From is null
            || message.ReplyToMessage is null
            || message.Text is null)
            return;

        var user = await ReadAsync(message.From.Id);
        if (user is null)
            return;

        switch (user.State)
        {
            case State.Github when message.ReplyToMessage.MessageId == user.GithubMessageId:
                await HandleGithubAnswerAsync(message.Text, message.MessageId, message.Chat.Id, user);
                break;
            case State.Youtube when message.ReplyToMessage.MessageId == user.YoutubeMessageId:
                await HandleYoutubeAnswerAsync(message.Text, message.MessageId, message.Chat.Id, user);
                break;
            case State.Position when message.ReplyToMessage.MessageId == user.PositionMessageId:
                await HandlePositionAnswerAsync(message.Text, message.MessageId, message.Chat.Id, user);
                break;
            case State.None:
            case State.End:
            default:
                return;
        }
    }

    private async Task HandleGithubAnswerAsync(string text, int messageId, long chatId, WelcomeUserInfo user)
    {
        var message = await client.SendTextMessageAsync(chatId,
            $"@{user.Username}, А твой ник на ютьюбе (ответь на это сообщение):",
            replyToMessageId: messageId);
        user = user with { GithubNick = text, YoutubeMessageId = message.MessageId, State = State.Youtube };
        await SaveAsync(user);
    }

    private async Task HandleYoutubeAnswerAsync(string text, int messageId, long chatId, WelcomeUserInfo user)
    {
        var message = await client.SendTextMessageAsync(chatId,
            $"@{user.Username}, А уровень (Учусь, Стажируюсь, Джуниор, Мидл, Сеньер, Лид) (ответь на это сообщение):",
            replyToMessageId: messageId);
        user = user with { YoutubeNick = text, PositionMessageId = message.MessageId, State = State.Position };
        await SaveAsync(user);
    }

    private async Task HandlePositionAnswerAsync(string text, int messageId, long chatId, WelcomeUserInfo user)
    {
        var selected = Positions.Select(x => x.Value)
            .FirstOrDefault(x => x.Name.Equals(text, StringComparison.CurrentCultureIgnoreCase));
        if (selected is not null)
        {
            user = user with { Position = selected, PositionId = selected.Id, State = State.End };
            await SaveAsync(user);
            await client.SendTextMessageAsync(chatId,
                $"@{user.Username}, Анкета успешно собрана! 👌😁👍",
                replyToMessageId: messageId);
        }
        else
        {
            var message = await client.SendTextMessageAsync(chatId,
                $"@{user.Username}, Не понял 🤨. Твой уровень из списка (Учусь, Стажируюсь, Джуниор, Мидл, Сеньер, Лид) (ответь на это сообщение):",
                replyToMessageId: messageId);
            user = user with { PositionMessageId = message.MessageId };
            await SaveAsync(user);
        }
    }

    private async Task SaveAsync(WelcomeUserInfo info)
    {
        _ = await UsingSemaphoreSlim(info.Id,
            async () =>
            {
                var fileName = GetFileName(info.Id);
                var directoryName = Path.GetDirectoryName(fileName)!;
                if (!Directory.Exists(directoryName))
                    Directory.CreateDirectory(directoryName);
                await using var fileStream = File.OpenWrite(fileName);
                fileStream.SetLength(0);
                await JsonSerializer.SerializeAsync(fileStream, info);
                return true;
            });
    }
    
    private async Task<WelcomeUserInfo?> ReadAsync(long id)
    {
        return await UsingSemaphoreSlim(id,
            async () =>
            {
                var fileName = GetFileName(id);
                if (!File.Exists(fileName)) return null;
                await using var fileStream = File.OpenRead(fileName);
                var result = await JsonSerializer.DeserializeAsync<WelcomeUserInfo>(fileStream);
                return result;
            });
    }

    private async Task<T> UsingSemaphoreSlim<T>(long id, Func<Task<T>> action)
    {
        var semaphore = writeSemaphoreSlims.GetOrAdd(id, new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync();
        try { return await action(); }
        finally { semaphore.Release(); }
    }

    private string GetFileName(long id)
    {
        var basePath = config["WelcomesPath"] ?? "Welcomes";
        return Path.Combine(basePath, $"{id.ToString()}.json");
    }

    public async Task HandleChatMember(User member, long chatId)
    {
        var fromId = member.Id;
        var user = await ReadAsync(fromId);
        if (user is not null)
            return;

        user = new(fromId,
            member.Username ?? member.FirstName,
            member.FirstName,
            member.LastName ?? string.Empty,
            State: State.Github);

        var githubMessage = await client.SendTextMessageAsync(chatId,
            $"Привет! 👀👀👀 @{member.Username ?? member.FirstName}. Твой никнейм в гитхаб (ответь на это сообщение):");
        user = user with { GithubMessageId = githubMessage.MessageId };
        await SaveAsync(user);
    }
}