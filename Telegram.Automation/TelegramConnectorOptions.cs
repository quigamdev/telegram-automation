namespace Telegram.Automation;

public class TelegramConnectorOptions
{
    public int ApiID { get; set; }
    public string ApiHash { get; set; }
    public string DatabaseLocation { get; set; }
    public string PhoneNumber { get; set; }
    public long AutomationChatId { get; set; }
    public string AutomationChatName { get; set; }
}

