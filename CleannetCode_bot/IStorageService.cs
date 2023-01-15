namespace CleannetCode_bot
{
    public interface IStorageService
    {
        Task AddObject(object obj, Type type, string metodName, CancellationToken cts);
    }
}