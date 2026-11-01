namespace DragonEmu.Persistence;

internal static class ItemHolds
{
    public static string NotHeld(string column) =>
        $"NOT EXISTS (SELECT 1 FROM marketplace_offers m WHERE m.item_ref = {column})"
        + $" AND NOT EXISTS (SELECT 1 FROM trax_machine_sound_sets ts WHERE ts.sound_set_item_ref = {column})"
        + $" AND NOT EXISTS (SELECT 1 FROM jukebox_disks jd WHERE jd.disk_item_ref = {column})";

    public const string ReleaseMachine =
        """
        DELETE FROM trax_machine_sound_sets WHERE machine_item_ref = @itemId;
        DELETE FROM jukebox_playlists WHERE machine_item_ref = @itemId;
        DELETE FROM jukebox_disks WHERE machine_item_ref = @itemId;
        """;

    public const string ReleaseRoom =
        """
        DELETE h FROM trax_machine_sound_sets h INNER JOIN users_items i ON i.id = h.machine_item_ref WHERE i.room_ref = @roomId;
        DELETE h FROM jukebox_playlists h INNER JOIN users_items i ON i.id = h.machine_item_ref WHERE i.room_ref = @roomId;
        DELETE h FROM jukebox_disks h INNER JOIN users_items i ON i.id = h.machine_item_ref WHERE i.room_ref = @roomId;
        """;
}
