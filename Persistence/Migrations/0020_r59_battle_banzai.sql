ALTER TABLE `definition_furniture`
  MODIFY COLUMN `interaction_logic` enum('none','toggle','teleport','vending','gate','roller','petfood','petnest','petwater','pettoy','pettreat','totem_leg','totem_head','totem_planet','hockey_light','sound_disk','football','football_goal','football_countdown','football_score_counter','clothing_gate','toggle_multiheight','pool_inflatable','pressure_plate','banzai_tile','banzai_gate','banzai_score','banzai_countdown','banzai_puck') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'none';

UPDATE `definition_furniture` SET `interaction_logic` = 'banzai_tile' WHERE `classname` = 'bb_patch1';
UPDATE `definition_furniture` SET `interaction_logic` = 'banzai_gate' WHERE `classname` IN ('bb_gate_r', 'bb_gate_g', 'bb_gate_b', 'bb_gate_y');
UPDATE `definition_furniture` SET `interaction_logic` = 'banzai_score' WHERE `classname` IN ('bb_score_r', 'bb_score_g', 'bb_score_b', 'bb_score_y');
UPDATE `definition_furniture` SET `interaction_logic` = 'banzai_countdown' WHERE `classname` = 'bb_counter';
UPDATE `definition_furniture` SET `interaction_logic` = 'banzai_puck' WHERE `classname` = 'bb_puck';

INSERT INTO `definition_catalog_page_offers` (`ref_page`, `offer_code`, `price_credits`, `price_activity_points`, `club_only`, `giftable`)
  VALUES ((SELECT `id` FROM `definition_catalog_page_header` WHERE JSON_UNQUOTE(JSON_EXTRACT(`display_name`, '$.en')) = 'Battle Banzai' LIMIT 1), 'bb_patch1', 9, 0, 0, 1);
INSERT INTO `definition_catalog_page_offers_products` (`ref_offer_id`, `ref_furni_def`, `product_type`, `product_extra_data`, `product_count`, `product_rental_duration`)
  VALUES (LAST_INSERT_ID(), (SELECT `id` FROM `definition_furniture` WHERE `classname` = 'bb_patch1' AND `type` = 's' LIMIT 1), 's', NULL, 20, -1);
