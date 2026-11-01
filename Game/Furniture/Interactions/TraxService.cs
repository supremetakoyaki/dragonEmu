using DragonEmu.Domain;
using DragonEmu.Game.Rooms;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;
using DragonEmu.Protocol.Messages.Outgoing.Sound;
using DragonEmu.Protocol.Messages.Outgoing.Trax;

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
        if (await _userItems.GetByIdAsync(machine.Id, cancellationToken) is { } stored && stored.RoomRef == room.Id)
        {
            await SwitchAsync(room, machine, stored.ExtraData != On, cancellationToken);
        }
    }

    public async Task SwitchAsync(Room room, RoomFloorItem machine, bool on, CancellationToken cancellationToken)
    {
        if (await _userItems.GetByIdAsync(machine.Id, cancellationToken) is not { } stored || stored.RoomRef != room.Id)
        {
            return;
        }

        bool playing = room.Playback?.MachineItemId == machine.Id;
        if ((stored.ExtraData == On) == on && playing == on)
        {
            return;
        }

        string state = on ? On : Off;
        await _userItems.UpdateExtraDataAsync(machine.Id, state, cancellationToken);
        _broadcaster.Broadcast(room, new ObjectDataUpdateMessageEvent(machine.Id, state).Compose());

        bool jukebox = machine.LogicClass == FurnitureLogicClass.Jukebox;
        if (on)
        {
            IReadOnlyList<TraxSong> songs = await PlaylistOfAsync(machine, cancellationToken);
            room.StartPlayback(machine.Id, songs, DateTime.UtcNow);
            _broadcaster.Broadcast(room, new RoomMusicMessageEvent(0, songs).Compose());
            if (jukebox)
            {
                _broadcaster.Broadcast(room, new PlayListMessageEvent(0, songs).Compose());
            }

            return;
        }

        room.StopPlayback();
        _broadcaster.Broadcast(room, RoomMusicMessageEvent.Silence.Compose());
        if (jukebox && await _userItems.GetByIdAsync(machine.Id, cancellationToken) is { } current)
        {
            _broadcaster.Broadcast(room, new ObjectRemoveMessageEvent(machine.Id).Compose());
            _broadcaster.Broadcast(room, new ObjectAddMessageEvent(current).Compose());
        }
    }

    public async Task<bool> IsOnAsync(RoomFloorItem machine, CancellationToken cancellationToken) =>
        await _userItems.GetByIdAsync(machine.Id, cancellationToken) is { ExtraData: On };

    public async Task PlaylistChangedAsync(Room room, RoomFloorItem machine, CancellationToken cancellationToken)
    {
        if (room.Playback?.MachineItemId != machine.Id)
        {
            return;
        }

        DateTime now = DateTime.UtcNow;
        if (room.ChangePlaylist(await PlaylistOfAsync(machine, cancellationToken), now) is { } playback)
        {
            _broadcaster.Broadcast(room, new PlayListMessageEvent(playback.PositionSeconds(now), playback.Songs).Compose());
            _broadcaster.Broadcast(room, new RoomMusicMessageEvent(playback.PositionSeconds(now), playback.Songs).Compose());
        }
    }

    private Task<IReadOnlyList<TraxSong>> PlaylistOfAsync(RoomFloorItem machine, CancellationToken cancellationToken) =>
        _trax.GetPlaylistOfAsync(machine.Id, machine.LogicClass, cancellationToken);

    public void SendPlaylist(IGameSession session, Room room)
    {
        if (room.Playback is { } playback)
        {
            int position = playback.PositionSeconds(DateTime.UtcNow);
            session.Send(new PlayListMessageEvent(position, playback.Songs).Compose());
            session.Send(new RoomMusicMessageEvent(position, playback.Songs).Compose());
        }
        else
        {
            session.Send(PlayListMessageEvent.Empty.Compose());
            session.Send(RoomMusicMessageEvent.Silence.Compose());
        }
    }

    public void SendSong(IGameSession session, Room room, int songId)
    {
        if (room.Playback?.Song(songId) is { } song)
        {
            session.Send(new TraxSongInfoMessageEvent(song).Compose());
        }
    }
}
