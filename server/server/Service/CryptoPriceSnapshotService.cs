using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using server.Date;
using server.Models;

namespace server.Service;

public class CryptoPriceSnapshotService : ICryptoPriceSnapshotService
{
    private static readonly JsonSerializerOptions SnapshotJsonOptions = new(JsonSerializerDefaults.Web);

    private readonly AppDbContext _db;
    private readonly ICryptoService _cryptoService;
    private readonly CryptoPriceSnapshotSettings _settings;
    private readonly ILogger<CryptoPriceSnapshotService> _logger;

    public CryptoPriceSnapshotService(
        AppDbContext db,
        ICryptoService cryptoService,
        CryptoPriceSnapshotSettings settings,
        ILogger<CryptoPriceSnapshotService> logger)
    {
        _db = db;
        _cryptoService = cryptoService;
        _settings = settings;
        _logger = logger;
    }

    public async Task CaptureSnapshotAsync(
        bool force,
        CancellationToken cancellationToken = default)
    {
        var latestSnapshot = await GetLatestSnapshotEntityAsync(cancellationToken);
        await CaptureSnapshotEntityAsync(latestSnapshot, force, cancellationToken);
    }

    public async Task<IReadOnlyList<CryptoAssetDto>> GetLatestAssetsAsync(
        CancellationToken cancellationToken = default)
    {
        var latestSnapshot = await GetLatestSnapshotEntityAsync(cancellationToken);

        try
        {
            var snapshot = await CaptureSnapshotEntityAsync(
                latestSnapshot,
                force: false,
                cancellationToken);

            return DeserializeAssets(snapshot.AssetsJson);
        }
        catch (ExternalServiceException ex) when (latestSnapshot is not null)
        {
            _logger.LogWarning(
                ex,
                "Falling back to the latest stored crypto price snapshot captured at {Timestamp}",
                latestSnapshot.Timestamp);

            return DeserializeAssets(latestSnapshot.AssetsJson);
        }
    }

    public async Task<IReadOnlyList<CryptoAssetPricePointDto>> GetAssetHistoryAsync(
        string assetId,
        CancellationToken cancellationToken = default)
    {
        var normalizedAssetId = NormalizeAssetId(assetId);

        var snapshots = await _db.CryptoPriceSnapshots
            .AsNoTracking()
            .OrderBy(snapshot => snapshot.Timestamp)
            .ToListAsync(cancellationToken);

        if (snapshots.Count == 0)
        {
            return Array.Empty<CryptoAssetPricePointDto>();
        }

        var history = new List<CryptoAssetPricePointDto>(snapshots.Count);

        foreach (var snapshot in snapshots)
        {
            var asset = DeserializeAssets(snapshot.AssetsJson)
                .FirstOrDefault(item => string.Equals(
                    item.AssetId,
                    normalizedAssetId,
                    StringComparison.OrdinalIgnoreCase));

            if (asset is null)
            {
                continue;
            }

            history.Add(new CryptoAssetPricePointDto(
                snapshot.Timestamp,
                asset.CurrentPrice));
        }

        return history;
    }

    private async Task<CryptoPriceSnapshot> CaptureSnapshotEntityAsync(
        CryptoPriceSnapshot? latestSnapshot,
        bool force,
        CancellationToken cancellationToken)
    {
        if (!force &&
            latestSnapshot is not null &&
            DateTime.UtcNow - latestSnapshot.Timestamp < _settings.SnapshotInterval)
        {
            return latestSnapshot;
        }

        var assets = await _cryptoService.GetAssetsAsync(cancellationToken);
        var snapshot = new CryptoPriceSnapshot
        {
            Timestamp = DateTime.UtcNow,
            AssetsJson = JsonSerializer.Serialize(assets, SnapshotJsonOptions)
        };

        _db.CryptoPriceSnapshots.Add(snapshot);
        await _db.SaveChangesAsync(cancellationToken);

        return snapshot;
    }

    private async Task<CryptoPriceSnapshot?> GetLatestSnapshotEntityAsync(
        CancellationToken cancellationToken)
    {
        return await _db.CryptoPriceSnapshots
            .AsNoTracking()
            .OrderByDescending(snapshot => snapshot.Timestamp)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static IReadOnlyList<CryptoAssetDto> DeserializeAssets(string assetsJson)
    {
        if (string.IsNullOrWhiteSpace(assetsJson))
        {
            return Array.Empty<CryptoAssetDto>();
        }

        return JsonSerializer.Deserialize<List<CryptoAssetDto>>(assetsJson, SnapshotJsonOptions)
            ?? [];
    }

    private static string NormalizeAssetId(string assetId)
    {
        var normalizedAssetId = assetId.Trim();

        if (string.IsNullOrWhiteSpace(normalizedAssetId))
        {
            throw new ArgumentException("Asset id is required.", nameof(assetId));
        }

        return normalizedAssetId;
    }
}
