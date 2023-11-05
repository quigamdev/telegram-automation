using Microsoft.Extensions.Logging;
using System.Threading;
using Telegram.Automation;

internal class ScheduleHostedService : IHostedService
{
    private readonly IScheduleExecutor executor;
    private readonly ILogger<ScheduleHostedService> logger;

    public ScheduleHostedService(IScheduleExecutor executor, ILogger<ScheduleHostedService> logger)
    {
        this.executor = executor;
        this.logger = logger;
    }
    private PeriodicTimer timer;
    private CancellationToken cancellationToken;
    private CancellationTokenSource cancellationTokenSource;

    private Task processingTask;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        cancellationTokenSource = new CancellationTokenSource();
        processingTask = Task.Factory.StartNew(ExecuteSafe, TaskCreationOptions.LongRunning);

        return Task.CompletedTask;
    }

    private async Task ExecuteSafe()
    {
        try
        {
            await Execute();
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "ScheduledHostedService failed to execute schedule");
        }

    }

    private async Task Execute()
    {
        while (await timer.WaitForNextTickAsync(cancellationToken) && !cancellationToken.IsCancellationRequested)
        {
            await executor.Execute(cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        cancellationTokenSource.Cancel();
        cancellationTokenSource.Dispose();
        processingTask.Dispose();
        timer.Dispose();
        return Task.CompletedTask;
    }
}