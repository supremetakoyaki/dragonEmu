using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.Users;

namespace DragonEmu.Protocol.Messages.Incoming.Users;

public sealed class ScrGetUserInfoMessageComposer : IMessageComposer
{
    private readonly IUserRepository _userRepository;
    private string _subscriptionId = string.Empty;

    public ScrGetUserInfoMessageComposer(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public void Parse(ClientPacket packet)
    {
        _subscriptionId = packet.PopString();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account)
        {
            return;
        }

        string normalized = _subscriptionId.ToLowerInvariant();
        if (normalized is not ("habbo_club" or "club_habbo"))
        {
            session.Send(ScrSendUserInfoEvent.NotSubscribed(_subscriptionId).Compose());
            return;
        }

        (DateTime? clubExpiresAt, _, _) = await _userRepository.GetClubStatusAsync(account.Id, cancellationToken);

        if (clubExpiresAt is not { } expiresAt || expiresAt <= DateTime.UtcNow)
        {
            session.Send(ScrSendUserInfoEvent.NotSubscribed("habbo_club").Compose());
            return;
        }

        session.Send(ScrSendUserInfoEvent.HabboClubStatus(expiresAt).Compose());
    }
}
