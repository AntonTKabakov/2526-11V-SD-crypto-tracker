namespace server.Service;

public class CryptoPriceSnapshotBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CryptoPriceSnapshotBackgroundService> _logger;
    private readonly CryptoPriceSnapshotSettings _settings;

    public CryptoPriceSnapshotBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<CryptoPriceSnapshotBackgroundService> logger,
        CryptoPriceSnapshotSettings settings)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _settings = settings;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_settings.SnapshotInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            await CaptureSnapshotAsync(stoppingToken);

            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task CaptureSnapshotAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var snapshotService = scope.ServiceProvider.GetRequiredService<ICryptoPriceSnapshotService>();

        try
        {
            await snapshotService.CaptureSnapshotAsync(
                force: false,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Crypto price snapshot capture failed.");
        }
    }
}
