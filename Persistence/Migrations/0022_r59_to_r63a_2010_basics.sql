ALTER TABLE `definition_catalog_page_offers`
  ADD COLUMN `activity_point_type` tinyint unsigned NOT NULL DEFAULT 0 AFTER `price_activity_points`;

ALTER TABLE `definition_furniture`
  MODIFY COLUMN `interaction_logic` enum('none','toggle','teleport','vending','gate','roller','petfood','petnest','petwater','pettoy','pettreat','totem_leg','totem_head','totem_planet','hockey_light','sound_disk','football','football_goal','football_countdown','football_score_counter','clothing_gate','toggle_multiheight','pool_inflatable','pressure_plate','banzai_tile','banzai_gate','banzai_score','banzai_countdown','banzai_puck','banzai_teleport','banzai_apparatus','banzai_pyramid','firework','icetag_tag_pole','icetag_skating_patch','lever_switch') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'none';

ALTER TABLE `definition_achievement_levels`
  ADD COLUMN `reward_activity_point_type` tinyint unsigned NOT NULL DEFAULT 0 AFTER `reward_activity_points`;

ALTER TABLE `definition_achievements`
  ADD COLUMN `client_type` int unsigned NOT NULL DEFAULT 0 AFTER `code_base`;
UPDATE `definition_achievements` a
JOIN (
  SELECT 'ACH_HappyHour' AS code_base, 3 AS client_type UNION ALL
  SELECT 'ACH_Login', 4 UNION ALL
  SELECT 'ACH_AvatarLooks', 6 UNION ALL
  SELECT 'ACH_RoomEntry', 8 UNION ALL
  SELECT 'ACH_Motto', 9 UNION ALL
  SELECT 'ACH_EmailVerification', 10 UNION ALL
  SELECT 'ACH_RegistrationDuration', 11 UNION ALL
  SELECT 'ACH_AllTimeHotelPresence', 14 UNION ALL
  SELECT 'ACH_AvatarTags', 16 UNION ALL
  SELECT 'ACH_RespectEarned', 17 UNION ALL
  SELECT 'ACH_RespectGiven', 18 UNION ALL
  SELECT 'ACH_TraderPass', 19 UNION ALL
  SELECT 'ACH_Name', 20 UNION ALL
  SELECT 'ACH_PetLover', 23 UNION ALL
  SELECT 'ACH_PetLevelUp', 24 UNION ALL
  SELECT 'ACH_PetFeeding', 25 UNION ALL
  SELECT 'ACH_PetRespectGiver', 26 UNION ALL
  SELECT 'ACH_PetRespectReceiver', 27 UNION ALL
  SELECT 'ACH_GiftGiver', 29 UNION ALL
  SELECT 'ACH_GiftReceiver', 30 UNION ALL
  SELECT 'ACH_VipClub', 31 UNION ALL
  SELECT 'ACH_BasicClub', 32 UNION ALL
  SELECT 'HC', 32 UNION ALL
  SELECT 'ACH_BattleBallTilesLocked', 33 UNION ALL
  SELECT 'ACH_GamePlayerExperience', 34 UNION ALL
  SELECT 'ACH_BattleBallWinner', 35 UNION ALL
  SELECT 'ACH_BattleBallPlayer', 35 UNION ALL
  SELECT 'ACH_CameraPhotoCount', 1001 UNION ALL
  SELECT 'ACH_FootballGoalScored', 1002 UNION ALL
  SELECT 'ACH_AIPerformanceVote', 1003 UNION ALL
  SELECT 'ACH_FootballGoalScoredInRoom', 1004
) t ON t.code_base = a.code_base
SET a.`client_type` = t.client_type;

UPDATE `users` u
SET u.`achievement_score` = (
  SELECT COALESCE(SUM(l.`reward_achievement_score`), 0)
  FROM `users_achievement_progress` p
  JOIN `definition_achievements` a ON a.`id` = p.`achievement_ref` AND a.`enabled` = 1
  JOIN `definition_achievement_levels` l ON l.`achievement_ref` = p.`achievement_ref` AND l.`level` <= p.`current_level`
  WHERE p.`user_ref` = u.`id`);

INSERT INTO `global_message_texts` (`message_key`, `message_text`) VALUES
  ('currency_snowflakes','{"en": "snowflakes", "es": "copos de nieve", "pt-br": "flocos de neve"}')
ON DUPLICATE KEY UPDATE `message_text` = JSON_MERGE_PATCH(VALUES(`message_text`), `message_text`);

