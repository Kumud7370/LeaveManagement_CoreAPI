using AttendanceManagementSystem.Services.Interfaces;

namespace AttendanceManagementSystem.BackgroundServices
{

    public class WorkloadNotificationBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<WorkloadNotificationBackgroundService> _logger;

        public WorkloadNotificationBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<WorkloadNotificationBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("WorkloadNotificationBackgroundService started.");


            DateTime? lastOverdueCheck = null;
            DateTime? lastWeeklySummary = null;
            DateTime? lastWorkloadCheck = null;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.UtcNow;


                    if (now.Hour == 8 && now.Minute < 5)
                    {
                        var today = now.Date;
                        if (lastOverdueCheck == null || lastOverdueCheck.Value.Date < today)
                        {
                            lastOverdueCheck = now;
                            _logger.LogInformation("Running daily overdue alert job.");
                            await RunScopedAsync(svc => svc.SendOverdueAlertsAsync(), stoppingToken);
                        }
                    }


                    if (now.DayOfWeek == DayOfWeek.Monday && now.Hour == 8 && now.Minute < 5)
                    {
                        var thisWeek = now.Date;
                        if (lastWeeklySummary == null || lastWeeklySummary.Value.Date < thisWeek)
                        {
                            lastWeeklySummary = now;
                            _logger.LogInformation("Running weekly workload summary job.");
                            await RunScopedAsync(svc => svc.SendWeeklySummaryAsync(), stoppingToken);
                        }
                    }


                    if (lastWorkloadCheck == null || (now - lastWorkloadCheck.Value).TotalHours >= 2)
                    {
                        lastWorkloadCheck = now;
                        _logger.LogInformation("Running workload utilization check.");
                        await RunScopedAsync(svc => svc.CheckAndNotifyWorkloadAsync(), stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled error in WorkloadNotificationBackgroundService loop.");
                }


                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }

            _logger.LogInformation("WorkloadNotificationBackgroundService stopped.");
        }

        private async Task RunScopedAsync(Func<INotificationService, Task> action, CancellationToken ct)
        {
            if (ct.IsCancellationRequested) return;
            using var scope = _scopeFactory.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<INotificationService>();
            await action(svc);
        }
    }
}