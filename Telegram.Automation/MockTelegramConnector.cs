namespace Telegram.Automation;

public class MockTelegramConnector : ITelegramConnector
{
    public Task CheckAuthCode(string code)
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
    }

    public IEnumerable<MessageLog> GetLog()
    {
        return new List<MessageLog>();
    }

    public Task InitAuthentication()
    {
        return Task.CompletedTask;
    }

    public Task<AuthenticationResult> IsAuthenticated()
    {
        return Task.FromResult(AuthenticationResult.Authenticated);
    }

    public async Task<string> SendMessage(string message, Func<string, bool> messagePredicate, CancellationToken? token = null)
    {
        await Task.CompletedTask;
        if (message == CommandBuilder.GetAccountsStatus()) return File.ReadAllText("bin/debug/net9.0/Mocks/StatusCommandResponse.txt");

        if (message.StartsWith(CommandBuilder.StartAccount(""))) return "Starting account...";

        if (message.StartsWith(CommandBuilder.StopAccount(""))) return "Stopping account...";

        return "";
    }

    public Task<AuthenticationResult> Start()
    {
        return Task.FromResult(AuthenticationResult.Authenticated);
    }
}
