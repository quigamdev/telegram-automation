namespace Telegram.Automation;

public class MessageLog
{
    public string Message { get; set; } = "";
    public DateTime ReceivedDate { get; set; }
    public string Sender { get; set; } = "";
}