
namespace Telegram.Automation;

public interface IScheduleExecutor
{
    Task AddToScheduleAsync(AccountScheduleRequest data);
    Task RemoveFromSchedule(AccountScheduleRequest data);
    Task CreateSchedule(string name);
    Task Execute(CancellationToken cancellationToken);
    Task<List<ScheduleItem>> GetPlan(int id = 1);
}