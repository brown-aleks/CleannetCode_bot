using CleannetCodeBot.Infrastructure.DataAccess.Interfaces;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace CleannetCodeBot.Features.Welcome.HandlerChains;

public class RequestRemoveInformationHandlerChain : WelcomePrivateHandlerChain
{
    private readonly ILogger<RequestRemoveInformationHandlerChain> _logger;

    public RequestRemoveInformationHandlerChain(
        IWelcomeBotClient welcomeBotClient,
        IGenericRepository<long, WelcomeUserInfo> welcomeUserInfoRepository,
        ILogger<RequestRemoveInformationHandlerChain> logger) : base(
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
        if (text != WelcomeBotCommandNames.ClearMyInfoCommand)
            return WelcomeHandlerHelpers.NotMatchingStateResult;
        await WelcomeUserInfoRepository.RemoveAsync(key: userId, cancellationToken: cancellationToken);
        await WelcomeBotClient.SendInformationRemovedSuccessfulAsync(
            chatId: user.PersonalChatId!.Value,
            cancellationToken: cancellationToken);
        _logger.LogInformation(message: "{Result}", "Success information remove");
        return Result.Success();
    }
}