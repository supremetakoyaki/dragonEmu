ALTER TABLE `global_game_settings`
  ADD COLUMN `max_spectators` int NOT NULL DEFAULT '100' COMMENT 'how many may watch a full room at once, 0 none' AFTER `idle_disconnect_time`;
