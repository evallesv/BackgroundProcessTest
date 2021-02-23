using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BackgroundProcessTest.BackgroundProcess
{

public class QueueProcessService : BackgroundService
{
    private readonly ILogger<QueueProcessService> _logger;
    public QueueProcessService(IBackgroundTaskQueue taskQueue, 
        ILogger<QueueProcessService> logger)
    {
        _logger = logger;
        TaskQueue = taskQueue;
    }
    public IBackgroundTaskQueue TaskQueue { get; }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Queue Service: is running.");
        await BackgroundProcessing(stoppingToken);
    }

    private async Task BackgroundProcessing(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var workItem = 
                await TaskQueue.DequeueAsync(stoppingToken);

            try
            {
                _logger.LogInformation("Queue Service: Task {WorkItem}.", nameof(workItem));
                await workItem(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Queue Service: Error occurred executing {WorkItem}.", nameof(workItem));
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Queue Service: is stopping.");
        await base.StopAsync(cancellationToken);
    }
}

}