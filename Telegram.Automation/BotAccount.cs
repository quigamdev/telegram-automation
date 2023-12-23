namespace Telegram.Automation;

public class BotAccount
{
    public string Name { get; set; }
    public string AccountNumber { get; set; }
    public BotAccountStatus Status { get; set; }
    public bool IsScheduled { get; set; }
    public DateTime NextSchedule { get; set; }
}
