using DragonEmu.Game.Pets;

namespace DragonEmu.Protocol.Messages.Incoming.Pets;

public sealed class RemovePetFromFlatMessageComposer : IMessageComposer
{
    private readonly PetService _pets;

    private int _webId;

    public RemovePetFromFlatMessageComposer(PetService pets)
    {
        _pets = pets;
    }

    public void Parse(ClientPacket packet)
    {
        _webId = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account)
        {
            await _pets.PickUpAsync(session, account, _webId, cancellationToken);
        }
    }
}
