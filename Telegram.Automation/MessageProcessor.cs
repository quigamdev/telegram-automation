namespace Telegram.Automation;
public class MessageProcessor
{
    public static bool IsStatusMessage(string input) => input.Contains("Active:") &&
            input.Contains("/ Online:") &&
            input.Contains("-- Online --") &&
            input.Contains("-- Offline --") 
        || !input.Contains("\n") && input.Contains("]") 
        || input.Contains("\n") && input.Contains("[") && input.Contains("]");

    public static List<BotAccount> ProcessStatusMessage(string input)
    {
        if (input is null || !input.Contains("Active:")) return new List<BotAccount>();

        var lines = input.Split(new[] { '\r', '\n' }).Select(s => s.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)).Skip(1).ToList();
        var isOnline = false;
        var result = new List<BotAccount>();

        foreach (var line in lines)
        {
            isOnline = DetermineStatus(isOnline, line);
            var words = line.Split("[");

            if (words.Length != 2) continue;

            var account = new BotAccount();
            account.Name = words[0].Trim();
            account.AccountNumber = words[1].Trim().TrimEnd(']');
            account.Status = isOnline ? BotAccountStatus.Online : BotAccountStatus.Offline;

            result.Add(account);
        }
        return result;
    }

    internal static bool IsStartingAccountMessage(string input) => input.Contains("Starting");


    internal static bool IsStopingAccountMessage(string input) => input.Contains("Stopping");

    private static bool DetermineStatus(bool isOnline, string? line)
    {
        if (line is null) return isOnline;

        if (line.Contains("online", StringComparison.OrdinalIgnoreCase))
        {
            isOnline = true;
        }
        if (line.Contains("offline", StringComparison.OrdinalIgnoreCase))
        {
            isOnline = false;
        }

        return isOnline;
    }
}
