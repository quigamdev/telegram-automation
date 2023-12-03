using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Telegram.Automation.Web.Pages;

public class ScheduleModel : PageModel
{
    public string InitData { get; set; } = "{id:1}"; // TODO: create based on query param
    public void OnGet()
    {
    }
}
