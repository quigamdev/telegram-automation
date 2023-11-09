using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Telegram.Automation;

namespace Telegram.Automation.Web.Pages;
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly AccountsManager accountsManager;

    public List<BotAccount> Accounts { get; set; } = new List<BotAccount>();
    public string Message { get; set; }

    public IndexModel(ILogger<IndexModel> logger, AccountsManager accountsManager)
    {
        _logger = logger;
        this.accountsManager = accountsManager;
    }

    public async Task OnGet()
    {
        Accounts = await accountsManager.GetBotAccountsAsync();
    }

    public async Task<IActionResult> OnPostStartAccountAsync(string account)
    {

        var result = await accountsManager.StartAccount(account);
        Message = $"Account activation in process! R: {result}";
        return Page();
    }

    public async Task<IActionResult> OnPostStopAccountAsync(string account)
    {
        var result = await accountsManager.StopAccount(account);
        Message = $"Account deactivation in process! R: {result}";
        return Redirect("/?result=Account deactivation in process!");
    }
}
