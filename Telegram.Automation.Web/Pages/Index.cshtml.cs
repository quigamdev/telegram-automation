using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Telegram.Automation.Web.Pages;
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly AccountsManager accountsManager;

    public List<BotAccount> Accounts { get; set; } = new List<BotAccount>();

    public IndexModel(ILogger<IndexModel> logger, AccountsManager accountsManager)
    {
        _logger = logger;
        this.accountsManager = accountsManager;
    }

    public async Task OnGet()
    {
        Accounts = await accountsManager.GetBotAccountsAsync();
    }
}
