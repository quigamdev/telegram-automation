using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Runtime.CompilerServices;

namespace Telegram.Automation.Web.Pages;

public class AuthenticationModel : PageModel
{
    private readonly TelegramConnector telegramConnector;

    public AuthenticationModel(TelegramConnector telegramConnector)
    {
        this.telegramConnector = telegramConnector;
    }
    [BindRequired]
    public string SecurityCode { get; set; }
    public string InvalidCode { get; set; }

    public void OnGet()
    {
    }

    public async Task OnPost()
    {
        SecurityCode = Request.Form["SecurityCode"];

        await telegramConnector.Start();
        try
        {
            await telegramConnector.CheckAuthCode(SecurityCode);
            InvalidCode = $"Code valid!";

        }
        catch
        {
            InvalidCode = $"The code is invalid! ({SecurityCode})";
        }
    }
}