INSERT INTO `definition_furniture` (`id`, `type`, `ref_furnidata`, `classname`, `x_dim`, `y_dim`, `z_dim`, `localization_name`, `localization_desc`, `localized_name`, `localized_desc`, `logic_class`, `visualization_class`, `interaction_logic`, `states`, `is_sittable`, `is_bed`, `is_walkable`, `is_stackable`, `allow_trade`, `allow_recycle`)
SELECT * FROM (VALUES
  ROW(2537, 's', 3663, 'wf_act_move_rotate', 1, 1, 0.65, 'WIRED Effect: Move And Rotate Furni', 'This effect moves and rotates the selected Furni', '{"es": "Efecto WIRED: Mover y Rotar Furni", "pt-br": "EFEITO WIRED: Mover e Rotacionar Mobi"}', '{"es": "Este efecto mueve y rota los Furni seleccionados", "pt-br": "O efeito move e rotaciona o Mobi selecionado"}', 'multistate', 'animated', 'none', '[0, 1]', 0, 0, 1, 1, 1, 1),
  ROW(2538, 's', 3664, 'wf_wire2', 1, 1, 0.0, 'Wire Junction', 'Wires that can be laid down in a room.', '{"es": "Cableado: Intersección", "pt-br": "Conexão de Cabos"}', '{"es": "A colocar en una Sala", "pt-br": "Cabos que podem ser conectados no seu Quarto"}', 'multistate', 'animated', 'toggle', '[0, 1, 2, 3]', 0, 0, 1, 1, 1, 1),
  ROW(2539, 's', 3665, 'wf_cnd_time_more_than', 1, 1, 0.65, 'WIRED Condition: More Than x Secs Elapsed Since Timer Reset', 'Triggering is allowed if more than x seconds have elapsed since timer reset.', '{"es": "Condición WIRED: + segundos", "pt-br": "CONDIÇÃO WIRED: Mais de X segundos passados"}', '{"es": "El Efecto se produce si han transcurrido más de x segundos desde el reseteo.", "pt-br": "Ativadores funcionarão se mais de x segundos tiverem passado desde o reinicio do temporizador"}', 'multistate', 'animated', 'none', '[0, 1]', 0, 0, 1, 1, 1, 1),
  ROW(2540, 's', 3666, 'wf_colortile', 1, 1, 0.2, 'Color Tile', 'Changes color.', '{"es": "Casilla Color", "pt-br": "Piso Colorido"}', '{"es": "Cambia el color", "pt-br": "Troca de cor"}', 'multistate', 'animated', 'toggle', '[0, 1, 2, 3, 4, 5, 6]', 0, 0, 1, 1, 1, 1),
  ROW(2541, 's', 3667, 'wf_floor_switch2', 1, 1, 1.0, 'Floor Switch 2', 'Switch has four different states. Can only be operated from an adjacent tile.', '{"es": "Interruptor de Suelo 2", "pt-br": "Alavanca de Chão 2"}', '{"es": "Cuatro estados diferentes. Manejable sólo desde la casilla de al lado", "pt-br": "Alavanca com quatro estados que só pode ser operada de perto."}', 'multistate', 'animated', 'lever_switch', '[0, 1, 2, 3]', 0, 0, 0, 0, 1, 1),
  ROW(2542, 's', 3668, 'wf_trg_state_changed', 1, 1, 0.65, 'WIRED Trigger: Furni State Is Changed', 'Effects are triggered when the state of a Furni is changed by double clicking it.', '{"es": "Causante WIRED: Furni es Usado", "pt-br": "ATIVADOR WIRED: Mobi é usado"}', '{"es": "Los efectos tienen lugar cuando cambia el estado de un Furni mediante un doble clic.", "pt-br": "Efeitos são ativados quando o estado do Mobi é alterado clicando duas vezes."}', 'multistate', 'animated', 'none', '[0, 1]', 0, 0, 1, 1, 1, 1),
  ROW(2543, 's', 3669, 'wf_xtra_random', 1, 1, 0.37, 'WIRED Add-on: Random Effect', 'Only one random Effect in this stack is triggered at a time.', '{"es": "Complemento WIRED: Efecto Aleatorio", "pt-br": "WIRED EXTRA: Efeito Aleatório"}', '{"es": "Sólo un Efecto aleatorio tiene lugar a la vez en esta pila", "pt-br": "Apenas um Efeito aleatório poderá ser feito por vez."}', 'multistate', 'animated', 'none', '[0, 1]', 0, 0, 1, 1, 1, 1),
  ROW(2544, 's', 3670, 'wf_xtra_unseen', 1, 1, 0.37, 'WIRED Add-on: Unseen Effect', 'Only one previously unused Effect in this stack is triggered at a time.', '{"es": "Complemento WIRED: Efecto WIRED No Visto", "pt-br": "WIRED EXTRA: Efeito Inédito"}', '{"es": "Sólo un Efecto previamente no usado tiene lugar a la vez en esta pila", "pt-br": "Apenas um Efeito inédito pode ser ativado por vez."}', 'multistate', 'animated', 'none', '[0, 1]', 0, 0, 1, 1, 1, 1),
  ROW(2545, 's', 3671, 'wf_trg_periodically', 1, 1, 0.65, 'WIRED Trigger: Repeat Effect', 'Effects are triggered every x seconds.', '{"es": "Causante WIRED: Periodos", "pt-br": "ATIVADOR WIRED: Repetir Efeito"}', '{"es": "Los efectos se cuenta cuando el primer personaje entre a la Sala o cuando se resetea el Furni.", "pt-br": "Efeitos são ativados a cada x segundos"}', 'multistate', 'animated', 'none', '[0, 1]', 0, 0, 1, 1, 1, 1),
  ROW(2546, 's', 3672, 'wf_pyramid', 1, 1, 0.01, 'Hidden Pyramid', 'Hides in the ground and periodically rises, acting as a divider', '{"es": "Pirámide Oculta", "pt-br": "Pirâmide Wired"}', '{"es": "Se esconde en el suelo y periódicamente aparece, actuando como un divisor", "pt-br": "Divisória que bloqueia a passagem. Aparece e desaparece no chão por tempo determinado"}', 'multistate', 'animated', 'banzai_pyramid', '[0, 1]', 0, 0, 1, 0, 1, 1),
  ROW(2547, 's', 3673, 'wf_trg_score_achieved', 1, 1, 0.65, 'WIRED Trigger: Score Is Achieved', 'Effects are triggered when a team has achieved x points in a game.', '{"es": "Causante WIRED: Puntos Conseguidos", "pt-br": "ATIVADOR WIRED: Pontuação obtida"}', '{"es": "Se activa cuando se alcanza una puntuación determinada", "pt-br": "Disparado quando a pontuação definida é atingida"}', 'multistate', 'animated', 'none', '[0, 1]', 0, 0, 1, 1, 1, 1),
  ROW(2548, 's', 3674, 'wf_act_teleport_to', 1, 1, 0.65, 'WIRED Effect: Teleport To Furni', 'This Effect teleports the triggering user to a Furni. If you select more than one Furni, the user is randomly teleported to one of them.', '{"es": "Efecto WIRED: ¡Adiós!", "pt-br": "EFEITO WIRED: Teleportar para Mobi"}', '{"es": "Este efecto teletransporta al personaje que se haya programado hasta otro Furni. Si has seleccionado más de un Furni, es teletransportado aleatoriamente..", "pt-br": "O efeito teleporta um Habbo para um Mobi. Se selecionar mais de um Mobi, o Habbo é teleportado aleatoriamente para um deles."}', 'multistate', 'animated', 'none', '[0, 1]', 0, 0, 1, 1, 1, 1),
  ROW(2549, 's', 3675, 'wf_trg_says_something', 1, 1, 0.65, 'WIRED Trigger: User Says Keyword', 'Effects are triggered when a user says a certain word or phrase.', '{"es": "Causante WIRED: Clave", "pt-br": "ATIVADOR WIRED: Habbo diz algo"}', '{"es": "Los efectos tienen lugar cuando un personaje dice una palabra o frase concreta.", "pt-br": "Efeitos são ativados quando o Habbo diz uma palavra ou frase."}', 'multistate', 'animated', 'none', '[0, 1]', 0, 0, 1, 1, 1, 1),
  ROW(2550, 's', 3676, 'wf_colorwheel', 1, 1, 1.0, 'Color Wheel', 'Which color now?', '{"es": "Casilla Ruleta", "pt-br": "Roda de Cores"}', '{"es": "¿Y ahora qué color?", "pt-br": "Qual cor agora?"}', 'multistate', 'animated', 'toggle', '[0, 1, 2, 3, 4, 5, 6, 7, 8, 9]', 0, 0, 0, 0, 1, 1),
  ROW(2551, 's', 3677, 'wf_wire4', 1, 1, 0.0, 'Three Way Wires', 'Wires that can be laid down in a room.', '{"es": "Cableado: Tres vías", "pt-br": "Conexão de 3 Cabos"}', '{"es": "A colocar en una Sala", "pt-br": "Cabos que podem ser conectados no seu Quarto"}', 'multistate', 'animated', 'toggle', '[0, 1, 2, 3]', 0, 0, 1, 1, 1, 1),
  ROW(2552, 's', 3678, 'wf_trg_walks_off_furni', 1, 1, 0.65, 'WIRED Trigger: User Walks Off Furni', 'Effects are triggered when a user steps off a Furni.', '{"es": "Causante WIRED: Vete", "pt-br": "ATIVADOR WIRED: Habbo sai do Mobi"}', '{"es": "Los efectos tienen lugar cuando un personaje se aleje del Furni.", "pt-br": "Efeitos são ativados quando o Habbo sai de cima do Mobi."}', 'multistate', 'animated', 'none', '[0, 1]', 0, 0, 1, 1, 1, 1),
  ROW(2553, 's', 3679, 'wf_trg_at_given_time', 1, 1, 0.65, 'WIRED Trigger: At Set Time', 'Effects are triggered at a given time after the timer is reset.', '{"es": "Causante WIRED: ¡Ya!", "pt-br": "ATIVADOR WIRED: No tempo escolhido"}', '{"es": "Los efectos tienen lugar a una hora concreta. El tiempo comienza a contar cuando el primer personaje entre a la Sala o cuando sea programado un reseteo.", "pt-br": "Efeitos são ativados em uma hora definida depois do contador ser reiniciado."}', 'multistate', 'animated', 'none', '[0, 1]', 0, 0, 1, 1, 1, 1),
  ROW(2554, 's', 3680, 'wf_trg_game_ends', 1, 1, 0.65, 'WIRED Trigger: Game Ends', 'Effects are triggered when a game ends in the room.', '{"es": "Causante WIRED: Acaba el juego", "pt-br": "ATIVADOR WIRED: Final de Jogo"}', '{"es": "Los efectos tienen lugar al acabar un juego de Battle Banzai en una Sala.", "pt-br": "Efeitos são ativados quando um jogo de Battle Banzai terminar."}', 'multistate', 'animated', 'none', '[0, 1]', 0, 0, 1, 1, 1, 1),
  ROW(2555, 's', 3681, 'wf_act_show_message', 1, 1, 0.65, 'WIRED Effect: Show Message', 'This effect shows a secret message only to the triggering users.', '{"es": "Efecto WIRED: Mensaje", "pt-br": "EFEITO WIRED: Mostrar mensagem"}', '{"es": "Este efecto muestra un mensaje secreto sólo al personaje seleccionado.", "pt-br": "Este Efeito mostra uma mensagem secreta apenas para os Habbos que o ativaram."}', 'multistate', 'animated', 'none', '[0, 1]', 0, 0, 1, 1, 1, 1),
  ROW(2556, 's', 3682, 'wf_cnd_time_less_than', 1, 1, 0.65, 'WIRED Condition: Less Than x Secs Elapsed Since Timer Reset', 'Triggering is allowed if less than x seconds have elapsed since timer reset.', '{"es": "Condición WIRED: - segundos", "pt-br": "CONDIÇÃO WIRED: Menos de X segundos passados desde o reinicio do temporizador"}', '{"es": "El Efecto se produce si han transcurrido menos de x segundos desde el reseteo.", "pt-br": "Ativadores funcionarão se menos de x segundos tiverem passado desde o reinicio do temporizador"}', 'multistate', 'animated', 'none', '[0, 1]', 0, 0, 1, 1, 1, 1),
  ROW(2557, 's', 3683, 'wf_trg_enter_room', 1, 1, 0.65, 'WIRED Trigger: User Enters Room', 'Effects are triggered when a user enters the room.', '{"es": "Causante WIRED: Entrada", "pt-br": "ATIVADOR WIRED: Habbo entra no Quarto"}', '{"es": "Los efectos tienen lugar cuando un personaje entra en la Sala.", "pt-br": "Efeitos são ativados quando um Habbo entra no Quarto."}', 'multistate', 'animated', 'none', '[0, 1]', 0, 0, 1, 1, 1, 1),
  ROW(2558, 's', 3684, 'ads_hh_safe', 1, 1, 1.0, 'ads_hh_safe name', 'ads_hh_safe desc', '{"es": "ads_hh_safe name", "pt-br": "ads_hh_safe name"}', '{"es": "ads_hh_safe desc", "pt-br": "ads_hh_safe desc"}', 'multistate', 'animated', 'toggle', '[0, 1]', 0, 0, 0, 0, 0, 1),
  ROW(2559, 's', 3685, 'wf_act_toggle_state', 1, 1, 0.65, 'WIRED Effect: Toggle Furni State', 'This Effect changes the state of the chosen Furni as if the Furni has just been double clicked.', '{"es": "Efecto WIRED: Mutar", "pt-br": "EFEITO WIRED: Mudar estado do Mobi"}', '{"es": "Este efecto cambia el estado del Furni seleccionado al recibir un doble clic.", "pt-br": "Este Efeito muda o estado do Mobi escolhido como se alguém tivesse clicando duas vezes nele."}', 'multistate', 'animated', 'none', '[0, 1]', 0, 0, 1, 1, 1, 1),
  ROW(2560, 's', 3686, 'wf_firegate', 1, 1, 0.0, 'Wall Of Flame', 'Acts as a fiery divider', '{"es": "Muro de Llamas", "pt-br": "Muralha de Fogo"}', '{"es": "Actúa como un divisor ardiente", "pt-br": "Funciona como uma divisória quente"}', 'multistate', 'animated', 'gate', '[0, 1]', 0, 0, 1, 0, 1, 1),
  ROW(2561, 's', 3687, 'wf_ringplate', 1, 1, 0.2, 'Ringplate', 'Can be pressed down and lights up.', '{"es": "Casilla Timbre", "pt-br": "Piso de Anel"}', '{"es": "Al presionarlo, se encienden las luces", "pt-br": "Ao ser pressionado as luzes acendem"}', 'multistate', 'animated', 'toggle', '[0, 1]', 0, 0, 1, 1, 1, 1),
  ROW(2562, 's', 3688, 'wf_pressureplate', 1, 1, 0.2, 'Pressure Plate', 'What does this plate do?', '{"es": "Casilla Presión", "pt-br": "Piso Pressionável"}', '{"es": "¿Qué hace esta casilla?", "pt-br": "O que este piso faz?"}', 'multistate', 'animated', 'pressure_plate', '[0, 1]', 0, 0, 1, 1, 1, 1),
  ROW(2563, 's', 3689, 'wf_glowball', 1, 1, 1.0, 'Glowball', 'Glowball rises and lights up.', '{"es": "Bola Brillante", "pt-br": "Esfera Flutuante"}', '{"es": "Iluminando", "pt-br": "Esfera que flutua e acende"}', 'multistate', 'animated', 'toggle', '[0, 1]', 0, 0, 0, 0, 1, 1),
  ROW(2564, 's', 3690, 'bb_lightstrip', 1, 1, 0.0, 'Banzai Light', 'bb_lightstrip desc', '{"es": "Separador", "pt-br": "Luz Banzai"}', '{"es": "Cada cosa en su sitio", "pt-br": "Dá um toque especial ao seu jogo!"}', 'multistate', 'animated', 'toggle', '[0, 1, 2]', 0, 0, 1, 1, 1, 1),
  ROW(2565, 's', 3691, 'wf_act_reset_timers', 1, 1, 0.65, 'WIRED Effect: Timer Reset', 'This Effect resets triggers that use timers.', '{"es": "Efecto WIRED: Reset", "pt-br": "EFEITO WIRED: Reiniciar tempo"}', '{"es": "Este efecto resetea los tiempos usados en los Causantes: ¡Ya! y Periodos", "pt-br": "Este efeito reinicia Ativadores que usam temporizadores"}', 'multistate', 'animated', 'none', '[0, 1]', 0, 0, 1, 1, 1, 1),
  ROW(2566, 's', 3692, 'wf_cnd_furnis_hv_avtrs', 1, 1, 0.65, 'WIRED Condition: Furni has users', 'Triggering is allowed if all the selected Furni have users on them.', '{"es": "Condición WIRED: Furnis con Usuarios", "pt-br": "CONDIÇÃO WIRED: Mobis com Habbos"}', '{"es": "El Efecto tiene lugar si todos los Furni seleccionados tienen un avatar encima", "pt-br": "Ativador funcionará se todos os Mobis selecionados tiverem Habbos em cima."}', 'multistate', 'animated', 'none', '[0, 1]', 0, 0, 1, 1, 1, 1),
  ROW(2567, 's', 3693, 'wf_arrowplate', 1, 1, 0.2, 'Arrow Plate', 'This way or that way?', '{"es": "Casilla Flecha", "pt-br": "Piso de Seta"}', '{"es": "¿Por aquí o por allí?", "pt-br": "Por este ou por aquele caminho?"}', 'multistate', 'animated', 'pressure_plate', '[0, 1]', 0, 0, 1, 1, 1, 1),
  ROW(2568, 's', 3694, 'wf_cnd_trggrer_on_frn', 1, 1, 0.65, 'WIRED Condition: Triggering User Is On Furni', 'Triggering is allowed if the triggering user is on one of the selected Furni.', '{"es": "Condición WIRED: Desencadenantes Sobre Furni", "pt-br": "CONDIÇÃO WIRED: Habbo Ativador está no Mobi"}', '{"es": "El Efecto tiene lugar si el usuario seleccionado está sobre uno de los Furni programados.", "pt-br": "Ativação é permitida se o Habbo Ativador estiver no Mobi selecionado"}', 'multistate', 'animated', 'none', '[0, 1]', 0, 0, 1, 1, 1, 1),
  ROW(2569, 's', 3695, 'wf_cnd_match_snapshot', 1, 1, 0.65, 'WIRED Condition: Furni States And Positions Match', 'Triggering is allowed if the states and positions of the selected Furni match.', '{"es": "Condición WIRED: Posición o Estado Coincide", "pt-br": "CONDIÇÃO WIRED: Estado e Posição do Mobi combinam"}', '{"es": "El Efecto tiene lugar si los estados de los Furni seleccionados coinciden.", "pt-br": "Ativador funcionará se a posição e o estado do Mobi combinarem."}', 'multistate', 'animated', 'none', '[0, 1]', 0, 0, 1, 1, 1, 1),
  ROW(2570, 's', 3696, 'wf_wire1', 1, 1, 0.0, 'Floor Wiring', 'Wires that can be laid down in a room', '{"es": "Cableado: Recto", "pt-br": "Cabos WIRED"}', '{"es": "A colocar en una Sala", "pt-br": "Cabos que podem ser conectados no seu Quarto"}', 'multistate', 'animated', 'toggle', '[0, 1, 2, 3]', 0, 0, 1, 1, 1, 1),
  ROW(2571, 's', 3697, 'wf_act_give_score', 1, 1, 0.65, 'WIRED Effect: Give Points', 'This Effect gives the set amount of points to the team that triggers the Effect. You can also choose how many times in a game the amount can be given.', '{"es": "Efecto WIRED: Dar Puntos", "pt-br": "EFEITO WIRED: Dar Pontos"}', '{"es": "Este efecto da la cantidad de puntos que corresponda y se haya programado a los jugadores de un equipo. También puedes elegir cuántas veces se puede dar esos puntos en un mismo juego.", "pt-br": "Este Efeito dá uma quantidade x de pontos para os jogadores. Você poderá criar grandes jogos!"}', 'multistate', 'animated', 'none', '[0, 1]', 0, 0, 1, 1, 1, 1),
  ROW(2572, 's', 3698, 'wf_wire3', 1, 1, 0.0, 'Wire Corner', 'Wires that can be laid down in a room.', '{"es": "Cableado: Esquina", "pt-br": "Curva de Cabos"}', '{"es": "A colocar en una Sala", "pt-br": "Cabos que podem ser conectados no seu Quarto"}', 'multistate', 'animated', 'toggle', '[0, 1, 2, 3]', 0, 0, 1, 1, 1, 1),
  ROW(2573, 's', 3699, 'wf_glassdoor', 1, 1, 0.2, 'Glass Door', 'When Effect is triggered Glass Door goes up or down.', '{"es": "Puerta de Cristal", "pt-br": "Porta de Vidro"}', '{"es": "Hacia arriba y abajo", "pt-br": "Sobe e desce"}', 'multistate', 'animated', 'gate', '[0, 1]', 0, 0, 1, 0, 1, 1),
  ROW(2574, 's', 3700, 'wf_act_match_to_sshot', 1, 1, 0.65, 'WIRED Effect: Match Furni to Position & State', 'This Effect returns the selected Furni to its set conditions.', '{"es": "Efecto WIRED: A Cero", "pt-br": "EFEITO WIRED: Mobis que voltam à programação original"}', '{"es": "Este efecto devuelve al Furni seleccionado a sus condiciones originales.", "pt-br": "Este Efeito faz o Mobi selecionado voltar ao estado original que você o programou."}', 'multistate', 'animated', 'none', '[0, 1]', 0, 0, 1, 1, 1, 1),
  ROW(2575, 's', 3701, 'wf_floor_switch1', 1, 1, 1.0, 'Floor Switch 1', 'Two states- Switch can be turned on/off. Can only be operated from an adjacent tile.', '{"es": "Interruptor de Suelo 1", "pt-br": "Alavanca de Chão 1"}', '{"es": "Dos estados: encendido/apagado. Manejable sólo desde la casilla de al lado", "pt-br": "Dois estados - ligada e desligada. Só pode ser operada de perto."}', 'multistate', 'animated', 'lever_switch', '[0, 1]', 0, 0, 0, 0, 1, 1),
  ROW(2576, 's', 3702, 'wf_trg_game_starts', 1, 1, 0.65, 'WIRED Trigger: Game Starts', 'Effects are triggered when a game starts in the room', '{"es": "Causante WIRED: Comienza el Juego", "pt-br": "ATIVADOR WIRED: Começo de Jogo"}', '{"es": "Los efectos tienen lugar cuando se inicia un juego de Battle Banzai en una Sala", "pt-br": "Efeitos são ativados quando um jogo de Battle Banzai começar."}', 'multistate', 'animated', 'none', '[0, 1]', 0, 0, 1, 1, 1, 1),
  ROW(2577, 's', 3703, 'wf_trg_walks_on_furni', 1, 1, 0.65, 'WIRED Trigger: User Walks On Furni', 'Actions are triggered when a user steps or sits on a Furni.', '{"es": "Causante WIRED: Sobre un Furni", "pt-br": "ATIVADOR WIRED: Habbo anda no Mobi"}', '{"es": "Los efectos tienen lugar cuando un personaje camina sobre un Furni.", "pt-br": "Efeitos são ativados quando alguém pisa ou senta no Mobi."}', 'multistate', 'animated', 'none', '[0, 1]', 0, 0, 1, 1, 1, 1),
  ROW(2578, 's', 3704, 'exe_copier', 1, 1, 1.9, 'Copier', 'Paper only!', '{"es": "Fotocopiadora", "pt-br": "Copiadora"}', '{"es": "¿Conseguirás que salga bien?", "pt-br": "Copiadora"}', 'multistate', 'animated', 'toggle', '[0, 1]', 1, 0, 0, 0, 1, 1),
  ROW(2579, 's', 3705, 'exe_wrkdesk', 2, 1, 1.0, 'Work Desk', 'Do not disturb!', '{"es": "Ordenador Executive", "pt-br": "Escrivaninha"}', '{"es": "Dirige tu empresa a golpe de clic", "pt-br": "Que Bagunça!"}', 'multistate', 'animated', 'toggle', '[0, 1]', 0, 0, 0, 0, 1, 1),
  ROW(2580, 's', 3706, 'exe_glassdvdr', 2, 1, 1.0, 'Executive Divider', 'Divide and conquer', '{"es": "Divisor Executive", "pt-br": "Balcão de Vidro"}', '{"es": "A esto se le llama tener clase", "pt-br": "Balcão de Vidro"}', 'multistate', 'animated', 'toggle', '[0, 1]', 0, 0, 0, 0, 1, 1),
  ROW(2581, 's', 3707, 'exe_seccam', 1, 1, 1.0E-6, 'Security Camera', 'Smile! You''re on camera.', '{"es": "Cámara de Seguridad", "pt-br": "Câmera de Segurança"}', '{"es": "Te estamos grabando...", "pt-br": "Estamos de olho em você!"}', 'multistate', 'animated', 'toggle', '[0, 1]', 0, 0, 1, 1, 1, 1),
  ROW(2582, 's', 3708, 'qt_penguin_icehockey', 1, 1, 0.0, 'Hockey Penguin', 'Aptenodytes Gretzky', '{"es": "Pingüino de Hockey", "pt-br": "Pinguim de Hóquei"}', '{"es": "Aptenodytes Gretzky", "pt-br": "Qualquer descuido pode ser perigoso."}', 'multistate', 'animated', 'toggle', '[0]', 0, 0, 0, 0, 0, 1),
  ROW(2583, 's', 3709, 'duck_vip', 1, 1, 1.0, 'VIP Trial - 7 Days', 'Seven days of VIP with a free Duck!', '{"es": "Pato HC", "pt-br": "Pato HC"}', '{"es": "¡Viva el Club HC!", "pt-br": "O Natal 2010 trouxe um quack diferente."}', 'basic', 'static', 'none', '[0]', 0, 0, 0, 0, 0, 1),
  ROW(2584, 's', 3710, 'duck_hc', 1, 1, 1.0, 'HC Trial - 7 Days', 'Seven days of HC with a free Duck!', '{"es": "Pato HC", "pt-br": "Pato HC"}', '{"es": "¡Viva el Habbo Club!", "pt-br": "O Natal 2010 trouxe um quack diferente."}', 'basic', 'static', 'none', '[0]', 0, 0, 0, 0, 0, 1),
  ROW(2585, 's', 3711, 'qt_penguin_ballet', 1, 1, 0.0, 'Ballerina Penguin', 'Aptenodytes Vaganova', '{"es": "Pingüino Bailarín", "pt-br": "Pinguinha Bailarina"}', '{"es": "Aptenodytes Vaganova", "pt-br": "Movimentos deslizantes e velozes."}', 'multistate', 'animated', 'toggle', '[0]', 0, 0, 0, 0, 0, 1),
  ROW(2586, 's', 3712, 'es_bench', 2, 1, 1.25, 'Park Bench', 'Rest your feet after a hard day of shopping.', '{"es": "Banco de Madera", "pt-br": "Banco de Praça"}', '{"es": "Cómodo, lo que se dice cómodo...", "pt-br": "A mesma praça, o mesmo banco..."}', 'multistate', 'animated', 'toggle', '[0, 1]', 1, 0, 0, 0, 1, 1),
  ROW(2587, 's', 3713, 'es_icestar', 1, 1, 1.0E-6, 'Blue Ice Star', 'Twinkle, twinkle melting star.', '{"es": "Estrella Azul", "pt-br": "Estrela Cadente Gelo"}', '{"es": "¿Ya la has colgado?", "pt-br": "Do alto do céu para dentro do seu Quarto."}', 'multistate', 'animated', 'toggle', '[0, 1, 2]', 0, 0, 1, 1, 1, 1),
  ROW(2588, 's', 3714, 'es_fnc_2', 2, 1, 1.0, 'City Street Divider', 'Christmas, city style.', '{"es": "Valla de Piedra", "pt-br": "Divisória Nevada"}', '{"es": "Fría como un témpano de hielo", "pt-br": "Cada toque é uma fria."}', 'multistate', 'animated', 'toggle', '[0, 1]', 0, 0, 0, 0, 1, 1),
  ROW(2589, 's', 3715, 'qt_xm10_monkey_lamp', 1, 1, 1.0, 'Monkey On Ice', 'A warm glow on a cold Xmas night.', '{"es": "Lámpara Mono Helada", "pt-br": "Lâmpada Fria de Macaco"}', '{"es": "¿Seguirá iluminando?", "pt-br": "Quem disse que gelo não ilumina?"}', 'multistate', 'animated', 'toggle', '[0, 1]', 0, 0, 0, 0, 0, 1),
  ROW(2590, 's', 3716, 'es_sidewalk', 2, 2, 0.2, 'Snowy Sidewalk', 'Slippery when wet.', '{"es": "Pavimento Nevado", "pt-br": "Calçada Nevada"}', '{"es": "¡No te resbales!", "pt-br": "Nada de correr em cima dela."}', 'multistate', 'animated', 'toggle', '[0, 1, 2, 3]', 0, 0, 1, 1, 1, 1),
  ROW(2591, 's', 3717, 'es_statue', 1, 1, 1.0, 'City Statue', 'Some days you''re the pigeon, some days you''re the statue.', '{"es": "Estatua de Piedra", "pt-br": "Estátua de Rua"}', '{"es": "Igual tiene 1.000 años", "pt-br": "Uma grandiosa homenagem ao nosso famoso Fulano."}', 'multistate', 'animated', 'toggle', '[0, 1, 2, 3]', 0, 0, 0, 0, 1, 1),
  ROW(2592, 's', 3718, 'qt_xm10_iceclubsofa', 2, 1, 1.0, 'Cool Club Sofa', 'The pillows should keep you warm.', '{"es": "El Club Sofá Helado", "pt-br": "Sofá Nevado"}', '{"es": "Posaderas HC congeladas", "pt-br": "O Sofá do Clube numa nova versão."}', 'basic', 'animated', 'none', '[0, 1]', 1, 0, 0, 0, 1, 1),
  ROW(2593, 's', 3719, 'qt_xm10_xmduck', 1, 1, 1.0, 'Jolly Duck', 'Quack... brrr... quack.', '{"es": "Pato de Navidad Helado", "pt-br": "Pato Nevado de Natal"}', '{"es": "Entrañable pero frío, muy frío", "pt-br": "Não aperte muito, poderá derretê-lo."}', 'basic', 'animated', 'none', '[0, 1]', 0, 0, 0, 0, 0, 1),
  ROW(2594, 's', 3720, 'qt_xm10_palm_tree', 1, 1, 1.0, 'Frozen Fronds', 'Definitely not a tropical native.', '{"es": "Palmera Helada", "pt-br": "Palmeira de Neve"}', '{"es": "¿Qué subespecie será ésta?", "pt-br": "Sombra mais fresca não há."}', 'basic', 'animated', 'none', '[0, 1]', 0, 0, 0, 0, 0, 1),
  ROW(2595, 's', 3721, 'es_fnc_crnr', 1, 1, 1.0, 'City Street Corner', 'Xmas is just around the corner!', '{"es": "Esquina Valla de Piedra", "pt-br": "Canto Nevado"}', '{"es": "Fría como un témpano de hielo", "pt-br": "O que é um pontinho branco no canto do Quarto?"}', 'multistate', 'animated', 'toggle', '[0, 1]', 0, 0, 0, 0, 1, 1),
  ROW(2596, 's', 3722, 'qt_xm10_trex', 3, 7, 1.0, 'Frozen In Time', 'Maybe the Ice Age was responsible after all?', '{"es": "T-Rex Helado", "pt-br": "Raro Gelado Rex"}', '{"es": "Así y todo da un miedo...", "pt-br": "Um Tiranossauro de gelo, nada mais."}', 'basic', 'animated', 'none', '[0, 1]', 0, 0, 0, 0, 1, 1),
  ROW(2597, 's', 3723, 'es_epictree', 2, 2, 1.0, 'Epic Christmas Tree', 'Any taller and we''d have to raise the invisible roof...', '{"es": "Gran Árbol", "pt-br": "Árvore de Natal Rara"}', '{"es": "¡Qué buena pinta tiene!", "pt-br": "Faça um pedido quando ela brilhar."}', 'multistate', 'animated', 'toggle', '[0, 1, 2]', 0, 0, 0, 0, 1, 1),
  ROW(2598, 's', 3724, 'qt_xm10_elephant', 1, 1, 1.0, 'Elephant Minibar', 'Now serving icy cool drinks!', '{"es": "Elefante Congelado", "pt-br": "Elefante Gelado"}', '{"es": "En Bollywood también hace frío", "pt-br": "A melhor água gelada da região."}', 'multistate', 'animated', 'toggle', '[0, 1]', 0, 0, 0, 0, 0, 1),
  ROW(2599, 's', 3725, 'es_wpost', 1, 1, 1.0, 'City Hydrant', 'If Xmas gets you hot under the collar, use this.', '{"es": "Boca de Agua Roja", "pt-br": "Hidrante de Inverno"}', '{"es": "¡Ni un incendio más!", "pt-br": "Mesmo no frio, incêndios acontecem."}', 'multistate', 'animated', 'toggle', '[0, 1]', 0, 0, 0, 0, 1, 1),
  ROW(2600, 's', 3726, 'qt_xm10_gnome', 1, 1, 1.0, 'Garden Gnome', 'Someone left him out in the cold for too long!', '{"es": "Gnomo Helado", "pt-br": "Gnomano Nórdico"}', '{"es": "Pequeño y congelado", "pt-br": "Um tchauzinho friorento pra você também."}', 'multistate', 'animated', 'toggle', '[0, 1]', 0, 0, 0, 0, 1, 1),
  ROW(2601, 's', 3727, 'qt_xm10_iceduck', 1, 1, 1.0, 'Plain Icy Duck', 'It''s quack-tastic!', '{"es": "Pato Helado", "pt-br": "Pato de Gelo"}', '{"es": "Fue un momento a la nevera y...", "pt-br": "Lindo bichinho de cristal."}', 'basic', 'animated', 'none', '[0, 1]', 0, 0, 0, 0, 0, 1),
  ROW(2602, 's', 3728, 'qt_xm10_icetiger', 2, 1, 1.35, 'Ice of the Tiger', 'It''s the cream of the fight.', '{"es": "Tigre Helado", "pt-br": "Tigre Congelado"}', '{"es": "Es lo que tiene el mal tiempo", "pt-br": "Para guardar fortes de gelo."}', 'basic', 'animated', 'none', '[0, 1]', 1, 0, 0, 0, 0, 1),
  ROW(2603, 's', 3729, 'es_fnc_1', 1, 1, 1.0, 'City Street Barrier', 'Dodge the crowds.', '{"es": "Mini Valla de Piedra", "pt-br": "Quadrado Nevado"}', '{"es": "Fría como un témpano de hielo", "pt-br": "Quatro lados iguais do mais puro gelo sobre cimento."}', 'multistate', 'animated', 'toggle', '[0, 1]', 0, 0, 0, 0, 1, 1),
  ROW(2604, 's', 3730, 'qt_xm10_iceplasto', 1, 1, 1.0, 'Pod Chair', 'Anyone for frozen cheeks?', '{"es": "Pod Congelado", "pt-br": "Plasto Gelado"}', '{"es": "¿Seguro que quieres sentarte?", "pt-br": "Anatomicamente moldada no formato do corpo."}', 'basic', 'animated', 'none', '[0, 1]', 1, 0, 0, 0, 0, 1),
  ROW(2605, 's', 3731, 'es_icestar_g', 1, 1, 1.0E-6, 'Green Ice Star', 'Twinkle, twinkle melting star.', '{"es": "Estrella Verde", "pt-br": "Estrela Cadente Verde"}', '{"es": "¿Ya la has colgado?", "pt-br": "Do alto do céu para dentro do seu Quarto."}', 'multistate', 'animated', 'toggle', '[0, 1, 2]', 0, 0, 1, 1, 1, 1),
  ROW(2606, 's', 3732, 'qt_xm10_icelupin', 1, 1, 1.0, 'Cool Lupin', 'Needs no sun to flourish.', '{"es": "Lupino Congelado", "pt-br": "Arbusto de Gelo"}', '{"es": "Para tu jardín de invierno", "pt-br": "As flores de plástico não morrem."}', 'multistate', 'animated', 'toggle', '[0, 1, 2]', 0, 0, 0, 0, 0, 1),
  ROW(2607, 's', 3733, 'prizetrophy_nrj_2011', 1, 1, 0.0, 'NRJ Trophy (2011)', 'prizetrophy_nrj_2011 desc', '{"es": "Trofeo NRJ (2011)", "pt-br": "Troféu NRJ (2011)"}', '{"es": "¡Has nacido para estar sobre el escenario!", "pt-br": "prizetrophy_nrj_2011 desc"}', 'trophy', 'static', 'none', NULL, 0, 0, 0, 0, 1, 1),
  ROW(2608, 's', 3734, 'qt_xm10_iceteddy', 1, 1, 0.9, 'Frozen Ted', 'Cold yet cuddly.', '{"es": "Osito Teddy Helado", "pt-br": "Urso Teddy Gelado"}', '{"es": "¿Quién lo habrá dejado a la intemperie?", "pt-br": "Um carinho geladinho."}', 'basic', 'animated', 'none', '[0, 1]', 1, 0, 0, 0, 0, 1),
  ROW(2609, 's', 3735, 'es_lmppst', 1, 1, 3.0, 'Street Lamp Post', 'The pretty city lights.', '{"es": "Farola victoriana", "pt-br": "Lâmpada de Rua"}', '{"es": "¿A que es monísima?", "pt-br": "Sob a luz do candeeiro, uma noite especial."}', 'multistate', 'animated', 'toggle', '[0, 1]', 0, 0, 0, 0, 1, 1),
  ROW(2610, 's', 3736, 'es_skating_ice', 2, 2, 0.0, 'Ice Skating Patch', 'Walk on it to receive your blades of glory!', '{"es": "Hielo Resbaladizo", "pt-br": "Piso de Esqui"}', '{"es": "Cuidado no te caigas", "pt-br": "Cuidado para não escorregar."}', 'basic', 'static', 'icetag_skating_patch', '[0]', 0, 0, 1, 1, 1, 1),
  ROW(2611, 's', 3737, 'qt_xm10_icepillar', 1, 1, 3.5, 'Frozen Pillar', 'Won''t crumble... might melt though.', '{"es": "Pilar Helado", "pt-br": "Pilar de Neve"}', '{"es": "Y no se resquebraja, oiga...", "pt-br": "Formando as estruturas do seu iglu particular."}', 'basic', 'animated', 'none', '[0, 1]', 0, 0, 0, 1, 1, 1),
  ROW(2612, 's', 3738, 'es_roaster', 1, 1, 1.0, 'Chestnut Roaster', 'You''d be nuts not to want one!', '{"es": "Castañas Asadas", "pt-br": "Grelha de Assar"}', '{"es": "¡Cuida que no se quemen!", "pt-br": "Uma castanha assada, que tal?"}', 'multistate', 'animated', 'toggle', '[0, 1]', 0, 0, 0, 0, 1, 1),
  ROW(2613, 's', 3739, 'es_puck', 1, 1, 0.2, 'Ice Hockey Puck', 'Take to the ice!', '{"es": "Pastilla de Hockey", "pt-br": "Puck"}', '{"es": "¡Apunta bien!", "pt-br": "É tipo a bola do jogo de hóquei."}', 'pushable', 'static', 'football', '[0]', 0, 0, 1, 0, 1, 1),
  ROW(2614, 's', 3740, 'es_santa', 1, 1, 1.0, 'Santa', 'Ho, Ho, Ho Habbos!', '{"es": "Santa", "pt-br": "João-Bobo Noel"}', '{"es": "¡Con su aspecto bonachón!", "pt-br": "Papai Noel, Pai Natal, Santa Claus, Ho Ho Ho."}', 'multistate', 'animated', 'toggle', '[0, 1, 2, 3]', 0, 0, 0, 0, 1, 1),
  ROW(2615, 's', 3741, 'es_tagging', 1, 1, 0.0, 'Tag Pole', 'You''re it!', '{"es": "Luz de salida", "pt-br": "Luz de saída"}', '{"es": "¡Comienza el juego!", "pt-br": "O jogo vai começar!"}', 'multistate', 'animated', 'icetag_tag_pole', '[0]', 0, 0, 0, 0, 1, 1),
  ROW(2616, 's', 3742, 'qt_xm10_nest', 1, 1, 0.2, 'Frozen Nest', 'Perfect for your Polar Bear.', '{"es": "Cesta Helada", "pt-br": "Ninho Frio"}', '{"es": "No es precisamente un dulce hogar", "pt-br": "Aconchegantemente gelado."}', 'basic', 'static', 'none', NULL, 0, 0, 1, 0, 0, 1),
  ROW(2617, 's', 3743, 'qt_xm10_icesilotable', 1, 1, 1.0, 'Chilly Table', 'Dinner''s ready! Get it while it''''s... cold?', '{"es": "Mesa Plastic Helada", "pt-br": "Mesa de Ocasião Fria"}', '{"es": "Decorando el exterior", "pt-br": "Vamos quebrar o gelo dessa conversa."}', 'basic', 'animated', 'none', '[0, 1]', 0, 0, 0, 1, 0, 1),
  ROW(2618, 's', 3744, 'petfood16', 1, 1, 0.0, 'Popsicles', 'The perfect pet treat!', '{"es": "Polos Helados", "pt-br": "Picolés"}', '{"es": "¡El trato perfecto para una Mascota! (Energía Baja)", "pt-br": "Fresco e gostoso (130 nutrientes)"}', 'multistate', 'animated', 'petfood', '[0, 1, 2, 3, 4]', 0, 0, 1, 0, 0, 0),
  ROW(2619, 's', 3745, 'es_icestar_r', 1, 1, 1.0E-6, 'Red Ice Star', 'Twinkle, twinkle melting star.', '{"es": "Estrella Roja", "pt-br": "Estrela Cadente Vermelha"}', '{"es": "¿Ya la has colgado?", "pt-br": "Do alto do céu para dentro do seu Quarto."}', 'multistate', 'animated', 'toggle', '[0, 1, 2]', 0, 0, 1, 1, 1, 1),
  ROW(2620, 's', 3746, 'qt_xm10_icedragon', 1, 1, 1.0, 'Blizzard Dragon', 'With a fiery breath.', '{"es": "Dragón Helado", "pt-br": "Lâmpada do Dragão de Gelo"}', '{"es": "¡Y aún es capaz de sacar una llamarada!", "pt-br": "Uma nova raridade presente no Hotel."}', 'multistate', 'animated', 'toggle', '[0, 1]', 0, 0, 0, 0, 1, 1),
  ROW(2621, 's', 3747, 'qt_xm10_stone', 1, 2, 1.06, 'Stone', 'Build your ice fortress.', '{"es": "Muro de Piedra Helado", "pt-br": "Muro de Gelo"}', '{"es": "¡A que no lo saltas!", "pt-br": "Seu iglu com poucos blocos."}', 'basic', 'animated', 'none', '[0, 1]', 0, 0, 0, 1, 0, 1),
  ROW(2622, 's', 3748, 'qt_xm10_ice_fish', 1, 2, 1.0, 'Frozen Fish', 'It''s the catch of the day!', '{"es": "Pescado Helado", "pt-br": "Fonte de Peixe"}', '{"es": "¡Cualquiera le hinca el diente!", "pt-br": "Embelezando os lugares mais gelados."}', 'multistate', 'animated', 'toggle', '[0, 1]', 0, 0, 0, 0, 1, 1),
  ROW(2623, 's', 3749, 'es_icestar_y', 1, 1, 1.0E-6, 'Yellow Ice Star', 'Twinkle, twinkle melting star.', '{"es": "Estrella Amarilla", "pt-br": "Estrela Cadente Amarela"}', '{"es": "¿Ya la has colgado?", "pt-br": "Do alto do céu para dentro do seu Quarto."}', 'multistate', 'animated', 'toggle', '[0, 1, 2]', 0, 0, 1, 1, 1, 1),
  ROW(2624, 's', 3750, 'qt_xm10_bench', 2, 1, 1.2, 'Bench', 'Cool down after a hard game of Ice Hockey.', '{"es": "Banco Helado", "pt-br": "Banco de Gelo"}', '{"es": "Posaderas congeladas", "pt-br": "Pra dar uma esfriadinha na buzanfa."}', 'multistate', 'animated', 'toggle', '[0, 1]', 1, 0, 0, 0, 0, 1),
  ROW(2625, 's', 3751, 'xmas10_surprise', 1, 1, 0.0, 'Mystery Ball', 'What could it be?', '{"es": "Bola incandescente", "pt-br": "Bola misteriosa"}', '{"es": "Bola incandescente", "pt-br": "Bola misteriosa"}', 'basic', 'animated', 'none', '[0, 1]', 0, 0, 1, 1, 1, 1),
  ROW(2626, 's', 3752, 'xmas10_fireplace', 1, 1, 0.0, 'Santa''s Fireplace Teleport', 'Used by gnomes to spy on Habbos for the naughty or nice list.', '{"es": "Teleport Chimenea de Santa", "pt-br": "Teleporte Natalino"}', '{"es": "Utilizado por gnomos para espiar a los niños", "pt-br": "Usado por gnomos para espiar as crianças"}', 'multistate', 'animated', 'teleport', '[0, 1, 2]', 0, 0, 0, 0, 1, 1),
  ROW(2627, 's', 3753, 'es_geysir', 1, 1, 0.01, 'Freeze Geyser', 'Ruuun! Or be snap frozen.', '{"es": "Géiser", "pt-br": "Gêiser FREEZE!"}', '{"es": "No te acerques tanto, es peligroso...", "pt-br": "Águas termais para refrescar os jogadores"}', 'multistate', 'animated', 'toggle', '[0, 1]', 0, 0, 1, 0, 1, 1),
  ROW(2628, 's', 3754, 'xmas10_present', 1, 1, 1.0, 'Mystery Christmas Gift', 'Wonder what''s inside?...', '{"es": "Regalo Navideño misterioso", "pt-br": "Presente Misterioso de Natal"}', '{"es": "Ya te gustaría saber qué incluye...", "pt-br": "Você não imagina o que tem dentro!"}', 'basic', 'static', 'none', '[0]', 0, 0, 0, 1, 0, 0),
  ROW(2629, 's', 3755, 'qt_xm10_iceelephant', 1, 1, 1.0, 'Woah Nelly!', 'So cool it''s freezing!', '{"es": "Mamut Helado", "pt-br": "Mamute Gelado"}', '{"es": "De las frías estepas pixeladas", "pt-br": "Diga oi para a Lela"}', 'basic', 'animated', 'none', '[0, 1]', 0, 0, 0, 0, 1, 1),
  ROW(2630, 's', 3756, 'es_score_b', 1, 1, 1.0, 'Blue Freeze Scoreboard', 'Who''s winning?', '{"es": "Marcador Freeze Azul", "pt-br": "Placar Azul FREEZE!"}', '{"es": "La victoria está en los números", "pt-br": "Contando vitórias e derrotas"}', 'score', 'score_board', 'none', '[0, 1]', 0, 0, 0, 0, 1, 1),
  ROW(2631, 's', 3757, 'es_tile', 1, 1, 0.001, 'Freeze Tile', 'Got cold feet?', '{"es": "Casilla Freeze", "pt-br": "Piso de Arena"}', '{"es": "¡Pues sí que está helada!", "pt-br": "FREEZE-se!"}', 'multistate', 'animated', 'none', '[0, 1, 2, 3, 4, 6, 11]', 0, 0, 1, 1, 1, 1),
  ROW(2632, 's', 3758, 'es_box', 1, 1, 1.0E-4, 'Freeze Ice Block', 'Place on FREEZE Tile to work!', '{"es": "Bloque Freeze", "pt-br": "Bloco de Gelo"}', '{"es": "Vete a saber lo que oculta en su interior...", "pt-br": "Que tal quebrar alguns blocos FREEZE hoje?"}', 'multistate', 'animated', 'none', '[0, 1, 2, 3, 4, 5, 6, 7, 11, 12, 13, 14, 15, 16, 17]', 0, 0, 1, 0, 1, 1),
  ROW(2633, 's', 3759, 'es_exit', 1, 1, 0.03, 'Exit Tile', 'That''s enough snow-balling for one day.', '{"es": "Casilla de Salida", "pt-br": "Saída da Arena"}', '{"es": "Al acabar el juego, te teletransportarás a ella", "pt-br": "Chega de gelo por hoje."}', 'multistate', 'animated', 'none', '[0, 1]', 0, 0, 1, 0, 1, 1),
  ROW(2634, 's', 3760, 'es_gate_r', 1, 1, 0.01, 'Red Freeze Gate', 'Let the snow-balling begin!', '{"es": "Vestuario Freeze Rojo", "pt-br": "Portão Vermelho FREEZE!"}', '{"es": "¡Equípate con lo necesario!", "pt-br": "Pegue sua munição e prepare-se"}', 'multistate', 'animated', 'none', '[0, 1, 2, 3, 4, 5]', 0, 0, 1, 0, 1, 1),
  ROW(2635, 's', 3761, 'es_gate_g', 1, 1, 0.01, 'Green Freeze Gate', 'Let the snow-balling begin!', '{"es": "Vestuario Freeze Verde", "pt-br": "Portão Verde FREEZE!"}', '{"es": "¡Equípate con lo necesario!", "pt-br": "Pegue sua munição e prepare-se"}', 'multistate', 'animated', 'none', '[0, 1, 2, 3, 4, 5]', 0, 0, 1, 0, 1, 1),
  ROW(2636, 's', 3762, 'es_gate_b', 1, 1, 0.01, 'Blue Freeze Gate', 'Let the snow-balling begin!', '{"es": "Vestuario Freeze Azul", "pt-br": "Portão Azul FREEZE!"}', '{"es": "¡Equípate con lo necesario!", "pt-br": "Pegue sua munição e prepare-se"}', 'multistate', 'animated', 'none', '[0, 1, 2, 3, 4, 5]', 0, 0, 1, 0, 1, 1),
  ROW(2637, 's', 3763, 'es_gate_y', 1, 1, 0.01, 'Yellow Freeze Gate', 'Let the snow-balling begin!', '{"es": "Vestuario Freeze Amarillo", "pt-br": "Portão Amarelo FREEZE!"}', '{"es": "¡Equípate con lo necesario!", "pt-br": "Pegue sua munição e prepare-se"}', 'multistate', 'animated', 'none', '[0, 1, 2, 3, 4, 5]', 0, 0, 1, 0, 1, 1),
  ROW(2638, 's', 3764, 'es_counter', 1, 3, 1.0, 'Freeze Counter', 'It''s the final countdown!', '{"es": "Temporizador Freeze", "pt-br": "Relógio FREEZE!"}', '{"es": "Y el juego comienza en 3, 2, 1...", "pt-br": "Quanto tempo você consegue ficar congelado?"}', 'counter_clock', 'counter_clock', 'none', '[0, 1]', 0, 0, 0, 0, 1, 1),
  ROW(2639, 's', 3765, 'es_score_r', 1, 1, 1.0, 'Red Freeze Scoreboard', 'Who''s winning?', '{"es": "Marcador Freeze Rojo", "pt-br": "Placar Vermelho FREEZE!"}', '{"es": "La victoria está en los números", "pt-br": "Contando vitórias e derrotas"}', 'score', 'score_board', 'none', '[0, 1]', 0, 0, 0, 0, 1, 1),
  ROW(2640, 's', 3766, 'es_score_g', 1, 1, 1.0, 'Green Freeze Scoreboard', 'Who''s winning?', '{"es": "Marcador Freeze Verde", "pt-br": "Placar Verde FREEZE!"}', '{"es": "La victoria está en los números", "pt-br": "Contando vitórias e derrotas"}', 'score', 'score_board', 'none', '[0, 1]', 0, 0, 0, 0, 1, 1),
  ROW(2641, 's', 3767, 'es_score_y', 1, 1, 1.0, 'Yellow Freeze Scoreboard', 'Who''s winning?', '{"es": "Marcador Freeze Amarillo", "pt-br": "Placar Amarelo FREEZE!"}', '{"es": "La victoria está en los números", "pt-br": "Contando vitórias e derrotas"}', 'score', 'score_board', 'none', '[0, 1]', 0, 0, 0, 0, 1, 1),
  ROW(2642, 's', 3768, 'year2011_waver_y', 1, 1, 1.0, 'Yellow Waver', 'Be hypnotized!', '{"es": "Tubo de Aire Amarillo", "pt-br": "Tubo Amarelo"}', '{"es": "¡Invita a bailar!", "pt-br": "Toda festa precisa de um."}', 'multistate', 'animated', 'toggle', '[0, 1]', 0, 0, 0, 0, 1, 1),
  ROW(2643, 's', 3769, 'prizetrophy_2011_w', 1, 1, 1.0, 'Habbo Award Trophy', 'Congratulations for being nominated in the 2010 Habbo Awards!', '{"es": "Popular 2010", "pt-br": "O Melhor do Ano"}', '{"es": "Has recibido Trofeos de regalo como el que más", "pt-br": "Especial para os destaques de 2010."}', 'trophy', 'animated', 'none', '[0, 1]', 0, 0, 0, 0, 1, 1),
  ROW(2644, 's', 3770, 'prizetrophy_2011_r', 1, 1, 1.0, 'Hottest Girl', 'Has one lucky lady caught your eye?', '{"es": "Look 2011", "pt-br": "Troféu do Ano Vermelho"}', '{"es": "Estás preparado para marcar tendencia en el nuevo año", "pt-br": "Pra você, pra mim, pra todos nós."}', 'trophy', 'animated', 'none', '[0, 1]', 0, 0, 0, 0, 1, 1),
  ROW(2645, 's', 3771, 'prizetrophy_2011_y', 1, 1, 1.0, 'Best Friend of 2010', 'Show your BFF you care!', '{"es": "Simpatía 2011", "pt-br": "Troféu do Ano Amarelo"}', '{"es": "En 2011 no te faltarán amigos en la Comunidad Latina", "pt-br": "Pra você, pra mim, pra todos nós."}', 'trophy', 'animated', 'none', '[0, 1]', 0, 0, 0, 0, 1, 1),
  ROW(2646, 's', 3772, 'fireworks_01', 1, 1, 1.0, 'Green Firework Blaster', 'KABOOM!', '{"es": "Fuegos Artificiales Verdes", "pt-br": "Fogos de Artifício 1"}', '{"es": "¡Bonito espectáculo!", "pt-br": "No céu, uma luz para nos mostrar que há esperança."}', 'multistate', 'animated', 'firework', '[0, 1, 2]', 0, 0, 0, 0, 1, 1),
  ROW(2647, 's', 3773, 'prizetrophy_2011_b', 1, 1, 1.0, 'Hottest Guy', 'His pixels make you melt.', '{"es": "Trofeo Habbo 2011", "pt-br": "Troféu do Ano Azul"}', '{"es": "Hay quien piensa que has sido el mejor miembro de la Comunidad durante todo un año", "pt-br": "Pra você, pra mim, pra todos nós."}', 'trophy', 'animated', 'none', '[0, 1]', 0, 0, 0, 0, 1, 1),
  ROW(2648, 's', 3774, 'fireworks_03', 1, 1, 1.0, 'Yellow Firework Blaster', 'Let the celebrations begin!', '{"es": "Fuegos Artificiales Amarillos", "pt-br": "Fogos de Artifício 3"}', '{"es": "¡Bonito espectáculo!", "pt-br": "No céu, uma luz para nos mostrar que há esperança."}', 'multistate', 'animated', 'firework', '[0, 1, 2]', 0, 0, 0, 0, 1, 1),
  ROW(2649, 's', 3775, 'prizetrophy_2011_p', 1, 1, 1.0, 'Most Fashionable Habbo', 'Who''s on your best dressed list?', '{"es": "Creador de Juegos 2010", "pt-br": "Troféu do Ano Rosa"}', '{"es": "Gracias por esos juegos tan divertidos", "pt-br": "Pra você, pra mim, pra todos nós."}', 'trophy', 'animated', 'none', '[0, 1]', 0, 0, 0, 0, 1, 1),
  ROW(2650, 's', 3776, 'fireworks_02', 1, 1, 1.0, 'Red Firework Blaster', '3... 2... 1... Happy New Year!', '{"es": "Fuegos Artificiales Rojos", "pt-br": "Fogos de Artifício 2"}', '{"es": "¡Bonito espectáculo!", "pt-br": "No céu, uma luz para nos mostrar que há esperança."}', 'multistate', 'animated', 'firework', '[0, 1, 2]', 0, 0, 0, 0, 1, 1),
  ROW(2651, 's', 3777, 'year2011_waver_r', 1, 1, 1.0, 'Red Waver', 'Be hypnotized!', '{"es": "Tubo de Aire Rojo", "pt-br": "Tubo Vermelho"}', '{"es": "¡Invita a bailar!", "pt-br": "Toda festa precisa de um."}', 'multistate', 'animated', 'toggle', '[0, 1]', 0, 0, 0, 0, 1, 1),
  ROW(2652, 's', 3778, 'prizetrophy_2011_g', 1, 1, 1.0, 'Best Room Builder', 'Whose rooms do you admire most?', '{"es": "Creador de Salas 2010", "pt-br": "Troféu do Ano Verde"}', '{"es": "Tus Salas gustan, que lo sepas", "pt-br": "Pra você, pra mim, pra todos nós."}', 'trophy', 'animated', 'none', '[0, 1]', 0, 0, 0, 0, 1, 1),
  ROW(2653, 's', 3779, 'fireworks_05', 1, 1, 1.0, 'Big Bang Bug Sparkler', 'Light up the sky!', '{"es": "Fuegos Artificiales Mosca", "pt-br": "Mosca de Fogo"}', '{"es": "¡Bonito espectáculo!", "pt-br": "No céu, uma luz para nos mostrar que há esperança."}', 'multistate', 'animated', 'firework', '[0, 1, 2]', 0, 0, 0, 0, 1, 1),
  ROW(2654, 's', 3780, 'year2011_waver_g', 1, 1, 1.0, 'Green Waver', 'Be hypnotized!', '{"es": "Tubo de Aire Verde", "pt-br": "Tubo Verde"}', '{"es": "¡Invita a bailar!", "pt-br": "Toda festa precisa de um."}', 'multistate', 'animated', 'toggle', '[0, 1]', 0, 0, 0, 0, 1, 1),
  ROW(2655, 's', 3781, 'year2011_waver_b', 1, 1, 1.0, 'Blue Waver', 'Be hypnotized!', '{"es": "Tubo de Aire Azul", "pt-br": "Tubo Azul"}', '{"es": "¡Invita a bailar!", "pt-br": "Toda festa precisa de um."}', 'multistate', 'animated', 'toggle', '[0, 1]', 0, 0, 0, 0, 1, 1),
  ROW(2656, 's', 3782, 'fireworks_07', 1, 1, 1.0, 'Nuclear Firework', 'Seriously, you might want to take cover.', '{"es": "El Gran Cohete", "pt-br": "Foguete de Luz"}', '{"es": "¡¡Arribaaaa!!", "pt-br": "Vai decolar em 10, 9, 8, 7..."}', 'multistate', 'animated', 'firework', '[0, 1, 2]', 0, 0, 0, 0, 1, 1),
  ROW(2657, 's', 3783, 'fireworks_06', 1, 1, 1.0, 'Bee Buster Blaster', 'Light up the sky!', '{"es": "Abeja de Fuego", "pt-br": "Fogo de Abelha"}', '{"es": "Cuidado que pica", "pt-br": "Zumbizando em seu ouvido."}', 'multistate', 'animated', 'firework', '[0, 1, 2]', 0, 0, 0, 0, 1, 1),
  ROW(2658, 's', 3784, 'fireworks_04', 1, 1, 1.0, 'Purple Fireworks Blaster', 'Let the celebrations begin!', '{"es": "Fuegos Artificiales Morados", "pt-br": "Fogos de Artifício 4"}', '{"es": "¡Qué bonito!", "pt-br": "Um momento mais que especial em seu Quarto."}', 'multistate', 'animated', 'firework', '[0, 1, 2]', 0, 0, 0, 0, 1, 1),
  ROW(2659, 'i', 4352, 'ads_nokiax3_wall2', NULL, NULL, 0.1, 'Nokia Lights 2', 'Touch and Type - super slim, super sleek!', '{"es": "Luces Nokia 2", "pt-br": "Luzes Nokia 2"}', '{"es": "Touch and Type: súper fino y súper elegante.", "pt-br": "Touch and Type - superfino e superelegante!"}', 'basic', 'static', 'none', '[0]', 0, 0, 0, 0, 1, 1),
  ROW(2660, 'i', 4353, 'ads_nokiax3_wall3', NULL, NULL, 0.1, 'Nokia Lights 3', 'Touch and Type - super slim, super sleek!', '{"es": "Luces Nokia 3", "pt-br": "Luzes Nokia 3"}', '{"es": "Touch and Type: súper fino y súper elegante.", "pt-br": "Touch and Type - superfino e superelegante!"}', 'basic', 'static', 'none', '[0]', 0, 0, 0, 0, 1, 1),
  ROW(2661, 'i', 4354, 'ads_nokiax3_wall1', NULL, NULL, 0.1, 'Nokia Lights', 'Touch and Type - super slim, super sleek!', '{"es": "Luces Nokia", "pt-br": "Luzes Nokia"}', '{"es": "Touch and Type: súper fino y súper elegante.", "pt-br": "Touch and Type - superfino e superelegante!"}', 'basic', 'static', 'none', '[0]', 0, 0, 0, 0, 1, 1),
  ROW(2662, 'i', 4355, 'ads_nokiax3_phn', NULL, NULL, 0, 'Nokia X3', 'Check out the latest, super slim Nokia X3 Touch and Type design range!', '{"es": "Nokia X3", "pt-br": "Nokia X3"}', '{"es": "¡Descubre la nueva gama Nokia X3 Touch and Type, súper fina!", "pt-br": "Confira a nova linha Nokia X3 Touch and Type, superfina!"}', 'basic', 'static', 'none', '[0]', 0, 0, 0, 0, 1, 1),
  ROW(2663, 'i', 4356, 'exe_sensor', NULL, NULL, 0.87, 'Security Sensor', 'Don''t move!', '{"es": "Sensor", "pt-br": "Sensor de Movimento"}', '{"es": "Indispensable para una buena vigilancia", "pt-br": "Só precisa de um passo"}', 'multistate', 'animated', 'toggle', '[0, 1]', 0, 0, 0, 0, 1, 1),
  ROW(2664, 'i', 4357, 'qt_calendar2', NULL, NULL, 1, 'Blue Calendar', 'Double click to start your quests!', '{"es": "Calendario de Nieve", "pt-br": "Calendário Nevado"}', '{"es": "Da frío sólo con verlo", "pt-br": "Cada dia uma nova aventura."}', 'multistate', 'animated', 'none', '[0, 1]', 0, 0, 0, 0, 0, 0),
  ROW(2665, 'i', 4358, 'ads_r7logo', NULL, NULL, 0.1, 'ads_r7logo name', 'ads_r7logo desc', '{"es": "ads_r7logo name", "pt-br": "R7"}', '{"es": "ads_r7logo desc", "pt-br": "www.R7.com"}', 'basic', 'static', 'none', '[0]', 0, 0, 0, 0, 0, 1),
  ROW(2666, 'i', 4359, 'qt_calendar0', NULL, NULL, 1, 'Green Calendar', 'Double click to start your quests!', '{"es": "Calendario Verde", "pt-br": "Calendário Verde"}', '{"es": "¡Haz doble clic para empezar tus retos!", "pt-br": "Dê um duplo clique para começar suas tarefas!"}', 'multistate', 'animated', 'none', '[0, 1]', 0, 0, 0, 0, 0, 0),
  ROW(2667, 'i', 4360, 'qt_calendar1', NULL, NULL, 1, 'Santa Calendar', 'Double click to start your quests!', '{"es": "Calendario Santa", "pt-br": "Calendário Noel"}', '{"es": "¿Qué habrá preparado?", "pt-br": "Um ho-ho-ho pro seu dia-a-dia."}', 'multistate', 'animated', 'none', '[0, 1]', 0, 0, 0, 0, 0, 0),
  ROW(2668, 'i', 4361, 'qt_xm10_bauble2', NULL, NULL, 1, 'Icy Bauble 2', 'Will give you the chills.', '{"es": "Adorno Helado 2", "pt-br": "Enfeite Natalino"}', '{"es": "¡Feliz Navidad!", "pt-br": "Congelando coraçoes, esquentando sentimentos."}', 'basic', 'animated', 'none', '[0, 1]', 0, 0, 0, 0, 0, 1),
  ROW(2669, 'i', 4362, 'es_build1', NULL, NULL, 1, 'Skyscraper 1', 'Talk about an ''Empire State of Mind''.', '{"es": "Rascacielos 1", "pt-br": "Paisagem Urbana 1"}', '{"es": "¡No apto para personas con vértigo!", "pt-br": "Uma vista concreta e contemporânea."}', 'multistate', 'animated', 'toggle', '[0, 1]', 0, 0, 0, 0, 1, 1),
  ROW(2670, 'i', 4363, 'es_build4', NULL, NULL, 1, 'Skyscraper 4', 'Heading to the big smoke?', '{"es": "Rascacielos 4", "pt-br": "Paisagem Urbana 4"}', '{"es": "¡No apto para personas con vértigo!", "pt-br": "Uma vista concreta e contemporânea."}', 'multistate', 'animated', 'toggle', '[0, 1]', 0, 0, 0, 0, 1, 1),
  ROW(2671, 'i', 4364, 'es_build2', NULL, NULL, 1, 'Skyscraper 2', 'Create your concrete jungle.', '{"es": "Rascacielos 2", "pt-br": "Paisagem Urbana 2"}', '{"es": "¡No apto para personas con vértigo!", "pt-br": "Uma vista concreta e contemporânea."}', 'multistate', 'animated', 'toggle', '[0, 1]', 0, 0, 0, 0, 1, 1),
  ROW(2672, 'i', 4365, 'qt_xm10_bauble1', NULL, NULL, 1, 'Icy Bauble 1', 'Frozen festive Furni.', '{"es": "Adorno Helado 1", "pt-br": "Bolinha de Árvore"}', '{"es": "¡Feliz Navidad!", "pt-br": "Um toque gelado na sua decoração."}', 'basic', 'animated', 'none', '[0, 1]', 0, 0, 0, 0, 0, 1),
  ROW(2673, 'i', 4366, 'es_build3', NULL, NULL, 1, 'Skyscraper 3', 'Look up in awe.', '{"es": "Rascacielos 3", "pt-br": "Paisagem Urbana 3"}', '{"es": "¡No apto para personas con vértigo!", "pt-br": "Uma vista concreta e contemporânea."}', 'multistate', 'animated', 'toggle', '[0, 1]', 0, 0, 0, 0, 1, 1),
  ROW(2674, 'i', 4367, 'es_window', NULL, NULL, 3.15, 'Shop Window', 'When you''re broke there''s always window shopping.', '{"es": "Escaparate de Navidad", "pt-br": "Vitrine Natalina"}', '{"es": "Quiero eso, y eso, y eso otro...", "pt-br": "Uma paisagem inspiradora, sem dúvida."}', 'multistate', 'animated', 'toggle', '[0, 1, 2, 3, 4]', 0, 0, 0, 0, 1, 1),
  ROW(2675, 'i', 4368, 'qt_xm10_bauble3', NULL, NULL, 1, 'Icy Bauble 3', 'A gift worth giving.', '{"es": "Adorno Helado 3", "pt-br": "Decoração Congelada"}', '{"es": "¡Feliz Navidad!", "pt-br": "É no Natal gelado que a gente se aquece."}', 'basic', 'animated', 'none', '[0, 1]', 0, 0, 0, 0, 0, 1),
  ROW(2676, 'i', 4369, 'qt_xm10_icestar', NULL, NULL, 1, 'Ice Star', 'Keeping the nights even cooler.', '{"es": "Estrella Helada", "pt-br": "Estrela de Neve"}', '{"es": "Deseos congelados", "pt-br": "Não é tempestade de neve, é chuva de estrelas!"}', 'basic', 'animated', 'none', '[0, 1]', 0, 0, 0, 0, 0, 1),
  ROW(2677, 'i', 4370, 'es_big1', NULL, NULL, 1, 'City Building 1', 'Enter the party here', '{"es": "Gran Rascacielos 1", "pt-br": "Cidade Grande 1"}', '{"es": "¡Menudas vistas!", "pt-br": "Uma construção humana que busca chegar ao céu."}', 'basic', 'static', 'none', '[0]', 0, 0, 0, 0, 1, 1),
  ROW(2678, 'i', 4371, 'es_big3', NULL, NULL, 1, 'City Building 3', 'Make your way up to the party', '{"es": "Gran Rascacielos 3", "pt-br": "Cidade Grande 3"}', '{"es": "¡Menudas vistas!", "pt-br": "Uma construção humana que busca chegar ao céu."}', 'basic', 'static', 'none', '[0]', 0, 0, 0, 0, 1, 1),
  ROW(2679, 'i', 4372, 'es_big2', NULL, NULL, 1, 'City Building 2', 'Best view of the Fireworks', '{"es": "Gran Rascacielos 2", "pt-br": "Cidade Grande 2"}', '{"es": "¡Menudas vistas!", "pt-br": "Uma construção humana que busca chegar ao céu."}', 'basic', 'static', 'none', '[0]', 0, 0, 0, 0, 1, 1),
  ROW(2680, 'i', 4373, 'year2011', NULL, NULL, 0.35, 'Countdown Building', 'Bring in 2011!', '{"es": "Año 2011", "pt-br": "Painel 2011"}', '{"es": "¡Ya no queda nada!", "pt-br": "O registro do ano novo mais aguardado do mundo."}', 'multistate', 'animated', 'toggle', '[0, 1]', 0, 0, 0, 0, 1, 1)
) n (`id`, `type`, `ref_furnidata`, `classname`, `x_dim`, `y_dim`, `z_dim`, `localization_name`, `localization_desc`, `localized_name`, `localized_desc`, `logic_class`, `visualization_class`, `interaction_logic`, `states`, `is_sittable`, `is_bed`, `is_walkable`, `is_stackable`, `allow_trade`, `allow_recycle`)
WHERE NOT EXISTS (SELECT 1 FROM `definition_furniture` f WHERE f.`type` = n.`type` AND f.`classname` = n.`classname`);

INSERT IGNORE INTO `definition_pet_food` (`furni_def_ref`, `portions`, `full_state`, `nutrition`, `energy`, `experience`)
SELECT `id`, 5, 0, 400, 0, 0 FROM `definition_furniture` WHERE `type` = 's' AND `classname` = 'petfood16';

CREATE TABLE `definition_fireworks` (
  `furni_def_ref` int unsigned NOT NULL,
  `price_credits` int unsigned NOT NULL DEFAULT 0,
  `price_activity_points` int unsigned NOT NULL DEFAULT 20,
  `activity_point_type` tinyint unsigned NOT NULL DEFAULT 0,
  `charge_amount` int unsigned NOT NULL DEFAULT 10,
  `launch_duration_ms` int unsigned NOT NULL DEFAULT 7000,
  PRIMARY KEY (`furni_def_ref`),
  CONSTRAINT `fk_definition_fireworks_furni` FOREIGN KEY (`furni_def_ref`) REFERENCES `definition_furniture` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT IGNORE INTO `definition_fireworks` (`furni_def_ref`, `price_credits`, `price_activity_points`, `activity_point_type`, `charge_amount`, `launch_duration_ms`)
SELECT f.`id`, 0, 20, 0, 10, d.`ms`
FROM (VALUES
    ROW('fireworks_01', 2500),
    ROW('fireworks_02', 7600),
    ROW('fireworks_03', 5000),
    ROW('fireworks_04', 4300),
    ROW('fireworks_05', 4300),
    ROW('fireworks_06', 4900),
    ROW('fireworks_07', 10700)
  ) d (`classname`, `ms`)
JOIN `definition_furniture` f ON f.`type` = 's' AND f.`classname` = d.`classname`;

INSERT INTO `definition_catalog_page_header` (`id`, `parent_id`, `color`, `icon`, `display_name`, `coming_soon`, `enabled`, `min_rank`, `order_num`)
SELECT * FROM (VALUES
  ROW(16, -1, 1, 80, '{"en": "Wired", "es": "Wired", "pt-br": "Wired"}', 0, 1, 1, 7),
  ROW(19, -1, 5, 51, '{"en": "Holidays 2010", "es": "Navidades 2010", "pt-br": "Natal 2010"}', 0, 0, 0, 1),
  ROW(143, 14, 0, 87, '{"en": "Freeze!", "es": "Freeze!", "pt-br": "Freeze!"}', 0, 1, 1, 3),
  ROW(144, 14, 11, 88, '{"en": "Ice Tag", "es": "Tú la llevas", "pt-br": "Pega-Pega"}', 0, 1, 1, 4),
  ROW(161, 16, 0, 84, '{"en": "Deals", "es": "Series", "pt-br": "Pacotes"}', 0, 1, 1, 0),
  ROW(162, 16, 0, 81, '{"en": "Triggers", "es": "Causantes", "pt-br": "Ativadores"}', 0, 1, 1, 1),
  ROW(163, 16, 0, 82, '{"en": "Effects", "es": "Efectos", "pt-br": "Efeitos"}', 0, 1, 1, 2),
  ROW(164, 16, 0, 83, '{"en": "Conditions", "es": "Condiciones", "pt-br": "Condições"}', 0, 1, 1, 3),
  ROW(165, 16, 0, 85, '{"en": "Add-Ons", "es": "Complementos", "pt-br": "Extras"}', 0, 1, 1, 4),
  ROW(264, 19, 0, 86, '{"en": "Snowflake Gifts", "es": "Furni Nevado", "pt-br": "Flocos de Neve"}', 0, 1, 0, 0),
  ROW(265, 19, 0, 90, '{"en": "Christmas 2010", "es": "Navidad 2010", "pt-br": "Natal 2010"}', 0, 1, 0, 1),
  ROW(266, 19, 3, 91, '{"en": "New Years", "es": "Año Nuevo", "pt-br": "Ano Novo"}', 0, 1, 0, 2),
  ROW(267, 19, 0, 60, '{"en": "New Years'' Trophies", "es": "Trofeos de Año Nuevo", "pt-br": "Troféus de Ano Novo"}', 0, 1, 0, 3)
) n (`id`, `parent_id`, `color`, `icon`, `display_name`, `coming_soon`, `enabled`, `min_rank`, `order_num`)
WHERE NOT EXISTS (SELECT 1 FROM `definition_catalog_page_header` h WHERE h.`id` = n.`id`);

INSERT INTO `definition_catalog_page_body` (`ref_page_id`, `layout_code`, `localization_images`, `localization_texts`)
SELECT * FROM (VALUES
  ROW(16, 'pets2', '{"en": ["catalog_wired_header1", "ctlg_pic_teaser_wired"]}', '{"en": ["Wired Furni lets you make your Furni do almost anything by learning how to use Triggers, Effects and Conditions.", "How do the Wired Furni work? ", "To get Wired you will need three types of Wired Furni:\\n\\n1. Triggers: Things you need to do, to make an Effect happen.\\n\\n2. Effects: Things that happen, after you activate a Trigger.\\n\\n3. Conditions (optional): The conditions that need to exist, before the Trigger works.\\n", "Each of these can be programmed separately by double clicking on them. The possibilities are almost unlimited!"], "es": ["Los Furni Wired permiten que un Furni haga casi cualquier cosa una vez aprendes cómo manejar sus Causantes, Efectos y Condiciones.", "¿Cómo funcionan?", "Para utilizarlos necesitas tres tipos de Furnis:\\n\\n1. Los Causantes: necesitas activarlos para que un Efecto aparezca.\\n\\n2. Los Efectos: es lo que aparece cuando activas un Causante.\\n\\n3. Condiciones (opcional): las condiciones que necesitas que existan antes de que un Causante funcione.", "Se pueden programar por separado haciendo doble clic sobre ellos. ¡Las posibilidades son infinitas!"], "pt-br": ["Mobis Wired lhe dão infinitas possibilidades - você pode programar seus Mobis para fazerem qualquer coisa aprendendo a utilizar os Ativadores, Efeitos e Condições.", "Como funciona?", "Para tornar seu Quarto Wired você precisa conhecer os Mobis:\\n\\n1. Ativadores: coisas que você precisa fazer para um Efeito funcionar.\\n\\n2. Efeitos: coisas que acontecem quando você liga um Ativador.\\n\\n3. Condições (opcionais): as condições necessárias para o Ativador funcionar.", "Cada um destes pode ser programado separadamente. Empilhe seu Ativador com os Efeitos e Condições e eles transformarão o seu Quarto!", ""]}'),
  ROW(19, 'default_3x3', '{}', '{}'),
  ROW(143, 'default_3x3', '{"en": ["catalog_freeze_en", "ctlg_teaser_es"]}', '{"en": ["Freeze! Try the newest game to hit Habbo! Team up and plant snowball bombs to snap freeze your competitors. You could say it''s so cool, it''s frozen ;)", "Click an item for details."], "es": ["Seguro que después de participar en este juego ya no te gusta tanto la nieve... Compite por equipos y consigue que impacten el mayor número de bolas de nieve posibles sobre tus contrincantes. ¡Son como auténticas bombas!", ""], "pt-br": ["Seja bem vindo ao mais novo jogo do Habbo Hotel! FREEZE é uma disputa de até quatro times onde apenas os mais inteligentes e rápidos vencerão!", ""]}'),
  ROW(144, 'default_3x3', '{"en": ["catalog_tag_general_1", "catalog_teaser_xm10_skate", "catalog_special_txtbg2"], "es": ["catalog_tag_br", "catalog_teaser_xm10_skate", ""], "pt-br": ["catalog_tag_br", null, ""]}', '{"en": ["The fun is ON with our new game! Skate on ice and play Tag with your friends using the new Tag Pole and Ice Skating Patch! Not sure how to put on your skates? Step on an Ice Patch and you''re good to go.", null, "Freeze! You''re it!"], "es": ["Crea tu propio juego de ''tú la llevas'', ''pega-pega'' o ''pilla-pilla''. Para ponerte los patines basta con pisar el Hielo, pero para el juego, necesitas que también haya una Luz de Salida sobre el Hielo.", null, ""], "pt-br": ["A diversão começou com nosso novo jogo! Skate no Gelo e Pega-Pega usando os novos mobis! Não sabe como usar o Skate? É só ficar em cima do Gelo e pronto!", null, ""]}'),
  ROW(161, 'default_3x3', '{"en": ["catalog_wired_header6_en", "ctlg_pic_wired_deals"], "es": ["catalog_wired_header6_es"], "pt-br": ["catalog_wired_header6_pt"]}', '{"en": ["The easiest way to get wired is to use the deals! These deals contain all Furni needed to make specific actions like ''''Password doors'''', and ''''Switch operated doors'''' possible.", "Click on a deal to see what''s inside!"], "es": ["¡La forma más fácil de usar Wired es utilizando las Series! Contienen todos los Furni necesarios para crear acciones específicas, como \\"puertas con contraseña\\" o \\"puertas a control remoto\\".", "Haz clic en una Serie para ver qué contiene."], "pt-br": ["A maneira mais fácil de se conectar é utilizando estes pacotes! Eles contém todos os Mobis necessários para fazer coisas como \\"Portas como semhas\\" e \\"portas controladas remotamente\\".", "Clique em um Pacote e descubra o que tem dentro!"]}'),
  ROW(162, 'default_3x3_extrainfo', '{"en": ["catalog_wired_header2_en", "ctlg_pic_wired_triggers"], "es": ["catalog_wired_header2_es"], "pt-br": ["catalog_wired_header2_pt"]}', '{"en": ["Triggers let you define what needs to happen for an Effect to take place. To program a Trigger, place it in a room, double click on it and set it up! You need to stack an Effect with the Trigger to make it work."], "es": ["Los Causantes permiten definir qué se necesita que pase para que tenga lugar un efecto. Para programar un Causante, colócalo en una Sala, haz doble clic en él y ponlo en marcha. Necesitarás apilar un Efecto sobre un Causante.", ""], "pt-br": ["Ativadores lhe permitem definir ligar os Efeitos no seu Quarto. Para programar un Ativador, coloque-o no Quarto, dê dois cliques e está pronto! Você precisiará combiná-lo com um Efeito.", "Clique em um item para ver como o Ativador funciona!"]}'),
  ROW(163, 'default_3x3_extrainfo', '{"en": ["catalog_wired_header3_en", "ctlg_pic_wired_effects"], "es": ["catalog_wired_header3_es"], "pt-br": ["catalog_wired_header3_pt"]}', '{"en": ["Effects are what happens after you activate a Trigger. To program an Effect, place it in your room, double click on it and set it up! You will always need to stack a Trigger with the Effect to make it work."], "es": ["Los Efectos son lo que sucede después de haber activado un Causante. Para programar un Efecto, colócalo en una Sala, haz doble clic en él y ponlo en marcha. Necesitarás siempre apilar un Causante sobre un Efecto.", ""], "pt-br": ["Efeitos são o resultado de quando você liga um Ativador. Para programar um Efeito, coloque-o no Quarto, dê dois cliques e divirta-se! Você sempre precisará juntar um Ativador com um Efeito para fazer funcionar.", "Clique em um item para ver o que o Efeito faz!"]}'),
  ROW(164, 'default_3x3_extrainfo', '{"en": ["catalog_wired_header4_en", "ctlg_pic_wired_conditions"], "es": ["catalog_wired_header4_es"], "pt-br": ["catalog_wired_header4_pt"]}', '{"en": ["Conditions are specific things that need to be done before a Trigger will work. If you want to use a Condition, you will need to stack it with a Trigger and Effect."], "es": ["Las Condiciones son determinados criterios que deben ser especificados antes de que el Causante se ponga en marcha. Si quieres usar una Condición, deberás apilarla sobre un Causante y un Efecto.", ""], "pt-br": ["Condições são certos critérios que precisam funcionar para um Ativador ligar o Efeito. Para utilizar uma Condicão você precisará juntá-la com um Ativador e um Efeito.", "Clique em um item para ver como a Condição funciona!"]}'),
  ROW(165, 'default_3x3_extrainfo', '{"en": ["catalog_wired_header5_en", "ctlg_pic_wired_support_furniture"], "es": ["catalog_wired_header5_es"], "pt-br": ["catalog_wired_header5_pt"]}', '{"en": ["Add-Ons are a special type of Wired that is used to add extra configurations to an existing Wired stack. Each Add-On has its own unique purpose."], "es": ["Los complementos son un tipo de Wired especial. Se usan para añadir configuraciones extra a una pila de Wired que hayas montado. Cada complemento tiene su propósito concreto."]}'),
  ROW(264, 'default_3x3', '{"en": ["catalog_snowflake_en", "ctlg_pic_snowflake_december"], "es": ["catalog_snowflake_es_001", "ctlg_pic_snowflake_december"], "pt-br": ["catalog_snowflake_br", "ctlg_pic_snowflake_december"]}', '{"en": ["It''s almost Christmas so it''s time for presents! Start doing the daily Christmas Quests and you will earn special Snowflakes which you can use here to choose your gifts.", "Click on the Snowflake in the Toolbar to start your Quests!"], "es": ["¡Este año tú decides que regalarte! Completa los retos diarios y logra Copos de Nieve. Canjéalos luego aquí por Furni Exclusivo. Hazte con el ''Calendario Santa'' o ''Calendario de Nieve'', que incluyen los retos.", ""], "pt-br": ["Utilize seus Flocos de Neve para adquirir os presentes abaixo.", "Clique no item para ver detalhes."]}'),
  ROW(265, 'default_3x3', '{"en": ["catalog_xmas_headline2_en", "ctlg_pic_es_xm10"], "es": ["catalog_xmas_headline2_es", "ctlg_pic_es_xm10"], "pt-br": ["catalog_xmas_headline2_br", "ctlg_pic_es_xm10"]}', '{"en": ["A city-scape covered in crisp, white snow! The Holiday season, we''re going ice skating in the park, eating roasted chestnuts on a bench, throwing long glances at our holiday crush..."], "es": ["¡Rascacielos y calles cubiertas de nieve! Estas fiestas, sal a patinar al parque, disfruta de unas castañas asadas en un banco y cruza miradas con esa persona especial. ¡El invierno también tiene su encanto!"], "pt-br": ["Arranha-céus e ruas cobertos de neve! Neste fim de ano, patine no parque, saboreie castanhas assadas num banco e troque olhares com seu crush. O inverno também tem seu charme!"]}'),
  ROW(266, 'default_3x3', '{"en": ["catalog_header_year2011_en", "catalog_teaser_year2011"]}', '{"en": ["2010 will be a hard year to forget, but let''s give 2011 a BIG welcome! Gather your friends, count down to midnight and set off the fireworks. KABOOM!"], "es": ["¡Será difícil olvidar 2010, pero recibamos 2011 a lo GRANDE! Reúne a tus amigos, comienza la cuenta atrás y lanza los fuegos artificiales. ¡KABOOM!"], "pt-br": ["Vai ser difícil esquecer 2010, mas vamos receber 2011 em GRANDE estilo! Reúna os amigos, faça a contagem regressiva até a meia-noite e solte os fogos. KABOOM!"]}'),
  ROW(267, 'trophies', '{"en": ["catalog_trophies_headline1_uk"], "es": ["catalog_trophies_headline1_es"], "pt-br": ["catalog_trophies_headline1_br"]}', '{"en": ["Here''s to the friends who made your year! Celebrate the memories and give your favourite people a glittering New Year trophy.\\n\\nChoose a trophy using the arrows, then add a personal message and we''ll engrave it with your name and today''s date.", null, null, "Click the box and add your own engraving. Be careful - it''s permanent!"], "es": ["¡Por los amigos que hicieron especial este año! Celebra los buenos momentos y sorprende a tus personas favoritas con un brillante trofeo de Año Nuevo.\\n\\nElige un trofeo con las flechas y escribe un mensaje personal: lo grabaremos junto con tu nombre y la fecha de hoy.", null, null, "Haz clic en la caja de texto para añadir el grabado cuidadosamente - ¡es permanente!"], "pt-br": ["Aos amigos que fizeram seu ano especial! Celebre os bons momentos e surpreenda suas pessoas favoritas com um brilhante troféu de Ano Novo.\\n\\nEscolha um troféu usando as setas e escreva uma mensagem especial: vamos gravá-la com seu nome e a data de hoje.", null, null, "Clique na caixa de texto para adicionar sua mensagem com cuidado. A gravação é permanente!"]}')
) n (`ref_page_id`, `layout_code`, `localization_images`, `localization_texts`)
WHERE NOT EXISTS (SELECT 1 FROM `definition_catalog_page_body` b WHERE b.`ref_page_id` = n.`ref_page_id`);

UPDATE `definition_catalog_page_header` h
JOIN (VALUES
  ROW(2, 3),
  ROW(3, 5),
  ROW(4, 9),
  ROW(5, 11),
  ROW(6, 4),
  ROW(7, 10),
  ROW(9, 14),
  ROW(10, 8),
  ROW(11, 12),
  ROW(12, 2),
  ROW(13, 13),
  ROW(14, 6),
  ROW(25, 2),
  ROW(201, 4),
  ROW(202, 6),
  ROW(203, 8),
  ROW(204, 9),
  ROW(205, 10),
  ROW(206, 11),
  ROW(207, 12),
  ROW(208, 13),
  ROW(209, 14),
  ROW(210, 15),
  ROW(211, 17),
  ROW(212, 18),
  ROW(213, 19),
  ROW(214, 20),
  ROW(215, 22),
  ROW(216, 23),
  ROW(217, 24),
  ROW(218, 25),
  ROW(219, 26),
  ROW(220, 27),
  ROW(221, 28),
  ROW(222, 29),
  ROW(223, 30),
  ROW(224, 31),
  ROW(225, 32),
  ROW(226, 33),
  ROW(227, 34),
  ROW(228, 36),
  ROW(229, 37),
  ROW(230, 38),
  ROW(231, 39),
  ROW(232, 40),
  ROW(233, 41),
  ROW(234, 35),
  ROW(235, 42),
  ROW(236, 43),
  ROW(237, 45),
  ROW(238, 46),
  ROW(239, 47),
  ROW(240, 48),
  ROW(241, 49),
  ROW(242, 51),
  ROW(243, 52),
  ROW(244, 53),
  ROW(245, 54),
  ROW(246, 55),
  ROW(247, 57),
  ROW(248, 58),
  ROW(249, 60),
  ROW(250, 3),
  ROW(251, 16),
  ROW(252, 21),
  ROW(253, 44),
  ROW(254, 56),
  ROW(255, 7),
  ROW(257, 50),
  ROW(259, 5),
  ROW(260, 59)
) n (`id`, `order_num`) ON n.`id` = h.`id`
SET h.`order_num` = n.`order_num`;

UPDATE `definition_catalog_page_body` SET `localization_texts` = '{"en": ["Bigger, Better, Bouncier - it''s Battle Banzai! Now YOU set the rules for your own game, in your own room. You got the Banzai Power!"], "es": ["¿Más grande? ¿Mejor? ¡Banzai! Ahora tú eres quien decide las reglas de tu propio juego de Battle Banzai, el que montes en una de tus Salas Privadas. ¡Tú tienes el poder!"], "pt-br": ["O jogo mais divertido de todos os pixels está de volta ao Hotel! Crie sua própria arena de Battle e chame os amigos."]}' WHERE `ref_page_id` = 142;
UPDATE `definition_catalog_page_body` SET `localization_images` = '{"en": ["catalog_xmas_headline1", "xmas2009_catalogue"], "es": ["catalog_xmas_headline1_es", "xmas2009_catalogue"], "pt-br": ["catalog_xmas_headline1_br", "xmas2009_catalogue"]}' WHERE `ref_page_id` = 251;

CREATE TEMPORARY TABLE `new_offers_2010` (`id` int unsigned PRIMARY KEY, `product_id` int unsigned, `ref_page` int, `offer_code` varchar(100), `price_credits` int,
  `price_activity_points` int, `activity_point_type` tinyint unsigned, `furni_type` char(1))
  DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
INSERT INTO `new_offers_2010` (`id`, `product_id`, `ref_page`, `offer_code`, `price_credits`, `price_activity_points`, `activity_point_type`, `furni_type`) VALUES
  (2898, 3887, 143, 'es_geysir', 1, 0, 0, 's'),
  (2899, 3888, 143, 'es_counter', 1, 0, 0, 's'),
  (2900, 3889, 143, 'es_exit', 1, 0, 0, 's'),
  (2901, 3890, 143, 'es_tile', 1, 0, 0, 's'),
  (2902, 3891, 143, 'es_box', 1, 0, 0, 's'),
  (2903, 3892, 143, 'es_score_r', 1, 0, 0, 's'),
  (2904, 3893, 143, 'es_score_y', 1, 0, 0, 's'),
  (2905, 3894, 143, 'es_score_g', 1, 0, 0, 's'),
  (2906, 3895, 143, 'es_score_b', 1, 0, 0, 's'),
  (2907, 3896, 143, 'es_gate_r', 1, 0, 0, 's'),
  (2908, 3897, 143, 'es_gate_y', 1, 0, 0, 's'),
  (2909, 3898, 143, 'es_gate_g', 1, 0, 0, 's'),
  (2910, 3899, 143, 'es_gate_b', 1, 0, 0, 's'),
  (2911, 3900, 144, 'es_tagging', 1, 0, 0, 's'),
  (2912, 3901, 144, 'es_skating_ice', 1, 0, 0, 's'),
  (2913, 3902, 144, 'es_puck', 1, 0, 0, 's'),
  (2914, 3903, 162, 'wf_trg_walks_on_furni', 1, 0, 0, 's'),
  (2915, 3904, 162, 'wf_trg_walks_off_furni', 1, 0, 0, 's'),
  (2916, 3905, 162, 'wf_trg_says_something', 1, 0, 0, 's'),
  (2917, 3906, 162, 'wf_trg_state_changed', 1, 0, 0, 's'),
  (2918, 3907, 162, 'wf_trg_enter_room', 1, 0, 0, 's'),
  (2919, 3908, 162, 'wf_trg_periodically', 1, 0, 0, 's'),
  (2920, 3909, 162, 'wf_trg_at_given_time', 1, 0, 0, 's'),
  (2921, 3910, 162, 'wf_trg_game_starts', 1, 0, 0, 's'),
  (2922, 3911, 162, 'wf_trg_game_ends', 1, 0, 0, 's'),
  (2923, 3912, 162, 'wf_trg_score_achieved', 1, 0, 0, 's'),
  (2924, 3913, 163, 'wf_act_toggle_state', 1, 0, 0, 's'),
  (2925, 3914, 163, 'wf_act_move_rotate', 1, 0, 0, 's'),
  (2926, 3915, 163, 'wf_act_teleport_to', 1, 0, 0, 's'),
  (2927, 3916, 163, 'wf_act_show_message', 1, 0, 0, 's'),
  (2928, 3917, 163, 'wf_act_match_to_sshot', 1, 0, 0, 's'),
  (2929, 3918, 163, 'wf_act_reset_timers', 1, 0, 0, 's'),
  (2930, 3919, 163, 'wf_act_give_score', 1, 0, 0, 's'),
  (2931, 3920, 164, 'wf_cnd_trggrer_on_frn', 1, 0, 0, 's'),
  (2932, 3921, 164, 'wf_cnd_furnis_hv_avtrs', 1, 0, 0, 's'),
  (2933, 3922, 164, 'wf_cnd_match_snapshot', 1, 0, 0, 's'),
  (2934, 3923, 164, 'wf_cnd_time_more_than', 1, 0, 0, 's'),
  (2935, 3924, 164, 'wf_cnd_time_less_than', 1, 0, 0, 's'),
  (2936, 3925, 165, 'wf_xtra_random', 1, 0, 0, 's'),
  (2937, 3926, 165, 'wf_xtra_unseen', 1, 0, 0, 's'),
  (2938, 3927, 165, 'wf_floor_switch1', 1, 0, 0, 's'),
  (2939, 3928, 165, 'wf_floor_switch2', 1, 0, 0, 's'),
  (2940, 3929, 165, 'wf_pressureplate', 1, 0, 0, 's'),
  (2941, 3930, 165, 'wf_arrowplate', 1, 0, 0, 's'),
  (2942, 3931, 165, 'wf_ringplate', 1, 0, 0, 's'),
  (2943, 3932, 165, 'wf_colortile', 1, 0, 0, 's'),
  (2944, 3933, 165, 'wf_colorwheel', 1, 0, 0, 's'),
  (2945, 3934, 165, 'wf_glowball', 1, 0, 0, 's'),
  (2946, 3935, 165, 'wf_pyramid', 1, 0, 0, 's'),
  (2947, 3936, 165, 'wf_firegate', 1, 0, 0, 's'),
  (2948, 3937, 165, 'wf_glassdoor', 1, 0, 0, 's'),
  (2949, 3938, 165, 'wf_wire1', 1, 0, 0, 's'),
  (2950, 3939, 165, 'wf_wire2', 1, 0, 0, 's'),
  (2951, 3940, 165, 'wf_wire3', 1, 0, 0, 's'),
  (2952, 3941, 165, 'wf_wire4', 1, 0, 0, 's'),
  (2953, 3942, 264, 'qt_calendar0', 0, 400, 0, 'i'),
  (2954, 3943, 264, 'qt_calendar1', 0, 400, 0, 'i'),
  (2955, 3944, 264, 'qt_calendar2', 0, 400, 0, 'i'),
  (2956, 3945, 264, 'qt_xm10_bauble1', 0, 5, 1, 'i'),
  (2957, 3946, 264, 'qt_xm10_bauble2', 0, 5, 1, 'i'),
  (2958, 3947, 264, 'qt_xm10_bauble3', 0, 5, 1, 'i'),
  (2959, 3948, 264, 'qt_xm10_icestar', 0, 10, 1, 'i'),
  (2960, 3949, 264, 'qt_xm10_bench', 0, 10, 1, 's'),
  (2961, 3950, 264, 'qt_xm10_elephant', 0, 25, 1, 's'),
  (2962, 3951, 264, 'qt_xm10_gnome', 0, 75, 1, 's'),
  (2963, 3952, 264, 'qt_xm10_monkey_lamp', 0, 25, 1, 's'),
  (2964, 3953, 264, 'qt_xm10_nest', 0, 25, 1, 's'),
  (2965, 3954, 264, 'qt_xm10_palm_tree', 0, 25, 1, 's'),
  (2966, 3955, 264, 'qt_xm10_stone', 0, 10, 1, 's'),
  (2967, 3956, 264, 'qt_xm10_trex', 0, 125, 1, 's'),
  (2968, 3957, 264, 'qt_xm10_xmduck', 0, 5, 1, 's'),
  (2969, 3958, 264, 'qt_xm10_ice_fish', 0, 100, 1, 's'),
  (2970, 3959, 264, 'qt_xm10_iceclubsofa', 10, 25, 1, 's'),
  (2971, 3960, 264, 'qt_xm10_icedragon', 20, 75, 1, 's'),
  (2972, 3961, 264, 'qt_xm10_iceduck', 0, 5, 1, 's'),
  (2973, 3962, 264, 'qt_xm10_iceelephant', 0, 75, 1, 's'),
  (2974, 3963, 264, 'qt_xm10_icelupin', 0, 25, 1, 's'),
  (2975, 3964, 264, 'qt_xm10_icepillar', 0, 75, 1, 's'),
  (2976, 3965, 264, 'qt_xm10_iceplasto', 0, 10, 1, 's'),
  (2977, 3966, 264, 'qt_xm10_icesilotable', 0, 10, 1, 's'),
  (2978, 3967, 264, 'qt_xm10_iceteddy', 0, 10, 1, 's'),
  (2979, 3968, 264, 'qt_xm10_icetiger', 0, 25, 1, 's'),
  (2980, 3969, 264, 'qt_penguin_ballet', 0, 2, 1, 's'),
  (2981, 3970, 264, 'qt_penguin_icehockey', 0, 3, 1, 's'),
  (2982, 3971, 265, 'xmas10_fireplace', 1, 0, 0, 's'),
  (2983, 3972, 265, 'es_epictree', 1, 0, 0, 's'),
  (2984, 3973, 265, 'es_santa', 1, 0, 0, 's'),
  (2985, 3974, 265, 'es_roaster', 1, 0, 0, 's'),
  (2986, 3975, 265, 'es_bench', 1, 0, 0, 's'),
  (2987, 3976, 265, 'es_lmppst', 1, 0, 0, 's'),
  (2988, 3977, 265, 'es_wpost', 1, 0, 0, 's'),
  (2989, 3978, 265, 'es_statue', 1, 0, 0, 's'),
  (2990, 3979, 265, 'es_sidewalk', 1, 0, 0, 's'),
  (2991, 3980, 265, 'es_fnc_1', 1, 0, 0, 's'),
  (2992, 3981, 265, 'es_fnc_2', 1, 0, 0, 's'),
  (2993, 3982, 265, 'es_fnc_crnr', 1, 0, 0, 's'),
  (2994, 3983, 265, 'es_icestar', 1, 0, 0, 's'),
  (2995, 3984, 265, 'es_icestar_r', 1, 0, 0, 's'),
  (2996, 3985, 265, 'es_icestar_g', 1, 0, 0, 's'),
  (2997, 3986, 265, 'es_icestar_y', 1, 0, 0, 's'),
  (2998, 3987, 265, 'es_build1', 1, 0, 0, 'i'),
  (2999, 3988, 265, 'es_build2', 1, 0, 0, 'i'),
  (3000, 3989, 265, 'es_build3', 1, 0, 0, 'i'),
  (3001, 3990, 265, 'es_build4', 1, 0, 0, 'i'),
  (3002, 3991, 265, 'es_big1', 1, 0, 0, 'i'),
  (3003, 3992, 265, 'es_big2', 1, 0, 0, 'i'),
  (3004, 3993, 265, 'es_big3', 1, 0, 0, 'i'),
  (3005, 3994, 265, 'es_window', 1, 0, 0, 'i'),
  (3006, 3995, 266, 'year2011', 1, 0, 0, 'i'),
  (3007, 3996, 266, 'year2011_waver_r', 1, 0, 0, 's'),
  (3008, 3997, 266, 'year2011_waver_g', 1, 0, 0, 's'),
  (3009, 3998, 266, 'year2011_waver_b', 1, 0, 0, 's'),
  (3010, 3999, 266, 'year2011_waver_y', 1, 0, 0, 's'),
  (3011, 4000, 266, 'fireworks_01', 1, 0, 0, 's'),
  (3012, 4001, 266, 'fireworks_02', 1, 0, 0, 's'),
  (3013, 4002, 266, 'fireworks_03', 1, 0, 0, 's'),
  (3014, 4003, 266, 'fireworks_04', 1, 0, 0, 's'),
  (3015, 4004, 266, 'fireworks_05', 1, 0, 0, 's'),
  (3016, 4005, 266, 'fireworks_06', 1, 0, 0, 's'),
  (3017, 4006, 266, 'fireworks_07', 1, 0, 0, 's'),
  (3018, 4007, 267, 'prizetrophy_2011_r', 1, 0, 0, 's'),
  (3019, 4008, 267, 'prizetrophy_2011_y', 1, 0, 0, 's'),
  (3020, 4009, 267, 'prizetrophy_2011_g', 1, 0, 0, 's'),
  (3021, 4010, 267, 'prizetrophy_2011_b', 1, 0, 0, 's'),
  (3022, 4011, 267, 'prizetrophy_2011_p', 1, 0, 0, 's'),
  (3023, 4012, 267, 'prizetrophy_2011_w', 1, 0, 0, 's'),
  (3024, 4013, 92, 'xmas10_surprise', 1, 0, 0, 's'),
  (3025, 4014, 92, 'xmas10_present', 1, 0, 0, 's'),
  (3026, 4015, 92, 'duck_vip', 1, 0, 0, 's'),
  (3027, 4016, 92, 'duck_hc', 1, 0, 0, 's'),
  (3028, 4017, 213, 'exe_copier', 1, 0, 0, 's'),
  (3029, 4018, 213, 'exe_glassdvdr', 1, 0, 0, 's'),
  (3030, 4019, 213, 'exe_seccam', 1, 0, 0, 's'),
  (3031, 4020, 213, 'exe_sensor', 1, 0, 0, 'i'),
  (3032, 4021, 213, 'exe_wrkdesk', 1, 0, 0, 's'),
  (3033, 4022, 67, 'petfood16', 1, 0, 0, 's'),
  (3034, 4023, 93, 'prizetrophy_nrj_2011', 1, 0, 0, 's'),
  (3035, 4024, 142, 'bb_lightstrip', 1, 0, 0, 's'),
  (3036, 4025, 91, 'ads_hh_safe', 1, 0, 0, 's'),
  (3037, 4026, 91, 'ads_r7logo', 1, 0, 0, 'i'),
  (3038, 4027, 91, 'ads_nokiax3_wall1', 1, 0, 0, 'i'),
  (3039, 4028, 91, 'ads_nokiax3_wall2', 1, 0, 0, 'i'),
  (3040, 4029, 91, 'ads_nokiax3_wall3', 1, 0, 0, 'i'),
  (3041, 4030, 91, 'ads_nokiax3_phn', 1, 0, 0, 'i');

INSERT INTO `definition_catalog_page_offers` (`id`, `ref_page`, `offer_code`, `price_credits`, `price_activity_points`, `activity_point_type`, `club_only`, `giftable`)
SELECT n.`id`, n.`ref_page`, n.`offer_code`, n.`price_credits`, n.`price_activity_points`, n.`activity_point_type`, 0, 1
FROM `new_offers_2010` n
WHERE NOT EXISTS (SELECT 1 FROM `definition_catalog_page_offers` o WHERE o.`ref_page` = n.`ref_page` AND o.`offer_code` = n.`offer_code`)
  AND NOT EXISTS (SELECT 1 FROM `definition_catalog_page_offers` o WHERE o.`id` = n.`id`);

INSERT INTO `definition_catalog_page_offers_products` (`id`, `ref_offer_id`, `ref_furni_def`, `product_type`, `product_extra_data`, `product_count`, `product_rental_duration`)
SELECT n.`product_id`, n.`id`, f.`id`, n.`furni_type`, NULL, 1, -1
FROM `new_offers_2010` n
JOIN `definition_catalog_page_offers` o ON o.`id` = n.`id` AND o.`offer_code` = n.`offer_code`
JOIN `definition_furniture` f ON f.`type` = n.`furni_type` AND f.`classname` = n.`offer_code`
WHERE NOT EXISTS (SELECT 1 FROM `definition_catalog_page_offers_products` p WHERE p.`ref_offer_id` = n.`id`);

DROP TEMPORARY TABLE `new_offers_2010`;

CREATE TEMPORARY TABLE `new_deals_2010` (`id` int unsigned PRIMARY KEY, `ref_page` int, `offer_code` varchar(100), `price_credits` int)
  DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
INSERT INTO `new_deals_2010` (`id`, `ref_page`, `offer_code`, `price_credits`) VALUES
  (3042, 161, 'wf_deal1', 2),
  (3043, 161, 'wf_deal2', 3),
  (3044, 161, 'wf_deal3', 4),
  (3045, 161, 'wf_deal4', 3),
  (3046, 161, 'wf_deal5', 1),
  (3047, 161, 'wf_deal6', 2),
  (3048, 161, 'wf_deal7', 7),
  (3049, 161, 'wf_deal8', 3),
  (3050, 161, 'wf_deal9', 4);
CREATE TEMPORARY TABLE `new_deal_products_2010` (`id` int unsigned PRIMARY KEY, `ref_offer_id` int unsigned, `classname` varchar(100), `furni_type` char(1), `product_count` int)
  DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
INSERT INTO `new_deal_products_2010` (`id`, `ref_offer_id`, `classname`, `furni_type`, `product_count`) VALUES
  (4031, 3042, 'wf_glassdoor', 's', 1),
  (4032, 3042, 'wf_trg_says_something', 's', 1),
  (4033, 3042, 'wf_act_toggle_state', 's', 1),
  (4034, 3043, 'wf_glassdoor', 's', 1),
  (4035, 3043, 'wf_trg_state_changed', 's', 1),
  (4036, 3043, 'wf_act_toggle_state', 's', 1),
  (4037, 3043, 'wf_floor_switch1', 's', 1),
  (4038, 3044, 'wf_glassdoor', 's', 1),
  (4039, 3044, 'wf_act_toggle_state', 's', 1),
  (4040, 3044, 'wf_trg_walks_on_furni', 's', 1),
  (4041, 3044, 'wf_trg_walks_off_furni', 's', 1),
  (4042, 3044, 'wf_pressureplate', 's', 1),
  (4043, 3045, 'wf_glassdoor', 's', 1),
  (4044, 3045, 'wf_pressureplate', 's', 1),
  (4045, 3045, 'wf_trg_walks_on_furni', 's', 1),
  (4046, 3045, 'wf_act_toggle_state', 's', 1),
  (4047, 3046, 'wf_trg_enter_room', 's', 1),
  (4048, 3046, 'wf_act_show_message', 's', 1),
  (4049, 3047, 'wf_trg_says_something', 's', 1),
  (4050, 3047, 'wf_colortile', 's', 1),
  (4051, 3047, 'wf_act_teleport_to', 's', 1),
  (4052, 3048, 'wf_arrowplate', 's', 4),
  (4053, 3048, 'wf_act_move_rotate', 's', 4),
  (4054, 3048, 'wf_trg_walks_on_furni', 's', 4),
  (4055, 3048, 'wf_glowball', 's', 1),
  (4056, 3049, 'wf_arrowplate', 's', 1),
  (4057, 3049, 'wf_act_move_rotate', 's', 1),
  (4058, 3049, 'wf_trg_walks_on_furni', 's', 1),
  (4059, 3049, 'wf_glowball', 's', 1),
  (4060, 3050, 'wf_act_toggle_state', 's', 1),
  (4061, 3050, 'wf_trg_periodically', 's', 1),
  (4062, 3050, 'wf_firegate', 's', 4);

INSERT INTO `definition_catalog_page_offers` (`id`, `ref_page`, `offer_code`, `price_credits`, `price_activity_points`, `activity_point_type`, `club_only`, `giftable`)
SELECT n.`id`, n.`ref_page`, n.`offer_code`, n.`price_credits`, 0, 0, 0, 1
FROM `new_deals_2010` n
WHERE NOT EXISTS (SELECT 1 FROM `definition_catalog_page_offers` o WHERE o.`ref_page` = n.`ref_page` AND o.`offer_code` = n.`offer_code`)
  AND NOT EXISTS (SELECT 1 FROM `definition_catalog_page_offers` o WHERE o.`id` = n.`id`);

INSERT INTO `definition_catalog_page_offers_products` (`id`, `ref_offer_id`, `ref_furni_def`, `product_type`, `product_extra_data`, `product_count`, `product_rental_duration`)
SELECT p.`id`, p.`ref_offer_id`, f.`id`, p.`furni_type`, NULL, p.`product_count`, -1
FROM `new_deal_products_2010` p
JOIN `new_deals_2010` n ON n.`id` = p.`ref_offer_id`
JOIN `definition_catalog_page_offers` o ON o.`id` = n.`id` AND o.`offer_code` = n.`offer_code`
JOIN `definition_furniture` f ON f.`type` = p.`furni_type` AND f.`classname` = p.`classname`
WHERE NOT EXISTS (SELECT 1 FROM `definition_catalog_page_offers_products` x WHERE x.`id` = p.`id`);

DROP TEMPORARY TABLE `new_deal_products_2010`;
DROP TEMPORARY TABLE `new_deals_2010`;

CREATE TABLE `users_achievement_items` (
  `user_ref` int unsigned NOT NULL,
  `achievement_ref` int unsigned NOT NULL,
  `item_ref` int unsigned NOT NULL,
  `counted_at` datetime NOT NULL DEFAULT (utc_timestamp()),
  PRIMARY KEY (`user_ref`,`achievement_ref`,`item_ref`),
  KEY `fk_users_achievement_items_achievement` (`achievement_ref`),
  CONSTRAINT `fk_users_achievement_items_achievement` FOREIGN KEY (`achievement_ref`) REFERENCES `definition_achievements` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_users_achievement_items_user` FOREIGN KEY (`user_ref`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `definition_quests` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `campaign` varchar(32) COLLATE utf8mb4_unicode_ci NOT NULL,
  `localization_code` varchar(32) COLLATE utf8mb4_unicode_ci NOT NULL,
  `quest_type` enum('FIND_STUFF','FIND_HIDDEN_STUFF') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'FIND_STUFF',
  `furni_classname` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `reward_activity_points` int unsigned NOT NULL DEFAULT 1,
  `reward_activity_point_type` tinyint unsigned NOT NULL DEFAULT 1,
  `starts_at` datetime DEFAULT NULL,
  `ends_at` datetime DEFAULT NULL,
  `enabled` tinyint(1) NOT NULL DEFAULT 1,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_definition_quests_campaign_code` (`campaign`, `localization_code`),
  KEY `idx_definition_quests_furni` (`furni_classname`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `users_quest_active` (
  `user_ref` int unsigned NOT NULL,
  `quest_ref` int unsigned NOT NULL,
  `accepted_at` datetime NOT NULL DEFAULT (utc_timestamp()),
  PRIMARY KEY (`user_ref`),
  KEY `idx_users_quest_active_quest` (`quest_ref`),
  CONSTRAINT `fk_users_quest_active_user` FOREIGN KEY (`user_ref`) REFERENCES `users` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_users_quest_active_quest` FOREIGN KEY (`quest_ref`) REFERENCES `definition_quests` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `users_quest_completed` (
  `user_ref` int unsigned NOT NULL,
  `quest_ref` int unsigned NOT NULL,
  `completed_at` datetime NOT NULL DEFAULT (utc_timestamp()),
  PRIMARY KEY (`user_ref`, `quest_ref`),
  KEY `idx_users_quest_completed_quest` (`quest_ref`),
  CONSTRAINT `fk_users_quest_completed_user` FOREIGN KEY (`user_ref`) REFERENCES `users` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_users_quest_completed_quest` FOREIGN KEY (`quest_ref`) REFERENCES `definition_quests` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO `definition_quests` (`campaign`, `localization_code`, `quest_type`, `furni_classname`, `reward_activity_points`, `starts_at`, `ends_at`)
SELECT 'xmas10', CONCAT('DAY', LPAD(q.`day`, 2, '0')), q.`type`, q.`furni`, 5,
       TIMESTAMP('2026-12-01 05:00:00') + INTERVAL (q.`day` - 1) DAY,
       TIMESTAMP('2026-12-01 05:00:00') + INTERVAL (q.`day` - 1 + 7) DAY
FROM (VALUES
    ROW(1, 'FIND_STUFF', 'tree5'),
    ROW(2, 'FIND_STUFF', 'fireplace_armas'),
    ROW(3, 'FIND_STUFF', 'es_lmppst'),
    ROW(4, 'FIND_STUFF', 'xm09_man_c'),
    ROW(5, 'FIND_STUFF', 'es_skating_ice'),
    ROW(6, 'FIND_HIDDEN_STUFF', 'es_roaster'),
    ROW(7, 'FIND_STUFF', 'es_statue'),
    ROW(8, 'FIND_STUFF', 'es_sidewalk'),
    ROW(9, 'FIND_HIDDEN_STUFF', 'es_epictree'),
    ROW(10, 'FIND_STUFF', 'es_icestar'),
    ROW(11, 'FIND_HIDDEN_STUFF', 'es_wpost'),
    ROW(12, 'FIND_STUFF', 'es_bench'),
    ROW(13, 'FIND_STUFF', 'xmas08_lantern'),
    ROW(14, 'FIND_STUFF', 'es_geysir'),
    ROW(15, 'FIND_STUFF', 'es_puck'),
    ROW(16, 'FIND_HIDDEN_STUFF', 'es_gate_b'),
    ROW(17, 'FIND_STUFF', 'xmas08_hottub'),
    ROW(18, 'FIND_HIDDEN_STUFF', 'qt_xm10_trex'),
    ROW(19, 'FIND_STUFF', 'pudding'),
    ROW(20, 'FIND_STUFF', 'turkey'),
    ROW(21, 'FIND_STUFF', 'qt_xm10_gnome'),
    ROW(22, 'FIND_HIDDEN_STUFF', 'christmas_reindeer'),
    ROW(23, 'FIND_STUFF', 'christmas_sleigh'),
    ROW(24, 'FIND_STUFF', 'es_santa'),
    ROW(25, 'FIND_STUFF', 'xm09_table'),
    ROW(26, 'FIND_STUFF', 'tree1')
  ) AS q(`day`, `type`, `furni`);

INSERT INTO `definition_quests` (`campaign`, `localization_code`, `quest_type`, `furni_classname`, `reward_activity_points`, `starts_at`, `ends_at`) VALUES
  ('xmas10', 'PILOT1', 'FIND_STUFF', 'bling_toilet', 1, '2026-11-23 05:00:00', '2027-01-04 05:00:00'),
  ('xmas10', 'PILOT2', 'FIND_STUFF', 'bling_pool', 1, '2026-11-23 05:00:00', '2027-01-04 05:00:00'),
  ('xmas10', 'PILOT3', 'FIND_STUFF', 'bling_bed', 1, '2026-11-23 05:00:00', '2027-01-04 05:00:00');

INSERT INTO `definition_achievements` (`code_base`, `client_type`, `max_level`, `enabled`)
SELECT n.`code_base`, n.`client_type`, n.`max_level`, 1 FROM (VALUES
    ROW('ACH_FireworksCharger', 1005, 20),
    ROW('ACH_Xm10QuestCompleted', 1006, 10),
    ROW('ACH_TagA', 1007, 20),
    ROW('ACH_TagB', 1008, 20),
    ROW('ACH_TagC', 1009, 20)
  ) n (`code_base`, `client_type`, `max_level`)
WHERE NOT EXISTS (SELECT 1 FROM `definition_achievements` a WHERE a.`code_base` = n.`code_base`);

INSERT IGNORE INTO `definition_achievement_levels` (`achievement_ref`, `level`, `threshold`, `badge_code`, `reward_activity_points`, `reward_activity_point_type`, `reward_achievement_score`)
SELECT a.`id`, l.`level`, l.`threshold`, CONCAT(a.`code_base`, l.`level`), l.`pixels`, 0, 10
FROM (VALUES
    ROW('ACH_FireworksCharger', 1, 20, 5),
    ROW('ACH_FireworksCharger', 2, 100, 10),
    ROW('ACH_FireworksCharger', 3, 300, 20),
    ROW('ACH_FireworksCharger', 4, 600, 30),
    ROW('ACH_FireworksCharger', 5, 900, 40),
    ROW('ACH_FireworksCharger', 6, 1200, 50),
    ROW('ACH_FireworksCharger', 7, 1500, 50),
    ROW('ACH_FireworksCharger', 8, 1800, 60),
    ROW('ACH_FireworksCharger', 9, 2100, 70),
    ROW('ACH_FireworksCharger', 10, 2400, 80),
    ROW('ACH_FireworksCharger', 11, 2700, 90),
    ROW('ACH_FireworksCharger', 12, 3000, 90),
    ROW('ACH_FireworksCharger', 13, 3300, 100),
    ROW('ACH_FireworksCharger', 14, 3600, 110),
    ROW('ACH_FireworksCharger', 15, 3900, 120),
    ROW('ACH_FireworksCharger', 16, 4200, 130),
    ROW('ACH_FireworksCharger', 17, 4500, 140),
    ROW('ACH_FireworksCharger', 18, 4800, 140),
    ROW('ACH_FireworksCharger', 19, 5100, 150),
    ROW('ACH_FireworksCharger', 20, 5400, 160),
    ROW('ACH_Xm10QuestCompleted', 1, 1, 15),
    ROW('ACH_Xm10QuestCompleted', 2, 2, 15),
    ROW('ACH_Xm10QuestCompleted', 3, 4, 15),
    ROW('ACH_Xm10QuestCompleted', 4, 6, 15),
    ROW('ACH_Xm10QuestCompleted', 5, 9, 15),
    ROW('ACH_Xm10QuestCompleted', 6, 12, 15),
    ROW('ACH_Xm10QuestCompleted', 7, 16, 15),
    ROW('ACH_Xm10QuestCompleted', 8, 20, 15),
    ROW('ACH_Xm10QuestCompleted', 9, 24, 15),
    ROW('ACH_Xm10QuestCompleted', 10, 28, 15),
    ROW('ACH_TagA', 1, 10, 5),
    ROW('ACH_TagA', 2, 20, 10),
    ROW('ACH_TagA', 3, 30, 20),
    ROW('ACH_TagA', 4, 45, 30),
    ROW('ACH_TagA', 5, 60, 40),
    ROW('ACH_TagA', 6, 80, 50),
    ROW('ACH_TagA', 7, 100, 50),
    ROW('ACH_TagA', 8, 125, 60),
    ROW('ACH_TagA', 9, 150, 70),
    ROW('ACH_TagA', 10, 175, 80),
    ROW('ACH_TagA', 11, 200, 90),
    ROW('ACH_TagA', 12, 250, 90),
    ROW('ACH_TagA', 13, 300, 100),
    ROW('ACH_TagA', 14, 350, 110),
    ROW('ACH_TagA', 15, 400, 120),
    ROW('ACH_TagA', 16, 500, 130),
    ROW('ACH_TagA', 17, 600, 140),
    ROW('ACH_TagA', 18, 750, 140),
    ROW('ACH_TagA', 19, 950, 150),
    ROW('ACH_TagA', 20, 1200, 160),
    ROW('ACH_TagB', 1, 1, 5),
    ROW('ACH_TagB', 2, 4, 10),
    ROW('ACH_TagB', 3, 9, 20),
    ROW('ACH_TagB', 4, 17, 30),
    ROW('ACH_TagB', 5, 29, 40),
    ROW('ACH_TagB', 6, 44, 50),
    ROW('ACH_TagB', 7, 62, 50),
    ROW('ACH_TagB', 8, 87, 60),
    ROW('ACH_TagB', 9, 114, 70),
    ROW('ACH_TagB', 10, 144, 80),
    ROW('ACH_TagB', 11, 186, 90),
    ROW('ACH_TagB', 12, 242, 90),
    ROW('ACH_TagB', 13, 314, 100),
    ROW('ACH_TagB', 14, 402, 110),
    ROW('ACH_TagB', 15, 498, 120),
    ROW('ACH_TagB', 16, 618, 130),
    ROW('ACH_TagB', 17, 754, 140),
    ROW('ACH_TagB', 18, 904, 140),
    ROW('ACH_TagB', 19, 1084, 150),
    ROW('ACH_TagB', 20, 1284, 160),
    ROW('ACH_TagC', 1, 3, 5),
    ROW('ACH_TagC', 2, 8, 10),
    ROW('ACH_TagC', 3, 16, 20),
    ROW('ACH_TagC', 4, 31, 30),
    ROW('ACH_TagC', 5, 51, 40),
    ROW('ACH_TagC', 6, 81, 50),
    ROW('ACH_TagC', 7, 121, 50),
    ROW('ACH_TagC', 8, 171, 60),
    ROW('ACH_TagC', 9, 231, 70),
    ROW('ACH_TagC', 10, 301, 80),
    ROW('ACH_TagC', 11, 381, 90),
    ROW('ACH_TagC', 12, 471, 90),
    ROW('ACH_TagC', 13, 571, 100),
    ROW('ACH_TagC', 14, 681, 110),
    ROW('ACH_TagC', 15, 801, 120),
    ROW('ACH_TagC', 16, 931, 130),
    ROW('ACH_TagC', 17, 1071, 140),
    ROW('ACH_TagC', 18, 1221, 140),
    ROW('ACH_TagC', 19, 1381, 150),
    ROW('ACH_TagC', 20, 1551, 160)
  ) l (`code_base`, `level`, `threshold`, `pixels`)
JOIN `definition_achievements` a ON a.`code_base` = l.`code_base`;

INSERT INTO `definition_figure_sets` (`id`, `settype_ref`, `gender`, `club`, `colorable`, `selectable`) VALUES
  (3139, 'ha', 'U', 2, 1, 1),
  (3140, 'ha', 'U', 2, 1, 1),
  (3141, 'ea', 'U', 2, 0, 1),
  (3144, 'ha', 'U', 2, 1, 1),
  (3145, 'ha', 'U', 2, 1, 1),
  (3146, 'he', 'U', 1, 1, 1),
  (3147, 'fa', 'U', 1, 1, 1),
  (3148, 'ea', 'U', 1, 1, 1),
  (3149, 'he', 'U', 1, 1, 1),
  (3150, 'ha', 'U', 2, 1, 1),
  (3151, 'ca', 'U', 2, 1, 1),
  (3152, 'cc', 'M', 2, 1, 1),
  (3153, 'cc', 'M', 2, 1, 1),
  (3154, 'sh', 'U', 2, 1, 1),
  (3155, 'he', 'U', 1, 1, 1),
  (3156, 'ha', 'U', 2, 1, 1),
  (3157, 'cc', 'F', 2, 1, 1),
  (3158, 'cc', 'M', 2, 1, 1),
  (3159, 'cc', 'F', 2, 1, 1),
  (9534, 'hr', 'F', 2, 1, 1),
  (9563, 'cc', 'F', 2, 1, 1)
ON DUPLICATE KEY UPDATE `settype_ref` = VALUES(`settype_ref`), `gender` = VALUES(`gender`), `club` = VALUES(`club`),
  `colorable` = VALUES(`colorable`), `selectable` = VALUES(`selectable`);
