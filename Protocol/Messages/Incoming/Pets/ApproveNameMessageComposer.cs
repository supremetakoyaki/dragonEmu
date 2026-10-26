using DragonEmu.Game.Pets;
using DragonEmu.Protocol.Messages.Outgoing.Pets;

namespace DragonEmu.Protocol.Messages.Incoming.Pets;

public sealed class ApproveNameMessageComposer : IMessageComposer
{
    private readonly PetService _pets;

    private string _name = string.Empty;

    public ApproveNameMessageComposer(PetService pets)
    {
        _pets = pets;
    }

    public void Parse(ClientPacket packet)
    {
        _name = packet.PopString();
        packet.PopInt32();
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not null)
        {
            session.Send(new ApproveNameMessageEvent(_pets.CheckName(_name)).Compose());
        }

        return ValueTask.CompletedTask;
    }
}
