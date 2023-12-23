using System.Security.Principal;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Telegram.Automation;

public class ScheduleStore
{
    private const string scheduleFile = "schedule.json";
    private const string scheduleAccountsFile = "scheduleAccounts.json";
    public List<Schedule> GetActiveSchedules()
    {
        return GetAll().ToList();
    }

    public Schedule GetSchedule(int id = 1)
    {
        var schedules = GetAll();
        return schedules.FirstOrDefault(s => s.Id == id) ?? new();

    }
    private List<Schedule> GetAll()
    {
        try
        {
            var file = File.ReadAllText(scheduleFile);
            return JsonSerializer.Deserialize<List<Schedule>>(file) ?? new();
        }
        catch (Exception)
        {
            return new();
        }
    }

    public void Save(Schedule schedule)
    {
        var schedules = GetAll();

        //if (schedule.Id == 0)
        //{
        //    schedule.Id = schedules.Count + 1;
        //}

        //var toBeSaved = schedules.Where(s => s.Id != schedule.Id).ToList();
        //toBeSaved.Add(schedule);


        var data = JsonSerializer.Serialize(new List<Schedule>() { schedule });
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