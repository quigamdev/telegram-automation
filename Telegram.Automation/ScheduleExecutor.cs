namespace Telegram.Automation;

public class ScheduleExecutor : IScheduleExecutor
{
    private readonly AccountsManager manager;
    private readonly ScheduleStore store;
    private List<ScheduleItem> ActiveItems = new();

    public ScheduleExecutor(AccountsManager manager, ScheduleStore store)
    {
        this.manager = manager;
        this.store = store;
    }

    public async Task AddToScheduleAsync(AccountScheduleRequest data, int concurrency)
    {
        var accounts = store.GetAccountsForScheduling();
        accounts.Add(data.AccountNumber);
        accounts = accounts.Distinct().ToList();

        store.SaveAccountsForScheduling(accounts);

        await CreateSchedule("All", concurrency);
    }

    public async Task CreateSchedule(string name, int concurrency)
    {
        var accountsForScheduling = store.GetAccountsForScheduling();
        var scheduleItems = await manager.GetBotAccountsAsync();

        scheduleItems = scheduleItems
             .Where(s => accountsForScheduling.Contains(s.AccountNumber)).ToList();

        scheduleItems = scheduleItems.Concat(scheduleItems).ToList(); // schedule the accounts twice

        List<ScheduleItem> scheduled = CreateSchedulePlan(scheduleItems, concurrency);

        var schedule = new Schedule()
        {
            Name = name,
            Plan = scheduled,
            Id = 1
        };

        store.Save(schedule);
    }
    private List<ScheduleItem> CreateSchedulePlan(List<BotAccount> scheduleItems, int concurrent)
    {
        var count = scheduleItems.Count;
        
        var n = Math.Ceiling(count / (double)concurrent);
        var period = (24 * 60 * 60 - 1) / n;

        var scheduled = scheduleItems.Select((acc, i) =>
            {
                var j = i / concurrent;
                var start = TimeSpan.FromSeconds(j * period);
                var end = TimeSpan.FromSeconds((j + 1) * period);
                return new ScheduleItem(acc.Name, acc.AccountNumber, acc.AccountNumber,
                        new ScheduleTime(start.Hours, start.Minutes, start.Seconds),
                        new ScheduleTime(end.Hours, end.Minutes, end.Seconds));
            })
         .ToList();
        return scheduled;
    }

    public async Task Execute(CancellationToken cancellationToken)
    {
        var schedules = store.GetActiveSchedules();
        foreach (var schedule in schedules)
        {
            await ExecuteSchedule(schedule, cancellationToken);
        }
    }

    private async Task ExecuteSchedule(Schedule schedule, CancellationToken cancellationToken)
    {

        var current = GetCurrent(schedule.Plan);

        if (current.SequenceEqual(ActiveItems)) return;

        if (ActiveItems.Count == 0)
        {
            if (current.Count != 0)
            {
                await StopUnattanded(schedule.Plan, current, cancellationToken);

                var accounts = await manager.GetBotAccountsAsync();
                var currentNames = current.Select(s => s.accountNumber).ToList();
                var toBeActivated = accounts.Where(s => currentNames.Contains(s.AccountNumber) && s.Status != BotAccountStatus.Online).ToList();
                if (toBeActivated.Count == 0)
                {
                    // skip if current is already active
                    ActiveItems = current;
                    return;
                }
                foreach (var item in toBeActivated)
                {
                    // Activate the current
                    await Task.Delay(Random.Shared.Next(100, 2_000), cancellationToken);
                    if (cancellationToken.IsCancellationRequested) return;

                    await manager.StartAccount(item.AccountNumber);
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
                    await Task.Delay(Random.Shared.Next(100, 3_000), cancellationToken);
                    if (cancellationToken.IsCancellationRequested) return;

                    await manager.StopAccount(item.accountNumber);
                }
                ActiveItems = new();
            }
            else
            {
                foreach (var item in ActiveItems)
                {
                    // main switching sequence
                    await Task.Delay(Random.Shared.Next(100, 10_000), cancellationToken);
                    if (cancellationToken.IsCancellationRequested) return;
                    await manager.StopAccount(item.accountNumber);
                }
                foreach (var item in current)
                {
                    await Task.Delay(Random.Shared.Next(100, 10_000), cancellationToken);
                    if (cancellationToken.IsCancellationRequested) return;
                    await manager.StartAccount(item.accountNumber);
                }
                ActiveItems = current;
            }
        }
    }

    private async Task StopUnattanded(List<ScheduleItem> schedule, List<ScheduleItem> current, CancellationToken token)
    {
        var accountsInSchedule = schedule.Select(s => s.accountNumber).ToList();
        var accounts = await manager.GetBotAccountsAsync();

        var currentAccs = current.Select(s => s.accountNumber).ToList();
        var toBeStoped = accounts.Where(s => s.Status == BotAccountStatus.Online)
            .Where(s => accountsInSchedule.Contains(s.AccountNumber))
            .Where(s => !currentAccs.Contains(s.AccountNumber)).ToList();

        foreach (var a in toBeStoped)
        {
            if (token.IsCancellationRequested) return;
            await Task.Delay(Random.Shared.Next(100, 1000), token);
            await manager.StopAccount(a.AccountNumber);
        }

    }

    private List<ScheduleItem> GetCurrent(List<ScheduleItem> schedule)
    {
        var now = DateTime.Now;
        return schedule.Where(
            s => new TimeSpan(s.start.hour, s.start.minute, s.start.seconds) <= now.TimeOfDay &&
            new TimeSpan(s.end.hour, s.end.minute, s.end.seconds) > now.TimeOfDay).ToList();
    }

    public Task<List<ScheduleItem>> GetPlan(int id = 1)
    {
        var schedule = store.GetSchedule().Plan.OrderBy(s => s.name).ToList();
        return Task.FromResult(schedule.ToList());
    }

    public async Task RemoveFromSchedule(AccountScheduleRequest data, int concurrency)
    {
        var accounts = store.GetAccountsForScheduling();
        accounts = accounts.Where(s => data.AccountNumber != s).ToList();

        store.SaveAccountsForScheduling(accounts);
        await CreateSchedule("All", concurrency);
    }
}
