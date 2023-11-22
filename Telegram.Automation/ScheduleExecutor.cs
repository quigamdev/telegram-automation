using Microsoft.Extensions.Logging;

namespace Telegram.Automation;

public class ScheduleExecutor : IScheduleExecutor
{
    private readonly ILogger<ScheduleExecutor> logger;
    private readonly AccountsManager manager;
    private readonly ScheduleStore store;
    public ScheduleExecutor(ILogger<ScheduleExecutor> logger, AccountsManager manager, ScheduleStore store)
    {
        this.logger = logger;
        this.manager = manager;
        this.store = store;
    }

    public Task AddToSchedule(ScheduleItem data)
    {
        var schedule = store.GetSchedule();
        schedule.Add(data);
        store.Save(schedule);
        return Task.CompletedTask;
    }

    public Task Execute(CancellationToken cancellationToken)
    {
        logger.LogInformation($"Scheduled action triggered ({DateTime.Now})");
        return Task.CompletedTask;
    }

    public Task<List<ScheduleItem>> GetPlan()
    {
        var schedule = store.GetSchedule().OrderBy(s => s.name).ToList();
        return Task.FromResult(schedule.ToList());
    }
}
