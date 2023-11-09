using Microsoft.Extensions.Logging;

namespace Telegram.Automation;

public class ScheduleExecutor : IScheduleExecutor
{
    private readonly ILogger<ScheduleExecutor> logger;
    private readonly AccountsManager manager;

    public ScheduleExecutor(ILogger<ScheduleExecutor> logger, AccountsManager manager)
    {
        this.logger = logger;
        this.manager = manager;
    }
    public Task Execute(CancellationToken cancellationToken)
    {
        logger.LogInformation($"Scheduled action triggered ({DateTime.Now})");
        return Task.CompletedTask;
    }

    public async Task<List<ScheduleItem>> GetPlan()
    {
        await Task.CompletedTask;
        var accounts = await manager.GetBotAccountsAsync();
        return accounts.Select(s => new ScheduleItem(s.Name, "", 
            new(Random.Shared.Next(10), 0), new(10 + Random.Shared.Next(14),0))).ToList();
    }

}
