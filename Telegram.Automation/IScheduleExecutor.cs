namespace Telegram.Automation;

public interface IScheduleExecutor
{
    Task AddToSchedule(ScheduleItem data);
    Task Execute(CancellationToken cancellationToken);
    Task<List<ScheduleItem>> GetPlan();
}
