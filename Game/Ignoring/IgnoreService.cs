using DragonEmu.Domain;
using DragonEmu.Game.Fuses;
using DragonEmu.Game.Settings;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.Users;

namespace DragonEmu.Game.Ignoring;

public sealed class IgnoreService
{
    private const string ModeratorFuse = "fuse_moderator_access";

    private const int NothingChanged = 0;
    private const int Ignored = 1;
    private const int IgnoredOldestDropped = 2;
    private const int Unignored = 3;

    private readonly IIgnoreRepository _ignoreRepository;
    private readonly IUserRepository _userRepository;
    private readonly FuseService _fuses;
    private readonly GameSettingsCache _gameSettings;

    public IgnoreService(IIgnoreRepository ignoreRepository, IUserRepository userRepository, FuseService fuses, GameSettingsCache gameSettings)
    {
        _ignoreRepository = ignoreRepository;
        _userRepository = userRepository;
        _fuses = fuses;
        _gameSettings = gameSettings;
    }

    public async Task LoadAsync(IGameSession session, int userId, CancellationToken cancellationToken)
    {
        IReadOnlyList<(int UserId, string Username)> ignored = await _ignoreRepository.GetIgnoredAsync(userId, cancellationToken);
        session.IgnoredUsers = ignored.Select(i => i.UserId).ToHashSet();
    }

    public async Task SendListAsync(IGameSession session, UserAccount account, CancellationToken cancellationToken)
    {
        IReadOnlyList<(int UserId, string Username)> ignored = await _ignoreRepository.GetIgnoredAsync(account.Id, cancellationToken);
        session.Send(new IgnoredUsersMessageEvent([.. ignored.Select(i => i.Username)]).Compose());
    }

    public async Task IgnoreAsync(IGameSession session, UserAccount account, string username, CancellationToken cancellationToken)
    {
        int result = NothingChanged;
        if (await _userRepository.GetByUsernameAsync(username, cancellationToken) is { } target && target.Id != account.Id
            && !await _fuses.HoldsAsync(target, ModeratorFuse, cancellationToken: cancellationToken))
        {
            (bool added, IReadOnlyList<int> dropped) = await _ignoreRepository.AddAsync(
                account.Id, target.Id, Math.Max(1, _gameSettings.Current.MaxIgnoredUsers), cancellationToken);
            if (added)
            {
                var ignored = new HashSet<int>(session.IgnoredUsers) { target.Id };
                ignored.ExceptWith(dropped);
                session.IgnoredUsers = ignored;
                result = dropped.Count > 0 ? IgnoredOldestDropped : Ignored;
            }
        }

        session.Send(new IgnoreResultMessageEvent(result).Compose());
    }

    public async Task UnignoreAsync(IGameSession session, UserAccount account, string username, CancellationToken cancellationToken)
    {
        int result = NothingChanged;
        if (await _userRepository.GetByUsernameAsync(username, cancellationToken) is { } target
            && await _ignoreRepository.RemoveAsync(account.Id, target.Id, cancellationToken))
        {
            var ignored = new HashSet<int>(session.IgnoredUsers);
            ignored.Remove(target.Id);
            session.IgnoredUsers = ignored;
            result = Unignored;
        }

        session.Send(new IgnoreResultMessageEvent(result).Compose());
    }
}
