ALTER TABLE `web_settings`
  ADD COLUMN `cloudflare_turnstile_site_key` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  ADD COLUMN `cloudflare_turnstile_secret_key` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL;

ALTER TABLE `web_settings`
  ADD COLUMN `maintenance_status` enum('0','1') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '0';

ALTER TABLE `global_game_settings`
  ADD COLUMN `idle_disconnect_time` int unsigned NOT NULL DEFAULT '30' COMMENT 'in minutes, 0 means it wont disconnect';

ALTER TABLE `web_settings`
  ADD COLUMN `smtp_host` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  ADD COLUMN `smtp_port` smallint unsigned NOT NULL DEFAULT '587',
  ADD COLUMN `smtp_username` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  ADD COLUMN `smtp_password` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  ADD COLUMN `mail_from` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'e.g. Dragon Hotel <no-reply@yourdomain.com>';

CREATE TABLE `web_password_reset_tokens` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `ref_user` int unsigned NOT NULL,
  `token_hash` char(64) COLLATE utf8mb4_unicode_ci NOT NULL,
  `ip_address` varchar(45) COLLATE utf8mb4_unicode_ci NOT NULL,
  `created_at` datetime NOT NULL DEFAULT (utc_timestamp()),
  `expires_at` datetime NOT NULL,
  `used_at` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_web_password_reset_tokens_hash` (`token_hash`),
  KEY `fk_web_password_reset_tokens_user` (`ref_user`),
  CONSTRAINT `fk_web_password_reset_tokens_user` FOREIGN KEY (`ref_user`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `web_email_verification_tokens` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `ref_user` int unsigned NOT NULL,
  `email` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `token_hash` char(64) COLLATE utf8mb4_unicode_ci NOT NULL,
  `ip_address` varchar(45) COLLATE utf8mb4_unicode_ci NOT NULL,
  `created_at` datetime NOT NULL DEFAULT (utc_timestamp()),
  `expires_at` datetime NOT NULL,
  `used_at` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_web_email_verification_tokens_hash` (`token_hash`),
  KEY `fk_web_email_verification_tokens_user` (`ref_user`),
  CONSTRAINT `fk_web_email_verification_tokens_user` FOREIGN KEY (`ref_user`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `web_user_logins` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `ref_user` int unsigned NOT NULL,
  `ip_address` varchar(45) COLLATE utf8mb4_unicode_ci NOT NULL,
  `created_at` datetime NOT NULL DEFAULT (utc_timestamp()),
  PRIMARY KEY (`id`),
  KEY `idx_web_user_logins_user_created` (`ref_user`,`created_at`),
  KEY `idx_web_user_logins_ip` (`ip_address`),
  CONSTRAINT `fk_web_user_logins_user` FOREIGN KEY (`ref_user`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

ALTER TABLE `users`
  ADD COLUMN `web_session_stamp` varchar(32) COLLATE utf8mb4_unicode_ci DEFAULT NULL;

CREATE TABLE `web_emulator_commands` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `ref_user` int unsigned NOT NULL,
  `command` enum('disconnect','email_verified') COLLATE utf8mb4_unicode_ci NOT NULL,
  `created_at` datetime NOT NULL DEFAULT (utc_timestamp()),
  PRIMARY KEY (`id`),
  KEY `fk_web_emulator_commands_user` (`ref_user`),
  CONSTRAINT `fk_web_emulator_commands_user` FOREIGN KEY (`ref_user`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
