using LazyCache;
using System.Data.Common;
using Telegram.Automation;

public class AccountsManager
{
    private const string CachKey = "Accounts";
    private IAppCache cache = new CachingService();

    public AccountsManager(ITelegramConnector telegramConnector)
    {
        connector = telegramConnector;
    }

    private ITelegramConnector connector { get; }

    public async Task<List<BotAccount>> GetBotAccountsAsync()
    {
        return await cache.GetOrAddAsync(CachKey, async (e) =>
        {
            e.AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(2);
            return await RefreshBotsAccounts();
        });
    }

    private async Task<List<BotAccount>> RefreshBotsAccounts()
    {
        var command = CommandBuilder.GetAccountsStatus();

        await InitConnector();
        var message = await connector.SendMessage(command, MessageProcessor.IsStatusMessage);

        return MessageProcessor.ProcessStatusMessage(message)
            .OrderBy(s => s.Name).ToList();
    }

    private async Task InitConnector()
    {
        var result = await connector.Start();
        if (result != AuthenticationResult.Authenticated)
        {
            throw new Exception("Authentication Failed!");
        }
    }

    public async Task<string> StartAccount(string account)
    {
        await InitConnector();
        var command = CommandBuilder.StartAccount(account);

        var response = await connector.SendMessage(command, MessageProcessor.IsStartingAccountMessage);
        cache.Remove(CachKey);
        return response;
    }
    public async Task<string> StopAccount(string account)
    {
        await InitConnector();
        var command = CommandBuilder.StopAccount(account);

        var response = await connector.SendMessage(command, MessageProcessor.IsStopingAccountMessage);
        cache.Remove(CachKey);
        return response;
    }
}