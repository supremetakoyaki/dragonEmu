UPDATE `definition_catalog_page_header` SET `display_name` = JSON_SET(`display_name`, '$.es', 'Regalos HC', '$."pt-br"', 'Presentes do Clube') WHERE `id` = 72;
UPDATE `definition_catalog_page_header` SET `display_name` = JSON_SET(`display_name`, '$.es', 'Hazte Habbo Club') WHERE `id` = 7;

INSERT INTO `definition_achievements` (`code_base`, `max_level`, `enabled`) VALUES
  ('ACH_BattleBallTilesLocked', 20, 1),
  ('ACH_GamePlayerExperience', 20, 1),
  ('ACH_BattleBallWinner', 20, 1),
  ('ACH_BattleBallPlayer', 20, 1),
  ('ACH_FootballGoalScored', 5, 1),
  ('ACH_FootballGoalScoredInRoom', 5, 1);

INSERT INTO `definition_achievement_levels` (`achievement_ref`, `level`, `threshold`, `badge_code`, `reward_activity_points`)
SELECT a.`id`, l.`level`, l.`threshold`, CONCAT(a.`code_base`, l.`level`), l.`pixels`
FROM `definition_achievements` a JOIN (VALUES
    ROW(1, 25, 10),
    ROW(2, 65, 15),
    ROW(3, 125, 20),
    ROW(4, 205, 25),
    ROW(5, 335, 30),
    ROW(6, 525, 40),
    ROW(7, 805, 50),
    ROW(8, 1235, 60),
    ROW(9, 1875, 70),
    ROW(10, 2875, 80),
    ROW(11, 4375, 100),
    ROW(12, 6875, 120),
    ROW(13, 10775, 140),
    ROW(14, 17075, 160),
    ROW(15, 27175, 180),
    ROW(16, 43275, 200),
    ROW(17, 69075, 250),
    ROW(18, 110375, 300),
    ROW(19, 176375, 400),
    ROW(20, 282075, 500)) AS l (`level`, `threshold`, `pixels`)
WHERE a.`code_base` = 'ACH_BattleBallTilesLocked';

INSERT INTO `definition_achievement_levels` (`achievement_ref`, `level`, `threshold`, `badge_code`, `reward_activity_points`)
SELECT a.`id`, l.`level`, l.`threshold`, CONCAT(a.`code_base`, l.`level`), l.`pixels`
FROM `definition_achievements` a JOIN (VALUES
    ROW(1, 100, 10),
    ROW(2, 250, 15),
    ROW(3, 480, 20),
    ROW(4, 820, 25),
    ROW(5, 1330, 30),
    ROW(6, 2090, 40),
    ROW(7, 3230, 50),
    ROW(8, 4930, 60),
    ROW(9, 7530, 70),
    ROW(10, 12430, 80),
    ROW(11, 21730, 100),
    ROW(12, 39330, 120),
    ROW(13, 72730, 140),
    ROW(14, 136230, 160),
    ROW(15, 256830, 180),
    ROW(16, 485930, 200),
    ROW(17, 921230, 250),
    ROW(18, 1748230, 300),
    ROW(19, 3319530, 400),
    ROW(20, 6305030, 500)) AS l (`level`, `threshold`, `pixels`)
WHERE a.`code_base` = 'ACH_GamePlayerExperience';

INSERT INTO `definition_achievement_levels` (`achievement_ref`, `level`, `threshold`, `badge_code`, `reward_activity_points`)
SELECT a.`id`, l.`level`, l.`threshold`, CONCAT(a.`code_base`, l.`level`), l.`pixels`
FROM `definition_achievements` a JOIN (VALUES
    ROW(1, 50, 10),
    ROW(2, 105, 15),
    ROW(3, 240, 20),
    ROW(4, 410, 25),
    ROW(5, 655, 30),
    ROW(6, 1045, 40),
    ROW(7, 1615, 50),
    ROW(8, 2465, 60),
    ROW(9, 3765, 70),
    ROW(10, 6215, 80),
    ROW(11, 10865, 100),
    ROW(12, 19665, 120),
    ROW(13, 36365, 140),
    ROW(14, 68115, 160),
    ROW(15, 128415, 180),
    ROW(16, 242965, 200),
    ROW(17, 460615, 250),
    ROW(18, 874115, 300),
    ROW(19, 1659765, 400),
    ROW(20, 3152515, 500)) AS l (`level`, `threshold`, `pixels`)
