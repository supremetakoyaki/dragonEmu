UPDATE `definition_room_heightmap` SET `door_x` = 0 WHERE `label` = 'model_k';
UPDATE `definition_room_heightmap` SET `door_x` = 10 WHERE `label` = 'model_q';
UPDATE `definition_room_heightmap` SET `door_z` = 1 WHERE `label` = 'model_o';

UPDATE `definition_room_heightmap` SET `door_x` = 0, `door_y` = 23, `door_z` = 2, `door_dir` = 2,
  `heightmap` = CONCAT_WS('\r\n',
    'xxxxxxxxxxxxxxxxxxx',
    'xxxxxxx222222222222',
    'xxxxxxx222222222222',
    'xxxxxxx222222222222',
    'xxxxxxx222222222222',
    'xxxxxxx222222222222',
    'xxxxxxx222222222222',
    'xxxxxxx22222222xxxx',
    'xxxxxxx11111111xxxx',
    'x222221111111111111',
    'x222221111111111111',
    'x222221111111111111',
    'x222221111111111111',
    'x222221111111111111',
    'x222221111111111111',
    'x222221111111111111',
    'x222221111111111111',
    'x2222xx11111111xxxx',
    'x2222xx00000000xxxx',
    'x2222xx000000000000',
    'x2222xx000000000000',
    'x2222xx000000000000',
    'x2222xx000000000000',
    'x2222xx000000000000',
    'x2222xx000000000000',
    'xxxxxxxxxxxxxxxxxxx')
WHERE `label` = 'model_p';
