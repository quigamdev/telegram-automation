namespace Telegram.Automation;

public record ScheduleItem(
    string name, 
    string description, 
    ScheduleTime start, 
    ScheduleTime end);
