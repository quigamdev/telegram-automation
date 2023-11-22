using System.Text.Json;
using System.Text.Json.Nodes;

namespace Telegram.Automation;

public class ScheduleStore
{
    private const string fileName = "schedule.json";
    public List<ScheduleItem> GetSchedule()
    {
        try
        {
            var file = File.ReadAllText(fileName);
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
        File.WriteAllText(fileName, data);
    }
}