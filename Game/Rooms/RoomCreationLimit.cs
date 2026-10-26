using DragonEmu.Domain;
using DragonEmu.Game.Settings;
using DragonEmu.Persistence;

namespace DragonEmu.Game.Rooms;

public sealed class RoomCreationLimit
{
    public const int BypassRank = 4;

    private readonly IPrivateRoomRepository _privateRoomRepository;
    private readonly GameSettingsCache _gameSettings;

    public RoomCreationLimit(IPrivateRoomRepository privateRoomRepository, GameSettingsCache gameSettings)
    {
        _privateRoomRepository = privateRoomRepository;
        _gameSettings = gameSettings;
    }

    public int Limit => _gameSettings.Current.PrivateRoomsLimit;

    public async Task<bool> MayCreateAsync(UserAccount account, CancellationToken cancellationToken) =>
        account.Rank > BypassRank || await _privateRoomRepository.CountOwnedByAsync(account.Id, cancellationToken) < Limit;
}
