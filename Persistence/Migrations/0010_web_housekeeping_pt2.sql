INSERT INTO `definition_fuse` (`label`, `requirement`) VALUES
  ('fuse_housekeeping_access', 'rank>=3'),
  ('fuse_housekeeping_create_articles', 'rank>=3'),
  ('fuse_housekeeping_edit_any_article', 'rank>=5'),
  ('fuse_housekeeping_manage_article_categories', 'rank>=6'),
  ('fuse_housekeeping_create_promos', 'rank>=3'),
  ('fuse_housekeeping_inbox', 'rank>=4'),
  ('fuse_housekeeping_reset_passwords', 'rank>=5'),
  ('fuse_housekeeping_web_settings', 'rank>=7'),
  ('fuse_housekeeping_word_filter', 'rank>=6'),
  ('fuse_client_debug', 'rank>=7')
ON DUPLICATE KEY UPDATE `label` = `label`;

ALTER TABLE `web_settings`
  ADD COLUMN `hk_logo_url` varchar(512) COLLATE utf8mb4_unicode_ci DEFAULT NULL;

ALTER TABLE `web_articles`
  ADD COLUMN `updated_at` datetime DEFAULT NULL,
  ADD COLUMN `updated_by_ref` int unsigned DEFAULT NULL,
  ADD CONSTRAINT `fk_web_articles_updated_by` FOREIGN KEY (`updated_by_ref`) REFERENCES `users` (`id`) ON DELETE SET NULL;

ALTER TABLE `web_promos`
  ADD COLUMN `created_by_ref` int unsigned DEFAULT NULL,
  ADD CONSTRAINT `fk_web_promos_created_by` FOREIGN KEY (`created_by_ref`) REFERENCES `users` (`id`) ON DELETE SET NULL;

ALTER TABLE `web_hot_campaigns`
  ADD COLUMN `created_by_ref` int unsigned DEFAULT NULL,
  ADD CONSTRAINT `fk_web_hot_campaigns_created_by` FOREIGN KEY (`created_by_ref`) REFERENCES `users` (`id`) ON DELETE SET NULL;

ALTER TABLE `users_bans`
  ADD COLUMN `lifted_at` datetime DEFAULT NULL,
  ADD COLUMN `lifted_by_ref` int unsigned DEFAULT NULL,
  ADD CONSTRAINT `fk_users_bans_lifted_by` FOREIGN KEY (`lifted_by_ref`) REFERENCES `users` (`id`) ON DELETE SET NULL;

ALTER TABLE `web_emulator_commands`
  MODIFY COLUMN `command` enum('disconnect','email_verified','ban','word_filter') COLLATE utf8mb4_unicode_ci NOT NULL;

CREATE TABLE `web_housekeeping_log` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `created_at` datetime NOT NULL DEFAULT (utc_timestamp()),
  `ref_user` int unsigned DEFAULT NULL,
  `action` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `target_type` varchar(30) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `target_id` bigint unsigned DEFAULT NULL,
  `details` varchar(1000) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '',
  `ip_address` varchar(45) COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_web_housekeeping_log_created` (`created_at`),
  KEY `idx_web_housekeeping_log_target` (`target_type`,`target_id`),
  KEY `fk_web_housekeeping_log_user` (`ref_user`),
  CONSTRAINT `fk_web_housekeeping_log_user` FOREIGN KEY (`ref_user`) REFERENCES `users` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
