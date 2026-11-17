namespace FarmGate_Backend.Media;

public sealed class PendingImageCleanup(FileImageStorage storage, ILogger<PendingImageCleanup> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromHours(1));
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await storage.CleanupPendingAsync(stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex) { logger.LogError(ex, "Pending image cleanup failed; retaining files"); }
            if (!await timer.WaitForNextTickAsync(stoppingToken)) break;
        }
    }
}
