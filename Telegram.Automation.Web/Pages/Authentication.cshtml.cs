using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Runtime.CompilerServices;

namespace Telegram.Automation.Web.Pages;

public class AuthenticationModel : PageModel
{
    private readonly ITelegramConnector telegramConnector;

    public AuthenticationModel(ITelegramConnector telegramConnector)
    {
        this.telegramConnector = telegramConnector;
    }
    [BindProperty]
    public string SecurityCode { get; set; }
    [BindProperty]
    public AuthenticationResult Status { get; set; }
    public string Message { get; set; }

    public async Task OnGet()
    {
        Status = await telegramConnector.IsAuthenticated();
        Message = "Current status: " + Status;
    }

    public async Task OnPost()
    {
        SecurityCode = Request.Form["SecurityCode"];
        await telegramConnector.Start();
        try
        {
            await telegramConnector.CheckAuthCode(SecurityCode);
            Message = $"Code valid and system authenticated!";

        }
        catch
        {
            Message = $"The code is invalid! ({SecurityCode})";
        }
    }
}
