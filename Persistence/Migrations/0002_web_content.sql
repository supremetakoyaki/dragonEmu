CREATE TABLE `web_articles_categories` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `caption` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `web_articles` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `category_ref` int unsigned NOT NULL,
  `created_at` datetime NOT NULL DEFAULT (utc_timestamp()),
  `created_by_ref` int unsigned DEFAULT NULL,
  `title` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `content_html` text COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_web_articles_category_created` (`category_ref`,`created_at`),
  KEY `idx_web_articles_created` (`created_at`),
  KEY `fk_web_articles_created_by` (`created_by_ref`),
  CONSTRAINT `fk_web_articles_category` FOREIGN KEY (`category_ref`) REFERENCES `web_articles_categories` (`id`) ON DELETE RESTRICT,
  CONSTRAINT `fk_web_articles_created_by` FOREIGN KEY (`created_by_ref`) REFERENCES `users` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `web_promos` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `created_at` datetime NOT NULL DEFAULT (utc_timestamp()),
  `image_url` varchar(512) COLLATE utf8mb4_unicode_ci NOT NULL,
  `caption` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `caption_color` char(7) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '#FFFFFF',
  `description` varchar(1000) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '',
  `description_color` char(7) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '#FFFFFF',
  `button_text` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '',
  `button_type` enum('none','go_to_client','go_to_article','go_to_url') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'none',
  `button_url` varchar(512) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `article_ref` int unsigned DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_web_promos_created` (`created_at`),
  KEY `fk_web_promos_article` (`article_ref`),
  CONSTRAINT `fk_web_promos_article` FOREIGN KEY (`article_ref`) REFERENCES `web_articles` (`id`) ON DELETE SET NULL,
  CONSTRAINT `chk_web_promos_caption_color` CHECK (regexp_like(`caption_color`, _utf8mb4'^#[0-9A-Fa-f]{6}$')),
  CONSTRAINT `chk_web_promos_description_color` CHECK (regexp_like(`description_color`, _utf8mb4'^#[0-9A-Fa-f]{6}$'))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `web_hot_campaigns` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `created_at` datetime NOT NULL DEFAULT (utc_timestamp()),
  `image_url` varchar(512) COLLATE utf8mb4_unicode_ci NOT NULL,
  `caption` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `description` varchar(1000) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '',
  `button_url` varchar(512) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_web_hot_campaigns_created` (`created_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `web_settings` (
  `hotel_name` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'dragonHotel',
  `icon_url` varchar(512) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'dragon48.png',
  `logo_url` varchar(512) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'dragon_logo.png',
  `register_enabled` enum('0','1') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '1',
  `footer_text` varchar(500) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'dragonHotel is a not for profit recreation of Habbo v48. Habbo is a registered trademark of Sulake Oy and dragonHotel is not affiliated with it.'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO `web_settings` () VALUES ();
