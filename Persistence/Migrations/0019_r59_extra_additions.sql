ALTER TABLE `definition_furniture`
  MODIFY COLUMN `interaction_logic` enum('none','toggle','teleport','vending','gate','roller','petfood','petnest','petwater','pettoy','pettreat','totem_leg','totem_head','totem_planet','hockey_light','sound_disk','football','football_goal','football_countdown','football_score_counter','clothing_gate','toggle_multiheight','pool_inflatable','pressure_plate') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'none';

UPDATE `definition_furniture` SET `interaction_logic` = 'pool_inflatable' WHERE `classname` IN ('bw_boat', 'bw_boat_g', 'bw_boat_p', 'bw_chair', 'bw_chair_g', 'bw_chair_p', 'bw_mttrss', 'bw_mttrss_g', 'bw_mttrss_p', 'bw_fin', 'bw_fnc', 'bw_fnc_g', 'bw_fnc_p', 'bw_fnc_crnr', 'bw_fnc_crnr_g', 'bw_fnc_crnr_p', 'bw_sboard', 'bw_sboard_g', 'bw_sboard_p', 'bw_sofa', 'bw_sofa_g', 'bw_sofa_p', 'bw_table', 'bw_table_g', 'bw_table_p', 'bw_croc');
UPDATE `definition_furniture` SET `interaction_logic` = 'pressure_plate' WHERE `classname` IN ('bling_shwr', 'bw_shower');

UPDATE `definition_furniture` SET `interaction_logic` = 'football' WHERE `classname` = 'bw_bball';
UPDATE `definition_furniture` SET `interaction_logic` = 'teleport' WHERE `classname` = 'bw_tele';
UPDATE `definition_furniture` SET `interaction_logic` = 'teleport' WHERE `classname` = 'hween10_tele';

UPDATE `definition_furniture` SET `interaction_ids` = JSON_ARRAY(30) WHERE `classname` = 'bw_water_1';
UPDATE `definition_furniture` SET `interaction_ids` = JSON_ARRAY(28) WHERE `classname` = 'bw_water_2';
UPDATE `definition_furniture` SET `interaction_ids` = JSON_ARRAY(37) WHERE `classname` = 'hween10_pond';

INSERT INTO `definition_catalog_page_offers` (`ref_page`, `offer_code`, `price_credits`, `price_activity_points`, `club_only`, `giftable`)
  VALUES ((SELECT `id` FROM `definition_catalog_page_header` WHERE JSON_UNQUOTE(JSON_EXTRACT(`display_name`, '$.en')) = 'Special Effects' LIMIT 1), 'avatar_effect31', 0, 80, 0, 1);
INSERT INTO `definition_catalog_page_offers_products` (`ref_offer_id`, `ref_furni_def`, `product_type`, `product_extra_data`, `product_count`, `product_rental_duration`)
  VALUES (LAST_INSERT_ID(), NULL, 'e', '31', 1, 86400);
INSERT INTO `definition_catalog_page_offers` (`ref_page`, `offer_code`, `price_credits`, `price_activity_points`, `club_only`, `giftable`)
  VALUES ((SELECT `id` FROM `definition_catalog_page_header` WHERE JSON_UNQUOTE(JSON_EXTRACT(`display_name`, '$.en')) = 'Special Effects' LIMIT 1), 'avatar_effect32', 0, 80, 0, 1);
INSERT INTO `definition_catalog_page_offers_products` (`ref_offer_id`, `ref_furni_def`, `product_type`, `product_extra_data`, `product_count`, `product_rental_duration`)
  VALUES (LAST_INSERT_ID(), NULL, 'e', '32', 1, 86400);
INSERT INTO `definition_catalog_page_offers` (`ref_page`, `offer_code`, `price_credits`, `price_activity_points`, `club_only`, `giftable`)
  VALUES ((SELECT `id` FROM `definition_catalog_page_header` WHERE JSON_UNQUOTE(JSON_EXTRACT(`display_name`, '$.en')) = 'Special Effects' LIMIT 1), 'avatar_effect44', 0, 80, 0, 1);
INSERT INTO `definition_catalog_page_offers_products` (`ref_offer_id`, `ref_furni_def`, `product_type`, `product_extra_data`, `product_count`, `product_rental_duration`)
  VALUES (LAST_INSERT_ID(), NULL, 'e', '44', 1, 86400);
