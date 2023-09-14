namespace CleannetCodeBot.Features.Welcome;

public interface IWelcomeStickersBotClient
{
    Task SendRandomWelcomeStickerFromSetAsync(long chatId, CancellationToken cancellationToken);
}