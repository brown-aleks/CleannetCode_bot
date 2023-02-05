using System.Collections.Concurrent;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using File = System.IO.File;

namespace CleannetCode_bot.Features.Welcome;

public record Position(int Id, string Name);

public enum State
{
    Github,
    Youtube,
    Position,
    End
}

public record WelcomeUserInfo(
    long Id,
    string Username,
    string FirstName,
    string LastName,
    long? YoutubeMessageId,
    string? Youtube,
    long? GithubMessageId,
    string? Github,
    long? PositionMessageId,
    int? PositionId,
    State State);

public class WelcomeHandler
{
    private readonly ITelegramBotClient _client;

    private ConcurrentDictionary<long, SemaphoreSlim> _writeSemaphoreSlims = new();

    private async Task SaveAsync(WelcomeUserInfo info)
    {
        using var semaphore = _writeSemaphoreSlims.GetOrAdd(info.Id, new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync();
        await using var fileStream = File.OpenWrite(Path.Combine("Welcomes", info.Id.ToString()));
        await JsonSerializer.SerializeAsync(fileStream, info);
    }

    private async Task<WelcomeUserInfo?> ReadAsync(long id)
    {
        using var semaphore = _writeSemaphoreSlims.GetOrAdd(id, new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync();
        var fileName = GetFileName(id);
        if (!File.Exists(fileName)) return null;
        await using var fileStream = File.OpenWrite(fileName);
        var result = await JsonSerializer.DeserializeAsync<WelcomeUserInfo>(fileStream);
        return result;
    }

    private static string GetFileName(long id)
    {
        return Path.Combine("Welcomes", id.ToString());
    }

    private static readonly ConcurrentDictionary<long, long> RepliesMessage = new();

    private static readonly Dictionary<int, Position> Positions = new()
    {
        { 1, new(1, "Учусь") },
        { 2, new(2, "Стажируюсь") },
        { 3, new(3, "Джуниор") },
        { 4, new(4, "Мидл") },
        { 5, new(5, "Сеньер") },
        { 6, new(6, "Лид") },
    };

    public WelcomeHandler(ITelegramBotClient client)
    {
        _client = client;
    }

    public async Task ReplyHandle(Message message)
    {
        if (message.From is null
            || message.ReplyToMessage is null
            || message.Text is null
            || !RepliesMessage.TryGetValue(message.ReplyToMessage.MessageId, out var userId))
            return;

        var user = await ReadAsync(userId);
        if (user is null)
            return;

        switch (user.State)
        {
            case State.Github:
                await HandleGithubAsync(message.Text, message.Chat.Id, user);
                break;
            case State.Youtube:
                await HandleYoutube(message.Text, message.Chat.Id, user);
                break;
            case State.Position:
                await HandleGithubAsync(message.Text, message.Chat.Id, user);
                break;
            case State.End:
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(message));
        }
    }

    private async Task HandleGithubAsync(string text, long chatId, WelcomeUserInfo user)
    {
        var message = await _client.SendTextMessageAsync(chatId,
            $"@{user.Username}, А твой ник на ютьюбе (ответь на это сообщение):");
        user = user with { Github = text, YoutubeMessageId = message.MessageId, State = State.Youtube };
        await SaveAsync(user);
        RepliesMessage.AddOrUpdate(message.MessageId, user.Id, (_, _) => user.Id);
    }

    private async Task HandleYoutube(string text, long chatId, WelcomeUserInfo user)
    {
        var message = await _client.SendTextMessageAsync(chatId,
            $"@{user.Username}, А уровень (Учусь, Стажируюсь, Джуниор, Мидл, Сеньер, Лид) (ответь на это сообщение):");
        user = user with { Youtube = text, PositionMessageId = message.MessageId, State = State.Position };
        await SaveAsync(user);
        RepliesMessage.AddOrUpdate(message.MessageId, user.Id, (_, _) => user.Id);
    }

    private async Task HandlePosition(string text, long messageId, long chatId, WelcomeUserInfo user)
    {
        var selected = Positions.Select(x => x.Value)
            .FirstOrDefault(x => x.Name.Equals(text, StringComparison.CurrentCultureIgnoreCase));
        if (selected is not null)
        {
            user = user with { PositionId = selected.Id, State = State.End };
            await SaveAsync(user);
            RepliesMessage.Remove(messageId, out _);
        }

        var message = await _client.SendTextMessageAsync(chatId,
            $"@{user.Username}, А уровень (Учусь, Стажируюсь, Джуниор, Мидл, Сеньер, Лид) (ответь на это сообщение еще раз):");
        user = user with { PositionMessageId = message.MessageId };
        await SaveAsync(user);
        RepliesMessage.AddOrUpdate(message.MessageId, user.Id, (_, _) => user.Id);
    }

    public async Task WelcomeMemberHandle(User user, long chatId)
    {
        var fromId = user.Id;
        var nick = user.FirstName;
        var githubMessage = await _client.SendTextMessageAsync(chatId,
            $"Привет! 👀👀👀 @{nick}. Твой никнейм в гитхаб (ответь на это сообщение):");
        var userInfo = new WelcomeUserInfo(fromId,
            user.Username ?? string.Empty,
            user.FirstName,
            user.LastName ?? string.Empty,
            null,
            string.Empty,
            githubMessage.MessageId,
            string.Empty,
            null,
            null,
            State.Github);
        RepliesMessage.AddOrUpdate(githubMessage.MessageId, fromId, (_, _) => fromId);
        await SaveAsync(userInfo);
    }
}