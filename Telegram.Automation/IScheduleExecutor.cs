
namespace Telegram.Automation;

public interface IScheduleExecutor
{
    Task AddToScheduleAsync(AccountScheduleRequest data, ScheduleOptions scheduleOptions);
    Task RemoveFromSchedule(AccountScheduleRequest data, ScheduleOptions scheduleOptions);
    Task CreateSchedule(string name, ScheduleOptions concurrency);
    Task Execute(CancellationToken cancellationToken);
    Task<List<ScheduleItem>> GetPlan(int id = 1);
}