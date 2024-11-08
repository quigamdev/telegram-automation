using NSubstitute;
using NSubstitute.Core.DependencyInjection;

namespace Telegram.Automation.UnitTests;

public class ScheduleExecutorTests
{
    [Fact]
    public async Task ExecuteAt23h59m()
    {
        var store = new ScheduleStore();

        var dateTimeProvider = Substitute.For<IDateTimeProvider>();
        dateTimeProvider.Now.Returns(new DateTime(2024, 1, 1, 0, 0, 1));

        var testTelegramConnector = Substitute.For<ITelegramConnector>();
        testTelegramConnector.Start().Returns(AuthenticationResult.Authenticated);

        var executor = new ScheduleExecutor(new AccountsManager(testTelegramConnector, store), store, dateTimeProvider);

        await executor.Execute(CancellationToken.None);

        await testTelegramConnector.Received().Start();
    }
}