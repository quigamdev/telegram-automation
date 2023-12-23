namespace Telegram.Automation;

public record ScheduleItem(
    string name, 
    string accountNumber, 
    string description, 
    ScheduleTime start, 
    ScheduleTime end);
