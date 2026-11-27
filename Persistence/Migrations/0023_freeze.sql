ALTER TABLE `definition_furniture`
  MODIFY COLUMN `interaction_logic` enum('none','toggle','teleport','vending','gate','roller','petfood','petnest','petwater','pettoy','pettreat','totem_leg','totem_head','totem_planet','hockey_light','sound_disk','football','football_goal','football_countdown','football_score_counter','clothing_gate','toggle_multiheight','pool_inflatable','pressure_plate','banzai_tile','banzai_gate','banzai_score','banzai_countdown','banzai_puck','banzai_teleport','banzai_apparatus','banzai_pyramid','firework','icetag_tag_pole','icetag_skating_patch','lever_switch','freeze_tile','freeze_block','freeze_gate','freeze_counter','freeze_exit','freeze_score') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'none';

UPDATE `definition_furniture` f
JOIN (
  SELECT 'es_tile' AS classname, 'freeze_tile' AS logic UNION ALL
  SELECT 'es_box', 'freeze_block' UNION ALL
  SELECT 'es_gate_r', 'freeze_gate' UNION ALL
  SELECT 'es_gate_g', 'freeze_gate' UNION ALL
  SELECT 'es_gate_b', 'freeze_gate' UNION ALL
  SELECT 'es_gate_y', 'freeze_gate' UNION ALL
  SELECT 'es_counter', 'freeze_counter' UNION ALL
  SELECT 'es_exit', 'freeze_exit' UNION ALL
  SELECT 'es_score_r', 'freeze_score' UNION ALL
  SELECT 'es_score_g', 'freeze_score' UNION ALL
  SELECT 'es_score_b', 'freeze_score' UNION ALL
  SELECT 'es_score_y', 'freeze_score' UNION ALL
  SELECT 'es_geysir', 'banzai_pyramid'
) t ON t.classname = f.classname
SET f.`interaction_logic` = t.logic;

UPDATE `definition_furniture` SET `states` = '[0, 1]' WHERE `classname` = 'bb_pyramid' AND `states` = CAST('[0, 1, 100, 101]' AS JSON);

INSERT INTO `definition_achievements` (`code_base`, `client_type`, `max_level`, `enabled`)
SELECT n.`code_base`, n.`client_type`, 20, 1 FROM (VALUES
    ROW('ACH_EsA', 1010),
    ROW('ACH_FreezePlayer', 1011),
    ROW('ACH_FreezeWinner', 1012),
    ROW('ACH_FreezePowerUp', 1013)
  ) n (`code_base`, `client_type`)
WHERE NOT EXISTS (SELECT 1 FROM `definition_achievements` a WHERE a.`code_base` = n.`code_base`);

INSERT IGNORE INTO `definition_achievement_levels` (`achievement_ref`, `level`, `threshold`, `badge_code`, `reward_activity_points`, `reward_activity_point_type`, `reward_achievement_score`)
SELECT a.`id`, l.`level`, CASE a.`code_base`
    WHEN 'ACH_EsA' THEN l.`fighter`
    WHEN 'ACH_FreezePowerUp' THEN l.`power_ups`
    ELSE l.`points` END,
  CONCAT(a.`code_base`, l.`level`), l.`pixels`, 0, 10
FROM (VALUES
    ROW(1, 2, 50, 100, 10),
    ROW(2, 5, 125, 300, 15),
    ROW(3, 10, 240, 600, 20),
    ROW(4, 18, 410, 1000, 25),
    ROW(5, 30, 655, 1500, 30),
    ROW(6, 50, 1045, 2100, 40),
    ROW(7, 80, 1615, 2900, 50),
    ROW(8, 125, 2465, 3900, 60),
    ROW(9, 200, 3765, 5200, 70),
    ROW(10, 300, 6215, 6800, 80),
    ROW(11, 420, 10865, 8800, 100),
    ROW(12, 600, 19665, 11200, 120),
    ROW(13, 900, 36365, 14100, 140),
    ROW(14, 1500, 68115, 17600, 160),
    ROW(15, 2500, 128415, 22100, 180),
    ROW(16, 3700, 242965, 28100, 200),
    ROW(17, 5400, 460615, 36100, 250),
    ROW(18, 8000, 874115, 48100, 300),
    ROW(19, 12000, 1659765, 66100, 400),
    ROW(20, 20000, 3132515, 96100, 500)
  ) l (`level`, `fighter`, `points`, `power_ups`, `pixels`)
JOIN `definition_achievements` a ON a.`code_base` IN ('ACH_EsA', 'ACH_FreezePlayer', 'ACH_FreezeWinner', 'ACH_FreezePowerUp');
