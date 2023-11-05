using Microsoft.Extensions.Logging;

namespace Telegram.Automation;

public class ScheduleExecutor : IScheduleExecutor
{
    private readonly ILogger<ScheduleExecutor> logger;

    public ScheduleExecutor(ILogger<ScheduleExecutor> logger)
    {
        this.logger = logger;
    }
    public Task Execute(CancellationToken cancellationToken)
    {
        logger.LogInformation("Scheduled action triggered");
        return Task.CompletedTask;
    }
}