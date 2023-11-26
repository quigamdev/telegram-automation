namespace Telegram.Automation;

public interface IScheduleExecutor
{
    Task AddToSchedule(ScheduleItem data);
    void CreateSchadule();
    Task Execute(CancellationToken cancellationToken);
    Task<List<ScheduleItem>> GetPlan();
}
