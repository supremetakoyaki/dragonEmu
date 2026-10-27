ALTER TABLE `web_settings`
  ADD COLUMN `web_url` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'http://localhost',
  ADD COLUMN `assets_url` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'http://localhost/game_assets',
  ADD COLUMN `credits_at_register` int unsigned NOT NULL DEFAULT '100',
  ADD COLUMN `pixels_at_register` int unsigned NOT NULL DEFAULT '25';

CREATE TABLE `web_page_rules` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `title` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `html_content` text COLLATE utf8mb4_unicode_ci NOT NULL,
  `image_url` varchar(512) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `web_page_ranks` (
  `rank_id` int NOT NULL,
  `caption` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`rank_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `web_reported_bugs` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `created_at` datetime NOT NULL DEFAULT (utc_timestamp()),
  `reporter_ref` int unsigned DEFAULT NULL,
  `area` enum('hotel','website','other') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'hotel',
  `title` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `description` text COLLATE utf8mb4_unicode_ci NOT NULL,
  `browser` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '',
  `status` enum('new','confirmed','fixed','not_a_bug','duplicate') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'new',
  `staff_reply` text COLLATE utf8mb4_unicode_ci,
  PRIMARY KEY (`id`),
  KEY `idx_web_reported_bugs_status_created` (`status`,`created_at`),
  KEY `fk_web_reported_bugs_reporter` (`reporter_ref`),
  CONSTRAINT `fk_web_reported_bugs_reporter` FOREIGN KEY (`reporter_ref`) REFERENCES `users` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `global_server_stats` (
  `server_up` enum('0','1') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '0',
  `uptime_seconds` int unsigned NOT NULL DEFAULT '0',
  `users_online` int unsigned NOT NULL DEFAULT '0',
  `updated_at` datetime DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO `global_server_stats` () VALUES ();
