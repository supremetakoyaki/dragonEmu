using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface IVoucherRepository
{
    Task<VoucherCode?> GetActiveByHashAsync(string hashCode, CancellationToken cancellationToken = default);

    Task<bool> TryClaimAsync(int id, int userId, string userIp, CancellationToken cancellationToken = default);
}
