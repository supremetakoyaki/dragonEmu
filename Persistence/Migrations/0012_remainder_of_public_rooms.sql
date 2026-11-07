ALTER TABLE `definition_public_room_libraries`
  ADD COLUMN `room_ref` int unsigned DEFAULT NULL AFTER `heightmap_ref`,
  DROP INDEX `uq_definition_public_room_libraries_heightmap`,
  ADD KEY `idx_definition_public_room_libraries_heightmap` (`heightmap_ref`),
  ADD UNIQUE KEY `uq_definition_public_room_libraries_room` (`room_ref`),
  ADD CONSTRAINT `fk_definition_public_room_libraries_room` FOREIGN KEY (`room_ref`) REFERENCES `rooms` (`id`) ON DELETE CASCADE;

ALTER TABLE `definition_public_room_items`
  ADD COLUMN `room_ref` int unsigned DEFAULT NULL AFTER `heightmap_ref`,
  ADD KEY `idx_definition_public_room_items_room_ref` (`room_ref`),
  ADD CONSTRAINT `fk_definition_public_room_items_room` FOREIGN KEY (`room_ref`) REFERENCES `rooms` (`id`) ON DELETE CASCADE;

UPDATE `definition_public_room_furniture` SET `classname` = 'crs_tvtable' WHERE `classname` = 'tvtable';
UPDATE `definition_public_room_furniture` SET `classname` = 'crs_coffeetable_start' WHERE `classname` = 'ctable1';
UPDATE `definition_public_room_furniture` SET `classname` = 'crs_coffeetable_end' WHERE `classname` = 'ctable2';

UPDATE `definition_public_room_items` i
JOIN `definition_room_heightmap` h ON h.`id` = i.`heightmap_ref` JOIN `definition_public_room_furniture` f ON f.`id` = i.`furni_ref`
SET i.`rotation` = 4, i.`y` = i.`y` - 1 WHERE h.`label` = 'cr_staff' AND f.`classname` = 'crs_tvtable';
INSERT INTO `definition_public_room_furniture` (`classname`, `x_dim`, `y_dim`, `z_dim`) VALUES
  ('crs_biljard_start', 1, 1, 0.0010),
  ('crs_biljard_end', 1, 1, 0.0010);
UPDATE `definition_public_room_items` i
JOIN `definition_room_heightmap` h ON h.`id` = i.`heightmap_ref` JOIN `definition_public_room_furniture` f ON f.`id` = i.`furni_ref`
JOIN `definition_public_room_furniture` b ON b.`classname` = REPLACE(f.`classname`, 'crs_coffeetable_', 'crs_biljard_')
SET i.`furni_ref` = b.`id`, i.`rotation` = 4, i.`x` = i.`x` - 1, i.`y` = i.`y` - 2 WHERE h.`label` = 'cr_staff' AND f.`classname` IN ('crs_coffeetable_start', 'crs_coffeetable_end');
UPDATE `definition_public_room_items` i
JOIN `definition_room_heightmap` h ON h.`id` = i.`heightmap_ref` JOIN `definition_public_room_furniture` f ON f.`id` = i.`furni_ref`
JOIN `definition_public_room_furniture` c ON c.`classname` = IF(f.`classname` = 'crs_woodtable_end', 'crs_coffeetable_start', 'crs_coffeetable_end')
SET i.`furni_ref` = c.`id`, i.`x` = IF(f.`classname` = 'crs_woodtable_end', i.`x` - 1, i.`x`)
WHERE h.`label` = 'cr_staff' AND f.`classname` IN ('crs_woodtable_start', 'crs_woodtable_end');

DELETE i FROM `definition_public_room_items` i
JOIN `definition_room_heightmap` h ON h.`id` = i.`heightmap_ref` JOIN `definition_public_room_furniture` f ON f.`id` = i.`furni_ref`
WHERE h.`label` = 'netcafe' AND f.`classname` = 'watermatic';

INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'The Dudesons'' Ranch', 'The Dudesons'' very own ranch.', 10, `id`, NULL, 0, 50, 0, '201', '111', ''
FROM `definition_room_heightmap` WHERE `label` = 'picnic';
SET @dudesons = LAST_INSERT_ID();

INSERT INTO `definition_public_room_libraries` (`heightmap_ref`, `room_ref`, `library_name`, `nav_code`)
SELECT `id`, @dudesons, 'hh_room_picnic_dudesons', 'picnic_dudesons' FROM `definition_room_heightmap` WHERE `label` = 'picnic';

