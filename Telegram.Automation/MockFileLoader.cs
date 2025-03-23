
namespace Telegram.Automation;

public  class MockFileLoader(string currentDir)
{

    public string GetStatusResponseFull() => File.ReadAllText(Path.Combine(currentDir, "Mocks/StatusCommandResponse.txt"));
    public string GetStatusResponsePart1() => File.ReadAllText(Path.Combine(currentDir, "Mocks/StatusCommandResponsePart1.txt"));
    public string GetStatusResponsePart2() => File.ReadAllText(Path.Combine(currentDir, "Mocks/StatusCommandResponsePart2.txt"));

}