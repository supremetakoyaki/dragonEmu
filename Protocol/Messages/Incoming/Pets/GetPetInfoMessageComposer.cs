using DragonEmu.Game.Pets;

namespace DragonEmu.Protocol.Messages.Incoming.Pets;

public sealed class GetPetInfoMessageComposer : IMessageComposer
{
    private readonly PetService _pets;

    private int _webId;

    public GetPetInfoMessageComposer(PetService pets)
    {
        _pets = pets;
    }

    public void Parse(ClientPacket packet)
    {
        _webId = packet.PopInt32();
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not null)
        {
            _pets.SendInfo(session, _webId);
        }

        return ValueTask.CompletedTask;
    }
}
