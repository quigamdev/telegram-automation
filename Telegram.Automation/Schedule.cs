namespace Telegram.Automation;

public class Schedule
{
    public string Name { get; set; } = "Unknown";
    public int Id { get; set; }
    public List<ScheduleItem> Plan { get; set; } = new();
    public bool IsActive { get; set; } 
}
