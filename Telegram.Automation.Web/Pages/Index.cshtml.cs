using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Sockets;
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

    public string RenderHideClass(bool shouldRender)
    {
        if (shouldRender)
        {
            return "hide";
        }
        return "";
    }
}

