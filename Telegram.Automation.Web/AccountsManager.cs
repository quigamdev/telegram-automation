using System.Data.Common;
using Telegram.Automation;

public class AccountsManager
{
    public AccountsManager(TelegramConnector telegramConnector)
    {
        connector = telegramConnector;
    }

    private TelegramConnector connector { get; }

    public async Task<List<BotAccount>> GetBotAccountsAsync()
    {
        return new List<BotAccount>();

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
}