INSERT INTO `definition_catalog_page_header` (`id`, `parent_id`, `color`, `icon`, `display_name`, `coming_soon`, `enabled`, `min_rank`, `order_num`) VALUES
  (13, -1, 6, 0, 'Camera', 0, 1, 1, 10),
  (131, 13, 0, 502, 'Camera', 0, 1, 1, 1),
  (132, 13, 0, 42, 'Instructions', 0, 1, 1, 2);

ALTER TABLE `definition_furniture`
  MODIFY COLUMN `logic_class` enum('basic','camera','credit','dice','ecotron_box','habbowheel','hockey_score','hotspot','jukebox','multistate','one_way_door','photo','planet_system','present','randomstate','roomdimmer','sound_machine','stickie','trophy','voting_chair','window') COLLATE utf8mb4_unicode_ci NOT NULL;

UPDATE `definition_furniture` SET `logic_class` = 'camera', `allow_trade` = 1, `allow_recycle` = 0 WHERE `classname` = 'camera';
UPDATE `definition_furniture` SET `logic_class` = 'photo', `interaction_logic` = 'none', `allow_trade` = 1, `allow_recycle` = 0 WHERE `classname` = 'photo';

ALTER TABLE `definition_catalog_page_offers_products`
  MODIFY COLUMN `product_type` enum('i','s','e','h','f') COLLATE utf8mb4_unicode_ci NOT NULL;

CREATE TABLE `photos` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `item_ref` int unsigned NOT NULL,
  `user_ref` int unsigned NOT NULL,
  `room_ref` int unsigned NOT NULL,
  `caption` varchar(105) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '',
  `token` char(16) COLLATE utf8mb4_unicode_ci NOT NULL,
  `image` mediumblob NOT NULL,
  `taken_at` datetime NOT NULL,
  `removed_at` datetime DEFAULT NULL,
  `removed_by_ref` int unsigned DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_photos_item` (`item_ref`),
  KEY `idx_photos_taken` (`taken_at`),
  KEY `fk_photos_user` (`user_ref`),
  KEY `fk_photos_removed_by` (`removed_by_ref`),
  CONSTRAINT `fk_photos_item` FOREIGN KEY (`item_ref`) REFERENCES `users_items` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_photos_user` FOREIGN KEY (`user_ref`) REFERENCES `users` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_photos_removed_by` FOREIGN KEY (`removed_by_ref`) REFERENCES `users` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO `definition_fuse` (`label`, `requirement`) VALUES
  ('fuse_remove_photos', 'rank>=5')
ON DUPLICATE KEY UPDATE `label` = `label`;

INSERT INTO `definition_catalog_page_body` (`ref_page_id`, `layout_code`, `localization_images`, `localization_texts`) VALUES
  (131, 'camera1', JSON_ARRAY('catalog_camera_headline1', 'campic_cam', '', 'campic_film'), JSON_ARRAY(
    'Take picture and keep a record of those special moments forever. With your Habbo camera you can take pictures of just about anything in the Hotel - Even a celebrity sitting on the loo...\n\nThe Camera comes with 2 free photos.',
    'When you\'ve used your free photos, you\'ll need to buy more. Each roll of film takes five photos. Your Camera will show how much film you have left and loads the next roll automatically.')),
  (132, 'camera2', JSON_ARRAY('catalog_camera_headline1', 'campic_help'), JSON_ARRAY(
    'CAMERA FUNCTIONS\n\n1. Press this button to take a photo.\n2. Photo cancel - for when you\'ve chopped off your friend\'s head!\n3. Zoom in and out.\n4. Photo counter - shows how much film you have left.\n5. Caption box - write your caption before saving the photo.\n6. Save - this moves the photo to your inventory. You can give photos to your friends, or put them on the wall like posters.'));

DELETE FROM `definition_catalog_page_offers` WHERE `ref_page` = 92 AND `offer_code` = 'a2 kamera';
INSERT INTO `definition_catalog_page_offers` (`ref_page`, `offer_code`, `price_credits`, `price_activity_points`, `club_only`, `giftable`) VALUES
  (131, 'a2 kamera', 10, 0, 0, 0);
SET @camera_offer = LAST_INSERT_ID();
INSERT INTO `definition_catalog_page_offers` (`ref_page`, `offer_code`, `price_credits`, `price_activity_points`, `club_only`, `giftable`) VALUES
  (131, 'a2 film', 6, 0, 0, 1);
SET @film_offer = LAST_INSERT_ID();

INSERT INTO `definition_catalog_page_offers_products` (`ref_offer_id`, `ref_furni_def`, `product_type`, `product_extra_data`, `product_count`, `product_rental_duration`)
SELECT @camera_offer, `id`, 's', NULL, 1, -1 FROM `definition_furniture` WHERE `classname` = 'camera';
INSERT INTO `definition_catalog_page_offers_products` (`ref_offer_id`, `ref_furni_def`, `product_type`, `product_extra_data`, `product_count`, `product_rental_duration`) VALUES
  (@camera_offer, NULL, 'f', '2', 1, -1),
  (@film_offer, NULL, 'f', '5', 1, -1);

INSERT INTO `definition_achievements` (`code_base`, `max_level`, `enabled`) VALUES ('ACH_CameraPhotoCount', 10, 1);

INSERT INTO `definition_achievement_levels` (`achievement_ref`, `level`, `threshold`, `badge_code`, `reward_activity_points`)
SELECT a.`id`, v.level, v.threshold, CONCAT('ACH_CameraPhotoCount', v.level), v.pixels
FROM `definition_achievements` a
JOIN (
    SELECT 1 AS level, 1 AS threshold, 20 AS pixels
    UNION ALL SELECT 2 AS level, 5 AS threshold, 20 AS pixels
    UNION ALL SELECT 3 AS level, 15 AS threshold, 20 AS pixels
    UNION ALL SELECT 4 AS level, 30 AS threshold, 20 AS pixels
    UNION ALL SELECT 5 AS level, 50 AS threshold, 40 AS pixels
    UNION ALL SELECT 6 AS level, 100 AS threshold, 40 AS pixels
    UNION ALL SELECT 7 AS level, 200 AS threshold, 40 AS pixels
    UNION ALL SELECT 8 AS level, 400 AS threshold, 60 AS pixels
    UNION ALL SELECT 9 AS level, 800 AS threshold, 100 AS pixels
    UNION ALL SELECT 10 AS level, 1600 AS threshold, 200 AS pixels
) v
WHERE a.`code_base` = 'ACH_CameraPhotoCount';
