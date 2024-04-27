
namespace Telegram.Automation;

public interface IScheduleExecutor
{
    Task AddToScheduleAsync(AccountScheduleRequest data, int concurrency);
    Task RemoveFromSchedule(AccountScheduleRequest data, int concurrency);
    Task CreateSchedule(string name, int concurrency);
    Task Execute(CancellationToken cancellationToken);
    Task<List<ScheduleItem>> GetPlan(int id = 1);
}