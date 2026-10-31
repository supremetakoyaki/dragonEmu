ALTER TABLE `definition_furniture`
  MODIFY COLUMN `logic_class` enum('basic','credit','dice','ecotron_box','habbowheel','hockey_score','hotspot','jukebox','multistate','one_way_door','planet_system','present','randomstate','roomdimmer','sound_machine','stickie','trophy','voting_chair','window') COLLATE utf8mb4_unicode_ci NOT NULL;

UPDATE `definition_furniture` SET `logic_class` = 'voting_chair' WHERE `classname` IN ('ads_idol_voting_ch', 'voting_ch');
UPDATE `definition_furniture` SET `logic_class` = 'hotspot' WHERE `classname` = 'ads_idol_hotspot';

UPDATE `definition_furniture` SET `is_walkable` = 1 WHERE `classname` = 'ads_idol_hotspot';
UPDATE `definition_furniture` SET `states` = '[-1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99]' WHERE `classname` = 'ads_idol_hotspot';

INSERT INTO `definition_achievements` (`code_base`, `max_level`, `enabled`) VALUES ('ACH_AIPerformanceVote', 10, 1);

INSERT INTO `definition_achievement_levels` (`achievement_ref`, `level`, `threshold`, `badge_code`, `reward_activity_points`)
SELECT a.`id`, v.level, v.threshold, CONCAT('ACH_AIPerformanceVote', v.level), v.pixels
FROM `definition_achievements` a
JOIN (
    SELECT 1 AS level, 1 AS threshold, 20 AS pixels
    UNION ALL SELECT 2 AS level, 21 AS threshold, 20 AS pixels
    UNION ALL SELECT 3 AS level, 71 AS threshold, 20 AS pixels
    UNION ALL SELECT 4 AS level, 171 AS threshold, 20 AS pixels
    UNION ALL SELECT 5 AS level, 351 AS threshold, 40 AS pixels
    UNION ALL SELECT 6 AS level, 551 AS threshold, 40 AS pixels
    UNION ALL SELECT 7 AS level, 751 AS threshold, 40 AS pixels
    UNION ALL SELECT 8 AS level, 1051 AS threshold, 60 AS pixels
    UNION ALL SELECT 9 AS level, 1351 AS threshold, 100 AS pixels
    UNION ALL SELECT 10 AS level, 1651 AS threshold, 200 AS pixels
) v
WHERE a.`code_base` = 'ACH_AIPerformanceVote';
