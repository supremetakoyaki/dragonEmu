using DragonEmu.Domain;
using DragonEmu.Game.Rooms;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;
using DragonEmu.Protocol.Messages.Outgoing.Sound;

namespace DragonEmu.Game.Furniture;

public sealed class TraxService
{
    public const string Off = "0";
    public const string On = "1";

    private readonly IUserItemRepository _userItems;
    private readonly ITraxRepository _trax;
    private readonly RoomBroadcaster _broadcaster;

    public TraxService(IUserItemRepository userItems, ITraxRepository trax, RoomBroadcaster broadcaster)
    {
        _userItems = userItems;
        _trax = trax;
        _broadcaster = broadcaster;
    }

    public async Task ToggleAsync(Room room, RoomFloorItem machine, CancellationToken cancellationToken)
    {
        if (await _userItems.GetByIdAsync(machine.Id, cancellationToken) is not { } stored || stored.RoomRef != room.Id)
        {
            return;
        }

        bool switchingOn = stored.ExtraData != On;
        string state = switchingOn ? On : Off;
        await _userItems.UpdateExtraDataAsync(machine.Id, state, cancellationToken);
        _broadcaster.Broadcast(room, new ObjectDataUpdateMessageEvent(machine.Id, state).Compose());

        bool jukebox = machine.LogicClass == FurnitureLogicClass.Jukebox;
        if (switchingOn)
        {
            IReadOnlyList<TraxSong> songs = await _trax.GetPlaylistAsync(machine.Id, cancellationToken);
            room.StartPlayback(machine.Id, songs, DateTime.UtcNow);
            if (jukebox)
            {
                _broadcaster.Broadcast(room, new PlayListMessageEvent(0, songs).Compose());
            }

            return;
        }

        room.StopPlayback();
        if (jukebox && await _userItems.GetByIdAsync(machine.Id, cancellationToken) is { } current)
        {
            _broadcaster.Broadcast(room, new ObjectRemoveMessageEvent(machine.Id).Compose());
            _broadcaster.Broadcast(room, new ObjectAddMessageEvent(current).Compose());
        }
    }

    public void SendPlaylist(IGameSession session, Room room) =>
        session.Send((room.Playback is { } playback
            ? new PlayListMessageEvent(playback.PositionSeconds(DateTime.UtcNow), playback.Songs)
            : PlayListMessageEvent.Empty).Compose());

    public void SendSong(IGameSession session, Room room, int songId)
    {
        if (room.Playback?.Song(songId) is { } song)
        {
            session.Send(new TraxSongInfoMessageEvent(song).Compose());
        }
    }
}
