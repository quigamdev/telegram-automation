namespace Telegram.Automation;

public interface ITelegramConnector : IDisposable
{
    Task CheckAuthCode(string code);
    Task InitAuthentication();
    Task<string> SendMessage(string message, CancellationToken? token = null);
    Task<AuthenticationResult> Start();
}
