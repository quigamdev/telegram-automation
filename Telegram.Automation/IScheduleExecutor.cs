
namespace Telegram.Automation;

public interface IScheduleExecutor
{
    Task AddToSchedule(int scheduleId, ScheduleItem data);
    void CreateSchadule(string name);
    Task Execute(CancellationToken cancellationToken);
    Task<List<ScheduleItem>> GetPlan(int id);
    List<ScheduleSimple> GetSchedules();
}