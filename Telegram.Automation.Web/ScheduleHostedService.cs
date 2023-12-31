﻿using Telegram.Automation;

internal class ScheduleHostedService : IHostedService
{
    private readonly IScheduleExecutor executor;
    private readonly ILogger<ScheduleHostedService> logger;

    public ScheduleHostedService(IScheduleExecutor executor, ILogger<ScheduleHostedService> logger)
    {
        this.executor = executor;
        this.logger = logger;
    }
    private PeriodicTimer? timer;
    private CancellationToken cancellationToken;
    private CancellationTokenSource cancellationTokenSource = new();

    private Task? processingTask;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        cancellationTokenSource = new CancellationTokenSource();
        processingTask = Task.Factory.StartNew(ExecuteSafe, TaskCreationOptions.LongRunning);
     
        await executor.Execute(cancellationToken);
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
            logger.LogCritical(ex, $"ScheduledHostedService failed to execute schedule ({DateTime.Now})");
        }

    }

    private async Task Execute()
    {
        while (await timer!.WaitForNextTickAsync(cancellationToken) && !cancellationToken.IsCancellationRequested)
        {
            await executor.Execute(cancellationToken);
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
