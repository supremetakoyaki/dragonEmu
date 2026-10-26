using System.Text.Json;
using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class GameSettingsRepository : IGameSettingsRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GameSettingsRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<GameSettings> GetAsync(CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            SELECT private_rooms_limit, max_favourite_rooms,
                   max_furniture_in_room, max_stickies_in_room, max_queuetiles_in_room, max_pets_in_room,
                   queuetile_furni_fusion_enabled, profanity_filter_enabled, profanity_filter_replacement_word,
                   room_controllers_list_limit, infobus_status, infobus_room_ref, timezone, happy_hour_windows,
                   recycler_status, recycler_cooldown, recycler_prize_box_ref,
                   trading_enabled, marketplace_enabled, daily_respect_grant, daily_pet_respect_grant,
                   friends_limit, friends_limit_club, max_items_in_a_trade, max_ignored_users, room_events_enabled,
                   room_ban_default_time, room_idle_time_sleep, room_idle_time_kick,
                   ban_default_hours, superban_default_days, ban_default_reason, mute_default_hours, room_mute_default_hours,
                   pets_enabled, pet_name_max_length, pet_name_allowed_chars, pet_respect_min_account_days,
                   groups_limit, groups_limit_club, username_change_allowed, username_change_cooldown,
                   username_min_length, username_max_length, username_allowed_chars, max_guide_bots_in_hotel_at_once,
                   username_change_limit
            FROM global_game_settings
            LIMIT 1;
            """, connection);

        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return GameSettings.Default;
        }

        using JsonDocument windows = JsonDocument.Parse(reader.GetString(13));
        (TimeOnly weekdayStart, TimeOnly weekdayEnd) = ParseWindow(windows.RootElement.GetProperty("weekday"));
        (TimeOnly weekendStart, TimeOnly weekendEnd) = ParseWindow(windows.RootElement.GetProperty("weekend"));

        return new GameSettings(
            PrivateRoomsLimit: reader.GetInt32(0),
            MaxFavouriteRooms: reader.GetInt32(1),
            new RoomFurnitureLimits(reader.GetInt32(2), reader.GetInt32(3), reader.GetInt32(4), reader.GetInt32(5)),
            QueuetileFurniFusionEnabled: reader.GetString(6) == "1",
            ProfanityFilterEnabled: reader.GetBoolean(7),
            ProfanityFilterReplacementWord: reader.GetString(8),
            RoomControllersListLimit: reader.GetInt32(9),
            InfobusOpen: reader.GetString(10) == "open",
            InfobusRoomRef: reader.IsDBNull(11) ? null : reader.GetInt32(11),
            new HappyHourSettings(reader.GetString(12), weekdayStart, weekdayEnd, weekendStart, weekendEnd),
            new RecyclerSettings(reader.GetString(14) == "1", reader.GetInt32(15), reader.GetInt32(16)),
            TradingEnabled: reader.GetString(17) == "1",
            MarketplaceEnabled: reader.GetString(18) == "1",
            DailyRespectGrant: reader.GetInt32(19),
            DailyPetRespectGrant: reader.GetInt32(20),
            FriendsLimit: reader.GetInt32(21),
            FriendsLimitClub: reader.GetInt32(22),
            MaxItemsInATrade: reader.GetInt32(23),
            MaxIgnoredUsers: reader.GetInt32(24),
            RoomEventsEnabled: reader.GetString(25) == "1",
            RoomBanDefaultTime: reader.GetInt32(26),
            RoomIdleTimeSleep: reader.GetInt32(27),
            RoomIdleTimeKick: reader.GetInt32(28),
            BanDefaultHours: reader.GetInt32(29),
            SuperbanDefaultDays: reader.GetInt32(30),
            BanDefaultReason: reader.GetString(31),
            MuteDefaultHours: reader.GetInt32(32),
            RoomMuteDefaultHours: reader.GetInt32(33),
            new PetSettings(reader.GetString(34) == "1", reader.GetInt32(35), reader.GetString(36), reader.GetInt32(37)),
            new GroupSettings(reader.GetInt32(38), reader.GetInt32(39)),
            new UsernameSettings(reader.GetString(40) == "1", reader.GetInt32(41), reader.GetInt32(46), reader.GetInt32(42), reader.GetInt32(43), reader.GetString(44)),
            MaxGuideBotsInHotelAtOnce: reader.GetInt32(45));
    }

    public async Task SetInfobusOpenAsync(bool open, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand("UPDATE global_game_settings SET infobus_status = @status;", connection);
        command.Parameters.AddWithValue("@status", open ? "open" : "closed");
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static (TimeOnly Start, TimeOnly End) ParseWindow(JsonElement window) =>
        (TimeOnly.ParseExact(window.GetProperty("start").GetString()!, "HH:mm"),
         TimeOnly.ParseExact(window.GetProperty("end").GetString()!, "HH:mm"));
}
