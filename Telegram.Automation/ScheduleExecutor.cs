using Microsoft.Extensions.Logging;

namespace Telegram.Automation;

public class ScheduleExecutor : IScheduleExecutor
{
    private readonly ILogger<ScheduleExecutor> logger;
    private readonly AccountsManager manager;
    private readonly ScheduleStore store;
    private List<ScheduleItem> ActiveItems = new();

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

    public void CreateSchadule()
    {
        var scheduleItems = store.GetAccountsForScheduling();

        scheduleItems = scheduleItems.Concat(scheduleItems).ToList();

        var count = scheduleItems.Count();
        var period = 24 * 60 / count;
        var scheduled = scheduleItems.Select((name, i) =>
            new ScheduleItem(name, name, new ScheduleTime(i * period / 60, i * period % 60), new ScheduleTime((i + 1) * period / 60, (i + 1) * period % 60)))
             .ToList();

        store.Save(scheduled);
    }

    public async Task Execute(CancellationToken cancellationToken)
    {
        var schedule = store.GetSchedule();
        var current = GetCurrent(schedule);


        if (current.SequenceEqual(ActiveItems)) return;

        if (ActiveItems.Count == 0)
        {
            if (current.Count != 0)
            {
                await StopUnattanded(schedule, current);

                var accounts = await manager.GetBotAccountsAsync();
                var currentNames = current.Select(s => s.name).ToList();
                var toBeActivated = accounts.Where(s => currentNames.Contains(s.Name) && s.Status != BotAccountStatus.Online).ToList();
                if (toBeActivated.Count == 0)
                {
                    // skip if current is already active
                    ActiveItems = current;
                    return;
                }
                foreach (var item in toBeActivated)
                {
                    // Activate the current
                    await Task.Delay(Random.Shared.Next(100, 2_000));
                    await manager.StartAccount(item.Name);
                }
                ActiveItems = current;
            }
        }
        else
        {
            if (current.Count == 0)
            {
                foreach (var item in ActiveItems)
                {
                    await Task.Delay(Random.Shared.Next(100, 3_000));
                    await manager.StopAccount(item.name);
                }
                ActiveItems = new();
            }
            else
            {
                foreach (var item in ActiveItems)
                {
                    // main switching sequence
                    await Task.Delay(Random.Shared.Next(100, 16_000));
                    await manager.StopAccount(item.name);
                }
                foreach (var item in current)
                {
                    await Task.Delay(Random.Shared.Next(100, 16_000));
                    await manager.StartAccount(item.name);
                }
                ActiveItems = current;
            }
        }
    }

    private async Task StopUnattanded(List<ScheduleItem> schedule, List<ScheduleItem> current)
    {
        var accountsInSchedule = schedule.Select(s => s.name).ToList();
        var accounts = await manager.GetBotAccountsAsync();

        var currentAccs = current.Select(s => s.name).ToList();
        var toBeStoped = accounts.Where(s => s.Status == BotAccountStatus.Online)
            .Where(s => accountsInSchedule.Contains(s.Name))
            .Where(s => !currentAccs.Contains(s.Name)).ToList();

        foreach (var a in toBeStoped)
        {
            await Task.Delay(Random.Shared.Next(100, 1000));
            await manager.StopAccount(a.Name);
        }

    }

    private List<ScheduleItem> GetCurrent(List<ScheduleItem> schedule)
    {
        var now = DateTime.Now;
        return schedule.Where(
            s => s.start.hour * 60 + s.start.minute <= now.Hour * 60 + now.Minute &&
            s.end.hour * 60 + s.end.minute > now.Hour * 60 + now.Minute).ToList();
    }

    public Task<List<ScheduleItem>> GetPlan()
    {
        var schedule = store.GetSchedule().OrderBy(s => s.name).ToList();
        return Task.FromResult(schedule.ToList());
    }
}
