namespace Telegram.Automation;

public enum AuthenticationResult
{
    Unauthorized = 0,
    WaitingForCode,
    Authenticated, 
}