INSERT INTO `definition_catalog_page_offers` (`ref_page`, `offer_code`, `price_credits`, `price_activity_points`, `club_only`, `giftable`)
  VALUES ((SELECT `id` FROM `definition_catalog_page_header` WHERE JSON_UNQUOTE(JSON_EXTRACT(`display_name`, '$.en')) = 'Special Effects' LIMIT 1), 'avatar_effect47', 0, 80, 0, 1);
INSERT INTO `definition_catalog_page_offers_products` (`ref_offer_id`, `ref_furni_def`, `product_type`, `product_extra_data`, `product_count`, `product_rental_duration`)
  VALUES (LAST_INSERT_ID(), NULL, 'e', '47', 1, 86400);

INSERT INTO `definition_catalog_page_offers` (`ref_page`, `offer_code`, `price_credits`, `price_activity_points`, `club_only`, `giftable`)
  VALUES ((SELECT `id` FROM `definition_catalog_page_header` WHERE JSON_UNQUOTE(JSON_EXTRACT(`display_name`, '$.en')) = 'Summer' LIMIT 1), 'bw_water_1', 5, 0, 0, 1);
INSERT INTO `definition_catalog_page_offers_products` (`ref_offer_id`, `ref_furni_def`, `product_type`, `product_extra_data`, `product_count`, `product_rental_duration`)
  VALUES (LAST_INSERT_ID(), (SELECT `id` FROM `definition_furniture` WHERE `classname` = 'bw_water_1' AND `type` = 's' LIMIT 1), 's', NULL, 10, -1);
INSERT INTO `definition_catalog_page_offers` (`ref_page`, `offer_code`, `price_credits`, `price_activity_points`, `club_only`, `giftable`)
  VALUES ((SELECT `id` FROM `definition_catalog_page_header` WHERE JSON_UNQUOTE(JSON_EXTRACT(`display_name`, '$.en')) = 'Summer' LIMIT 1), 'bw_water_2', 5, 0, 0, 1);
INSERT INTO `definition_catalog_page_offers_products` (`ref_offer_id`, `ref_furni_def`, `product_type`, `product_extra_data`, `product_count`, `product_rental_duration`)
  VALUES (LAST_INSERT_ID(), (SELECT `id` FROM `definition_furniture` WHERE `classname` = 'bw_water_2' AND `type` = 's' LIMIT 1), 's', NULL, 10, -1);

INSERT INTO `definition_catalog_page_offers` (`ref_page`, `offer_code`, `price_credits`, `price_activity_points`, `club_only`, `giftable`)
  VALUES ((SELECT `id` FROM `definition_catalog_page_header` WHERE JSON_UNQUOTE(JSON_EXTRACT(`display_name`, '$.en')) = 'Voodoo' LIMIT 1), 'hween10_pond', 5, 0, 0, 1);
INSERT INTO `definition_catalog_page_offers_products` (`ref_offer_id`, `ref_furni_def`, `product_type`, `product_extra_data`, `product_count`, `product_rental_duration`)
  VALUES (LAST_INSERT_ID(), (SELECT `id` FROM `definition_furniture` WHERE `classname` = 'hween10_pond' AND `type` = 's' LIMIT 1), 's', NULL, 10, -1);
INSERT INTO `definition_catalog_page_offers` (`ref_page`, `offer_code`, `price_credits`, `price_activity_points`, `club_only`, `giftable`)
  VALUES ((SELECT `id` FROM `definition_catalog_page_header` WHERE JSON_UNQUOTE(JSON_EXTRACT(`display_name`, '$.en')) = 'Voodoo' LIMIT 1), 'hween10_swamp', 5, 0, 0, 1);
INSERT INTO `definition_catalog_page_offers_products` (`ref_offer_id`, `ref_furni_def`, `product_type`, `product_extra_data`, `product_count`, `product_rental_duration`)
  VALUES (LAST_INSERT_ID(), (SELECT `id` FROM `definition_furniture` WHERE `classname` = 'hween10_swamp' AND `type` = 's' LIMIT 1), 's', NULL, 10, -1);

INSERT INTO `definition_catalog_page_offers` (`ref_page`, `offer_code`, `price_credits`, `price_activity_points`, `club_only`, `giftable`)
  VALUES ((SELECT `id` FROM `definition_catalog_page_header` WHERE JSON_UNQUOTE(JSON_EXTRACT(`display_name`, '$.en')) = 'Tiki' LIMIT 1), 'Tikitoa_sandmattedeal', 3, 0, 0, 1);
