using System.Text.Json;
using System.Text.Json.Nodes;

namespace Telegram.Automation;

public class ScheduleStore
{
    private const string scheduleFile = "schedule.json";
    private const string scheduleAccountsFile = "scheduleAccounts.json";
    public List<ScheduleItem> GetSchedule()
    {
        try
        {
            var file = File.ReadAllText(scheduleFile);
            return JsonSerializer.Deserialize<List<ScheduleItem>>(file) ?? new();
        }
        catch (Exception)
        {
            return new();
        }
    }
    public void Save(List<ScheduleItem> schedule)
    {
        var data = JsonSerializer.Serialize(schedule);
        File.WriteAllText(scheduleFile, data);
    }

    public List<string> GetAccountsForScheduling()
    {
        try
        {
            var file = File.ReadAllText(scheduleAccountsFile);
            return JsonSerializer.Deserialize<List<string>>(file) ?? new();
        }
        catch (Exception)
        {
            return new();
        }
    }
    public void SaveAccountsForScheduling(List<string> schedule)
    {
        var data = JsonSerializer.Serialize(schedule);
        File.WriteAllText(scheduleAccountsFile, data);
    }
}