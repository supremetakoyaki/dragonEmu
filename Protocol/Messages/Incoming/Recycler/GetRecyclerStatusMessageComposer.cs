using DragonEmu.Game.Settings;
using DragonEmu.Domain;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.Recycler;

namespace DragonEmu.Protocol.Messages.Incoming.Recycler;

public sealed class GetRecyclerStatusMessageComposer : IMessageComposer
{
    private readonly GameSettingsCache _gameSettings;
    private readonly IRecyclerRepository _recyclerRepository;

    private readonly HotelStatus _hotel;

    public GetRecyclerStatusMessageComposer(GameSettingsCache gameSettings, IRecyclerRepository recyclerRepository, HotelStatus hotel)
    {
        _hotel = hotel;
        _gameSettings = gameSettings;
        _recyclerRepository = recyclerRepository;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account)
        {
            return;
        }

        RecyclerSettings settings = _gameSettings.Current.Recycler;
        if (!settings.Open || _hotel.IsShuttingDown)
        {
            session.Send(RecyclerStatusMessageEvent.Closed().Compose());
            return;
        }

        DateTime? lastRecycledAt = await _recyclerRepository.GetLastRecycledAtAsync(account.Id, cancellationToken);
        int secondsLeft = settings.SecondsLeft(lastRecycledAt, DateTime.UtcNow);
        session.Send((secondsLeft > 0 ? RecyclerStatusMessageEvent.Cooldown(secondsLeft) : RecyclerStatusMessageEvent.Ready()).Compose());
    }
}
