using server.Models;

namespace server.Service;

public interface ICryptoService
{
    Task<IReadOnlyList<CryptoAssetDto>> GetAssetsAsync(
        CancellationToken cancellationToken = default);
}
