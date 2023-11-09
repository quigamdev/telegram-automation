namespace Telegram.Automation;

public interface IScheduleExecutor
{
    Task Execute(CancellationToken cancellationToken);
    Task<List<ScheduleItem>> GetPlan();
}
