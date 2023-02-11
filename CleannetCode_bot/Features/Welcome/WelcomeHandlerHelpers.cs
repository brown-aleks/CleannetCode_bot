using CleannetCode_bot.Infrastructure;
using CSharpFunctionalExtensions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CleannetCode_bot.Features.Welcome;

public static class WelcomeHandlerHelpers
{
    public static Result NotMatchingStateResult { get; } = Result.Failure("Not matching state for welcome features handler");

    public static Result CheckAndGetPrivateChatParameters(this TelegramRequest request, out long userId, out string text)
    {
        userId = request.Update.Message?.From?.Id ?? default;
        text = request.Update.Message?.Text ?? string.Empty;

        if (request.Update.Message is not { Text: {}, From: {}, Chat: { Type: ChatType.Private } })
            return HandlerResults.NotMatchingType;
        return Result.Success();
    }

    public static WelcomeUserInfo ParseUser(this User member)
    {
        var username = member.Username
            ?? $"{member.FirstName}{(string.IsNullOrEmpty(member.LastName) ? string.Empty : " " + member.LastName)}";
        var user = new WelcomeUserInfo(
            Id: member.Id,
            Username: username,
            FirstName: member.FirstName,
            LastName: member.LastName ?? string.Empty);
        return user;
    }
}