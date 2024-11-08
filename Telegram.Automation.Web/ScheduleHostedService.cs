using Telegram.Automation;

internal class ScheduleHostedService : IHostedService
{
    private readonly IScheduleExecutor executor;
    private readonly ILogger<ScheduleHostedService> logger;
    private readonly IDateTimeProvider dateTimeProvider;

    public ScheduleHostedService(IScheduleExecutor executor, ILogger<ScheduleHostedService> logger, IDateTimeProvider dateTimeProvider)
    {
        this.executor = executor;
        this.logger = logger;
        this.dateTimeProvider = dateTimeProvider;
    }
    private PeriodicTimer? timer;
    private CancellationToken cancellationToken;
    private CancellationTokenSource cancellationTokenSource = new();

    private Task? processingTask;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        cancellationTokenSource = new CancellationTokenSource();
        processingTask = Task.Factory.StartNew(ExecutePeriodicaly, TaskCreationOptions.LongRunning);

        _ = Task.Run(Execute);

        return Task.CompletedTask;
    }

    private async Task ExecutePeriodicaly()
    {
        while (await timer!.WaitForNextTickAsync(cancellationToken) && !cancellationToken.IsCancellationRequested)
        {
            await Execute();
        }
    }

    private async Task Execute()
    {
        try
        {
            await executor.Execute(cancellationToken);
        }
        catch (TaskCanceledException)
        {
            // ignore
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, $"ScheduledHostedService failed to execute schedule ({dateTimeProvider.Now})");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        cancellationTokenSource.Dispose();
        processingTask?.Dispose();
        timer?.Dispose();

        processingTask = null;
        timer = null;

        return Task.CompletedTask;
    }

}
