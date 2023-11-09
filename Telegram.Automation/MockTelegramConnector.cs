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

    public async Task<string> SendMessage(string message, CancellationToken? token = null)
    {
        await Task.CompletedTask;
        if (message == CommandBuilder.GetAccountsStatus()) return File.ReadAllText("bin/debug/net7.0/Mocks/StatusCommandResponse.txt");

        return "";
    }

    public Task<AuthenticationResult> Start()
    {
        return Task.FromResult(AuthenticationResult.Authenticated);
    }
}
