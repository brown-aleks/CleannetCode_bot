using CleannetCodeBot.Infrastructure.DataAccess.Interfaces;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace CleannetCodeBot.Features.Welcome.HandlerChains;

public class YoutubeAnswerHandlerChain : WelcomePrivateHandlerChain
{
    private readonly ILogger<YoutubeAnswerHandlerChain> _logger;

    public YoutubeAnswerHandlerChain(
        IWelcomeBotClient welcomeBotClient,
        IGenericRepository<long, WelcomeUserInfo> welcomeUserInfoRepository,
        ILogger<YoutubeAnswerHandlerChain> logger) : base(
        welcomeBotClient: welcomeBotClient,
        welcomeUserInfoRepository: welcomeUserInfoRepository)
    {
        _logger = logger;
    }

    protected override WelcomeUserInfoState TargetState => WelcomeUserInfoState.AskingYoutube;

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
                YoutubeName = text, State = WelcomeUserInfoState.Idle
            },
            cancellationToken: cancellationToken);
        await WelcomeBotClient.SendYoutubeConfirmedAsync(
            chatId: user.PersonalChatId!.Value,
            cancellationToken: cancellationToken);
        _logger.LogInformation(message: "{Result}", "Success youtube answer handling");
        return Result.Success();
    }
}