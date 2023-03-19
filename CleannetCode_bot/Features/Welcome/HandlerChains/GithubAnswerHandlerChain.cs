using CleannetCode_bot.Infrastructure.DataAccess.Interfaces;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace CleannetCode_bot.Features.Welcome.HandlerChains;

public class GithubAnswerHandlerChain : WelcomePrivateHandlerChain
{
    private readonly ILogger<GithubAnswerHandlerChain> _logger;

    public GithubAnswerHandlerChain(
        IWelcomeBotClient welcomeBotClient,
        IGenericRepository<long, WelcomeUserInfo> welcomeUserInfoRepository,
        ILogger<GithubAnswerHandlerChain> logger) : base(
        welcomeBotClient: welcomeBotClient,
        welcomeUserInfoRepository: welcomeUserInfoRepository)
    {
        _logger = logger;
    }

    protected override WelcomeUserInfoState TargetState => WelcomeUserInfoState.AskingGithub;

    protected override async Task<Result> ProcessUserAsync(
        long userId,
        WelcomeUserInfo user,
        string text,
        CancellationToken cancellationToken)
    {
        await WelcomeUserInfoRepository.SaveAsync(
            key: userId,
            entity: user with
            {
                GithubNick = text, State = WelcomeUserInfoState.Idle
            },
            cancellationToken: cancellationToken);
        await WelcomeBotClient.SendGithubConfirmedAsync(
            chatId: user.PersonalChatId!.Value,
            cancellationToken: cancellationToken);
        // TODO: Сделать проверку на существование профиля в Github
        _logger.LogInformation(message: "{Result}", "Success github answer handling");
        return Result.Success();
    }
}