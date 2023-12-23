namespace Telegram.Automation.Web.Facades;

public class MaintenanceFacade
{
    private readonly ScheduleStore store;
    private readonly AccountsManager accountsManager;

    public MaintenanceFacade(ScheduleStore store,
                             AccountsManager accountsManager)
    {
        this.store = store;
        this.accountsManager = accountsManager;
    }

    public async Task<IEnumerable<string>> RewriteAccountsIds()
    {
        var bots = await accountsManager.GetBotAccountsAsync();
        var scheduledBots = store.GetAccountsForScheduling();

        foreach (var bot in bots.Where(s => !s.IsScheduled))
        {
            bot.IsScheduled = scheduledBots.Contains(bot.Name);
        }

        var ids = bots.Where(s => s.IsScheduled).Select(s => s.AccountNumber).ToList();
        store.SaveAccountsForScheduling(ids);
        return ids;
    }
}
