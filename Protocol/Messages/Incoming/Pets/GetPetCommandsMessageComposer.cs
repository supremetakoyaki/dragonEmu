using DragonEmu.Game.Pets;

namespace DragonEmu.Protocol.Messages.Incoming.Pets;

public sealed class GetPetCommandsMessageComposer : IMessageComposer
{
    private readonly PetService _pets;

    private int _webId;

    public GetPetCommandsMessageComposer(PetService pets)
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
            _pets.SendCommands(session, _webId);
        }

        return ValueTask.CompletedTask;
    }
}