INSERT INTO `definition_catalog_page_offers_products` (`ref_offer_id`, `ref_furni_def`, `product_type`, `product_extra_data`, `product_count`, `product_rental_duration`)
  VALUES (LAST_INSERT_ID(), (SELECT `id` FROM `definition_furniture` WHERE `classname` = 'tiki_sand' AND `type` = 's' LIMIT 1), 's', NULL, 5, -1);
INSERT INTO `definition_catalog_page_offers` (`ref_page`, `offer_code`, `price_credits`, `price_activity_points`, `club_only`, `giftable`)
  VALUES ((SELECT `id` FROM `definition_catalog_page_header` WHERE JSON_UNQUOTE(JSON_EXTRACT(`display_name`, '$.en')) = 'Tiki' LIMIT 1), 'deal_tikisand', 6, 0, 0, 1);
INSERT INTO `definition_catalog_page_offers_products` (`ref_offer_id`, `ref_furni_def`, `product_type`, `product_extra_data`, `product_count`, `product_rental_duration`)
  VALUES (LAST_INSERT_ID(), (SELECT `id` FROM `definition_furniture` WHERE `classname` = 'tiki_sand' AND `type` = 's' LIMIT 1), 's', NULL, 12, -1);

INSERT INTO `definition_figure_settypes` (`type`, `palette_ref`, `mand_m_0`, `mand_f_0`, `mand_m_1`, `mand_f_1`) VALUES
  ('cp', 3, 0, 0, 0, 0)
ON DUPLICATE KEY UPDATE `palette_ref` = VALUES(`palette_ref`);

