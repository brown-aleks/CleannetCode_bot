using CleannetCodeBot.Infrastructure.DataAccess.Interfaces;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace CleannetCodeBot.Features.Welcome.HandlerChains;

public class YoutubePromptHandlerChain : WelcomePrivateHandlerChain
{
    private readonly ILogger<YoutubePromptHandlerChain> _logger;

    public YoutubePromptHandlerChain(
        IWelcomeBotClient welcomeBotClient,
        IGenericRepository<long, WelcomeUserInfo> welcomeUserInfoRepository,
        ILogger<YoutubePromptHandlerChain> logger) : base(
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
        if (text != WelcomeBotCommandNames.ChangeYoutubeInfoCommand)
            return WelcomeHandlerHelpers.NotMatchingStateResult;

        await WelcomeUserInfoRepository.SaveAsync(
            key: userId,
            entity: user with
            {
                State = WelcomeUserInfoState.AskingYoutube
            },
            cancellationToken: cancellationToken);
        await WelcomeBotClient.SendYoutubePromptAsync(
            chatId: user.PersonalChatId!.Value,
            cancellationToken: cancellationToken);
        // TODO: Сделать проверку на существование профиля в Github
        _logger.LogInformation(message: "{Result}", "Success youtube prompt");
        return Result.Success();
    }
}