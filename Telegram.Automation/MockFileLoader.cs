
namespace Telegram.Automation;

public class MockFileLoader(string currentDir)
{
    public string GetStatusResponseFull() => File.ReadAllText(Path.Combine(currentDir, "Mocks/StatusCommandResponse.txt"));
}