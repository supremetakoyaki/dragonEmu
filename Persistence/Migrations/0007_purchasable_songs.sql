CREATE TABLE `definition_catalog_prebuilt_songs` (
  `ref_product_offer_id` int unsigned NOT NULL,
  `song_name` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `song_author` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `song_data` text COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`ref_product_offer_id`),
  CONSTRAINT `fk_definition_catalog_prebuilt_songs_offer` FOREIGN KEY (`ref_product_offer_id`) REFERENCES `definition_catalog_page_offers` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
