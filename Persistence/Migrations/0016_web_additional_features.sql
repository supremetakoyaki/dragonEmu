INSERT INTO `definition_fuse` (`label`, `requirement`) VALUES
  ('fuse_housekeeping_catalog', 'rank>=6')
ON DUPLICATE KEY UPDATE `label` = `label`;

ALTER TABLE `web_emulator_commands`
  MODIFY COLUMN `command` enum('disconnect','email_verified','ban','word_filter','catalog') COLLATE utf8mb4_unicode_ci NOT NULL;

ALTER TABLE `users`
  ADD COLUMN `country` varchar(2) COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'ISO 3166-1 alpha-2 - NULL: not said';

UPDATE `web_page_rules` r JOIN (VALUES
    ROW('Treat everyone with respect!', 'gi_bullying.gif'),
    ROW('Play fair, play nice!', 'gi_pi_disclosed.gif'),
    ROW('Keep it SFW!', 'gi_blackmail.gif'),
    ROW('Privacy is key!', 'gi_webcam.gif')) AS pictures (title, file)
  ON r.`title` ->> '$.en' = pictures.title
SET r.`image_url` = CONCAT('%web_url%/images/rules/', pictures.file)
WHERE r.`image_url` IS NULL OR r.`image_url` = '';
