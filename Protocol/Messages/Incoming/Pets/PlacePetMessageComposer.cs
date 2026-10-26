using DragonEmu.Game.Pets;

namespace DragonEmu.Protocol.Messages.Incoming.Pets;

public sealed class PlacePetMessageComposer : IMessageComposer
{
    private readonly PetService _pets;

    private int _petId;
    private int _x;
    private int _y;

    public PlacePetMessageComposer(PetService pets)
    {
        _pets = pets;
    }

    public void Parse(ClientPacket packet)
    {
        _petId = packet.PopInt32();
        _x = packet.PopInt32();
        _y = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account)
        {
            await _pets.PlaceAsync(session, account, _petId, _x, _y, cancellationToken);
        }
    }
}
