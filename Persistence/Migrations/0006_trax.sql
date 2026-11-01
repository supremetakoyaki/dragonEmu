CREATE TABLE `definition_trax_sound_sets` (
  `id` smallint unsigned NOT NULL,
  `furni_def_ref` int unsigned NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_definition_trax_sound_sets_furni` (`furni_def_ref`),
  CONSTRAINT `fk_definition_trax_sound_sets_furni` FOREIGN KEY (`furni_def_ref`) REFERENCES `definition_furniture` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO `definition_trax_sound_sets` (`id`, `furni_def_ref`)
SELECT CAST(SUBSTRING(`classname`, 11) AS UNSIGNED), `id`
FROM `definition_furniture`
WHERE `classname` REGEXP '^sound_set_[0-9]+$';

CREATE TABLE `definition_trax_samples` (
  `id` smallint unsigned NOT NULL,
  `sound_set_ref` smallint unsigned NOT NULL,
  `position` tinyint unsigned NOT NULL,
  `bars` tinyint unsigned NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_definition_trax_samples_set_position` (`sound_set_ref`, `position`),
  CONSTRAINT `fk_definition_trax_samples_set` FOREIGN KEY (`sound_set_ref`) REFERENCES `definition_trax_sound_sets` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO `definition_trax_samples` (`id`, `sound_set_ref`, `position`, `bars`) VALUES
  (1,1,1,1), (2,1,2,2), (3,1,3,2), (4,1,4,4), (5,1,5,2), (6,1,6,1), (7,1,7,2), (8,1,8,1), (9,1,9,2), (10,2,1,2), (11,2,2,2), (12,2,3,2),
  (13,2,4,2), (14,2,5,4), (15,2,6,1), (16,2,7,1), (17,2,8,1), (18,2,9,2), (19,3,1,2), (20,3,2,1), (21,3,3,1), (22,3,4,2), (23,3,5,4), (24,3,6,1),
  (25,3,7,1), (26,3,8,1), (27,3,9,2), (28,4,1,4), (29,4,2,2), (30,4,3,4), (31,4,4,2), (32,4,5,2), (33,4,6,1), (34,4,7,2), (35,4,8,2), (36,4,9,2),
  (37,5,1,4), (38,5,2,1), (39,5,3,2), (40,5,4,2), (41,5,5,2), (42,5,6,6), (43,5,7,1), (44,5,8,2), (45,5,9,1), (46,6,1,3), (47,6,2,4), (48,6,3,2),
  (49,6,4,4), (50,6,5,2), (51,6,6,4), (52,6,7,1), (53,6,8,2), (54,6,9,3), (55,7,1,1), (56,7,2,2), (57,7,3,1), (58,7,4,1), (59,7,5,1), (60,7,6,1),
  (61,7,7,2), (62,7,8,1), (63,7,9,2), (64,8,1,1), (65,8,2,2), (66,8,3,2), (67,8,4,4), (68,8,5,2), (69,8,6,1), (70,8,7,2), (71,8,8,2), (72,8,9,1),
  (73,9,1,2), (74,9,2,2), (75,9,3,3), (76,9,4,2), (77,9,5,4), (78,9,6,4), (79,9,7,2), (80,9,8,4), (81,9,9,3), (82,10,1,1), (83,10,2,2), (84,10,3,1),
  (85,10,4,1), (86,10,5,1), (87,10,6,4), (88,10,7,1), (89,10,8,2), (90,10,9,1), (91,11,1,2), (92,11,2,1), (93,11,3,4), (94,11,4,2), (95,11,5,2), (96,11,6,2),
  (97,11,7,2), (98,11,8,2), (99,11,9,2), (100,12,1,1), (101,12,2,1), (102,12,3,4), (103,12,4,1), (104,12,5,1), (105,12,6,2), (106,12,7,2), (107,12,8,4), (108,12,9,2),
  (109,13,1,1), (110,13,2,4), (111,13,3,1), (112,13,4,2), (113,13,5,2), (114,13,6,2), (115,13,7,2), (116,13,8,2), (117,13,9,4), (118,14,1,1), (119,14,2,4), (120,14,3,2),
  (121,14,4,2), (122,14,5,2), (123,14,6,2), (124,14,7,1), (125,14,8,1), (126,14,9,2), (127,15,1,4), (128,15,2,4), (129,15,3,2), (130,15,4,2), (131,15,5,1), (132,15,6,2),
  (133,15,7,1), (134,15,8,4), (135,15,9,4), (136,16,1,2), (137,16,2,2), (138,16,3,2), (139,16,4,2), (140,16,5,2), (141,16,6,2), (142,16,7,2), (143,16,8,4), (144,16,9,2),
  (145,17,1,4), (146,17,2,1), (147,17,3,1), (148,17,4,1), (149,17,5,1), (150,17,6,2), (151,17,7,2), (152,17,8,4), (153,17,9,4), (154,18,1,1), (155,18,2,2), (156,18,3,1),
  (157,18,4,1), (158,18,5,1), (159,18,6,1), (160,18,7,1), (161,18,8,1), (162,18,9,1), (163,19,1,2), (164,19,2,2), (165,19,3,2), (166,19,4,2), (167,19,5,1), (168,19,6,1),
  (169,19,7,2), (170,19,8,1), (171,19,9,1), (172,20,1,4), (173,20,2,4), (174,20,3,2), (175,20,4,1), (176,20,5,2), (177,20,6,2), (178,20,7,2), (179,20,8,2), (180,20,9,2),
  (181,21,1,4), (182,21,2,4), (183,21,3,4), (184,21,4,1), (185,21,5,1), (186,21,6,1), (187,21,7,1), (188,21,8,4), (189,21,9,4), (190,22,1,1), (191,22,2,1), (192,22,3,1),
  (193,22,4,2), (194,22,5,4), (195,22,6,2), (196,22,7,4), (197,22,8,4), (198,22,9,1), (199,23,1,3), (200,23,2,2), (201,23,3,1), (202,23,4,1), (203,23,5,2), (204,23,6,1),
  (205,23,7,2), (206,23,8,2), (207,23,9,3), (208,24,1,1), (209,24,2,2), (210,24,3,2), (211,24,4,2), (212,24,5,2), (213,24,6,4), (214,24,7,2), (215,24,8,1), (216,24,9,2),
  (217,25,1,1), (218,25,2,4), (219,25,3,4), (220,25,4,1), (221,25,5,4), (222,25,6,1), (223,25,7,1), (224,25,8,4), (225,25,9,4), (226,26,1,2), (227,26,2,2), (228,26,3,1),
  (229,26,4,4), (230,26,5,1), (231,26,6,1), (232,26,7,1), (233,26,8,1), (234,26,9,1), (235,27,1,2), (236,27,2,2), (237,27,3,2), (238,27,4,1), (239,27,5,1), (240,27,6,2),
  (241,27,7,2), (242,27,8,2), (243,27,9,2), (244,28,1,2), (245,28,2,4), (246,28,3,4), (247,28,4,4), (248,28,5,4), (249,28,6,2), (250,28,7,4), (251,28,8,4), (252,28,9,4),
  (253,29,1,1), (254,29,2,2), (255,29,3,2), (256,29,4,1), (257,29,5,1), (258,29,6,2), (259,29,7,2), (260,29,8,1), (261,29,9,2), (262,30,1,2), (263,30,2,4), (264,30,3,2),
  (265,30,4,4), (266,30,5,4), (267,30,6,4), (268,30,7,4), (269,30,8,4), (270,30,9,4), (271,31,1,4), (272,31,2,4), (273,31,3,1), (274,31,4,4), (275,31,5,4), (276,31,6,4),
  (277,31,7,1), (278,31,8,1), (279,31,9,1), (280,32,1,4), (281,32,2,4), (282,32,3,4), (283,32,4,4), (284,32,5,4), (285,32,6,4), (286,32,7,4), (287,32,8,4), (288,32,9,4),
  (289,33,1,2), (290,33,2,2), (291,33,3,2), (292,33,4,4), (293,33,5,2), (294,33,6,2), (295,33,7,2), (296,33,8,2), (297,33,9,2), (298,34,1,2), (299,34,2,2), (300,34,3,2),
  (301,34,4,2), (302,34,5,2), (303,34,6,2), (304,34,7,2), (305,34,8,2), (306,34,9,2), (307,35,1,1), (308,35,2,1), (309,35,3,1), (310,35,4,1), (311,35,5,1), (312,35,6,1),
  (313,35,7,1), (314,35,8,1), (315,35,9,1), (316,36,1,2), (317,36,2,2), (318,36,3,2), (319,36,4,2), (320,36,5,2), (321,36,6,2), (322,36,7,2), (323,36,8,2), (324,36,9,2),
  (325,37,1,4), (326,37,2,4), (327,37,3,4), (328,37,4,2), (329,37,5,2), (330,37,6,2), (331,37,7,2), (332,37,8,2), (333,37,9,2), (334,38,1,2), (335,38,2,2), (336,38,3,1),
  (337,38,4,2), (338,38,5,4), (339,38,6,1), (340,38,7,2), (341,38,8,2), (342,38,9,4), (343,39,1,4), (344,39,2,2), (345,39,3,2), (346,39,4,1), (347,39,5,1), (348,39,6,1),
  (349,39,7,2), (350,39,8,2), (351,39,9,2), (352,40,1,2), (353,40,2,1), (354,40,3,4), (355,40,4,2), (356,40,5,2), (357,40,6,1), (358,40,7,4), (359,40,8,2), (360,40,9,1),
  (361,41,1,1), (362,41,2,2), (363,41,3,2), (364,41,4,2), (365,41,5,2), (366,41,6,1), (367,41,7,4), (368,41,8,2), (369,41,9,4), (370,42,1,3), (371,42,2,4), (372,42,3,4),
  (373,42,4,4), (374,42,5,4), (375,42,6,4), (376,42,7,4), (377,42,8,2), (378,42,9,4), (379,43,1,4), (380,43,2,4), (381,43,3,4), (382,43,4,4), (383,43,5,3), (384,43,6,2),
  (385,43,7,2), (386,43,8,2), (387,43,9,4), (388,44,1,4), (389,44,2,4), (390,44,3,4), (391,44,4,4), (392,44,5,2), (393,44,6,2), (394,44,7,4), (395,44,8,2), (396,44,9,4),
  (397,45,1,2), (398,45,2,1), (399,45,3,2), (400,45,4,2), (401,45,5,2), (402,45,6,2), (403,45,7,2), (404,45,8,2), (405,45,9,4), (406,46,1,2), (407,46,2,1), (408,46,3,1),
  (409,46,4,2), (410,46,5,2), (411,46,6,2), (412,46,7,2), (413,46,8,2), (414,46,9,4), (415,47,1,2), (416,47,2,2), (417,47,3,2), (418,47,4,2), (419,47,5,1), (420,47,6,2),
  (421,47,7,2), (422,47,8,2), (423,47,9,2), (424,48,1,1), (425,48,2,2), (426,48,3,2), (427,48,4,2), (428,48,5,1), (429,48,6,2), (430,48,7,2), (431,48,8,4), (432,48,9,1),
  (433,49,1,2), (434,49,2,2), (435,49,3,2), (436,49,4,2), (437,49,5,2), (438,49,6,2), (439,49,7,4), (440,49,8,4), (441,49,9,2), (442,50,1,1), (443,50,2,2), (444,50,3,1),
  (445,50,4,1), (446,50,5,1), (447,50,6,2), (448,50,7,1), (449,50,8,2), (450,50,9,2), (451,51,1,2), (452,51,2,4), (453,51,3,4), (454,51,4,2), (455,51,5,2), (456,51,6,2),
  (457,51,7,2), (458,51,8,2), (459,51,9,1), (460,52,1,2), (461,52,2,2), (462,52,3,2), (463,52,4,2), (464,52,5,1), (465,52,6,2), (466,52,7,2), (467,52,8,2), (468,52,9,2),
  (469,53,1,2), (470,53,2,4), (471,53,3,4), (472,53,4,2), (473,53,5,4), (474,53,6,4), (475,53,7,4), (476,53,8,4), (477,53,9,2), (478,54,1,4), (479,54,2,2), (480,54,3,4),
  (481,54,4,4), (482,54,5,4), (483,54,6,2), (484,54,7,4), (485,54,8,4), (486,54,9,2), (487,55,1,2), (488,55,2,2), (489,55,3,2), (490,55,4,2), (491,55,5,2), (492,55,6,2),
  (493,55,7,4), (494,55,8,2), (495,55,9,2), (496,56,1,2), (497,56,2,4), (498,56,3,2), (499,56,4,4), (500,56,5,4), (501,56,6,2), (502,56,7,4), (503,56,8,2), (504,56,9,1),
  (505,57,1,4), (506,57,2,4), (507,57,3,2), (508,57,4,4), (509,57,5,2), (510,57,6,2), (511,57,7,2), (512,57,8,2), (513,57,9,1), (514,58,1,4), (515,58,2,4), (516,58,3,4),
  (517,58,4,2), (518,58,5,1), (519,58,6,2), (520,58,7,4), (521,58,8,2), (522,58,9,2), (523,59,1,2), (524,59,2,2), (525,59,3,2), (526,59,4,2), (527,59,5,2), (528,59,6,2),
  (529,59,7,1), (530,59,8,2), (531,59,9,2), (532,60,1,1), (533,60,2,2), (534,60,3,2), (535,60,4,1), (536,60,5,1), (537,60,6,2), (538,60,7,2), (539,60,8,1), (540,60,9,2),
  (541,61,1,1), (542,61,2,2), (543,61,3,2), (544,61,4,2), (545,61,5,2), (546,61,6,4), (547,61,7,2), (548,61,8,2), (549,61,9,2), (550,62,1,4), (551,62,2,4), (552,62,3,4),
  (553,62,4,4), (554,62,5,4), (555,62,6,2), (556,62,7,2), (557,62,8,2), (558,62,9,4), (559,63,1,4), (560,63,2,4), (561,63,3,4), (562,63,4,4), (563,63,5,4), (564,63,6,4),
  (565,63,7,2), (566,63,8,2), (567,63,9,2), (568,64,1,2), (569,64,2,2), (570,64,3,4), (571,64,4,1), (572,64,5,4), (573,64,6,2), (574,64,7,4), (575,64,8,4), (576,64,9,4),
  (577,65,1,4), (578,65,2,4), (579,65,3,4), (580,65,4,4), (581,65,5,4), (582,65,6,4), (583,65,7,4), (584,65,8,4), (585,65,9,4), (586,66,1,4), (587,66,2,4), (588,66,3,4),
  (589,66,4,4), (590,66,5,4), (591,66,6,4), (592,66,7,4), (593,66,8,4), (594,66,9,4), (595,67,1,4), (596,67,2,4), (597,67,3,4), (598,67,4,4), (599,67,5,4), (600,67,6,4),
  (601,67,7,4), (602,67,8,4), (603,67,9,4), (604,68,1,2), (605,68,2,2), (606,68,3,1), (607,68,4,4), (608,68,5,4), (609,68,6,1), (610,68,7,2), (611,68,8,4), (612,68,9,4),
  (613,69,1,4), (614,69,2,2), (615,69,3,1), (616,69,4,2), (617,69,5,4), (618,69,6,2), (619,69,7,2), (620,69,8,1), (621,69,9,2), (622,70,1,4), (623,70,2,2), (624,70,3,2),
  (625,70,4,2), (626,70,5,2), (627,70,6,2), (628,70,7,2), (629,70,8,4), (630,70,9,4), (631,71,1,4), (632,71,2,4), (633,71,3,4), (634,71,4,4), (635,71,5,4), (636,71,6,4),
  (637,71,7,4), (638,71,8,4), (639,71,9,4), (640,72,1,4), (641,72,2,4), (642,72,3,4), (643,72,4,4), (644,72,5,4), (645,72,6,4), (646,72,7,4), (647,72,8,1), (648,72,9,1);

CREATE TABLE `trax_machine_sound_sets` (
  `machine_item_ref` int unsigned NOT NULL,
  `slot` tinyint unsigned NOT NULL,
  `sound_set_item_ref` int unsigned NOT NULL,
  PRIMARY KEY (`machine_item_ref`, `slot`),
  UNIQUE KEY `uq_trax_machine_sound_sets_item` (`sound_set_item_ref`),
  CONSTRAINT `fk_trax_machine_sound_sets_machine` FOREIGN KEY (`machine_item_ref`) REFERENCES `users_items` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_trax_machine_sound_sets_item` FOREIGN KEY (`sound_set_item_ref`) REFERENCES `users_items` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `trax_machine_playlists` (
  `machine_item_ref` int unsigned NOT NULL,
  `position` tinyint unsigned NOT NULL,
  `song_ref` int unsigned NOT NULL,
  PRIMARY KEY (`machine_item_ref`, `position`),
  KEY `idx_trax_machine_playlists_song` (`song_ref`),
  CONSTRAINT `fk_trax_machine_playlists_machine` FOREIGN KEY (`machine_item_ref`) REFERENCES `users_items` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_trax_machine_playlists_song` FOREIGN KEY (`song_ref`) REFERENCES `users_songs` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `users_songs_disks` (
  `disk_item_ref` int unsigned NOT NULL,
  `song_ref` int unsigned NOT NULL,
  PRIMARY KEY (`disk_item_ref`),
  KEY `idx_users_songs_disks_song` (`song_ref`),
  CONSTRAINT `fk_users_songs_disks_item` FOREIGN KEY (`disk_item_ref`) REFERENCES `users_items` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_users_songs_disks_song` FOREIGN KEY (`song_ref`) REFERENCES `users_songs` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO `users_songs_disks` (`disk_item_ref`, `song_ref`)
SELECT `item_ref`, `id` FROM `users_songs` WHERE `item_ref` IS NOT NULL;

ALTER TABLE `users_songs`
  DROP FOREIGN KEY `fk_users_songs_item`,
  DROP INDEX `uq_users_songs_item`,
  DROP COLUMN `item_ref`,
  ADD COLUMN `deleted_at` datetime DEFAULT NULL AFTER `song_data`;

RENAME TABLE `sound_machine_playlists` TO `jukebox_disks`;

ALTER TABLE `jukebox_disks`
  DROP FOREIGN KEY `fk_sound_machine_playlists_disk`,
  DROP FOREIGN KEY `fk_sound_machine_playlists_machine`;

ALTER TABLE `jukebox_disks`
  RENAME COLUMN `position` TO `slot`,
  RENAME INDEX `uq_sound_machine_playlists_disk` TO `uq_jukebox_disks_disk`;

ALTER TABLE `jukebox_disks`
  ADD CONSTRAINT `fk_jukebox_disks_disk` FOREIGN KEY (`disk_item_ref`) REFERENCES `users_items` (`id`) ON DELETE CASCADE,
  ADD CONSTRAINT `fk_jukebox_disks_machine` FOREIGN KEY (`machine_item_ref`) REFERENCES `users_items` (`id`) ON DELETE CASCADE;

CREATE TABLE `jukebox_playlists` (
  `machine_item_ref` int unsigned NOT NULL,
  `position` tinyint unsigned NOT NULL,
  `disk_item_ref` int unsigned NOT NULL,
  PRIMARY KEY (`machine_item_ref`, `position`),
  UNIQUE KEY `uq_jukebox_playlists_disk` (`disk_item_ref`),
  CONSTRAINT `fk_jukebox_playlists_machine` FOREIGN KEY (`machine_item_ref`) REFERENCES `users_items` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_jukebox_playlists_disk` FOREIGN KEY (`disk_item_ref`) REFERENCES `users_items` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

UPDATE `definition_catalog_page_header` SET `coming_soon` = 0 WHERE `id` = 11;

INSERT IGNORE INTO `definition_catalog_page_header` (`id`, `parent_id`, `color`, `icon`, `display_name`, `coming_soon`, `enabled`, `min_rank`, `order_num`) VALUES
  (1101,11,0,4,'Traxmachines',0,1,1,1),
  (1102,11,0,4,'Ambient',0,1,1,2),
  (1103,11,0,4,'Dance',0,1,1,3),
  (1104,11,0,4,'Rock',0,1,1,4),
  (1105,11,0,4,'SFX',0,1,1,5),
  (1106,11,0,4,'Urban',0,1,1,6);

INSERT IGNORE INTO `definition_catalog_page_body` (`ref_page_id`, `layout_code`, `localization_images`, `localization_texts`) VALUES
  (1101,'default_3x3','["catalog_djshop_headline1_uk", "catalog_djshop_teaser1"]','["You must own a Traxmachine and at least one trax-pax. Place the Traxmachine in your room and the trax-pax in your inventory. Double click the Traxmachine, click \\"Trax Editor\\" and start editing music. When ready, save, select the tune, turn on the Traxmachine and enjoy.", "Cool, my own music!"]'),
  (1102,'default_3x3','["catalog_trx_header1_en", "catalog_trx_teaser1_en"]','["Welcome to the Ambient Trax Store! With groovy beats and chilled out melodies, this is the section for some cool and relaxing tunes."]'),
  (1103,'default_3x3','["catalog_trx_header2_en", "catalog_trx_teaser2_en"]','["Welcome to the Dance Trax Store! With funky beats and catchy melodies, this is the section for every clubber to indulge in."]'),
  (1104,'default_3x3','["catalog_trx_header3_en", "catalog_trx_teaser3_en"]','["Welcome to the Rock Trax Store! With heavy beats and rockin'' riffs, this is the section for every rock fan to experiment with."]'),
  (1105,'default_3x3','["catalog_trx_header4_en", "catalog_trx_teaser4_en"]','["Welcome to the SFX Trax Store! With crazy sounds and weird noises, this is the section for every creative room builder to indulge in."]'),
  (1106,'default_3x3','["catalog_trx_header5_en", "catalog_trx_teaser5_en"]','["Welcome to the Urban Trax Store! With hip hop beats and RnB vocals, this is the section for every city bopper to indulge in."]');

UPDATE `definition_catalog_page_offers` SET `ref_page` = 1101 WHERE `offer_code` IN ('sound_machine','sound_machine1','sound_machine3','sound_machine5','sound_machine7','jukebox_1');
UPDATE `definition_catalog_page_offers` SET `ref_page` = 94 WHERE `offer_code` IN ('sound_machine2','sound_machine4','sound_machine6','sound_machine_pro');
UPDATE `definition_catalog_page_offers` SET `ref_page` = 1102 WHERE `offer_code` IN ('A0 sound_set_4','A0 sound_set_8','A0 sound_set_6','A0 sound_set_5','A0 sound_set_26','A0 sound_set_27','A0 sound_set_17','A0 sound_set_18','A0 sound_set_30','A0 sound_set_32','A0 sound_set_37','A0 sound_set_42','A0 sound_set_44','A0 sound_set_45','A0 sound_set_52','A0 sound_set_53','A0 sound_set_54','A0 sound_set_62','A0 sound_set_63','A0 sound_set_64','A0 sound_set_65','A0 sound_set_66','A0 sound_set_67','A0 sound_set_70','A0 sound_set_71','A0 sound_set_72');
UPDATE `definition_catalog_page_offers` SET `ref_page` = 1103 WHERE `offer_code` IN ('A0 sound_set_11','A0 sound_set_13','A0 sound_set_25','A0 sound_set_29','A0 sound_set_3','A0 sound_set_31','A0 sound_set_35','A0 sound_set_46','A0 sound_set_47','A0 sound_set_48','A0 sound_set_49','A0 sound_set_50','A0 sound_set_51','A0 sound_set_9');
UPDATE `definition_catalog_page_offers` SET `ref_page` = 1104 WHERE `offer_code` IN ('A0 sound_set_21','A0 sound_set_28','A0 sound_set_33','A0 sound_set_34','A0 sound_set_38','A0 sound_set_39','A0 sound_set_40','A0 sound_set_41');
UPDATE `definition_catalog_page_offers` SET `ref_page` = 1105 WHERE `offer_code` IN ('A0 sound_set_1','A0 sound_set_12','A0 sound_set_2','A0 sound_set_20','A0 sound_set_22','A0 sound_set_23','A0 sound_set_24','A0 sound_set_43','A0 sound_set_68','A0 sound_set_69','A0 sound_set_7');
UPDATE `definition_catalog_page_offers` SET `ref_page` = 1106 WHERE `offer_code` IN ('A0 sound_set_10','A0 sound_set_14','A0 sound_set_15','A0 sound_set_16','A0 sound_set_19','A0 sound_set_36','A0 sound_set_55','A0 sound_set_56','A0 sound_set_57','A0 sound_set_58','A0 sound_set_59','A0 sound_set_60','A0 sound_set_61');

INSERT INTO `definition_catalog_page_offers` (`ref_page`, `offer_code`, `price_credits`, `price_activity_points`, `club_only`, `giftable`)
SELECT 1101, `offer_code`, `price_credits`, `price_activity_points`, `club_only`, `giftable`
FROM `definition_catalog_page_offers` WHERE `offer_code` IN ('ads_idol_trax','ads_idol_jukebox') AND `ref_page` <> 1101;

INSERT INTO `definition_catalog_page_offers_products` (`ref_offer_id`, `ref_furni_def`, `product_type`, `product_extra_data`, `product_count`, `product_rental_duration`)
SELECT copy.`id`, product.`ref_furni_def`, product.`product_type`, product.`product_extra_data`, product.`product_count`, product.`product_rental_duration`
FROM `definition_catalog_page_offers` copy
JOIN `definition_catalog_page_offers` original ON original.`offer_code` = copy.`offer_code` AND original.`ref_page` <> 1101
JOIN `definition_catalog_page_offers_products` product ON product.`ref_offer_id` = original.`id`
WHERE copy.`ref_page` = 1101 AND copy.`offer_code` IN ('ads_idol_trax','ads_idol_jukebox');