WHERE a.`code_base` = 'ACH_BattleBallWinner';

INSERT INTO `definition_achievement_levels` (`achievement_ref`, `level`, `threshold`, `badge_code`, `reward_activity_points`)
SELECT a.`id`, l.`level`, l.`threshold`, CONCAT(a.`code_base`, l.`level`), l.`pixels`
FROM `definition_achievements` a JOIN (VALUES
    ROW(1, 50, 10),
    ROW(2, 105, 15),
    ROW(3, 240, 20),
    ROW(4, 410, 25),
    ROW(5, 655, 30),
    ROW(6, 1045, 40),
    ROW(7, 1615, 50),
    ROW(8, 2465, 60),
    ROW(9, 3765, 70),
    ROW(10, 6215, 80),
    ROW(11, 10865, 100),
    ROW(12, 19665, 120),
    ROW(13, 36365, 140),
    ROW(14, 68115, 160),
    ROW(15, 128415, 180),
    ROW(16, 242965, 200),
    ROW(17, 460615, 250),
    ROW(18, 874115, 300),
    ROW(19, 1659765, 400),
    ROW(20, 3152515, 500)) AS l (`level`, `threshold`, `pixels`)
WHERE a.`code_base` = 'ACH_BattleBallPlayer';

INSERT INTO `definition_achievement_levels` (`achievement_ref`, `level`, `threshold`, `badge_code`, `reward_activity_points`)
SELECT a.`id`, l.`level`, l.`threshold`, CONCAT(a.`code_base`, l.`level`), l.`pixels`
FROM `definition_achievements` a JOIN (VALUES
    ROW(1, 1, 10),
    ROW(2, 10, 25),
    ROW(3, 100, 50),
    ROW(4, 1000, 100),
    ROW(5, 10000, 200)) AS l (`level`, `threshold`, `pixels`)
WHERE a.`code_base` = 'ACH_FootballGoalScored';

INSERT INTO `definition_achievement_levels` (`achievement_ref`, `level`, `threshold`, `badge_code`, `reward_activity_points`)
SELECT a.`id`, l.`level`, l.`threshold`, CONCAT(a.`code_base`, l.`level`), l.`pixels`
FROM `definition_achievements` a JOIN (VALUES
    ROW(1, 1, 10),
    ROW(2, 20, 25),
    ROW(3, 400, 50),
    ROW(4, 8000, 100),
    ROW(5, 160000, 200)) AS l (`level`, `threshold`, `pixels`)
WHERE a.`code_base` = 'ACH_FootballGoalScoredInRoom';

ALTER TABLE `definition_furniture`
  MODIFY COLUMN `interaction_logic` enum('none','toggle','teleport','vending','gate','roller','petfood','petnest','petwater','pettoy','pettreat','totem_leg','totem_head','totem_planet','hockey_light','sound_disk','football','football_goal','football_countdown','football_score_counter','clothing_gate','toggle_multiheight','pool_inflatable','pressure_plate','banzai_tile','banzai_gate','banzai_score','banzai_countdown','banzai_puck','banzai_teleport','banzai_apparatus','banzai_pyramid') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'none';

UPDATE `definition_furniture` SET `interaction_logic` = 'banzai_apparatus' WHERE `classname` = 'bb_apparatus';
UPDATE `definition_furniture` SET `interaction_logic` = 'banzai_pyramid' WHERE `classname` = 'bb_pyramid';

INSERT INTO `definition_pet_commands` (`id`, `name`, `required_level`, `experience`, `energy_cost`, `nutrition_cost`)
  VALUES (17, '{"en": "play football", "es": "fútbol", "pt-br": "jogar futebol"}', 17, 64, 40, 20);
