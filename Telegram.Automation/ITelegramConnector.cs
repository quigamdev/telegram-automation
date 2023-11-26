namespace Telegram.Automation;

public interface ITelegramConnector : IDisposable
{
    Task CheckAuthCode(string code);
    Task InitAuthentication();
    Task<string> SendMessage(string message, Func<string, bool> messagePredicate, CancellationToken? token = null);
    Task<AuthenticationResult> Start();
    IEnumerable<MessageLog> GetLog();
    Task<AuthenticationResult> IsAuthenticated();
}
