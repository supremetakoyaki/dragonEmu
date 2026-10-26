using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.Users;

namespace DragonEmu.Protocol.Messages.Incoming.Users;

public sealed class GetHabboGroupDetailsMessageComposer : IMessageComposer
{
    private readonly IGroupRepository _groups;

    private int _groupId;

    public GetHabboGroupDetailsMessageComposer(IGroupRepository groups)
    {
        _groups = groups;
    }

    public void Parse(ClientPacket packet)
    {
        _groupId = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is null)
        {
            return;
        }

        session.Send(new HabboGroupDetailsMessageEvent(await _groups.GetDetailsAsync(_groupId, cancellationToken)).Compose());
    }
}
