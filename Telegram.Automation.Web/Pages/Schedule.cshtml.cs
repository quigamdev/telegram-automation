using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Telegram.Automation.Web.Pages;

public class ScheduleModel : PageModel
{
    private readonly IScheduleExecutor scheduleExecutor;

    [FromQuery]
    public int Id { get; set; }
    public string InitData { get; set; } = "";
    public List<ScheduleSimple> Schedules { get; private set; } = new();

    public ScheduleModel(IScheduleExecutor scheduleExecutor)
    {
        this.scheduleExecutor = scheduleExecutor;
    }

    public void OnGet()
    {
        if (Id == 0)
        {
            Id = 1; // set default
        }

        InitData = $"{{id:{Id}}}";
    }

}
