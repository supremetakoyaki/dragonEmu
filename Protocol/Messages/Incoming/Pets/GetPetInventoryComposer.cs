using DragonEmu.Game.Pets;

namespace DragonEmu.Protocol.Messages.Incoming.Pets;

public sealed class GetPetInventoryComposer : IMessageComposer
{
    private readonly PetService _pets;

    public GetPetInventoryComposer(PetService pets)
    {
        _pets = pets;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account)
        {
            await _pets.SendInventoryAsync(session, account, cancellationToken);
        }
    }
}
