using LazyCache;
using System.Data.Common;
using Telegram.Automation;

public class AccountsManager
{
    private IAppCache cache = new CachingService();

    public AccountsManager(ITelegramConnector telegramConnector)
    {
        connector = telegramConnector;
    }

    private ITelegramConnector connector { get; }

    public async Task<List<BotAccount>> GetBotAccountsAsync()
    {
        return await cache.GetOrAddAsync("Accounts", async (e) =>
        {
            e.AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(2);
            return await RefreshBotsAccounts();
        });
    }

    private async Task<List<BotAccount>> RefreshBotsAccounts()
    {
        var command = CommandBuilder.GetAccountsStatus();

        await InitConnector();
        var message = await connector.SendMessage(command);

        return MessageProcessor.ProcessStatusMessage(message);
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
        return await connector.SendMessage(command);
    }
    public async Task<string> StopAccount(string account)
    {
        await InitConnector();
        var command = CommandBuilder.StopAccount(account);
        return await connector.SendMessage(command);

    }
}