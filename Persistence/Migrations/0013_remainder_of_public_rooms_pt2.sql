ALTER TABLE `definition_public_room_warp_tiles`
  ADD COLUMN `room_ref` int unsigned DEFAULT NULL AFTER `heightmap_ref`,
  ADD COLUMN `target_room_ref` int unsigned DEFAULT NULL AFTER `target_heightmap_ref`,
  DROP INDEX `uq_definition_public_room_warp_tiles_tile`,
  ADD UNIQUE KEY `uq_definition_public_room_warp_tiles_tile` (`heightmap_ref`, (IFNULL(`room_ref`, 0)), `x`, `y`), -- a tile once a model, and once a room
  ADD KEY `idx_definition_public_room_warp_tiles_room_ref` (`room_ref`),
  ADD CONSTRAINT `fk_definition_public_room_warp_tiles_room` FOREIGN KEY (`room_ref`) REFERENCES `rooms` (`id`) ON DELETE CASCADE,
  ADD CONSTRAINT `fk_definition_public_room_warp_tiles_target_room` FOREIGN KEY (`target_room_ref`) REFERENCES `rooms` (`id`) ON DELETE CASCADE;

UPDATE `rooms` r
JOIN `definition_public_room_libraries` l ON l.`room_ref` = r.`id`
SET r.`label` = IF(l.`library_name` = 'hh_room_starlounge_cloudy', 'Cloudy With a Chance of Meatballs', r.`label`),
    r.`description` = CASE l.`library_name`
      WHEN 'hh_room_starlounge_cloudy' THEN 'Food is falling from the sky! Cloudy With a Chance of Meatballs, now in cinemas.'
      WHEN 'hh_room_branded_m6mobile' THEN 'Lost in a desert sandstorm? Your mobile still has signal, M6 Mobile by Orange.'
      WHEN 'hh_room_branded_iceage' THEN 'Chill out with the herd and the dinosaurs.'
    END
WHERE l.`library_name` IN ('hh_room_starlounge_cloudy', 'hh_room_branded_m6mobile', 'hh_room_branded_iceage');

UPDATE `definition_navigator_public_rooms` n
JOIN `definition_public_room_libraries` l ON l.`room_ref` = n.`room_ref`
SET n.`image_type` = 'external', n.`image` = CASE l.`library_name`
    WHEN 'hh_room_starlounge_cloudy' THEN 'officialrooms_hq/uk_cloudy_navigator.gif'
    WHEN 'hh_room_sport_fireservices' THEN 'officialrooms_gb/fireservices_02.png'
    WHEN 'hh_room_tv_studio_antena3' THEN 'officialrooms_es/antena3.png'
    WHEN 'hh_room_theater_idol_latin' THEN 'officialrooms_es/latin_idol_navigator.png'
    WHEN 'hh_room_brgrefusa' THEN 'officialrooms_es/grefusa_icon.png'
    WHEN 'hh_room_theater_coolpol' THEN 'officialrooms_nl/coolpolitics_rooms.gif'
  END
WHERE l.`library_name` IN ('hh_room_starlounge_cloudy', 'hh_room_sport_fireservices', 'hh_room_tv_studio_antena3',
                           'hh_room_theater_idol_latin', 'hh_room_brgrefusa', 'hh_room_theater_coolpol');

SELECT `id` INTO @sponsored FROM `definition_navigator_public_rooms_categories` WHERE `caption` = 'Sponsored Rooms';

INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'Clearasil Star Lounge', 'Clear skin, clear stage, shine at the Clearasil Star Lounge!', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'star_lounge';
SET @room = LAST_INSERT_ID();
INSERT INTO `definition_public_room_libraries` (`heightmap_ref`, `room_ref`, `library_name`, `nav_code`)
SELECT `id`, @room, 'hh_room_starlounge_clear', 'starlounge_clear' FROM `definition_room_heightmap` WHERE `label` = 'star_lounge';
INSERT INTO `definition_navigator_public_rooms` (`order_num`, `image_size`, `image`, `image_type`, `room_ref`, `category_ref`, `club_only`, `visible`, `nav_index`)
VALUES (0, 'wide', 'officialrooms_hq/clearasil_nav.png', 'external', @room, @sponsored, 0, '1', 0);

INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'NRJ Music Awards', 'Walk the red carpet at the NRJ Music Awards!', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'star_lounge';
SET @room = LAST_INSERT_ID();
INSERT INTO `definition_public_room_libraries` (`heightmap_ref`, `room_ref`, `library_name`, `nav_code`)
SELECT `id`, @room, 'hh_room_starlounge_nrj', 'starlounge_nrj' FROM `definition_room_heightmap` WHERE `label` = 'star_lounge';
INSERT INTO `definition_navigator_public_rooms` (`order_num`, `image_size`, `image`, `image_type`, `room_ref`, `category_ref`, `club_only`, `visible`, `nav_index`)
VALUES (0, 'wide', 'officialrooms_hq/hh_room_starlounge_nrj.png', 'external', @room, @sponsored, 0, '1', 0);

INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'Nokia Club', 'Music gets you talking, so dance the night away at the Nokia Club!', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'malja_bar_a';
SET @down = LAST_INSERT_ID();
INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'Nokia Club (2)', 'Catch your breath upstairs at the Nokia Club.', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'malja_bar_b';
SET @up = LAST_INSERT_ID();
INSERT INTO `definition_public_room_libraries` (`heightmap_ref`, `room_ref`, `library_name`, `nav_code`)
SELECT `id`, IF(`label` = 'malja_bar_a', @down, @up), 'hh_room_disco_nokia', 'disco_nokia' FROM `definition_room_heightmap` WHERE `label` IN ('malja_bar_a', 'malja_bar_b');
INSERT INTO `definition_navigator_public_rooms` (`order_num`, `image_size`, `image`, `image_type`, `room_ref`, `category_ref`, `club_only`, `visible`, `nav_index`)
VALUES (0, 'wide', 'officialrooms_hq/hh_room_disco_nokia.png', 'external', @down, @sponsored, 0, '1', 0),
       (0, 'wide', 'officialrooms_hq/hh_room_disco_nokia.png', 'external', @up, @sponsored, 0, '0', 0);
INSERT INTO `definition_public_room_warp_tiles` (`heightmap_ref`, `room_ref`, `x`, `y`, `target_heightmap_ref`, `target_room_ref`, `target_x`, `target_y`)
SELECT w.`heightmap_ref`, IF(h.`label` = 'malja_bar_a', @down, @up), w.`x`, w.`y`, w.`target_heightmap_ref`, IF(h.`label` = 'malja_bar_a', @up, @down), w.`target_x`, w.`target_y`
FROM `definition_public_room_warp_tiles` w JOIN `definition_room_heightmap` h ON h.`id` = w.`heightmap_ref`
WHERE h.`label` IN ('malja_bar_a', 'malja_bar_b') AND w.`room_ref` IS NULL ORDER BY w.`id`;

INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'Nokia X6', 'Free turn-by-turn navigation with the new Ovi Maps, so you''ll never miss the party on your Nokia X6!', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'malja_bar_a';
SET @down = LAST_INSERT_ID();
INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'Nokia X6 (2)', 'Catch your breath upstairs at the Nokia X6.', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'malja_bar_b';
SET @up = LAST_INSERT_ID();
INSERT INTO `definition_public_room_libraries` (`heightmap_ref`, `room_ref`, `library_name`, `nav_code`)
SELECT `id`, IF(`label` = 'malja_bar_a', @down, @up), 'hh_room_disco_nokia_x6', 'disco_nokia_x6' FROM `definition_room_heightmap` WHERE `label` IN ('malja_bar_a', 'malja_bar_b');
INSERT INTO `definition_navigator_public_rooms` (`order_num`, `image_size`, `image`, `image_type`, `room_ref`, `category_ref`, `club_only`, `visible`, `nav_index`)
VALUES (0, 'wide', 'officialrooms_hq/hh_room_disco_nokia_x6.png', 'external', @down, @sponsored, 0, '1', 0),
       (0, 'wide', 'officialrooms_hq/hh_room_disco_nokia_x6.png', 'external', @up, @sponsored, 0, '0', 0);
INSERT INTO `definition_public_room_warp_tiles` (`heightmap_ref`, `room_ref`, `x`, `y`, `target_heightmap_ref`, `target_room_ref`, `target_x`, `target_y`)
SELECT w.`heightmap_ref`, IF(h.`label` = 'malja_bar_a', @down, @up), w.`x`, w.`y`, w.`target_heightmap_ref`, IF(h.`label` = 'malja_bar_a', @up, @down), w.`target_x`, w.`target_y`
FROM `definition_public_room_warp_tiles` w JOIN `definition_room_heightmap` h ON h.`id` = w.`heightmap_ref`
WHERE h.`label` IN ('malja_bar_a', 'malja_bar_b') AND w.`room_ref` IS NULL ORDER BY w.`id`;

UPDATE `definition_navigator_public_rooms` n
JOIN (SELECT n2.`id`, ROW_NUMBER() OVER (ORDER BY h.`label`, l.`library_name`, n2.`visible` DESC) AS `place`
      FROM `definition_navigator_public_rooms` n2
      JOIN `definition_public_room_libraries` l ON l.`room_ref` = n2.`room_ref`
      JOIN `definition_room_heightmap` h ON h.`id` = l.`heightmap_ref`
      WHERE n2.`category_ref` = @sponsored) ordered ON ordered.`id` = n.`id`
SET n.`order_num` = ordered.`place`;