INSERT INTO `definition_navigator_public_rooms_categories` (`order_num`, `caption`, `image`) VALUES (175, 'Sponsored Rooms', '');
SET @sponsored = LAST_INSERT_ID();

INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'Garnier Beauty Salon', 'Fresh looks, the natural way.', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'beauty_salon0';
SET @room = LAST_INSERT_ID();
INSERT INTO `definition_public_room_libraries` (`heightmap_ref`, `room_ref`, `library_name`, `nav_code`)
SELECT `id`, @room, 'hh_room_beauty_salon_garnier', 'beauty_salon_garnier' FROM `definition_room_heightmap` WHERE `label` = 'beauty_salon0';
INSERT INTO `definition_navigator_public_rooms` (`order_num`, `image_size`, `image`, `image_type`, `room_ref`, `category_ref`, `club_only`, `visible`, `nav_index`)
VALUES (1, 'wide', 'officialrooms_hq/hh_room_beauty_salon_garnier.png', 'external', @room, @sponsored, 0, '1', 0);

INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'Libra Beauty Salon', 'Pamper yourself in pink.', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'beauty_salon0';
SET @room = LAST_INSERT_ID();
INSERT INTO `definition_public_room_libraries` (`heightmap_ref`, `room_ref`, `library_name`, `nav_code`)
SELECT `id`, @room, 'hh_room_beauty_salon_libra', 'beauty_salon_libra' FROM `definition_room_heightmap` WHERE `label` = 'beauty_salon0';
INSERT INTO `definition_navigator_public_rooms` (`order_num`, `image_size`, `image`, `image_type`, `room_ref`, `category_ref`, `club_only`, `visible`, `nav_index`)
VALUES (2, 'wide', 'officialrooms_hq/hh_room_beauty_salon_libra.png', 'external', @room, @sponsored, 0, '1', 0);

INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'Orange Crush Ice Lounge', 'Cool off with an icy Orange Crush.', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'branded';
SET @room = LAST_INSERT_ID();
INSERT INTO `definition_public_room_libraries` (`heightmap_ref`, `room_ref`, `library_name`, `nav_code`)
SELECT `id`, @room, 'hh_room_ads_crush', 'ads_crush' FROM `definition_room_heightmap` WHERE `label` = 'branded';
INSERT INTO `definition_navigator_public_rooms` (`order_num`, `image_size`, `image`, `image_type`, `room_ref`, `category_ref`, `club_only`, `visible`, `nav_index`)
VALUES (3, 'wide', 'officialrooms_hq/hh_room_ads_crush.png', 'external', @room, @sponsored, 0, '1', 0);

INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'The Capri-Sun Theme Park', 'Get involved to get your hands on exclusive branded Furni and loads of other great prizes.', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'branded';
SET @room = LAST_INSERT_ID();
INSERT INTO `definition_public_room_libraries` (`heightmap_ref`, `room_ref`, `library_name`, `nav_code`)
SELECT `id`, @room, 'hh_room_branded_capri', 'branded_capri' FROM `definition_room_heightmap` WHERE `label` = 'branded';
INSERT INTO `definition_navigator_public_rooms` (`order_num`, `image_size`, `image`, `image_type`, `room_ref`, `category_ref`, `club_only`, `visible`, `nav_index`)
VALUES (4, 'wide', 'officialrooms_hq/hh_room_branded_capri.png', 'external', @room, @sponsored, 0, '1', 0);

INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'Cheetolab', 'A giant Chester, TV cameras and Cheetos machines! Ask us for a bag of Cheetos and it''s yours. See you there!', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'branded';
SET @room = LAST_INSERT_ID();
INSERT INTO `definition_public_room_libraries` (`heightmap_ref`, `room_ref`, `library_name`, `nav_code`)
SELECT `id`, @room, 'hh_room_branded_cheetos', 'branded_cheetos' FROM `definition_room_heightmap` WHERE `label` = 'branded';
INSERT INTO `definition_navigator_public_rooms` (`order_num`, `image_size`, `image`, `image_type`, `room_ref`, `category_ref`, `club_only`, `visible`, `nav_index`)
VALUES (5, 'wide', 'officialrooms_hq/hh_room_branded_cheetos.png', 'external', @room, @sponsored, 0, '1', 0);

INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'Chupa Chups', 'For a less serious summer!', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'branded';
SET @room = LAST_INSERT_ID();
INSERT INTO `definition_public_room_libraries` (`heightmap_ref`, `room_ref`, `library_name`, `nav_code`)
SELECT `id`, @room, 'hh_room_branded_chupa', 'branded_chupa' FROM `definition_room_heightmap` WHERE `label` = 'branded';
INSERT INTO `definition_navigator_public_rooms` (`order_num`, `image_size`, `image`, `image_type`, `room_ref`, `category_ref`, `club_only`, `visible`, `nav_index`)
VALUES (6, 'wide', 'officialrooms_hq/hh_room_branded_chupa.png', 'external', @room, @sponsored, 0, '1', 0);

INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'CREATE2011 City', 'Are you a budding Engineer who''s brimming with ideas? Then the CREATE2011 is for you!', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'branded';
SET @room = LAST_INSERT_ID();
INSERT INTO `definition_public_room_libraries` (`heightmap_ref`, `room_ref`, `library_name`, `nav_code`)
SELECT `id`, @room, 'hh_room_branded_create', 'branded_create' FROM `definition_room_heightmap` WHERE `label` = 'branded';
INSERT INTO `definition_navigator_public_rooms` (`order_num`, `image_size`, `image`, `image_type`, `room_ref`, `category_ref`, `club_only`, `visible`, `nav_index`)
VALUES (7, 'wide', 'officialrooms_hq/hh_room_branded_create.png', 'external', @room, @sponsored, 0, '1', 0);

INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'Crush', 'Come by for a brand new Cherry Crush!', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'branded';
SET @room = LAST_INSERT_ID();
INSERT INTO `definition_public_room_libraries` (`heightmap_ref`, `room_ref`, `library_name`, `nav_code`)
SELECT `id`, @room, 'hh_room_branded_crush', 'branded_crush' FROM `definition_room_heightmap` WHERE `label` = 'branded';
INSERT INTO `definition_navigator_public_rooms` (`order_num`, `image_size`, `image`, `image_type`, `room_ref`, `category_ref`, `club_only`, `visible`, `nav_index`)
VALUES (8, 'wide', 'officialrooms_hq/hh_room_branded_crush.png', 'external', @room, @sponsored, 0, '1', 0);

INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'Disney XD', 'Hang out in the Disney XD room.', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'branded';
SET @room = LAST_INSERT_ID();
INSERT INTO `definition_public_room_libraries` (`heightmap_ref`, `room_ref`, `library_name`, `nav_code`)
SELECT `id`, @room, 'hh_room_branded_disneyxd', 'branded_disneyxd' FROM `definition_room_heightmap` WHERE `label` = 'branded';
INSERT INTO `definition_navigator_public_rooms` (`order_num`, `image_size`, `image`, `image_type`, `room_ref`, `category_ref`, `club_only`, `visible`, `nav_index`)
VALUES (9, 'wide', 'officialrooms_hq/hh_room_branded_disneyxd.png', 'external', @room, @sponsored, 0, '1', 0);

INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'Disney XD II', 'Hang out in the Disney XD room.', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'branded';
SET @room = LAST_INSERT_ID();
INSERT INTO `definition_public_room_libraries` (`heightmap_ref`, `room_ref`, `library_name`, `nav_code`)
SELECT `id`, @room, 'hh_room_branded_disneyxd2', 'branded_disneyxd2' FROM `definition_room_heightmap` WHERE `label` = 'branded';
INSERT INTO `definition_navigator_public_rooms` (`order_num`, `image_size`, `image`, `image_type`, `room_ref`, `category_ref`, `club_only`, `visible`, `nav_index`)
VALUES (10, 'wide', 'officialrooms_hq/hh_room_branded_disneyxd2.png', 'external', @room, @sponsored, 0, '1', 0);

INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'Disney XD III', 'Hang out in the Disney XD room.', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'branded';
SET @room = LAST_INSERT_ID();
INSERT INTO `definition_public_room_libraries` (`heightmap_ref`, `room_ref`, `library_name`, `nav_code`)
SELECT `id`, @room, 'hh_room_branded_disneyxd3', 'branded_disneyxd3' FROM `definition_room_heightmap` WHERE `label` = 'branded';
INSERT INTO `definition_navigator_public_rooms` (`order_num`, `image_size`, `image`, `image_type`, `room_ref`, `category_ref`, `club_only`, `visible`, `nav_index`)
VALUES (11, 'wide', 'officialrooms_hq/hh_room_branded_disneyxd3.png', 'external', @room, @sponsored, 0, '1', 0);

INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'Disney XD IV', 'Hang out in the Disney XD room.', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'branded';
SET @room = LAST_INSERT_ID();
INSERT INTO `definition_public_room_libraries` (`heightmap_ref`, `room_ref`, `library_name`, `nav_code`)
SELECT `id`, @room, 'hh_room_branded_disneyxd4', 'branded_disneyxd4' FROM `definition_room_heightmap` WHERE `label` = 'branded';
INSERT INTO `definition_navigator_public_rooms` (`order_num`, `image_size`, `image`, `image_type`, `room_ref`, `category_ref`, `club_only`, `visible`, `nav_index`)
VALUES (12, 'wide', 'officialrooms_hq/hh_room_branded_disneyxd4.png', 'external', @room, @sponsored, 0, '1', 0);

INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'Disney XD V', 'Hang out in the Disney XD room.', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'branded';
SET @room = LAST_INSERT_ID();
INSERT INTO `definition_public_room_libraries` (`heightmap_ref`, `room_ref`, `library_name`, `nav_code`)
SELECT `id`, @room, 'hh_room_branded_disneyxd5', 'branded_disneyxd5' FROM `definition_room_heightmap` WHERE `label` = 'branded';
INSERT INTO `definition_navigator_public_rooms` (`order_num`, `image_size`, `image`, `image_type`, `room_ref`, `category_ref`, `club_only`, `visible`, `nav_index`)
VALUES (13, 'wide', 'officialrooms_hq/hh_room_branded_disneyxd5.png', 'external', @room, @sponsored, 0, '1', 0);

INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'Disney XD VI', 'Hang out in the Disney XD room.', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'branded';
SET @room = LAST_INSERT_ID();
INSERT INTO `definition_public_room_libraries` (`heightmap_ref`, `room_ref`, `library_name`, `nav_code`)
SELECT `id`, @room, 'hh_room_branded_disneyxd6', 'branded_disneyxd6' FROM `definition_room_heightmap` WHERE `label` = 'branded';
INSERT INTO `definition_navigator_public_rooms` (`order_num`, `image_size`, `image`, `image_type`, `room_ref`, `category_ref`, `club_only`, `visible`, `nav_index`)
VALUES (14, 'wide', 'officialrooms_hq/hh_room_branded_disneyxd6.png', 'external', @room, @sponsored, 0, '1', 0);

INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'Disney XD VII', 'Hang out in the Disney XD room.', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'branded';
SET @room = LAST_INSERT_ID();
INSERT INTO `definition_public_room_libraries` (`heightmap_ref`, `room_ref`, `library_name`, `nav_code`)
SELECT `id`, @room, 'hh_room_branded_disneyxd7', 'branded_disneyxd7' FROM `definition_room_heightmap` WHERE `label` = 'branded';
INSERT INTO `definition_navigator_public_rooms` (`order_num`, `image_size`, `image`, `image_type`, `room_ref`, `category_ref`, `club_only`, `visible`, `nav_index`)
VALUES (15, 'wide', 'officialrooms_hq/hh_room_branded_disneyxd7.png', 'external', @room, @sponsored, 0, '1', 0);

INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'The House of Hiccup', 'How To Train Your Dragon is out in cinemas nationwide on 31st March.', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'branded';
SET @room = LAST_INSERT_ID();
INSERT INTO `definition_public_room_libraries` (`heightmap_ref`, `room_ref`, `library_name`, `nav_code`)
SELECT `id`, @room, 'hh_room_branded_dragon', 'branded_dragon' FROM `definition_room_heightmap` WHERE `label` = 'branded';
INSERT INTO `definition_navigator_public_rooms` (`order_num`, `image_size`, `image`, `image_type`, `room_ref`, `category_ref`, `club_only`, `visible`, `nav_index`)
VALUES (16, 'wide', 'officialrooms_hq/hh_room_branded_dragon.png', 'external', @room, @sponsored, 0, '1', 0);

INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'Hacker Hunt HQ', 'Catch the hackers before they catch you.', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'branded';
SET @room = LAST_INSERT_ID();
INSERT INTO `definition_public_room_libraries` (`heightmap_ref`, `room_ref`, `library_name`, `nav_code`)
SELECT `id`, @room, 'hh_room_branded_hacker', 'branded_hacker' FROM `definition_room_heightmap` WHERE `label` = 'branded';
INSERT INTO `definition_navigator_public_rooms` (`order_num`, `image_size`, `image`, `image_type`, `room_ref`, `category_ref`, `club_only`, `visible`, `nav_index`)
VALUES (17, 'wide', 'officialrooms_hq/hh_room_branded_hacker.png', 'external', @room, @sponsored, 0, '1', 0);

INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'Ice Age: Dawn of the Dinosaurs', 'Chill out with the herd - and the dinosaurs.', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'branded';
SET @room = LAST_INSERT_ID();
INSERT INTO `definition_public_room_libraries` (`heightmap_ref`, `room_ref`, `library_name`, `nav_code`)
SELECT `id`, @room, 'hh_room_branded_iceage', 'branded_iceage' FROM `definition_room_heightmap` WHERE `label` = 'branded';
INSERT INTO `definition_navigator_public_rooms` (`order_num`, `image_size`, `image`, `image_type`, `room_ref`, `category_ref`, `club_only`, `visible`, `nav_index`)
VALUES (18, 'wide', 'officialrooms_hq/hh_room_branded_iceage.png', 'external', @room, @sponsored, 0, '1', 0);

INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'Kellogg''s Trésor', 'Join the hunt for the choco-devourers and win the best prizes!', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'branded';
SET @room = LAST_INSERT_ID();
INSERT INTO `definition_public_room_libraries` (`heightmap_ref`, `room_ref`, `library_name`, `nav_code`)
SELECT `id`, @room, 'hh_room_branded_kelloggs', 'branded_kelloggs' FROM `definition_room_heightmap` WHERE `label` = 'branded';
INSERT INTO `definition_navigator_public_rooms` (`order_num`, `image_size`, `image`, `image_type`, `room_ref`, `category_ref`, `club_only`, `visible`, `nav_index`)
VALUES (19, 'wide', 'officialrooms_hq/hh_room_branded_kelloggs.png', 'external', @room, @sponsored, 0, '1', 0);

INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'M6 Mobile Sandstorm', 'Lost in a desert sandstorm? Your mobile still has signal - M6 Mobile by Orange.', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'branded';
SET @room = LAST_INSERT_ID();
INSERT INTO `definition_public_room_libraries` (`heightmap_ref`, `room_ref`, `library_name`, `nav_code`)
SELECT `id`, @room, 'hh_room_branded_m6mobile', 'branded_m6mobile' FROM `definition_room_heightmap` WHERE `label` = 'branded';
INSERT INTO `definition_navigator_public_rooms` (`order_num`, `image_size`, `image`, `image_type`, `room_ref`, `category_ref`, `club_only`, `visible`, `nav_index`)
VALUES (20, 'wide', 'officialrooms_hq/hh_room_branded_m6mobile.png', 'external', @room, @sponsored, 0, '1', 0);

INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'ELEMAGIKA', 'Come in and check out Paddle Pop Lion in his all new feature movie! Join the adventure NOW!', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'branded';
SET @room = LAST_INSERT_ID();
INSERT INTO `definition_public_room_libraries` (`heightmap_ref`, `room_ref`, `library_name`, `nav_code`)
SELECT `id`, @room, 'hh_room_branded_paddlepop', 'branded_paddlepop' FROM `definition_room_heightmap` WHERE `label` = 'branded';
INSERT INTO `definition_navigator_public_rooms` (`order_num`, `image_size`, `image`, `image_type`, `room_ref`, `category_ref`, `club_only`, `visible`, `nav_index`)
VALUES (21, 'wide', 'officialrooms_hq/hh_room_branded_paddlepop.png', 'external', @room, @sponsored, 0, '1', 0);

INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'Mount Olympus', 'Percy Jackson''s world of gods and heroes.', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'branded';
SET @room = LAST_INSERT_ID();
INSERT INTO `definition_public_room_libraries` (`heightmap_ref`, `room_ref`, `library_name`, `nav_code`)
SELECT `id`, @room, 'hh_room_branded_percy', 'branded_percy' FROM `definition_room_heightmap` WHERE `label` = 'branded';
INSERT INTO `definition_navigator_public_rooms` (`order_num`, `image_size`, `image`, `image_type`, `room_ref`, `category_ref`, `club_only`, `visible`, `nav_index`)
VALUES (22, 'wide', 'officialrooms_hq/hh_room_branded_percy.png', 'external', @room, @sponsored, 0, '1', 0);

INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'Volkswagen Showroom', 'Take a look at the latest Volkswagen.', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'branded';
SET @room = LAST_INSERT_ID();
INSERT INTO `definition_public_room_libraries` (`heightmap_ref`, `room_ref`, `library_name`, `nav_code`)
SELECT `id`, @room, 'hh_room_branded_volkswagen', 'branded_volkswagen' FROM `definition_room_heightmap` WHERE `label` = 'branded';
INSERT INTO `definition_navigator_public_rooms` (`order_num`, `image_size`, `image`, `image_type`, `room_ref`, `category_ref`, `club_only`, `visible`, `nav_index`)
VALUES (23, 'wide', 'officialrooms_hq/hh_room_branded_volkswagen.png', 'external', @room, @sponsored, 0, '1', 0);

INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'The Wizards of Waverly Place Lair', 'Talk about what messed up magic you would create! WOWP on the Disney Channel from the 25th April in the UK.', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'branded';
SET @room = LAST_INSERT_ID();
INSERT INTO `definition_public_room_libraries` (`heightmap_ref`, `room_ref`, `library_name`, `nav_code`)
SELECT `id`, @room, 'hh_room_branded_wizards', 'branded_wizards' FROM `definition_room_heightmap` WHERE `label` = 'branded';
INSERT INTO `definition_navigator_public_rooms` (`order_num`, `image_size`, `image`, `image_type`, `room_ref`, `category_ref`, `club_only`, `visible`, `nav_index`)
VALUES (24, 'wide', 'officialrooms_hq/hh_room_branded_wizards.png', 'external', @room, @sponsored, 0, '1', 0);

INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'CREATE2010 City', 'Got an idea that could change the world? Bring your best invention to the CREATE2010 contest and show the city what you can build!', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'branded';
SET @room = LAST_INSERT_ID();
INSERT INTO `definition_public_room_libraries` (`heightmap_ref`, `room_ref`, `library_name`, `nav_code`)
SELECT `id`, @room, 'hh_room_brcroate', 'brcroate' FROM `definition_room_heightmap` WHERE `label` = 'branded';
INSERT INTO `definition_navigator_public_rooms` (`order_num`, `image_size`, `image`, `image_type`, `room_ref`, `category_ref`, `club_only`, `visible`, `nav_index`)
VALUES (25, 'wide', 'officialrooms_hq/hh_room_brcroate.png', 'external', @room, @sponsored, 0, '1', 0);

INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'GREFUPARTY ROOM', 'It''s Grefusito''s birthday! Celebrate at the Grefuparty and win the best prizes this summer!', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'branded';
SET @room = LAST_INSERT_ID();
INSERT INTO `definition_public_room_libraries` (`heightmap_ref`, `room_ref`, `library_name`, `nav_code`)
SELECT `id`, @room, 'hh_room_brgrefusa', 'brgrefusa' FROM `definition_room_heightmap` WHERE `label` = 'branded';
INSERT INTO `definition_navigator_public_rooms` (`order_num`, `image_size`, `image`, `image_type`, `room_ref`, `category_ref`, `club_only`, `visible`, `nav_index`)
VALUES (26, 'wide', 'officialrooms_hq/hh_room_brgrefusa.png', 'external', @room, @sponsored, 0, '1', 0);

INSERT INTO `definition_navigator_public_rooms` (`order_num`, `image_size`, `image`, `image_type`, `room_ref`, `category_ref`, `club_only`, `visible`, `nav_index`)
VALUES (27, 'wide', 'officialrooms_hq/hh_room_picnic_dudesons.png', 'external', @dudesons, @sponsored, 0, '1', 0);

INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'Fire Station', 'Where Habbo''s bravest hang out.', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'sport';
SET @room = LAST_INSERT_ID();
INSERT INTO `definition_public_room_libraries` (`heightmap_ref`, `room_ref`, `library_name`, `nav_code`)
SELECT `id`, @room, 'hh_room_sport_fireservices', 'sport_fireservices' FROM `definition_room_heightmap` WHERE `label` = 'sport';
INSERT INTO `definition_navigator_public_rooms` (`order_num`, `image_size`, `image`, `image_type`, `room_ref`, `category_ref`, `club_only`, `visible`, `nav_index`)
VALUES (28, 'wide', 'officialrooms_hq/hh_room_sport_fireservices.png', 'external', @room, @sponsored, 0, '1', 0);
INSERT INTO `definition_public_room_items` (`furni_ref`, `heightmap_ref`, `room_ref`, `x`, `y`, `z`, `rotation`)
SELECT i.`furni_ref`, i.`heightmap_ref`, @room, i.`x`, i.`y`, i.`z`, i.`rotation` FROM `definition_public_room_items` i
JOIN `definition_public_room_furniture` f ON f.`id` = i.`furni_ref` JOIN `definition_room_heightmap` h ON h.`id` = i.`heightmap_ref`
WHERE h.`label` = 'sport' AND i.`room_ref` IS NULL AND f.`classname` NOT IN ('sodagreen', 'sodapink') ORDER BY i.`id`;

INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'Sunset Star Lounge', 'The Star Lounge under a golden sky.', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'star_lounge';
SET @room = LAST_INSERT_ID();
INSERT INTO `definition_public_room_libraries` (`heightmap_ref`, `room_ref`, `library_name`, `nav_code`)
SELECT `id`, @room, 'hh_room_starlounge_cloudy', 'starlounge_cloudy' FROM `definition_room_heightmap` WHERE `label` = 'star_lounge';
INSERT INTO `definition_navigator_public_rooms` (`order_num`, `image_size`, `image`, `image_type`, `room_ref`, `category_ref`, `club_only`, `visible`, `nav_index`)
VALUES (29, 'wide', 'officialrooms_hq/hh_room_starlounge_cloudy.png', 'external', @room, @sponsored, 0, '1', 0);

INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'Coolpolitics Theatre', 'Speak up and be heard on the Coolpolitics stage.', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'theater';
SET @room = LAST_INSERT_ID();
INSERT INTO `definition_public_room_libraries` (`heightmap_ref`, `room_ref`, `library_name`, `nav_code`)
SELECT `id`, @room, 'hh_room_theater_coolpol', 'theater_coolpol' FROM `definition_room_heightmap` WHERE `label` = 'theater';
INSERT INTO `definition_navigator_public_rooms` (`order_num`, `image_size`, `image`, `image_type`, `room_ref`, `category_ref`, `club_only`, `visible`, `nav_index`)
VALUES (30, 'wide', 'officialrooms_hq/hh_room_theater_coolpol.png', 'external', @room, @sponsored, 0, '1', 0);

INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'American Idol Theatre', 'Take the stage, Idol style.', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'theater';
SET @room = LAST_INSERT_ID();
INSERT INTO `definition_public_room_libraries` (`heightmap_ref`, `room_ref`, `library_name`, `nav_code`)
SELECT `id`, @room, 'hh_room_theater_idol', 'theater_idol' FROM `definition_room_heightmap` WHERE `label` = 'theater';
INSERT INTO `definition_navigator_public_rooms` (`order_num`, `image_size`, `image`, `image_type`, `room_ref`, `category_ref`, `club_only`, `visible`, `nav_index`)
VALUES (31, 'wide', 'officialrooms_hq/hh_room_theater_idol.png', 'external', @room, @sponsored, 0, '1', 0);

INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'Latin American Idol Theatre', 'Take the stage, Latin American Idol style.', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'theater';
SET @room = LAST_INSERT_ID();
INSERT INTO `definition_public_room_libraries` (`heightmap_ref`, `room_ref`, `library_name`, `nav_code`)
SELECT `id`, @room, 'hh_room_theater_idol_latin', 'theater_idol_latin' FROM `definition_room_heightmap` WHERE `label` = 'theater';
INSERT INTO `definition_navigator_public_rooms` (`order_num`, `image_size`, `image`, `image_type`, `room_ref`, `category_ref`, `club_only`, `visible`, `nav_index`)
VALUES (32, 'wide', 'officialrooms_hq/hh_room_theater_idol_latin.png', 'external', @room, @sponsored, 0, '1', 0);

INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'MTV EMA 2010', 'Support your favorite artists! Find the MTV EMAI article to vote for them right now and you could win a Trophy!', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'theater';
SET @room = LAST_INSERT_ID();
INSERT INTO `definition_public_room_libraries` (`heightmap_ref`, `room_ref`, `library_name`, `nav_code`)
SELECT `id`, @room, 'hh_room_theater_mtv', 'theater_mtv' FROM `definition_room_heightmap` WHERE `label` = 'theater';
INSERT INTO `definition_navigator_public_rooms` (`order_num`, `image_size`, `image`, `image_type`, `room_ref`, `category_ref`, `club_only`, `visible`, `nav_index`)
VALUES (33, 'wide', 'officialrooms_hq/hh_room_theater_mtv.png', 'external', @room, @sponsored, 0, '1', 0);

INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'Nouvelle Star Theatre', 'Sing your heart out on the Nouvelle Star stage.', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'theater';
SET @room = LAST_INSERT_ID();
INSERT INTO `definition_public_room_libraries` (`heightmap_ref`, `room_ref`, `library_name`, `nav_code`)
SELECT `id`, @room, 'hh_room_theater_nouvelle', 'theater_nouvelle' FROM `definition_room_heightmap` WHERE `label` = 'theater';
INSERT INTO `definition_navigator_public_rooms` (`order_num`, `image_size`, `image`, `image_type`, `room_ref`, `category_ref`, `club_only`, `visible`, `nav_index`)
VALUES (34, 'wide', 'officialrooms_hq/hh_room_theater_nouvelle.png', 'external', @room, @sponsored, 0, '1', 0);

INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'Animax TV', 'Be different!', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'tv_studio';
SET @room = LAST_INSERT_ID();
INSERT INTO `definition_public_room_libraries` (`heightmap_ref`, `room_ref`, `library_name`, `nav_code`)
SELECT `id`, @room, 'hh_room_tv_studio_animax', 'tv_studio_animax' FROM `definition_room_heightmap` WHERE `label` = 'tv_studio';
INSERT INTO `definition_navigator_public_rooms` (`order_num`, `image_size`, `image`, `image_type`, `room_ref`, `category_ref`, `club_only`, `visible`, `nav_index`)
VALUES (35, 'wide', 'officialrooms_hq/hh_room_tv_studio_animax.png', 'external', @room, @sponsored, 0, '1', 0);

INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'Antena3', 'Interact with Antena3!', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'tv_studio';
SET @room = LAST_INSERT_ID();
INSERT INTO `definition_public_room_libraries` (`heightmap_ref`, `room_ref`, `library_name`, `nav_code`)
SELECT `id`, @room, 'hh_room_tv_studio_antena3', 'tv_studio_antena3' FROM `definition_room_heightmap` WHERE `label` = 'tv_studio';
INSERT INTO `definition_navigator_public_rooms` (`order_num`, `image_size`, `image`, `image_type`, `room_ref`, `category_ref`, `club_only`, `visible`, `nav_index`)
VALUES (36, 'wide', 'officialrooms_hq/hh_room_tv_studio_antena3.png', 'external', @room, @sponsored, 0, '1', 0);

INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'M6 Studio', 'On air with M6.', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'tv_studio';
SET @room = LAST_INSERT_ID();
INSERT INTO `definition_public_room_libraries` (`heightmap_ref`, `room_ref`, `library_name`, `nav_code`)
SELECT `id`, @room, 'hh_room_tv_studio_m62', 'tv_studio_m62' FROM `definition_room_heightmap` WHERE `label` = 'tv_studio';
INSERT INTO `definition_navigator_public_rooms` (`order_num`, `image_size`, `image`, `image_type`, `room_ref`, `category_ref`, `club_only`, `visible`, `nav_index`)
VALUES (37, 'wide', 'officialrooms_hq/hh_room_tv_studio_m62.png', 'external', @room, @sponsored, 0, '1', 0);

INSERT INTO `rooms` (`label`, `description`, `category_ref`, `heightmap_ref`, `owner_ref`, `access_type`, `max_users`, `score`, `wallpaper`, `floor`, `landscape`)
SELECT 'MTV VIP Lounge', 'Join thousands of fans in the MTV VIP Lounge!', 10, `id`, NULL, 0, 50, 0, '201', '111', '' FROM `definition_room_heightmap` WHERE `label` = 'tv_studio';
SET @room = LAST_INSERT_ID();
INSERT INTO `definition_public_room_libraries` (`heightmap_ref`, `room_ref`, `library_name`, `nav_code`)
SELECT `id`, @room, 'hh_room_tv_studio_mtv', 'tv_studio_mtv' FROM `definition_room_heightmap` WHERE `label` = 'tv_studio';
INSERT INTO `definition_navigator_public_rooms` (`order_num`, `image_size`, `image`, `image_type`, `room_ref`, `category_ref`, `club_only`, `visible`, `nav_index`)
VALUES (38, 'wide', 'officialrooms_hq/hh_room_tv_studio_mtv.png', 'external', @room, @sponsored, 0, '1', 0);