INSERT INTO `definition_figure_sets` (`id`, `settype_ref`, `gender`, `club`, `colorable`, `selectable`) VALUES
  (3001, 'ch', 'M', 2, 1, 1),
  (3002, 'cc', 'M', 2, 1, 1),
  (3003, 'cc', 'F', 2, 1, 1),
  (3004, 'hr', 'F', 2, 1, 1),
  (3005, 'ch', 'F', 2, 1, 1),
  (3006, 'lg', 'F', 2, 1, 1),
  (3007, 'cc', 'M', 2, 1, 1),
  (3008, 'cc', 'F', 2, 1, 1),
  (3009, 'cc', 'M', 2, 1, 1),
  (3010, 'cc', 'F', 2, 1, 1),
  (3011, 'hr', 'U', 2, 1, 1),
  (3012, 'hr', 'F', 2, 1, 1),
  (3013, 'ch', 'F', 2, 1, 1),
  (3014, 'ch', 'F', 2, 1, 1),
  (3015, 'ch', 'M', 2, 1, 1),
  (3016, 'sh', 'U', 2, 1, 1),
  (3017, 'lg', 'U', 2, 1, 1),
  (3018, 'lg', 'F', 2, 1, 1),
  (3019, 'lg', 'F', 2, 1, 1),
  (3020, 'hr', 'M', 2, 1, 1),
  (3021, 'hr', 'U', 2, 1, 1),
  (3022, 'ch', 'M', 2, 1, 1),
  (3023, 'lg', 'U', 2, 1, 1),
  (3024, 'hr', 'F', 2, 1, 1),
  (3025, 'hr', 'M', 2, 1, 1),
  (3026, 'ha', 'U', 2, 1, 1),
  (3027, 'sh', 'U', 2, 1, 1),
  (3030, 'ch', 'M', 0, 1, 1),
  (3032, 'ch', 'M', 2, 1, 1),
  (3033, 'ch', 'F', 2, 1, 1),
  (3035, 'sh', 'U', 2, 1, 1),
  (3036, 'ch', 'F', 2, 1, 1),
  (3037, 'hr', 'F', 2, 1, 1),
  (3038, 'ch', 'M', 2, 1, 1),
  (3039, 'cc', 'M', 2, 1, 1),
  (3040, 'hr', 'F', 2, 1, 1),
  (3041, 'hr', 'U', 2, 1, 1),
  (3043, 'hr', 'U', 2, 1, 1),
  (3044, 'hr', 'F', 2, 1, 1),
  (3045, 'ch', 'F', 2, 1, 1),
  (3046, 'ch', 'F', 2, 1, 1),
  (3047, 'lg', 'F', 2, 1, 1),
  (3048, 'hr', 'U', 2, 1, 1),
  (3049, 'ch', 'F', 2, 1, 1),
  (3050, 'ch', 'M', 2, 1, 1),
  (3051, 'ch', 'F', 2, 1, 1),
  (3054, 'ha', 'U', 2, 1, 1),
  (3056, 'hr', 'M', 2, 1, 1),
  (3057, 'lg', 'U', 2, 1, 1),
  (3058, 'lg', 'U', 2, 1, 1),
  (3059, 'ch', 'M', 2, 1, 1),
  (3060, 'ch', 'F', 2, 1, 1),
  (3061, 'lg', 'F', 2, 1, 1),
  (3064, 'sh', 'F', 2, 1, 1),
  (3066, 'cc', 'F', 2, 1, 1),
  (3067, 'ch', 'F', 2, 1, 1),
  (3068, 'sh', 'U', 2, 1, 1),
  (3069, 'he', 'U', 2, 1, 1),
  (3070, 'he', 'U', 2, 1, 1),
  (3071, 'he', 'U', 2, 1, 1),
  (3072, 'wa', 'U', 2, 1, 1),
  (3073, 'wa', 'U', 2, 1, 1),
  (3074, 'wa', 'U', 2, 1, 1),
  (3075, 'cc', 'M', 2, 1, 1),
  (3076, 'ch', 'F', 2, 1, 1),
  (3077, 'ch', 'M', 2, 1, 1),
  (3078, 'lg', 'U', 2, 1, 1),
  (3079, 'he', 'U', 2, 1, 1),
  (3080, 'wa', 'U', 2, 1, 1),
  (3081, 'he', 'U', 2, 1, 1),
  (3082, 'he', 'U', 2, 1, 1),
  (3083, 'ea', 'U', 2, 1, 1),
  (3084, 'ca', 'U', 2, 1, 1),
  (3085, 'ca', 'U', 2, 1, 1),
  (3086, 'ha', 'U', 2, 1, 1),
  (3087, 'cc', 'M', 1, 1, 1),
  (3088, 'lg', 'U', 1, 1, 1),
  (3089, 'sh', 'U', 1, 1, 1),
  (3090, 'hr', 'U', 1, 1, 1),
  (3107, 'ea', 'U', 2, 1, 1),
  (3108, 'ea', 'U', 2, 1, 1),
  (3109, 'ch', 'M', 0, 1, 1),
  (3110, 'ch', 'M', 0, 1, 1),
  (3111, 'ch', 'M', 0, 1, 1),
  (3112, 'ch', 'F', 0, 1, 1),
  (3113, 'ch', 'F', 0, 1, 1),
  (3114, 'ch', 'F', 0, 1, 1),
  (3115, 'sh', 'U', 0, 1, 1),
  (3116, 'lg', 'U', 0, 1, 1),
  (3117, 'ha', 'U', 2, 1, 1),
  (3118, 'ha', 'U', 2, 1, 1),
  (3119, 'cp', 'U', 0, 1, 1),
  (3120, 'cp', 'U', 0, 1, 1),
  (3121, 'cp', 'U', 0, 1, 1),
  (3122, 'cp', 'U', 0, 1, 1),
  (3123, 'cp', 'U', 0, 1, 1),
  (3124, 'cp', 'U', 0, 1, 1),
  (3125, 'cp', 'U', 0, 1, 1),
  (3126, 'cp', 'U', 0, 1, 1),
  (3127, 'cp', 'U', 0, 1, 1),
  (3128, 'cp', 'U', 0, 1, 1),
  (3129, 'ha', 'U', 2, 1, 1),
  (3130, 'ha', 'U', 2, 1, 1),
  (3131, 'ca', 'U', 2, 1, 1),
  (3133, 'ch', 'F', 2, 1, 1),
  (3134, 'lg', 'F', 2, 1, 1),
  (3135, 'ch', 'F', 1, 1, 1),
  (3136, 'lg', 'U', 1, 1, 1),
  (3137, 'ch', 'F', 2, 1, 1),
  (3138, 'lg', 'U', 2, 1, 1)
ON DUPLICATE KEY UPDATE `settype_ref` = VALUES(`settype_ref`), `gender` = VALUES(`gender`), `club` = VALUES(`club`), `colorable` = VALUES(`colorable`), `selectable` = VALUES(`selectable`);
