using CleannetCodeBot.Infrastructure.DataAccess.Interfaces;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace CleannetCodeBot.Features.Welcome.HandlerChains;

public class RequestInformationHandlerChain : WelcomePrivateHandlerChain
{
    private readonly ILogger<RequestInformationHandlerChain> _logger;

    public RequestInformationHandlerChain(
        IWelcomeBotClient welcomeBotClient,
        IGenericRepository<long, WelcomeUserInfo> welcomeUserInfoRepository,
        ILogger<RequestInformationHandlerChain> logger) : base(
        welcomeBotClient: welcomeBotClient,
        welcomeUserInfoRepository: welcomeUserInfoRepository)
    {
        _logger = logger;
    }

    protected override WelcomeUserInfoState TargetState => WelcomeUserInfoState.Idle;

    protected override async Task<Result> ProcessUserAsync(
        long userId,
        WelcomeUserInfo user,
        string text,
        CancellationToken cancellationToken)
    {
        if (text != WelcomeBotCommandNames.GetMyInfoCommand)
            return WelcomeHandlerHelpers.NotMatchingStateResult;
        await WelcomeBotClient.SendInformationAsync(
            chatId: user.PersonalChatId!.Value,
            username: user.Username,
            githubNick: user.GithubNick,
            youtubeName: user.YoutubeName,
            cancellationToken: cancellationToken);
        _logger.LogInformation(message: "{Result}", "Success return information");
        return Result.Success();
    }
}