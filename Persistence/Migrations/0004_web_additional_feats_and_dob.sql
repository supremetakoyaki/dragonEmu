ALTER TABLE `web_settings`
  ADD COLUMN `web_slogan` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'Reminisce early 2010 Habbo Hotel!' AFTER `footer_text`;

ALTER TABLE `web_promos`
  ADD COLUMN `requires_login` enum('0','1') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '0';

ALTER TABLE `web_hot_campaigns`
  ADD COLUMN `url_text` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'Go there' AFTER `button_url`;

ALTER TABLE `users`
  ADD COLUMN `date_of_birth` datetime DEFAULT NULL AFTER `email`;

ALTER TABLE `users_settings`
  ADD COLUMN `allow_3pl_cookies` enum('0','1','2') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '0';

ALTER TABLE `web_settings`
  ADD COLUMN `minimum_age` tinyint unsigned NOT NULL DEFAULT '16' AFTER `register_enabled`;

ALTER TABLE `web_settings`
  ADD COLUMN `hotel_icon_ico` varchar(512) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'dragon48.ico' AFTER `icon_url`;

ALTER TABLE `web_settings`
  ADD COLUMN `discord_embed` varchar(1000) COLLATE utf8mb4_unicode_ci DEFAULT NULL AFTER `web_slogan`;

ALTER TABLE `web_settings`
  ADD COLUMN `flash_client_enabled` enum('0','1') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '0' AFTER `register_enabled`;

CREATE TABLE `web_contact_form` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `created_at` datetime NOT NULL DEFAULT (utc_timestamp()),
  `user_ref` int unsigned DEFAULT NULL,
  `name` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `email` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `subject` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `message` text COLLATE utf8mb4_unicode_ci NOT NULL,
  `ip_address` varchar(45) COLLATE utf8mb4_unicode_ci NOT NULL,
  `status` enum('new','read','answered') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'new',
  PRIMARY KEY (`id`),
  KEY `idx_web_contact_form_ip_created` (`ip_address`,`created_at`),
  KEY `idx_web_contact_form_status_created` (`status`,`created_at`),
  KEY `fk_web_contact_form_user` (`user_ref`),
  CONSTRAINT `fk_web_contact_form_user` FOREIGN KEY (`user_ref`) REFERENCES `users` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

ALTER TABLE `web_page_ranks`
  ADD COLUMN `box_color` varchar(7) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '#2e7cbf';

CREATE TABLE `web_client_vars` (
  `client` enum('both','flash','ruffle') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'both',
  `var_key` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `var_value` varchar(1000) COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`client`,`var_key`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO `web_client_vars` (`client`, `var_key`, `var_value`) VALUES
  ('both', 'connection.info.host', '127.0.0.1'),
  ('flash', 'connection.info.port', '30000'),
  ('ruffle', 'connection.info.port', '30001'),
  ('both', 'site.url', '%web_url%'),
  ('both', 'url.prefix', '%web_url%'),
  ('both', 'flash.client.url', '%assets_url%/r48/RELEASE48-DRAGON-20261029/'),
  ('both', 'external.variables.txt', '%assets_url%/r48/gamedata/external_variables.txt'),
  ('both', 'external.texts.txt', '%assets_url%/r48/gamedata/external_flash_texts.txt'),
  ('both', 'client.starting', 'Please wait! %hotelName% is loading...'),
  ('both', 'client.allow.cross.domain', '1'),
  ('both', 'client.notify.cross.domain', '0'),
  ('both', 'processlog.enabled', '0'),
  ('both', 'flash.client.origin', 'popup');
