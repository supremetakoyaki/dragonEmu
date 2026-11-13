ALTER TABLE `definition_furniture`
  MODIFY COLUMN `interaction_logic` enum('none','toggle','teleport','vending','gate','roller','petfood','petnest','petwater','pettoy','pettreat','totem_leg','totem_head','totem_planet','hockey_light','sound_disk','football','football_goal','football_countdown','football_score_counter','clothing_gate','toggle_multiheight') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'none';

UPDATE `definition_furniture` SET `interaction_logic` = 'football' WHERE `classname` IN ('fball_ball', 'fball_ball2', 'fball_ball3', 'fball_ball4', 'fball_ball5');
UPDATE `definition_furniture` SET `interaction_logic` = 'football_goal' WHERE `classname` IN ('fball_goal_b', 'fball_goal_r', 'fball_goal_y', 'fball_goal_g');
UPDATE `definition_furniture` SET `interaction_logic` = 'football_score_counter' WHERE `classname` IN ('fball_score_r', 'fball_score_g', 'fball_score_y', 'fball_score_b');
UPDATE `definition_furniture` SET `interaction_logic` = 'football_countdown' WHERE `classname` = 'fball_counter';
UPDATE `definition_furniture` SET `interaction_logic` = 'clothing_gate' WHERE `classname` = 'fball_gate';
UPDATE `definition_furniture` SET `interaction_logic` = 'toggle_multiheight', `interaction_ids` = JSON_ARRAY(1.4, 2.04, 2.68) WHERE `classname` = 'fball_audbench';
