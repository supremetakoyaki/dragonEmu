ALTER TABLE `users_settings`
  ADD COLUMN `preferred_language` enum('en','es','pt-br') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'en' COMMENT 'the language the hotel is in for them';
UPDATE `web_client_vars`
SET `var_value` = REPLACE(REPLACE(`var_value`,
  '/r48/gamedata/external_flash_texts.txt', '/r48/gamedata/external_flash_texts_%language%.txt'),
  '/r48/gamedata/external_flash_texts_en.txt', '/r48/gamedata/external_flash_texts_%language%.txt')
WHERE `var_key` = 'external.texts.txt';

UPDATE `global_message_texts` SET `message_text` = JSON_OBJECT('en', `message_text`);
ALTER TABLE `global_message_texts` MODIFY `message_text` json NOT NULL;

INSERT INTO `global_message_texts` (`message_key`, `message_text`) VALUES
  ('action_too_fast','{"en": "You''re doing that too fast. Wait a moment and try again.", "es": "Estás haciendo eso muy rápido. Espera un momento y vuelve a intentarlo.", "pt-br": "Você está fazendo isso rápido demais. Espere um pouco e tente de novo."}'),
  ('alert_sent','{"en": "Alert sent to {username}.", "es": "Alerta enviada a {username}.", "pt-br": "Alerta enviado para {username}."}'),
  ('badge_already_owned','{"en": "{username} already has the badge {badge}.", "es": "{username} ya tiene la placa {badge}.", "pt-br": "{username} já tem o emblema {badge}."}'),
  ('badge_given','{"en": "Gave {username} the badge {badge}.", "es": "Le diste a {username} la placa {badge}.", "pt-br": "Você deu o emblema {badge} para {username}."}'),
  ('ban_applied','{"en": "{username} is banned {length}: {reason}", "es": "{username} está baneado {length}: {reason}", "pt-br": "{username} está banido {length}: {reason}"}'),
  ('ban_length_for','{"en": "for {duration}", "es": "por {duration}", "pt-br": "por {duration}"}'),
  ('ban_length_permanent','{"en": "permanently", "es": "para siempre", "pt-br": "para sempre"}'),
  ('bots_reloaded','{"en": "Bots reloaded. {bots} bot(s) in {rooms} loaded room(s).", "es": "Bots recargados. {bots} bot(s) en {rooms} sala(s) cargada(s).", "pt-br": "Bots recarregados. {bots} bot(s) em {rooms} quarto(s) carregado(s)."}'),
  ('choose_ban_length','{"en": "Choose how long the ban should last.", "es": "Elige cuánto debe durar el baneo.", "pt-br": "Escolha quanto tempo o banimento deve durar."}'),
  ('currency_given','{"en": "Gave {username} {amount} {currency}. They now have {balance}.", "es": "Le diste a {username} {amount} {currency}. Ahora tiene {balance}.", "pt-br": "Você deu {amount} {currency} para {username}. Agora tem {balance}."}'),
  ('duration_days','{"en": "{count} days", "es": "{count} días", "pt-br": "{count} dias"}'),
  ('duration_hours','{"en": "{count} hours", "es": "{count} horas", "pt-br": "{count} horas"}'),
  ('duration_minutes','{"en": "{count} minutes", "es": "{count} minutos", "pt-br": "{count} minutos"}'),
  ('duration_one_day','{"en": "1 day", "es": "1 día", "pt-br": "1 dia"}'),
  ('duration_one_hour','{"en": "1 hour", "es": "1 hora", "pt-br": "1 hora"}'),
  ('duration_one_minute','{"en": "1 minute", "es": "1 minuto", "pt-br": "1 minuto"}'),
  ('furni_command_tip','{"en": " Tip: use the :furni command!", "es": " Consejo: ¡usa el comando :furni!", "pt-br": " Dica: use o comando :furni!"}'),
  ('kick_cant_be_kicked','{"en": "{username} can''t be kicked.", "es": "{username} no puede ser expulsado.", "pt-br": "{username} não pode ser expulso."}'),
  ('kick_not_allowed','{"en": "You can''t kick anyone out of this room.", "es": "No puedes expulsar a nadie de esta sala.", "pt-br": "Você não pode expulsar ninguém deste quarto."}'),
  ('kick_not_here','{"en": "{username} is not in this room.", "es": "{username} no está en esta sala.", "pt-br": "{username} não está neste quarto."}'),
  ('logged_in_elsewhere','{"en": "You have logged in somewhere else, so you have been disconnected here.", "es": "Iniciaste sesión en otro lugar, así que se te desconectó aquí.", "pt-br": "Você entrou em outro lugar, então foi desconectado aqui."}'),
  ('messenger_too_fast','{"en": "You''re sending messages too fast. Wait {seconds} seconds before sending another.", "es": "Estás enviando mensajes muy rápido. Espera {seconds} segundos antes de enviar otro.", "pt-br": "Você está enviando mensagens rápido demais. Espere {seconds} segundos antes de enviar outra."}'),
  ('mood_light_one_per_room','{"en": "You can only place one Mood Light per room. If you can''t find the one you already placed, it is probably well-hidden.", "es": "Solo puedes poner un Regulador de Luces por sala. Si no encuentras el que ya pusiste, seguro está bien escondido.", "pt-br": "Você só pode colocar um Regulador de Luzes por quarto. Se não encontra o que já colocou, ele deve estar bem escondido."}'),
  ('motto_too_fast','{"en": "You''re changing your motto too fast - wait a moment and try again.", "es": "Estás cambiando tu misión muy rápido, espera un momento y vuelve a intentarlo.", "pt-br": "Você está mudando sua missão rápido demais, espere um pouco e tente de novo."}'),
  ('mute_applied','{"en": "{username} is muted for {duration}.", "es": "{username} está silenciado por {duration}.", "pt-br": "{username} está silenciado por {duration}."}'),
  ('name_change_every_days','{"en": "once every {days} day(s)", "es": "una vez cada {days} día(s)", "pt-br": "uma vez a cada {days} dia(s)"}'),
  ('name_change_every_hour','{"en": "once an hour", "es": "una vez por hora", "pt-br": "uma vez por hora"}'),
  ('name_change_summary','{"en": "{changes} You can change it at most {every}.{wait}", "es": "{changes} Puedes cambiarlo como máximo {every}.{wait}", "pt-br": "{changes} Você pode mudá-lo no máximo {every}.{wait}"}'),
  ('name_change_wait','{"en": " You can change it again in {wait}.", "es": " Puedes volver a cambiarlo en {wait}.", "pt-br": " Você pode mudá-lo de novo em {wait}."}'),
  ('name_changes_unlimited','{"en": "You have changed your name {changes} time(s) and can change it as often as you like.", "es": "Cambios de nombre: {changes}. Puedes cambiarlo cuantas veces quieras.", "pt-br": "Mudanças de nome: {changes}. Você pode mudá-lo quantas vezes quiser."}'),
  ('name_changes_used','{"en": "You have used {used} of your {limit} name change(s) and have {left} left.", "es": "Usaste {used} de tus {limit} cambio(s) de nombre y te quedan {left}.", "pt-br": "Você usou {used} de suas {limit} mudança(s) de nome e restam {left}."}'),
  ('no_message','{"en": "Write a message first.", "es": "Escribe un mensaje primero.", "pt-br": "Escreva uma mensagem primeiro."}'),
  ('no_such_user','{"en": "There is no such user.", "es": "No existe ese usuario.", "pt-br": "Esse usuário não existe."}'),
  ('not_in_room','{"en": "You are not in a room.", "es": "No estás en una sala.", "pt-br": "Você não está em um quarto."}'),
  ('not_muted','{"en": "{username} isn''t muted.", "es": "{username} no está silenciado.", "pt-br": "{username} não está silenciado."}'),
  ('pickall_owner_only','{"en": "Only the room''s owner can pick up all its furniture.", "es": "Solo el dueño de la sala puede recoger todos sus furnis.", "pt-br": "Só o dono do quarto pode pegar todos os seus mobis."}'),
  ('public_rooms_reloaded','{"en": "Public rooms reloaded. {rooms} room(s) in {folders} categorie(s).", "es": "Salas públicas recargadas. {rooms} sala(s) en {folders} categoría(s).", "pt-br": "Quartos públicos recarregados. {rooms} quarto(s) em {folders} categoria(s)."}'),
  ('purchases_off_shutdown','{"en": "The Hotel is shutting down - purchasing furniture and Habbo Club, and trading, are disabled.", "es": "El Hotel se está cerrando, así que las compras de furnis y de Habbo Club, y los tradeos, están desactivados.", "pt-br": "O Hotel está fechando, então as compras de mobis e de Habbo Club, e as trocas, estão desativadas."}'),
  ('rank_refusal_lower_only','{"en": "You can''t {verb} {username} as they are your same or higher rank than you.", "es": "No puedes {verb} a {username} porque tiene tu mismo rango o uno mayor.", "pt-br": "Você não pode {verb} {username} porque tem o mesmo nível que você ou um maior."}'),
  ('rank_refusal_self','{"en": "You can''t {verb} {username}.", "es": "No puedes {verb} a {username}.", "pt-br": "Você não pode {verb} {username}."}'),
  ('rank_refusal_yourself','{"en": "You can''t {verb} yourself.", "es": "No te puedes {verb} a ti mismo.", "pt-br": "Você não pode se {verb}."}'),
  ('roller_interval_many','{"en": "{count} ticks", "es": "{count} ticks", "pt-br": "{count} ticks"}'),
  ('roller_interval_one','{"en": "tick", "es": "tick", "pt-br": "tick"}'),
  ('roller_speed_current','{"en": "Roller speed is {speed} (every {interval}). Use :roller_speed 1-4.", "es": "La velocidad de los rollers es {speed} (cada {interval}). Usa :roller_speed 1-4.", "pt-br": "A velocidade dos rollers é {speed} (a cada {interval}). Use :roller_speed 1-4."}'),
  ('roller_speed_no_rights','{"en": "You need rights in this room to change its roller speed.", "es": "Necesitas derechos en esta sala para cambiar la velocidad de sus rollers.", "pt-br": "Você precisa de direitos neste quarto para mudar a velocidade dos rollers."}'),
  ('roller_speed_set','{"en": "Roller speed set to {speed} (every {interval}).", "es": "Velocidad de los rollers: {speed} (cada {interval}).", "pt-br": "Velocidade dos rollers: {speed} (a cada {interval})."}'),
  ('room_mute_applied','{"en": "This room is muted for {duration}.", "es": "Esta sala está silenciada por {duration}.", "pt-br": "Este quarto está silenciado por {duration}."}'),
  ('room_name_inappropriate','{"en": "Inappropriate to Hotel Management.", "es": "Inapropiado para la Gerencia del Hotel.", "pt-br": "Impróprio para a Gerência do Hotel."}'),
  ('room_not_muted','{"en": "This room isn''t muted.", "es": "Esta sala no está silenciada.", "pt-br": "Este quarto não está silenciado."}'),
  ('room_unmute_applied','{"en": "This room isn''t muted any more.", "es": "Esta sala ya no está silenciada.", "pt-br": "Este quarto não está mais silenciado."}'),
  ('shutdown_already','{"en": "The hotel is already shutting down.", "es": "El hotel ya se está cerrando.", "pt-br": "O hotel já está fechando."}'),
  ('shutdown_bad_time','{"en": "The time the hotel opens again is HH:mm. For example, :shutdown confirm 18:30.", "es": "La hora en que el hotel vuelve a abrir es HH:mm. Por ejemplo, :shutdown confirm 18:30.", "pt-br": "O horário em que o hotel abre de novo é HH:mm. Por exemplo, :shutdown confirm 18:30."}'),
  ('shutdown_confirm_prompt','{"en": "This closes the hotel in {minutes} minutes: trading, purchases, the recycler, the marketplace and pet training and feeding stop at once, nobody new can log in, and then everyone is disconnected and the emulator stops. Type :shutdown confirm [HH:mm] to do it.", "es": "Esto cierra el hotel en {minutes} minutos: los tradeos, las compras, el reciclador, el mercadillo y el entrenamiento y la alimentación de mascotas se detienen de inmediato, nadie nuevo puede entrar, y luego todos son desconectados y el emulador se detiene. Escribe :shutdown confirm [HH:mm] para hacerlo.", "pt-br": "Isto fecha o hotel em {minutes} minutos: as trocas, as compras, o reciclador, a feira e o treino e a alimentação dos mascotes param na hora, ninguém novo pode entrar, e depois todos são desconectados e o emulador para. Digite :shutdown confirm [HH:mm] para fazer isso."}'),
  ('softkick_not_allowed','{"en": "You can''t kick anyone in this room.", "es": "No puedes expulsar a nadie en esta sala.", "pt-br": "Você não pode expulsar ninguém neste quarto."}'),
  ('superban_applied','{"en": "{username} is superbanned {length}: {reason}", "es": "{username} está superbaneado {length}: {reason}", "pt-br": "{username} está superbanido {length}: {reason}"}'),
  ('teleport_bad_height','{"en": "The height must be a number, for example, :teleport on 1.5", "es": "La altura debe ser un número, por ejemplo, :teleport on 1.5", "pt-br": "A altura deve ser um número, por exemplo, :teleport on 1.5"}'),
  ('teleport_height_anywhere','{"en": "on the topmost height of the tile", "es": "en la altura más alta de la casilla", "pt-br": "na altura mais alta do quadrado"}'),
  ('teleport_height_at','{"en": "at height {height}", "es": "a la altura {height}", "pt-br": "na altura {height}"}'),
  ('teleport_off','{"en": "Teleport mode is off.", "es": "El modo teletransporte está desactivado.", "pt-br": "O modo teletransporte está desativado."}'),
  ('teleport_on','{"en": "Teleport mode is on with height set to {height}.", "es": "El modo teletransporte está activado con la altura en {height}.", "pt-br": "O modo teletransporte está ativado com a altura em {height}."}'),
  ('teleport_status_off','{"en": "Teleport mode is off. Use :teleport on [z] or :teleport off.", "es": "El modo teletransporte está desactivado. Usa :teleport on [z] o :teleport off.", "pt-br": "O modo teletransporte está desativado. Use :teleport on [z] ou :teleport off."}'),
  ('teleport_status_on','{"en": "Teleport mode is on, {height}. Use :teleport on [z] or :teleport off.", "es": "El modo teletransporte está activado, {height}. Usa :teleport on [z] o :teleport off.", "pt-br": "O modo teletransporte está ativado, {height}. Use :teleport on [z] ou :teleport off."}'),
  ('trade_nothing_exchanged','{"en": "The trade could not be completed, so nothing was exchanged.", "es": "No se pudo completar el tradeo, así que no se intercambió nada.", "pt-br": "A troca não pôde ser concluída, então nada foi trocado."}'),
  ('unmute_applied','{"en": "{username} can talk again.", "es": "{username} puede volver a hablar.", "pt-br": "{username} pode falar de novo."}'),
  ('usage_alert','{"en": "Use :alert [user] [message].", "es": "Usa :alert [usuario] [mensaje].", "pt-br": "Use :alert [usuário] [mensagem]."}'),
  ('usage_ban','{"en": "Use :ban [user] [hours - at least 1] [reason].", "es": "Usa :ban [usuario] [horas, al menos 1] [motivo].", "pt-br": "Use :ban [usuário] [horas, pelo menos 1] [motivo]."}'),
  ('usage_fx','{"en": "Use :fx [id] - 0 takes it off.", "es": "Usa :fx [id], el 0 lo quita.", "pt-br": "Use :fx [id], o 0 tira."}'),
  ('usage_give_currency','{"en": "Use :{command} [user] [amount] - amount above 0.", "es": "Usa :{command} [usuario] [cantidad], una cantidad mayor que 0.", "pt-br": "Use :{command} [usuário] [quantia], uma quantia maior que 0."}'),
  ('usage_givebadge','{"en": "Use :givebadge [user] [code] - a code is at most {max} characters.", "es": "Usa :givebadge [usuario] [código], un código tiene como máximo {max} caracteres.", "pt-br": "Use :givebadge [usuário] [código], um código tem no máximo {max} caracteres."}'),
  ('usage_handitem','{"en": "Use :handitem [id] - 0 puts it down.", "es": "Usa :handitem [id], el 0 lo suelta.", "pt-br": "Use :handitem [id], o 0 larga."}'),
  ('usage_kick','{"en": "Use :kick [user].", "es": "Usa :kick [usuario].", "pt-br": "Use :kick [usuário]."}'),
  ('usage_motto','{"en": "Use :motto <new motto>.", "es": "Usa :motto <nueva misión>.", "pt-br": "Use :motto <nova missão>."}'),
  ('usage_mute','{"en": "Use :mute [user] [hours - at least 1].", "es": "Usa :mute [usuario] [horas, al menos 1].", "pt-br": "Use :mute [usuário] [horas, pelo menos 1]."}'),
  ('usage_roller_speed','{"en": "Use :roller_speed 1, 2, 3 or 4 (4 is the fastest).", "es": "Usa :roller_speed 1, 2, 3 o 4 (4 es la más rápida).", "pt-br": "Use :roller_speed 1, 2, 3 ou 4 (4 é a mais rápida)."}'),
  ('usage_roomalert','{"en": "Use :roomalert [message].", "es": "Usa :roomalert [mensaje].", "pt-br": "Use :roomalert [mensagem]."}'),
  ('usage_roommute','{"en": "Use :roommute [hours - at least 1].", "es": "Usa :roommute [horas, al menos 1].", "pt-br": "Use :roommute [horas, pelo menos 1]."}'),
  ('usage_softkick','{"en": "Use :softkick [user].", "es": "Usa :softkick [usuario].", "pt-br": "Use :softkick [usuário]."}'),
  ('usage_start_vote','{"en": "Use :start_vote [question].", "es": "Usa :start_vote [pregunta].", "pt-br": "Use :start_vote [pergunta]."}'),
  ('usage_superban','{"en": "Use :superban [user] [days - at least 1] [reason].", "es": "Usa :superban [usuario] [días, al menos 1] [motivo].", "pt-br": "Use :superban [usuário] [dias, pelo menos 1] [motivo]."}'),
  ('usage_teleport','{"en": "Use :teleport on [z] or :teleport off.", "es": "Usa :teleport on [z] o :teleport off.", "pt-br": "Use :teleport on [z] ou :teleport off."}'),
  ('usage_unmute','{"en": "Use :unmute [user].", "es": "Usa :unmute [usuario].", "pt-br": "Use :unmute [usuário]."}'),
  ('user_not_found','{"en": "There is no user called {username}.", "es": "No hay ningún usuario llamado {username}.", "pt-br": "Não existe nenhum usuário chamado {username}."}'),
  ('user_not_in_room','{"en": "{username} isn''t in a room.", "es": "{username} no está en una sala.", "pt-br": "{username} não está em um quarto."}'),
  ('user_not_online','{"en": "{username} isn''t online.", "es": "{username} no está conectado.", "pt-br": "{username} não está online."}'),
  ('vote_already_going','{"en": "This room already has a vote going. You can end it with :end_vote.", "es": "Esta sala ya tiene una votación en curso. Puedes terminarla con :end_vote.", "pt-br": "Este quarto já tem uma votação em andamento. Você pode encerrá-la com :end_vote."}'),
  ('vote_just_ended','{"en": "A vote has only just ended in this room - wait a moment.", "es": "Acaba de terminar una votación en esta sala, espera un momento.", "pt-br": "Uma votação acabou de terminar neste quarto, espere um pouco."}'),
  ('vote_none','{"en": "There is no vote going in this room.", "es": "No hay ninguna votación en curso en esta sala.", "pt-br": "Não há nenhuma votação em andamento neste quarto."}'),
  ('vote_start_cooldown','{"en": "You can start a vote once every {seconds} seconds.", "es": "Puedes empezar una votación cada {seconds} segundos.", "pt-br": "Você pode começar uma votação a cada {seconds} segundos."}'),
  ('you_are_muted','{"en": "You are muted. Left: {duration}.", "es": "Estás silenciado. Queda: {duration}.", "pt-br": "Você está silenciado. Falta: {duration}."}'),
  ('chat_flood','{"en": "You''re typing too fast, don''t flood the room! - {seconds} seconds remaining", "es": "¡Estás escribiendo muy rápido, no inundes la sala! Quedan {seconds} segundos.", "pt-br": "Você está digitando rápido demais, não inunde o quarto! Faltam {seconds} segundos."}'),
  ('marketplace_search_placeholder','{"en": "Search by name...", "es": "Busca nombre", "pt-br": "Buscar nome"}'),
  ('verb_ban','{"en": "ban", "es": "banear", "pt-br": "banir"}'),
  ('verb_mute','{"en": "mute", "es": "silenciar", "pt-br": "silenciar"}'),
  ('verb_kick','{"en": "kick", "es": "expulsar", "pt-br": "expulsar"}'),
  ('verb_caution','{"en": "caution", "es": "advertir", "pt-br": "advertir"}'),
  ('currency_credits','{"en": "credits", "es": "créditos", "pt-br": "moedas"}'),
  ('currency_pixels','{"en": "pixels", "es": "píxeles", "pt-br": "pixels"}')
ON DUPLICATE KEY UPDATE `message_text` = JSON_MERGE_PATCH(VALUES(`message_text`), `message_text`);

UPDATE `global_game_settings` SET `ban_default_reason` = JSON_OBJECT('en', `ban_default_reason`);
ALTER TABLE `global_game_settings` MODIFY `ban_default_reason` json NOT NULL COMMENT 'a ban without a reason, in each language';
UPDATE `global_game_settings`
SET `ban_default_reason` = JSON_MERGE_PATCH('{"es": "No sigue la Manera Habbo.", "pt-br": "Não segue a Habbo Etiqueta."}', `ban_default_reason`)
WHERE `ban_default_reason` ->> '$.en' = 'Does not follow the Habbo Way.';

UPDATE `web_articles_categories` SET `caption` = JSON_OBJECT('en', `caption`);
ALTER TABLE `web_articles_categories` MODIFY `caption` json NOT NULL;

UPDATE `web_articles` SET `title` = JSON_OBJECT('en', `title`), `content_html` = JSON_OBJECT('en', `content_html`);
ALTER TABLE `web_articles`
  MODIFY `title` json NOT NULL,
  MODIFY `content_html` json NOT NULL;

UPDATE `web_promos`
SET `caption` = JSON_OBJECT('en', `caption`), `description` = JSON_OBJECT('en', `description`), `button_text` = JSON_OBJECT('en', `button_text`);
ALTER TABLE `web_promos`
  MODIFY `caption` json NOT NULL,
  MODIFY `description` json NOT NULL DEFAULT (JSON_OBJECT()),
  MODIFY `button_text` json NOT NULL DEFAULT (JSON_OBJECT());

UPDATE `web_hot_campaigns`
SET `caption` = JSON_OBJECT('en', `caption`), `description` = JSON_OBJECT('en', `description`), `url_text` = JSON_OBJECT('en', `url_text`);
ALTER TABLE `web_hot_campaigns`
  MODIFY `caption` json NOT NULL,
  MODIFY `description` json NOT NULL DEFAULT (JSON_OBJECT()),
  MODIFY `url_text` json NOT NULL DEFAULT (JSON_OBJECT()) COMMENT 'empty: the website''s "Go there"';

UPDATE `web_page_rules` SET `title` = JSON_OBJECT('en', `title`), `html_content` = JSON_OBJECT('en', `html_content`);
ALTER TABLE `web_page_rules`
  MODIFY `title` json NOT NULL,
  MODIFY `html_content` json NOT NULL;

UPDATE `web_page_ranks` SET `caption` = JSON_OBJECT('en', `caption`);
ALTER TABLE `web_page_ranks` MODIFY `caption` json NOT NULL;

UPDATE `web_settings` SET `footer_text` = JSON_OBJECT('en', `footer_text`), `web_slogan` = JSON_OBJECT('en', `web_slogan`);
ALTER TABLE `web_settings`
  MODIFY `footer_text` json NOT NULL DEFAULT (JSON_OBJECT(
    'en', 'dragonHotel is a not for profit recreation of Habbo v48. Habbo is a registered trademark of Sulake Oy and dragonHotel is not affiliated with it.',
    'es', 'dragonHotel es una recreación sin fines de lucro de Habbo v48. Habbo es una marca registrada de Sulake Oy y dragonHotel no está afiliado a ella.',
    'pt-br', 'dragonHotel é uma recriação sem fins lucrativos do Habbo v48. Habbo é uma marca registrada da Sulake Oy e o dragonHotel não tem vínculo com ela.')),
  MODIFY `web_slogan` json NOT NULL DEFAULT (JSON_OBJECT(
    'en', 'Reminisce early 2010 Habbo Hotel!',
    'es', '¡Revive el Habbo Hotel de principios de 2010!',
    'pt-br', 'Relembre o Habbo Hotel do começo de 2010!')) COMMENT 'empty hides it';
UPDATE `web_settings`
SET `footer_text` = JSON_MERGE_PATCH(`footer_text`, JSON_OBJECT(
  'es', 'dragonHotel es una recreación sin fines de lucro de Habbo v48. Habbo es una marca registrada de Sulake Oy y dragonHotel no está afiliado a ella.',
  'pt-br', 'dragonHotel é uma recriação sem fins lucrativos do Habbo v48. Habbo é uma marca registrada da Sulake Oy e o dragonHotel não tem vínculo com ela.'))
WHERE `footer_text` ->> '$.en' = 'dragonHotel is a not for profit recreation of Habbo v48. Habbo is a registered trademark of Sulake Oy and dragonHotel is not affiliated with it.';
UPDATE `web_settings`
SET `web_slogan` = JSON_MERGE_PATCH(`web_slogan`, JSON_OBJECT('es', '¡Revive el Habbo Hotel de principios de 2010!', 'pt-br', 'Relembre o Habbo Hotel do começo de 2010!'))
WHERE `web_slogan` ->> '$.en' = 'Reminisce early 2010 Habbo Hotel!';

ALTER TABLE `users`
  ADD COLUMN `online` enum('0','1') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '0' COMMENT 'in the hotel right now (the emulator)';

UPDATE `definition_catalog_page_header` SET `display_name` = JSON_OBJECT('en', `display_name`);
ALTER TABLE `definition_catalog_page_header`
  MODIFY `display_name` json NOT NULL;
UPDATE `definition_catalog_page_header` h JOIN (VALUES
    ROW('Front Page', 'Portada', 'Primeira Página'),
    ROW('Furni Shop', 'Tienda de Furni', 'Loja de Mobis'),
    ROW('Pixel Shop', 'Tienda de Píxeles', 'Loja de Pixels'),
    ROW('Habbo Exchange', 'Casa de Cambio', 'Casa de Câmbio'),
    ROW('Ecotron', 'Ecotron', 'Ecotron'),
    ROW('Pets Shop', 'Tienda de Mascotas', 'Loja de Mascotes'),
    ROW('Habbo Club', 'Habbo Club', 'Habbo Club'),
    ROW('Staff Only', 'Solo Staff', 'Só Equipe'),
    ROW('Marketplace', 'Mercadillo', 'Feira Livre'),
    ROW('Trax', 'Trax', 'Trax'),
    ROW('Rares', 'Rares', 'Raros'),
    ROW('Camera', 'Cámara', 'Câmera'),
    ROW('Spaces', 'Espacios', 'Espaços'),
    ROW('Rentals', 'Alquileres', 'Aluguéis'),
    ROW('Special Effects', 'Efectos especiales', 'Efeitos especiais'),
    ROW('Hello Furni', 'Hello Furni', 'Hello Furni'),
    ROW('Automobile', 'Automóvil', 'Automóvel'),
    ROW('Rewards', 'Premios', 'Prêmios'),
    ROW('Instructions', 'Instrucciones', 'Instruções'),
    ROW('Dogs', 'Perros', 'Cachorros'),
    ROW('Cats', 'Gatos', 'Gatos'),
    ROW('Crocodiles', 'Cocodrilos', 'Crocodilos'),
    ROW('Terriers', 'Terriers', 'Terriers'),
    ROW('Pigs', 'Cerdos', 'Porcos'),
    ROW('Bears', 'Osos', 'Ursos'),
    ROW('Pet Accessories', 'Accesorios para mascotas', 'Acessórios para mascotes'),
    ROW('Pets Information', 'Información de mascotas', 'Informações sobre mascotes'),
    ROW('Buy Habbo Club', 'Comprar Habbo Club', 'Assinar o Habbo Club'),
    ROW('Club Gifts', 'Regalos del Club', 'Presentes do Club'),
    ROW('Club Shop', 'Tienda del Club', 'Loja do Club'),
    ROW('Campaign Furni', 'Furni de campañas', 'Mobis de campanhas'),
    ROW('Uncategorized', 'Sin categoría', 'Sem categoria'),
    ROW('Rare Trophies', 'Trofeos raros', 'Troféus raros'),
    ROW('Classic Rares', 'Rares clásicos', 'Raros clássicos'),
    ROW('2008-10 Rares', 'Rares 2008-10', 'Raros 2008-10'),
    ROW('My Sales', 'Mis ventas', 'Minhas vendas'),
    ROW('Offers', 'Ofertas', 'Ofertas'),
    ROW('Accessories', 'Accesorios', 'Acessórios'),
    ROW('Alhambra', 'Alhambra', 'Alhambra'),
    ROW('Arctic', 'Ártico', 'Ártico'),
    ROW('Area', 'Area', 'Area'),
    ROW('Asian', 'Asiático', 'Asiático'),
    ROW('Bathroom', 'Baño', 'Banheiro'),
    ROW('Bensalem', 'Bensalem', 'Bensalem'),
    ROW('Bling', 'Bling', 'Bling'),
    ROW('Candy', 'Candy', 'Candy'),
    ROW('Country', 'Campestre', 'Fazenda'),
    ROW('Diner', 'Diner', 'Lanchonete'),
    ROW('Easter', 'Pascua', 'Páscoa'),
    ROW('Executive', 'Ejecutivo', 'Executivo'),
    ROW('Flags', 'Banderas', 'Bandeiras'),
    ROW('Gallery', 'Galería', 'Galeria'),
    ROW('Glass', 'Cristal', 'Vidro'),
    ROW('Gothic', 'Gótico', 'Gótico'),
    ROW('Greek', 'Griego', 'Grego'),
    ROW('Grunge', 'Grunge', 'Grunge'),
    ROW('Habbowood', 'Habbowood', 'Habbowood'),
    ROW('Halloween', 'Halloween', 'Halloween'),
    ROW('Iced', 'Iced', 'Iced'),
    ROW('Japan', 'Japón', 'Japão'),
    ROW('Kitchen', 'Cocina', 'Cozinha'),
    ROW('Lodge', 'Lodge', 'Lodge'),
    ROW('Lost Tribe', 'Tribu Perdida', 'Tribo Perdida'),
    ROW('Mad Science', 'Ciencia Loca', 'Ciência Maluca'),
    ROW('Mode', 'Mode', 'Mode'),
    ROW('Mood Lights', 'Reguladores de Luces', 'Reguladores de Luzes'),
    ROW('Neon', 'Neón', 'Neon'),
    ROW('Organo', 'Organo', 'Organo'),
    ROW('Plants', 'Plantas', 'Plantas'),
    ROW('Plasto', 'Plasto', 'Plasto'),
    ROW('Mall', 'Centro Comercial', 'Shopping'),
    ROW('Pura', 'Pura', 'Pura'),
    ROW('Relax', 'Relax', 'Relax'),
    ROW('Romantique', 'Romantique', 'Romantique'),
    ROW('Rugs', 'Alfombras', 'Tapetes'),
    ROW('Runway', 'Pasarela', 'Passarela'),
    ROW('Sci-Fi', 'Ciencia ficción', 'Ficção científica'),
    ROW('Shalimar', 'Shalimar', 'Shalimar'),
    ROW('Sports', 'Deportes', 'Esportes'),
    ROW('Summer', 'Verano', 'Verão'),
    ROW('Teleports', 'Teletransportes', 'Teleportes'),
    ROW('Tiki', 'Tiki', 'Tiki'),
    ROW('Trophies', 'Trofeos', 'Troféus'),
    ROW('Urban', 'Urbano', 'Urbano'),
    ROW('Valentines', 'San Valentín', 'Dia dos Namorados'),
    ROW('Virus', 'Virus', 'Vírus'),
    ROW('Windows', 'Ventanas', 'Janelas'),
    ROW('Christmas', 'Navidad', 'Natal'),
    ROW('Flower Power', 'Flower Power', 'Flower Power'),
    ROW('Rollers', 'Rollers', 'Rollers'),
    ROW('Twilight: New Moon', 'Crepúsculo: Luna nueva', 'Crepúsculo: Lua Nova'),
    ROW('American Idol', 'American Idol', 'American Idol'),
    ROW('Spiderwick', 'Spiderwick', 'Spiderwick'),
    ROW('Traxmachines', 'Máquinas de Trax', 'Máquinas de Trax'),
    ROW('Ambient', 'Ambient', 'Ambient'),
    ROW('Dance', 'Dance', 'Dance'),
    ROW('Rock', 'Rock', 'Rock'),
    ROW('SFX', 'SFX', 'SFX'),
    ROW('IRL Hits', 'Éxitos de la vida real', 'Hits da vida real')) AS tr (en, es, pt)
  ON h.`display_name` ->> '$.en' = tr.en
SET h.`display_name` = JSON_MERGE_PATCH(h.`display_name`, JSON_OBJECT('es', tr.es, 'pt-br', tr.pt));
UPDATE `definition_catalog_page_body` SET `localization_images` = JSON_OBJECT('en', `localization_images`), `localization_texts` = JSON_OBJECT('en', `localization_texts`);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["¡Bienvenido a Dragon Hotel!", "Hay un nuevo Rare esperándote en el catálogo...", "¡Llévame allí!", "¿Qué es Dragon Hotel? ", "¡Qué bueno que preguntas! Hice Dragon Hotel para revivir mis recuerdos de adolescente jugando a Habbo, sobre todo entre 2007 y 2010.\\nLa V48 es la última versión antes de los grandes cambios (por ejemplo, el VIP Club, Battle Banzai, Wired, etc.).\\n¡Intentaré añadir todas las funciones que había entonces (y quizá algunas nuevas!)", "Canjea aquí un código de cupón:", "Rares", "", "", "¡Genial! Ahora llévame a la página del HC>>", "Comprar Habbo Club"]' AS JSON), '$."pt-br"', CAST('["Bem-vindo ao Dragon Hotel!", "Tem um Raro novo esperando por você no catálogo...", "Me leva até lá!", "O que é o Dragon Hotel? ", "Que bom que você perguntou! Eu fiz o Dragon Hotel para relembrar meus tempos de adolescente jogando Habbo, principalmente entre 2007 e 2010.\\nA V48 é a última versão antes das grandes mudanças (por exemplo, o VIP Club, Battle Banzai, Wired etc.).\\nVou tentar trazer todas as funções que existiam naquela época (e talvez algumas novas!)", "Resgate um código de cupom aqui:", "Raros", "", "", "Ótimo, agora me leva à página do HC>>", "Assinar o Habbo Club"]' AS JSON))
WHERE `ref_page_id` = 1 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Welcome to Dragon Hotel!", "There''s a new Rare waiting in the catalog for you...", "Take me there!", "What is Dragon Hotel? ", "I''m glad you asked! I made Dragon Hotel as a way to reminisce my memories as a teenager playing Habbo mostly through 2007-2010.\\nV48 is the last version before major changes arrived (e.g. VIP club, Battle Banzai, Wired, etc.).\\nI will try to add all features that were available back (and maybe add some new ones!)", "Redeem a voucher code here:", "Rares", "#fff4d9", "#fff4d9", "Great, now take me to the HC Page>>", "Buy Habbo Club"]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Cambia tus Créditos de Habbo por Créditos intercambiables. ¡Así podrás usar Créditos en un tradeo por Furni!", "Haz clic en un objeto para ver más detalles", "¡Productos canjeables!"]' AS JSON), '$."pt-br"', CAST('["Troque suas Moedas do Habbo por Moedas negociáveis. Assim você pode usar Moedas em uma troca por Mobis!", "Clique em um item para ver mais detalhes", "Produtos reembolsáveis!"]' AS JSON))
WHERE `ref_page_id` = 4 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Exchange your Habbo Credits into tradeable Credits. Then you are able to use Credits in a Trade for Furni!", "Click on an item to see more details", "Refundable goods!"]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Cada Rare de aquí es un pedazo de historia: está disponible por poco tiempo y luego desaparece para siempre. Consigue uno mientras puedas, antes de que se convierta en una historia que les cuentes a los Habbos más nuevos.", "", ""]' AS JSON), '$."pt-br"', CAST('["Cada Raro daqui é um pedaço da história: fica disponível por pouco tempo e depois some para sempre. Garanta o seu enquanto pode, antes que vire uma história para contar aos Habbos mais novos.", "", ""]' AS JSON))
WHERE `ref_page_id` = 12 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Every Rare here is a piece of history - available for a short time, then gone for good. Grab one while you can, before it becomes a story you tell newer Habbos about.", "", ""]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Suelos, papeles pintados y paisajes: ¡consigue una combinación genial para tu sala! Usa nuestra sala de ejemplo de abajo para probar antes de comprar. ¡Elige tus diseños y colores favoritos y simplemente haz clic en Comprar!"]' AS JSON), '$."pt-br"', CAST('["Pisos, papéis de parede e paisagens: monte uma combinação incrível para o seu quarto! Use o nosso quarto de exemplo abaixo para experimentar antes de comprar. Escolha seus desenhos e cores favoritos e é só clicar em Comprar!"]' AS JSON))
WHERE `ref_page_id` = 25 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Floors, wallpapers and landscapes - get a groovy combination for your room! Use our sample room below to try before you buy! Select your favourite designs and colours and simply click Buy!"]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Alquila efectos geniales para darle un toque explosivo a tu sala. ¡Desde burbujas hasta lanzallamas, puedes darle un cambio enorme a tu sala!"]' AS JSON), '$."pt-br"', CAST('["Alugue efeitos incríveis para dar um toque explosivo ao seu quarto. De bolhas a lança-chamas, você pode transformar seu quarto de vez!"]' AS JSON))
WHERE `ref_page_id` = 31 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Hire some cool effects to add an explosive touch to your room. From bubbles to firestarters, you can enhance your room dramatically!"]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Ponle a tu personaje efectos geniales para cada ocasión. ¿Quieres salir volando en la alfombra roja o estar bajo los focos? ¡Esta es tu oportunidad!"]' AS JSON), '$."pt-br"', CAST('["Turbine seu personagem com efeitos incríveis para cada ocasião. Quer sair voando no tapete vermelho ou ficar sob os holofotes? Esta é a sua chance!"]' AS JSON))
WHERE `ref_page_id` = 32 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Tune your character with cool effects that fit the occasion. Do you want to fly away with the red carpet or be in the spotlight? Now is your chance!"]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Hello Furni se compra con Píxeles y es perfecto si estás decorando tu sala por primera vez. El Furni es tuyo para siempre, así que no se puede intercambiar.", "Haz clic en un objeto para ver más detalles."]' AS JSON), '$."pt-br"', CAST('["Hello Furni se compra com Pixels e é perfeito se você está decorando seu quarto pela primeira vez. Os Mobis são seus para sempre, então não podem ser trocados.", "Clique em um item para ver mais detalhes."]' AS JSON))
WHERE `ref_page_id` = 33 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Hello Furni is available with Pixels and is perfect if you are decorating your room for the very first time. The Furni is yours to keep and therefore cannot be traded.", "Click on an item for details."]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["¡Todo Habbo necesita un efecto de coche! Y qué mejor regalo para un amigo que una bonita señal de tráfico o una elegante pila de neumáticos..."]' AS JSON), '$."pt-br"', CAST('["Todo Habbo precisa de um efeito de carro! E que presente melhor para um amigo do que uma bela placa de trânsito ou uma elegante pilha de pneus..."]' AS JSON))
WHERE `ref_page_id` = 34 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Every Habbo needs a car effect! Not only do they bring a bit of the outside inside, they also enhance the air quality! And what better gift for a friend than a beautiful traffic sign or elegant pile of tires..."]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Arrastra 5 objetos a las casillas de abajo y haz clic en reciclar. Los objetos reciclables tienen una etiqueta de reciclable en tu inventario. Mira los premios y las instrucciones antes de reciclar."]' AS JSON), '$."pt-br"', CAST('["Arraste 5 itens para as caixas abaixo e clique em reciclar. Os itens recicláveis têm uma etiqueta de reciclável no seu inventário. Confira os prêmios e as instruções antes de reciclar."]' AS JSON))
WHERE `ref_page_id` = 51 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Drag 5 items to the boxes below and click recycle. Recyclable items display a recyclable tag in your inventory. Check out the prizes and the instructions for recycling before you recycle."]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["¿Cuáles son los premios? La caja del Ecotron puede contener uno de estos:"]' AS JSON), '$."pt-br"', CAST('["Quais são os prêmios? A caixa do Ecotron pode ter um destes:"]' AS JSON))
WHERE `ref_page_id` = 52 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["What are the prizes? Ecotron box may contain one of these:"]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["El Ecotron es un reciclador de furni. Deshazte de tus furnis viejos... ¿qué recibirás a cambio? ¡Es una sorpresa! Conviértete en un eco-guerrero Habbo. ¡No se aceptan devoluciones!", "¿Cómo se usa el Ecotron?", "1. Arrastra 5 objetos de tu inventario al Ecotron. Un objeto por casilla. Los objetos reciclables están marcados con una imagen en tu inventario. Cuando tengas 5 objetos en las casillas, haz clic en el botón \\"Reciclar\\". Encontrarás la caja de premio del Ecotron en tu inventario.\\r\\n2. Haz clic en la caja para ver su etiqueta. Ábrela, o intercámbiala sin abrir. El contador te dice cuánto tienes que esperar para volver a reciclar. Mira los premios antes de reciclar: ¡que la sorpresa no te sorprenda!", ""]' AS JSON), '$."pt-br"', CAST('["O Ecotron é um reciclador de mobis. Livre-se dos seus mobis velhos... e o que você ganha em troca? É surpresa! Vire um eco-guerreiro Habbo. Não há reembolso!", "Como usar o Ecotron?", "1. Arraste 5 itens do seu inventário para o Ecotron. Um item por caixa. Os itens recicláveis têm uma imagem no seu inventário. Quando tiver 5 itens nas caixas, clique no botão \\"Reciclar\\". A caixa de prêmio do Ecotron vai para o seu inventário.\\r\\n2. Clique na caixa para ver a etiqueta. Abra a caixa, ou troque fechada mesmo. O contador mostra quanto tempo falta para reciclar de novo. Confira os prêmios antes de reciclar: que a surpresa não te surpreenda!", ""]' AS JSON))
WHERE `ref_page_id` = 53 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["The Ecotron is a furni recycler. Get rid of old furni... what will you get in return? It''s a surprise! Become a Habbo eco-warrior. No refunds!", "How to use the Ecotron?", "1. Just drag 5 items from your inventory to the Ecotron. One item / square. Recyclable items are marked in your inventory with an image. When you have 5 items in the boxes, click the \\"Recycle\\" button. You can now find the Ecotron prize box from your inventory.\\r\\n2. Click the box to  see its tag. Open the box, or trade it unopened. The timer shows you how long you have to wait before you can recycle more items. Check the prizes before you recycle - don''t be surprised by the surprise!", ""]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["¡Hocicos húmedos, patas, ladridos y guaus! Estás a punto de entrar en el mundo de los Perros de Habbo. Un compañero adorable y fiel te espera moviendo la cola cada vez que te ve. Encuentra un nuevo amigo en nuestra selección, que cambia constantemente.", "Ponle nombre a tu Perro:", "Elige un color:", "Elige una raza:"]' AS JSON), '$."pt-br"', CAST('["Focinhos molhados, patas, latidos e au-aus! Você está entrando no mundo dos Cachorros do Habbo. Um companheiro adorável e fiel espera por você abanando o rabo toda vez que te vê. Encontre um novo amigo na nossa seleção, que muda sempre.", "Dê um nome ao seu Cachorro:", "Escolha uma cor:", "Escolha uma raça:"]' AS JSON))
WHERE `ref_page_id` = 61 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Wet noses, paws, yaps and woofs! You''re about to enter the world of Habbo Dogs. An adorable and faithful servant awaits you with a wagging tail everytime they see you. Find a new friend from our ever-changing selection.", "Name your Dog:", "Pick a color:", "Pick a breed:"]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["¡Pelusa, bigotes, miaus y ronroneos! Estás a punto de entrar en el mundo de los Gatos de Habbo. Estas criaturitas te harán compañía si las cuidas bien. Encuentra un nuevo amigo en nuestra selección, que cambia constantemente.", "Ponle nombre a tu Gato:", "Elige un color:", "Elige una raza:"]' AS JSON), '$."pt-br"', CAST('["Pelinhos, bigodes, miaus e ronrons! Você está entrando no mundo dos Gatos do Habbo. Esses bichinhos fofos vão fazer companhia se você cuidar bem deles. Encontre um novo amigo na nossa seleção, que muda sempre.", "Dê um nome ao seu Gato:", "Escolha uma cor:", "Escolha uma raça:"]' AS JSON))
WHERE `ref_page_id` = 62 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Fluff, whiskers, meows and purrs! You''re about to enter the world of Habbo Cats. These cute little critters will keep you company if you look after them well. Find a new friend from our ever-changing selection.", "Name your Cat:", "Pick a color:", "Pick a breed:"]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["¡Piel escamosa, gruñidos y mordiscos! Estás a punto de entrar en el mundo de los Cocodrilos de Habbo. Para vigilar tu sala o asustar a tus amigos, puedes tener un compañero fiel y sorprendentemente cariñoso. Encuentra un nuevo amigo en nuestra selección, que cambia constantemente.", "Ponle nombre a tu Cocodrilo:", "Elige un color:", "Elige una raza:"]' AS JSON), '$."pt-br"', CAST('["Pele escamosa, rosnados e mordidas! Você está entrando no mundo dos Crocodilos do Habbo. Para proteger seu quarto ou assustar seus amigos, um companheiro fiel e surpreendentemente carinhoso pode ser seu. Encontre um novo amigo na nossa seleção, que muda sempre.", "Dê um nome ao seu Crocodilo:", "Escolha uma cor:", "Escolha uma raça:"]' AS JSON))
WHERE `ref_page_id` = 63 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Scaly skin, growls and snaps! You''re about to enter the world of Habbo Crocs. Security for your room or to scare your friends a trustworthy and surprisingly loving companion can be yours. Find a new friend from our ever-changing selection.", "Name your Croc:", "Pick a color:", "Pick a breed:"]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Las mejores cosas vienen en frascos pequeños, y los Terriers de Habbo no son la excepción.", "Ponle un nombre:", "Elige un color:", "Elige una raza:"]' AS JSON), '$."pt-br"', CAST('["As melhores coisas vêm em embalagens pequenas, e os Terriers do Habbo não são exceção.", "Dê um nome:", "Escolha uma cor:", "Escolha uma raça:"]' AS JSON))
WHERE `ref_page_id` = 64 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Good things come in small packages and Habbo''s Terriers are no exception.", "Give a name:", "Pick a color:", "Pick a breed:"]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["¡Estos pequeños traviesos iban derechos a la mesa hasta que llegamos a un acuerdo con un granjero de la zona para rescatar sus adorables colitas! Adopta hoy a tu cerdito, solo te falta un nombre.", "Ponle un nombre:", "Elige un color:", "Elige una raza:"]' AS JSON), '$."pt-br"', CAST('["Esses pequenos levados iam direto para a mesa do jantar até fecharmos um acordo com um fazendeiro da região para salvar esses rabinhos enrolados! Adote seu porquinho hoje, só falta um nome.", "Dê um nome:", "Escolha uma cor:", "Escolha uma raça:"]' AS JSON))
WHERE `ref_page_id` = 65 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["These little scamps were destined for the dining table until we struck a deal with a local farmer to rescue their cute pigtails! Adopt your little piglet today, all you need now is a name.", "Give a name:", "Pick a color:", "Pick a breed:"]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Un mamífero grande y pesado que camina sobre las plantas de los pies, con un pelaje espeso y una cola muy corta. ¡Enamórate de nuestra adorable variedad de Osos, con el Grizzly y el Polar!", "Ponle un nombre:", "Elige un color:", "Elige una raza:"]' AS JSON), '$."pt-br"', CAST('["Um mamífero grande e pesado que anda sobre as solas dos pés, com pelagem grossa e um rabo bem curtinho. Apaixone-se pela nossa adorável variedade de Ursos, com o Pardo e o Polar!", "Dê um nome:", "Escolha uma cor:", "Escolha uma raça:"]' AS JSON))
WHERE `ref_page_id` = 66 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["A large, heavy, mammal that walks on the soles of its feet, with thick fur and a very short tail. Fall in love with our adorable range of Bears that include the Grizzly and Polar varieties!", "Give a name:", "Pick a color:", "Pick a breed:"]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Mantén a tu Mascota feliz y sana. Dale de comer para que esté fuerte, ¡pero no te olvides del ejercicio! Y tranquilo, ningún pez sufre al alimentar a tu Mascota.", "", "Hay que tenerlas felices"]' AS JSON), '$."pt-br"', CAST('["Mantenha seu Mascote feliz e saudável. Alimente para ele ficar forte, mas não se esqueça dos exercícios! E fique tranquilo: nenhum peixe sofre na alimentação do seu Mascote.", "", "Tem que deixá-los felizes"]' AS JSON))
WHERE `ref_page_id` = 67 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Keep your Pet happy and healthy. Feed it to keep strong but don''t forget about exercise! Please note, no fish are hurt in the feeding of your Pet.", "", "You gotta keep ''em happy"]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Las mascotas también viven en Habbo Hotel, así que todo dueño necesita saber un poco sobre ellas. Cuida a tu mascota repasando nuestros puntos clave de abajo.", "Algunas cosas que deberías saber:", "- No puedes intercambiar ni regalar mascotas. Una vez que las compras, son tuyas para siempre.\\r\\n- Puedes tener todas las que quieras, pero no más de 5 mascotas por sala.\\r\\n- Puedes llevar tus mascotas a otras salas, si el dueño de la sala te lo permite.\\r\\n- Tu mascota necesita entrenamiento antes de obedecer tus órdenes. Entrenando a tus mascotas también puedes ganar logros geniales.", ""]' AS JSON), '$."pt-br"', CAST('["Os mascotes também moram no Habbo Hotel, então todo dono precisa saber um pouco sobre eles. Cuide do seu mascote seguindo os pontos principais abaixo.", "Algumas coisas que você precisa saber:", "- Você não pode trocar nem dar mascotes. Depois de comprados, eles são seus para sempre.\\r\\n- Você pode ter quantos quiser, mas no máximo 5 mascotes por quarto.\\r\\n- Você pode levar seus mascotes para outros quartos, se o dono do quarto deixar.\\r\\n- Seu mascote precisa de treino antes de obedecer aos seus comandos. Treinando seus mascotes você também ganha conquistas incríveis.", ""]' AS JSON))
WHERE `ref_page_id` = 68 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Pets are inhabitants of Habbo Hotel too, so each pet owner needs to know a bit about them. Look after your pet by looking through our key points below.", "Few things you should know:", "- You cannot trade or give away pets. Once you purchase them, they will be yours forever.\\r\\n- You can own as many as you want, but you can have no more than 5 pets per room.\\r\\n- You can take pets with you to other rooms, should the room owner allow you to do so.\\r\\n- Your pet needs to be trained before it''ll listen to your commands. By training your pets, you can gain some awesome achievements as well.", ""]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Habbo Club es nuestro club VIP solo para miembros: ¡nada de chusma! Los miembros disfrutan de un montón de ventajas, como ropa exclusiva, regalos gratis y una lista de amigos más grande", "", ""]' AS JSON), '$."pt-br"', CAST('["O Habbo Club é o nosso clube VIP só para membros: nada de penetras! Os membros têm um monte de vantagens, como roupas exclusivas, presentes grátis e uma lista de amigos maior", "", ""]' AS JSON))
WHERE `ref_page_id` = 71 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Habbo Club is our VIP members-only club - absolutely no riff-raff admitted! Members enjoy a wide range of benefits, including exclusive clothes, free gifts and an extended friends list", "", ""]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["NUEVA gama de Furni del Habbo Club. Deja que estas elegantes delicias le den a tu sala un aire sofisticado y sencillo. ¡Quedan genial junto a tus regalos mensuales!", "Haz clic en un objeto para ver más detalles", "¡Solo para miembros del Habbo Club!"]' AS JSON), '$."pt-br"', CAST('["NOVA linha de Mobis do Habbo Club. Deixe essas peças elegantes darem ao seu quarto um ar sofisticado e simples. Ficam ótimas com seus presentes mensais!", "Clique em um item para ver mais detalhes", "Só para membros do Habbo Club!"]' AS JSON))
WHERE `ref_page_id` = 73 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["NEW Habbo Club Furni range. Allow these elegant delights to make your room sophisticated and humble. They look great placed with your monthly gifts!", "Click on an item to see more details", "For Habbo Club members only!"]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Este furni es solo para uso interno.", "Haz clic en el objeto que quieras para más información", ""]' AS JSON), '$."pt-br"', CAST('["Estes mobis são só para uso interno.", "Clique no item que quiser para mais informações", ""]' AS JSON))
WHERE `ref_page_id` = 91 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["This furniture is for internal use only.", "Click on the item you want for more information", ""]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Estos objetos variados no encajan en ninguna categoría del Catálogo, o tienen otra finalidad.", "Haz clic en el objeto que quieras para más información", ""]' AS JSON), '$."pt-br"', CAST('["Estes itens variados não se encaixam em nenhuma categoria do Catálogo, ou têm outra finalidade.", "Clique no item que quiser para mais informações", ""]' AS JSON))
WHERE `ref_page_id` = 92 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["These miscellaneous items do not fit a specific category within the Catalogue, or they are meant for a different purpose.", "Click on the item you want for more information", ""]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Los trofeos de esta página son para que el Staff los regale o como premio en concursos."]' AS JSON), '$."pt-br"', CAST('["Os troféus desta página são para a Equipe dar de presente ou como prêmio em concursos."]' AS JSON))
WHERE `ref_page_id` = 93 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["The trophies in this page are for Staff members to give away or as prize in competitions."]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Estos Rares son solo para uso interno."]' AS JSON), '$."pt-br"', CAST('["Estes Raros são só para uso interno."]' AS JSON))
WHERE `ref_page_id` = 94 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["These Rares are for internal use only."]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Estos Rares son solo para uso interno."]' AS JSON), '$."pt-br"', CAST('["Estes Raros são só para uso interno."]' AS JSON))
WHERE `ref_page_id` = 95 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["These Rares are for internal use only."]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["El Mercadillo de Habbo es una forma cómoda y segura de comprar y vender objetos en Habbo.", "¿Cómo funciona el Mercadillo?", "Vender objetos:\\r\\nPuedes vender cualquier objeto intercambiable seleccionándolo en tu inventario y haciendo clic en \\"Vender en el Mercadillo\\". Puedes poner 5 objetos a la venta por 1 crédito.\\r\\n\\r\\nComprar objetos:\\r\\nBusca el objeto que quieras comprar en la página de Ofertas del Mercadillo y haz clic en Comprar. Te garantizamos que conseguirás el objeto al precio más barato disponible en el momento de la compra.", ""]' AS JSON), '$."pt-br"', CAST('["A Feira Livre do Habbo é um jeito prático e seguro de comprar e vender itens no Habbo.", "Como funciona a Feira Livre?", "Vendendo itens:\\r\\nVocê pode vender qualquer item negociável selecionando o item no seu inventário e clicando em \\"Vender na Feira Livre\\". Você pode anunciar 5 itens à venda por 1 moeda.\\r\\n\\r\\nComprando itens:\\r\\nProcure o item que você quer comprar na página de Ofertas da Feira Livre e clique em Comprar. Garantimos que você vai levar o item pelo menor preço disponível no momento da compra.", ""]' AS JSON))
WHERE `ref_page_id` = 103 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["The Habbo Marketplace is a convenient and safe way to buy and sell items in Habbo.", "How does the Marketplace work?", "Selling items:\\r\\nYou can sell any item which is tradable by selecting the item in your inventory and clicking on \\"Sell in Marketplace\\". You are able to post 5 items for sale at the cost of 1 credit.\\r\\n\\r\\nBuying items:\\r\\nBrowse for the item you want to buy on the Marketplace Offers page and click Buy. We guarantee you''ll get the item at the cheapest price available at the moment of purchase.", ""]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Saca fotos y guarda esos momentos especiales para siempre. Con tu cámara Habbo puedes fotografiar casi cualquier cosa del Hotel. Hasta a un famoso sentado en el váter...\\n\\nLa Cámara viene con 2 fotos gratis.", "Cuando hayas usado tus fotos gratis, tendrás que comprar más. Cada carrete trae cinco fotos. Tu Cámara te muestra cuánto carrete te queda y carga el siguiente automáticamente."]' AS JSON), '$."pt-br"', CAST('["Tire fotos e guarde esses momentos especiais para sempre. Com a sua câmera Habbo você pode fotografar quase tudo no Hotel. Até uma celebridade sentada no trono...\\n\\nA Câmera vem com 2 fotos grátis.", "Quando acabarem as fotos grátis, você vai precisar comprar mais. Cada filme tira cinco fotos. A sua Câmera mostra quanto filme ainda resta e carrega o próximo sozinha."]' AS JSON))
WHERE `ref_page_id` = 131 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Take picture and keep a record of those special moments forever. With your Habbo camera you can take pictures of just about anything in the Hotel - Even a celebrity sitting on the loo...\\n\\nThe Camera comes with 2 free photos.", "When you''ve used your free photos, you''ll need to buy more. Each roll of film takes five photos. Your Camera will show how much film you have left and loads the next roll automatically."]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["FUNCIONES DE LA CÁMARA\\n\\n1. Pulsa este botón para sacar una foto.\\n2. Cancelar foto: ¡para cuando le cortas la cabeza a tu amigo!\\n3. Acercar y alejar.\\n4. Contador de fotos: muestra cuánto carrete te queda.\\n5. Cuadro de texto: escribe el pie de foto antes de guardarla.\\n6. Guardar: lleva la foto a tu inventario. Puedes regalar fotos a tus amigos o colgarlas en la pared como pósteres."]' AS JSON), '$."pt-br"', CAST('["FUNÇÕES DA CÂMERA\\n\\n1. Aperte este botão para tirar uma foto.\\n2. Cancelar foto: para quando você cortou a cabeça do seu amigo!\\n3. Aproximar e afastar.\\n4. Contador de fotos: mostra quanto filme ainda resta.\\n5. Legenda: escreva a legenda antes de salvar a foto.\\n6. Salvar: leva a foto para o seu inventário. Você pode dar fotos para seus amigos ou pendurá-las na parede como pôsteres."]' AS JSON))
WHERE `ref_page_id` = 132 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["CAMERA FUNCTIONS\\n\\n1. Press this button to take a photo.\\n2. Photo cancel - for when you''ve chopped off your friend''s head!\\n3. Zoom in and out.\\n4. Photo counter - shows how much film you have left.\\n5. Caption box - write your caption before saving the photo.\\n6. Save - this moves the photo to your inventory. You can give photos to your friends, or put them on the wall like posters."]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Pongas como pongas tus muebles esenciales, son los toques finales los que expresan tu verdadera personalidad. Y no lo olvides: como todo lo demás, estos accesorios se pueden mover según tu estado de ánimo.", "Haz clic en un objeto para más información", "Quiero a mi conejo..."]' AS JSON), '$."pt-br"', CAST('["Não importa como você arruma os móveis essenciais, são os toques finais que mostram sua verdadeira personalidade. E não se esqueça: como tudo mais, esses acessórios podem mudar de lugar conforme o seu humor.", "Clique em um item para mais informações", "Eu amo meu coelho..."]' AS JSON))
WHERE `ref_page_id` = 201 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["However you place your essential furniture, it''s the finishing touches that express your true personality. And don''t forget, like everything else, these accessories can be moved about to suit your mood.", "Click on an item for more information", "I love my rabbit..."]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Cambia lo de todos los días por un toque exótico. Biombos tallados, faroles brillantes y azulejos intrincados convierten cualquier sala en un palacio sacado de Las mil y una noches.", "Haz clic en un objeto para más información", "¡Digno de un sultán!"]' AS JSON), '$."pt-br"', CAST('["Troque o dia a dia por um toque exótico. Biombos entalhados, lanternas brilhantes e azulejos detalhados transformam qualquer quarto em um palácio saído das Mil e Uma Noites.", "Clique em um item para mais informações", "Digno de um sultão!"]' AS JSON))
WHERE `ref_page_id` = 202 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Trade the everyday for a taste of the exotic. Carved screens, glowing lanterns and intricate tilework turn any room into a palace straight out of Arabian nights.", "Click on an item for more information", "Fit for a sultan!"]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Mantente fresco (¡o calentito con nuestra fogata!) y crea tu propio País de las Maravillas invernal o un hogar humilde para tus pingüinos.", "Haz clic en el objeto que quieras para más información", ""]' AS JSON), '$."pt-br"', CAST('["Fique fresquinho (ou quentinho com a nossa fogueira!) e crie seu próprio País das Maravilhas de inverno ou um lar humilde para os seus pinguins.", "Clique no item que quiser para mais informações", ""]' AS JSON))
WHERE `ref_page_id` = 203 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Stay cool (or warm with out campfire!) with and create your own Winter Wonderland or Humble Homeland for your Penguins.", "Click on the item you want for more information", ""]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Líneas limpias y robustas hacen de esta colección algo propio del Habbo con los pies en la tierra. Es preciosa en su sencillez y acogedora para todos.", "Haz clic en el objeto que quieras para más información.", "¡Preciosa en su sencillez!"]' AS JSON), '$."pt-br"', CAST('["Linhas limpas e robustas fazem desta coleção a cara do Habbo pé no chão. É linda na sua simplicidade e acolhe todo mundo.", "Clique no item que quiser para mais informações.", "Linda na sua simplicidade!"]' AS JSON))
WHERE `ref_page_id` = 204 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Clean, chunky lines set this collection apart as a preserve of the down-to-earth Habbo. It''s beautiful in its simplicity, and welcoming to everyone.", "Click on the item you want for more information.", "Beautiful in its simplicity!"]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Dale un poco de equilibrio a tu sala con esta colección artesanal inspirada en antiguas tradiciones orientales. Con un toque de Yin y Yang y una pizca de Feng Shui, este furni se siente como en casa en cualquier interior Habbo.", "Haz clic en el objeto que quieras para más información", ""]' AS JSON), '$."pt-br"', CAST('["Traga um pouco de equilíbrio ao seu quarto com esta coleção artesanal inspirada em antigas tradições orientais. Com um toque de Yin e Yang e uma pitada de Feng Shui, estes mobis ficam em casa em qualquer decoração Habbo.", "Clique no item que quiser para mais informações", ""]' AS JSON))
WHERE `ref_page_id` = 205 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Bring a little balance to your room with this handcrafted collection inspired by ancient Eastern traditions. With a touch of Yin and Yang and a dash of Feng Shui, this furni feels right at home in any Habbo interior.", "Click on the item you want for more information", ""]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Diviértete con la alegre colección de baño. Date a ti y a tus invitados un lugar para refrescarse: es vital si quieres evitar malos olores. Aunque quizá sea buena idea poner el váter en un rincón...", "Haz clic en un objeto para más información.", "¡Todo Habbo necesita uno!"]' AS JSON), '$."pt-br"', CAST('["Divirta-se com a alegre coleção de banheiro. Dê a você e aos seus convidados um lugar para se refrescar: é essencial para evitar cheirinhos ruins. Mas talvez seja melhor deixar a privada num canto...", "Clique em um item para mais informações.", "Todo Habbo precisa de um!"]' AS JSON))
WHERE `ref_page_id` = 206 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Have some fun with the cheerful bathroom collection. Give yourself and your guests somewhere to freshen up - vital if you want to avoid nasty niffs. Might be an idea to put your loo in a corner though...", "Click on an item for more information.", "Every Habbo needs one!"]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Nuestro segundo Mundo es la Mitología. Mira este Furni para descubrir qué estamos haciendo en esta mística tierra de maravillas...", "Haz clic en un objeto para ver más detalles."]' AS JSON), '$."pt-br"', CAST('["Nosso segundo Mundo é a Mitologia. Confira estes Mobis para descobrir o que estamos fazendo nesta terra mística de maravilhas...", "Clique em um item para ver mais detalhes."]' AS JSON))
WHERE `ref_page_id` = 207 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Our second World is Mythology. Check this Furni to find out what we''re doing in this mystical land of wonder...", "Click an item for more details."]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["¡Llénalo todo de brillo con mucho glamour! Esperamos que hayas traído tu amuleto de la suerte, porque puede que lo necesites..."]' AS JSON), '$."pt-br"', CAST('["Encha tudo de brilho com muito glamour! Esperamos que você tenha trazido seu amuleto da sorte, porque talvez você precise dele..."]' AS JSON))
WHERE `ref_page_id` = 208 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Bling things up with some serious glitz and glamour! We hope you brought your lucky charm, cause you might just need it..."]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Candy combina las líneas limpias y modernas de la colección Mode con un estilo más suave y relajante. Es la elegancia urbana con un toque femenino.", "Haz clic en un objeto para más información.", "¡Tranquilo! Es piel sintética."]' AS JSON), '$."pt-br"', CAST('["Candy combina as linhas limpas e modernas da coleção Mode com um estilo mais suave e tranquilo. É a elegância urbana com um toque feminino.", "Clique em um item para mais informações.", "Relaxa! É pele sintética."]' AS JSON))
WHERE `ref_page_id` = 209 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Candy combines the coool, clean lines of the Mode collection with a softer, more soothing style. It''s urban sharpness with a hint of the feminine.", "Click an item for more info.", "Relax! It''s faux-fur."]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["El viejo McHabbo tenía una granja... ¡y ahora tendrás todo lo que necesitas para crear el refugio rural perfecto, lejos del ajetreo de la vida en la ciudad!", "Haz clic en un objeto para más información."]' AS JSON), '$."pt-br"', CAST('["O velho McHabbo tinha uma fazenda... e agora você vai ter tudo o que precisa para criar o refúgio rural perfeito, longe da correria da vida na cidade!", "Clique em um item para mais informações."]' AS JSON))
WHERE `ref_page_id` = 210 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Old McHabbo had a farm... and now you will have everything you need to create the perfect rural retreat away from the hustle and bustle of your city slicker life!", "Click on an item for more information."]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Viaja a la América de los años 50 con la colorida gama Diner. Vuelve cada día para ver la especialidad de la casa."]' AS JSON), '$."pt-br"', CAST('["Volte para os Estados Unidos dos anos 50 com a colorida linha Lanchonete. Volte todo dia para ver a especialidade da casa."]' AS JSON))
WHERE `ref_page_id` = 211 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Step into 1950s America with the bright and colourful Diner Range. Check back daily for the latest specialty dish."]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Furni de Pascua: conejitos saltarines, pollitos esponjosos, huevos de chocolate... ¡Sí, es Pascua! Celébralo con algo especial de nuestra gama de Pascua. Pero date prisa: ¡este año no estará mucho tiempo!", "Haz clic en un objeto para ver más detalles.", "¡Huevo-cionante!"]' AS JSON), '$."pt-br"', CAST('["Mobis de Páscoa: coelhinhos saltitantes, pintinhos fofos, ovos de chocolate... Sim, é Páscoa! Comemore com algo especial da nossa linha de Páscoa. Mas corra: este ano ela não fica por muito tempo!", "Clique em um item para ver mais detalhes.", "Ovo-cionante!"]' AS JSON))
WHERE `ref_page_id` = 212 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["''Egg''cellent furni - Bouncing bunnies, fluffy chicks, choccy eggs... Yep, it''s Easter! Celebrate with something ''eggstra'' special from our Easter range. But hurry - it''s not here for long this year!", "Click on an item for more details.", "''Egg''-Tastic!"]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Para el hombre de negocios definitivo, ¡la gama Ejecutiva con sus asientos de cuero y su café italiano es un sueño! ¿Por qué no pruebas la vida fácil?", "Haz clic en un objeto para más información."]' AS JSON), '$."pt-br"', CAST('["Para o executivo definitivo, a linha Executiva com seus assentos de couro e café italiano é um sonho! Que tal experimentar a vida boa?", "Clique em um item para mais informações."]' AS JSON))
WHERE `ref_page_id` = 213 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["For the ultimate business man, the Executive range with its leather seats and Italian coffee is a dream! How about you try the easy lifestyle?", "Click on an item for more information."]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Si te sientes patriota, consigue una bandera para demostrarlo. Nuestras mejores banderas de tela alegrarán hasta las paredes más aburridas.", "Haz clic en el objeto que quieras para más información.", "¡Marca esta sección para más tarde!"]' AS JSON), '$."pt-br"', CAST('["Se você está se sentindo patriota, pegue uma bandeira para provar. Nossas melhores bandeiras de tecido vão alegrar até as paredes mais sem graça.", "Clique no item que quiser para mais informações.", "Marque esta seção para depois!"]' AS JSON))
WHERE `ref_page_id` = 214 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["If you''re feeling patriotic, get a flag to prove it. Our finest cloth flags will brighten up the dullest walls.", "Click on the item you want for more information.", "Flag this section for later!"]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Adorna tus paredes con maravillosas obras de arte, pósteres, placas y tapices. La Galería está llena de objetos para todos los gustos, de lo kitsch a lo moderno, de lo tradicional a lo actual.", "Haz clic en el objeto para más información.", "¡Alegra tus paredes!"]' AS JSON), '$."pt-br"', CAST('["Enfeite suas paredes com obras de arte maravilhosas, pôsteres, placas e tapeçarias. A Galeria está cheia de itens para todos os gostos, do kitsch ao descolado, do tradicional ao moderno.", "Clique no item para mais informações.", "Alegre suas paredes!"]' AS JSON))
WHERE `ref_page_id` = 215 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Adorn your walls with wondrous works of art, posters, plaques and wall hangings. The Gallery is bursting with items to suit all tastes, from kitsch to cool, traditional to modern.", "Click on the item for more information.", "Brighten up your walls!"]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["¡Estilo transparente, opciones de colores! Esta gama de Furni está hecha de plexiglás y viene en muchos colores. Combina tus favoritos para crear un salón moderno.", "Haz clic en un objeto para más información."]' AS JSON), '$."pt-br"', CAST('["Estilo transparente, opções coloridas! Esta linha de Mobis é feita de acrílico e vem em várias cores. Misture suas favoritas para criar uma sala moderna.", "Clique em um item para mais informações."]' AS JSON))
WHERE `ref_page_id` = 216 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Clear style, colourful choices! This Furni range is made from plexiglass and comes in a variety of colours. Mix and match your favourites to create a modern lounge.", "Click on an item for more information."]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["¿Qué acecha bajo la superficie quieta y oscura? Crea tu propio inframundo con estos clásicos embrujados.", "Haz clic en un objeto para ver más detalles."]' AS JSON), '$."pt-br"', CAST('["O que se esconde sob a superfície calma e escura? Crie seu próprio submundo com estes clássicos assombrados.", "Clique em um item para ver mais detalhes."]' AS JSON))
WHERE `ref_page_id` = 217 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["What lurks beneath the still, dark surface? Create your own underworld with these Haunted classics.", "Click on an item for more details."]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["¡Que empiecen los juegos! Construye un estadio olímpico o un templo para los dioses con esta gama de furni inspirada en la antigua Grecia."]' AS JSON), '$."pt-br"', CAST('["Que comecem os jogos! Construa uma arena olímpica ou um templo para os deuses com esta linha de mobis inspirada na Grécia antiga."]' AS JSON))
WHERE `ref_page_id` = 218 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Let the games begin! Build an Olympic arena or a temple for the gods with this furni range inspired by ancient Greece."]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Habbos alternativos con un estilo alternativo, o estudiantes rebeldes con algo que demostrar. ¡La gama Grunge dejará tu dormitorio justo como te gusta: organizado, limpio y ordenado!"]' AS JSON), '$."pt-br"', CAST('["Habbos alternativos com estilo alternativo, ou estudantes rebeldes com algo a provar. A linha Grunge deixa seu quarto do jeitinho que você gosta: organizado, limpo e arrumado!"]' AS JSON))
WHERE `ref_page_id` = 219 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Alternative Habbos with alternative style or rebellious students with a point to prove. The Grunge range will get your bedroom looking just the way you  like it - organized, neat and tidy!"]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["¡Luces, cámara, acción! Ponte bajo los focos con el atrezo perfecto para hacer películas.", "Haz clic en un objeto para ver más detalles."]' AS JSON), '$."pt-br"', CAST('["Luzes, câmera, ação! Fique sob os holofotes com os acessórios perfeitos para fazer filmes.", "Clique em um item para ver mais detalhes."]' AS JSON))
WHERE `ref_page_id` = 220 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Lights, camera, action! Get in the spotlight with perfect props and movie-making.", "Click on an item for more details."]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Pon tus huesudas manos sobre esta escalofriante colección de golosinas macabras y convierte tu sala en una guarida embrujada. ¡Todo lo que necesitas para una buena fiesta de terror!", "Haz clic en un objeto para más información."]' AS JSON), '$."pt-br"', CAST('["Ponha suas mãos ossudas nesta coleção arrepiante de gostosuras macabras e transforme seu quarto num esconderijo assombrado. Tudo o que você precisa para uma boa festa de terror!", "Clique em um item para mais informações."]' AS JSON))
WHERE `ref_page_id` = 221 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Get your bony hands on this creepy collection of ghoulish goodies and turn your room into a haunted hideout. Everything you need for a proper spookfest!", "Click on an item for more information."]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Para el Habbo que no necesita presentación. Es tan chic que lo dice todo y nada a la vez. Es un lienzo negro: ¡deja volar tu imaginación!", "Haz clic en el objeto que quieras para más información", "Estas sillas son comodísimas."]' AS JSON), '$."pt-br"', CAST('["Para o Habbo que dispensa apresentações. É tão chique que diz tudo e nada ao mesmo tempo. É uma tela preta: deixe sua imaginação voar!", "Clique no item que quiser para mais informações", "Essas cadeiras são confortáveis demais."]' AS JSON))
WHERE `ref_page_id` = 222 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["For the Habbo who needs no introduction. It''s so chic, it says everything and nothing. It''s a black canvas, let your imagination to run wild!", "Click on the item you want for more information", "These chairs are so comfy."]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Del sushi a los tatamis y las katanas, ¡dale a tu sala un toque de Japón! Crea una tranquila casa de té, decora tu dojo o abre tu propio bar de sushi con este furni con tanto estilo.", "Haz clic en un objeto para ver más detalles."]' AS JSON), '$."pt-br"', CAST('["Do sushi aos tatames e às katanas, traga um toque do Japão para o seu quarto! Crie uma casa de chá tranquila, decore seu dojô ou abra seu próprio sushi bar com estes mobis cheios de estilo.", "Clique em um item para ver mais detalhes."]' AS JSON))
WHERE `ref_page_id` = 223 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["From sushi to tatami mats and katanas, bring a touch of Japan to your room! Create a peaceful tea house, deck out your dojo or open your own sushi bar with this stylish furniture.", "Click on an item for more details."]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["La Cocina del Hotel ha sido señalada como la causa de una epidemia de intoxicaciones. Piers está aprendiendo higiene alimentaria y te damos la oportunidad de unirte con nuestra fantástica y esperada gama de Furni de Cocina.", "Haz clic en un objeto para más información.", "¿Ratón de chocolate, dices?"]' AS JSON), '$."pt-br"', CAST('["A Cozinha do Hotel foi apontada como a causa de uma epidemia de intoxicação alimentar. O Piers está aprendendo higiene na cozinha e estamos dando a você a chance de entrar nessa com a nossa fantástica e aguardada linha de Mobis de Cozinha.", "Clique em um item para mais informações.", "Rato de chocolate, você disse?"]' AS JSON))
WHERE `ref_page_id` = 224 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["The Hotel Kitchen has been identified as the cause of food poisoning epidemic. Piers is being trained up in food hygiene and we''re giving you the change to join in with our fantastic, eagerly anticipated range of Kitchen Furni.", "Click an item for more info.", "Choccy mouse you say?"]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Para ese efecto de cabaña de esquí, especialmente bonito junto a una chimenea. La colección Lodge es para Habbos que decoran sin florituras y para quienes aprecian la belleza de la madera.", "Haz clic en un objeto para más información.", "¿Qué tal el snowboard hoy?"]' AS JSON), '$."pt-br"', CAST('["Para aquele efeito de chalé de esqui, que fica ainda melhor perto de uma lareira. A coleção Lodge é para Habbos que decoram sem frescura e para quem aprecia a beleza da madeira.", "Clique em um item para mais informações.", "Como foi o snowboard hoje?"]' AS JSON))
WHERE `ref_page_id` = 225 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["For that ski-lodge effect - especially good by an open fire. The Lodge Collection is for Habbos who have a no-frills attitude to home furnishing and those who appreciate the beauty of wood.", "Click on an item for more information.", "How was your boarding today?"]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["La Tribu Perdida ha sido encontrada, pero antes de explorar su mundo, es hora de decidir a qué tribu quieres unirte. ¿Cuál eliges? ¿Los Calamares, los Pájaros o los Simios?", "¡Ya está aquí el Furni de la Tribu Perdida!", "Ha vuelto Tiki, ¿pero con lava?"]' AS JSON), '$."pt-br"', CAST('["A Tribo Perdida foi encontrada, mas antes de explorar o mundo deles, é hora de decidir de qual tribo você quer participar. Qual você escolhe? As Lulas, os Pássaros ou os Símios?", "Os Mobis da Tribo Perdida já chegaram!", "O Tiki voltou, mas com lava?"]' AS JSON))
WHERE `ref_page_id` = 226 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["The Lost Tribe has been found, but before you explore their world, it''s time to decide which tribe you wish to join. Who will you pick? The Squidies, Birdies or Simians?", "Lost Tribe Furni out now!", "Tiki has returned, but with Lava?"]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["¡Todo científico loco necesita un laboratorio! Pociones extrañas, experimentos eléctricos y máquinas misteriosas te esperan en esta misteriosa colección. ¡Construye tu propio laboratorio y descubre a qué puedes dar vida!"]' AS JSON), '$."pt-br"', CAST('["Todo cientista maluco precisa de um laboratório! Poções estranhas, experimentos elétricos e máquinas misteriosas esperam por você nesta coleção misteriosa. Monte seu próprio laboratório e veja o que você consegue dar vida!"]' AS JSON))
WHERE `ref_page_id` = 227 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Every mad scientist needs a lab! Strange potions, electric experiments and mysterious machines await in this mysterious collection. Build your own laboratory and see what you can bring to life!"]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Metal gris acerado de estilo industrial con un diseño elegante. La gama perfecta para quien vive en la ciudad y se la sabe todas.", "Haz clic en un objeto para más información.", "Tan brillante y nuevo."]' AS JSON), '$."pt-br"', CAST('["Metal cinza aço de padrão industrial com um design elegante. A linha perfeita para quem mora na cidade e sabe das coisas.", "Clique em um item para mais informações.", "Tão brilhante e novinho."]' AS JSON))
WHERE `ref_page_id` = 228 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Steely, grey industrial standard metal with a sleek design. The perfect range for a streetwise city dweller.", "Click on an item for more information.", "So shiny and new."]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Nuestra gama de Reguladores de Luces te permite transformar tu sala de forma espectacular con solo un clic. ¿Cómo se verá tu sala hoy? Pulsa el interruptor y descúbrelo ahora.", "Haz clic en un objeto para más información."]' AS JSON), '$."pt-br"', CAST('["Nossa linha de Reguladores de Luzes transforma seu quarto de forma espetacular com um só clique. Como vai ficar o seu quarto hoje? Aperte o interruptor e descubra agora.", "Clique em um item para mais informações."]' AS JSON))
WHERE `ref_page_id` = 229 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Our range of Mood Lights allow you to spectacularly transform your room in just a click. What will your room look like today? Click the switch and find out now.", "Click on an item for more information."]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["¡Alegría para los fiesteros! Siente el bajo y alcanza los láseres con la gama de Furni Neón: ¡todo lo que necesitas para empezar la fiesta!"]' AS JSON), '$."pt-br"', CAST('["Alegria para os baladeiros! Sinta o grave e alcance os lasers com a linha de Mobis Neon: tudo o que você precisa para começar a festa!"]' AS JSON))
WHERE `ref_page_id` = 230 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Party animals rejoice! Feel the base and reach for the lasers with the Neon Furni range - everything you need to get the party started!"]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["¡Dale un toque psicodélico a tu sala con Organo! Formas atrevidas y curvas fluidas le dan a esta gama moderna un estilo único.", "", "¿Estoy alucinando?"]' AS JSON), '$."pt-br"', CAST('["Dê um toque psicodélico ao seu quarto com Organo! Formas ousadas e curvas fluidas dão a esta linha moderna um estilo único.", "", "Estou viajando?"]' AS JSON))
WHERE `ref_page_id` = 231 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Add a touch of psychedelia to your room with Organo! Bold shapes and flowing curves give this modern furni range a style all its own.", "", "Am I tripping?"]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["¡Toda sala necesita una planta! No solo traen un poco del exterior al interior, ¡también mejoran la calidad del aire!", "Haz clic en una planta para ver más detalles.", "¡Geo-me-tría!"]' AS JSON), '$."pt-br"', CAST('["Todo quarto precisa de uma planta! Elas não só trazem um pouco da natureza para dentro, como também melhoram a qualidade do ar!", "Clique em uma planta para ver mais detalhes.", "Geo-me-tria!"]' AS JSON))
WHERE `ref_page_id` = 232 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Every room needs a plant! Not only do they bring a bit of the outside inside, they also enhance the air quality!", "Click on a plant to see more details.", "Ge-om-e-tree!"]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["¿Sientes esa vibra de los años 70? Decora con Plasto y añade color a tu vida. Elige un color que refleje tu estado de ánimo, o simplemente tu tono favorito."]' AS JSON), '$."pt-br"', CAST('["Está sentindo essa vibe dos anos 70? Decore com Plasto e coloque cor na sua vida. Escolha uma cor que combine com o seu humor, ou simplesmente o seu tom favorito."]' AS JSON))
WHERE `ref_page_id` = 233 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Can you feel that 1970s vibe? Decorate with Plasto and add some colour to your life. Choose a colour that reflects your mood, or just pick your favourite shade."]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Tiendas, cafeterías y amplios pasillos: ¡todo lo que necesitas para crear tu propio centro comercial! Monta tus escaparates y dale a los Habbos un lugar para comprar, quedar y pasar el rato."]' AS JSON), '$."pt-br"', CAST('["Lojas, cafés e corredores amplos: tudo o que você precisa para criar seu próprio shopping! Monte suas vitrines e dê aos Habbos um lugar para fazer compras, se encontrar e passar o tempo."]' AS JSON))
WHERE `ref_page_id` = 234 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Shops, cafés and spacious walkways - everything you need to create your own mall! Set up your storefronts and give Habbos a place to shop, meet and hang out."]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["La gama más limpia y fresca. Casi puedes oírla respirar frescura y tranquilidad en tu sala. Úsala para crear un refugio lejos del ajetreo de Habbo Hotel.", "Haz clic en un objeto para más información.", "¡Cuántos colores!"]' AS JSON), '$."pt-br"', CAST('["A linha mais limpa e fresca. Dá quase para ouvir o frescor e a tranquilidade dela no seu quarto. Use para criar um refúgio longe da agitação do Habbo Hotel.", "Clique em um item para mais informações.", "Quantas cores!"]' AS JSON))
WHERE `ref_page_id` = 235 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["The cleanest, freshest range. You can almost hear it breath cool and tranquility within your room. Use it to create a haven away from the hectic lifestyle of Habbo Hotel.", "Click on an item for more information.", "So many colors!"]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Crea ambiente con nuestro furni Relax. Llena tu sala de velas aromáticas, acomódate en una silla colgante y mira cómo parpadean las llamas. ¡Pon los pies en alto y deja que tus preocupaciones se vayan flotando!"]' AS JSON), '$."pt-br"', CAST('["Crie o clima com os nossos mobis Relax. Encha seu quarto de velas aromáticas, acomode-se numa cadeira suspensa e veja as chamas tremularem. Coloque os pés para cima e deixe as preocupações irem embora!"]' AS JSON))
WHERE `ref_page_id` = 236 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Set the mood with our Relax furni. Fill your room with scented candles, settle into a hanging chair and watch the flames flicker. Put your feet up and let your worries drift away!"]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["La gama Romantique tiene pianos de cola, lámparas y mesas antiguas. Es la gama ideal para crear un ambiente cálido y cariñoso en tu sala. Arregla tu sala e invita a esa persona especial.", "Haz clic en el objeto que quieras para más información."]' AS JSON), '$."pt-br"', CAST('["A linha Romantique tem pianos de cauda, abajures e mesas antigas. É a linha ideal para criar um clima acolhedor e carinhoso no seu quarto. Dê um trato no quarto e convide aquela pessoa especial.", "Clique no item que quiser para mais informações."]' AS JSON))
WHERE `ref_page_id` = 237 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["The Romantique range features Grand Pianos, old antique lamps and tables. It is the ideal range for setting a warm and loving mood in your room. Spruce up your room and invite that special someone over.", "Click on the item you want for more information."]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Tenemos alfombras para cada ocasión. Todas son antideslizantes y lavables.", "Haz clic en el objeto que quieras para más información", "¡Alfombras, alfombras y más alfombras!"]' AS JSON), '$."pt-br"', CAST('["Temos tapetes para todas as ocasiões. Todos são antiderrapantes e laváveis.", "Clique no item que quiser para mais informações", "Tapetes, tapetes e mais tapetes!"]' AS JSON))
WHERE `ref_page_id` = 238 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["We have rugs for all ocassions. All rugs are non-slip and washable.", "Click on the item you want for more information", "Rugs, rugs and more rugs!"]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["¡Este Furni nunca pasa de moda! Conviértete en un icono de estilo, diseña tu propia colección o arrasa en la pasarela. ¿Tienes lo que hace falta para ser una estrella de la Pasarela Habbo?"]' AS JSON), '$."pt-br"', CAST('["Estes Mobis nunca saem de moda! Vire um ícone de estilo, crie sua própria coleção ou arrase na passarela. Você tem o que é preciso para ser uma estrela da Passarela Habbo?"]' AS JSON))
WHERE `ref_page_id` = 239 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["This Furni never goes out of style! Become a style icon, design your own collection or take the catwalk by storm. Have you got what it takes to be a Habbo Runway star?"]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Crea lo último en viajes espaciales, ya sea una guerra galáctica o un tranquilo rincón lunar para pasar el rato.", "Haz clic en un objeto para ver más detalles."]' AS JSON), '$."pt-br"', CAST('["Crie o máximo em viagens espaciais, seja uma guerra galáctica ou só um cantinho lunar para relaxar.", "Clique em um item para ver mais detalhes."]' AS JSON))
WHERE `ref_page_id` = 240 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Create the ultimate in space travel whether it be a galactic war or just a leisure lunar hangout.", "Click on an item to get more info."]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["¡Prepara el escenario para una historia de amor de Bollywood con Shalimar! Colores vivos, flores de loto y elefantes llevarán la magia del cine a tu sala. ¡Añade pétalos de rosa y que empiecen la música, el baile y el romance!", "Haz clic en un objeto para ver más detalles."]' AS JSON), '$."pt-br"', CAST('["Prepare o cenário para uma história de amor de Bollywood com Shalimar! Cores vivas, flores de lótus e elefantes vão levar a magia do cinema ao seu quarto. Jogue pétalas de rosa e que comecem a música, a dança e o romance!", "Clique em um item para ver mais detalhes."]' AS JSON))
WHERE `ref_page_id` = 241 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Set the scene for a Bollywood love story with Shalimar! Bright colours, lotus flowers and elephants will bring movie magic to your room. Add rose petals and let the music, dancing and romance begin!", "Click on an item for more details."]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["¿Con ganas de moverte? ¡Consigue aquí tu equipo deportivo y quema esas calorías! Desde pistas de atletismo hasta canchas de baloncesto, tenemos todo lo que necesitas para entrenar, competir y organizar tus propios juegos.", "Haz clic en un objeto para ver más detalles."]' AS JSON), '$."pt-br"', CAST('["Com vontade de se mexer? Pegue aqui seu equipamento esportivo e queime essas calorias! De pistas de corrida a quadras de basquete, temos tudo o que você precisa para treinar, competir e organizar seus próprios jogos.", "Clique em um item para ver mais detalhes."]' AS JSON))
WHERE `ref_page_id` = 242 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Feeling active? Get your sports gear here and burn those calories! From running tracks to basketball courts, we''ve got everything you need to train, compete and host your own games.", "Click on an item for more details."]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["¡Es verano! Los Habbos se van a destinos preciosos para disfrutar del sol, el mar y la arena. ¡Construye tu propio complejo de playa y monta las mejores fiestas de verano de Habbo!", "Haz clic en el objeto que quieras para más información", ""]' AS JSON), '$."pt-br"', CAST('["É verão! Os Habbos estão indo para destinos lindos curtir o sol, o mar e a areia. Monte seu próprio resort de praia e faça as melhores festas de verão do Habbo!", "Clique no item que quiser para mais informações", ""]' AS JSON))
WHERE `ref_page_id` = 243 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Summertime! Habbos are heading for beautiful destinations to enjoy the sun, sea and sand. Build your own beach resort and throw the best summer parties in Habbo!", "Click on the item you want for more information", ""]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Teletranspórtate de una sala a otra con nuestros teletransportes de la era espacial. Compra un par, pon uno en cada sala ¡y tendrás tu propia suite! Los teletransportes se venden en pares (¡obviamente!), así que si los consigues en un tradeo, comprueba que sea un par conectado.", "Haz clic en un objeto para más información.", ""]' AS JSON), '$."pt-br"', CAST('["Teleporte-se de um quarto para outro com os nossos teleportes da era espacial. Compre um par, coloque um em cada quarto e pronto: você tem sua própria suíte! Os teleportes são vendidos em pares (é claro!), então se você conseguir numa troca, confira se é um par conectado.", "Clique em um item para mais informações.", ""]' AS JSON))
WHERE `ref_page_id` = 244 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Beam yourself from one room to another with our space age teleports. Buy a pair, put one in each room, and you''ve got your own suite! Teleports are sold in pairs (obviously!), so if you trade for them, check that you''re getting a linked pair.", "Click on an item for more information.", ""]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Escápate a un paraíso tropical con el mobiliario Tiki. Los suaves tonos de la madera y los colores vivos del trópico son el escenario perfecto para una fiesta en la playa."]' AS JSON), '$."pt-br"', CAST('["Fuja para um paraíso tropical com os mobis Tiki. Os tons suaves da madeira e as cores vivas dos trópicos são o cenário perfeito para uma festa na praia."]' AS JSON))
WHERE `ref_page_id` = 245 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Whisk yourself away to a tropical paradise with Tiki furniture. The smooth wood tones and tropical brights make the perfect setting for a beach party."]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["¡Todos son ganadores! Premia a todos tus amigos con nuestra fabulosa colección de brillantes trofeos de bronce, plata y oro.\\r\\nPrimero elige el modelo de trofeo y el metal, y luego escribe con cuidado tu dedicatoria: ¡es permanente!\\r\\nGrabaremos la dedicatoria junto con tu nombre y la fecha."]' AS JSON), '$."pt-br"', CAST('["Todo mundo é vencedor! Premie todos os seus amigos com a nossa fabulosa coleção de troféus brilhantes de bronze, prata e ouro.\\r\\nPrimeiro escolha o modelo do troféu e o metal, depois escreva sua dedicatória com cuidado: ela é permanente!\\r\\nVamos gravar a dedicatória junto com o seu nome e a data."]' AS JSON))
WHERE `ref_page_id` = 246 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Everyone''s a winner! Reward all your friends with our fabulous glittering array of bronze, silver and gold trophies.\\r\\nFirst, choose the trophy model, the metal and then carefully type your inscription - it''s permanent!.\\r\\nWe''ll engrave the inscription along with your name and the date."]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Con su estilo inspirado en la ciudad, Urbano es perfecto para cualquier calle, callejón o carretera. Contenedores, farolas y bancos: ¡todo el mobiliario Urbano que necesitas!", "Haz clic en un objeto para ver más detalles."]' AS JSON), '$."pt-br"', CAST('["Com seu estilo inspirado na cidade, Urbano é perfeito para qualquer rua, beco ou estrada. Lixeiras, postes e bancos: todos os mobis Urbanos de que você precisa!", "Clique em um item para ver mais detalhes."]' AS JSON))
WHERE `ref_page_id` = 247 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["With its city-inspired style, Urban is perfect for any street, alleyway or road. Rubbish bins, street lights and benches - all the Urban furniture you need!", "Click on an item for more details."]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Es San Valentín y hora de expresar tu cariño y tu amor a tus amigos. ¡Desmádrate y deja Notas Corazón anónimas por todo el hotel!", "Haz clic en un objeto para ver más detalles"]' AS JSON), '$."pt-br"', CAST('["É Dia dos Namorados e hora de mostrar seu carinho e seu amor pelos seus amigos. Solte a imaginação e deixe Bilhetes de Coração anônimos pelo hotel todo!", "Clique em um item para ver mais detalhes"]' AS JSON))
WHERE `ref_page_id` = 248 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["It is Valentine''s Day and time to express your love and affection for your friends. Go wild and leave anonymous Heart Stickies all over the hotel!", "Click on an item to see more details"]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Un virus se está extendiendo por Habbo Hotel. Hay muchas víctimas y ¡podría ir a peor! Tanto si quieres contagiar a otros Habbos como si prefieres cuidarte, consigue nuestro terrorífico Furni de la Infección con las manos limpias.", "Haz clic en un objeto para ver más detalles."]' AS JSON), '$."pt-br"', CAST('["Um vírus está se espalhando pelo Habbo Hotel. Muitas vítimas foram registradas e pode piorar! Se você quer infectar outros Habbos ou só se proteger, pegue nossos aterrorizantes Mobis da Infecção com as mãos limpas.", "Clique em um item para ver mais detalhes."]' AS JSON))
WHERE `ref_page_id` = 249 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["A virus is spreading through Habbo Hotel. Many casualties reported and it could get much worse! Whether you are hoping to get infected Habbos or look after number one, get your clean hands on our terrifying Infection Furni.", "Click on an item to see more details."]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["¡Deja entrar un poco de sol! Nuestras ventanas vienen en muchos estilos para darle a tu sala un aspecto único. ¿Quién dijo que tu sala no tiene vistas? Ahora sí las tiene.", "Haz clic en el objeto que quieras para más información", "¡Oh, vistas nuevas!"]' AS JSON), '$."pt-br"', CAST('["Deixe o sol entrar! Nossas janelas vêm em vários estilos para dar um visual único ao seu quarto. Quem disse que o seu quarto não tem vista? Agora tem.", "Clique no item que quiser para mais informações", "Uau, vista nova!"]' AS JSON))
WHERE `ref_page_id` = 250 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Let some sunshine in! Our windows come in many styles to give a unique look to your room. Who said your room does not have a view? It does now.", "Click on the item you want for more information", "Ooh, new view!"]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Ponte en el espíritu navideño y descubre todos los objetos nuevos que hemos preparado para ti.", "Haz clic en el objeto que quieras para más información", ""]' AS JSON), '$."pt-br"', CAST('["Entre no espírito natalino e confira todos os itens novos que preparamos para você.", "Clique no item que quiser para mais informações", ""]' AS JSON))
WHERE `ref_page_id` = 251 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Get yourself into the Christmas spirit and check out all the new items we''ve prepared for you.", "Click on the item you want for more information", ""]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["El Hotel ha sido invadido por esta explosión botánica de verde y color. Con estas nuevas incorporaciones de setas, ¿qué vas a construir?"]' AS JSON), '$."pt-br"', CAST('["O Hotel foi tomado por esta explosão botânica de verde e cor. Com estes novos cogumelos, o que você vai construir?"]' AS JSON))
WHERE `ref_page_id` = 252 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["The Hotel has been overrun by this botanical bonanza of greenery and color. With these newest shroomy additions, what will you build?"]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["¡Mueve tu imaginación mientras mueves a tu Habbo! Perfectos para laberintos, juegos, para que tu cola avance o para que tu mascota dé vueltas durante horas. Se venden en paquetes: ¡cuantos más compras, más barato sale cada Roller!", "Haz clic en un Roller para más información."]' AS JSON), '$."pt-br"', CAST('["Movimente sua imaginação enquanto movimenta seu Habbo! Perfeitos para labirintos, jogos, para fazer a fila andar ou deixar seu mascote dando voltas por horas. Vendidos em pacotes: quanto mais você compra, mais barato sai cada Roller!", "Clique em um Roller para mais informações."]' AS JSON))
WHERE `ref_page_id` = 253 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Move your imagination, while you move your Habbo! Perfect for mazes, games, for keeping your queue moving or making your pet go round in circles for hours. Available in multi-packs - the more you buy, the cheaper the Roller!", "Click on a Roller for more information."]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["¡La Saga Crepúsculo: Luna nueva ya está aquí! Para celebrar la llegada de los Cullen, hemos preparado un furni especial para que crees tus propias salas de Crepúsculo."]' AS JSON), '$."pt-br"', CAST('["A Saga Crepúsculo: Lua Nova chegou! Para comemorar a chegada dos Cullen, criamos mobis especiais para você montar seus próprios quartos de Crepúsculo."]' AS JSON))
WHERE `ref_page_id` = 254 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["The Twilight Saga - New Moon is here! To celebrate the arrival of the Cullens, some special furniture has been made so you can create your own Twilight rooms."]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Crea tu propio mundo de American Idol en Habbo con este set exclusivo de furni de AI.", "Haz clic en un objeto para ver más detalles."]' AS JSON), '$."pt-br"', CAST('["Crie seu próprio mundo do American Idol no Habbo com este conjunto exclusivo de mobis do AI.", "Clique em um item para ver mais detalhes."]' AS JSON))
WHERE `ref_page_id` = 255 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Create your own American Idol world in Habbo with this exclusive AI furniture set.", "Click on an item to see more details."]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["La Exposición de Spiderwick ha llegado al \\"Museo de los Inventos\\" de Habbo Hotel. ¡Consigue abajo un recuerdo de Furni de edición limitada antes de que sea demasiado tarde!"]' AS JSON), '$."pt-br"', CAST('["A Exposição de Spiderwick chegou ao \\"Museu das Invenções\\" do Habbo Hotel. Garanta abaixo uma lembrança em Mobi de edição limitada antes que seja tarde demais!"]' AS JSON))
WHERE `ref_page_id` = 257 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["The Spiderwick Exhibition has arrived at the \\"Museum of Invention\\" in Habbo Hotel. Grab yourself a limited edition souvenir item of Furni below before it''s too late!"]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["Necesitas una Traxmachine y al menos un trax-pax. Pon la Traxmachine en tu sala y el trax-pax en tu inventario. Haz doble clic en la Traxmachine, haz clic en \\"Editor de Trax\\" y empieza a editar música. Cuando esté lista, guárdala, elige la canción, enciende la Traxmachine y disfruta.", "¡Genial, mi propia música!"]' AS JSON), '$."pt-br"', CAST('["Você precisa de uma Traxmachine e de pelo menos um trax-pax. Coloque a Traxmachine no seu quarto e o trax-pax no seu inventário. Clique duas vezes na Traxmachine, clique em \\"Editor de Trax\\" e comece a editar música. Quando estiver pronta, salve, escolha a música, ligue a Traxmachine e curta.", "Demais, minha própria música!"]' AS JSON))
WHERE `ref_page_id` = 1101 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["You must own a Traxmachine and at least one trax-pax. Place the Traxmachine in your room and the trax-pax in your inventory. Double click the Traxmachine, click \\"Trax Editor\\" and start editing music. When ready, save, select the tune, turn on the Traxmachine and enjoy.", "Cool, my own music!"]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["¡Bienvenido a la Tienda de Trax Ambient! Con ritmos suaves y melodías relajadas, esta es la sección para unas canciones tranquilas y geniales."]' AS JSON), '$."pt-br"', CAST('["Bem-vindo à Loja de Trax Ambient! Com batidas suaves e melodias relaxantes, esta é a seção para músicas tranquilas e legais."]' AS JSON))
WHERE `ref_page_id` = 1102 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Welcome to the Ambient Trax Store! With groovy beats and chilled out melodies, this is the section for some cool and relaxing tunes."]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["¡Bienvenido a la Tienda de Trax Dance! Con ritmos funky y melodías pegadizas, esta es la sección para que todo fiestero lo dé todo."]' AS JSON), '$."pt-br"', CAST('["Bem-vindo à Loja de Trax Dance! Com batidas funky e melodias chiclete, esta é a seção para todo baladeiro se esbaldar."]' AS JSON))
WHERE `ref_page_id` = 1103 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Welcome to the Dance Trax Store! With funky beats and catchy melodies, this is the section for every clubber to indulge in."]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["¡Bienvenido a la Tienda de Trax Rock! Con ritmos potentes y riffs rockeros, esta es la sección para que todo fan del rock experimente."]' AS JSON), '$."pt-br"', CAST('["Bem-vindo à Loja de Trax Rock! Com batidas pesadas e riffs de rock, esta é a seção para todo fã de rock experimentar."]' AS JSON))
WHERE `ref_page_id` = 1104 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Welcome to the Rock Trax Store! With heavy beats and rockin'' riffs, this is the section for every rock fan to experiment with."]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["¡Bienvenido a la Tienda de Trax SFX! Con sonidos locos y ruidos raros, esta es la sección para que todo creador de salas dé rienda suelta a su creatividad."]' AS JSON), '$."pt-br"', CAST('["Bem-vindo à Loja de Trax SFX! Com sons malucos e barulhos estranhos, esta é a seção para todo criador de quartos soltar a criatividade."]' AS JSON))
WHERE `ref_page_id` = 1105 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Welcome to the SFX Trax Store! With crazy sounds and weird noises, this is the section for every creative room builder to indulge in."]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["¡Bienvenido a la Tienda de Trax Urban! Con ritmos de hip hop y voces de RnB, esta es la sección para que todo urbanita lo disfrute a tope."]' AS JSON), '$."pt-br"', CAST('["Bem-vindo à Loja de Trax Urban! Com batidas de hip hop e vocais de RnB, esta é a seção para todo morador da cidade curtir à vontade."]' AS JSON))
WHERE `ref_page_id` = 1106 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Welcome to the Urban Trax Store! With hip hop beats and RnB vocals, this is the section for every city bopper to indulge in."]' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_texts` = JSON_SET(`localization_texts`, '$.es', CAST('["¿Un temazo o directo al manicomio de Khia? Elige tus éxitos favoritos, junta a tus amigos y canta como si nadie te escuchara."]' AS JSON), '$."pt-br"', CAST('["Hino certificado ou direto pro hospício da Khia? Escolha seus hits favoritos, chame os amigos e cante como se ninguém estivesse ouvindo."]' AS JSON))
WHERE `ref_page_id` = 1107 AND JSON_EXTRACT(`localization_texts`, '$.en') = CAST('["Certified banger or straight to the Khia asylum? Pick your favourite hits, grab your friends and sing like nobody''s listening."]' AS JSON);

UPDATE `definition_catalog_page_header` SET `display_name` = CAST('{"en": "Front Page", "es": "Inicio", "pt-br": "Principal"}' AS JSON)
WHERE `id` = 1 AND `display_name` = CAST('{"en": "Front Page", "es": "Portada", "pt-br": "Primeira Página"}' AS JSON);
UPDATE `definition_catalog_page_header` SET `display_name` = CAST('{"en": "Furni Shop", "es": "Mobiliario", "pt-br": "Mobis"}' AS JSON)
WHERE `id` = 2 AND `display_name` = CAST('{"en": "Furni Shop", "es": "Tienda de Furni", "pt-br": "Loja de Mobis"}' AS JSON);
UPDATE `definition_catalog_page_header` SET `display_name` = CAST('{"en": "Pixel Shop", "es": "Tienda Píxel", "pt-br": "Loja de Pixels"}' AS JSON)
WHERE `id` = 3 AND `display_name` = CAST('{"en": "Pixel Shop", "es": "Tienda de Píxeles", "pt-br": "Loja de Pixels"}' AS JSON);
UPDATE `definition_catalog_page_header` SET `display_name` = CAST('{"en": "Habbo Exchange", "es": "Habbo Cambio", "pt-br": "Habbo Câmbio"}' AS JSON)
WHERE `id` = 4 AND `display_name` = CAST('{"en": "Habbo Exchange", "es": "Casa de Cambio", "pt-br": "Casa de Câmbio"}' AS JSON);
UPDATE `definition_catalog_page_header` SET `color` = 2, `display_name` = CAST('{"en": "Pets Shop", "es": "Mascotas", "pt-br": "Mascotes"}' AS JSON), `enabled` = 0
WHERE `id` = 6 AND `color` = 7 AND `display_name` = CAST('{"en": "Pets Shop", "es": "Tienda de Mascotas", "pt-br": "Loja de Mascotes"}' AS JSON) AND `enabled` = 1;
UPDATE `definition_catalog_page_header` SET `enabled` = 0
WHERE `id` = 9 AND `enabled` = 1;
UPDATE `definition_catalog_page_header` SET `min_rank` = 1
WHERE `id` = 11 AND `min_rank` = 0;
UPDATE `definition_catalog_page_header` SET `icon` = 502, `enabled` = 0
WHERE `id` = 13 AND `icon` = 0 AND `enabled` = 1;
UPDATE `definition_catalog_page_header` SET `display_name` = CAST('{"en": "Spaces", "es": "Tapizados", "pt-br": "Decoração"}' AS JSON)
WHERE `id` = 25 AND `display_name` = CAST('{"en": "Spaces", "es": "Espacios", "pt-br": "Espaços"}' AS JSON);
UPDATE `definition_catalog_page_header` SET `display_name` = CAST('{"en": "Rentals", "es": "Alquiler", "pt-br": "Aluguéis"}' AS JSON)
WHERE `id` = 31 AND `display_name` = CAST('{"en": "Rentals", "es": "Alquileres", "pt-br": "Aluguéis"}' AS JSON);
UPDATE `definition_catalog_page_header` SET `display_name` = CAST('{"en": "Special Effects", "es": "Efectos Especiales", "pt-br": "Efeitos"}' AS JSON)
WHERE `id` = 32 AND `display_name` = CAST('{"en": "Special Effects", "es": "Efectos especiales", "pt-br": "Efeitos especiais"}' AS JSON);
UPDATE `definition_catalog_page_header` SET `display_name` = CAST('{"en": "Hello Furni", "es": "Furni Bienvenida", "pt-br": "Hello"}' AS JSON)
WHERE `id` = 33 AND `display_name` = CAST('{"en": "Hello Furni", "es": "Hello Furni", "pt-br": "Hello Furni"}' AS JSON);
UPDATE `definition_catalog_page_header` SET `display_name` = CAST('{"en": "Automobile", "es": "Automóvil", "pt-br": "Automóveis"}' AS JSON)
WHERE `id` = 34 AND `display_name` = CAST('{"en": "Automobile", "es": "Automóvil", "pt-br": "Automóvel"}' AS JSON);
UPDATE `definition_catalog_page_header` SET `display_name` = CAST('{"en": "Rewards", "es": "Recompensas", "pt-br": "Recompensas"}' AS JSON)
WHERE `id` = 52 AND `display_name` = CAST('{"en": "Rewards", "es": "Premios", "pt-br": "Prêmios"}' AS JSON);
UPDATE `definition_catalog_page_header` SET `display_name` = CAST('{"en": "Dogs", "es": "Perros", "pt-br": "Cães"}' AS JSON)
WHERE `id` = 61 AND `display_name` = CAST('{"en": "Dogs", "es": "Perros", "pt-br": "Cachorros"}' AS JSON);
UPDATE `definition_catalog_page_header` SET `display_name` = CAST('{"en": "Crocodiles", "es": "Cocodrilos", "pt-br": "Jacarés"}' AS JSON)
WHERE `id` = 63 AND `display_name` = CAST('{"en": "Crocodiles", "es": "Cocodrilos", "pt-br": "Crocodilos"}' AS JSON);
UPDATE `definition_catalog_page_header` SET `display_name` = CAST('{"en": "Terriers", "es": "Terriers", "pt-br": "Terrier"}' AS JSON)
WHERE `id` = 64 AND `display_name` = CAST('{"en": "Terriers", "es": "Terriers", "pt-br": "Terriers"}' AS JSON);
UPDATE `definition_catalog_page_header` SET `display_name` = CAST('{"en": "Pigs", "es": "Jabalíes", "pt-br": "Porquinhos"}' AS JSON)
WHERE `id` = 65 AND `display_name` = CAST('{"en": "Pigs", "es": "Cerdos", "pt-br": "Porcos"}' AS JSON);
UPDATE `definition_catalog_page_header` SET `display_name` = CAST('{"en": "Pet Accessories", "es": "Accesorios Mascotas", "pt-br": "Para Mascotes"}' AS JSON)
WHERE `id` = 67 AND `display_name` = CAST('{"en": "Pet Accessories", "es": "Accesorios para mascotas", "pt-br": "Acessórios para mascotes"}' AS JSON);
UPDATE `definition_catalog_page_header` SET `display_name` = CAST('{"en": "Pets Information", "es": "Info de Mascotas", "pt-br": "Bom saber"}' AS JSON)
WHERE `id` = 68 AND `display_name` = CAST('{"en": "Pets Information", "es": "Información de mascotas", "pt-br": "Informações sobre mascotes"}' AS JSON);
UPDATE `definition_catalog_page_header` SET `display_name` = CAST('{"en": "Buy Habbo Club", "es": "Hazte HC", "pt-br": "Entrar no Club"}' AS JSON)
WHERE `id` = 71 AND `display_name` = CAST('{"en": "Buy Habbo Club", "es": "Comprar Habbo Club", "pt-br": "Assinar o Habbo Club"}' AS JSON);
UPDATE `definition_catalog_page_header` SET `display_name` = CAST('{"en": "Club Gifts", "es": "Regalos HC", "pt-br": "Presentes do Club"}' AS JSON)
WHERE `id` = 72 AND `display_name` = CAST('{"en": "Club Gifts", "es": "Regalos del Club", "pt-br": "Presentes do Club"}' AS JSON);
UPDATE `definition_catalog_page_header` SET `display_name` = CAST('{"en": "Club Shop", "es": "Tienda HC", "pt-br": "Loja do Club"}' AS JSON)
WHERE `id` = 73 AND `display_name` = CAST('{"en": "Club Shop", "es": "Tienda del Club", "pt-br": "Loja do Club"}' AS JSON);
UPDATE `definition_catalog_page_header` SET `display_name` = CAST('{"en": "My Sales", "es": "Mis Ventas", "pt-br": "Minhas vendas"}' AS JSON)
WHERE `id` = 101 AND `display_name` = CAST('{"en": "My Sales", "es": "Mis ventas", "pt-br": "Minhas vendas"}' AS JSON);
UPDATE `definition_catalog_page_header` SET `display_name` = CAST('{"en": "Accessories", "es": "Extras", "pt-br": "Acessórios"}' AS JSON)
WHERE `id` = 201 AND `display_name` = CAST('{"en": "Accessories", "es": "Accesorios", "pt-br": "Acessórios"}' AS JSON);
UPDATE `definition_catalog_page_header` SET `display_name` = CAST('{"en": "Arctic", "es": "Arctic", "pt-br": "Arctic"}' AS JSON)
WHERE `id` = 203 AND `display_name` = CAST('{"en": "Arctic", "es": "Ártico", "pt-br": "Ártico"}' AS JSON);
UPDATE `definition_catalog_page_header` SET `display_name` = CAST('{"en": "Asian", "es": "Asia", "pt-br": "Asian"}' AS JSON)
WHERE `id` = 205 AND `display_name` = CAST('{"en": "Asian", "es": "Asiático", "pt-br": "Asiático"}' AS JSON);
UPDATE `definition_catalog_page_header` SET `display_name` = CAST('{"en": "Country", "es": "Country", "pt-br": "São João"}' AS JSON)
WHERE `id` = 210 AND `display_name` = CAST('{"en": "Country", "es": "Campestre", "pt-br": "Fazenda"}' AS JSON);
UPDATE `definition_catalog_page_header` SET `display_name` = CAST('{"en": "Diner", "es": "Diner", "pt-br": "Diner"}' AS JSON)
WHERE `id` = 211 AND `display_name` = CAST('{"en": "Diner", "es": "Diner", "pt-br": "Lanchonete"}' AS JSON);
UPDATE `definition_catalog_page_header` SET `display_name` = CAST('{"en": "Executive", "es": "Executive", "pt-br": "Executive"}' AS JSON)
WHERE `id` = 213 AND `display_name` = CAST('{"en": "Executive", "es": "Ejecutivo", "pt-br": "Executivo"}' AS JSON);
UPDATE `definition_catalog_page_header` SET `display_name` = CAST('{"en": "Glass", "es": "Cristal", "pt-br": "Glass"}' AS JSON)
WHERE `id` = 216 AND `display_name` = CAST('{"en": "Glass", "es": "Cristal", "pt-br": "Vidro"}' AS JSON);
UPDATE `definition_catalog_page_header` SET `display_name` = CAST('{"en": "Grunge", "es": "Grunge", "pt-br": "Mundrungo"}' AS JSON)
WHERE `id` = 219 AND `display_name` = CAST('{"en": "Grunge", "es": "Grunge", "pt-br": "Grunge"}' AS JSON);
UPDATE `definition_catalog_page_header` SET `display_name` = CAST('{"en": "Lodge", "es": "Rústico", "pt-br": "Rústico"}' AS JSON)
WHERE `id` = 225 AND `display_name` = CAST('{"en": "Lodge", "es": "Lodge", "pt-br": "Lodge"}' AS JSON);
UPDATE `definition_catalog_page_header` SET `display_name` = CAST('{"en": "Mood Lights", "es": "Iluminación", "pt-br": "Reguladores de Luz"}' AS JSON)
WHERE `id` = 229 AND `display_name` = CAST('{"en": "Mood Lights", "es": "Reguladores de Luces", "pt-br": "Reguladores de Luzes"}' AS JSON);
UPDATE `definition_catalog_page_header` SET `display_name` = CAST('{"en": "Plasto", "es": "Plastic", "pt-br": "Plastic"}' AS JSON)
WHERE `id` = 233 AND `display_name` = CAST('{"en": "Plasto", "es": "Plasto", "pt-br": "Plasto"}' AS JSON);
UPDATE `definition_catalog_page_header` SET `display_name` = CAST('{"en": "Mall", "es": "Mall", "pt-br": "Shopping"}' AS JSON)
WHERE `id` = 234 AND `display_name` = CAST('{"en": "Mall", "es": "Centro Comercial", "pt-br": "Shopping"}' AS JSON);
UPDATE `definition_catalog_page_header` SET `display_name` = CAST('{"en": "Relax", "es": "Relax", "pt-br": "Sussa"}' AS JSON)
WHERE `id` = 236 AND `display_name` = CAST('{"en": "Relax", "es": "Relax", "pt-br": "Relax"}' AS JSON);
UPDATE `definition_catalog_page_header` SET `display_name` = CAST('{"en": "Runway", "es": "Runway", "pt-br": "Runway"}' AS JSON)
WHERE `id` = 239 AND `display_name` = CAST('{"en": "Runway", "es": "Pasarela", "pt-br": "Passarela"}' AS JSON);
UPDATE `definition_catalog_page_header` SET `display_name` = CAST('{"en": "Sci-Fi", "es": "Ciencia ficción", "pt-br": "Sci-Fi"}' AS JSON)
WHERE `id` = 240 AND `display_name` = CAST('{"en": "Sci-Fi", "es": "Ciencia ficción", "pt-br": "Ficção científica"}' AS JSON);
UPDATE `definition_catalog_page_header` SET `display_name` = CAST('{"en": "Teleports", "es": "Teleports", "pt-br": "Teletransportes"}' AS JSON)
WHERE `id` = 244 AND `display_name` = CAST('{"en": "Teleports", "es": "Teletransportes", "pt-br": "Teleportes"}' AS JSON);
UPDATE `definition_catalog_page_header` SET `display_name` = CAST('{"en": "Urban", "es": "Urbano", "pt-br": "Cidade Grande"}' AS JSON)
WHERE `id` = 247 AND `display_name` = CAST('{"en": "Urban", "es": "Urbano", "pt-br": "Urbano"}' AS JSON);
UPDATE `definition_catalog_page_header` SET `display_name` = CAST('{"en": "Valentines", "es": "San Valentín", "pt-br": "Namorados"}' AS JSON)
WHERE `id` = 248 AND `display_name` = CAST('{"en": "Valentines", "es": "San Valentín", "pt-br": "Dia dos Namorados"}' AS JSON);
UPDATE `definition_catalog_page_header` SET `icon` = 501, `display_name` = CAST('{"en": "Traxmachines", "es": "Traxmachines", "pt-br": "Traxmachines"}' AS JSON)
WHERE `id` = 1101 AND `icon` = 4 AND `display_name` = CAST('{"en": "Traxmachines", "es": "Máquinas de Trax", "pt-br": "Máquinas de Trax"}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_frontpage_headline2_en", "shabbolin_300x187_E"], "es": ["catalog_frontpage_headline2_es_001", "shabbolin_300x187_E"], "pt-br": ["catalog_frontpage_headline2_pt_001", "shabbolin_300x187_E"]}' AS JSON), `localization_texts` = CAST('{"en": ["Welcome to Habtomo!", "There''s a new Rare waiting in the catalog for you...", "Take me there!", "What is Habtomo? ", "Habtomo takes you back to Habbo in early 2010. The classic furni, the Public Spaces and the old catalogue are all back.\\nWe''re adding the features you remember, one by one, plus a few new ideas. Welcome home!\\nThere''s a new Rare waiting for you in the Catalogue!", "Redeem a voucher code here:", "Rares", "#fff4d9", "#fff4d9", "Great, now take me to the HC Page>>", "Buy Habbo Club"], "es": ["¡Bienvenido a Habtomo!", "Hay un nuevo Rare esperándote en el catálogo...", "¡Llévame allí!", "¿Qué es Habtomo? ", "Habtomo te lleva de vuelta al Habbo de principios de 2010. Los Furni clásicos, las Salas Públicas y el catálogo de siempre han vuelto.\\nVamos añadiendo las funciones que recuerdas, una a una, más algunas ideas nuevas. ¡Bienvenido a casa!\\n¡Hay un nuevo Rare esperándote en el Catálogo!", "Canjea aquí un código de cupón:", "Rares", null, null, "¡Genial! Ahora llévame a la página del HC>>", "Comprar Habbo Club"], "pt-br": ["Bem-vindo ao Habtomo!", "Tem um Raro novo esperando por você no catálogo...", "Me leva até lá!", "O que é o Habtomo? ", "O Habtomo leva você de volta ao Habbo do começo de 2010. Os mobis clássicos, os Espaços Públicos e o catálogo de antigamente estão de volta.\\nEstamos trazendo as funções de que você se lembra, uma por uma, e algumas ideias novas. Bem-vindo de volta!\\nTem um Raro novo esperando por você no Catálogo!", "Resgate um código de cupom aqui:", "Raros", null, null, "Ótimo, agora me leva à página do HC>>", "Assinar o Habbo Club"]}' AS JSON)
WHERE `ref_page_id` = 1 AND `localization_images` = CAST('{"en": ["catalog_frontpage_headline2_en", "shabbolin_300x187_E", ""]}' AS JSON) AND `localization_texts` = CAST('{"en": ["Welcome to Dragon Hotel!", "There''s a new Rare waiting in the catalog for you...", "Take me there!", "What is Dragon Hotel? ", "I''m glad you asked! I made Dragon Hotel as a way to reminisce my memories as a teenager playing Habbo mostly through 2007-2010.\\nV48 is the last version before major changes arrived (e.g. VIP club, Battle Banzai, Wired, etc.).\\nI will try to add all features that were available back (and maybe add some new ones!)", "Redeem a voucher code here:", "Rares", "#fff4d9", "#fff4d9", "Great, now take me to the HC Page>>", "Buy Habbo Club"], "es": ["¡Bienvenido a Dragon Hotel!", "Hay un nuevo Rare esperándote en el catálogo...", "¡Llévame allí!", "¿Qué es Dragon Hotel? ", "¡Qué bueno que preguntas! Hice Dragon Hotel para revivir mis recuerdos de adolescente jugando a Habbo, sobre todo entre 2007 y 2010.\\nLa V48 es la última versión antes de los grandes cambios (por ejemplo, el VIP Club, Battle Banzai, Wired, etc.).\\n¡Intentaré añadir todas las funciones que había entonces (y quizá algunas nuevas!)", "Canjea aquí un código de cupón:", "Rares", "", "", "¡Genial! Ahora llévame a la página del HC>>", "Comprar Habbo Club"], "pt-br": ["Bem-vindo ao Dragon Hotel!", "Tem um Raro novo esperando por você no catálogo...", "Me leva até lá!", "O que é o Dragon Hotel? ", "Que bom que você perguntou! Eu fiz o Dragon Hotel para relembrar meus tempos de adolescente jogando Habbo, principalmente entre 2007 e 2010.\\nA V48 é a última versão antes das grandes mudanças (por exemplo, o VIP Club, Battle Banzai, Wired etc.).\\nVou tentar trazer todas as funções que existiam naquela época (e talvez algumas novas!)", "Resgate um código de cupom aqui:", "Raros", "", "", "Ótimo, agora me leva à página do HC>>", "Assinar o Habbo Club"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_bank_headline1", "catalog_bank_teaser", "catalog_special_txtbg1"], "es": ["catalog_bank_headline1_es", "catalog_bank_teaser", "catalog_special_txtbg1"], "pt-br": ["catalog_bank_headline1_br", "catalog_bank_teaser", "catalog_special_txtbg1"]}' AS JSON), `localization_texts` = CAST('{"en": ["Exchange your Habbo Credits into tradeable Credits. Then you are able to use Credits in a Trade for Furni!", "Click on an item to see more details", "Refundable goods!"], "es": ["Convierte tus Créditos en una moneda especial tradeable. Así, no sólo podrás intercambiar Furni, sino también comprarlos mientras tradeas. Nota: ten en cuenta que una cosa es el valor y otra el precio al que compras cada pieza.", null, "¡Bienes reembolsables!"], "pt-br": ["Troque suas Moedas do Habbo por Moedas negociáveis. Assim você pode usar Moedas em uma troca por Mobis!", null, "Nada de cheque sem fundo!"]}' AS JSON)
WHERE `ref_page_id` = 4 AND `localization_images` = CAST('{"en": ["catalog_bank_headline1", "catalog_bank_teaser", "catalog_special_txtbg1"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["Exchange your Habbo Credits into tradeable Credits. Then you are able to use Credits in a Trade for Furni!", "Click on an item to see more details", "Refundable goods!"], "es": ["Cambia tus Créditos de Habbo por Créditos intercambiables. ¡Así podrás usar Créditos en un tradeo por Furni!", "Haz clic en un objeto para ver más detalles", "¡Productos canjeables!"], "pt-br": ["Troque suas Moedas do Habbo por Moedas negociáveis. Assim você pode usar Moedas em uma troca por Mobis!", "Clique em um item para ver mais detalhes", "Produtos reembolsáveis!"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_rares_headline1", "catalog_cltbs_teaser_en"], "es": ["catalog_rares_headline1", "catalog_cltbs_teaser_en"], "pt-br": ["catalog_rares_headline1", "catalog_cltbs_teaser_en"]}' AS JSON), `localization_texts` = CAST('{"en": ["Every Rare here is a piece of history - available for a short time, then gone for good. Grab one while you can, before it becomes a story you tell newer Habbos about."], "es": ["Cada Rare de aquí es un pedazo de historia. Está disponible por poco tiempo y luego desaparece para siempre. Consigue uno mientras puedas, antes de que se convierta en una historia que les cuentes a los Habbos más nuevos."], "pt-br": ["Cada Raro daqui é um pedaço da história: fica disponível por pouco tempo e depois some para sempre. Garanta o seu enquanto pode, antes que vire uma história para contar aos Habbos mais novos."]}' AS JSON)
WHERE `ref_page_id` = 12 AND `localization_images` = CAST('{"en": ["catalog_rares_headline1", "catalog_cltbs_teaser_en"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["Every Rare here is a piece of history - available for a short time, then gone for good. Grab one while you can, before it becomes a story you tell newer Habbos about.", "", ""], "es": ["Cada Rare de aquí es un pedazo de historia: está disponible por poco tiempo y luego desaparece para siempre. Consigue uno mientras puedas, antes de que se convierta en una historia que les cuentes a los Habbos más nuevos.", "", ""], "pt-br": ["Cada Raro daqui é um pedaço da história: fica disponível por pouco tempo e depois some para sempre. Garanta o seu enquanto pode, antes que vire uma história para contar aos Habbos mais novos.", "", ""]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["look_feel"], "es": ["look_feel"], "pt-br": ["look_feel"]}' AS JSON), `localization_texts` = CAST('{"en": ["Floors, wallpapers and landscapes - get a groovy combination for your room! Use our sample room below to try before you buy! Select your favourite designs and colours and simply click Buy!"], "es": ["Suelos, paredes y vistas - consigue la combinación perfecta para tu Sala. Utiliza la visualización previa de debajo para testear los diseños y colores y, una vez lo tenga stood listo, haz clic en ''Comprar''."], "pt-br": ["Papel de parede, pisos e vistas - crie varios ambientes usando combinações diferentes. Veja uma prévia de como vai ficar o Quarto antes de comprar."]}' AS JSON)
WHERE `ref_page_id` = 25 AND `localization_images` = CAST('{"en": ["look_feel"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["Floors, wallpapers and landscapes - get a groovy combination for your room! Use our sample room below to try before you buy! Select your favourite designs and colours and simply click Buy!"], "es": ["Suelos, papeles pintados y paisajes: ¡consigue una combinación genial para tu sala! Usa nuestra sala de ejemplo de abajo para probar antes de comprar. ¡Elige tus diseños y colores favoritos y simplemente haz clic en Comprar!"], "pt-br": ["Pisos, papéis de parede e paisagens: monte uma combinação incrível para o seu quarto! Use o nosso quarto de exemplo abaixo para experimentar antes de comprar. Escolha seus desenhos e cores favoritos e é só clicar em Comprar!"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_pixelrent_headline1_en", "catalog_pxl_teaser3_en"], "es": ["catalog_pixelrent_headline1_es", "catalog_pxl_teaser3_en"], "pt-br": ["catalog_pixelrent_headline1_br", "catalog_pxl_teaser3_en"]}' AS JSON), `localization_texts` = CAST('{"en": ["Hire some cool effects to add an explosive touch to your room. From bubbles to firestarters, you can enhance your room dramatically!"], "es": ["Alquila efectos geniales para darle un toque explosivo a tu Sala. ¡Desde burbujas hasta lanzallamas, puedes darle un cambiazo a tu Sala!"], "pt-br": ["Incremente seu Quarto com esses Efeitos. Você aluga um e ele é automaticamente retirado quando o prazo acabar."]}' AS JSON)
WHERE `ref_page_id` = 31 AND `localization_images` = CAST('{"en": ["catalog_pixelrent_headline1_en", "catalog_pxl_teaser3_en"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["Hire some cool effects to add an explosive touch to your room. From bubbles to firestarters, you can enhance your room dramatically!"], "es": ["Alquila efectos geniales para darle un toque explosivo a tu sala. ¡Desde burbujas hasta lanzallamas, puedes darle un cambio enorme a tu sala!"], "pt-br": ["Alugue efeitos incríveis para dar um toque explosivo ao seu quarto. De bolhas a lança-chamas, você pode transformar seu quarto de vez!"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_pixeleffects_headline1_en", "catalog_pxl_teaser1_en"], "es": ["catalog_pixeleffects_headline1_es", "catalog_pxl_teaser1_en"], "pt-br": ["catalog_pixeleffects_headline1_br", "catalog_pxl_teaser1_en"]}' AS JSON), `localization_texts` = CAST('{"en": ["Tune your character with cool effects that fit the occasion. Do you want to fly away with the red carpet or be in the spotlight? Now is your chance!"], "es": ["Dale a tu Habbo efectos geniales para cada ocasión. ¿Quieres salir volando en la alfombra roja o estar bajo los focos? ¡Esta es tu oportunidad!"], "pt-br": ["Dê ao seu Habbo Efeitos super divertidos que se encaixam em qualquer ocasião. Quer voar num tapete vermelho ou ser o centro das atenções? É sua chance!"]}' AS JSON)
WHERE `ref_page_id` = 32 AND `localization_images` = CAST('{"en": ["catalog_pixeleffects_headline1_en", "catalog_pxl_teaser1_en"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["Tune your character with cool effects that fit the occasion. Do you want to fly away with the red carpet or be in the spotlight? Now is your chance!"], "es": ["Ponle a tu personaje efectos geniales para cada ocasión. ¿Quieres salir volando en la alfombra roja o estar bajo los focos? ¡Esta es tu oportunidad!"], "pt-br": ["Turbine seu personagem com efeitos incríveis para cada ocasião. Quer sair voando no tapete vermelho ou ficar sob os holofotes? Esta é a sua chance!"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_hello_header1_en", "catalog_hello_teaser1_en"], "es": ["catalog_hello_header1_es", "catalog_hello_teaser1_en"], "pt-br": ["catalog_hello_header1_en", "catalog_hello_teaser1_en"]}' AS JSON), `localization_texts` = CAST('{"en": ["Hello Furni is available with Pixels and is perfect if you are decorating your room for the very first time. The Furni is yours to keep and therefore cannot be traded.", "Click on an item for details."], "es": ["¿A qué esperas para conseguir los nuevos Furni ''Bienvenida''? ¡Los tienes todos disponibles aquí! Eso sí, ten en cuenta que no son tradeables. ¡Han sido creados para que tú seas quien los disfrute gracias a los Píxeles!", "Haz clic en un objeto para ver más detalles."], "pt-br": ["Mobis com grande estilo e pequeno preço. Custam apenas Pixels, conseguidos de graça convivendo no Habbo.", "Clique em um item para ver mais detalhes."]}' AS JSON)
WHERE `ref_page_id` = 33 AND `localization_images` = CAST('{"en": ["catalog_hello_header1_en", "catalog_hello_teaser1_en"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["Hello Furni is available with Pixels and is perfect if you are decorating your room for the very first time. The Furni is yours to keep and therefore cannot be traded.", "Click on an item for details."], "es": ["Hello Furni se compra con Píxeles y es perfecto si estás decorando tu sala por primera vez. El Furni es tuyo para siempre, así que no se puede intercambiar.", "Haz clic en un objeto para ver más detalles."], "pt-br": ["Hello Furni se compra com Pixels e é perfeito se você está decorando seu quarto pela primeira vez. Os Mobis são seus para sempre, então não podem ser trocados.", "Clique em um item para ver mais detalhes."]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_automobile_header1_en", "catalog_automobile_teaser1_en"], "es": ["catalog_automobile_header1_es", "catalog_automobile_teaser1_en"], "pt-br": ["catalog_automobile_header1_pt", "catalog_automobile_teaser1_en"]}' AS JSON), `localization_texts` = CAST('{"en": ["Every Habbo needs a car effect! Not only do they bring a bit of the outside inside, they also enhance the air quality! And what better gift for a friend than a beautiful traffic sign or elegant pile of tires..."], "es": ["¡Todo Habbo necesita un auto! Y qué mejor regalo para un amigo que una bonita señal de tráfico o una elegante pila de neumáticos..."], "pt-br": ["Está na hora de pegar seu carro e pisar fundo com esses mobis para corrida. Monte agora mesmo um autotromo."]}' AS JSON)
WHERE `ref_page_id` = 34 AND `localization_images` = CAST('{"en": ["catalog_automobile_header1_en", "catalog_automobile_teaser1_en"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["Every Habbo needs a car effect! Not only do they bring a bit of the outside inside, they also enhance the air quality! And what better gift for a friend than a beautiful traffic sign or elegant pile of tires..."], "es": ["¡Todo Habbo necesita un efecto de coche! Y qué mejor regalo para un amigo que una bonita señal de tráfico o una elegante pila de neumáticos..."], "pt-br": ["Todo Habbo precisa de um efeito de carro! E que presente melhor para um amigo do que uma bela placa de trânsito ou uma elegante pilha de pneus..."]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_recycler_headline3", "ctlg_ecotron_image"], "es": ["catalog_recycler_headline3", "ctlg_ecotron_image"], "pt-br": ["catalog_recycler_headline3", "ctlg_ecotron_image"]}' AS JSON), `localization_texts` = CAST('{"en": ["Drag 5 items to the boxes below and click recycle. Recyclable items display a recyclable tag in your inventory. Check out the prizes and the instructions for recycling before you recycle."], "es": ["Arrastra 5 objetos a las cajas de debajo y haz clic en reciclar. Los objetos reciclables tienen una marca en el Inventario. Echa un vistazo a los premios e instrucciones antes de comenzar a reciclar."], "pt-br": ["Arraste 5 itens para as caixas abaixo e clique em reciclar. Os itens recicláveis têm uma etiqueta de reciclável no seu inventário. Confira as recompensas e as instruções antes de reciclar."]}' AS JSON)
WHERE `ref_page_id` = 51 AND `localization_images` = CAST('{"en": ["catalog_recycler_headline3", "ctlg_ecotron_image"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["Drag 5 items to the boxes below and click recycle. Recyclable items display a recyclable tag in your inventory. Check out the prizes and the instructions for recycling before you recycle."], "es": ["Arrastra 5 objetos a las casillas de abajo y haz clic en reciclar. Los objetos reciclables tienen una etiqueta de reciclable en tu inventario. Mira los premios y las instrucciones antes de reciclar."], "pt-br": ["Arraste 5 itens para as caixas abaixo e clique em reciclar. Os itens recicláveis têm uma etiqueta de reciclável no seu inventário. Confira os prêmios e as instruções antes de reciclar."]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_recycler_headline3"], "es": ["catalog_recycler_headline3"], "pt-br": ["catalog_recycler_headline3"]}' AS JSON), `localization_texts` = CAST('{"en": ["What are the prizes? Ecotron box may contain one of these:"], "es": ["¿Qué puedo conseguir si reciclo Furni en el Ecotron? Pues cualquiera de estos Furni:"], "pt-br": ["Quais são os prêmios? As caixas Ecotron podem conter um destes:"]}' AS JSON)
WHERE `ref_page_id` = 52 AND `localization_images` = CAST('{"en": ["catalog_recycler_headline3"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["What are the prizes? Ecotron box may contain one of these:"], "es": ["¿Cuáles son los premios? La caja del Ecotron puede contener uno de estos:"], "pt-br": ["Quais são os prêmios? A caixa do Ecotron pode ter um destes:"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_recycler_headline3", "ctlg_ecotron_box"], "es": ["catalog_recycler_headline3", "ctlg_ecotron_box"], "pt-br": ["catalog_recycler_headline3", "ctlg_ecotron_box"]}' AS JSON), `localization_texts` = CAST('{"en": ["The Ecotron is a furni recycler. Get rid of old furni... what will you get in return? It''s a surprise! Become a Habbo eco-warrior. No refunds!", "How to use the Ecotron?", "1. Just drag 5 items from your inventory to the Ecotron. One item / square. Recyclable items are marked in your inventory with an image. When you have 5 items in the boxes, click the \\"Recycle\\" button. You can now find the Ecotron prize box from your inventory.\\r\\n2. Click the box to  see its tag. Open the box, or trade it unopened. The timer shows you how long you have to wait before you can recycle more items. Check the prizes before you recycle - don''t be surprised by the surprise!"], "es": ["El Ecotron es un reciclador de Furni. ¡Mete tus Furni antiguos en la máquina y obtén algo nuevo a cambio! ¡No se admiten devoluciones!", "¿Cómo se usa el Ecotron?", "1. Arrastra 5 Furni de tu inventario al Ecotron. Un objeto por casilla. Los objetos reciclables están marcados con un ícono en tu inventario. Cuando tengas 5 Furni en las casillas, haz clic en el botón \\"Reciclar\\". Encontrarás la caja de premio del Ecotron en tu inventario.\\n\\n2. Haz clic en la caja para ver su etiqueta. Abre la caja o tradéala sin abrirla\\n\\nPD: Echa un ojo a las Recompensas antes de reciclar: ¡que no te sorprenda la sorpresa!"], "pt-br": ["O Ecotron é um reciclador de Mobis. Livre-se dos seus Mobis velhos... e o que você ganha em troca? É surpresa! Não há reembolso!", "Como usar o Ecotron?", "1. Arraste 5 itens do seu inventário para o Ecotron. Um item por caixa. Os itens recicláveis têm uma imagem no seu inventário. Quando tiver 5 itens nas caixas, clique no botão \\"Reciclar\\". A caixa de prêmio do Ecotron vai para o seu inventário.\\n\\n2. Clique na caixa para ver a etiqueta. Abra a caixa, ou troque fechada mesmo. O contador mostra quanto tempo falta para reciclar de novo.\\n\\nConfira as recompensas antes de reciclar: que a surpresa não te surpreenda!"]}' AS JSON)
WHERE `ref_page_id` = 53 AND `localization_images` = CAST('{"en": ["catalog_recycler_headline3", "ctlg_ecotron_box"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["The Ecotron is a furni recycler. Get rid of old furni... what will you get in return? It''s a surprise! Become a Habbo eco-warrior. No refunds!", "How to use the Ecotron?", "1. Just drag 5 items from your inventory to the Ecotron. One item / square. Recyclable items are marked in your inventory with an image. When you have 5 items in the boxes, click the \\"Recycle\\" button. You can now find the Ecotron prize box from your inventory.\\r\\n2. Click the box to  see its tag. Open the box, or trade it unopened. The timer shows you how long you have to wait before you can recycle more items. Check the prizes before you recycle - don''t be surprised by the surprise!", ""], "es": ["El Ecotron es un reciclador de furni. Deshazte de tus furnis viejos... ¿qué recibirás a cambio? ¡Es una sorpresa! Conviértete en un eco-guerrero Habbo. ¡No se aceptan devoluciones!", "¿Cómo se usa el Ecotron?", "1. Arrastra 5 objetos de tu inventario al Ecotron. Un objeto por casilla. Los objetos reciclables están marcados con una imagen en tu inventario. Cuando tengas 5 objetos en las casillas, haz clic en el botón \\"Reciclar\\". Encontrarás la caja de premio del Ecotron en tu inventario.\\r\\n2. Haz clic en la caja para ver su etiqueta. Ábrela, o intercámbiala sin abrir. El contador te dice cuánto tienes que esperar para volver a reciclar. Mira los premios antes de reciclar: ¡que la sorpresa no te sorprenda!", ""], "pt-br": ["O Ecotron é um reciclador de mobis. Livre-se dos seus mobis velhos... e o que você ganha em troca? É surpresa! Vire um eco-guerreiro Habbo. Não há reembolso!", "Como usar o Ecotron?", "1. Arraste 5 itens do seu inventário para o Ecotron. Um item por caixa. Os itens recicláveis têm uma imagem no seu inventário. Quando tiver 5 itens nas caixas, clique no botão \\"Reciclar\\". A caixa de prêmio do Ecotron vai para o seu inventário.\\r\\n2. Clique na caixa para ver a etiqueta. Abra a caixa, ou troque fechada mesmo. O contador mostra quanto tempo falta para reciclar de novo. Confira os prêmios antes de reciclar: que a surpresa não te surpreenda!", ""]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_pet_headline1_en"], "es": ["catalog_pet_headline1_es"], "pt-br": ["catalog_pet_header1_pt"]}' AS JSON), `localization_texts` = CAST('{"en": ["Wet noses, paws, yaps and woofs! You''re about to enter the world of Habbo Dogs. An adorable and faithful servant awaits you with a wagging tail everytime they see you. Find a new friend from our ever-changing selection.", "Name your Dog:", "Pick a color:", "Pick a breed:"], "es": ["Hocicos peludos, zarpas y ladridos. ¡Sí, hablamos de Perros! ", "Dale un nombre:", "Elige un color:", "Escoge una raza:"], "pt-br": ["Latidos, bolas de pêlos, mordidas de leve e brincadeiras de seguir o próprio rabo. Ter um cachorrinho é tudo de bom!", "Dê um nome:", "Escolha uma cor:", "Escolha uma raça:"]}' AS JSON)
WHERE `ref_page_id` = 61 AND `localization_images` = CAST('{"en": ["catalog_pet_headline1_en"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["Wet noses, paws, yaps and woofs! You''re about to enter the world of Habbo Dogs. An adorable and faithful servant awaits you with a wagging tail everytime they see you. Find a new friend from our ever-changing selection.", "Name your Dog:", "Pick a color:", "Pick a breed:"], "es": ["¡Hocicos húmedos, patas, ladridos y guaus! Estás a punto de entrar en el mundo de los Perros de Habbo. Un compañero adorable y fiel te espera moviendo la cola cada vez que te ve. Encuentra un nuevo amigo en nuestra selección, que cambia constantemente.", "Ponle nombre a tu Perro:", "Elige un color:", "Elige una raza:"], "pt-br": ["Focinhos molhados, patas, latidos e au-aus! Você está entrando no mundo dos Cachorros do Habbo. Um companheiro adorável e fiel espera por você abanando o rabo toda vez que te vê. Encontre um novo amigo na nossa seleção, que muda sempre.", "Dê um nome ao seu Cachorro:", "Escolha uma cor:", "Escolha uma raça:"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_pet_headline1_en"], "es": ["catalog_pet_headline1_en"], "pt-br": ["catalog_pet_header1_pt"]}' AS JSON), `localization_texts` = CAST('{"en": ["Fluff, whiskers, meows and purrs! You''re about to enter the world of Habbo Cats. These cute little critters will keep you company if you look after them well. Find a new friend from our ever-changing selection.", "Name your Cat:", "Pick a color:", "Pick a breed:"], "es": ["Pelusas, bigotes, maullidos y ronroneos. ¡Sí, hablamos de Gatos!", "Dale un nombre:", "Elige un color:", "Escoge una raza:"], "pt-br": ["Quem resiste ao miado fino e aos carinhos sinceros de um bichano? Estes aqui precisam estar nos seus Quartos.", "Dê um nome ao seu Gato:", "Escolha uma cor:", "Escolha uma raça:"]}' AS JSON)
WHERE `ref_page_id` = 62 AND `localization_images` = CAST('{"en": ["catalog_pet_headline1_en"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["Fluff, whiskers, meows and purrs! You''re about to enter the world of Habbo Cats. These cute little critters will keep you company if you look after them well. Find a new friend from our ever-changing selection.", "Name your Cat:", "Pick a color:", "Pick a breed:"], "es": ["¡Pelusa, bigotes, miaus y ronroneos! Estás a punto de entrar en el mundo de los Gatos de Habbo. Estas criaturitas te harán compañía si las cuidas bien. Encuentra un nuevo amigo en nuestra selección, que cambia constantemente.", "Ponle nombre a tu Gato:", "Elige un color:", "Elige una raza:"], "pt-br": ["Pelinhos, bigodes, miaus e ronrons! Você está entrando no mundo dos Gatos do Habbo. Esses bichinhos fofos vão fazer companhia se você cuidar bem deles. Encontre um novo amigo na nossa seleção, que muda sempre.", "Dê um nome ao seu Gato:", "Escolha uma cor:", "Escolha uma raça:"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_pet_headline1_en"], "es": ["catalog_pet_headline1_es"], "pt-br": ["catalog_pet_header1_pt"]}' AS JSON), `localization_texts` = CAST('{"en": ["Scaly skin, growls and snaps! You''re about to enter the world of Habbo Crocs. Security for your room or to scare your friends a trustworthy and surprisingly loving companion can be yours. Find a new friend from our ever-changing selection.", "Name your Croc:", "Pick a color:", "Pick a breed:"], "es": ["Piel áspera, gruñidos y una dentadura de acero...", "Dale un nombre:", "Elige un color:", "Escoge una raza:"], "pt-br": ["Para quem quer ter um Mascote exótico, nada melhor que um Jacaré. Apesar da pele dura, ele tem o coração mole.", "Dê um nome ao seu Jacaré:", "Escolha uma cor:", "Escolha uma raça:"]}' AS JSON)
WHERE `ref_page_id` = 63 AND `localization_images` = CAST('{"en": ["catalog_pet_headline1_en"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["Scaly skin, growls and snaps! You''re about to enter the world of Habbo Crocs. Security for your room or to scare your friends a trustworthy and surprisingly loving companion can be yours. Find a new friend from our ever-changing selection.", "Name your Croc:", "Pick a color:", "Pick a breed:"], "es": ["¡Piel escamosa, gruñidos y mordiscos! Estás a punto de entrar en el mundo de los Cocodrilos de Habbo. Para vigilar tu sala o asustar a tus amigos, puedes tener un compañero fiel y sorprendentemente cariñoso. Encuentra un nuevo amigo en nuestra selección, que cambia constantemente.", "Ponle nombre a tu Cocodrilo:", "Elige un color:", "Elige una raza:"], "pt-br": ["Pele escamosa, rosnados e mordidas! Você está entrando no mundo dos Crocodilos do Habbo. Para proteger seu quarto ou assustar seus amigos, um companheiro fiel e surpreendentemente carinhoso pode ser seu. Encontre um novo amigo na nossa seleção, que muda sempre.", "Dê um nome ao seu Crocodilo:", "Escolha uma cor:", "Escolha uma raça:"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_pet_headline1_en"], "es": ["catalog_pet_headline1_es"], "pt-br": ["catalog_pet_header1_pt"]}' AS JSON), `localization_texts` = CAST('{"en": ["Good things come in small packages and Habbo''s Terriers are no exception.", "Give a name:", "Pick a color:", "Pick a breed:"], "es": ["Originario de las altas montañas de Escocia.", "Dale un nombre:", "Elige un color:", "Escoge una raza:"], "pt-br": ["Esta raça de cachorro é originíria das montanhas escosesas e conhecida como uma das mais antigas raças domesticadas da região, sendo utilizado para caça de presas em todo o norte da Europa.", "Dê um nome:", "Escolha uma cor:", "Escolha uma raça:"]}' AS JSON)
WHERE `ref_page_id` = 64 AND `localization_images` = CAST('{"en": ["catalog_pet_headline1_en"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["Good things come in small packages and Habbo''s Terriers are no exception.", "Give a name:", "Pick a color:", "Pick a breed:"], "es": ["Las mejores cosas vienen en frascos pequeños, y los Terriers de Habbo no son la excepción.", "Ponle un nombre:", "Elige un color:", "Elige una raza:"], "pt-br": ["As melhores coisas vêm em embalagens pequenas, e os Terriers do Habbo não são exceção.", "Dê um nome:", "Escolha uma cor:", "Escolha uma raça:"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_pet_headline1_en"], "es": ["catalog_pet_headline1_en"], "pt-br": ["catalog_pet_header1_pt"]}' AS JSON), `localization_texts` = CAST('{"en": ["These little scamps were destined for the dining table until we struck a deal with a local farmer to rescue their cute pigtails! Adopt your little piglet today, all you need now is a name.", "Give a name:", "Pick a color:", "Pick a breed:"], "es": ["Estos pequeños traviesos parecen tiernos, ¡pero son una especie salvaje con colmillos afilados! Adopta hoy a tu cerdito, solo te falta un nombre.", "Ponle un nombre:", "Elige un color:", "Escoge una raza:"], "pt-br": ["Depois de um grande rebuliço pelo Habbo Hotel, finalmente os Porquinhos foram autorizados e já podem ser adquiridos! Eles são doces, amáveis e bagunceiros!", "Dê um nome:", "Escolha uma cor:", "Escolha uma raça:"]}' AS JSON)
WHERE `ref_page_id` = 65 AND `localization_images` = CAST('{"en": ["catalog_pet_headline1_en"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["These little scamps were destined for the dining table until we struck a deal with a local farmer to rescue their cute pigtails! Adopt your little piglet today, all you need now is a name.", "Give a name:", "Pick a color:", "Pick a breed:"], "es": ["¡Estos pequeños traviesos iban derechos a la mesa hasta que llegamos a un acuerdo con un granjero de la zona para rescatar sus adorables colitas! Adopta hoy a tu cerdito, solo te falta un nombre.", "Ponle un nombre:", "Elige un color:", "Elige una raza:"], "pt-br": ["Esses pequenos levados iam direto para a mesa do jantar até fecharmos um acordo com um fazendeiro da região para salvar esses rabinhos enrolados! Adote seu porquinho hoje, só falta um nome.", "Dê um nome:", "Escolha uma cor:", "Escolha uma raça:"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_pet_headline1_en"], "es": ["catalog_pet_headline1_es"], "pt-br": ["catalog_pet_header1_pt"]}' AS JSON), `localization_texts` = CAST('{"en": ["A large, heavy, mammal that walks on the soles of its feet, with thick fur and a very short tail. Fall in love with our adorable range of Bears that include the Grizzly and Polar varieties!", "Give a name:", "Pick a color:", "Pick a breed:"], "es": ["Un mamífero grande y pesado que camina sobre las plantas de los pies, con un pelaje espeso y una cola muy corta. ¡Enamórate de nuestra adorable variedad de Osos, con el Pardo y el Polar!", "Ponle un nombre:", "Elige un color:", "Escoge una raza:"], "pt-br": ["Um mamífero grande e pesado que anda sobre as solas dos pés, com pelagem grossa e um rabo bem curtinho. Apaixone-se pela nossa adorável variedade de Ursos, com o Pardo e o Polar!", "Dê um nome:", "Escolha uma cor:", "Escolha uma raça:"]}' AS JSON)
WHERE `ref_page_id` = 66 AND `localization_images` = CAST('{"en": ["catalog_pet_headline1_en"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["A large, heavy, mammal that walks on the soles of its feet, with thick fur and a very short tail. Fall in love with our adorable range of Bears that include the Grizzly and Polar varieties!", "Give a name:", "Pick a color:", "Pick a breed:"], "es": ["Un mamífero grande y pesado que camina sobre las plantas de los pies, con un pelaje espeso y una cola muy corta. ¡Enamórate de nuestra adorable variedad de Osos, con el Grizzly y el Polar!", "Ponle un nombre:", "Elige un color:", "Elige una raza:"], "pt-br": ["Um mamífero grande e pesado que anda sobre as solas dos pés, com pelagem grossa e um rabo bem curtinho. Apaixone-se pela nossa adorável variedade de Ursos, com o Pardo e o Polar!", "Dê um nome:", "Escolha uma cor:", "Escolha uma raça:"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_pet_headline2", "ctlg_pet_teaser1", "catalog_special_txtbg2"], "es": ["catalog_pet_headline2_es", "ctlg_pet_teaser1", "catalog_special_txtbg2"], "pt-br": ["catalog_pet_headline2_br", "ctlg_pet_teaser1", "catalog_special_txtbg2"]}' AS JSON), `localization_texts` = CAST('{"en": ["Keep your Pet happy and healthy. Feed it to keep strong but don''t forget about exercise! Please note, no fish are hurt in the feeding of your Pet.", "", "You gotta keep ''em happy"], "es": ["Aquí tienes todo lo que necesitas para que tu mascota sea feliz, desde alimentos a juguetes y delicatessen... Es decir, todo para comer y hacer ejercicio ;)", "", "¡Uno a la vez!"], "pt-br": ["Deixe seu bichinho sempre saudável. Dê comida, bebida e carinho, mas não se esqueça dos exercícios.", "", "Um de cada vez!"]}' AS JSON)
WHERE `ref_page_id` = 67 AND `localization_images` = CAST('{"en": ["catalog_pet_headline2", "ctlg_pet_teaser1", "catalog_special_txtbg2"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["Keep your Pet happy and healthy. Feed it to keep strong but don''t forget about exercise! Please note, no fish are hurt in the feeding of your Pet.", "", "You gotta keep ''em happy"], "es": ["Mantén a tu Mascota feliz y sana. Dale de comer para que esté fuerte, ¡pero no te olvides del ejercicio! Y tranquilo, ningún pez sufre al alimentar a tu Mascota.", "", "Hay que tenerlas felices"], "pt-br": ["Mantenha seu Mascote feliz e saudável. Alimente para ele ficar forte, mas não se esqueça dos exercícios! E fique tranquilo: nenhum peixe sofre na alimentação do seu Mascote.", "", "Tem que deixá-los felizes"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_pet_headline1_en", "ctlg_pet_note"], "es": ["catalog_pet_headline1_es", "ctlg_pet_note"], "pt-br": ["catalog_pet_header1_pt", "ctlg_pet_note"]}' AS JSON), `localization_texts` = CAST('{"en": ["Pets are inhabitants of Habbo Hotel too, so each pet owner needs to know a bit about them. Look after your pet by looking through our key points below.", "Few things you should know:", "- You cannot trade or give away pets. Once you purchase them, they will be yours forever.\\r\\n- You can own as many as you want, but you can have no more than 5 pets per room.\\r\\n- You can take pets with you to other rooms, should the room owner allow you to do so.\\r\\n- Your pet needs to be trained before it''ll listen to your commands. By training your pets, you can gain some awesome achievements as well."], "es": ["Las Mascotas también son habitantes del Hotel, por lo que cada dueño necesita saber un poco de información sobre ellas. ¡Cuídalas como es debido!", "Algunos consejos a teenr en cuenta:", "- Puedes colocar Mascotas en tu Sala o llevarte una de ellas a la Sala de otro usuario que las admita.\\n- No puedes tradear tus Mascotas, pero ellas pueden ir a las Salas de tus amigos para recibir caricias.\\n- Puedes tener hasta 5 Mascotas en tu Sala, ¡pero deberás cuidarlas a todas!\\n- Cuida a tu Mascota alimentándola y tratándola bien. Echa un vistazo a la sección ''Accesorios''.\\n- Consigue Placas y Recompensas entrenando a tus Mascotas."], "pt-br": ["Mascotes são habitantes do Habbo Hotel e, assim como seus donos, precisam de alguns cuidados essenciais.", "Algumas coisas que você precisa saber:", "- Você pode colocar um Mascote no seu Quarto ou passear com ele em outros Quartos.\\n- Você não pode negociar Mascotes, mas eles podem ir ao Quarto dos seus amigos para ganhar carinho.\\n- Você pode ter no máximo 5 Mascotes no seu Quarto, mas cuide bem de todos!\\n- Confira as necessidades dele, como água, comida e brinquedos. Veja a seção Para Mascotes.\\n- Ganhe Emblemas de Conquista treinando seu Mascote."]}' AS JSON)
WHERE `ref_page_id` = 68 AND `localization_images` = CAST('{"en": ["catalog_pet_headline1_en", "ctlg_pet_note"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["Pets are inhabitants of Habbo Hotel too, so each pet owner needs to know a bit about them. Look after your pet by looking through our key points below.", "Few things you should know:", "- You cannot trade or give away pets. Once you purchase them, they will be yours forever.\\r\\n- You can own as many as you want, but you can have no more than 5 pets per room.\\r\\n- You can take pets with you to other rooms, should the room owner allow you to do so.\\r\\n- Your pet needs to be trained before it''ll listen to your commands. By training your pets, you can gain some awesome achievements as well.", ""], "es": ["Las mascotas también viven en Habbo Hotel, así que todo dueño necesita saber un poco sobre ellas. Cuida a tu mascota repasando nuestros puntos clave de abajo.", "Algunas cosas que deberías saber:", "- No puedes intercambiar ni regalar mascotas. Una vez que las compras, son tuyas para siempre.\\r\\n- Puedes tener todas las que quieras, pero no más de 5 mascotas por sala.\\r\\n- Puedes llevar tus mascotas a otras salas, si el dueño de la sala te lo permite.\\r\\n- Tu mascota necesita entrenamiento antes de obedecer tus órdenes. Entrenando a tus mascotas también puedes ganar logros geniales.", ""], "pt-br": ["Os mascotes também moram no Habbo Hotel, então todo dono precisa saber um pouco sobre eles. Cuide do seu mascote seguindo os pontos principais abaixo.", "Algumas coisas que você precisa saber:", "- Você não pode trocar nem dar mascotes. Depois de comprados, eles são seus para sempre.\\r\\n- Você pode ter quantos quiser, mas no máximo 5 mascotes por quarto.\\r\\n- Você pode levar seus mascotes para outros quartos, se o dono do quarto deixar.\\r\\n- Seu mascote precisa de treino antes de obedecer aos seus comandos. Treinando seus mascotes você também ganha conquistas incríveis.", ""]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_header_hc", "catalog_hc_teaser"], "es": ["catalog_header_hc", "catalog_hc_teaser"], "pt-br": ["catalog_header_hc", "catalog_hc_teaser"]}' AS JSON), `localization_texts` = CAST('{"en": ["Habbo Club is our VIP members-only club - absolutely no riff-raff admitted! Members enjoy a wide range of benefits, including exclusive clothes, free gifts and an extended friends list"], "es": ["Únete al Habbo Club y disfruta una amplia variedad de beneficios. Ropa exclusiva, regalos mensuales y más amigos en tu Habbo Consola."], "pt-br": ["Venha para o Habbo Club e participe da diversão com várias vantagens que só membros do Club podem ter."]}' AS JSON)
WHERE `ref_page_id` = 71 AND `localization_images` = CAST('{"en": ["catalog_header_hc", "catalog_hc_teaser", ""]}' AS JSON) AND `localization_texts` = CAST('{"en": ["Habbo Club is our VIP members-only club - absolutely no riff-raff admitted! Members enjoy a wide range of benefits, including exclusive clothes, free gifts and an extended friends list", "", ""], "es": ["Habbo Club es nuestro club VIP solo para miembros: ¡nada de chusma! Los miembros disfrutan de un montón de ventajas, como ropa exclusiva, regalos gratis y una lista de amigos más grande", "", ""], "pt-br": ["O Habbo Club é o nosso clube VIP só para membros: nada de penetras! Os membros têm um monte de vantagens, como roupas exclusivas, presentes grátis e uma lista de amigos maior", "", ""]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_header_hc"], "es": ["catalog_header_hc"], "pt-br": ["catalog_header_hc"]}' AS JSON), `localization_texts` = CAST('{}' AS JSON)
WHERE `ref_page_id` = 72 AND `localization_images` = CAST('{"en": ["catalog_header_hc"]}' AS JSON) AND `localization_texts` = CAST('{"en": []}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["hc2_clubtitle", "catalog_hc_teaser", "catalog_special_txtbg1"], "es": ["hc2_clubtitle", "catalog_hc_teaser", "catalog_special_txtbg1"], "pt-br": ["hc2_clubtitle", "catalog_hc_teaser", "catalog_special_txtbg1"]}' AS JSON), `localization_texts` = CAST('{"en": ["NEW Habbo Club Furni range. Allow these elegant delights to make your room sophisticated and humble. They look great placed with your monthly gifts!", "Click on an item to see more details", "For Habbo Club members only!"], "es": ["Los Furni más buscados y sólo para miembros Habbo Club se encuentran aquí. ¿Los vas a dejar escapar? Únete al Habbo Club.", "Haz clic en un objeto para ver más detalles", "Sólo para miembros del Habbo Club."], "pt-br": ["Aqui estão todos os Mobis do Club disponíveis para a compra. Esses belos mobis são exclusividades para membros do Habbo Club.", "Clique em um item para ver mais detalhes", "Só para membros do Habbo Club!"]}' AS JSON)
WHERE `ref_page_id` = 73 AND `localization_images` = CAST('{"en": ["hc2_clubtitle", "catalog_hc_teaser", "catalog_special_txtbg1"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["NEW Habbo Club Furni range. Allow these elegant delights to make your room sophisticated and humble. They look great placed with your monthly gifts!", "Click on an item to see more details", "For Habbo Club members only!"], "es": ["NUEVA gama de Furni del Habbo Club. Deja que estas elegantes delicias le den a tu sala un aire sofisticado y sencillo. ¡Quedan genial junto a tus regalos mensuales!", "Haz clic en un objeto para ver más detalles", "¡Solo para miembros del Habbo Club!"], "pt-br": ["NOVA linha de Mobis do Habbo Club. Deixe essas peças elegantes darem ao seu quarto um ar sofisticado e simples. Ficam ótimas com seus presentes mensais!", "Clique em um item para ver mais detalhes", "Só para membros do Habbo Club!"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["header_studio", "catalog_limited_teaser1_001", ""], "es": ["header_studio", "catalog_limited_teaser1_001", ""], "pt-br": ["header_studio", "catalog_limited_teaser1_001", ""]}' AS JSON)
WHERE `ref_page_id` = 91 AND `localization_images` = CAST('{"en": ["header_studio", "catalog_limited_teaser1_001", ""]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["header_studio", "catalog_limited_teaser1_001", ""], "es": ["header_studio", "catalog_limited_teaser1_001", ""], "pt-br": ["header_studio", "catalog_limited_teaser1_001", ""]}' AS JSON)
WHERE `ref_page_id` = 92 AND `localization_images` = CAST('{"en": ["header_studio", "catalog_limited_teaser1_001", ""]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_trophies_headline1_ca"], "es": ["catalog_trophies_headline1_ca"], "pt-br": ["catalog_trophies_headline1_ca"]}' AS JSON)
WHERE `ref_page_id` = 93 AND `localization_images` = CAST('{"en": ["catalog_trophies_headline1_ca"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_rares_headline1", "catalog_xma_teaser3_en"], "es": ["catalog_rares_headline1", "catalog_xma_teaser3_en"], "pt-br": ["catalog_rares_headline1", "catalog_xma_teaser3_en"]}' AS JSON)
WHERE `ref_page_id` = 94 AND `localization_images` = CAST('{"en": ["catalog_rares_headline1", "catalog_xma_teaser3_en"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_cltbs_header1_en", "catalog_cltbs_teaser_en"], "es": ["catalog_cltbs_header1_en", "catalog_cltbs_teaser_en"], "pt-br": ["catalog_cltbs_header1_en", "catalog_cltbs_teaser_en"]}' AS JSON)
WHERE `ref_page_id` = 95 AND `localization_images` = CAST('{"en": ["catalog_cltbs_header1_en", "catalog_cltbs_teaser_en"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_marketplace_header_en"], "es": ["catalog_marketplace_header_es"], "pt-br": ["catalog_marketplace_header_br"]}' AS JSON), `localization_texts` = CAST('{}' AS JSON)
WHERE `ref_page_id` = 101 AND `localization_images` = CAST('{"en": ["catalog_marketplace_header_en"]}' AS JSON) AND `localization_texts` = CAST('{"en": [""]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_marketplace_header_en"], "es": ["catalog_marketplace_header_en"], "pt-br": ["catalog_marketplace_header_en"]}' AS JSON)
WHERE `ref_page_id` = 102 AND `localization_images` = CAST('{"en": ["catalog_marketplace_header_en"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_marketplace_header_en", "catalog_note_marketplace"], "es": ["catalog_marketplace_header_es", "catalog_note_marketplace"], "pt-br": ["catalog_marketplace_header_br", "catalog_note_marketplace"]}' AS JSON), `localization_texts` = CAST('{"en": ["The Habbo Marketplace is a convenient and safe way to buy and sell items in Habbo.", "How does the Marketplace work?", "Selling items:\\r\\nYou can sell any item which is tradable by selecting the item in your inventory and clicking on \\"Sell in Marketplace\\". You are able to post 5 items for sale at the cost of 1 credit.\\r\\n\\r\\nBuying items:\\r\\nBrowse for the item you want to buy on the Marketplace Offers page and click Buy. We guarantee you''ll get the item at the cheapest price available at the moment of purchase."], "es": ["El Habbo Mercadillo es una forma segura y conveniente de comprar y vender objetos en Habbo.", "¿Cómo funciona el Mercadillo?", "Comprar objetos:\\nBusca el objeto que quieras comprar en las Ofertas del Mercadillo y haz clic en Comprar. Te garantizamos que tendrás el objeto al precio más bajo disponible en el momento de la compra. Podrás listar 5 Furni a la venta por 1 Crédito.\\n\\nVender objetos:\\nPuedes vender cualquier objeto que sea tradeable seleccionándolo en tu inventario y haciendo clic en \\"Vender en Mercadillo\\". Para vender un Furni en el Mercadillo necesitarás el \\"Pase de Tradeo\\"."], "pt-br": ["A Feira Livre do Habbo é um jeito prático e seguro de comprar e vender Mobis.", "Como funciona a Feira Livre?", "Vendendo itens:\\r\\nVocê pode vender qualquer item negociável selecionando o item no seu inventário e clicando em \\"Vender na Feira Livre\\". Você pode anunciar 5 itens à venda por 1 moeda.\\r\\n\\r\\nComprando itens:\\r\\nProcure o item que você quer comprar na página de Ofertas da Feira Livre e clique em Comprar. Garantimos que você vai levar o item pelo menor preço disponível no momento da compra."]}' AS JSON)
WHERE `ref_page_id` = 103 AND `localization_images` = CAST('{"en": ["catalog_marketplace_header_en", "catalog_note_marketplace"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["The Habbo Marketplace is a convenient and safe way to buy and sell items in Habbo.", "How does the Marketplace work?", "Selling items:\\r\\nYou can sell any item which is tradable by selecting the item in your inventory and clicking on \\"Sell in Marketplace\\". You are able to post 5 items for sale at the cost of 1 credit.\\r\\n\\r\\nBuying items:\\r\\nBrowse for the item you want to buy on the Marketplace Offers page and click Buy. We guarantee you''ll get the item at the cheapest price available at the moment of purchase.", ""], "es": ["El Mercadillo de Habbo es una forma cómoda y segura de comprar y vender objetos en Habbo.", "¿Cómo funciona el Mercadillo?", "Vender objetos:\\r\\nPuedes vender cualquier objeto intercambiable seleccionándolo en tu inventario y haciendo clic en \\"Vender en el Mercadillo\\". Puedes poner 5 objetos a la venta por 1 crédito.\\r\\n\\r\\nComprar objetos:\\r\\nBusca el objeto que quieras comprar en la página de Ofertas del Mercadillo y haz clic en Comprar. Te garantizamos que conseguirás el objeto al precio más barato disponible en el momento de la compra.", ""], "pt-br": ["A Feira Livre do Habbo é um jeito prático e seguro de comprar e vender itens no Habbo.", "Como funciona a Feira Livre?", "Vendendo itens:\\r\\nVocê pode vender qualquer item negociável selecionando o item no seu inventário e clicando em \\"Vender na Feira Livre\\". Você pode anunciar 5 itens à venda por 1 moeda.\\r\\n\\r\\nComprando itens:\\r\\nProcure o item que você quer comprar na página de Ofertas da Feira Livre e clique em Comprar. Garantimos que você vai levar o item pelo menor preço disponível no momento da compra.", ""]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_camera_headline1", "campic_cam", "", "campic_film"], "es": ["catalog_camera_headline1", "campic_cam", "", "campic_film"], "pt-br": ["catalog_camera_headline1_br", "campic_cam", "", "campic_film"]}' AS JSON), `localization_texts` = CAST('{"en": ["Take picture and keep a record of those special moments forever. With your Habbo camera you can take pictures of just about anything in the Hotel - Even a celebrity sitting on the loo...\\n\\nThe Camera comes with 2 free photos.", "When you''ve used your free photos, you''ll need to buy more. Each roll of film takes five photos. Your Camera will show how much film you have left and loads the next roll automatically."], "es": ["Saca fotos y guarda momentos especiales para siempre. Con tu Habo Cámara puedes fotografiar casi cualquier cosa del Hotel. Hasta a un famoso sentado en el váter...\\n\\nLa Cámara viene con 2 fotos gratis.", "Cuando hayas usado tus fotos gratis, tendrás que comprar más. Cada carrete trae cinco fotos. Tu Cámara te muestra cuánto carrete te queda y carga el siguiente automáticamente."], "pt-br": ["Tire fotos e guarde esses momentos especiais para sempre. Com a sua câmera Habbo você pode fotografar quase tudo no Hotel. Até uma celebridade sentada no trono...\\n\\nA Câmera vem com 2 fotos grátis.", "Quando acabarem as fotos grátis, você vai precisar comprar mais. Cada filme tira cinco fotos. A sua Câmera mostra quanto filme ainda resta e carrega o próximo sozinha."]}' AS JSON)
WHERE `ref_page_id` = 131 AND `localization_images` = CAST('{"en": ["catalog_camera_headline1", "campic_cam", "", "campic_film"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["Take picture and keep a record of those special moments forever. With your Habbo camera you can take pictures of just about anything in the Hotel - Even a celebrity sitting on the loo...\\n\\nThe Camera comes with 2 free photos.", "When you''ve used your free photos, you''ll need to buy more. Each roll of film takes five photos. Your Camera will show how much film you have left and loads the next roll automatically."], "es": ["Saca fotos y guarda esos momentos especiales para siempre. Con tu cámara Habbo puedes fotografiar casi cualquier cosa del Hotel. Hasta a un famoso sentado en el váter...\\n\\nLa Cámara viene con 2 fotos gratis.", "Cuando hayas usado tus fotos gratis, tendrás que comprar más. Cada carrete trae cinco fotos. Tu Cámara te muestra cuánto carrete te queda y carga el siguiente automáticamente."], "pt-br": ["Tire fotos e guarde esses momentos especiais para sempre. Com a sua câmera Habbo você pode fotografar quase tudo no Hotel. Até uma celebridade sentada no trono...\\n\\nA Câmera vem com 2 fotos grátis.", "Quando acabarem as fotos grátis, você vai precisar comprar mais. Cada filme tira cinco fotos. A sua Câmera mostra quanto filme ainda resta e carrega o próximo sozinha."]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_camera_headline1", "campic_help"], "es": ["catalog_camera_headline1", "campic_help"], "pt-br": ["catalog_camera_headline1", "campic_help"]}' AS JSON), `localization_texts` = CAST('{"en": ["CAMERA FUNCTIONS\\n\\n1. Press this button to take a photo.\\n2. Photo cancel - for when you''ve chopped off your friend''s head!\\n3. Zoom in and out.\\n4. Photo counter - shows how much film you have left.\\n5. Caption box - write your caption before saving the photo.\\n6. Save - this moves the photo to your inventory. You can give photos to your friends, or put them on the wall like posters."], "es": ["FUNCIONES DE LA CÁMARA\\n\\n1. Pulsa este botón para sacar una foto.\\n2. Cancelar foto: ¡para cuando le cortas la cabeza a tu amigo!\\n3. Acercar y alejar.\\n4. Contador de fotos: muestra cuánto carrete te queda.\\n5. Cuadro de texto: escribe el pie de foto antes de guardarla.\\n6. Guardar: lleva la foto a tu inventario. Puedes regalar fotos a tus amigos o colgarlas en la pared como pósters."], "pt-br": ["FUNÇÕES DA CÂMERA\\n\\n1. Aperte este botão para tirar uma foto.\\n2. Cancelar foto: para quando você cortou a cabeça do seu amigo!\\n3. Aproximar e afastar.\\n4. Contador de fotos: mostra quanto filme ainda resta.\\n5. Legenda: escreva a legenda antes de salvar a foto.\\n6. Salvar: leva a foto para o seu inventário. Você pode dar fotos para seus amigos ou pendurá-las na parede como pôsteres."]}' AS JSON)
WHERE `ref_page_id` = 132 AND `localization_images` = CAST('{"en": ["catalog_camera_headline1", "campic_help"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["CAMERA FUNCTIONS\\n\\n1. Press this button to take a photo.\\n2. Photo cancel - for when you''ve chopped off your friend''s head!\\n3. Zoom in and out.\\n4. Photo counter - shows how much film you have left.\\n5. Caption box - write your caption before saving the photo.\\n6. Save - this moves the photo to your inventory. You can give photos to your friends, or put them on the wall like posters."], "es": ["FUNCIONES DE LA CÁMARA\\n\\n1. Pulsa este botón para sacar una foto.\\n2. Cancelar foto: ¡para cuando le cortas la cabeza a tu amigo!\\n3. Acercar y alejar.\\n4. Contador de fotos: muestra cuánto carrete te queda.\\n5. Cuadro de texto: escribe el pie de foto antes de guardarla.\\n6. Guardar: lleva la foto a tu inventario. Puedes regalar fotos a tus amigos o colgarlas en la pared como pósteres."], "pt-br": ["FUNÇÕES DA CÂMERA\\n\\n1. Aperte este botão para tirar uma foto.\\n2. Cancelar foto: para quando você cortou a cabeça do seu amigo!\\n3. Aproximar e afastar.\\n4. Contador de fotos: mostra quanto filme ainda resta.\\n5. Legenda: escreva a legenda antes de salvar a foto.\\n6. Salvar: leva a foto para o seu inventário. Você pode dar fotos para seus amigos ou pendurá-las na parede como pôsteres."]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_extra_headline1", "catalog_extra_teaser1", "catalog_special_txtbg2"], "es": ["catalog_extra_headline1_de", "catalog_extra_teaser1", "catalog_special_txtbg2"], "pt-br": ["catalog_extra_headline1_br", "catalog_extra_teaser1", "catalog_special_txtbg2"]}' AS JSON), `localization_texts` = CAST('{"en": ["However you place your essential furniture, it''s the finishing touches that express your true personality. And don''t forget, like everything else, these accessories can be moved about to suit your mood.", "Click on an item for more information", "I love my rabbit..."], "es": ["Los accesorios son indispensables para dar personalidad. ¡Diferénciate!", "Haz clic en un objeto para más información", "Adoro a mi conejo..."], "pt-br": ["Não importa como você arruma os móveis essenciais, são os toques finais que mostram sua verdadeira personalidade. E não se esqueça: como tudo mais, esses acessórios podem mudar de lugar conforme o seu humor.", "Clique em um item para mais informações", "Eu amo meu coelho..."]}' AS JSON)
WHERE `ref_page_id` = 201 AND `localization_images` = CAST('{"en": ["catalog_extra_headline1", "catalog_extra_teaser1", "catalog_special_txtbg2"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["However you place your essential furniture, it''s the finishing touches that express your true personality. And don''t forget, like everything else, these accessories can be moved about to suit your mood.", "Click on an item for more information", "I love my rabbit..."], "es": ["Pongas como pongas tus muebles esenciales, son los toques finales los que expresan tu verdadera personalidad. Y no lo olvides: como todo lo demás, estos accesorios se pueden mover según tu estado de ánimo.", "Haz clic en un objeto para más información", "Quiero a mi conejo..."], "pt-br": ["Não importa como você arruma os móveis essenciais, são os toques finais que mostram sua verdadeira personalidade. E não se esqueça: como tudo mais, esses acessórios podem mudar de lugar conforme o seu humor.", "Clique em um item para mais informações", "Eu amo meu coelho..."]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_alh_headline2", "catalog_alh_teaser2", "catalog_special_txtbg2"], "es": ["catalog_alh_headline2", "catalog_alh_teaser2", "catalog_special_txtbg2"], "pt-br": ["catalog_alh_headline2", "catalog_alh_teaser2", "catalog_special_txtbg2"]}' AS JSON), `localization_texts` = CAST('{"en": ["Trade the everyday for a taste of the exotic. Carved screens, glowing lanterns and intricate tilework turn any room into a palace straight out of Arabian nights.", "Click on an item for more information", "Fit for a sultan!"], "es": ["Cambia lo de todos los días por un toque exótico. Biombos tallados, faroles brillantes y azulejos intrincados convierten cualquier sala en un palacio sacado de Las mil y una noches.", "Haz clic en un objeto para más información", "¡Digno de un sultán!"], "pt-br": ["Troque o dia a dia por um toque exótico. Biombos entalhados, lanternas brilhantes e azulejos detalhados transformam qualquer quarto em um palácio saído das Mil e Uma Noites.", "Clique no item para ver mais detalhes.", "Digno de um sultão!"]}' AS JSON)
WHERE `ref_page_id` = 202 AND `localization_images` = CAST('{"en": ["catalog_alh_headline2", "catalog_alh_teaser2", "catalog_special_txtbg2"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["Trade the everyday for a taste of the exotic. Carved screens, glowing lanterns and intricate tilework turn any room into a palace straight out of Arabian nights.", "Click on an item for more information", "Fit for a sultan!"], "es": ["Cambia lo de todos los días por un toque exótico. Biombos tallados, faroles brillantes y azulejos intrincados convierten cualquier sala en un palacio sacado de Las mil y una noches.", "Haz clic en un objeto para más información", "¡Digno de un sultán!"], "pt-br": ["Troque o dia a dia por um toque exótico. Biombos entalhados, lanternas brilhantes e azulejos detalhados transformam qualquer quarto em um palácio saído das Mil e Uma Noites.", "Clique em um item para mais informações", "Digno de um sultão!"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_arc_header1_en", "catalog_arc_teaser1_en"], "es": ["catalog_arc_header1_en", "catalog_arc_teaser1_en"], "pt-br": ["catalog_arc_header1_en", "catalog_arc_teaser1_en"]}' AS JSON), `localization_texts` = CAST('{"en": ["Stay cool (or warm with out campfire!) with and create your own Winter Wonderland or Humble Homeland for your Penguins.", "Click on the item you want for more information"], "es": ["Mantente fresco (¡o calentito con nuestra fogata!) y crea tu propio País de las Maravillas invernal o un hogar humilde para tus pingüinos.", "Haz clic en el objeto que quieras para más información"], "pt-br": ["Fique fresquinho (ou quentinho com a nossa fogueira!) e crie seu próprio País das Maravilhas de inverno ou um lar humilde para os seus pinguins.", "Clique no item que quiser para mais informações"]}' AS JSON)
WHERE `ref_page_id` = 203 AND `localization_images` = CAST('{"en": ["catalog_arc_header1_en", "catalog_arc_teaser1_en"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["Stay cool (or warm with out campfire!) with and create your own Winter Wonderland or Humble Homeland for your Penguins.", "Click on the item you want for more information", ""], "es": ["Mantente fresco (¡o calentito con nuestra fogata!) y crea tu propio País de las Maravillas invernal o un hogar humilde para tus pingüinos.", "Haz clic en el objeto que quieras para más información", ""], "pt-br": ["Fique fresquinho (ou quentinho com a nossa fogueira!) e crie seu próprio País das Maravilhas de inverno ou um lar humilde para os seus pinguins.", "Clique no item que quiser para mais informações", ""]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_area_headline1", "catalog_area_teaser1", "catalog_special_txtbg2"], "es": ["catalog_area_headline1_es", "catalog_area_teaser1", "catalog_special_txtbg2"], "pt-br": ["catalog_area_headline1_br", "catalog_area_teaser1", "catalog_special_txtbg2"]}' AS JSON), `localization_texts` = CAST('{"en": ["Clean, chunky lines set this collection apart as a preserve of the down-to-earth Habbo. It''s beautiful in its simplicity, and welcoming to everyone.", "Click on the item you want for more information.", "Beautiful in its simplicity!"], "es": ["Líneas limpias y robustas hacen de esta colección algo propio del Habbo con los pies en la tierra. Es preciosa en su sencillez y acogedora para todos.", "Haz clic en el objeto que quieras para más información.", "¡Preciosa en su sencillez!"], "pt-br": ["Limpa, arredondada e simples, esta coleção preserva o estilo da terra Habbo. A beleza está na simplicidade, todo mundo sabe.", "Clique no item para ver mais detalhes.", "Beleza nas coisas simples!"]}' AS JSON)
WHERE `ref_page_id` = 204 AND `localization_images` = CAST('{"en": ["catalog_area_headline1", "catalog_area_teaser1", "catalog_special_txtbg2"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["Clean, chunky lines set this collection apart as a preserve of the down-to-earth Habbo. It''s beautiful in its simplicity, and welcoming to everyone.", "Click on the item you want for more information.", "Beautiful in its simplicity!"], "es": ["Líneas limpias y robustas hacen de esta colección algo propio del Habbo con los pies en la tierra. Es preciosa en su sencillez y acogedora para todos.", "Haz clic en el objeto que quieras para más información.", "¡Preciosa en su sencillez!"], "pt-br": ["Linhas limpas e robustas fazem desta coleção a cara do Habbo pé no chão. É linda na sua simplicidade e acolhe todo mundo.", "Clique no item que quiser para mais informações.", "Linda na sua simplicidade!"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_asian_headline1", "catalog_asian_teaser1"], "es": ["catalog_asian_headline1_es", "catalog_asian_teaser1"], "pt-br": ["catalog_asian_headline1_br", "catalog_asian_teaser1"]}' AS JSON), `localization_texts` = CAST('{"en": ["Bring a little balance to your room with this handcrafted collection inspired by ancient Eastern traditions. With a touch of Yin and Yang and a dash of Feng Shui, this furni feels right at home in any Habbo interior.", "Click on the item you want for more information"], "es": ["Dale un poco de equilibrio a tu sala con esta colección artesanal inspirada en antiguas tradiciones orientales. Con un toque de Yin y Yang y una pizca de Feng Shui, este furni se siente como en casa en cualquier interior Habbo.", "Haz clic en el objeto que quieras para más información"], "pt-br": ["Traga um pouco de equilíbrio ao seu quarto com esta coleção artesanal inspirada em antigas tradições orientais. Com um toque de Yin e Yang e uma pitada de Feng Shui, estes mobis ficam em casa em qualquer decoração Habbo.", "Clique no item que quiser para mais informações"]}' AS JSON)
WHERE `ref_page_id` = 205 AND `localization_images` = CAST('{"en": ["catalog_asian_headline1", "catalog_asian_teaser1"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["Bring a little balance to your room with this handcrafted collection inspired by ancient Eastern traditions. With a touch of Yin and Yang and a dash of Feng Shui, this furni feels right at home in any Habbo interior.", "Click on the item you want for more information", ""], "es": ["Dale un poco de equilibrio a tu sala con esta colección artesanal inspirada en antiguas tradiciones orientales. Con un toque de Yin y Yang y una pizca de Feng Shui, este furni se siente como en casa en cualquier interior Habbo.", "Haz clic en el objeto que quieras para más información", ""], "pt-br": ["Traga um pouco de equilíbrio ao seu quarto com esta coleção artesanal inspirada em antigas tradições orientais. Com um toque de Yin e Yang e uma pitada de Feng Shui, estes mobis ficam em casa em qualquer decoração Habbo.", "Clique no item que quiser para mais informações", ""]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_bath_headline1", "catalog_bath_teaser1", "catalog_special_txtbg2"], "es": ["catalog_bath_headline1", "catalog_bath_teaser1", "catalog_special_txtbg2"], "pt-br": ["catalog_bath_headline1_br", "catalog_bath_teaser1", "catalog_special_txtbg2"]}' AS JSON), `localization_texts` = CAST('{"en": ["Have some fun with the cheerful bathroom collection. Give yourself and your guests somewhere to freshen up - vital if you want to avoid nasty niffs. Might be an idea to put your loo in a corner though...", "Click on an item for more information.", "Every Habbo needs one!"], "es": ["Diviértete con la alegre colección de baño. Date a ti y a tus invitados un lugar para refrescarse: es vital si quieres evitar malos olores. Aunque quizá sea buena idea poner el váter en un rincón...", "Haz clic en un objeto para más información.", "¡Todo Habbo necesita uno!"], "pt-br": ["Tenha momentos divertidos com a Coleção Banheiro. Um pouquinho de refrescância não faz mal a ninguém. Quando menos se espera.", "Clique no item para ver mais detalhes.", "Todo Habbo precisa de um!"]}' AS JSON)
WHERE `ref_page_id` = 206 AND `localization_images` = CAST('{"en": ["catalog_bath_headline1", "catalog_bath_teaser1", "catalog_special_txtbg2"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["Have some fun with the cheerful bathroom collection. Give yourself and your guests somewhere to freshen up - vital if you want to avoid nasty niffs. Might be an idea to put your loo in a corner though...", "Click on an item for more information.", "Every Habbo needs one!"], "es": ["Diviértete con la alegre colección de baño. Date a ti y a tus invitados un lugar para refrescarse: es vital si quieres evitar malos olores. Aunque quizá sea buena idea poner el váter en un rincón...", "Haz clic en un objeto para más información.", "¡Todo Habbo necesita uno!"], "pt-br": ["Divirta-se com a alegre coleção de banheiro. Dê a você e aos seus convidados um lugar para se refrescar: é essencial para evitar cheirinhos ruins. Mas talvez seja melhor deixar a privada num canto...", "Clique em um item para mais informações.", "Todo Habbo precisa de um!"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_lc_headline2_en", "catalog_lc_teaser1_en_001"], "es": ["catalog_lc_headline2_en", "catalog_lc_teaser1_en_001"], "pt-br": ["catalog_lc_headline2_en", "catalog_lc_teaser1_en_001"]}' AS JSON)
WHERE `ref_page_id` = 207 AND `localization_images` = CAST('{"en": ["catalog_lc_headline2_en", "catalog_lc_teaser1_en_001"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_header_bling_en", "bling_teaser"], "es": ["catalog_header_bling_en", "bling_teaser"], "pt-br": ["catalog_header_bling_en", "bling_teaser"]}' AS JSON)
WHERE `ref_page_id` = 208 AND `localization_images` = CAST('{"en": ["catalog_header_bling_en", "bling_teaser"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_candy_headline1", "catalog_candy_teaser1", "catalog_special_txtbg2"], "es": ["catalog_candy_headline1", "catalog_candy_teaser1", "catalog_special_txtbg2"], "pt-br": ["catalog_candy_headline1", "catalog_candy_teaser1", "catalog_special_txtbg2"]}' AS JSON), `localization_texts` = CAST('{"en": ["Candy combines the coool, clean lines of the Mode collection with a softer, more soothing style. It''s urban sharpness with a hint of the feminine.", "Click an item for more info.", "Relax! It''s faux-fur."], "es": ["Candy combina las líneas limpias y modernas de la colección Mode con un estilo más suave y relajante. Es la elegancia urbana con un toque femenino.", "Haz clic en un objeto para más información.", "¡Calma, el oso es de mentira!"], "pt-br": ["Candy combina as linhas limpas e modernas da coleção Mode com um estilo mais suave e tranquilo. É a elegância urbana com um toque feminino.", "Clique em um item para mais informações.", "Relaxa! É pele sintética."]}' AS JSON)
WHERE `ref_page_id` = 209 AND `localization_images` = CAST('{"en": ["catalog_candy_headline1", "catalog_candy_teaser1", "catalog_special_txtbg2"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["Candy combines the coool, clean lines of the Mode collection with a softer, more soothing style. It''s urban sharpness with a hint of the feminine.", "Click an item for more info.", "Relax! It''s faux-fur."], "es": ["Candy combina las líneas limpias y modernas de la colección Mode con un estilo más suave y relajante. Es la elegancia urbana con un toque femenino.", "Haz clic en un objeto para más información.", "¡Tranquilo! Es piel sintética."], "pt-br": ["Candy combina as linhas limpas e modernas da coleção Mode com um estilo mais suave e tranquilo. É a elegância urbana com um toque feminino.", "Clique em um item para mais informações.", "Relaxa! É pele sintética."]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["country_header1_en_001", "country_teaser1_en"], "es": ["country_header1_en_001", "country_teaser1_en"], "pt-br": ["country_header1_br_001", "country_teaser1_en"]}' AS JSON)
WHERE `ref_page_id` = 210 AND `localization_images` = CAST('{"en": ["country_header1_en_001", "country_teaser1_en"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_diner_header_en", "catalog_diner_teaser2_en"], "es": ["catalog_diner_header_en", "catalog_diner_teaser2_en"], "pt-br": ["catalog_diner_header_en", "catalog_diner_teaser2_en"]}' AS JSON)
WHERE `ref_page_id` = 211 AND `localization_images` = CAST('{"en": ["catalog_diner_header_en", "catalog_diner_teaser2_en"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_easter_headline1", "catalog_easter_teaser1", "catalog_special_txtbg2"], "es": ["catalog_easter_headline1_es", "catalog_easter_teaser1", "catalog_special_txtbg2"], "pt-br": ["catalog_easter_headline1", "catalog_easter_teaser1", "catalog_special_txtbg2"]}' AS JSON), `localization_texts` = CAST('{"en": ["''Egg''cellent furni - Bouncing bunnies, fluffy chicks, choccy eggs... Yep, it''s Easter! Celebrate with something ''eggstra'' special from our Easter range. But hurry - it''s not here for long this year!", "Click on an item for more details.", "''Egg''-Tastic!"], "es": ["Conejitos saltarines, pollitos, huevos de chocolate... ¡Sí, es Pascua! Celébralo con algo especial de nuestra gama de Pascua.", "Haz clic en un objeto para ver más detalles.", "¡Huevos de chocolate!"], "pt-br": ["Coelhinhos saltitantes, pintinhos fofos, ovos de chocolate... Sim, é Páscoa! Comemore com algo especial da nossa linha de Páscoa.", "Clique em um item para ver mais detalhes.", "Ovo-cionante!"]}' AS JSON)
WHERE `ref_page_id` = 212 AND `localization_images` = CAST('{"en": ["catalog_easter_headline1", "catalog_easter_teaser1", "catalog_special_txtbg2"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["''Egg''cellent furni - Bouncing bunnies, fluffy chicks, choccy eggs... Yep, it''s Easter! Celebrate with something ''eggstra'' special from our Easter range. But hurry - it''s not here for long this year!", "Click on an item for more details.", "''Egg''-Tastic!"], "es": ["Furni de Pascua: conejitos saltarines, pollitos esponjosos, huevos de chocolate... ¡Sí, es Pascua! Celébralo con algo especial de nuestra gama de Pascua. Pero date prisa: ¡este año no estará mucho tiempo!", "Haz clic en un objeto para ver más detalles.", "¡Huevo-cionante!"], "pt-br": ["Mobis de Páscoa: coelhinhos saltitantes, pintinhos fofos, ovos de chocolate... Sim, é Páscoa! Comemore com algo especial da nossa linha de Páscoa. Mas corra: este ano ela não fica por muito tempo!", "Clique em um item para ver mais detalhes.", "Ovo-cionante!"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_exe_headline1_en", "catalog_exe_teaser_en"], "es": ["catalog_exe_headline1_es", "catalog_exe_teaser_en"], "pt-br": ["catalog_exe_headline1_pt", "catalog_exe_teaser_en"]}' AS JSON), `localization_texts` = CAST('{"en": ["For the ultimate business man, the Executive range with its leather seats and Italian coffee is a dream! How about you try the easy lifestyle?", "Click on an item for more information."], "es": ["¡El Furni Executive es ideal para crear un entorno de trabajo sofisticado, ya sea en una oficina, un estudio o una sede de la mafia!", "Haz clic en un objeto para más información."], "pt-br": ["Para o executivo definitivo, a linha Executive com seus assentos de couro e café italiano é um sonho! Que tal experimentar a vida boa?", "Clique em um item para mais detalhes."]}' AS JSON)
WHERE `ref_page_id` = 213 AND `localization_images` = CAST('{"en": ["catalog_exe_headline1_en", "catalog_exe_teaser_en"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["For the ultimate business man, the Executive range with its leather seats and Italian coffee is a dream! How about you try the easy lifestyle?", "Click on an item for more information."], "es": ["Para el hombre de negocios definitivo, ¡la gama Ejecutiva con sus asientos de cuero y su café italiano es un sueño! ¿Por qué no pruebas la vida fácil?", "Haz clic en un objeto para más información."], "pt-br": ["Para o executivo definitivo, a linha Executiva com seus assentos de couro e café italiano é um sonho! Que tal experimentar a vida boa?", "Clique em um item para mais informações."]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_flags_headline1", "catalog_flags_teaser1", "catalog_special_txtbg2"], "es": ["catalog_flags_headline1_es", "catalog_flags_teaser1", "catalog_special_txtbg2"], "pt-br": ["catalog_flags_headline1_br", "catalog_flags_teaser1", "catalog_special_txtbg2"]}' AS JSON), `localization_texts` = CAST('{"en": ["If you''re feeling patriotic, get a flag to prove it. Our finest cloth flags will brighten up the dullest walls.", "Click on the item you want for more information.", "Flag this section for later!"], "es": ["Si te sientes patriota, consigue una bandera para demostrarlo. Nuestras mejores banderas alegrarán hasta las paredes más aburridas.", "Haz clic en el objeto que quieras para más información.", "¿Cuál es tu bandera?"], "pt-br": ["Se você está se sentindo patriota, pegue uma bandeira para provar. Nossas melhores bandeiras de tecido vão alegrar até as paredes mais sem graça.", "Clique no item que quiser para mais informações.", "Qual é a sua bandeira?"]}' AS JSON)
WHERE `ref_page_id` = 214 AND `localization_images` = CAST('{"en": ["catalog_flags_headline1", "catalog_flags_teaser1", "catalog_special_txtbg2"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["If you''re feeling patriotic, get a flag to prove it. Our finest cloth flags will brighten up the dullest walls.", "Click on the item you want for more information.", "Flag this section for later!"], "es": ["Si te sientes patriota, consigue una bandera para demostrarlo. Nuestras mejores banderas de tela alegrarán hasta las paredes más aburridas.", "Haz clic en el objeto que quieras para más información.", "¡Marca esta sección para más tarde!"], "pt-br": ["Se você está se sentindo patriota, pegue uma bandeira para provar. Nossas melhores bandeiras de tecido vão alegrar até as paredes mais sem graça.", "Clique no item que quiser para mais informações.", "Marque esta seção para depois!"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_gallery_headline1", "catalog_posters_teaser1", "catalog_special_txtbg2"], "es": ["catalog_gallery_headline1_br", "catalog_posters_teaser1", "catalog_special_txtbg2"], "pt-br": ["catalog_gallery_headline1_br", "catalog_posters_teaser1", "catalog_special_txtbg2"]}' AS JSON), `localization_texts` = CAST('{"en": ["Adorn your walls with wondrous works of art, posters, plaques and wall hangings. The Gallery is bursting with items to suit all tastes, from kitsch to cool, traditional to modern.", "Click on the item for more information.", "Brighten up your walls!"], "es": ["¿Geografía o Arte? ¿Eres alguien de Bandera o te inclinas más por los posters? ¡Tú decides!", "Haz clic en un objeto para más información.", "¡Alegra tus paredes!"], "pt-br": ["Um toque a mais na sua parede. Tem pra todo gosto, estilo, sabor e ocasião. Pendure suas preferências.", "Clique no item para ver mais detalhes.", "Sua parede cheia de graça!"]}' AS JSON)
WHERE `ref_page_id` = 215 AND `localization_images` = CAST('{"en": ["catalog_gallery_headline1", "catalog_posters_teaser1", "catalog_special_txtbg2"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["Adorn your walls with wondrous works of art, posters, plaques and wall hangings. The Gallery is bursting with items to suit all tastes, from kitsch to cool, traditional to modern.", "Click on the item for more information.", "Brighten up your walls!"], "es": ["Adorna tus paredes con maravillosas obras de arte, pósteres, placas y tapices. La Galería está llena de objetos para todos los gustos, de lo kitsch a lo moderno, de lo tradicional a lo actual.", "Haz clic en el objeto para más información.", "¡Alegra tus paredes!"], "pt-br": ["Enfeite suas paredes com obras de arte maravilhosas, pôsteres, placas e tapeçarias. A Galeria está cheia de itens para todos os gostos, do kitsch ao descolado, do tradicional ao moderno.", "Clique no item para mais informações.", "Alegre suas paredes!"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_gls_headline1_en", "catalog_gls_teaser1_fi"], "es": ["catalog_glass_headline1_es", "catalog_gls_teaser1_fi"], "pt-br": ["catalog_gls_headline1_en", "catalog_gls_teaser1_fi"]}' AS JSON), `localization_texts` = CAST('{"en": ["Clear style, colourful choices! This Furni range is made from plexiglass and comes in a variety of colours. Mix and match your favourites to create a modern lounge.", "Click on an item for more information."], "es": ["¡Estilo transparente con opciones de colores! Esta gama de Furni está hecha de plexiglás y viene en muchos colores. Combina tus favoritos para crear un salón moderno.", "Haz clic en un objeto para más información."], "pt-br": ["Estilo transparente, opções coloridas! Esta linha de Mobis é feita de acrílico e vem em várias cores. Misture suas favoritas para criar uma sala moderna.", "Clique em um item para mais detalhes."]}' AS JSON)
WHERE `ref_page_id` = 216 AND `localization_images` = CAST('{"en": ["catalog_gls_headline1_en", "catalog_gls_teaser1_fi"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["Clear style, colourful choices! This Furni range is made from plexiglass and comes in a variety of colours. Mix and match your favourites to create a modern lounge.", "Click on an item for more information."], "es": ["¡Estilo transparente, opciones de colores! Esta gama de Furni está hecha de plexiglás y viene en muchos colores. Combina tus favoritos para crear un salón moderno.", "Haz clic en un objeto para más información."], "pt-br": ["Estilo transparente, opções coloridas! Esta linha de Mobis é feita de acrílico e vem em várias cores. Misture suas favoritas para criar uma sala moderna.", "Clique em um item para mais informações."]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_gothic_headline1", "catalog_gothic_teaser1"], "es": ["catalog_gothic_headline1_es", "catalog_gothic_teaser1"], "pt-br": ["catalog_gothic_headline1_br", "catalog_gothic_teaser1"]}' AS JSON), `localization_texts` = CAST('{"en": ["What lurks beneath the still, dark surface? Create your own underworld with these Haunted classics.", "Click on an item for more details."], "es": ["¿Qué acecha bajo la superficie lúgubre? Crea tu propio inframundo con estos clásicos embrujados.", "Haz clic en un objeto para ver más detalles."], "pt-br": ["O que se esconde sob a superfície calma e escura? Crie seu próprio submundo com estes clássicos assombrados.", "Clique em um item para ver mais detalhes."]}' AS JSON)
WHERE `ref_page_id` = 217 AND `localization_images` = CAST('{"en": ["catalog_gothic_headline1", "catalog_gothic_teaser1"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["What lurks beneath the still, dark surface? Create your own underworld with these Haunted classics.", "Click on an item for more details."], "es": ["¿Qué acecha bajo la superficie quieta y oscura? Crea tu propio inframundo con estos clásicos embrujados.", "Haz clic en un objeto para ver más detalles."], "pt-br": ["O que se esconde sob a superfície calma e escura? Crie seu próprio submundo com estes clássicos assombrados.", "Clique em um item para ver mais detalhes."]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["greel_header", "greek_catateaser"], "es": ["greel_header", "greek_catateaser"], "pt-br": ["greel_header", "greek_catateaser"]}' AS JSON), `localization_texts` = CAST('{"en": ["Let the games begin! Build an Olympic arena or a temple for the gods with this furni range inspired by ancient Greece."], "es": ["¡Que empiecen los juegos! Construye un estadio olímpico o un templo para los dioses con esta gama de Furni inspirada en la antigua Grecia."], "pt-br": ["Que comecem os jogos! Construa uma arena olímpica ou um templo para os deuses com esta linha de mobis inspirada na Grécia antiga."]}' AS JSON)
WHERE `ref_page_id` = 218 AND `localization_images` = CAST('{"en": ["greel_header", "greek_catateaser"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["Let the games begin! Build an Olympic arena or a temple for the gods with this furni range inspired by ancient Greece."], "es": ["¡Que empiecen los juegos! Construye un estadio olímpico o un templo para los dioses con esta gama de furni inspirada en la antigua Grecia."], "pt-br": ["Que comecem os jogos! Construa uma arena olímpica ou um templo para os deuses com esta linha de mobis inspirada na Grécia antiga."]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_gru_headline1_en_001", "catalog_gru_teaser_en_001"], "es": ["catalog_gru_headline1_en_001", "catalog_gru_teaser_en_001"], "pt-br": ["catalog_gru_headline2_pt", "catalog_gru_teaser_en_001"]}' AS JSON), `localization_texts` = CAST('{"en": ["Alternative Habbos with alternative style or rebellious students with a point to prove. The Grunge range will get your bedroom looking just the way you  like it - organized, neat and tidy!"], "es": ["Nadie dijo que ibas a comenzar amueblando tu Sala con obras de arte. ¡Por algo hay que empezar!"], "pt-br": ["Habbos com estilo alternativo, ou estudantes rebeldes com algo a provar. A linha Mundrungo deixa seu quarto do jeitinho que você gosta: organizado, limpo e arrumado!"]}' AS JSON)
WHERE `ref_page_id` = 219 AND `localization_images` = CAST('{"en": ["catalog_gru_headline1_en_001", "catalog_gru_teaser_en_001"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["Alternative Habbos with alternative style or rebellious students with a point to prove. The Grunge range will get your bedroom looking just the way you  like it - organized, neat and tidy!"], "es": ["Habbos alternativos con un estilo alternativo, o estudiantes rebeldes con algo que demostrar. ¡La gama Grunge dejará tu dormitorio justo como te gusta: organizado, limpio y ordenado!"], "pt-br": ["Habbos alternativos com estilo alternativo, ou estudantes rebeldes com algo a provar. A linha Grunge deixa seu quarto do jeitinho que você gosta: organizado, limpo e arrumado!"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["ctlg_habbowood_headline1_en", "cine_2011_teaser"], "es": ["ctlg_habbowood_headline1_en", "cine_2011_teaser"], "pt-br": ["ctlg_habbowood_headline1_en", "cine_2011_teaser"]}' AS JSON)
WHERE `ref_page_id` = 220 AND `localization_images` = CAST('{"en": ["ctlg_habbowood_headline1_en", "cine_2011_teaser"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_halloween_headline1_ca", "catalog_halloween_teaser"], "es": ["catalog_halloween_headline1_ca", "catalog_halloween_teaser"], "pt-br": ["catalog_halloween_headline1_ca", "catalog_halloween_teaser"]}' AS JSON)
WHERE `ref_page_id` = 221 AND `localization_images` = CAST('{"en": ["catalog_halloween_headline1_ca", "catalog_halloween_teaser"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_iced_headline1_ca", "catalog_iced_teaser1", "catalog_special_txtbg2"], "es": ["catalog_iced_headline1_ca", "catalog_iced_teaser1", "catalog_special_txtbg2"], "pt-br": ["catalog_iced_headline1_ca", "catalog_iced_teaser1", "catalog_special_txtbg2"]}' AS JSON), `localization_texts` = CAST('{"en": ["For the Habbo who needs no introduction. It''s so chic, it says everything and nothing. It''s a black canvas, let your imagination to run wild!", "Click on the item you want for more information", "These chairs are so comfy."], "es": ["Guay y cómodo, así­ es el mobiliario para ese Habbo que busca relajarse y dejar que el tiempo simplemente pase...", "Haz clic en el objeto que quieras para más información", "Estas sillas son comodísimas."], "pt-br": ["Para o Habbo que dispensa apresentações. É tão chique que diz tudo e nada ao mesmo tempo. É uma tela preta: deixe sua imaginação voar!", "Clique no item que quiser para mais informações", "Essas cadeiras são confortáveis demais."]}' AS JSON)
WHERE `ref_page_id` = 222 AND `localization_images` = CAST('{"en": ["catalog_iced_headline1_ca", "catalog_iced_teaser1", "catalog_special_txtbg2"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["For the Habbo who needs no introduction. It''s so chic, it says everything and nothing. It''s a black canvas, let your imagination to run wild!", "Click on the item you want for more information", "These chairs are so comfy."], "es": ["Para el Habbo que no necesita presentación. Es tan chic que lo dice todo y nada a la vez. Es un lienzo negro: ¡deja volar tu imaginación!", "Haz clic en el objeto que quieras para más información", "Estas sillas son comodísimas."], "pt-br": ["Para o Habbo que dispensa apresentações. É tão chique que diz tudo e nada ao mesmo tempo. É uma tela preta: deixe sua imaginação voar!", "Clique no item que quiser para mais informações", "Essas cadeiras são confortáveis demais."]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_jap_header1_no", "catalog_jap_teaser3_en"], "es": ["japan_header_es", "catalog_jap_teaser3_en"], "pt-br": ["catalog_jap_header1_no", "catalog_jap_teaser3_en"]}' AS JSON), `localization_texts` = CAST('{"en": ["From sushi to tatami mats and katanas, bring a touch of Japan to your room! Create a peaceful tea house, deck out your dojo or open your own sushi bar with this stylish furniture.", "Click on an item for more details."], "es": ["Del sushi a los tatamis y las katanas, ¡dale a tu Sala un toque de Japón! Crea una tranquila casa de té, decora tu dojo o abre tu propio bar de sushi con este Furni con tanto estilo.", "Haz clic en un objeto para ver más detalles."], "pt-br": ["Do sushi aos tatames e às katanas, traga um toque do Japão para o seu quarto! Crie uma casa de chá tranquila, decore seu dojô ou abra seu próprio sushi bar com estes mobis cheios de estilo.", "Clique em um item para ver mais detalhes."]}' AS JSON)
WHERE `ref_page_id` = 223 AND `localization_images` = CAST('{"en": ["catalog_jap_header1_no", "catalog_jap_teaser3_en"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["From sushi to tatami mats and katanas, bring a touch of Japan to your room! Create a peaceful tea house, deck out your dojo or open your own sushi bar with this stylish furniture.", "Click on an item for more details."], "es": ["Del sushi a los tatamis y las katanas, ¡dale a tu sala un toque de Japón! Crea una tranquila casa de té, decora tu dojo o abre tu propio bar de sushi con este furni con tanto estilo.", "Haz clic en un objeto para ver más detalles."], "pt-br": ["Do sushi aos tatames e às katanas, traga um toque do Japão para o seu quarto! Crie uma casa de chá tranquila, decore seu dojô ou abra seu próprio sushi bar com estes mobis cheios de estilo.", "Clique em um item para ver mais detalhes."]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["kitchen_header_en", "kitchen_teaser_en", "catalog_special_txtbg2"], "es": ["kitchen_header_es", "kitchen_teaser_en", "catalog_special_txtbg2"], "pt-br": ["kitchen_header_pt", "kitchen_teaser_en", "catalog_special_txtbg2"]}' AS JSON), `localization_texts` = CAST('{"en": ["The Hotel Kitchen has been identified as the cause of food poisoning epidemic. Piers is being trained up in food hygiene and we''re giving you the change to join in with our fantastic, eagerly anticipated range of Kitchen Furni.", "Click an item for more info.", "Choccy mouse you say?"], "es": ["¿No sería lo más ser dueño de un Habbo restaurante? ¿O eres un amante de los catering para Habbos con estilo? ¡Con esta línea de furnis serás un expert@ de la cocina!", "Haz clic en un objeto para más información.", "¿Ratón de chocolate, dices?"], "pt-br": ["Para mestre-cucas e aprendizes, o melhor da cozinha em Mobis caprichados e saborosos.", "Clique em um item para mais informações.", "Rato de chocolate, você disse?"]}' AS JSON)
WHERE `ref_page_id` = 224 AND `localization_images` = CAST('{"en": ["kitchen_header_en", "kitchen_teaser_en", "catalog_special_txtbg2"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["The Hotel Kitchen has been identified as the cause of food poisoning epidemic. Piers is being trained up in food hygiene and we''re giving you the change to join in with our fantastic, eagerly anticipated range of Kitchen Furni.", "Click an item for more info.", "Choccy mouse you say?"], "es": ["La Cocina del Hotel ha sido señalada como la causa de una epidemia de intoxicaciones. Piers está aprendiendo higiene alimentaria y te damos la oportunidad de unirte con nuestra fantástica y esperada gama de Furni de Cocina.", "Haz clic en un objeto para más información.", "¿Ratón de chocolate, dices?"], "pt-br": ["A Cozinha do Hotel foi apontada como a causa de uma epidemia de intoxicação alimentar. O Piers está aprendendo higiene na cozinha e estamos dando a você a chance de entrar nessa com a nossa fantástica e aguardada linha de Mobis de Cozinha.", "Clique em um item para mais informações.", "Rato de chocolate, você disse?"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_lodge_headline1_uk", "catalog_lodge_teaser1", "catalog_special_txtbg2"], "es": ["catalog_lodge_headline1_br", "catalog_lodge_teaser1", "catalog_special_txtbg2"], "pt-br": ["catalog_lodge_headline1_br", "catalog_lodge_teaser1", "catalog_special_txtbg2"]}' AS JSON), `localization_texts` = CAST('{"en": ["For that ski-lodge effect - especially good by an open fire. The Lodge Collection is for Habbos who have a no-frills attitude to home furnishing and those who appreciate the beauty of wood.", "Click on an item for more information.", "How was your boarding today?"], "es": ["¡De pura cepa! Aquí encontrarás Furni de madera que darán a tu Sala un aire muy especial y verdaderamente cálido.", "Haz clic en un objeto para más información.", "¡Madera natural!"], "pt-br": ["Um estilo de vida bucólico e original. Mobis amadeirados e ecológicos, tudo pelo bem da natureza.", "Clique em um item para mais detalhes.", "Viva o sanduíche natural!"]}' AS JSON)
WHERE `ref_page_id` = 225 AND `localization_images` = CAST('{"en": ["catalog_lodge_headline1_uk", "catalog_lodge_teaser1", "catalog_special_txtbg2"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["For that ski-lodge effect - especially good by an open fire. The Lodge Collection is for Habbos who have a no-frills attitude to home furnishing and those who appreciate the beauty of wood.", "Click on an item for more information.", "How was your boarding today?"], "es": ["Para ese efecto de cabaña de esquí, especialmente bonito junto a una chimenea. La colección Lodge es para Habbos que decoran sin florituras y para quienes aprecian la belleza de la madera.", "Haz clic en un objeto para más información.", "¿Qué tal el snowboard hoy?"], "pt-br": ["Para aquele efeito de chalé de esqui, que fica ainda melhor perto de uma lareira. A coleção Lodge é para Habbos que decoram sem frescura e para quem aprecia a beleza da madeira.", "Clique em um item para mais informações.", "Como foi o snowboard hoje?"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["LT_header_en_001", "LT_teaser_en", "catalog_special_txtbg2"], "es": ["LT_header_es", "LT_teaser_en", "catalog_special_txtbg2"], "pt-br": ["LT_header_pt", "LT_teaser_en", "catalog_special_txtbg2"]}' AS JSON), `localization_texts` = CAST('{"en": ["The Lost Tribe has been found, but before you explore their world, it''s time to decide which tribe you wish to join. Who will you pick? The Squidies, Birdies or Simians?", "Lost Tribe Furni out now!", "Tiki has returned, but with Lava?"], "es": ["La Tribu Perdida ha sido encontrada, pero antes de explorar su mundo, es hora de decidir a qué tribu quieres unirte. ¿Cuál eliges? ¿Águilas, Calamares o Monos Enfadados?", "¡Ya está aquí el Furni de la Tribu Perdida!", "Ha vuelto Tiki, ¿pero con lava?"], "pt-br": ["A Tribo Perdida foi encontrada, mas antes de explorar o mundo deles, é hora de decidir de qual tribo você quer participar. Qual você escolhe? As Águias, os Polvos ou os Gorilas?", "Os Mobis da Tribo Perdida já chegaram!", "O Tiki voltou, mas com lava?"]}' AS JSON)
WHERE `ref_page_id` = 226 AND `localization_images` = CAST('{"en": ["LT_header_en_001", "LT_teaser_en", "catalog_special_txtbg2"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["The Lost Tribe has been found, but before you explore their world, it''s time to decide which tribe you wish to join. Who will you pick? The Squidies, Birdies or Simians?", "Lost Tribe Furni out now!", "Tiki has returned, but with Lava?"], "es": ["La Tribu Perdida ha sido encontrada, pero antes de explorar su mundo, es hora de decidir a qué tribu quieres unirte. ¿Cuál eliges? ¿Los Calamares, los Pájaros o los Simios?", "¡Ya está aquí el Furni de la Tribu Perdida!", "Ha vuelto Tiki, ¿pero con lava?"], "pt-br": ["A Tribo Perdida foi encontrada, mas antes de explorar o mundo deles, é hora de decidir de qual tribo você quer participar. Qual você escolhe? As Lulas, os Pássaros ou os Símios?", "Os Mobis da Tribo Perdida já chegaram!", "O Tiki voltou, mas com lava?"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_header_mad_scientist", "catalog_teaser_mad_scientist"], "es": ["catalog_header_mad_scientist", "catalog_teaser_mad_scientist"], "pt-br": ["catalog_header_mad_scientist", "catalog_teaser_mad_scientist"]}' AS JSON)
WHERE `ref_page_id` = 227 AND `localization_images` = CAST('{"en": ["catalog_header_mad_scientist", "catalog_teaser_mad_scientist"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_mode_headline1_uk", "catalog_mode_teaser1", "catalog_special_txtbg2"], "es": ["catalog_mode_headline1_es", "catalog_mode_teaser1", "catalog_special_txtbg2"], "pt-br": ["catalog_mode_headline1_uk", "catalog_mode_teaser1", "catalog_special_txtbg2"]}' AS JSON), `localization_texts` = CAST('{"en": ["Steely, grey industrial standard metal with a sleek design. The perfect range for a streetwise city dweller.", "Click on an item for more information.", "So shiny and new."], "es": ["¿Funcionalidad y diseño en uno? Estos Furni están reservados a Habbos que se sienten intocables y seguros de sí mismos. ¿Tienes estilo o no?", "Haz clic en un objeto para más información.", "¡Qué brillo!"], "pt-br": ["A praticidade do cinza e a funcionalidade do aço. Mobis limpos, úteis e modernos. Para Splashs descolados. Com novas cores!", "Clique no item para ver mais detalhes.", "É esse o que eu quero!"]}' AS JSON)
WHERE `ref_page_id` = 228 AND `localization_images` = CAST('{"en": ["catalog_mode_headline1_uk", "catalog_mode_teaser1", "catalog_special_txtbg2"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["Steely, grey industrial standard metal with a sleek design. The perfect range for a streetwise city dweller.", "Click on an item for more information.", "So shiny and new."], "es": ["Metal gris acerado de estilo industrial con un diseño elegante. La gama perfecta para quien vive en la ciudad y se la sabe todas.", "Haz clic en un objeto para más información.", "Tan brillante y nuevo."], "pt-br": ["Metal cinza aço de padrão industrial com um design elegante. A linha perfeita para quem mora na cidade e sabe das coisas.", "Clique em um item para mais informações.", "Tão brilhante e novinho."]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_dimmers_header_en", "dimmers_teaser"], "es": ["catalog_dimmers_header1_es", "dimmers_teaser"], "pt-br": ["catalog_dimmers_header1_br", "dimmers_teaser"]}' AS JSON), `localization_texts` = CAST('{"en": ["Our range of Mood Lights allow you to spectacularly transform your room in just a click. What will your room look like today? Click the switch and find out now.", "Click on an item for more information."], "es": ["¡Dale un vuelco a la iluminación de tu Sala simplemente pulsando un interruptor o el Panel de Fusibles!", "Haz clic en un objeto para más información."], "pt-br": ["Mude as luzes do seu Quarto de acordo com seu dia, humor, temperatura, gosto. Uma brincadeira para quem não tem medo do escuro.", "Clique no item para ver mais detalhes."]}' AS JSON)
WHERE `ref_page_id` = 229 AND `localization_images` = CAST('{"en": ["catalog_dimmers_header_en", "dimmers_teaser"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["Our range of Mood Lights allow you to spectacularly transform your room in just a click. What will your room look like today? Click the switch and find out now.", "Click on an item for more information."], "es": ["Nuestra gama de Reguladores de Luces te permite transformar tu sala de forma espectacular con solo un clic. ¿Cómo se verá tu sala hoy? Pulsa el interruptor y descúbrelo ahora.", "Haz clic en un objeto para más información."], "pt-br": ["Nossa linha de Reguladores de Luzes transforma seu quarto de forma espetacular com um só clique. Como vai ficar o seu quarto hoje? Aperte o interruptor e descubra agora.", "Clique em um item para mais informações."]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_neon_header1_en", "catalog_neon_teaser1_en"], "es": ["catalog_neon_header1_en", "catalog_neon_teaser1_en"], "pt-br": ["catalog_neon_header1_en", "catalog_neon_teaser1_en"]}' AS JSON), `localization_texts` = CAST('{"en": ["Party animals rejoice! Feel the base and reach for the lasers with the Neon Furni range - everything you need to get the party started!"], "es": ["Ya no tienes excusa para no montar el mejor de los Clubs o Salas de fiesta de Habbo. ¡Inicia ya tu obra maestra!"], "pt-br": ["A coleção mais divertida de Mobis chegou ao Habbo Hotel. Todo o colorido da luz Neon mais a beleza de pixels. Está pronto para a festa?"]}' AS JSON)
WHERE `ref_page_id` = 230 AND `localization_images` = CAST('{"en": ["catalog_neon_header1_en", "catalog_neon_teaser1_en"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["Party animals rejoice! Feel the base and reach for the lasers with the Neon Furni range - everything you need to get the party started!"], "es": ["¡Alegría para los fiesteros! Siente el bajo y alcanza los láseres con la gama de Furni Neón: ¡todo lo que necesitas para empezar la fiesta!"], "pt-br": ["Alegria para os baladeiros! Sinta o grave e alcance os lasers com a linha de Mobis Neon: tudo o que você precisa para começar a festa!"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["organo_cata_header", "organo_teaser", "catalog_special_txtbg2"], "es": ["organo_cata_header", "organo_teaser", "catalog_special_txtbg2"], "pt-br": ["organo_cata_header", "organo_teaser", "catalog_special_txtbg2"]}' AS JSON)
WHERE `ref_page_id` = 231 AND `localization_images` = CAST('{"en": ["organo_cata_header", "organo_teaser", "catalog_special_txtbg2"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_plants_headline1_uk", "catalog_plants_teaser1", "catalog_special_txtbg1"], "es": ["catalog_plants_headline1_es", "catalog_plants_teaser1", "catalog_special_txtbg1"], "pt-br": ["catalog_plants_headline1_br", "catalog_plants_teaser1", "catalog_special_txtbg1"]}' AS JSON), `localization_texts` = CAST('{"en": ["Every room needs a plant! Not only do they bring a bit of the outside inside, they also enhance the air quality!", "Click on a plant to see more details.", "Ge-om-e-tree!"], "es": ["Una Sala sin plantas es como un jardín sin flores. ¡Oxigena tu habitación y dale el frescor verde que necesita!", "Haz clic en una planta para ver más detalles.", "¡Soy un árbol!"], "pt-br": ["Todo Quarto precisa de uma Planta para melhorar a qualidade do ar! Além de ser um presente bastante original.", "Clique em uma planta para ver mais detalhes.", "*Sendo árvore...*"]}' AS JSON)
WHERE `ref_page_id` = 232 AND `localization_images` = CAST('{"en": ["catalog_plants_headline1_uk", "catalog_plants_teaser1", "catalog_special_txtbg1"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["Every room needs a plant! Not only do they bring a bit of the outside inside, they also enhance the air quality!", "Click on a plant to see more details.", "Ge-om-e-tree!"], "es": ["¡Toda sala necesita una planta! No solo traen un poco del exterior al interior, ¡también mejoran la calidad del aire!", "Haz clic en una planta para ver más detalles.", "¡Geo-me-tría!"], "pt-br": ["Todo quarto precisa de uma planta! Elas não só trazem um pouco da natureza para dentro, como também melhoram a qualidade do ar!", "Clique em uma planta para ver mais detalhes.", "Geo-me-tria!"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_plasto_headline1_uk"], "es": ["catalog_plasto_headline1_es"], "pt-br": ["catalog_plasto_headline1_br"]}' AS JSON), `localization_texts` = CAST('{"en": ["Can you feel that 1970s vibe? Decorate with Plasto and add some colour to your life. Choose a colour that reflects your mood, or just pick your favourite shade."], "es": ["¿Sientes esa vibra de los años 70? Decora con Plastic y añade color a tu vida. Elige un color que refleje tu estado de ánimo, o simplemente tu tono favorito."], "pt-br": ["Está sentindo essa vibe dos anos 70? Decore com Plastic e coloque cor na sua vida. Escolha uma cor que combine com o seu humor, ou simplesmente o seu tom favorito."]}' AS JSON)
WHERE `ref_page_id` = 233 AND `localization_images` = CAST('{"en": ["catalog_plasto_headline1_uk"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["Can you feel that 1970s vibe? Decorate with Plasto and add some colour to your life. Choose a colour that reflects your mood, or just pick your favourite shade."], "es": ["¿Sientes esa vibra de los años 70? Decora con Plasto y añade color a tu vida. Elige un color que refleje tu estado de ánimo, o simplemente tu tono favorito."], "pt-br": ["Está sentindo essa vibe dos anos 70? Decore com Plasto e coloque cor na sua vida. Escolha uma cor que combine com o seu humor, ou simplesmente o seu tom favorito."]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["FBP_header", "mall_teaser"], "es": ["FBP_header", "mall_teaser"], "pt-br": ["FBP_header", "mall_teaser"]}' AS JSON)
WHERE `ref_page_id` = 234 AND `localization_images` = CAST('{"en": ["FBP_header", "mall_teaser"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_pura_headline1", "catalog_pura_teaser1", "catalog_special_txtbg2"], "es": ["catalog_pura_headline1", "catalog_pura_teaser1", "catalog_special_txtbg2"], "pt-br": ["catalog_pura_headline1_br", "catalog_pura_teaser1", "catalog_special_txtbg2"]}' AS JSON), `localization_texts` = CAST('{"en": ["The cleanest, freshest range. You can almost hear it breath cool and tranquility within your room. Use it to create a haven away from the hectic lifestyle of Habbo Hotel.", "Click on an item for more information.", "So many colors!"], "es": ["Aquí tienes unos muebles de líneas y colores puros. ¡Relájate!", "Haz clic en un objeto para más información.", "¡Cuántos colores!"], "pt-br": ["Pureza, simplicidade e tranquilidade. Crie um ambiente cheio de estilo e satisfação.", "Clique em um item para mais detalhes.", "Junte Módulos!"]}' AS JSON)
WHERE `ref_page_id` = 235 AND `localization_images` = CAST('{"en": ["catalog_pura_headline1", "catalog_pura_teaser1", "catalog_special_txtbg2"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["The cleanest, freshest range. You can almost hear it breath cool and tranquility within your room. Use it to create a haven away from the hectic lifestyle of Habbo Hotel.", "Click on an item for more information.", "So many colors!"], "es": ["La gama más limpia y fresca. Casi puedes oírla respirar frescura y tranquilidad en tu sala. Úsala para crear un refugio lejos del ajetreo de Habbo Hotel.", "Haz clic en un objeto para más información.", "¡Cuántos colores!"], "pt-br": ["A linha mais limpa e fresca. Dá quase para ouvir o frescor e a tranquilidade dela no seu quarto. Use para criar um refúgio longe da agitação do Habbo Hotel.", "Clique em um item para mais informações.", "Quantas cores!"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["rela_header_en", "rela_teaser_en"], "es": ["rela_header_es", "rela_teaser_en"], "pt-br": ["rela_header2_pt", "rela_teaser_en"]}' AS JSON), `localization_texts` = CAST('{"en": ["Set the mood with our Relax furni. Fill your room with scented candles, settle into a hanging chair and watch the flames flicker. Put your feet up and let your worries drift away!"], "es": ["Crea ambiente con los Furni Relax. Llena tu sala de velas aromáticas, acomódate en una silla colgante y mira cómo parpadean las llamas. ¡Pon los pies en alto y deja que tus preocupaciones se vayan flotando!"], "pt-br": ["Crie o clima com os nossos mobis Sussa. Encha seu quarto de velas aromáticas, acomode-se numa cadeira suspensa e veja as chamas tremularem. Coloque os pés para cima e deixe as preocupações irem embora!"]}' AS JSON)
WHERE `ref_page_id` = 236 AND `localization_images` = CAST('{"en": ["rela_header_en", "rela_teaser_en"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["Set the mood with our Relax furni. Fill your room with scented candles, settle into a hanging chair and watch the flames flicker. Put your feet up and let your worries drift away!"], "es": ["Crea ambiente con nuestro furni Relax. Llena tu sala de velas aromáticas, acomódate en una silla colgante y mira cómo parpadean las llamas. ¡Pon los pies en alto y deja que tus preocupaciones se vayan flotando!"], "pt-br": ["Crie o clima com os nossos mobis Relax. Encha seu quarto de velas aromáticas, acomode-se numa cadeira suspensa e veja as chamas tremularem. Coloque os pés para cima e deixe as preocupações irem embora!"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_rom_headline1_en", "catalog_rom_teaser_en"], "es": ["catalog_rom_headline1_en", "catalog_rom_teaser_en"], "pt-br": ["catalog_rom_headline1_en", "catalog_rom_teaser_en"]}' AS JSON), `localization_texts` = CAST('{"en": ["The Romantique range features Grand Pianos, old antique lamps and tables. It is the ideal range for setting a warm and loving mood in your room. Spruce up your room and invite that special someone over.", "Click on the item you want for more information."], "es": ["La gama Romantique tiene pianos de cola, lámparas y mesas antiguas. Es la gama ideal para crear un ambiente cálido y cariñoso en tu sala. Decora tu Sala e invita a esa persona especial.", "Haz clic en el objeto que quieras para más información."], "pt-br": ["A linha Romantique tem pianos de cauda, abajures e mesas antigas. É a linha ideal para criar um clima acolhedor e carinhoso no seu quarto. Dê um trato no Quarto e convide aquela pessoa especial.", "Clique no item que quiser para mais detalhes."]}' AS JSON)
WHERE `ref_page_id` = 237 AND `localization_images` = CAST('{"en": ["catalog_rom_headline1_en", "catalog_rom_teaser_en"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["The Romantique range features Grand Pianos, old antique lamps and tables. It is the ideal range for setting a warm and loving mood in your room. Spruce up your room and invite that special someone over.", "Click on the item you want for more information."], "es": ["La gama Romantique tiene pianos de cola, lámparas y mesas antiguas. Es la gama ideal para crear un ambiente cálido y cariñoso en tu sala. Arregla tu sala e invita a esa persona especial.", "Haz clic en el objeto que quieras para más información."], "pt-br": ["A linha Romantique tem pianos de cauda, abajures e mesas antigas. É a linha ideal para criar um clima acolhedor e carinhoso no seu quarto. Dê um trato no quarto e convide aquela pessoa especial.", "Clique no item que quiser para mais informações."]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_rugs_headline1_uk", "catalog_rugs_teaser1", "catalog_special_txtbg2"], "es": ["catalog_rugs_headline1_es", "catalog_rugs_teaser1", "catalog_special_txtbg2"], "pt-br": ["catalog_rugs_headline1_br", "catalog_rugs_teaser1", "catalog_special_txtbg2"]}' AS JSON), `localization_texts` = CAST('{"en": ["We have rugs for all ocassions. All rugs are non-slip and washable.", "Click on the item you want for more information", "Rugs, rugs and more rugs!"], "es": ["¡Será por colores y diseños! Elige lo que te gusta y cambia cuando quieras.", "Haz clic en el objeto que quieras para más detalles.", "¡Quedará perfecta en mi Sala!"], "pt-br": ["Para todas as ocasiões. Não faça feio na frente de visitar!", "Clique no item que quiser para mais informações", "Este combina com meu quarto!"]}' AS JSON)
WHERE `ref_page_id` = 238 AND `localization_images` = CAST('{"en": ["catalog_rugs_headline1_uk", "catalog_rugs_teaser1", "catalog_special_txtbg2"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["We have rugs for all ocassions. All rugs are non-slip and washable.", "Click on the item you want for more information", "Rugs, rugs and more rugs!"], "es": ["Tenemos alfombras para cada ocasión. Todas son antideslizantes y lavables.", "Haz clic en el objeto que quieras para más información", "¡Alfombras, alfombras y más alfombras!"], "pt-br": ["Temos tapetes para todas as ocasiões. Todos são antiderrapantes e laváveis.", "Clique no item que quiser para mais informações", "Tapetes, tapetes e mais tapetes!"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["runway_header_en", "runway_teaser_en"], "es": ["runway_header_en", "runway_teaser_en"], "pt-br": ["runway_header_en", "runway_teaser_en"]}' AS JSON), `localization_texts` = CAST('{"en": ["This Furni never goes out of style! Become a style icon, design your own collection or take the catwalk by storm. Have you got what it takes to be a Habbo Runway star?"], "es": ["¡Este Furni nunca pasa de moda! Conviértete en un icono de estilo, diseña tu propia colección o arrasa en la pasarela. ¿Tienes lo que hace falta para ser una estrella de la Habbo Runway?"], "pt-br": ["Estes Mobis nunca saem de moda! Vire um ícone de estilo, crie sua própria coleção ou arrase na passarela. Você tem o que é preciso para ser uma estrela da Habbo Runway?"]}' AS JSON)
WHERE `ref_page_id` = 239 AND `localization_images` = CAST('{"en": ["runway_header_en", "runway_teaser_en"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["This Furni never goes out of style! Become a style icon, design your own collection or take the catwalk by storm. Have you got what it takes to be a Habbo Runway star?"], "es": ["¡Este Furni nunca pasa de moda! Conviértete en un icono de estilo, diseña tu propia colección o arrasa en la pasarela. ¿Tienes lo que hace falta para ser una estrella de la Pasarela Habbo?"], "pt-br": ["Estes Mobis nunca saem de moda! Vire um ícone de estilo, crie sua própria coleção ou arrase na passarela. Você tem o que é preciso para ser uma estrela da Passarela Habbo?"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["sf_header_en", "sf_teaser_en"], "es": ["sf_header_es", "sf_teaser_en"], "pt-br": ["sf_header_pt", "sf_teaser_en"]}' AS JSON)
WHERE `ref_page_id` = 240 AND `localization_images` = CAST('{"en": ["sf_header_en", "sf_teaser_en"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_shal_header1_en", "catalog_shal_teaser_en"], "es": ["catalog_shal_header1_en", "catalog_shal_teaser_en"], "pt-br": ["catalog_shal_header1_en", "catalog_shal_teaser_en"]}' AS JSON)
WHERE `ref_page_id` = 241 AND `localization_images` = CAST('{"en": ["catalog_shal_header1_en", "catalog_shal_teaser_en"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_sports_headline1_ca", "catalog_sports_teaser1"], "es": ["catalog_sports_headline1_es", "catalog_sports_teaser1"], "pt-br": ["catalog_sports_headline1_br", "catalog_sports_teaser1"]}' AS JSON)
WHERE `ref_page_id` = 242 AND `localization_images` = CAST('{"en": ["catalog_sports_headline1_ca", "catalog_sports_teaser1"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["Stars_header", "summer_teaser"], "es": ["Stars_header", "summer_teaser"], "pt-br": ["Stars_header", "summer_teaser"]}' AS JSON), `localization_texts` = CAST('{"en": ["Summertime! Habbos are heading for beautiful destinations to enjoy the sun, sea and sand. Build your own beach resort and throw the best summer parties in Habbo!", "Click on the item you want for more information"], "es": ["¡Es verano! Los Habbos se van a destinos preciosos para disfrutar del sol, el mar y la arena. ¡Construye tu propio complejo de playa y monta las mejores fiestas de verano de Habbo!", "Haz clic en el objeto que quieras para más información"], "pt-br": ["É verão! Os Habbos estão indo para destinos lindos curtir o sol, o mar e a areia. Monte seu próprio resort de praia e faça as melhores festas de verão do Habbo!", "Clique no item que quiser para mais detalhes."]}' AS JSON)
WHERE `ref_page_id` = 243 AND `localization_images` = CAST('{"en": ["Stars_header", "summer_teaser"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["Summertime! Habbos are heading for beautiful destinations to enjoy the sun, sea and sand. Build your own beach resort and throw the best summer parties in Habbo!", "Click on the item you want for more information", ""], "es": ["¡Es verano! Los Habbos se van a destinos preciosos para disfrutar del sol, el mar y la arena. ¡Construye tu propio complejo de playa y monta las mejores fiestas de verano de Habbo!", "Haz clic en el objeto que quieras para más información", ""], "pt-br": ["É verão! Os Habbos estão indo para destinos lindos curtir o sol, o mar e a areia. Monte seu próprio resort de praia e faça as melhores festas de verão do Habbo!", "Clique no item que quiser para mais informações", ""]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_doors_headline1", "catalog_teleports_teaser2_001"], "es": ["catalog_doors_headline1", "catalog_teleports_teaser2_001"], "pt-br": ["catalog_doors_headline1_br", "catalog_teleports_teaser2_001"]}' AS JSON), `localization_texts` = CAST('{"en": ["Beam yourself from one room to another with our space age teleports. Buy a pair, put one in each room, and you''ve got your own suite! Teleports are sold in pairs (obviously!), so if you trade for them, check that you''re getting a linked pair.", "Click on an item for more information."], "es": ["¡Cambia de Sala Privada a la velocidad de la luz! Al comprarlo, recibirás dos Teleports que deberás poner en Salas distintas para así poder viajar entre ellas. IMPORTANTE: los teleports siempre se venden en parejas, así que si tradeas, asegúrate de recibir un par linkeado.", "Haz clic en un objeto para más información."], "pt-br": ["Vá de um Quarto para outro num piscar de olhos. Os Teletransportes são vendidos em pares, fique atento para pegar o par certo se resolver trocá-lo depois com alguém.", "Clique em um item para mais detalhes."]}' AS JSON)
WHERE `ref_page_id` = 244 AND `localization_images` = CAST('{"en": ["catalog_doors_headline1", "catalog_teleports_teaser2_001", ""]}' AS JSON) AND `localization_texts` = CAST('{"en": ["Beam yourself from one room to another with our space age teleports. Buy a pair, put one in each room, and you''ve got your own suite! Teleports are sold in pairs (obviously!), so if you trade for them, check that you''re getting a linked pair.", "Click on an item for more information.", ""], "es": ["Teletranspórtate de una sala a otra con nuestros teletransportes de la era espacial. Compra un par, pon uno en cada sala ¡y tendrás tu propia suite! Los teletransportes se venden en pares (¡obviamente!), así que si los consigues en un tradeo, comprueba que sea un par conectado.", "Haz clic en un objeto para más información.", ""], "pt-br": ["Teleporte-se de um quarto para outro com os nossos teleportes da era espacial. Compre um par, coloque um em cada quarto e pronto: você tem sua própria suíte! Os teleportes são vendidos em pares (é claro!), então se você conseguir numa troca, confira se é um par conectado.", "Clique em um item para mais informações.", ""]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_tiki_header1_en", "catalog_tiki_teaser1_en"], "es": ["catalog_tiki_header1_en", "catalog_tiki_teaser1_en"], "pt-br": ["catalog_tiki_header1_en", "catalog_tiki_teaser1_en"]}' AS JSON), `localization_texts` = CAST('{"en": ["Whisk yourself away to a tropical paradise with Tiki furniture. The smooth wood tones and tropical brights make the perfect setting for a beach party."], "es": ["Escápate a un paraíso tropical con los Furni Tiki. Los finos acabados de la madera y los colores vivos del trópico son el escenario perfecto para una fiesta en la playa."], "pt-br": ["Fuja para um paraíso tropical com os mobis Tiki. Os tons suaves da madeira e as cores vivas dos trópicos são o cenário perfeito para uma festa na praia."]}' AS JSON)
WHERE `ref_page_id` = 245 AND `localization_images` = CAST('{"en": ["catalog_tiki_header1_en", "catalog_tiki_teaser1_en"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["Whisk yourself away to a tropical paradise with Tiki furniture. The smooth wood tones and tropical brights make the perfect setting for a beach party."], "es": ["Escápate a un paraíso tropical con el mobiliario Tiki. Los suaves tonos de la madera y los colores vivos del trópico son el escenario perfecto para una fiesta en la playa."], "pt-br": ["Fuja para um paraíso tropical com os mobis Tiki. Os tons suaves da madeira e as cores vivas dos trópicos são o cenário perfeito para uma festa na praia."]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_trophies_headline1_ca"], "es": ["catalog_trophies_headline1_es"], "pt-br": ["catalog_trophies_headline1_br"]}' AS JSON), `localization_texts` = CAST('{"en": ["Everyone''s a winner! Reward all your friends with our fabulous glittering array of bronze, silver and gold trophies.\\r\\nFirst, choose the trophy model, the metal and then carefully type your inscription - it''s permanent!.\\r\\nWe''ll engrave the inscription along with your name and the date."], "es": ["¿Tienes madera de campeón? Elige tu Trofeo preferido haciendo clic sobre él y, en la placa, escribe un texto recordatorio. ¡Ojo porque el espacio es limitado y el mensaje permanente."], "pt-br": ["Todo mundo é campeão. E agora você pode presentear quem quiser com Ouro, Prata ou Bronze. Escolha o modelo do Troféu na seta e depois clique na cor desejada."]}' AS JSON)
WHERE `ref_page_id` = 246 AND `localization_images` = CAST('{"en": ["catalog_trophies_headline1_ca"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["Everyone''s a winner! Reward all your friends with our fabulous glittering array of bronze, silver and gold trophies.\\r\\nFirst, choose the trophy model, the metal and then carefully type your inscription - it''s permanent!.\\r\\nWe''ll engrave the inscription along with your name and the date."], "es": ["¡Todos son ganadores! Premia a todos tus amigos con nuestra fabulosa colección de brillantes trofeos de bronce, plata y oro.\\r\\nPrimero elige el modelo de trofeo y el metal, y luego escribe con cuidado tu dedicatoria: ¡es permanente!\\r\\nGrabaremos la dedicatoria junto con tu nombre y la fecha."], "pt-br": ["Todo mundo é vencedor! Premie todos os seus amigos com a nossa fabulosa coleção de troféus brilhantes de bronze, prata e ouro.\\r\\nPrimeiro escolha o modelo do troféu e o metal, depois escreva sua dedicatória com cuidado: ela é permanente!\\r\\nVamos gravar a dedicatória junto com o seu nome e a data."]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["urban_header_en", "urban_teaser_en"], "es": ["urban_header_es", "urban_teaser_en"], "pt-br": ["urban_header_br", "urban_teaser_en"]}' AS JSON), `localization_texts` = CAST('{"en": ["With its city-inspired style, Urban is perfect for any street, alleyway or road. Rubbish bins, street lights and benches - all the Urban furniture you need!", "Click on an item for more details."], "es": ["Con un estilo inspirado en la ciudad, este Furni es perfecto cualquier calle, callejón o carretera. Contenedores, farolas y bancos. ¡Todo para armar las calles que necesitas!", "Haz clic en un objeto para ver más detalles."], "pt-br": ["Com seu estilo inspirado na cidade, Cidade Grande é perfeita para qualquer rua, beco ou estrada. Lixeiras, postes e bancos... todos os mobis que você precisa!", "Clique em um item para ver mais detalhes."]}' AS JSON)
WHERE `ref_page_id` = 247 AND `localization_images` = CAST('{"en": ["urban_header_en", "urban_teaser_en"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["With its city-inspired style, Urban is perfect for any street, alleyway or road. Rubbish bins, street lights and benches - all the Urban furniture you need!", "Click on an item for more details."], "es": ["Con su estilo inspirado en la ciudad, Urbano es perfecto para cualquier calle, callejón o carretera. Contenedores, farolas y bancos: ¡todo el mobiliario Urbano que necesitas!", "Haz clic en un objeto para ver más detalles."], "pt-br": ["Com seu estilo inspirado na cidade, Urbano é perfeito para qualquer rua, beco ou estrada. Lixeiras, postes e bancos: todos os mobis Urbanos de que você precisa!", "Clique em um item para ver mais detalhes."]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["Valentines_header", "catalog_love_teaser1"], "es": ["Valentines_header", "catalog_love_teaser1"], "pt-br": ["catalog_va2_headline1_en", "catalog_love_teaser1"]}' AS JSON)
WHERE `ref_page_id` = 248 AND `localization_images` = CAST('{"en": ["Valentines_header", "catalog_love_teaser1"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_vir_header1_en", "catalog_vir_teaser_en"], "es": ["catalog_vir_header1_en", "catalog_vir_teaser_en"], "pt-br": ["catalog_vir_header1_pt", "catalog_vir_teaser_en"]}' AS JSON), `localization_texts` = CAST('{"en": ["A virus is spreading through Habbo Hotel. Many casualties reported and it could get much worse! Whether you are hoping to get infected Habbos or look after number one, get your clean hands on our terrifying Infection Furni.", "Click on an item to see more details."], "es": ["Un virus se está extendiendo por Habbo Hotel. Hay muchas víctimas y ¡podría ir a peor! No importa si quieres contagiarte o prevenir la infección, aquí tienes nuestro terrorífico Furni Virus. Manéjalo con las manos limpias...", "Haz clic en un objeto para ver más detalles."], "pt-br": ["Um vírus está se espalhando pelo Habbo Hotel. Muitas vítimas foram registradas e pode piorar! Se você quer infectar outros Habbos ou só se proteger, pegue nossos aterrorizantes Mobis da Infecção com as mãos limpas.", "Clique em um item para ver mais detalhes."]}' AS JSON)
WHERE `ref_page_id` = 249 AND `localization_images` = CAST('{"en": ["catalog_vir_header1_en", "catalog_vir_teaser_en"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["A virus is spreading through Habbo Hotel. Many casualties reported and it could get much worse! Whether you are hoping to get infected Habbos or look after number one, get your clean hands on our terrifying Infection Furni.", "Click on an item to see more details."], "es": ["Un virus se está extendiendo por Habbo Hotel. Hay muchas víctimas y ¡podría ir a peor! Tanto si quieres contagiar a otros Habbos como si prefieres cuidarte, consigue nuestro terrorífico Furni de la Infección con las manos limpias.", "Haz clic en un objeto para ver más detalles."], "pt-br": ["Um vírus está se espalhando pelo Habbo Hotel. Muitas vítimas foram registradas e pode piorar! Se você quer infectar outros Habbos ou só se proteger, pegue nossos aterrorizantes Mobis da Infecção com as mãos limpas.", "Clique em um item para ver mais detalhes."]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["ctlg_windows_headline1_en", "ctlg_windows_teaser1_en", "catalog_special_txtbg1"], "es": ["ctlg_windows_headline1_es", "ctlg_windows_teaser1_en"], "pt-br": ["ctlg_windows_headline1_br", "ctlg_windows_teaser1_en", "catalog_special_txtbg1"]}' AS JSON), `localization_texts` = CAST('{"en": ["Let some sunshine in! Our windows come in many styles to give a unique look to your room. Who said your room does not have a view? It does now.", "Click on the item you want for more information", "Ooh, new view!"], "es": ["¡Deja que la luz entre en tu Sala! Tienes ventanas de diferentes estilos para darle un toque único.", "Haz clic en un objeto para más información.", "¡Nuevas vistas!"], "pt-br": ["Deixe o sol entrar! Tire o mofo do seu Quarto com uma Janela. Quem disse que você não pode ter uma vista?", "Clique no item que quiser para mais informações", "Isso é que é vista!"]}' AS JSON)
WHERE `ref_page_id` = 250 AND `localization_images` = CAST('{"en": ["ctlg_windows_headline1_en", "ctlg_windows_teaser1_en", "catalog_special_txtbg1"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["Let some sunshine in! Our windows come in many styles to give a unique look to your room. Who said your room does not have a view? It does now.", "Click on the item you want for more information", "Ooh, new view!"], "es": ["¡Deja entrar un poco de sol! Nuestras ventanas vienen en muchos estilos para darle a tu sala un aspecto único. ¿Quién dijo que tu sala no tiene vistas? Ahora sí las tiene.", "Haz clic en el objeto que quieras para más información", "¡Oh, vistas nuevas!"], "pt-br": ["Deixe o sol entrar! Nossas janelas vêm em vários estilos para dar um visual único ao seu quarto. Quem disse que o seu quarto não tem vista? Agora tem.", "Clique no item que quiser para mais informações", "Uau, vista nova!"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_xmas_headline2_en", "xmas2009_catalogue"], "es": ["catalog_xmas_headline2_es", "xmas2009_catalogue"], "pt-br": ["catalog_xmas_headline2_br", "xmas2009_catalogue"]}' AS JSON), `localization_texts` = CAST('{"en": ["Get yourself into the Christmas spirit and check out all the new items we''ve prepared for you.", "Click on the item you want for more information"], "es": ["En el mundo latino, la tradición anual más universal es Navidad. Arbolitos, velas, patitos, postres, el pavo navideño, guirnaldas... Aquí hay todo para pasar una feliz Navidad.", "Haz clic en el objeto que quieras para más información"], "pt-br": ["Entre no espírito natalino e confira todos os Mobis novos que preparamos para você.", "Clique no item para ver mais detalhes."]}' AS JSON)
WHERE `ref_page_id` = 251 AND `localization_images` = CAST('{"en": ["catalog_xmas_headline2_en", "xmas2009_catalogue"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["Get yourself into the Christmas spirit and check out all the new items we''ve prepared for you.", "Click on the item you want for more information", ""], "es": ["Ponte en el espíritu navideño y descubre todos los objetos nuevos que hemos preparado para ti.", "Haz clic en el objeto que quieras para más información", ""], "pt-br": ["Entre no espírito natalino e confira todos os itens novos que preparamos para você.", "Clique no item que quiser para mais informações", ""]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["flowerpower_header_en", "garden_teaser_en"], "es": ["flowerpower_header_en", "garden_teaser_en"], "pt-br": ["flowerpower_header_en", "garden_teaser_en"]}' AS JSON), `localization_texts` = CAST('{"en": ["The Hotel has been overrun by this botanical bonanza of greenery and color. With these newest shroomy additions, what will you build?"], "es": ["El Hotel ha sido invadido por esta explosión botánica de verde y color. Cuidado con la planta carnívora... ¿Qué vas a construir?"], "pt-br": ["O Hotel foi tomado por esta explosão botânica de verde e cor. Com estes novos cogumelos, o que você vai construir?"]}' AS JSON)
WHERE `ref_page_id` = 252 AND `localization_images` = CAST('{"en": ["flowerpower_header_en", "garden_teaser_en"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["The Hotel has been overrun by this botanical bonanza of greenery and color. With these newest shroomy additions, what will you build?"], "es": ["El Hotel ha sido invadido por esta explosión botánica de verde y color. Con estas nuevas incorporaciones de setas, ¿qué vas a construir?"], "pt-br": ["O Hotel foi tomado por esta explosão botânica de verde e cor. Com estes novos cogumelos, o que você vai construir?"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_roller_headline1", "catalog_teaser_rollers_en"], "es": ["catalog_roller_headline1", "catalog_teaser_rollers_en"], "pt-br": ["catalog_roller_headline1", "catalog_teaser_rollers_en"]}' AS JSON), `localization_texts` = CAST('{"en": ["Move your imagination, while you move your Habbo! Perfect for mazes, games, for keeping your queue moving or making your pet go round in circles for hours. Available in multi-packs - the more you buy, the cheaper the Roller!", "Click on a Roller for more information."], "es": ["Si con una simple palanca se puede mover el mundo, con un Habbo Roller pondrás patas arriba el ciberespacio entero. ¡Muévete, Habbo! ¡Muévete!", "Haz clic en un Roller para más información."], "pt-br": ["Movimente sua imaginação! Compre vários Rollers e crie jogos, festas, desafios, o que você quiser...", "Clique em um Roller para mais informações."]}' AS JSON)
WHERE `ref_page_id` = 253 AND `localization_images` = CAST('{"en": ["catalog_roller_headline1", "catalog_teaser_rollers_en"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["Move your imagination, while you move your Habbo! Perfect for mazes, games, for keeping your queue moving or making your pet go round in circles for hours. Available in multi-packs - the more you buy, the cheaper the Roller!", "Click on a Roller for more information."], "es": ["¡Mueve tu imaginación mientras mueves a tu Habbo! Perfectos para laberintos, juegos, para que tu cola avance o para que tu mascota dé vueltas durante horas. Se venden en paquetes: ¡cuantos más compras, más barato sale cada Roller!", "Haz clic en un Roller para más información."], "pt-br": ["Movimente sua imaginação enquanto movimenta seu Habbo! Perfeitos para labirintos, jogos, para fazer a fila andar ou deixar seu mascote dando voltas por horas. Vendidos em pacotes: quanto mais você compra, mais barato sai cada Roller!", "Clique em um Roller para mais informações."]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_twilight_header_en", "twi_teaser"], "es": ["catalog_twilight_header_es", "twi_teaser"], "pt-br": ["catalog_twilight_header_pt", "twi_teaser"]}' AS JSON)
WHERE `ref_page_id` = 254 AND `localization_images` = CAST('{"en": ["catalog_twilight_header_en", "twi_teaser"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_header_AI1_en", "catalog_teaser_AI1_en"], "es": ["catalog_header_AI1_en", "catalog_teaser_AI1_en"], "pt-br": ["catalog_header_AI1_en", "catalog_teaser_AI1_en"]}' AS JSON)
WHERE `ref_page_id` = 255 AND `localization_images` = CAST('{"en": ["catalog_header_AI1_en", "catalog_teaser_AI1_en"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_spw_header3_de", "catalog_spw_teaser1_es"], "es": ["catalog_spw_header3_de", "catalog_spw_teaser1_es"], "pt-br": ["catalog_spw_header3_de", "catalog_spw_teaser1_es"]}' AS JSON)
WHERE `ref_page_id` = 257 AND `localization_images` = CAST('{"en": ["catalog_spw_header2_es", "catalog_spw_teaser1_es"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_djshop_headline1_uk", "catalog_djshop_teaser1"], "es": ["catalog_djshop_headline1_uk", "catalog_djshop_teaser1"], "pt-br": ["catalog_djshop_headline1_uk", "catalog_djshop_teaser1"]}' AS JSON), `localization_texts` = CAST('{"en": ["You must own a Traxmachine and at least one trax-pax. Place the Traxmachine in your room and the trax-pax in your inventory. Double click the Traxmachine, click \\"Trax Editor\\" and start editing music. When ready, save, select the tune, turn on the Traxmachine and enjoy.", "Cool, my own music!"], "es": ["Necesitas una Traxmachine y al menos un trax-pax. Pon la Traxmachine en tu sala y el trax-pax en tu inventario. Haz doble clic en la Traxmachine, haz clic en \\"Editor de Trax\\" y empieza a editar música. Cuando esté lista, guárdala, elige la canción, enciende la Traxmachine y disfruta.", "¡Genial, mi propia música!"], "pt-br": ["Você precisa de uma Traxmachine e de pelo menos um trax-pax. Coloque a Traxmachine no seu quarto. Clique duas vezes na Traxmachine, clique em \\"Editor de Trax\\" e comece a editar música. Quando estiver pronta, salve, escolha a música, ligue a Traxmachine e curta.", "Demais, minha própria música!"]}' AS JSON)
WHERE `ref_page_id` = 1101 AND `localization_images` = CAST('{"en": ["catalog_djshop_headline1_uk", "catalog_djshop_teaser1"]}' AS JSON) AND `localization_texts` = CAST('{"en": ["You must own a Traxmachine and at least one trax-pax. Place the Traxmachine in your room and the trax-pax in your inventory. Double click the Traxmachine, click \\"Trax Editor\\" and start editing music. When ready, save, select the tune, turn on the Traxmachine and enjoy.", "Cool, my own music!"], "es": ["Necesitas una Traxmachine y al menos un trax-pax. Pon la Traxmachine en tu sala y el trax-pax en tu inventario. Haz doble clic en la Traxmachine, haz clic en \\"Editor de Trax\\" y empieza a editar música. Cuando esté lista, guárdala, elige la canción, enciende la Traxmachine y disfruta.", "¡Genial, mi propia música!"], "pt-br": ["Você precisa de uma Traxmachine e de pelo menos um trax-pax. Coloque a Traxmachine no seu quarto e o trax-pax no seu inventário. Clique duas vezes na Traxmachine, clique em \\"Editor de Trax\\" e comece a editar música. Quando estiver pronta, salve, escolha a música, ligue a Traxmachine e curta.", "Demais, minha própria música!"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_trx_header1_en", "catalog_trx_teaser1_en"], "es": ["catalog_trx_header1_en", "catalog_trx_teaser1_en"], "pt-br": ["catalog_trx_header1_en", "catalog_trx_teaser1_en"]}' AS JSON)
WHERE `ref_page_id` = 1102 AND `localization_images` = CAST('{"en": ["catalog_trx_header1_en", "catalog_trx_teaser1_en"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_trx_header2_en", "catalog_trx_teaser2_en"], "es": ["catalog_trx_header2_en", "catalog_trx_teaser2_en"], "pt-br": ["catalog_trx_header2_en", "catalog_trx_teaser2_en"]}' AS JSON)
WHERE `ref_page_id` = 1103 AND `localization_images` = CAST('{"en": ["catalog_trx_header2_en", "catalog_trx_teaser2_en"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_trx_header3_en", "catalog_trx_teaser3_en"], "es": ["catalog_trx_header3_en", "catalog_trx_teaser3_en"], "pt-br": ["catalog_trx_header3_en", "catalog_trx_teaser3_en"]}' AS JSON)
WHERE `ref_page_id` = 1104 AND `localization_images` = CAST('{"en": ["catalog_trx_header3_en", "catalog_trx_teaser3_en"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_trx_header4_en", "catalog_trx_teaser4_en"], "es": ["catalog_trx_header4_en", "catalog_trx_teaser4_en"], "pt-br": ["catalog_trx_header4_en", "catalog_trx_teaser4_en"]}' AS JSON)
WHERE `ref_page_id` = 1105 AND `localization_images` = CAST('{"en": ["catalog_trx_header4_en", "catalog_trx_teaser4_en"]}' AS JSON);
UPDATE `definition_catalog_page_body` SET `localization_images` = CAST('{"en": ["catalog_trx_header5_en", "catalog_trx_teaser5_en"], "es": ["catalog_trx_header5_en", "catalog_trx_teaser5_en"], "pt-br": ["catalog_trx_header5_en", "catalog_trx_teaser5_en"]}' AS JSON)
WHERE `ref_page_id` = 1106 AND `localization_images` = CAST('{"en": ["catalog_trx_header5_en", "catalog_trx_teaser5_en"]}' AS JSON);
INSERT IGNORE INTO `definition_catalog_page_header` (`id`, `parent_id`, `color`, `icon`, `display_name`, `coming_soon`, `enabled`, `min_rank`, `order_num`)
SELECT 1107, 11, 0, 257, CAST('{"en": "IRL Hits", "es": "Éxitos de la vida real", "pt-br": "Hits da IRL"}' AS JSON), 0, 1, 1, 7
FROM `definition_catalog_page_header` WHERE `id` = 11 AND `display_name` ->> '$.en' = 'Trax';
INSERT IGNORE INTO `definition_catalog_page_body` (`ref_page_id`, `layout_code`, `localization_images`, `localization_texts`)
SELECT `id`, 'default_3x3', CAST('{"en": ["music_header", "cata_trax_teaser"], "es": ["music_header", "cata_trax_teaser"], "pt-br": ["music_header", "cata_trax_teaser"]}' AS JSON), CAST('{"en": ["Certified banger or straight to the Khia asylum? Pick your favourite hits, grab your friends and sing like nobody''s listening."], "es": ["¿Un temazo o directo al Khia Asylum? Elige tus éxitos favoritos, junta a tus amigos y canta como si nadie te escuchara."], "pt-br": ["Hino certificado ou direto pro Khia Asylum? Escolha seus hits favoritos, chame os amigos e cante como se ninguém estivesse ouvindo."]}' AS JSON)
FROM `definition_catalog_page_header` WHERE `id` = 1107 AND `display_name` = CAST('{"en": "IRL Hits", "es": "Éxitos de la vida real", "pt-br": "Hits da IRL"}' AS JSON);
INSERT IGNORE INTO `definition_catalog_page_body` (`ref_page_id`, `layout_code`, `localization_images`, `localization_texts`)
SELECT `id`, 'default_3x3', CAST('{}' AS JSON), CAST('{}' AS JSON)
FROM `definition_catalog_page_header` WHERE `id` = 2;
INSERT IGNORE INTO `definition_catalog_page_body` (`ref_page_id`, `layout_code`, `localization_images`, `localization_texts`)
SELECT `id`, 'default_3x3', CAST('{}' AS JSON), CAST('{}' AS JSON)
FROM `definition_catalog_page_header` WHERE `id` = 6;
INSERT IGNORE INTO `definition_catalog_page_body` (`ref_page_id`, `layout_code`, `localization_images`, `localization_texts`)
SELECT `id`, 'default_3x3', CAST('{}' AS JSON), CAST('{}' AS JSON)
FROM `definition_catalog_page_header` WHERE `id` = 9;
INSERT IGNORE INTO `definition_catalog_page_body` (`ref_page_id`, `layout_code`, `localization_images`, `localization_texts`)
SELECT `id`, 'default_3x3', CAST('{}' AS JSON), CAST('{}' AS JSON)
FROM `definition_catalog_page_header` WHERE `id` = 13;

UPDATE `definition_navigator_public_rooms_categories` SET `caption` = JSON_OBJECT('en', `caption`);
ALTER TABLE `definition_navigator_public_rooms_categories`
  MODIFY `caption` json NOT NULL;
UPDATE `definition_navigator_public_rooms_categories` c JOIN (VALUES
    ROW('Lobbies', 'Vestíbulos', 'Saguões'),
    ROW('Cafés', 'Cafeterías', 'Cafés'),
    ROW('Outside Spaces', 'Espacios al aire libre', 'Áreas externas'),
    ROW('Dance Clubs', 'Discotecas', 'Baladas'),
    ROW('Restaurants', 'Restaurantes', 'Restaurantes'),
    ROW('Entertainment', 'Entretenimiento', 'Entretenimento'),
    ROW('Habbo Club', 'Habbo Club', 'Habbo Club'),
    ROW('Sponsored Rooms', 'Salas patrocinadas', 'Quartos patrocinados')) AS tr (en, es, pt)
  ON c.`caption` ->> '$.en' = tr.en
SET c.`caption` = JSON_MERGE_PATCH(c.`caption`, JSON_OBJECT('es', tr.es, 'pt-br', tr.pt));
ALTER TABLE `definition_navigator_public_rooms`
  ADD COLUMN `caption` json DEFAULT NULL COMMENT 'its name in the navigator, in each language - NULL: the room''s label' AFTER `image_type`,
  ADD COLUMN `description` json DEFAULT NULL COMMENT 'in each language - NULL: the room''s description' AFTER `caption`;
UPDATE `definition_navigator_public_rooms` np JOIN `rooms` r ON r.`id` = np.`room_ref` JOIN (VALUES
    ROW('Ballroom', 'Estadio', 'Estádio Nike'),
    ROW('Club Massiva', 'Chill-out', 'Chill-out'),
    ROW('Dancefloor', 'Dance Floor', 'Pista de dança'),
    ROW('Battle Ball Lobby', 'Vestíbulo de Battle Ball', 'Saguão do Battle Ball'),
    ROW('Beauty Salon', 'Salón de Belleza', 'Salão Loreal'),
    ROW('Beauty Salon (2)', 'Salón de Belleza (2)', 'Salão Loreal (2)'),
    ROW('Cafe Olé', 'Cafe Olé', 'Café olé'),
    ROW('Zen Garden', 'Sala Relax', 'Jardim Zen'),
    ROW('Club Mammoth', 'Club Mamut', 'Clube Mamute'),
    ROW('Hotel Kitchen', 'Cocina con Fruta', 'Cozinha do Hotel'),
    ROW('Dusty Lounge', 'Dusty Lounge', 'Saguão Empoeirado'),
    ROW('Emperor''s Hall', 'Sala del Emperador', 'Salão do Imperador'),
    ROW('Median Lobby', 'El Salón Azul', 'Saguão Intermediário'),
    ROW('Basement Lobby', 'El Salón Verde', 'Saguão no Subsolo'),
    ROW('Skylight Lobby', 'Buhardilla', 'Saguão com Teto Solar'),
    ROW('Floating Garden', 'Jardín Flotante', 'Jardim Flutuante'),
    ROW('Imperial Park', 'Los Viajes de Gulliver', 'Parque Imperial'),
    ROW('Imperial Park (2)', 'Los Viajes de Gulliver (2)', 'Parque Imperial (2)'),
    ROW('Habburgers', 'Habburgers', 'Habburgers'),
    ROW('Main Lobby', 'La Antecámara', 'Saguão Principal'),
    ROW('Hotel Reception', 'El Recibidor', 'Recepção'),
    ROW('Old Skool Habbo', 'Sala Finlandia', 'Habbo Velha Guarda'),
    ROW('Old Skool Habbo (2)', 'Sala Finlandia (2)', 'Habbo Velha Guarda (2)'),
    ROW('Slice of Life', 'Pizza Nuestra', 'A Fatia'),
    ROW('Habbo Lido', 'Habbo Lido', 'A Piscina'),
    ROW('The Lido', 'Habbo Lido', 'A Piscina'),
    ROW('Rooftop Cafe', 'La Azotea', 'Café Cobertura'),
    ROW('Rooftop Cafe (2)', 'La Azotea (2)', 'Café Cobertura (2)'),
    ROW('The Space Cafe', 'La Sala de la revista TÚ', 'Café Trakinas'),
    ROW('Power Gym', 'El Gimnasio', 'Ginásio Habbo'),
    ROW('Sun Terrace', 'Terraza del Sol', 'Terraço SuperTeen'),
    ROW('Tea Room', 'Salón de Té', 'Sala de Chá Chinesa'),
    ROW('Eric''s Eaterie', 'El Rinconcito', 'Erics'),
    ROW('The Chromide Club', 'MTV Day', 'Chromide Club'),
    ROW('The Chromide Club (2)', 'MTV Day (2)', 'Chromide Club (2)'),
    ROW('Rooftop Rumble', 'AquaHabbo', 'Terraço Panorâmico'),
    ROW('SnowStorm Lobby', 'Salón SnowStorm', 'SnowStorm'),
    ROW('Ice Cafe', 'Media Game', 'Ice Café'),
    ROW('Theatredrome', 'Teatro', 'Teatro'),
    ROW('Club Orient', 'Club Oriente', 'Oriente Club'),
    ROW('Picnic Garden', 'El Parque', 'Piquenique Fanta!'),
    ROW('Netcafe', 'Net Café', 'Net Café'),
    ROW('The Dirty Duck Pub', 'The Dirty Duck Pub', 'Pub Dirty Duck'),
    ROW('Star Lounge', 'Palacio de Gelatina', 'Saguão das Estrelas'),
    ROW('Habbo Gardens', 'Parque', 'Jardim'),
    ROW('The Infobus', 'El Parque', 'Parque'),
    ROW('Hallway', 'Pasillo', 'Corredores que somem'),
    ROW('Hallway II', 'Pasillo II', 'Corredor II'),
    ROW('Habbo Cinema', 'Habbo Cine', 'Cinema Habbo'),
    ROW('The Den', 'El club billar', 'A Caverna'),
    ROW('Kultakahvila', 'Sonora', 'Kultakahvila'),
    ROW('Habbo Library', 'Biblioteca', 'Biblioteca'),
    ROW('Sunset Cafe', 'Sunset Café', 'Café Sunset'),
    ROW('MuchMusic HQ', 'Antena3', 'MuchMusic HQ'),
    ROW('The Dudesons'' Ranch', 'El Rancho de The Dudesons', 'O Rancho dos Dudesons'),
    ROW('The Capri-Sun Theme Park', 'El Parque Temático de Capri-Sun', 'O Parque Temático da Capri-Sun'),
    ROW('Cheetolab', 'Cheetolab', 'Cheetolab'),
    ROW('Disney XD', 'Disney XD', 'Disney XD'),
    ROW('Disney XD II', 'Disney XD II', 'Disney XD II'),
    ROW('Disney XD III', 'Disney XD III', 'Disney XD III'),
    ROW('Disney XD IV', 'Disney XD IV', 'Disney XD IV'),
    ROW('Disney XD V', 'Disney XD V', 'Disney XD V'),
    ROW('Disney XD VI', 'Disney XD VI', 'Disney XD VI'),
    ROW('Disney XD VII', 'Disney XD VII', 'Disney XD VII'),
    ROW('Hacker Hunt HQ', 'Cuartel de la Caza de Hackers', 'QG da Caça aos Hackers'),
    ROW('Kellogg''s Trésor', 'Kellogg''s Trésor', 'Kellogg''s Trésor'),
    ROW('M6 Mobile Sandstorm', 'M6 Mobile Tormenta de Arena', 'M6 Mobile Tempestade de Areia'),
    ROW('ELEMAGIKA', 'ELEMAGIKA', 'ELEMAGIKA'),
    ROW('Volkswagen Showroom', 'Concesionario Volkswagen', 'Concessionária Volkswagen'),
    ROW('The Wizards of Waverly Place Lair', 'La Guarida de Los magos de Waverly Place', 'O Covil de Os Feiticeiros de Waverly Place'),
    ROW('CREATE2011 City', 'Ciudad CREATE2011', 'Cidade CREATE2011'),
    ROW('Crush', 'Crush', 'Crush'),
    ROW('Coolpolitics Theatre', 'Teatro Coolpolitics', 'Teatro Coolpolitics'),
    ROW('Animax TV', 'Animax TV', 'Animax TV'),
    ROW('MTV VIP Lounge', 'MTV VIP Lounge', 'MTV VIP Lounge'),
    ROW('Garnier Beauty Salon', 'Salón de Belleza Garnier', 'Salão de Beleza Garnier'),
    ROW('Libra Beauty Salon', 'Salón de Belleza Libra', 'Salão de Beleza Libra'),
    ROW('Orange Crush Ice Lounge', 'Orange Crush Ice Lounge', 'Orange Crush Ice Lounge'),
    ROW('Chupa Chups', 'Chupa Chups', 'Chupa Chups'),
    ROW('The House of Hiccup', 'La Casa de Hipo', 'A Casa do Soluço'),
    ROW('Ice Age: Dawn of the Dinosaurs', 'Ice Age 3: El origen de los dinosaurios', 'A Era do Gelo 3'),
    ROW('Mount Olympus', 'Monte Olimpo', 'Monte Olimpo'),
    ROW('CREATE2010 City', 'Ciudad CREATE2010', 'Cidade CREATE2010'),
    ROW('GREFUPARTY ROOM', 'SALA GREFUPARTY', 'SALA GREFUPARTY'),
    ROW('Fire Station', 'Estación de Bomberos', 'Quartel dos Bombeiros'),
    ROW('Cloudy With a Chance of Meatballs', 'Lluvia de albóndigas', 'Tá Chovendo Hambúrguer'),
    ROW('American Idol Theatre', 'Teatro American Idol', 'Teatro American Idol'),
    ROW('Latin American Idol Theatre', 'Teatro Latin American Idol', 'Teatro Latin American Idol'),
    ROW('MTV EMA 2010', 'MTV EMA 2010', 'MTV EMA 2010'),
    ROW('Nouvelle Star Theatre', 'Teatro Nouvelle Star', 'Teatro Nouvelle Star'),
    ROW('Antena3', 'Antena3', 'Antena3'),
    ROW('M6 Studio', 'Estudio M6', 'Estúdio M6'),
    ROW('Clearasil Star Lounge', 'Clearasil Star Lounge', 'Clearasil Star Lounge'),
    ROW('NRJ Music Awards', 'NRJ Music Awards', 'NRJ Music Awards'),
    ROW('Nokia Club', 'Nokia Club', 'Nokia Club'),
    ROW('Nokia Club (2)', 'Nokia Club (2)', 'Nokia Club (2)'),
    ROW('Nokia X6', 'Nokia X6', 'Nokia X6'),
    ROW('Nokia X6 (2)', 'Nokia X6 (2)', 'Nokia X6 (2)')) AS tr (en, es, pt)
  ON r.`label` = tr.en
SET np.`caption` = JSON_OBJECT('en', r.`label`, 'es', tr.es, 'pt-br', tr.pt);
UPDATE `definition_navigator_public_rooms` np JOIN `rooms` r ON r.`id` = np.`room_ref` JOIN (VALUES
    ROW('Ballroom', 'Fiebre por el fútbol', 'Quantos gol vai fazer hoje, hem?'),
    ROW('Club Massiva', 'Baila, si eres un poco más atrevido', 'Descanse os pés depois de dançar!'),
    ROW('Dancefloor', '...y si eres un poco más atrevido... ¡Puedes pasar a bailar!', 'A balada é aqui'),
    ROW('Battle Ball Lobby', '¡Ven a jugar gratis!', 'Venha jogar de graça!'),
    ROW('Beauty Salon', '¡Nada de cirugía de píxeles, tan sólo maquillajes naturales!', 'Nada de cirurgia pixelástica, apenas maquiagem natural!'),
    ROW('Beauty Salon (2)', '¡Nada de cirugía de píxeles, tan sólo maquillajes naturales!', 'Nada de cirurgia pixelástica, apenas maquiagem natural!'),
    ROW('Cafe Olé', '¿Relajarse? Con un buen café', 'Relaxe com os amigos tomando um dos cafés exclusivos preparados pela Maria'),
    ROW('Zen Garden', 'En paz con tu Habbo alma', 'Pronto para meditar?'),
    ROW('Club Mammoth', 'Magnífico y monumental. Sólo para Habbos VIP o del Habbo Club', 'Monumental e magnífico. Somente para membros do Habbo Club.'),
    ROW('Hotel Kitchen', '¡Huele que alimenta!', 'Com fome? Quer um lanche rápido e gostoso? Entre e faça seu pedido! Hummm...'),
    ROW('Dusty Lounge', '¿Alérgic@ al polvo?', 'Nada que uma faxina não ajude'),
    ROW('Emperor''s Hall', '¿Tienes audiencia con él?', 'Apenas os Habbos majestosos!'),
    ROW('Median Lobby', '¡Paséate por Habbo!', 'Um meio-termo para o seu dia'),
    ROW('Basement Lobby', 'Entra y chatea con tus amigos de MiniJuegos.com, rétalos y juega con ellos a tus juegos favoritos', 'Para passar o tempo lá embaixo'),
    ROW('Skylight Lobby', '¡Descúbrelo!', 'Levante a cabeça e sinta a luz'),
    ROW('Floating Garden', 'Disfruta de la naturaleza', 'Sabe o que quer dizer "Suzhou"? Descubra aqui!'),
    ROW('Imperial Park', '¡Vive una enorme aventura!', 'Harmonia, simetria e... ninjas de pixels?!'),
    ROW('Imperial Park (2)', '¡Vive una enorme aventura!', 'Harmonia, simetria e... ninjas de pixels?!'),
    ROW('Habburgers', 'A la rica hamburguesa', 'Peça aqui seu lanche nutritivo e delicioso. Visite também o Giraffas mais próximo da sua casa.'),
    ROW('Main Lobby', 'Pasa, pasa, sin miedo...', 'Um dos principais pontos de partida do Habbo Hotel. Entre aqui, e ali, e acolá.'),
    ROW('Hotel Reception', 'Pasa, pasa, no tengas miedo...', 'Novo? Perdido? Receba nossas calorosas boas-vindas aqui.'),
    ROW('Old Skool Habbo', 'Una Sala para mirar al pasado', 'O lugar onde o Habbo começou'),
    ROW('Old Skool Habbo (2)', 'Una Sala para mirar al pasado', 'O lugar onde o Habbo começou'),
    ROW('Slice of Life', 'Las pizzas más sabrosas de todo habbo', 'A mansão da pizza'),
    ROW('Habbo Lido', 'Nada relaja más que un buen chapuzón', 'Mergulhe, espirre água e corra com sua roupa de banho.'),
    ROW('The Lido', 'Nada relaja más que un buen chapuzón', 'Mergulhe, espirre água e corra com sua roupa de banho.'),
    ROW('Rooftop Cafe', '¡Menudas vistas!', 'O lugar perfeito para você se divertir e encontrar amigos!'),
    ROW('Rooftop Cafe (2)', '¡Menudas vistas!', 'O lugar perfeito para você se divertir e encontrar amigos!'),
    ROW('The Space Cafe', 'Un lugar ideal para conocer gente, tomar algo y bailar hasta el amanecer', 'Ponto de encontro para Habbonautas'),
    ROW('Power Gym', 'Aquí podrás cultivar tu cuerpo', 'Esportistas de pixels, aqui é o seu lugar'),
    ROW('Sun Terrace', 'Nada como ponerse moreno...', 'Para tardes tranqüilas e um excelente bronzeado!'),
    ROW('Tea Room', 'Relájate al máximo', 'Uma atmosfera tranqüila e uma maravilhosas misturas de chás.'),
    ROW('Eric''s Eaterie', '¿Buscas un ambiente romántico para cenar con tu Habbopareja? Éste es el lugar ideal...', 'Virgula: a sala mais badalada do Hotel. Dance e divirta-se.'),
    ROW('The Chromide Club', ' Todos los estilos, todos los ritmos, todo el mundo.', 'Peixes de aquário também dançam'),
    ROW('The Chromide Club (2)', 'Mesas en la sala de delante y pista de baile al fondo', 'Peixes de aquário também dançam'),
    ROW('Rooftop Rumble', 'Desde lo más alto de Habbo', 'Jogue com equilíbrio e agilidade'),
    ROW('SnowStorm Lobby', '¡Ven y sé una avalancha para tus rivales!', 'SnowStorm: seja campeão.'),
    ROW('Ice Cafe', 'No te pierdas el último estreno de Jim Carrey', 'Mais gelado que o freezer de um esquimó'),
    ROW('Theatredrome', 'Estará con todos vosotros el domingo 10 a las 17:00 hora de México D.F.', 'Silêncio na platéia!'),
    ROW('Club Orient', 'Tenemos un hueco reservado para ti', 'Monumental e magnífico. Somente para membros do Habbo Club.'),
    ROW('Picnic Garden', '¡No hay mejor sitio para irse de picnic!', 'O lugar perfeito para você se divertir e encontrar amigos!'),
    ROW('Netcafe', 'Una Sala donde vivirás mil aventuras', 'Mobis de tecla de computador é entretenimento digital.'),
    ROW('The Dirty Duck Pub', 'Cuidado con lo que deseas...', 'O lugar perfeito para relaxar'),
    ROW('Star Lounge', 'La Expo para preparar la fiesta de tus 15 años el 2 y 3 de Julio en México DF', 'Ponto de encontro de grandes estrelas ;)'),
    ROW('Habbo Gardens', 'Visita el parque y el famoso Infobús', 'Visite o parque e o famoso Infobus'),
    ROW('The Infobus', 'Nada como estar en plena naturaleza', 'Sempre alguém interessante, com muito a dizer.'),
    ROW('Hallway', '¿Quieres conocer al maestro de las llaves de Habbo?', 'Passeie pelos corredores do hotel'),
    ROW('Hallway II', 'Un lugar apacible para leer o jugar al escondite', 'Passeie pelos corredores do hotel'),
    ROW('Habbo Cinema', '¿A ver qué estrenan hoy?', 'Também em cartaz: "Kick Warz 8 - Desta vez eu bano!"'),
    ROW('The Den', 'Próxima charla: martes 14 de junio, a las 17.00 hora peninsular española', 'Para relaxar e se recuperar'),
    ROW('Kultakahvila', 'Música como tú quieres', 'Apenas uma ilusão...'),
    ROW('Habbo Library', 'Leer es de sabios', 'Entre e vasculhe nossa incrível biblioteca de pixels.'),
    ROW('Sunset Cafe', '¡Siéntete como en casa!', 'Que beleza é o pôr do sol.'),
    ROW('MuchMusic HQ', '¡Interactúa con Antena3!', 'Local de trabalho dos gerentes do Hotel'),
    ROW('The Dudesons'' Ranch', 'El rancho de The Dudesons.', 'O rancho dos Dudesons.'),
    ROW('The Capri-Sun Theme Park', 'Participa para conseguir Furni exclusivo de la marca y un montón de premios geniales más.', 'Participe para ganhar Mobis exclusivos da marca e um monte de outros prêmios incríveis.'),
    ROW('Cheetolab', '¡Un Chester gigante, cámaras de televisión y máquinas de Cheetos! Pídenos una bolsa de Cheetos y es tuya. ¡Nos vemos allí!', 'Um Chester gigante, câmeras de TV e máquinas de Cheetos! Peça um pacote de Cheetos e é seu. Te vejo lá!'),
    ROW('Disney XD', 'Pasa el rato en la sala de Disney XD.', 'Fique de bobeira na sala da Disney XD.'),
    ROW('Disney XD II', 'Pasa el rato en la sala de Disney XD.', 'Fique de bobeira na sala da Disney XD.'),
    ROW('Disney XD III', 'Pasa el rato en la sala de Disney XD.', 'Fique de bobeira na sala da Disney XD.'),
    ROW('Disney XD IV', 'Pasa el rato en la sala de Disney XD.', 'Fique de bobeira na sala da Disney XD.'),
    ROW('Disney XD V', 'Pasa el rato en la sala de Disney XD.', 'Fique de bobeira na sala da Disney XD.'),
    ROW('Disney XD VI', 'Pasa el rato en la sala de Disney XD.', 'Fique de bobeira na sala da Disney XD.'),
    ROW('Disney XD VII', 'Pasa el rato en la sala de Disney XD.', 'Fique de bobeira na sala da Disney XD.'),
    ROW('Hacker Hunt HQ', 'Atrapa a los hackers antes de que te atrapen a ti.', 'Pegue os hackers antes que eles peguem você.'),
    ROW('Kellogg''s Trésor', '¡Únete a la caza de los devoradores de chocolate y gana los mejores premios!', 'Entre na caça aos devoradores de chocolate e ganhe os melhores prêmios!'),
    ROW('M6 Mobile Sandstorm', '¿Perdido en una tormenta de arena en el desierto? Tu móvil sigue teniendo señal, M6 Mobile de Orange.', 'Perdido numa tempestade de areia no deserto? Seu celular continua com sinal, M6 Mobile da Orange.'),
    ROW('ELEMAGIKA', '¡Entra a ver al León de Paddle Pop en su nueva película! ¡Únete a la aventura YA!', 'Entre e veja o Leão do Paddle Pop no seu novo filme! Entre na aventura JÁ!'),
    ROW('Volkswagen Showroom', 'Echa un vistazo al último Volkswagen.', 'Dê uma olhada no Volkswagen mais novo.'),
    ROW('The Wizards of Waverly Place Lair', '¡Cuéntanos qué magia desastrosa crearías! Los magos de Waverly Place en Disney Channel desde el 25 de abril en el Reino Unido.', 'Conte que magia desastrada você criaria! Os Feiticeiros de Waverly Place no Disney Channel a partir de 25 de abril no Reino Unido.'),
    ROW('CREATE2011 City', '¿Eres un ingeniero en ciernes con la cabeza llena de ideas? ¡Entonces CREATE2011 es para ti!', 'Você é um engenheiro em formação cheio de ideias? Então o CREATE2011 é para você!'),
    ROW('Crush', '¡Pásate por un Cherry Crush nuevecito!', 'Passe aqui para um Cherry Crush novinho!'),
    ROW('Coolpolitics Theatre', 'Alza la voz y hazte oír en el escenario de Coolpolitics.', 'Fale alto e seja ouvido no palco do Coolpolitics.'),
    ROW('Animax TV', '¡Sé diferente!', 'Seja diferente!'),
    ROW('MTV VIP Lounge', '¡Únete a miles de fans en el MTV VIP Lounge!', 'Junte-se a milhares de fãs no MTV VIP Lounge!'),
    ROW('Garnier Beauty Salon', 'Looks frescos, al natural.', 'Visuais renovados, do jeito natural.'),
    ROW('Libra Beauty Salon', 'Mímate en rosa.', 'Mime-se de rosa.'),
    ROW('Orange Crush Ice Lounge', 'Refréscate con un Orange Crush bien helado.', 'Refresque-se com um Orange Crush bem gelado.'),
    ROW('Chupa Chups', '¡Por un verano menos serio!', 'Por um verão menos sério!'),
    ROW('The House of Hiccup', 'Cómo entrenar a tu dragón llega a los cines de todo el país el 31 de marzo.', 'Como Treinar o Seu Dragão chega aos cinemas de todo o país em 31 de março.'),
    ROW('Ice Age: Dawn of the Dinosaurs', 'Relájate con la manada y los dinosaurios.', 'Relaxe com a turma e os dinossauros.'),
    ROW('Mount Olympus', 'El mundo de dioses y héroes de Percy Jackson.', 'O mundo de deuses e heróis de Percy Jackson.'),
    ROW('CREATE2010 City', '¿Tienes una idea que podría cambiar el mundo? ¡Trae tu mejor invento al concurso CREATE2010 y enséñale a la ciudad lo que sabes construir!', 'Tem uma ideia que pode mudar o mundo? Traga sua melhor invenção para o concurso CREATE2010 e mostre para a cidade o que você sabe construir!'),
    ROW('GREFUPARTY ROOM', '¡Es el cumpleaños de Grefusito! ¡Celébralo en la Grefuparty y gana los mejores premios este verano!', 'É o aniversário do Grefusito! Comemore na Grefuparty e ganhe os melhores prêmios neste verão!'),
    ROW('Fire Station', 'Donde pasan el rato los más valientes de Habbo.', 'Onde os mais corajosos do Habbo ficam de bobeira.'),
    ROW('Cloudy With a Chance of Meatballs', '¡Cae comida del cielo! Lluvia de albóndigas, ya en cines.', 'Está caindo comida do céu! Tá Chovendo Hambúrguer, já nos cinemas.'),
    ROW('American Idol Theatre', 'Sube al escenario al estilo Idol.', 'Suba ao palco no estilo Idol.'),
    ROW('Latin American Idol Theatre', 'Sube al escenario al estilo Latin American Idol.', 'Suba ao palco no estilo Latin American Idol.'),
    ROW('MTV EMA 2010', '¡Apoya a tus artistas favoritos! Busca el artículo MTV EMAI para votarlos ahora mismo y podrías ganar un Trofeo.', 'Apoie seus artistas favoritos! Procure o artigo MTV EMAI para votar neles agora mesmo e você pode ganhar um Troféu!'),
    ROW('Nouvelle Star Theatre', 'Canta con todo tu corazón en el escenario de Nouvelle Star.', 'Cante com todo o coração no palco do Nouvelle Star.'),
    ROW('Antena3', '¡Interactúa con Antena3!', 'Interaja com a Antena3!'),
    ROW('M6 Studio', 'En antena con M6.', 'No ar com a M6.'),
    ROW('Clearasil Star Lounge', 'Piel limpia, escenario limpio: ¡brilla en el Clearasil Star Lounge!', 'Pele limpa, palco limpo: brilhe no Clearasil Star Lounge!'),
    ROW('NRJ Music Awards', '¡Desfila por la alfombra roja de los NRJ Music Awards!', 'Desfile no tapete vermelho do NRJ Music Awards!'),
    ROW('Nokia Club', 'La música te hace hablar, ¡así que baila toda la noche en el Nokia Club!', 'A música faz você falar, então dance a noite toda no Nokia Club!'),
    ROW('Nokia Club (2)', 'Recupera el aliento arriba, en el Nokia Club.', 'Recupere o fôlego lá em cima, no Nokia Club.'),
    ROW('Nokia X6', 'Navegación paso a paso gratis con el nuevo Ovi Maps, ¡para que nunca te pierdas la fiesta con tu Nokia X6!', 'Navegação passo a passo grátis com o novo Ovi Maps, para você nunca perder a festa com o seu Nokia X6!'),
    ROW('Nokia X6 (2)', 'Recupera el aliento arriba, en el Nokia X6.', 'Recupere o fôlego lá em cima, no Nokia X6.')) AS tr (en, es, pt)
  ON r.`label` = tr.en
SET np.`description` = JSON_OBJECT('en', r.`description`, 'es', tr.es, 'pt-br', tr.pt);

ALTER TABLE `definition_room_category` DROP INDEX `uq_definition_room_category_label`;
UPDATE `definition_room_category` SET `label` = JSON_OBJECT('en', `label`);
ALTER TABLE `definition_room_category`
  MODIFY `label` json NOT NULL;
ALTER TABLE `definition_room_category` ADD UNIQUE KEY `uq_definition_room_category_label` ((CAST(`label` ->> '$.en' AS CHAR(100)) COLLATE utf8mb4_unicode_ci));
UPDATE `definition_room_category` c JOIN (VALUES
    ROW('Chatting', 'Charlar', 'Bate-papo'),
    ROW('Games', 'Juegos', 'Jogos'),
    ROW('Habbo Club', 'Habbo Club', 'Habbo Club'),
    ROW('Help & Guides', 'Ayuda y guías', 'Ajuda e guias'),
    ROW('No category', 'Sin categoría', 'Sem categoria'),
    ROW('Parties', 'Fiestas', 'Festas'),
    ROW('Public Rooms', 'Salas públicas', 'Quartos públicos'),
    ROW('Roleplay', 'Rol', 'RPG'),
    ROW('Staff only', 'Solo Staff', 'Só Equipe'),
    ROW('Trading', 'Tradeos', 'Trocas')) AS tr (en, es, pt)
  ON c.`label` ->> '$.en' = tr.en
SET c.`label` = JSON_MERGE_PATCH(c.`label`, JSON_OBJECT('es', tr.es, 'pt-br', tr.pt));

UPDATE `definition_moderation_presets` SET `category` = JSON_OBJECT('en', `category`), `name` = JSON_OBJECT('en', `name`), `message` = JSON_OBJECT('en', `message`);
ALTER TABLE `definition_moderation_presets`
  MODIFY `category` json NOT NULL,
  MODIFY `name` json NOT NULL,
  MODIFY `message` json NOT NULL;
UPDATE `definition_moderation_presets` p JOIN (VALUES
    ROW('Sexual content', 'Contenido sexual', 'Conteúdo sexual'),
    ROW('Personal information', 'Información personal', 'Informações pessoais'),
    ROW('Scamming', 'Estafas', 'Golpes'),
    ROW('Bullying', 'Acoso', 'Bullying'),
    ROW('Disruption', 'Molestar', 'Bagunça'),
    ROW('Sexual talk', 'Conversación sexual', 'Conversa sexual'),
    ROW('Sharing personal details', 'Compartir datos personales', 'Compartilhar dados pessoais'),
    ROW('Bullying or harassment', 'Acoso o intimidación', 'Bullying ou assédio'),
    ROW('Disrupting', 'Molestar', 'Atrapalhar'),
    ROW('Please stop what you are doing - it goes against the Habbo Way.', 'Por favor, deja de hacer lo que estás haciendo: va en contra de la Manera Habbo.', 'Por favor, pare com o que está fazendo: isso vai contra a Habbo Etiqueta.'),
    ROW('Please do not share personal information in the hotel, yours or anyone else''s.', 'Por favor, no compartas información personal en el hotel, ni tuya ni de nadie.', 'Por favor, não compartilhe informações pessoais no hotel, nem suas nem de ninguém.'),
    ROW('Please be kind to other Habbos.', 'Por favor, sé amable con los demás Habbos.', 'Por favor, seja gentil com os outros Habbos.'),
    ROW('Please keep this room friendly and follow the Habbo Way.', 'Por favor, mantén esta sala amigable y sigue la Manera Habbo.', 'Por favor, mantenha este quarto amigável e siga a Habbo Etiqueta.'),
    ROW('This room has been reported to the moderators. Please behave.', 'Esta sala ha sido reportada a los moderadores. Por favor, compórtense.', 'Este quarto foi denunciado aos moderadores. Por favor, comportem-se.'),
    ROW('Sexual talk is not allowed in Habbo Hotel.', 'Las conversaciones sexuales no están permitidas en Habbo Hotel.', 'Conversas sexuais não são permitidas no Habbo Hotel.'),
    ROW('Never share personal details such as your address, phone number or passwords.', 'Nunca compartas datos personales como tu dirección, tu teléfono o tus contraseñas.', 'Nunca compartilhe dados pessoais como seu endereço, telefone ou senhas.'),
    ROW('Scamming other Habbos is not allowed.', 'No está permitido estafar a otros Habbos.', 'Não é permitido aplicar golpes em outros Habbos.'),
    ROW('Bullying or harassing other Habbos is not allowed.', 'No está permitido acosar ni intimidar a otros Habbos.', 'Não é permitido praticar bullying ou assediar outros Habbos.'),
    ROW('Disrupting rooms or flooding the chat is not allowed.', 'No está permitido molestar en las salas ni inundar el chat.', 'Não é permitido atrapalhar os quartos nem inundar o chat.')) AS tr (en, es, pt)
  ON p.`category` ->> '$.en' = tr.en
SET p.`category` = JSON_MERGE_PATCH(p.`category`, JSON_OBJECT('es', tr.es, 'pt-br', tr.pt));
UPDATE `definition_moderation_presets` p JOIN (VALUES
    ROW('Sexual content', 'Contenido sexual', 'Conteúdo sexual'),
    ROW('Personal information', 'Información personal', 'Informações pessoais'),
    ROW('Scamming', 'Estafas', 'Golpes'),
    ROW('Bullying', 'Acoso', 'Bullying'),
    ROW('Disruption', 'Molestar', 'Bagunça'),
    ROW('Sexual talk', 'Conversación sexual', 'Conversa sexual'),
    ROW('Sharing personal details', 'Compartir datos personales', 'Compartilhar dados pessoais'),
    ROW('Bullying or harassment', 'Acoso o intimidación', 'Bullying ou assédio'),
    ROW('Disrupting', 'Molestar', 'Atrapalhar'),
    ROW('Please stop what you are doing - it goes against the Habbo Way.', 'Por favor, deja de hacer lo que estás haciendo: va en contra de la Manera Habbo.', 'Por favor, pare com o que está fazendo: isso vai contra a Habbo Etiqueta.'),
    ROW('Please do not share personal information in the hotel, yours or anyone else''s.', 'Por favor, no compartas información personal en el hotel, ni tuya ni de nadie.', 'Por favor, não compartilhe informações pessoais no hotel, nem suas nem de ninguém.'),
    ROW('Please be kind to other Habbos.', 'Por favor, sé amable con los demás Habbos.', 'Por favor, seja gentil com os outros Habbos.'),
    ROW('Please keep this room friendly and follow the Habbo Way.', 'Por favor, mantén esta sala amigable y sigue la Manera Habbo.', 'Por favor, mantenha este quarto amigável e siga a Habbo Etiqueta.'),
    ROW('This room has been reported to the moderators. Please behave.', 'Esta sala ha sido reportada a los moderadores. Por favor, compórtense.', 'Este quarto foi denunciado aos moderadores. Por favor, comportem-se.'),
    ROW('Sexual talk is not allowed in Habbo Hotel.', 'Las conversaciones sexuales no están permitidas en Habbo Hotel.', 'Conversas sexuais não são permitidas no Habbo Hotel.'),
    ROW('Never share personal details such as your address, phone number or passwords.', 'Nunca compartas datos personales como tu dirección, tu teléfono o tus contraseñas.', 'Nunca compartilhe dados pessoais como seu endereço, telefone ou senhas.'),
    ROW('Scamming other Habbos is not allowed.', 'No está permitido estafar a otros Habbos.', 'Não é permitido aplicar golpes em outros Habbos.'),
    ROW('Bullying or harassing other Habbos is not allowed.', 'No está permitido acosar ni intimidar a otros Habbos.', 'Não é permitido praticar bullying ou assediar outros Habbos.'),
    ROW('Disrupting rooms or flooding the chat is not allowed.', 'No está permitido molestar en las salas ni inundar el chat.', 'Não é permitido atrapalhar os quartos nem inundar o chat.')) AS tr (en, es, pt)
  ON p.`name` ->> '$.en' = tr.en
SET p.`name` = JSON_MERGE_PATCH(p.`name`, JSON_OBJECT('es', tr.es, 'pt-br', tr.pt));
UPDATE `definition_moderation_presets` p JOIN (VALUES
    ROW('Sexual content', 'Contenido sexual', 'Conteúdo sexual'),
    ROW('Personal information', 'Información personal', 'Informações pessoais'),
    ROW('Scamming', 'Estafas', 'Golpes'),
    ROW('Bullying', 'Acoso', 'Bullying'),
    ROW('Disruption', 'Molestar', 'Bagunça'),
    ROW('Sexual talk', 'Conversación sexual', 'Conversa sexual'),
    ROW('Sharing personal details', 'Compartir datos personales', 'Compartilhar dados pessoais'),
    ROW('Bullying or harassment', 'Acoso o intimidación', 'Bullying ou assédio'),
    ROW('Disrupting', 'Molestar', 'Atrapalhar'),
    ROW('Please stop what you are doing - it goes against the Habbo Way.', 'Por favor, deja de hacer lo que estás haciendo: va en contra de la Manera Habbo.', 'Por favor, pare com o que está fazendo: isso vai contra a Habbo Etiqueta.'),
    ROW('Please do not share personal information in the hotel, yours or anyone else''s.', 'Por favor, no compartas información personal en el hotel, ni tuya ni de nadie.', 'Por favor, não compartilhe informações pessoais no hotel, nem suas nem de ninguém.'),
    ROW('Please be kind to other Habbos.', 'Por favor, sé amable con los demás Habbos.', 'Por favor, seja gentil com os outros Habbos.'),
    ROW('Please keep this room friendly and follow the Habbo Way.', 'Por favor, mantén esta sala amigable y sigue la Manera Habbo.', 'Por favor, mantenha este quarto amigável e siga a Habbo Etiqueta.'),
    ROW('This room has been reported to the moderators. Please behave.', 'Esta sala ha sido reportada a los moderadores. Por favor, compórtense.', 'Este quarto foi denunciado aos moderadores. Por favor, comportem-se.'),
    ROW('Sexual talk is not allowed in Habbo Hotel.', 'Las conversaciones sexuales no están permitidas en Habbo Hotel.', 'Conversas sexuais não são permitidas no Habbo Hotel.'),
    ROW('Never share personal details such as your address, phone number or passwords.', 'Nunca compartas datos personales como tu dirección, tu teléfono o tus contraseñas.', 'Nunca compartilhe dados pessoais como seu endereço, telefone ou senhas.'),
    ROW('Scamming other Habbos is not allowed.', 'No está permitido estafar a otros Habbos.', 'Não é permitido aplicar golpes em outros Habbos.'),
    ROW('Bullying or harassing other Habbos is not allowed.', 'No está permitido acosar ni intimidar a otros Habbos.', 'Não é permitido praticar bullying ou assediar outros Habbos.'),
    ROW('Disrupting rooms or flooding the chat is not allowed.', 'No está permitido molestar en las salas ni inundar el chat.', 'Não é permitido atrapalhar os quartos nem inundar o chat.')) AS tr (en, es, pt)
  ON p.`message` ->> '$.en' = tr.en
SET p.`message` = JSON_MERGE_PATCH(p.`message`, JSON_OBJECT('es', tr.es, 'pt-br', tr.pt));

UPDATE `definition_help_faq_categories` SET `name` = JSON_OBJECT('en', `name`), `description` = JSON_OBJECT('en', `description`);
ALTER TABLE `definition_help_faq_categories`
  MODIFY `name` json NOT NULL,
  MODIFY `description` json NOT NULL;
UPDATE `definition_help_faq_categories` c JOIN (VALUES
    ROW('How do I get Credits?', '¿Cómo consigo Créditos?', 'Como consigo Moedas?'),
    ROW('Password Reset', 'Cambiar la contraseña', 'Trocar a senha'),
    ROW('Help with Habbo Hotel', 'Ayuda con Habbo Hotel', 'Ajuda com o Habbo Hotel'),
    ROW('The Habbo Way', 'La Manera Habbo', 'A Habbo Etiqueta'),
    ROW('Safety Tips', 'Consejos de seguridad', 'Dicas de segurança'),
    ROW('Contact Us', 'Contáctanos', 'Fale conosco')) AS tr (en, es, pt)
  ON c.`name` ->> '$.en' = tr.en
SET c.`name` = JSON_MERGE_PATCH(c.`name`, JSON_OBJECT('es', tr.es, 'pt-br', tr.pt));
UPDATE `definition_help_faq_entries` SET `question` = JSON_OBJECT('en', `question`), `answer` = JSON_OBJECT('en', `answer`);
ALTER TABLE `definition_help_faq_entries`
  MODIFY `question` json NOT NULL,
  MODIFY `answer` json NOT NULL;

UPDATE `polls` SET `name` = JSON_OBJECT('en', `name`), `thanks_message` = JSON_OBJECT('en', `thanks_message`);
ALTER TABLE `polls`
  MODIFY `name` json NOT NULL,
  MODIFY `thanks_message` json NOT NULL DEFAULT (JSON_OBJECT('en', 'Thank you for answering the poll!', 'es', '¡Gracias por responder la encuesta!', 'pt-br', 'Obrigado por responder à enquete!'));
UPDATE `polls` p JOIN (VALUES
    ROW('Thank you for answering the poll!', '¡Gracias por responder la encuesta!', 'Obrigado por responder à enquete!')) AS tr (en, es, pt)
  ON p.`thanks_message` ->> '$.en' = tr.en
SET p.`thanks_message` = JSON_MERGE_PATCH(p.`thanks_message`, JSON_OBJECT('es', tr.es, 'pt-br', tr.pt));
UPDATE `polls_questions` SET `content` = JSON_OBJECT('en', `content`);
ALTER TABLE `polls_questions`
  MODIFY `content` json NOT NULL;
UPDATE `polls_question_options` SET `content` = JSON_OBJECT('en', `content`);
ALTER TABLE `polls_question_options`
  MODIFY `content` json NOT NULL;

ALTER TABLE `definition_pet_commands` DROP INDEX `uq_definition_pet_commands_name`;
UPDATE `definition_pet_commands` SET `name` = JSON_OBJECT('en', `name`);
ALTER TABLE `definition_pet_commands`
  MODIFY `name` json NOT NULL;
ALTER TABLE `definition_pet_commands` ADD UNIQUE KEY `uq_definition_pet_commands_name` ((CAST(`name` ->> '$.en' AS CHAR(32)) COLLATE utf8mb4_unicode_ci));
UPDATE `definition_pet_commands` SET `name` = JSON_MERGE_PATCH(`name`, JSON_OBJECT('es', 'descansa', 'pt-br', 'livre')) WHERE `id` = 0 AND `name` ->> '$.en' = 'free';
UPDATE `definition_pet_commands` SET `name` = JSON_MERGE_PATCH(`name`, JSON_OBJECT('es', 'siéntate', 'pt-br', 'senta')) WHERE `id` = 1 AND `name` ->> '$.en' = 'sit';
UPDATE `definition_pet_commands` SET `name` = JSON_MERGE_PATCH(`name`, JSON_OBJECT('es', 'túmbate', 'pt-br', 'deita')) WHERE `id` = 2 AND `name` ->> '$.en' = 'down';
UPDATE `definition_pet_commands` SET `name` = JSON_MERGE_PATCH(`name`, JSON_OBJECT('es', 'ven aquí', 'pt-br', 'vem')) WHERE `id` = 3 AND `name` ->> '$.en' = 'here';
UPDATE `definition_pet_commands` SET `name` = JSON_MERGE_PATCH(`name`, JSON_OBJECT('es', 'pide', 'pt-br', 'pede')) WHERE `id` = 4 AND `name` ->> '$.en' = 'beg';
UPDATE `definition_pet_commands` SET `name` = JSON_MERGE_PATCH(`name`, JSON_OBJECT('es', 'haz el muerto', 'pt-br', 'finge de morto')) WHERE `id` = 5 AND `name` ->> '$.en' = 'play dead';
UPDATE `definition_pet_commands` SET `name` = JSON_MERGE_PATCH(`name`, JSON_OBJECT('es', 'quieto', 'pt-br', 'fica')) WHERE `id` = 6 AND `name` ->> '$.en' = 'stay';
UPDATE `definition_pet_commands` SET `name` = JSON_MERGE_PATCH(`name`, JSON_OBJECT('es', 'sígueme', 'pt-br', 'segue')) WHERE `id` = 7 AND `name` ->> '$.en' = 'follow';
UPDATE `definition_pet_commands` SET `name` = JSON_MERGE_PATCH(`name`, JSON_OBJECT('es', 'levanta', 'pt-br', 'parado')) WHERE `id` = 8 AND `name` ->> '$.en' = 'stand';
UPDATE `definition_pet_commands` SET `name` = JSON_MERGE_PATCH(`name`, JSON_OBJECT('es', 'salta', 'pt-br', 'pula')) WHERE `id` = 9 AND `name` ->> '$.en' = 'jump';
UPDATE `definition_pet_commands` SET `name` = JSON_MERGE_PATCH(`name`, JSON_OBJECT('es', 'habla', 'pt-br', 'fala')) WHERE `id` = 10 AND `name` ->> '$.en' = 'speak';
UPDATE `definition_pet_commands` SET `name` = JSON_MERGE_PATCH(`name`, JSON_OBJECT('es', 'juega', 'pt-br', 'brinca')) WHERE `id` = 11 AND `name` ->> '$.en' = 'play';
UPDATE `definition_pet_commands` SET `name` = JSON_MERGE_PATCH(`name`, JSON_OBJECT('es', 'calla', 'pt-br', 'cala')) WHERE `id` = 12 AND `name` ->> '$.en' = 'silent';
UPDATE `definition_pet_commands` SET `name` = JSON_MERGE_PATCH(`name`, JSON_OBJECT('es', 'a casa', 'pt-br', 'descansa')) WHERE `id` = 13 AND `name` ->> '$.en' = 'nest';
UPDATE `definition_pet_commands` SET `name` = JSON_MERGE_PATCH(`name`, JSON_OBJECT('es', 'bebe', 'pt-br', 'bebe')) WHERE `id` = 14 AND `name` ->> '$.en' = 'drink';
UPDATE `definition_pet_commands` SET `name` = JSON_MERGE_PATCH(`name`, JSON_OBJECT('es', 'izquierda', 'pt-br', 'esquerda')) WHERE `id` = 15 AND `name` ->> '$.en' = 'follow left';
UPDATE `definition_pet_commands` SET `name` = JSON_MERGE_PATCH(`name`, JSON_OBJECT('es', 'derecha', 'pt-br', 'direita')) WHERE `id` = 16 AND `name` ->> '$.en' = 'follow right';
UPDATE `definition_pet_speech` SET `text` = JSON_OBJECT('en', `text`);
ALTER TABLE `definition_pet_speech`
  MODIFY `text` json NOT NULL;
UPDATE `definition_pet_speech` s JOIN (VALUES
    ROW('Woof!', '¡Guau!', 'Au!'),
    ROW('*sniffs around*', '*olfatea por todas partes*', '*fareja tudo*'),
    ROW('*wags its tail*', '*mueve la cola*', '*abana o rabo*'),
    ROW('*chases its own tail*', '*persigue su propia cola*', '*corre atrás do próprio rabo*'),
    ROW('Woof woof!', '¡Guau guau!', 'Au au!'),
    ROW('*wags its tail happily*', '*mueve la cola feliz*', '*abana o rabo feliz*'),
    ROW('*looks proud*', '*parece orgulloso*', '*parece orgulhoso*'),
    ROW('*ignores you*', '*te ignora*', '*ignora você*'),
    ROW('*yawns*', '*bosteza*', '*boceja*'),
    ROW('Grrr...', 'Grrr...', 'Grrr...'),
    ROW('*munch munch*', '*ñam ñam*', '*nhac nhac*'),
    ROW('*crunches happily*', '*mastica feliz*', '*mastiga feliz*'),
    ROW('*bounces around*', '*da saltitos*', '*fica pulando*'),
    ROW('*wants to play!*', '*¡quiere jugar!*', '*quer brincar!*'),
    ROW('*laps up some water*', '*bebe un poco de agua*', '*bebe um pouco de água*'),
    ROW('*slurp slurp*', '*slurp slurp*', '*slurp slurp*'),
    ROW('Meow!', '¡Miau!', 'Miau!'),
    ROW('*is hunting a bird*', '*está cazando un pájaro*', '*está caçando um passarinho*'),
    ROW('Purrr...', 'Purrr...', 'Rrrron...'),
    ROW('*licks its paw*', '*se lame la pata*', '*lambe a pata*'),
    ROW('*stretches lazily*', '*se estira con pereza*', '*se espreguiça*'),
    ROW('*purrs*', '*ronronea*', '*ronrona*'),
    ROW('*rubs against your leg*', '*se frota contra tu pierna*', '*se esfrega na sua perna*'),
    ROW('*turns its back on you*', '*te da la espalda*', '*vira as costas para você*'),
    ROW('Hiss!', '¡Fffff!', 'Fffff!'),
    ROW('*pretends not to hear*', '*hace como que no oye*', '*finge que não ouviu*'),
    ROW('*nibbles delicately*', '*mordisquea con delicadeza*', '*mordisca com delicadeza*'),
    ROW('*munch*', '*ñam*', '*nhac*'),
    ROW('*pounces!*', '*¡salta encima!*', '*dá o bote!*'),
    ROW('*chases an invisible mouse*', '*persigue un ratón invisible*', '*persegue um rato invisível*'),
    ROW('*laps the water delicately*', '*bebe el agua con delicadeza*', '*bebe a água com delicadeza*'),
    ROW('*slurp*', '*slurp*', '*slurp*'),
    ROW('*snaps its jaws*', '*chasquea las mandíbulas*', '*bate as mandíbulas*'),
    ROW('*basks in the sun*', '*toma el sol*', '*toma sol*'),
    ROW('Grrrr...', 'Grrrr...', 'Grrrr...'),
    ROW('*looks at you hungrily*', '*te mira con hambre*', '*olha para você com fome*'),
    ROW('*snaps happily*', '*chasquea feliz*', '*bate a boca feliz*'),
    ROW('*wiggles its tail*', '*menea la cola*', '*balança o rabo*'),
    ROW('*growls*', '*gruñe*', '*rosna*'),
    ROW('*shows its teeth*', '*enseña los dientes*', '*mostra os dentes*'),
    ROW('*chomp chomp*', '*ñac ñac*', '*nhoc nhoc*'),
    ROW('*CHOMP*', '*ÑAC*', '*NHOC*'),
    ROW('*rolls around*', '*se revuelca*', '*rola no chão*'),
    ROW('*splashes about*', '*chapotea*', '*chapinha na água*'),
    ROW('*splashes in the water*', '*salpica en el agua*', '*espirra água*'),
    ROW('*gulp*', '*glup*', '*glup*'),
    ROW('Yip yip!', '¡Yip yip!', 'Yip yip!'),
    ROW('*digs a hole*', '*cava un agujero*', '*cava um buraco*'),
    ROW('*sniffs everything*', '*lo olfatea todo*', '*fareja tudo*'),
    ROW('Arf!', '¡Arf!', 'Arf!'),
    ROW('Yip!', '¡Yip!', 'Yip!'),
    ROW('*jumps with joy*', '*salta de alegría*', '*pula de alegria*'),
    ROW('*barks back at you*', '*te ladra de vuelta*', '*late de volta para você*'),
    ROW('*runs in circles*', '*corre en círculos*', '*corre em círculos*'),
    ROW('*gobbles it up*', '*lo engulle*', '*devora tudo*'),
    ROW('*zooms around the room*', '*corre a toda velocidad por la sala*', '*dispara pelo quarto*'),
    ROW('Yip yip yip!', '¡Yip yip yip!', 'Yip yip yip!'),
    ROW('Grrr!', '¡Grrr!', 'Grrr!'),
    ROW('*sniffs for honey*', '*busca miel con el hocico*', '*fareja atrás de mel*'),
    ROW('*scratches its back*', '*se rasca la espalda*', '*coça as costas*'),
    ROW('*yawns loudly*', '*bosteza muy fuerte*', '*boceja bem alto*'),
    ROW('*growls happily*', '*gruñe feliz*', '*rosna feliz*'),
    ROW('*claps its paws*', '*aplaude con las patas*', '*bate as patas*'),
    ROW('*grumbles*', '*refunfuña*', '*resmunga*'),
    ROW('*turns away*', '*se da la vuelta*', '*vira para o outro lado*'),
    ROW('*munches happily*', '*come feliz*', '*come feliz*'),
    ROW('*nom nom*', '*ñam ñam*', '*nham nham*'),
    ROW('*rolls on its back*', '*se pone panza arriba*', '*deita de barriga para cima*'),
    ROW('*wrestles with the air*', '*lucha contra el aire*', '*luta com o ar*'),
    ROW('*slurps loudly*', '*sorbe muy fuerte*', '*faz slurp bem alto*'),
    ROW('*gulp gulp*', '*glup glup*', '*glup glup*'),
    ROW('Oink!', '¡Oink!', 'Oinc!'),
    ROW('*rolls in the mud*', '*se revuelca en el barro*', '*rola na lama*'),
    ROW('*sniffs for truffles*', '*busca trufas con el hocico*', '*fareja atrás de trufas*'),
    ROW('Oink oink!', '¡Oink oink!', 'Oinc oinc!'),
    ROW('*oinks happily*', '*gruñe feliz*', '*grunhe feliz*'),
    ROW('*wiggles its curly tail*', '*menea su colita rizada*', '*balança o rabinho enrolado*'),
    ROW('*snorts*', '*resopla*', '*bufa*'),
    ROW('*snorts happily while eating*', '*resopla feliz mientras come*', '*bufa feliz enquanto come*'),
    ROW('Oink oink oink!', '¡Oink oink oink!', 'Oinc oinc oinc!'),
    ROW('*splashes water everywhere*', '*salpica agua por todas partes*', '*espirra água para todo lado*'),
    ROW('*zzzzz...*', '*zzzzz...*', '*zzzzz...*'),
    ROW('*ZzZZZ...*', '*ZzZZZ...*', '*ZzZZZ...*'),
    ROW('*sleeping like a log... ZzZ*', '*duerme como un tronco... ZzZ*', '*dormindo como uma pedra... ZzZ*'),
    ROW('*Yay! You''re home!*', '*¡Bien! ¡Ya estás en casa!*', '*Oba! Você chegou!*')) AS tr (en, es, pt)
  ON s.`text` ->> '$.en' = tr.en
SET s.`text` = JSON_MERGE_PATCH(s.`text`, JSON_OBJECT('es', tr.es, 'pt-br', tr.pt));

UPDATE `room_bots` SET `name` = JSON_OBJECT('en', `name`), `motto` = JSON_OBJECT('en', `motto`);
ALTER TABLE `room_bots`
  MODIFY `name` json NOT NULL,
  MODIFY `motto` json NOT NULL;
UPDATE `room_bots` b JOIN (VALUES
    ROW('Harry', 'Harry', 'Harry'),
    ROW('Marcel', 'Marcel', 'Marcel'),
    ROW('Guide Bot', 'Bot Guía', 'Robô Guia'),
    ROW('Welcome to Habbo Hotel!', '¡Bienvenido a Habbo Hotel!', 'Bem-vindo ao Habbo Hotel!'),
    ROW('Step into a world of reading!', '¡Entra en un mundo de lectura!', 'Entre num mundo de leitura!'),
    ROW('Here to help for 24 hours!', '¡Aquí para ayudarte durante 24 horas!', 'Aqui para ajudar por 24 horas!'),
    ROW('Please keep it down, people are trying to think!', '¡Bajen la voz, que la gente intenta pensar!', 'Falem mais baixo, tem gente tentando pensar!'),
    ROW('Only use the Call for help in an emergency!', '¡Usa la Llamada de ayuda solo en una emergencia!', 'Só use o Pedido de ajuda numa emergência!'),
    ROW('Want to know more about Habbo Hotel? Ask a Habbo Guide!', '¿Quieres saber más sobre Habbo Hotel? ¡Pregúntale a un Guía Habbo!', 'Quer saber mais sobre o Habbo Hotel? Pergunte a um Guia Habbo!'),
    ROW('Is it me or is something BIG about to happen?', '¿Soy yo o está a punto de pasar algo GRANDE?', 'É impressão minha ou algo GRANDE está para acontecer?'),
    ROW('In Trouble? Call for Moderator assistance using the Yellow Question Mark!', '¿En apuros? ¡Pide ayuda a un Moderador con el Signo de Interrogación Amarillo!', 'Com problemas? Chame um Moderador usando o Ponto de Interrogação Amarelo!'),
    ROW('There''s no such thing as a free lunch or free credits!', '¡No existen ni la comida gratis ni los créditos gratis!', 'Não existe almoço grátis, nem moedas grátis!'),
    ROW('Why Hello there! *Shakes Habbo Hand* My name''s Harry.', '¡Vaya, hola! *Estrecha la mano Habbo* Me llamo Harry.', 'Ora, olá! *Aperta a mão Habbo* Meu nome é Harry.'),
    ROW('Hello, Hello, Hello!', '¡Hola, hola, hola!', 'Olá, olá, olá!'),
    ROW('Hello and welcome to Habbo Hotel! Enjoy your stay! :)', '¡Hola y bienvenido a Habbo Hotel! ¡Disfruta tu estancia! :)', 'Olá e bem-vindo ao Habbo Hotel! Aproveite a estadia! :)'),
    ROW('Better to remain silent and be thought a fool than to speak and remove all doubt.', 'Mejor callar y parecer tonto que hablar y despejar todas las dudas.', 'Melhor ficar calado e parecer tolo do que falar e tirar todas as dúvidas.'),
    ROW('In this room, we keep quiet. Learning takes diligence.', 'En esta sala guardamos silencio. Aprender requiere dedicación.', 'Neste quarto, ficamos em silêncio. Aprender exige dedicação.'),
    ROW('Can I be helping any Habbo?', '¿Puedo ayudar a algún Habbo?', 'Posso ajudar algum Habbo?'),
    ROW('English is the tongue of Shakespeare', 'El español es la lengua de Cervantes', 'O português é a língua de Camões'),
    ROW('Not to be absolutely certain is, I think, one of the essential things in rationality.', 'No estar absolutamente seguro es, creo, una de las cosas esenciales de la racionalidad.', 'Não ter certeza absoluta é, eu acho, uma das coisas essenciais da racionalidade.'),
    ROW('If one does understand a person, one tends to regard him as a fool.', 'Si uno entiende a una persona, tiende a considerarla tonta.', 'Quando se entende uma pessoa, a gente tende a achá-la tola.'),
    ROW('I miss fresh bread daily', 'Echo de menos el pan recién hecho de cada día', 'Sinto falta de pão fresquinho todo dia'),
    ROW('With an onion and a tomato, every meal is a feast', 'Con una cebolla y un tomate, cada comida es un banquete', 'Com uma cebola e um tomate, toda refeição é um banquete'),
    ROW('Ah, so you want a book.', 'Ah, así que quieres un libro.', 'Ah, então você quer um livro.'),
    ROW('Ah, I see you''re a total bookworm.', 'Ah, ya veo que eres un ratón de biblioteca.', 'Ah, vejo que você é um rato de biblioteca.'),
    ROW('Have those books back by the weekend!', '¡Devuelve esos libros antes del fin de semana!', 'Devolva esses livros até o fim de semana!'),
    ROW('Welcome, and step into literary paradise.', 'Bienvenido, adelante, pasa al paraíso literario.', 'Bem-vindo, entre no paraíso literário.'),
    ROW('Hi buddy, all good?', 'Hola, amigo, ¿todo bien?', 'Oi, amigão, tudo certo?'),
    ROW('Greetings.', 'Saludos.', 'Saudações.'),
    ROW('Is that so?', '¿Ah, sí?', 'É mesmo?'),
    ROW('Whaddayamean?', '¿Qué quieres decir?', 'Como assim?'),
    ROW('Easy you ruffians. I got only two hands and I work full-time.', 'Calma, gamberros. Solo tengo dos manos y trabajo a tiempo completo.', 'Calma aí, baderneiros. Só tenho duas mãos e trabalho em tempo integral.'),
    ROW('Yes, what would you like?', 'Sí, ¿qué te gustaría?', 'Sim, o que você gostaria?'),
    ROW('Turn your Bobba filter on and bad words are gone!', '¡Activa tu filtro Bobba y las palabrotas desaparecerán!', 'Ligue seu filtro Bobba e os palavrões somem!'),
    ROW('Be careful', 'Ten cuidado', 'Tome cuidado'),
    ROW('Looking for people to hang out with?', '¿Buscas gente con quien pasar el rato?', 'Procurando gente para ficar de bobeira?'),
    ROW('Open the Navigator and see where everyone else is.', 'Abre el Navegador y mira dónde está todo el mundo.', 'Abra o Navegador e veja onde está todo mundo.'),
    ROW('To move, click the floor on the spot you want to go to.', 'Para moverte, haz clic en el suelo donde quieras ir.', 'Para andar, clique no chão onde você quer ir.'),
    ROW('AI can''t feel love! But you might find a real virtual connection in a dating room.', '¡La IA no puede sentir amor! Pero quizá encuentres una conexión virtual de verdad en una sala de citas.', 'IA não sente amor! Mas talvez você encontre uma conexão virtual de verdade num quarto de paquera.'),
    ROW('You can call for help if someone is giving you a hard time.', 'Puedes pedir ayuda si alguien te está haciendo pasar un mal rato.', 'Você pode pedir ajuda se alguém estiver te incomodando.'),
    ROW('Just click on the yellow question mark icon.', 'Solo haz clic en el icono del signo de interrogación amarillo.', 'É só clicar no ícone do ponto de interrogação amarelo.'),
    ROW('If you feel like waving, click the face icon and click wave.', 'Si tienes ganas de saludar, haz clic en el icono de la cara y luego en saludar.', 'Se quiser acenar, clique no ícone do rosto e depois em acenar.'),
    ROW('Buy your furry friend from the catalogue.', 'Compra tu amigo peludo en el catálogo.', 'Compre seu amigo peludo no catálogo.'),
    ROW('He''ll need food and toys to be happy and content and remember, a pet is for life, you cannot give it away!', 'Necesitará comida y juguetes para estar feliz y contento. Y recuerda: una mascota es para toda la vida, ¡no puedes regalarla!', 'Ele vai precisar de comida e brinquedos para ficar feliz e contente. E lembre-se: um mascote é para a vida toda, você não pode dar ele para ninguém!'),
    ROW('Participate in some games like falling furni.', 'Participa en juegos como el de los furnis que caen.', 'Participe de jogos como o dos mobis que caem.'),
    ROW('The trick is to always stand one square away from other users.', 'El truco es quedarse siempre a una casilla de los demás usuarios.', 'O truque é ficar sempre a um quadrado dos outros usuários.'),
    ROW('When the owner drops a chair to the room, try to sit on it.', 'Cuando el dueño suelte una silla en la sala, intenta sentarte en ella.', 'Quando o dono soltar uma cadeira no quarto, tente sentar nela.'),
    ROW('The one left out without a chair is losing. The one sitting in the chair at last is the winner.', 'Quien se queda sin silla pierde. Quien queda sentado en la última silla gana.', 'Quem fica sem cadeira perde. Quem fica sentado na última cadeira ganha.'),
    ROW('Falling Furni games are marked as FF or Falling Furni.', 'Los juegos de furnis que caen se marcan como FF o Falling Furni.', 'Os jogos de mobis que caem são marcados como FF ou Falling Furni.'),
    ROW('Hiya!', '¡Holi!', 'Oiê!'),
    ROW('You can kick other users from your room if they are getting too bossy or give a hard time.', 'Puedes echar a otros usuarios de tu sala si se ponen muy mandones o te hacen pasar un mal rato.', 'Você pode expulsar outros usuários do seu quarto se eles ficarem mandões demais ou te incomodarem.'),
    ROW('Shouting is not recommended. Make sure you stand quite close to the people you want to chat with.', 'No se recomienda gritar. Asegúrate de estar bastante cerca de la gente con la que quieres hablar.', 'Gritar não é recomendado. Fique bem perto das pessoas com quem você quer conversar.')) AS tr (en, es, pt)
  ON b.`name` ->> '$.en' = tr.en
SET b.`name` = JSON_MERGE_PATCH(b.`name`, JSON_OBJECT('es', tr.es, 'pt-br', tr.pt));
UPDATE `room_bots` b JOIN (VALUES
    ROW('Harry', 'Harry', 'Harry'),
    ROW('Marcel', 'Marcel', 'Marcel'),
    ROW('Guide Bot', 'Bot Guía', 'Robô Guia'),
    ROW('Welcome to Habbo Hotel!', '¡Bienvenido a Habbo Hotel!', 'Bem-vindo ao Habbo Hotel!'),
    ROW('Step into a world of reading!', '¡Entra en un mundo de lectura!', 'Entre num mundo de leitura!'),
    ROW('Here to help for 24 hours!', '¡Aquí para ayudarte durante 24 horas!', 'Aqui para ajudar por 24 horas!'),
    ROW('Please keep it down, people are trying to think!', '¡Bajen la voz, que la gente intenta pensar!', 'Falem mais baixo, tem gente tentando pensar!'),
    ROW('Only use the Call for help in an emergency!', '¡Usa la Llamada de ayuda solo en una emergencia!', 'Só use o Pedido de ajuda numa emergência!'),
    ROW('Want to know more about Habbo Hotel? Ask a Habbo Guide!', '¿Quieres saber más sobre Habbo Hotel? ¡Pregúntale a un Guía Habbo!', 'Quer saber mais sobre o Habbo Hotel? Pergunte a um Guia Habbo!'),
    ROW('Is it me or is something BIG about to happen?', '¿Soy yo o está a punto de pasar algo GRANDE?', 'É impressão minha ou algo GRANDE está para acontecer?'),
    ROW('In Trouble? Call for Moderator assistance using the Yellow Question Mark!', '¿En apuros? ¡Pide ayuda a un Moderador con el Signo de Interrogación Amarillo!', 'Com problemas? Chame um Moderador usando o Ponto de Interrogação Amarelo!'),
    ROW('There''s no such thing as a free lunch or free credits!', '¡No existen ni la comida gratis ni los créditos gratis!', 'Não existe almoço grátis, nem moedas grátis!'),
    ROW('Why Hello there! *Shakes Habbo Hand* My name''s Harry.', '¡Vaya, hola! *Estrecha la mano Habbo* Me llamo Harry.', 'Ora, olá! *Aperta a mão Habbo* Meu nome é Harry.'),
    ROW('Hello, Hello, Hello!', '¡Hola, hola, hola!', 'Olá, olá, olá!'),
    ROW('Hello and welcome to Habbo Hotel! Enjoy your stay! :)', '¡Hola y bienvenido a Habbo Hotel! ¡Disfruta tu estancia! :)', 'Olá e bem-vindo ao Habbo Hotel! Aproveite a estadia! :)'),
    ROW('Better to remain silent and be thought a fool than to speak and remove all doubt.', 'Mejor callar y parecer tonto que hablar y despejar todas las dudas.', 'Melhor ficar calado e parecer tolo do que falar e tirar todas as dúvidas.'),
    ROW('In this room, we keep quiet. Learning takes diligence.', 'En esta sala guardamos silencio. Aprender requiere dedicación.', 'Neste quarto, ficamos em silêncio. Aprender exige dedicação.'),
    ROW('Can I be helping any Habbo?', '¿Puedo ayudar a algún Habbo?', 'Posso ajudar algum Habbo?'),
    ROW('English is the tongue of Shakespeare', 'El español es la lengua de Cervantes', 'O português é a língua de Camões'),
    ROW('Not to be absolutely certain is, I think, one of the essential things in rationality.', 'No estar absolutamente seguro es, creo, una de las cosas esenciales de la racionalidad.', 'Não ter certeza absoluta é, eu acho, uma das coisas essenciais da racionalidade.'),
    ROW('If one does understand a person, one tends to regard him as a fool.', 'Si uno entiende a una persona, tiende a considerarla tonta.', 'Quando se entende uma pessoa, a gente tende a achá-la tola.'),
    ROW('I miss fresh bread daily', 'Echo de menos el pan recién hecho de cada día', 'Sinto falta de pão fresquinho todo dia'),
    ROW('With an onion and a tomato, every meal is a feast', 'Con una cebolla y un tomate, cada comida es un banquete', 'Com uma cebola e um tomate, toda refeição é um banquete'),
    ROW('Ah, so you want a book.', 'Ah, así que quieres un libro.', 'Ah, então você quer um livro.'),
    ROW('Ah, I see you''re a total bookworm.', 'Ah, ya veo que eres un ratón de biblioteca.', 'Ah, vejo que você é um rato de biblioteca.'),
    ROW('Have those books back by the weekend!', '¡Devuelve esos libros antes del fin de semana!', 'Devolva esses livros até o fim de semana!'),
    ROW('Welcome, and step into literary paradise.', 'Bienvenido, adelante, pasa al paraíso literario.', 'Bem-vindo, entre no paraíso literário.'),
    ROW('Hi buddy, all good?', 'Hola, amigo, ¿todo bien?', 'Oi, amigão, tudo certo?'),
    ROW('Greetings.', 'Saludos.', 'Saudações.'),
    ROW('Is that so?', '¿Ah, sí?', 'É mesmo?'),
    ROW('Whaddayamean?', '¿Qué quieres decir?', 'Como assim?'),
    ROW('Easy you ruffians. I got only two hands and I work full-time.', 'Calma, gamberros. Solo tengo dos manos y trabajo a tiempo completo.', 'Calma aí, baderneiros. Só tenho duas mãos e trabalho em tempo integral.'),
    ROW('Yes, what would you like?', 'Sí, ¿qué te gustaría?', 'Sim, o que você gostaria?'),
    ROW('Turn your Bobba filter on and bad words are gone!', '¡Activa tu filtro Bobba y las palabrotas desaparecerán!', 'Ligue seu filtro Bobba e os palavrões somem!'),
    ROW('Be careful', 'Ten cuidado', 'Tome cuidado'),
    ROW('Looking for people to hang out with?', '¿Buscas gente con quien pasar el rato?', 'Procurando gente para ficar de bobeira?'),
    ROW('Open the Navigator and see where everyone else is.', 'Abre el Navegador y mira dónde está todo el mundo.', 'Abra o Navegador e veja onde está todo mundo.'),
    ROW('To move, click the floor on the spot you want to go to.', 'Para moverte, haz clic en el suelo donde quieras ir.', 'Para andar, clique no chão onde você quer ir.'),
    ROW('AI can''t feel love! But you might find a real virtual connection in a dating room.', '¡La IA no puede sentir amor! Pero quizá encuentres una conexión virtual de verdad en una sala de citas.', 'IA não sente amor! Mas talvez você encontre uma conexão virtual de verdade num quarto de paquera.'),
    ROW('You can call for help if someone is giving you a hard time.', 'Puedes pedir ayuda si alguien te está haciendo pasar un mal rato.', 'Você pode pedir ajuda se alguém estiver te incomodando.'),
    ROW('Just click on the yellow question mark icon.', 'Solo haz clic en el icono del signo de interrogación amarillo.', 'É só clicar no ícone do ponto de interrogação amarelo.'),
    ROW('If you feel like waving, click the face icon and click wave.', 'Si tienes ganas de saludar, haz clic en el icono de la cara y luego en saludar.', 'Se quiser acenar, clique no ícone do rosto e depois em acenar.'),
    ROW('Buy your furry friend from the catalogue.', 'Compra tu amigo peludo en el catálogo.', 'Compre seu amigo peludo no catálogo.'),
    ROW('He''ll need food and toys to be happy and content and remember, a pet is for life, you cannot give it away!', 'Necesitará comida y juguetes para estar feliz y contento. Y recuerda: una mascota es para toda la vida, ¡no puedes regalarla!', 'Ele vai precisar de comida e brinquedos para ficar feliz e contente. E lembre-se: um mascote é para a vida toda, você não pode dar ele para ninguém!'),
    ROW('Participate in some games like falling furni.', 'Participa en juegos como el de los furnis que caen.', 'Participe de jogos como o dos mobis que caem.'),
    ROW('The trick is to always stand one square away from other users.', 'El truco es quedarse siempre a una casilla de los demás usuarios.', 'O truque é ficar sempre a um quadrado dos outros usuários.'),
    ROW('When the owner drops a chair to the room, try to sit on it.', 'Cuando el dueño suelte una silla en la sala, intenta sentarte en ella.', 'Quando o dono soltar uma cadeira no quarto, tente sentar nela.'),
    ROW('The one left out without a chair is losing. The one sitting in the chair at last is the winner.', 'Quien se queda sin silla pierde. Quien queda sentado en la última silla gana.', 'Quem fica sem cadeira perde. Quem fica sentado na última cadeira ganha.'),
    ROW('Falling Furni games are marked as FF or Falling Furni.', 'Los juegos de furnis que caen se marcan como FF o Falling Furni.', 'Os jogos de mobis que caem são marcados como FF ou Falling Furni.'),
    ROW('Hiya!', '¡Holi!', 'Oiê!'),
    ROW('You can kick other users from your room if they are getting too bossy or give a hard time.', 'Puedes echar a otros usuarios de tu sala si se ponen muy mandones o te hacen pasar un mal rato.', 'Você pode expulsar outros usuários do seu quarto se eles ficarem mandões demais ou te incomodarem.'),
    ROW('Shouting is not recommended. Make sure you stand quite close to the people you want to chat with.', 'No se recomienda gritar. Asegúrate de estar bastante cerca de la gente con la que quieres hablar.', 'Gritar não é recomendado. Fique bem perto das pessoas com quem você quer conversar.')) AS tr (en, es, pt)
  ON b.`motto` ->> '$.en' = tr.en
SET b.`motto` = JSON_MERGE_PATCH(b.`motto`, JSON_OBJECT('es', tr.es, 'pt-br', tr.pt));
UPDATE `guide_bot_data` SET `bot_name` = JSON_OBJECT('en', `bot_name`), `bot_motto` = JSON_OBJECT('en', `bot_motto`);
ALTER TABLE `guide_bot_data`
  MODIFY `bot_name` json NOT NULL DEFAULT (JSON_OBJECT('en', 'Guide Bot', 'es', 'Bot Guía', 'pt-br', 'Robô Guia')),
  MODIFY `bot_motto` json NOT NULL DEFAULT (JSON_OBJECT('en', 'Here to help for 24 hours!', 'es', '¡Aquí para ayudarte durante 24 horas!', 'pt-br', 'Aqui para ajudar por 24 horas!'));
UPDATE `guide_bot_data` g JOIN (VALUES
    ROW('Harry', 'Harry', 'Harry'),
    ROW('Marcel', 'Marcel', 'Marcel'),
    ROW('Guide Bot', 'Bot Guía', 'Robô Guia'),
    ROW('Welcome to Habbo Hotel!', '¡Bienvenido a Habbo Hotel!', 'Bem-vindo ao Habbo Hotel!'),
    ROW('Step into a world of reading!', '¡Entra en un mundo de lectura!', 'Entre num mundo de leitura!'),
    ROW('Here to help for 24 hours!', '¡Aquí para ayudarte durante 24 horas!', 'Aqui para ajudar por 24 horas!'),
    ROW('Please keep it down, people are trying to think!', '¡Bajen la voz, que la gente intenta pensar!', 'Falem mais baixo, tem gente tentando pensar!'),
    ROW('Only use the Call for help in an emergency!', '¡Usa la Llamada de ayuda solo en una emergencia!', 'Só use o Pedido de ajuda numa emergência!'),
    ROW('Want to know more about Habbo Hotel? Ask a Habbo Guide!', '¿Quieres saber más sobre Habbo Hotel? ¡Pregúntale a un Guía Habbo!', 'Quer saber mais sobre o Habbo Hotel? Pergunte a um Guia Habbo!'),
    ROW('Is it me or is something BIG about to happen?', '¿Soy yo o está a punto de pasar algo GRANDE?', 'É impressão minha ou algo GRANDE está para acontecer?'),
    ROW('In Trouble? Call for Moderator assistance using the Yellow Question Mark!', '¿En apuros? ¡Pide ayuda a un Moderador con el Signo de Interrogación Amarillo!', 'Com problemas? Chame um Moderador usando o Ponto de Interrogação Amarelo!'),
    ROW('There''s no such thing as a free lunch or free credits!', '¡No existen ni la comida gratis ni los créditos gratis!', 'Não existe almoço grátis, nem moedas grátis!'),
    ROW('Why Hello there! *Shakes Habbo Hand* My name''s Harry.', '¡Vaya, hola! *Estrecha la mano Habbo* Me llamo Harry.', 'Ora, olá! *Aperta a mão Habbo* Meu nome é Harry.'),
    ROW('Hello, Hello, Hello!', '¡Hola, hola, hola!', 'Olá, olá, olá!'),
    ROW('Hello and welcome to Habbo Hotel! Enjoy your stay! :)', '¡Hola y bienvenido a Habbo Hotel! ¡Disfruta tu estancia! :)', 'Olá e bem-vindo ao Habbo Hotel! Aproveite a estadia! :)'),
    ROW('Better to remain silent and be thought a fool than to speak and remove all doubt.', 'Mejor callar y parecer tonto que hablar y despejar todas las dudas.', 'Melhor ficar calado e parecer tolo do que falar e tirar todas as dúvidas.'),
    ROW('In this room, we keep quiet. Learning takes diligence.', 'En esta sala guardamos silencio. Aprender requiere dedicación.', 'Neste quarto, ficamos em silêncio. Aprender exige dedicação.'),
    ROW('Can I be helping any Habbo?', '¿Puedo ayudar a algún Habbo?', 'Posso ajudar algum Habbo?'),
    ROW('English is the tongue of Shakespeare', 'El español es la lengua de Cervantes', 'O português é a língua de Camões'),
    ROW('Not to be absolutely certain is, I think, one of the essential things in rationality.', 'No estar absolutamente seguro es, creo, una de las cosas esenciales de la racionalidad.', 'Não ter certeza absoluta é, eu acho, uma das coisas essenciais da racionalidade.'),
    ROW('If one does understand a person, one tends to regard him as a fool.', 'Si uno entiende a una persona, tiende a considerarla tonta.', 'Quando se entende uma pessoa, a gente tende a achá-la tola.'),
    ROW('I miss fresh bread daily', 'Echo de menos el pan recién hecho de cada día', 'Sinto falta de pão fresquinho todo dia'),
    ROW('With an onion and a tomato, every meal is a feast', 'Con una cebolla y un tomate, cada comida es un banquete', 'Com uma cebola e um tomate, toda refeição é um banquete'),
    ROW('Ah, so you want a book.', 'Ah, así que quieres un libro.', 'Ah, então você quer um livro.'),
    ROW('Ah, I see you''re a total bookworm.', 'Ah, ya veo que eres un ratón de biblioteca.', 'Ah, vejo que você é um rato de biblioteca.'),
    ROW('Have those books back by the weekend!', '¡Devuelve esos libros antes del fin de semana!', 'Devolva esses livros até o fim de semana!'),
    ROW('Welcome, and step into literary paradise.', 'Bienvenido, adelante, pasa al paraíso literario.', 'Bem-vindo, entre no paraíso literário.'),
    ROW('Hi buddy, all good?', 'Hola, amigo, ¿todo bien?', 'Oi, amigão, tudo certo?'),
    ROW('Greetings.', 'Saludos.', 'Saudações.'),
    ROW('Is that so?', '¿Ah, sí?', 'É mesmo?'),
    ROW('Whaddayamean?', '¿Qué quieres decir?', 'Como assim?'),
    ROW('Easy you ruffians. I got only two hands and I work full-time.', 'Calma, gamberros. Solo tengo dos manos y trabajo a tiempo completo.', 'Calma aí, baderneiros. Só tenho duas mãos e trabalho em tempo integral.'),
    ROW('Yes, what would you like?', 'Sí, ¿qué te gustaría?', 'Sim, o que você gostaria?'),
    ROW('Turn your Bobba filter on and bad words are gone!', '¡Activa tu filtro Bobba y las palabrotas desaparecerán!', 'Ligue seu filtro Bobba e os palavrões somem!'),
    ROW('Be careful', 'Ten cuidado', 'Tome cuidado'),
    ROW('Looking for people to hang out with?', '¿Buscas gente con quien pasar el rato?', 'Procurando gente para ficar de bobeira?'),
    ROW('Open the Navigator and see where everyone else is.', 'Abre el Navegador y mira dónde está todo el mundo.', 'Abra o Navegador e veja onde está todo mundo.'),
    ROW('To move, click the floor on the spot you want to go to.', 'Para moverte, haz clic en el suelo donde quieras ir.', 'Para andar, clique no chão onde você quer ir.'),
    ROW('AI can''t feel love! But you might find a real virtual connection in a dating room.', '¡La IA no puede sentir amor! Pero quizá encuentres una conexión virtual de verdad en una sala de citas.', 'IA não sente amor! Mas talvez você encontre uma conexão virtual de verdade num quarto de paquera.'),
    ROW('You can call for help if someone is giving you a hard time.', 'Puedes pedir ayuda si alguien te está haciendo pasar un mal rato.', 'Você pode pedir ajuda se alguém estiver te incomodando.'),
    ROW('Just click on the yellow question mark icon.', 'Solo haz clic en el icono del signo de interrogación amarillo.', 'É só clicar no ícone do ponto de interrogação amarelo.'),
    ROW('If you feel like waving, click the face icon and click wave.', 'Si tienes ganas de saludar, haz clic en el icono de la cara y luego en saludar.', 'Se quiser acenar, clique no ícone do rosto e depois em acenar.'),
    ROW('Buy your furry friend from the catalogue.', 'Compra tu amigo peludo en el catálogo.', 'Compre seu amigo peludo no catálogo.'),
    ROW('He''ll need food and toys to be happy and content and remember, a pet is for life, you cannot give it away!', 'Necesitará comida y juguetes para estar feliz y contento. Y recuerda: una mascota es para toda la vida, ¡no puedes regalarla!', 'Ele vai precisar de comida e brinquedos para ficar feliz e contente. E lembre-se: um mascote é para a vida toda, você não pode dar ele para ninguém!'),
    ROW('Participate in some games like falling furni.', 'Participa en juegos como el de los furnis que caen.', 'Participe de jogos como o dos mobis que caem.'),
    ROW('The trick is to always stand one square away from other users.', 'El truco es quedarse siempre a una casilla de los demás usuarios.', 'O truque é ficar sempre a um quadrado dos outros usuários.'),
    ROW('When the owner drops a chair to the room, try to sit on it.', 'Cuando el dueño suelte una silla en la sala, intenta sentarte en ella.', 'Quando o dono soltar uma cadeira no quarto, tente sentar nela.'),
    ROW('The one left out without a chair is losing. The one sitting in the chair at last is the winner.', 'Quien se queda sin silla pierde. Quien queda sentado en la última silla gana.', 'Quem fica sem cadeira perde. Quem fica sentado na última cadeira ganha.'),
    ROW('Falling Furni games are marked as FF or Falling Furni.', 'Los juegos de furnis que caen se marcan como FF o Falling Furni.', 'Os jogos de mobis que caem são marcados como FF ou Falling Furni.'),
    ROW('Hiya!', '¡Holi!', 'Oiê!'),
    ROW('You can kick other users from your room if they are getting too bossy or give a hard time.', 'Puedes echar a otros usuarios de tu sala si se ponen muy mandones o te hacen pasar un mal rato.', 'Você pode expulsar outros usuários do seu quarto se eles ficarem mandões demais ou te incomodarem.'),
    ROW('Shouting is not recommended. Make sure you stand quite close to the people you want to chat with.', 'No se recomienda gritar. Asegúrate de estar bastante cerca de la gente con la que quieres hablar.', 'Gritar não é recomendado. Fique bem perto das pessoas com quem você quer conversar.')) AS tr (en, es, pt)
  ON g.`bot_name` ->> '$.en' = tr.en
SET g.`bot_name` = JSON_MERGE_PATCH(g.`bot_name`, JSON_OBJECT('es', tr.es, 'pt-br', tr.pt));
UPDATE `guide_bot_data` g JOIN (VALUES
    ROW('Harry', 'Harry', 'Harry'),
    ROW('Marcel', 'Marcel', 'Marcel'),
    ROW('Guide Bot', 'Bot Guía', 'Robô Guia'),
    ROW('Welcome to Habbo Hotel!', '¡Bienvenido a Habbo Hotel!', 'Bem-vindo ao Habbo Hotel!'),
    ROW('Step into a world of reading!', '¡Entra en un mundo de lectura!', 'Entre num mundo de leitura!'),
    ROW('Here to help for 24 hours!', '¡Aquí para ayudarte durante 24 horas!', 'Aqui para ajudar por 24 horas!'),
    ROW('Please keep it down, people are trying to think!', '¡Bajen la voz, que la gente intenta pensar!', 'Falem mais baixo, tem gente tentando pensar!'),
    ROW('Only use the Call for help in an emergency!', '¡Usa la Llamada de ayuda solo en una emergencia!', 'Só use o Pedido de ajuda numa emergência!'),
    ROW('Want to know more about Habbo Hotel? Ask a Habbo Guide!', '¿Quieres saber más sobre Habbo Hotel? ¡Pregúntale a un Guía Habbo!', 'Quer saber mais sobre o Habbo Hotel? Pergunte a um Guia Habbo!'),
    ROW('Is it me or is something BIG about to happen?', '¿Soy yo o está a punto de pasar algo GRANDE?', 'É impressão minha ou algo GRANDE está para acontecer?'),
    ROW('In Trouble? Call for Moderator assistance using the Yellow Question Mark!', '¿En apuros? ¡Pide ayuda a un Moderador con el Signo de Interrogación Amarillo!', 'Com problemas? Chame um Moderador usando o Ponto de Interrogação Amarelo!'),
    ROW('There''s no such thing as a free lunch or free credits!', '¡No existen ni la comida gratis ni los créditos gratis!', 'Não existe almoço grátis, nem moedas grátis!'),
    ROW('Why Hello there! *Shakes Habbo Hand* My name''s Harry.', '¡Vaya, hola! *Estrecha la mano Habbo* Me llamo Harry.', 'Ora, olá! *Aperta a mão Habbo* Meu nome é Harry.'),
    ROW('Hello, Hello, Hello!', '¡Hola, hola, hola!', 'Olá, olá, olá!'),
    ROW('Hello and welcome to Habbo Hotel! Enjoy your stay! :)', '¡Hola y bienvenido a Habbo Hotel! ¡Disfruta tu estancia! :)', 'Olá e bem-vindo ao Habbo Hotel! Aproveite a estadia! :)'),
    ROW('Better to remain silent and be thought a fool than to speak and remove all doubt.', 'Mejor callar y parecer tonto que hablar y despejar todas las dudas.', 'Melhor ficar calado e parecer tolo do que falar e tirar todas as dúvidas.'),
    ROW('In this room, we keep quiet. Learning takes diligence.', 'En esta sala guardamos silencio. Aprender requiere dedicación.', 'Neste quarto, ficamos em silêncio. Aprender exige dedicação.'),
    ROW('Can I be helping any Habbo?', '¿Puedo ayudar a algún Habbo?', 'Posso ajudar algum Habbo?'),
    ROW('English is the tongue of Shakespeare', 'El español es la lengua de Cervantes', 'O português é a língua de Camões'),
    ROW('Not to be absolutely certain is, I think, one of the essential things in rationality.', 'No estar absolutamente seguro es, creo, una de las cosas esenciales de la racionalidad.', 'Não ter certeza absoluta é, eu acho, uma das coisas essenciais da racionalidade.'),
    ROW('If one does understand a person, one tends to regard him as a fool.', 'Si uno entiende a una persona, tiende a considerarla tonta.', 'Quando se entende uma pessoa, a gente tende a achá-la tola.'),
    ROW('I miss fresh bread daily', 'Echo de menos el pan recién hecho de cada día', 'Sinto falta de pão fresquinho todo dia'),
    ROW('With an onion and a tomato, every meal is a feast', 'Con una cebolla y un tomate, cada comida es un banquete', 'Com uma cebola e um tomate, toda refeição é um banquete'),
    ROW('Ah, so you want a book.', 'Ah, así que quieres un libro.', 'Ah, então você quer um livro.'),
    ROW('Ah, I see you''re a total bookworm.', 'Ah, ya veo que eres un ratón de biblioteca.', 'Ah, vejo que você é um rato de biblioteca.'),
    ROW('Have those books back by the weekend!', '¡Devuelve esos libros antes del fin de semana!', 'Devolva esses livros até o fim de semana!'),
    ROW('Welcome, and step into literary paradise.', 'Bienvenido, adelante, pasa al paraíso literario.', 'Bem-vindo, entre no paraíso literário.'),
    ROW('Hi buddy, all good?', 'Hola, amigo, ¿todo bien?', 'Oi, amigão, tudo certo?'),
    ROW('Greetings.', 'Saludos.', 'Saudações.'),
    ROW('Is that so?', '¿Ah, sí?', 'É mesmo?'),
    ROW('Whaddayamean?', '¿Qué quieres decir?', 'Como assim?'),
    ROW('Easy you ruffians. I got only two hands and I work full-time.', 'Calma, gamberros. Solo tengo dos manos y trabajo a tiempo completo.', 'Calma aí, baderneiros. Só tenho duas mãos e trabalho em tempo integral.'),
    ROW('Yes, what would you like?', 'Sí, ¿qué te gustaría?', 'Sim, o que você gostaria?'),
    ROW('Turn your Bobba filter on and bad words are gone!', '¡Activa tu filtro Bobba y las palabrotas desaparecerán!', 'Ligue seu filtro Bobba e os palavrões somem!'),
    ROW('Be careful', 'Ten cuidado', 'Tome cuidado'),
    ROW('Looking for people to hang out with?', '¿Buscas gente con quien pasar el rato?', 'Procurando gente para ficar de bobeira?'),
    ROW('Open the Navigator and see where everyone else is.', 'Abre el Navegador y mira dónde está todo el mundo.', 'Abra o Navegador e veja onde está todo mundo.'),
    ROW('To move, click the floor on the spot you want to go to.', 'Para moverte, haz clic en el suelo donde quieras ir.', 'Para andar, clique no chão onde você quer ir.'),
    ROW('AI can''t feel love! But you might find a real virtual connection in a dating room.', '¡La IA no puede sentir amor! Pero quizá encuentres una conexión virtual de verdad en una sala de citas.', 'IA não sente amor! Mas talvez você encontre uma conexão virtual de verdade num quarto de paquera.'),
    ROW('You can call for help if someone is giving you a hard time.', 'Puedes pedir ayuda si alguien te está haciendo pasar un mal rato.', 'Você pode pedir ajuda se alguém estiver te incomodando.'),
    ROW('Just click on the yellow question mark icon.', 'Solo haz clic en el icono del signo de interrogación amarillo.', 'É só clicar no ícone do ponto de interrogação amarelo.'),
    ROW('If you feel like waving, click the face icon and click wave.', 'Si tienes ganas de saludar, haz clic en el icono de la cara y luego en saludar.', 'Se quiser acenar, clique no ícone do rosto e depois em acenar.'),
    ROW('Buy your furry friend from the catalogue.', 'Compra tu amigo peludo en el catálogo.', 'Compre seu amigo peludo no catálogo.'),
    ROW('He''ll need food and toys to be happy and content and remember, a pet is for life, you cannot give it away!', 'Necesitará comida y juguetes para estar feliz y contento. Y recuerda: una mascota es para toda la vida, ¡no puedes regalarla!', 'Ele vai precisar de comida e brinquedos para ficar feliz e contente. E lembre-se: um mascote é para a vida toda, você não pode dar ele para ninguém!'),
    ROW('Participate in some games like falling furni.', 'Participa en juegos como el de los furnis que caen.', 'Participe de jogos como o dos mobis que caem.'),
    ROW('The trick is to always stand one square away from other users.', 'El truco es quedarse siempre a una casilla de los demás usuarios.', 'O truque é ficar sempre a um quadrado dos outros usuários.'),
    ROW('When the owner drops a chair to the room, try to sit on it.', 'Cuando el dueño suelte una silla en la sala, intenta sentarte en ella.', 'Quando o dono soltar uma cadeira no quarto, tente sentar nela.'),
    ROW('The one left out without a chair is losing. The one sitting in the chair at last is the winner.', 'Quien se queda sin silla pierde. Quien queda sentado en la última silla gana.', 'Quem fica sem cadeira perde. Quem fica sentado na última cadeira ganha.'),
    ROW('Falling Furni games are marked as FF or Falling Furni.', 'Los juegos de furnis que caen se marcan como FF o Falling Furni.', 'Os jogos de mobis que caem são marcados como FF ou Falling Furni.'),
    ROW('Hiya!', '¡Holi!', 'Oiê!'),
    ROW('You can kick other users from your room if they are getting too bossy or give a hard time.', 'Puedes echar a otros usuarios de tu sala si se ponen muy mandones o te hacen pasar un mal rato.', 'Você pode expulsar outros usuários do seu quarto se eles ficarem mandões demais ou te incomodarem.'),
    ROW('Shouting is not recommended. Make sure you stand quite close to the people you want to chat with.', 'No se recomienda gritar. Asegúrate de estar bastante cerca de la gente con la que quieres hablar.', 'Gritar não é recomendado. Fique bem perto das pessoas com quem você quer conversar.')) AS tr (en, es, pt)
  ON g.`bot_motto` ->> '$.en' = tr.en
SET g.`bot_motto` = JSON_MERGE_PATCH(g.`bot_motto`, JSON_OBJECT('es', tr.es, 'pt-br', tr.pt));
UPDATE `room_bots_actions` SET `responses` = CAST('[{"text": {"en": "Please keep it down, people are trying to think!", "es": "¡Bajen la voz, que la gente intenta pensar!", "pt-br": "Falem mais baixo, tem gente tentando pensar!"}, "type": "say"}, {"text": {"en": "Only use the Call for help in an emergency!", "es": "¡Usa la Llamada de ayuda solo en una emergencia!", "pt-br": "Só use o Pedido de ajuda numa emergência!"}, "type": "say"}, {"text": {"en": "Want to know more about Habbo Hotel? Ask a Habbo Guide!", "es": "¿Quieres saber más sobre Habbo Hotel? ¡Pregúntale a un Guía Habbo!", "pt-br": "Quer saber mais sobre o Habbo Hotel? Pergunte a um Guia Habbo!"}, "type": "say"}, {"text": {"en": "Is it me or is something BIG about to happen?", "es": "¿Soy yo o está a punto de pasar algo GRANDE?", "pt-br": "É impressão minha ou algo GRANDE está para acontecer?"}, "type": "say"}, {"text": {"en": "In Trouble? Call for Moderator assistance using the Yellow Question Mark!", "es": "¿En apuros? ¡Pide ayuda a un Moderador con el Signo de Interrogación Amarillo!", "pt-br": "Com problemas? Chame um Moderador usando o Ponto de Interrogação Amarelo!"}, "type": "say"}, {"text": {"en": "There''s no such thing as a free lunch or free credits!", "es": "¡No existen ni la comida gratis ni los créditos gratis!", "pt-br": "Não existe almoço grátis, nem moedas grátis!"}, "type": "say"}]' AS JSON)
WHERE `id` = 1 AND `responses` = CAST('[{"text": "Please keep it down, people are trying to think!", "type": "say"}, {"text": "Only use the Call for help in an emergency!", "type": "say"}, {"text": "Want to know more about Habbo Hotel? Ask a Habbo Guide!", "type": "say"}, {"text": "Is it me or is something BIG about to happen?", "type": "say"}, {"text": "In Trouble? Call for Moderator assistance using the Yellow Question Mark!", "type": "say"}, {"text": "There''s no such thing as a free lunch or free credits!", "type": "say"}]' AS JSON);
UPDATE `room_bots_actions` SET `responses` = CAST('[{"text": {"en": "Why Hello there! *Shakes Habbo Hand* My name''s Harry.", "es": "¡Vaya, hola! *Estrecha la mano Habbo* Me llamo Harry.", "pt-br": "Ora, olá! *Aperta a mão Habbo* Meu nome é Harry."}, "type": "say"}, {"text": {"en": "Hello, Hello, Hello!", "es": "¡Hola, hola, hola!", "pt-br": "Olá, olá, olá!"}, "type": "say"}, {"text": {"en": "Hello and welcome to Habbo Hotel! Enjoy your stay! :)", "es": "¡Hola y bienvenido a Habbo Hotel! ¡Disfruta tu estancia! :)", "pt-br": "Olá e bem-vindo ao Habbo Hotel! Aproveite a estadia! :)"}, "type": "say"}]' AS JSON), `trigger_data` = CAST('{"match": "exact", "phrases": {"en": ["hello harry", "hi harry"], "es": ["hola harry", "hola, harry"], "pt-br": ["oi harry", "olá harry", "oi, harry"]}, "case_sensitive": false}' AS JSON)
WHERE `id` = 2 AND `responses` = CAST('[{"text": "Why Hello there! *Shakes Habbo Hand* My name''s Harry.", "type": "say"}, {"text": "Hello, Hello, Hello!", "type": "say"}, {"text": "Hello and welcome to Habbo Hotel! Enjoy your stay! :)", "type": "say"}]' AS JSON) AND `trigger_data` = CAST('{"match": "exact", "phrases": ["hello harry", "hi harry"], "case_sensitive": false}' AS JSON);
UPDATE `room_bots_actions` SET `responses` = CAST('[{"text": {"en": "Better to remain silent and be thought a fool than to speak and remove all doubt.", "es": "Mejor callar y parecer tonto que hablar y despejar todas las dudas.", "pt-br": "Melhor ficar calado e parecer tolo do que falar e tirar todas as dúvidas."}, "type": "say"}, {"text": {"en": "In this room, we keep quiet. Learning takes diligence.", "es": "En esta sala guardamos silencio. Aprender requiere dedicación.", "pt-br": "Neste quarto, ficamos em silêncio. Aprender exige dedicação."}, "type": "say"}, {"text": {"en": "Can I be helping any Habbo?", "es": "¿Puedo ayudar a algún Habbo?", "pt-br": "Posso ajudar algum Habbo?"}, "type": "say"}, {"text": {"en": "English is the tongue of Shakespeare", "es": "El español es la lengua de Cervantes", "pt-br": "O português é a língua de Camões"}, "type": "say"}, {"text": {"en": "Not to be absolutely certain is, I think, one of the essential things in rationality.", "es": "No estar absolutamente seguro es, creo, una de las cosas esenciales de la racionalidad.", "pt-br": "Não ter certeza absoluta é, eu acho, uma das coisas essenciais da racionalidade."}, "type": "say"}, {"text": {"en": "If one does understand a person, one tends to regard him as a fool.", "es": "Si uno entiende a una persona, tiende a considerarla tonta.", "pt-br": "Quando se entende uma pessoa, a gente tende a achá-la tola."}, "type": "say"}, {"text": {"en": "I miss fresh bread daily", "es": "Echo de menos el pan recién hecho de cada día", "pt-br": "Sinto falta de pão fresquinho todo dia"}, "type": "say"}, {"text": {"en": "With an onion and a tomato, every meal is a feast", "es": "Con una cebolla y un tomate, cada comida es un banquete", "pt-br": "Com uma cebola e um tomate, toda refeição é um banquete"}, "type": "say"}]' AS JSON)
WHERE `id` = 4 AND `responses` = CAST('[{"text": "Better to remain silent and be thought a fool than to speak and remove all doubt.", "type": "say"}, {"text": "In this room, we keep quiet. Learning takes diligence.", "type": "say"}, {"text": "Can I be helping any Habbo?", "type": "say"}, {"text": "English is the tongue of Shakespeare", "type": "say"}, {"text": "Not to be absolutely certain is, I think, one of the essential things in rationality.", "type": "say"}, {"text": "If one does understand a person, one tends to regard him as a fool.", "type": "say"}, {"text": "I miss fresh bread daily", "type": "say"}, {"text": "With an onion and a tomato, every meal is a feast", "type": "say"}]' AS JSON);
UPDATE `room_bots_actions` SET `responses` = CAST('[{"text": {"en": "Ah, so you want a book.", "es": "Ah, así que quieres un libro.", "pt-br": "Ah, então você quer um livro."}, "type": "say"}, {"text": {"en": "Ah, I see you''re a total bookworm.", "es": "Ah, ya veo que eres un ratón de biblioteca.", "pt-br": "Ah, vejo que você é um rato de biblioteca."}, "type": "say"}, {"text": {"en": "Have those books back by the weekend!", "es": "¡Devuelve esos libros antes del fin de semana!", "pt-br": "Devolva esses livros até o fim de semana!"}, "type": "say"}]' AS JSON), `trigger_data` = CAST('{"match": "contains", "phrases": {"en": ["book", "1 book", "one book", "books"], "es": ["libro", "libros"], "pt-br": ["livro", "livros"]}, "case_sensitive": false}' AS JSON)
WHERE `id` = 6 AND `responses` = CAST('[{"text": "Ah, so you want a book.", "type": "say"}, {"text": "Ah, I see you''re a total bookworm.", "type": "say"}, {"text": "Have those books back by the weekend!", "type": "say"}]' AS JSON) AND `trigger_data` = CAST('{"match": "contains", "phrases": ["book", "1 book", "one book", "books"], "case_sensitive": false}' AS JSON);
UPDATE `room_bots_actions` SET `responses` = CAST('[{"text": {"en": "Welcome, and step into literary paradise.", "es": "Bienvenido, adelante, pasa al paraíso literario.", "pt-br": "Bem-vindo, entre no paraíso literário."}, "type": "say"}, {"text": {"en": "Hi buddy, all good?", "es": "Hola, amigo, ¿todo bien?", "pt-br": "Oi, amigão, tudo certo?"}, "type": "say"}, {"text": {"en": "Greetings.", "es": "Saludos.", "pt-br": "Saudações."}, "type": "say"}]' AS JSON), `trigger_data` = CAST('{"match": "contains", "phrases": {"en": ["hello", "hi everyone"], "es": ["hola", "hola a todos"], "pt-br": ["oi", "olá", "oi pessoal"]}, "case_sensitive": false}' AS JSON)
WHERE `id` = 7 AND `responses` = CAST('[{"text": "Welcome, and step into literary paradise.", "type": "say"}, {"text": "Hi buddy, all good?", "type": "say"}, {"text": "Greetings.", "type": "say"}]' AS JSON) AND `trigger_data` = CAST('{"match": "contains", "phrases": ["hello", "hi everyone"], "case_sensitive": false}' AS JSON);
UPDATE `room_bots_actions` SET `responses` = CAST('[{"text": {"en": "Is that so?", "es": "¿Ah, sí?", "pt-br": "É mesmo?"}, "type": "say"}, {"text": {"en": "Whaddayamean?", "es": "¿Qué quieres decir?", "pt-br": "Como assim?"}, "type": "say"}, {"text": {"en": "Easy you ruffians. I got only two hands and I work full-time.", "es": "Calma, gamberros. Solo tengo dos manos y trabajo a tiempo completo.", "pt-br": "Calma aí, baderneiros. Só tenho duas mãos e trabalho em tempo integral."}, "type": "say"}, {"text": {"en": "Yes, what would you like?", "es": "Sí, ¿qué te gustaría?", "pt-br": "Sim, o que você gostaria?"}, "type": "say"}]' AS JSON), `trigger_data` = CAST('{"match": "contains", "phrases": {"en": ["marcel", "uhh", "yow", "i think", "can i"], "es": ["marcel", "creo que", "puedo"], "pt-br": ["marcel", "eu acho", "posso"]}, "case_sensitive": false}' AS JSON)
WHERE `id` = 8 AND `responses` = CAST('[{"text": "Is that so?", "type": "say"}, {"text": "Whaddayamean?", "type": "say"}, {"text": "Easy you ruffians. I got only two hands and I work full-time.", "type": "say"}, {"text": "Yes, what would you like?", "type": "say"}]' AS JSON) AND `trigger_data` = CAST('{"match": "contains", "phrases": ["marcel", "uhh", "yow", "i think", "can i"], "case_sensitive": false}' AS JSON);
UPDATE `room_bots_actions` SET `responses` = CAST('[{"text": {"en": "Turn your Bobba filter on and bad words are gone!", "es": "¡Activa tu filtro Bobba y las palabrotas desaparecerán!", "pt-br": "Ligue seu filtro Bobba e os palavrões somem!"}, "type": "say"}]' AS JSON), `trigger_data` = CAST('{"match": "contains", "phrases": {"en": ["bobba"], "es": ["bobba"], "pt-br": ["bobba"]}, "case_sensitive": false}' AS JSON)
WHERE `id` = 9 AND `responses` = CAST('[{"text": "Turn your Bobba filter on and bad words are gone!", "type": "say"}]' AS JSON) AND `trigger_data` = CAST('{"match": "contains", "phrases": ["bobba"], "case_sensitive": false}' AS JSON);
UPDATE `guide_bot_actions` SET `responses` = CAST('[[{"text": {"en": "Be careful", "es": "Ten cuidado", "pt-br": "Tome cuidado"}, "type": "say"}]]' AS JSON), `trigger_data` = CAST('{"match": "contains", "phrases": {"en": ["scam"], "es": ["estafa", "timo"], "pt-br": ["golpe"]}, "cooldown_ticks": 6}' AS JSON)
WHERE `id` = 3 AND `responses` = CAST('[[{"text": "Be careful", "type": "say"}]]' AS JSON) AND `trigger_data` = CAST('{"match": "contains", "phrases": ["scam"], "cooldown_ticks": 6}' AS JSON);
UPDATE `guide_bot_actions` SET `responses` = CAST('[[{"text": {"en": "Looking for people to hang out with?", "es": "¿Buscas gente con quien pasar el rato?", "pt-br": "Procurando gente para ficar de bobeira?"}, "type": "say"}, {"type": "wait", "ticks": 1}, {"text": {"en": "Open the Navigator and see where everyone else is.", "es": "Abre el Navegador y mira dónde está todo el mundo.", "pt-br": "Abra o Navegador e veja onde está todo mundo."}, "type": "say"}]]' AS JSON), `trigger_data` = CAST('{"match": "contains", "phrases": {"en": ["friends"], "es": ["amigos"], "pt-br": ["amigos"]}, "cooldown_ticks": 6}' AS JSON)
WHERE `id` = 4 AND `responses` = CAST('[[{"text": "Looking for people to hang out with?", "type": "say"}, {"type": "wait", "ticks": 1}, {"text": "Open the Navigator and see where everyone else is.", "type": "say"}]]' AS JSON) AND `trigger_data` = CAST('{"match": "contains", "phrases": ["friends"], "cooldown_ticks": 6}' AS JSON);
UPDATE `guide_bot_actions` SET `responses` = CAST('[[{"text": {"en": "To move, click the floor on the spot you want to go to.", "es": "Para moverte, haz clic en el suelo donde quieras ir.", "pt-br": "Para andar, clique no chão onde você quer ir."}, "type": "say"}]]' AS JSON), `trigger_data` = CAST('{"match": "contains", "phrases": {"en": ["move"], "es": ["mover", "caminar"], "pt-br": ["andar", "mover"]}, "cooldown_ticks": 6}' AS JSON)
WHERE `id` = 5 AND `responses` = CAST('[[{"text": "To move, click the floor on the spot you want to go to.", "type": "say"}]]' AS JSON) AND `trigger_data` = CAST('{"match": "contains", "phrases": ["move"], "cooldown_ticks": 6}' AS JSON);
UPDATE `guide_bot_actions` SET `responses` = CAST('[[{"text": {"en": "AI can''t feel love! But you might find a real virtual connection in a dating room.", "es": "¡La IA no puede sentir amor! Pero quizá encuentres una conexión virtual de verdad en una sala de citas.", "pt-br": "IA não sente amor! Mas talvez você encontre uma conexão virtual de verdade num quarto de paquera."}, "type": "say"}]]' AS JSON), `trigger_data` = CAST('{"match": "contains", "phrases": {"en": ["love"], "es": ["amor"], "pt-br": ["amor"]}, "cooldown_ticks": 6}' AS JSON)
WHERE `id` = 6 AND `responses` = CAST('[[{"text": "AI can''t feel love! But you might find a real virtual connection in a dating room.", "type": "say"}]]' AS JSON) AND `trigger_data` = CAST('{"match": "contains", "phrases": ["love"], "cooldown_ticks": 6}' AS JSON);
UPDATE `guide_bot_actions` SET `responses` = CAST('[[{"text": {"en": "You can call for help if someone is giving you a hard time.", "es": "Puedes pedir ayuda si alguien te está haciendo pasar un mal rato.", "pt-br": "Você pode pedir ajuda se alguém estiver te incomodando."}, "type": "say"}, {"type": "wait", "ticks": 1}, {"text": {"en": "Just click on the yellow question mark icon.", "es": "Solo haz clic en el icono del signo de interrogación amarillo.", "pt-br": "É só clicar no ícone do ponto de interrogação amarelo."}, "type": "say"}]]' AS JSON), `trigger_data` = CAST('{"match": "contains", "phrases": {"en": ["help"], "es": ["ayuda"], "pt-br": ["ajuda"]}, "cooldown_ticks": 6}' AS JSON)
WHERE `id` = 7 AND `responses` = CAST('[[{"text": "You can call for help if someone is giving you a hard time.", "type": "say"}, {"type": "wait", "ticks": 1}, {"text": "Just click on the yellow question mark icon.", "type": "say"}]]' AS JSON) AND `trigger_data` = CAST('{"match": "contains", "phrases": ["help"], "cooldown_ticks": 6}' AS JSON);
UPDATE `guide_bot_actions` SET `responses` = CAST('[[{"type": "wave"}, {"type": "wait", "ticks": 1}, {"text": {"en": "If you feel like waving, click the face icon and click wave.", "es": "Si tienes ganas de saludar, haz clic en el icono de la cara y luego en saludar.", "pt-br": "Se quiser acenar, clique no ícone do rosto e depois em acenar."}, "type": "say"}]]' AS JSON), `trigger_data` = CAST('{"match": "contains", "phrases": {"en": ["wave"], "es": ["saludar"], "pt-br": ["acenar"]}, "cooldown_ticks": 6}' AS JSON)
WHERE `id` = 8 AND `responses` = CAST('[[{"type": "wave"}, {"type": "wait", "ticks": 1}, {"text": "If you feel like waving, click the face icon and click wave.", "type": "say"}]]' AS JSON) AND `trigger_data` = CAST('{"match": "contains", "phrases": ["wave"], "cooldown_ticks": 6}' AS JSON);
UPDATE `guide_bot_actions` SET `responses` = CAST('[[{"text": {"en": "Buy your furry friend from the catalogue.", "es": "Compra tu amigo peludo en el catálogo.", "pt-br": "Compre seu amigo peludo no catálogo."}, "type": "say"}, {"type": "wait", "ticks": 1}, {"text": {"en": "He''ll need food and toys to be happy and content and remember, a pet is for life, you cannot give it away!", "es": "Necesitará comida y juguetes para estar feliz y contento. Y recuerda: una mascota es para toda la vida, ¡no puedes regalarla!", "pt-br": "Ele vai precisar de comida e brinquedos para ficar feliz e contente. E lembre-se: um mascote é para a vida toda, você não pode dar ele para ninguém!"}, "type": "say"}]]' AS JSON), `trigger_data` = CAST('{"match": "contains", "phrases": {"en": ["pets"], "es": ["mascotas"], "pt-br": ["mascotes"]}, "cooldown_ticks": 6}' AS JSON)
WHERE `id` = 9 AND `responses` = CAST('[[{"text": "Buy your furry friend from the catalogue.", "type": "say"}, {"type": "wait", "ticks": 1}, {"text": "He''ll need food and toys to be happy and content and remember, a pet is for life, you cannot give it away!", "type": "say"}]]' AS JSON) AND `trigger_data` = CAST('{"match": "contains", "phrases": ["pets"], "cooldown_ticks": 6}' AS JSON);
UPDATE `guide_bot_actions` SET `responses` = CAST('[[{"text": {"en": "Participate in some games like falling furni.", "es": "Participa en juegos como el de los furnis que caen.", "pt-br": "Participe de jogos como o dos mobis que caem."}, "type": "say"}, {"type": "wait", "ticks": 1}, {"text": {"en": "The trick is to always stand one square away from other users.", "es": "El truco es quedarse siempre a una casilla de los demás usuarios.", "pt-br": "O truque é ficar sempre a um quadrado dos outros usuários."}, "type": "say"}, {"type": "wait", "ticks": 1}, {"text": {"en": "When the owner drops a chair to the room, try to sit on it.", "es": "Cuando el dueño suelte una silla en la sala, intenta sentarte en ella.", "pt-br": "Quando o dono soltar uma cadeira no quarto, tente sentar nela."}, "type": "say"}, {"type": "wait", "ticks": 1}, {"text": {"en": "The one left out without a chair is losing. The one sitting in the chair at last is the winner.", "es": "Quien se queda sin silla pierde. Quien queda sentado en la última silla gana.", "pt-br": "Quem fica sem cadeira perde. Quem fica sentado na última cadeira ganha."}, "type": "say"}, {"type": "wait", "ticks": 1}, {"text": {"en": "Falling Furni games are marked as FF or Falling Furni.", "es": "Los juegos de furnis que caen se marcan como FF o Falling Furni.", "pt-br": "Os jogos de mobis que caem são marcados como FF ou Falling Furni."}, "type": "say"}]]' AS JSON), `trigger_data` = CAST('{"match": "contains", "phrases": {"en": ["ff"], "es": ["ff"], "pt-br": ["ff"]}, "cooldown_ticks": 6}' AS JSON)
WHERE `id` = 10 AND `responses` = CAST('[[{"text": "Participate in some games like falling furni.", "type": "say"}, {"type": "wait", "ticks": 1}, {"text": "The trick is to always stand one square away from other users.", "type": "say"}, {"type": "wait", "ticks": 1}, {"text": "When the owner drops a chair to the room, try to sit on it.", "type": "say"}, {"type": "wait", "ticks": 1}, {"text": "The one left out without a chair is losing. The one sitting in the chair at last is the winner.", "type": "say"}, {"type": "wait", "ticks": 1}, {"text": "Falling Furni games are marked as FF or Falling Furni.", "type": "say"}]]' AS JSON) AND `trigger_data` = CAST('{"match": "contains", "phrases": ["ff"], "cooldown_ticks": 6}' AS JSON);
UPDATE `guide_bot_actions` SET `trigger_data` = CAST('{"match": "contains", "phrases": {"en": ["dance"], "es": ["bailar"], "pt-br": ["dançar"]}, "cooldown_ticks": 6}' AS JSON)
WHERE `id` = 11 AND `trigger_data` = CAST('{"match": "contains", "phrases": ["dance"], "cooldown_ticks": 6}' AS JSON);
UPDATE `guide_bot_actions` SET `responses` = CAST('[[{"text": {"en": "Hiya!", "es": "¡Holi!", "pt-br": "Oiê!"}, "type": "say"}]]' AS JSON), `trigger_data` = CAST('{"match": "contains", "phrases": {"en": ["hi"], "es": ["hola"], "pt-br": ["oi"]}, "cooldown_ticks": 6}' AS JSON)
WHERE `id` = 12 AND `responses` = CAST('[[{"text": "Hiya!", "type": "say"}]]' AS JSON) AND `trigger_data` = CAST('{"match": "contains", "phrases": ["hi"], "cooldown_ticks": 6}' AS JSON);
UPDATE `guide_bot_actions` SET `responses` = CAST('[[{"text": {"en": "You can kick other users from your room if they are getting too bossy or give a hard time.", "es": "Puedes echar a otros usuarios de tu sala si se ponen muy mandones o te hacen pasar un mal rato.", "pt-br": "Você pode expulsar outros usuários do seu quarto se eles ficarem mandões demais ou te incomodarem."}, "type": "say"}]]' AS JSON), `trigger_data` = CAST('{"match": "contains", "phrases": {"en": ["kick"], "es": ["echar", "expulsar"], "pt-br": ["expulsar"]}, "cooldown_ticks": 6}' AS JSON)
WHERE `id` = 13 AND `responses` = CAST('[[{"text": "You can kick other users from your room if they are getting too bossy or give a hard time.", "type": "say"}]]' AS JSON) AND `trigger_data` = CAST('{"match": "contains", "phrases": ["kick"], "cooldown_ticks": 6}' AS JSON);
UPDATE `guide_bot_actions` SET `responses` = CAST('[[{"text": {"en": "Shouting is not recommended. Make sure you stand quite close to the people you want to chat with.", "es": "No se recomienda gritar. Asegúrate de estar bastante cerca de la gente con la que quieres hablar.", "pt-br": "Gritar não é recomendado. Fique bem perto das pessoas com quem você quer conversar."}, "type": "say"}]]' AS JSON), `trigger_data` = CAST('{"match": "contains", "phrases": {"en": ["shout"], "es": ["gritar"], "pt-br": ["gritar"]}, "cooldown_ticks": 6}' AS JSON)
WHERE `id` = 14 AND `responses` = CAST('[[{"text": "Shouting is not recommended. Make sure you stand quite close to the people you want to chat with.", "type": "say"}]]' AS JSON) AND `trigger_data` = CAST('{"match": "contains", "phrases": ["shout"], "cooldown_ticks": 6}' AS JSON);

INSERT INTO `global_message_texts` (`message_key`, `message_text`) VALUES
  ('infobus_closed','{"en": "The Infobus is not currently running.", "es": "El Infobús no está funcionando ahora mismo.", "pt-br": "O Infobus não está funcionando no momento."}'),
  ('position_rotation','{"en": "Rotation: {rotation}", "es": "Rotación: {rotation}", "pt-br": "Rotação: {rotation}"}'),
  ('position_room_id','{"en": "Room ID: {id}", "es": "ID de la sala: {id}", "pt-br": "ID do quarto: {id}"}'),
  ('position_room_name','{"en": "Room name: {name}", "es": "Nombre de la sala: {name}", "pt-br": "Nome do quarto: {name}"}'),
  ('position_room_model','{"en": "Room model: {model}", "es": "Modelo de la sala: {model}", "pt-br": "Modelo do quarto: {model}"}'),
  ('info_users_online','{"en": "Users online: {count}", "es": "Usuarios conectados: {count}", "pt-br": "Usuários online: {count}"}'),
  ('info_rooms_loaded','{"en": "Rooms loaded: {count}", "es": "Salas cargadas: {count}", "pt-br": "Quartos carregados: {count}"}')
ON DUPLICATE KEY UPDATE `message_text` = JSON_MERGE_PATCH(VALUES(`message_text`), `message_text`);

ALTER TABLE `definition_furniture`
  ADD COLUMN `localized_name` json DEFAULT NULL COMMENT '{"es": ..., "pt-br": ...} - the furnidata''s' AFTER `localization_desc`,
  ADD COLUMN `localized_desc` json DEFAULT NULL COMMENT '{"es": ..., "pt-br": ...} - the furnidata''s' AFTER `localized_name`;
UPDATE `definition_furniture` f JOIN (VALUES
    ROW('BattleShip', '{}', '{}'),
    ROW('CFC_100_coin_gold', '{"es": "Moneda de Oro (China)", "pt-br": "Gold Coin (China)"}', '{"es": "Artículo no-redimible", "pt-br": "Non-redeemable item"}'),
    ROW('CFC_10_coin_bronze', '{"es": "Moneda de Bronce (China)", "pt-br": "Bronze Coin (China)"}', '{"es": "Artículo no-redimible", "pt-br": "Non-redeemable item"}'),
    ROW('CFC_200_moneybag', '{"es": "Saco de Créditos (China)", "pt-br": "Sack of Credits (China)"}', '{"es": "Artículo no-redimible", "pt-br": "Non-redeemable item"}'),
    ROW('CFC_500_goldbar', '{"es": "Barra de Oro (China)", "pt-br": "Gold Bar (China)"}', '{"es": "Artículo no-redimible", "pt-br": "Non-redeemable item"}'),
    ROW('CFC_50_coin_silver', '{"es": "Moneda de Plata (China)", "pt-br": "Silver Coin (China)"}', '{"es": "Artículo no-redimible", "pt-br": "Non-redeemable item"}'),
    ROW('CF_10_coin_gold', '{"es": "Moneda de Oro", "pt-br": "Moeda de Ouro - Vale 10c"}', '{"es": "Vale 10 Créditos", "pt-br": "Vale 10 Habbo Moedas"}'),
    ROW('CF_1_coin_bronze', '{"es": "Moneda de Bronce", "pt-br": "Moeda de Bronze - Vale 1c"}', '{"es": "Vale 1 Crédito", "pt-br": "Vale 1 Habbo Moeda"}'),
    ROW('CF_20_moneybag', '{"es": "Saco de Créditos", "pt-br": "Sacola - Vale 20c"}', '{"es": "Vale 20 Créditos", "pt-br": "Vale 20 Moedas"}'),
    ROW('CF_50_goldbar', '{"es": "Barra de Oro", "pt-br": "Barra de Ouro"}', '{"es": "Vale 50 Créditos", "pt-br": "Vale 50 Moedas"}'),
    ROW('CF_5_coin_silver', '{"es": "Moneda de Plata", "pt-br": "Moeda de Prata"}', '{"es": "Vale 5 Créditos", "pt-br": "Vale 5 Habbo Moedas"}'),
    ROW('Chess', '{}', '{}'),
    ROW('LT_pillar', '{"es": "Gran Antorcha", "pt-br": "Grande Tocha"}', '{"es": "¡Ilumina tu camino!", "pt-br": "Para espantar Dragão"}'),
    ROW('LT_pillar2', '{"es": "Mini Antorcha", "pt-br": "Pequena Tocha"}', '{"es": "¡Ilumina tu camino!", "pt-br": "Para espantar mosquitinhos"}'),
    ROW('LT_skull', '{"es": "Calaveras", "pt-br": "Caveira"}', '{"es": "¡Menudo trofeo!", "pt-br": "Aqui jaz um Habbo"}'),
    ROW('LT_throne', '{"es": "Sillón Tribal", "pt-br": "Assento Tribal"}', '{"es": "¡Desde donde dirigirlo todo!", "pt-br": "Para os reis tribais"}'),
    ROW('Poker', '{}', '{}'),
    ROW('SF_alien', '{"es": "K-nick-4", "pt-br": "Alienígena"}', '{"es": "De otro mundo", "pt-br": "ET, minha casa, telefone"}'),
    ROW('SF_chair_blue', '{"es": "Silla Espacial Mediana", "pt-br": "Cadeira do Co-Piloto"}', '{"es": "Astronauta comodón, ¡siéntate!", "pt-br": "Conforto azul"}'),
    ROW('SF_chair_green', '{"es": "Silla Espacial Pequeña", "pt-br": "Cadeira da Tripulação"}', '{"es": "Para grumetes del espacio", "pt-br": "Conforto verde"}'),
    ROW('SF_chair_red', '{"es": "Silla del Capitán", "pt-br": "Cadeira do Comandante"}', '{"es": "¡Controla tu nave desde aquí!", "pt-br": "Conforto em vermelho"}'),
    ROW('SF_crate_1', '{"es": "Cajón Galáctico", "pt-br": "Bagagem de mão"}', '{"es": "Fíchalo para tu equipo", "pt-br": "Apenas o essencial"}'),
    ROW('SF_crate_2', '{"es": "Baúl Galáctico", "pt-br": "Bagagem Grande"}', '{"es": "Guarda tu traje espacial", "pt-br": "Cabe tudo aqui"}'),
    ROW('SF_floor_1', '{"es": "Suelo de la Nave Beige", "pt-br": "Piso Claro de Nave"}', '{"es": "Pisa fuerte sobre el espacio", "pt-br": "Dá uma vida ao ambiente artificial"}'),
    ROW('SF_floor_2', '{"es": "Suelo de La Nave Azul", "pt-br": "Piso Escuro de Nave"}', '{"es": "Pasea por tu Nave", "pt-br": "Não deixa muitos rastros"}'),
    ROW('SF_lamp', '{"es": "Lámpara Espacial", "pt-br": "Lâmpada do Espaço"}', '{"es": "La luz de la nave", "pt-br": "Reflexo anti-gravidade"}'),
    ROW('SF_panel1', '{"es": "Generador de rutas", "pt-br": "Painel de Controle"}', '{"es": "Observa el mapa", "pt-br": "Houston, temos um probleminha..."}'),
    ROW('SF_panel2', '{"es": "Panel del Ingeniero", "pt-br": "Painel Grande Sci-Fi"}', '{"es": "¡Controla la nave!", "pt-br": "Qual botão devo apertar?"}'),
    ROW('SF_panel3', '{"es": "Radar espacial", "pt-br": "Painel de Navegação"}', '{"es": "¡Cuidado con los meteoritos!", "pt-br": "Por onde é que vai mesmo?"}'),
    ROW('SF_reactor', '{"es": "Reactor", "pt-br": "Reator Nuclear"}', '{"es": "Sin ellos, tu nave no despega", "pt-br": "A parte mais importante de qualquer nave"}'),
    ROW('SF_table', '{"es": "Mesa de la Nave", "pt-br": "Mesa Interplanetária"}', '{"es": "Para comer píldoras de jamón", "pt-br": "Faça uma conexão com outros mundos"}'),
    ROW('TicTacToe', '{}', '{}'),
    ROW('ads_1800tele', '{"es": "Cabina 1800 Reverse", "pt-br": "Cabine 1800"}', '{}'),
    ROW('ads_711*1', '{"es": "ads_711", "pt-br": "ads_711"}', '{}'),
    ROW('ads_711*2', '{"es": "ads_711", "pt-br": "ads_711"}', '{}'),
    ROW('ads_711*3', '{"es": "ads_711", "pt-br": "ads_711"}', '{}'),
    ROW('ads_711*4', '{"es": "ads_711", "pt-br": "ads_711"}', '{}'),
    ROW('ads_711*5', '{"es": "ads_711", "pt-br": "ads_711"}', '{}'),
    ROW('ads_711*6', '{"es": "ads_711", "pt-br": "ads_711"}', '{}'),
    ROW('ads_711*7', '{"es": "ads_711", "pt-br": "ads_711"}', '{}'),
    ROW('ads_711shelf', '{"es": "ads_711shelf name", "pt-br": "ads_711shelf name"}', '{"es": "ads_711shelf desc", "pt-br": "ads_711shelf desc"}'),
    ROW('ads_boost_surfb', '{"es": "ads_boost_surfb name", "pt-br": "ads_boost_surfb name"}', '{"es": "ads_boost_surfb text", "pt-br": "ads_boost_surfb text"}'),
    ROW('ads_calip_chair', '{"es": "Silla Calippo", "pt-br": "ads_calip_chair name"}', '{"es": "¡De lo más refrescante!", "pt-br": "ads_calip_chair text"}'),
    ROW('ads_calip_cola*1', '{"es": "Calippo Lima", "pt-br": "ads_calip_cola_1 name"}', '{"es": "¡De lo más refrescante!", "pt-br": "ads_calip_cola_1 text"}'),
    ROW('ads_calip_cola*2', '{"es": "Calippo Cola", "pt-br": "ads_calip_cola_2 name"}', '{"es": "¡De lo más refrescante!", "pt-br": "ads_calip_cola_2 text"}'),
    ROW('ads_calip_cola*3', '{"es": "Calippo Fresa", "pt-br": "ads_calip_cola_3 name"}', '{"es": "¡De lo más refrescante!", "pt-br": "ads_calip_cola_3 text"}'),
    ROW('ads_calip_cola*4', '{"es": "Calippo Loco", "pt-br": "Super Habbo Cola"}', '{"es": "¡De lo más refrescante!", "pt-br": "Seu refri geladinho e cheio de gás."}'),
    ROW('ads_calip_fan', '{"es": "Ventolera Calippo", "pt-br": "ads_calip_fan"}', '{"es": "Vaya si refresca", "pt-br": "ads_cali desc"}'),
    ROW('ads_calip_lava', '{"es": "Lámpara Calippo", "pt-br": "ads_calip_lava name"}', '{"es": "No le quites el ojo", "pt-br": "ads_calip_lava desc"}'),
    ROW('ads_calip_parasol', '{"es": "Sombrilla Calippo", "pt-br": "ads_calip_parasol name"}', '{"es": "No te achicharres", "pt-br": "ads_calip_parasol desc"}'),
    ROW('ads_calip_pool', '{"es": "Piscina Calippo", "pt-br": "ads_calip_pool name"}', '{"es": "¡De lo más refrescante!", "pt-br": "ads_calip_pool desc"}'),
    ROW('ads_calip_tele', '{"es": "Teleport Calippo", "pt-br": "ads_calip_tele name"}', '{"es": "Viaja a un lugar refrescante", "pt-br": "ads_calip_tele text"}'),
    ROW('ads_campguitar', '{"es": "ads_campguitar", "pt-br": "ads_campguitar"}', '{"es": "shuubiduubi", "pt-br": "shuubiduubi"}'),
    ROW('ads_cheetos', '{"es": "Máquina de Cheetos", "pt-br": "ads_cheetos name"}', '{"es": "Cheetos a tutiplén", "pt-br": "ads_cheetos desc"}'),
    ROW('ads_chups', '{"es": "Máquina Chupa Chups", "pt-br": "ads_chups name"}', '{"es": "Endulza tu vida", "pt-br": "ads_chups desc"}'),
    ROW('ads_cl_jukeb', '{"es": "ads_cl_jukeb name", "pt-br": "ads_cl_jukeb name"}', '{"es": "ads_cl_jukeb txt", "pt-br": "ads_cl_jukeb txt"}'),
    ROW('ads_cl_moodi', '{"es": "ads_cl_moodi name", "pt-br": "ads_cl_moodi name"}', '{"es": "ads_cl_moodi desc", "pt-br": "ads_cl_moodi desc"}'),
    ROW('ads_cl_sofa', '{"es": "ads_cl_sofa name", "pt-br": "ads_cl_sofa name"}', '{"es": "ads_cl_sofa desc", "pt-br": "ads_cl_sofa desc"}'),
    ROW('ads_clcake', '{"es": "ads_clcake", "pt-br": "ads_clcake"}', '{"es": "ads_clcake", "pt-br": "ads_clcake"}'),
    ROW('ads_cldesk', '{"es": "ads_cldesk", "pt-br": "ads_cldesk"}', '{"es": "old campaign product desc", "pt-br": "old campaign product desc"}'),
    ROW('ads_clfloor', '{"es": "ads_clfloor", "pt-br": "ads_clfloor"}', '{"es": "ads_clfloor", "pt-br": "ads_clfloor"}'),
    ROW('ads_cllava2', '{"es": "ads_cllava2 name", "pt-br": "ads_cllava2 name"}', '{"es": "ads_cllava2 desc", "pt-br": "ads_cllava2 desc"}'),
    ROW('ads_cltele', '{"es": "ads_cltele", "pt-br": "ads_cltele"}', '{"es": "ads_cltele", "pt-br": "ads_cltele"}'),
    ROW('ads_clwall1', '{"es": "ads_clwall1", "pt-br": "ads_clwall1"}', '{"es": "ads_clwall1", "pt-br": "ads_clwall1"}'),
    ROW('ads_clwall2', '{"es": "ads_clwall2", "pt-br": "ads_clwall2"}', '{"es": "ads_clwall2", "pt-br": "ads_clwall2"}'),
    ROW('ads_clwall3', '{"es": "ads_clwall3", "pt-br": "ads_clwall3"}', '{"es": "ads_clwall3", "pt-br": "ads_clwall3"}'),
    ROW('ads_cmusic', '{"es": "Póster Central Musical", "pt-br": "TBD Central Musical TV"}', '{"es": "¡La música inunda tu Sala!", "pt-br": "TBD Click it away"}'),
    ROW('ads_dave_cns', '{"es": "ads_dave_cns", "pt-br": "ads_dave_cns"}', '{}'),
    ROW('ads_dave_wall', '{"es": "ads_dave_wall", "pt-br": "ads_dave_wall"}', '{"es": "ads_dave_wall", "pt-br": "ads_dave_wall"}'),
    ROW('ads_droetker_paula', '{"es": "ads_droetker_paula name", "pt-br": "ads_droetker_paula name"}', '{"es": "ads_droetker_paula desc", "pt-br": "ads_droetker_paula desc"}'),
    ROW('ads_elisa_gnome', '{"es": "ads_elisa_gnome name", "pt-br": "ads_elisa_gnome name"}', '{"es": "ads_elisa_gnome desc", "pt-br": "ads_elisa_gnome desc"}'),
    ROW('ads_frankb', '{"es": "ads_frankb", "pt-br": "ads_frankb"}', '{"es": "ads_frankb", "pt-br": "ads_frankb"}'),
    ROW('ads_goldtabl', '{"es": "ads_goldtabl", "pt-br": "ads_goldtabl"}', '{"es": "ads_goldtabl", "pt-br": "ads_goldtabl"}'),
    ROW('ads_grefusa_cactus', '{"es": "Cáctus SuperTijuana", "pt-br": "ads_grefusa_cactus name"}', '{"es": "¡Lo más explosivo de Pipas G!", "pt-br": "ads_grefusa_cactus desc"}'),
    ROW('ads_grefusa_yum', '{"es": "Dispensador Grefusa", "pt-br": "ads_grefusa_yum name"}', '{"es": "Un sabor inconfundible", "pt-br": "ads_grefusa_yum desc"}'),
    ROW('ads_gsArcade_1', '{"es": "Arcade Tank", "pt-br": "Fliperama Tank"}', '{"es": "Game over", "pt-br": "ads_gsArcade_1 desc"}'),
    ROW('ads_gsArcade_2', '{"es": "Máquina recreativa II", "pt-br": "ads_gsArcade_2 name"}', '{"es": "Game over", "pt-br": "ads_gsArcade_2 desc"}'),
    ROW('ads_idol_audChr', '{"es": "Silla Jurado", "pt-br": "ads_idol_audChr name"}', '{"es": "¡Tú decides quién pasa el corte!", "pt-br": "ads_idol_audChr desc"}'),
    ROW('ads_idol_carpet', '{"es": "ads_idol_carpet name", "pt-br": "ads_idol_carpet name"}', '{"es": "ads_idol_carpet desc", "pt-br": "ads_idol_carpet desc"}'),
    ROW('ads_idol_ch', '{"es": "Silla American Idol", "pt-br": "Cadeira de Estrela"}', '{"es": "¡Lugar solo apto para los mejores!", "pt-br": "O mundo fica melhor com uma dessas"}'),
    ROW('ads_idol_chmpgn', '{"es": "Cubo de Bubblejuice", "pt-br": "Balde Bubblejuice"}', '{"es": "¡Brindemos por la buena música!", "pt-br": "ads_idol_chmpgn"}'),
    ROW('ads_idol_clRack', '{"es": "Vestimenta", "pt-br": "ads_idol_clRack name"}', '{"es": "¿Qué te pondrás para la actuación de hoy?", "pt-br": "ads_idol_clRack desc"}'),
    ROW('ads_idol_cork', '{"es": "Corcho American Idol", "pt-br": "ads_idol_cork name"}', '{"es": "Estos son los candidatos", "pt-br": "ads_idol_cork desc"}'),
    ROW('ads_idol_desk', '{"es": "Mesa del Jurado", "pt-br": "ads_idol_desk"}', '{"es": "Donde se toman las grandes decisiones"}'),
    ROW('ads_idol_drape', '{"es": "Cortinón Azul", "pt-br": "Cortina Azul"}', '{"es": "¿Detrás? Es donde se prepara el espectáculo...", "pt-br": "Sua hora de brilhar"}'),
    ROW('ads_idol_floor1', '{"es": "Suelo Idol I", "pt-br": "ads_idol_floor1 name"}', '{"es": "La estrellas andan sobre él", "pt-br": "ads_idol_floor1 desc"}'),
    ROW('ads_idol_floor2', '{"es": "Suelo Idol II", "pt-br": "ads_idol_floor2 name"}', '{"es": "Las estrellas pisan sobre él", "pt-br": "ads_idol_floor2 desc"}'),
    ROW('ads_idol_hotspot', '{"es": "Círculo de Votación", "pt-br": "Painel Iluminado"}', '{"es": "¿Qué nota conseguirás?", "pt-br": "Quantos pontos você vai iluminar dessa vez?"}'),
    ROW('ads_idol_ichair', '{"es": "Taburate American Idol", "pt-br": "Cadeira Pequena"}', '{"es": "Asiento reservado", "pt-br": "Para aspirantes a estrelas"}'),
    ROW('ads_idol_jukebox*1', '{"es": "Jukebox Idol", "pt-br": "ads_idol_jukebox name"}', '{"es": "La mejor música, en tu Sala", "pt-br": "ads_idol_jukebox desc"}'),
    ROW('ads_idol_l_carpet', '{"es": "Alfombra Latin American Idol", "pt-br": "ads_idol_l_carpet name"}', '{"es": "Deslízate por la música", "pt-br": "ads_idol_l_carpet desc"}'),
    ROW('ads_idol_l_logo', '{"es": "Logo Latin American Idol", "pt-br": "ads_idol_l_logo name"}', '{"es": "Vive a tope la música", "pt-br": "ads_idol_l_logo desc"}'),
    ROW('ads_idol_l_tv', '{"es": "Latin American Idol TV", "pt-br": "Latin American Idol TV"}', '{"es": "TBD Click it away", "pt-br": "TBD Click it away"}'),
    ROW('ads_idol_lamp', '{"es": "Lámpara de boda", "pt-br": "Lâmpada de Cristal Azul"}', '{"es": "¡Ilumina tu amor virtual!", "pt-br": "Seu futuro é azulzinho."}'),
    ROW('ads_idol_logo', '{"es": "ads_idol_logo name", "pt-br": "ads_idol_logo name"}', '{"es": "ads_idol_logo desc", "pt-br": "ads_idol_logo desc"}'),
    ROW('ads_idol_mic', '{"es": "Micro Idol", "pt-br": "Microfone Profissional"}', '{"es": "¡Suelta tu chorro de voz!", "pt-br": "Só para quem quer seguir carreira artística."}'),
    ROW('ads_idol_mirror', '{"es": "Espejo Idol", "pt-br": "ads_idol_mirror name"}', '{"es": "Ponte espectacular para la actuación", "pt-br": "ads_idol_mirror desc"}'),
    ROW('ads_idol_newsDsk', '{"es": "ads_idol_newsDsk name", "pt-br": "Mesa para Laptop"}', '{"es": "s_idol_newsDsk desc", "pt-br": "Quem disse que eu não trabalho?"}'),
    ROW('ads_idol_pchair', '{"es": "Silla de Piano Idol", "pt-br": "ads_idol_pchair name"}', '{"es": "¡Déjanos boquiabiertos con tu partitura!", "pt-br": "ads_idol_pchair txt"}'),
    ROW('ads_idol_piano', '{"es": "Piano Idol", "pt-br": "ads_idol_piano name"}', '{"es": "¡Déjanos boquiabiertos con tu partitura!", "pt-br": "ads_idol_piano desc"}'),
    ROW('ads_idol_tblCloth', '{"es": "Mesa de Bodas", "pt-br": "Mesinha com enfeite"}', '{"es": "¿Listo para un buen banquete?", "pt-br": "Todo mundo precisa de uma horinha extra"}'),
    ROW('ads_idol_tele', '{"es": "Puerta-Teleport Idol", "pt-br": "ads_idol_tele"}', '{"es": "Teletranspórtate hasta tu programa favorito", "pt-br": "ads_idol_tele"}'),
    ROW('ads_idol_trax', '{"es": "Trax Idol", "pt-br": "ads_idol_trax name"}', '{"es": "Sube el volumen", "pt-br": "description"}'),
    ROW('ads_idol_trophy', '{"es": "ads_idol_trophy name", "pt-br": "ads_idol_trophy name"}', '{"es": "ads_idol_trophy desc", "pt-br": "ads_idol_trophy desc"}'),
    ROW('ads_idol_tube', '{"es": "ads_idol_tube name", "pt-br": "ads_idol_tube name"}', '{"es": "ads_idol_tube desc", "pt-br": "ads_idol_tube desc"}'),
    ROW('ads_idol_tv', '{"es": "Latin American Idol TV", "pt-br": "American Idol TV"}', '{"es": "Sumérgete en tu programa favorito", "pt-br": "TBD Click it away"}'),
    ROW('ads_idol_voting_ch', '{"es": "Silla del Jurado", "pt-br": "ads_idol_voting_ch name"}', '{"es": "A votaaaar", "pt-br": "ads_idol_voting_ch desc"}'),
    ROW('ads_idol_wall', '{"es": "American Idol", "pt-br": "ads_idol_wall name"}', '{"es": "Sólo apto para las estrellas", "pt-br": "ads_idol_wall desc"}'),
    ROW('ads_igor_dsk', '{"es": "Mesa Supervillano", "pt-br": "Escrivaninha de Cientista"}', '{"es": "¡Tracemos un plan maquiavélico!", "pt-br": "Não mexa nos meus projetos!"}'),
    ROW('ads_igor_flask', '{"es": "Brebaje Supervillano", "pt-br": "Suco Misterioso"}', '{"es": "¡Poder maligno!", "pt-br": "Qual o sabor?"}'),
    ROW('ads_igor_wall', '{"es": "Plano Supervillano", "pt-br": "Fórmula Secreta"}', '{"es": "¿A ver qué teníamos planeado?", "pt-br": "Anos de estudo em um gráfico de parede"}'),
    ROW('ads_igorbrain', '{"es": "Cerebro", "pt-br": "O Cérebro"}', '{"es": "Muajajaja, lo que se me ha ocurrido...", "pt-br": "Faz falta às vezes"}'),
    ROW('ads_igorevilb', '{"es": "Panel Supervillano", "pt-br": "Câmara Quente"}', '{"es": "¡Pura maldad a la vista!", "pt-br": "Observe de longe"}'),
    ROW('ads_igorraygun', '{"es": "Rayo Supervillano", "pt-br": "Arma de Raio Laser"}', '{"es": "¡La que voy a armar!", "pt-br": "Atravessa paredes"}'),
    ROW('ads_igorswitch', '{"es": "Mando Supervillano", "pt-br": "Controlador"}', '{"es": "¡La que voy a armar!", "pt-br": "Puxar a alavanca pode ser perigoso"}'),
    ROW('ads_latrobe_flag', '{"es": "ads_latrobe_flag name", "pt-br": "ads_latrobe_flag name"}', '{"es": "ads_latrobe_flag text", "pt-br": "ads_latrobe_flag text"}'),
    ROW('ads_lin_wh_c', '{"es": "ads_lin_wh_c name", "pt-br": "ads_lin_wh_c name"}', '{"es": "ads_lin_wh_c text", "pt-br": "ads_lin_wh_c text"}'),
    ROW('ads_malaco_gu', '{"es": "ads_malaco_gu", "pt-br": "ads_malaco_gu"}', '{"es": "ads_malaco_gu", "pt-br": "ads_malaco_gu"}'),
    ROW('ads_malaco_rug', '{"es": "ads_malaco_rug_name", "pt-br": "ads_malaco_rug_name"}', '{}'),
    ROW('ads_malaco_tv', '{"es": "ads_malaco_tv name", "pt-br": "ads_malaco_tv name"}', '{"es": "ads_malaco_tv text", "pt-br": "ads_malaco_tv text"}'),
    ROW('ads_mall_coffeem', '{"es": "Máquina de café", "pt-br": "Quick Coffee Stop"}', '{"es": "Sabe a gloria", "pt-br": "Exclusively Mall coffee and free to boot!"}'),
    ROW('ads_mall_elevator', '{"es": "Ascensor Habbo Centro Comercial", "pt-br": "Elevador Habbo Shopping"}', '{"es": "Pase, pase...", "pt-br": "Teletransporte seu Habbo!"}'),
    ROW('ads_mall_kiosk', '{"es": "Quiosco Habbo Centro Comercial", "pt-br": "Quiosque Habbo Shopping"}', '{"es": "¿Puedo ayudarte?", "pt-br": "Posso te ajudar?"}'),
    ROW('ads_mall_tele', '{"es": "Puertas Habbo Centro Comercial", "pt-br": "Portas do Habbo Shopping"}', '{"es": "Pase, pase...", "pt-br": "Teletransporte seu Habbo!"}'),
    ROW('ads_mall_winbea', '{"es": "Salón de Belleza", "pt-br": "ads_mall_winbea"}', '{"es": "Entra y disfruta", "pt-br": "ads_mall_winbea"}'),
    ROW('ads_mall_winchi', '{"es": "ads_mall_winchi", "pt-br": "ads_mall_winchi"}', '{"es": "ads_mall_winchi", "pt-br": "ads_mall_winchi"}'),
    ROW('ads_mall_wincin', '{"es": "Cine", "pt-br": "ads_mall_wincin"}', '{"es": "Tenemos una cartelera espectacular", "pt-br": "ads_mall_wincin"}'),
    ROW('ads_mall_winclo', '{"es": "Tienda de ropa", "pt-br": "ads_mall_winclo"}', '{"es": "Pasa y veamos qué tal te sienta...", "pt-br": "ads_mall_winclo"}'),
    ROW('ads_mall_window', '{"es": "Escaparate", "pt-br": "Vitrine"}', '{"es": "¿Puedes ver eso?", "pt-br": "Olha isso, que máximo!"}'),
    ROW('ads_mall_winfur', '{"es": "Tienda de Muebles", "pt-br": "Loja de Mobis"}', '{"es": "¿Puedes ver eso?", "pt-br": "Olha isso, que máximo!"}'),
    ROW('ads_mall_wingar', '{"es": "ads_mall_wingar", "pt-br": "ads_mall_wingar"}', '{"es": "ads_mall_wingar", "pt-br": "ads_mall_wingar"}'),
    ROW('ads_mall_winice', '{"es": "Heladería", "pt-br": "Sorveteria"}', '{"es": "¿Puedes ver eso?", "pt-br": "Olha isso, que máximo!"}'),
    ROW('ads_mall_winmus', '{"es": "Tienda de Música", "pt-br": "Loja musical"}', '{"es": "¿Puedes ver eso?", "pt-br": "Olha isso, que máximo!"}'),
    ROW('ads_mall_winpet', '{"es": "Tienda de Mascotas", "pt-br": "ads_mall_winpet"}', '{"es": "¿En busca de un amigo fiel?", "pt-br": "ads_mall_winpet"}'),
    ROW('ads_mall_winspo', '{"es": "Tienda de Deportes", "pt-br": "ads_mall_winspo"}', '{"es": "Todo lo necesario para ponerse en forma", "pt-br": "ads_mall_winspo"}'),
    ROW('ads_mall_wintra', '{"es": "Agencia de Viajes", "pt-br": "ads_mall_wintra"}', '{"es": "¿A qué paraje idílico quieres viajar?", "pt-br": "ads_mall_wintra"}'),
    ROW('ads_mirror', '{"es": "Mini Espejo", "pt-br": "Espelho de Artista"}', '{"es": "¿Quién es el más guap@?", "pt-br": "Concentração nessa hora"}'),
    ROW('ads_mtv_bigtv', '{"es": "Súper Pantalla MTV", "pt-br": "Telão MTV"}', '{"es": "Sigue el ritmo de la música", "pt-br": "Diversão tamanho gigante"}'),
    ROW('ads_mtv_tv', '{"es": "Pantalla MTV", "pt-br": "TV MTV"}', '{"es": "Sigue el ritmo de la música", "pt-br": "Seus videoclipes preferidos passam aqui"}'),
    ROW('ads_mtvtrophy_gold', '{"es": "Trofeo EMA de Oro", "pt-br": "Troféu de Ouro MTV EMA"}', '{"es": "Que viva la música", "pt-br": "Todo artista sonha com o reconhecimento"}'),
    ROW('ads_mtvtrophy_silver', '{"es": "Trofeo EMA de Plata", "pt-br": "Troféu de Prata MTV EMA"}', '{"es": "Que viva la música", "pt-br": "Chegar aqui não foi fácil, hem?"}'),
    ROW('ads_nokia_logo', '{"es": "Logo Nokia", "pt-br": "ads_nokia_logo name"}', '{"es": "Logo Nokia", "pt-br": "ads_nokia_logo desc"}'),
    ROW('ads_nokia_phone', '{"es": "ads_nokia_tel nombre", "pt-br": "ads_nokia_phone name"}', '{"es": "ads_nokia_tel desc", "pt-br": "ads_nokia_phone desc"}'),
    ROW('ads_nokia_x6', '{"es": "Nokia X6", "pt-br": "ads_nokia_x6 name"}', '{"es": "Pedazo de móvil", "pt-br": "ads_nokia_x6 desc"}'),
    ROW('ads_ob_pillow', '{"es": "ads_ob_pillow name", "pt-br": "ads_ob_pillow name"}', '{"es": "The perfect way to relax", "pt-br": "The perfect way to relax"}'),
    ROW('ads_ob_wall', '{"es": "ads_ob_wall", "pt-br": "ads_ob_wall"}', '{"es": "TBD ads_ob_wall", "pt-br": "TBD ads_ob_wall"}'),
    ROW('ads_oc_soda', '{"es": "ads_oc_soda", "pt-br": "ads_oc_soda"}', '{"es": "ads_oc_soda desc", "pt-br": "ads_oc_soda desc"}'),
    ROW('ads_percyrock', '{"es": "ads_percyrock name", "pt-br": "ads_percyrock name"}', '{"es": "ads_percyrock desc", "pt-br": "ads_percyrock desc"}'),
    ROW('ads_percyw', '{"es": "ads_percyw name", "pt-br": "ads_percyw name"}', '{"es": "ads_percyw desc", "pt-br": "ads_percyw desc"}'),
    ROW('ads_puffet_tv', '{"es": "ads_puffet_tv name", "pt-br": "ads_puffet_tv name"}', '{"es": "ads_puffet_tv text", "pt-br": "ads_puffet_tv text"}'),
    ROW('ads_reebok_block2', '{"es": "ads_reebok_block2", "pt-br": "ads_reebok_block2"}', '{}'),
    ROW('ads_reebok_tv', '{"es": "ads_reebok_tv name", "pt-br": "ads_reebok_tv name"}', '{"es": "ads_reebok_tv text", "pt-br": "ads_reebok_tv text"}'),
    ROW('ads_spang_sleep', '{"es": "Saco de Dormir Spangaz Op Survival", "pt-br": "Saco de Dormir Spangaz Op"}', '{"es": "ads_spang_sleep text", "pt-br": "ads_spang_sleep text"}'),
    ROW('ads_sunnyd', '{"es": "Máquina Sunny", "pt-br": "Pôster Sunny"}', '{"es": "El zumo con vitaminas que sabe... Huuum...", "pt-br": "Soolll e muita vibe"}'),
    ROW('ads_target_wall', '{"es": "ads_target_wall name", "pt-br": "ads_target_wall name"}', '{"es": "ads_target_wall desc", "pt-br": "ads_target_wall desc"}'),
    ROW('ads_tlc_wheel', '{"es": "ads_tlc_wheel name", "pt-br": "ads_tlc_wheel name"}', '{"es": "ads_tlc_wheel desc", "pt-br": "ads_tlc_wheel desc"}'),
    ROW('ads_tv_jaapuisto', '{"es": "ads_tv_jaapuisto name", "pt-br": "ads_tv_jaapuisto name"}', '{"es": "ads_tv_jaapuisto text", "pt-br": "ads_tv_jaapuisto text"}'),
    ROW('ads_tv_yle', '{"es": "ads_tv_yle name", "pt-br": "ads_tv_yle name"}', '{"es": "ads_tv_yle text", "pt-br": "ads_tv_yle text"}'),
    ROW('ads_twi_bwall1', '{"es": "Pared Establo", "pt-br": "Parede de Madeira"}', '{"es": "Disfruta de la ''Saga Crepúsculo: Luna Nueva''", "pt-br": "Madeira-de-lei, segura e resistente"}'),
    ROW('ads_twi_bwall2', '{"es": "Camioneta y Motos", "pt-br": "Garagem"}', '{"es": "Disfruta de la ''Saga Crepúsculo: Luna Nueva''", "pt-br": "Evite reboque. Não estacione na frente"}'),
    ROW('ads_twi_chair', '{"es": "Real Silla Volturi", "pt-br": "Trono Volturi"}', '{"es": "Disfruta de la ''Saga Crepúsculo: Luna Nueva''", "pt-br": "Para você que faz parte da nobreza dos vampiros"}'),
    ROW('ads_twi_crest', '{"es": "Escudo Volturi", "pt-br": "Brasão"}', '{"es": "Disfruta de la ''Saga Crepúsculo: Luna Nueva''", "pt-br": "Prova de sangue azul"}'),
    ROW('ads_twi_dreamc', '{"es": "Receptor de sueños", "pt-br": "Apanhador de Sonhos"}', '{"es": "Disfruta de la ''Saga Crepúsculo: Luna Nueva''", "pt-br": "Para se livrar de pesadelos"}'),
    ROW('ads_twi_dvdr1', '{"es": "Mini Pared", "pt-br": "Divisória Ocre"}', '{"es": "Disfruta de la ''Saga Crepúsculo: Luna Nueva''", "pt-br": "Combina com qualquer decoração"}'),
    ROW('ads_twi_dvdr2', '{"es": "Pared de la Torre", "pt-br": "Parede de Volterra"}', '{"es": "Disfruta de la ''Saga Crepúsculo: Luna Nueva''", "pt-br": "Um pedaço da terra dos Volturi"}'),
    ROW('ads_twi_fountn', '{"es": "Fuente", "pt-br": "Fonte Luminosa"}', '{"es": "Disfruta de la ''Saga Crepúsculo: Luna Nueva''", "pt-br": "Na história, Bella corta caminho por ela"}'),
    ROW('ads_twi_mist', '{"es": "Niebla", "pt-br": "Névoa"}', '{"es": "Disfruta de la ''Saga Crepúsculo: Luna Nueva''", "pt-br": "Indício de mistério"}'),
    ROW('ads_twi_paint', '{"es": "Pintura", "pt-br": "Pintura Obscura"}', '{"es": "Disfruta de la ''Saga Crepúsculo: Luna Nueva''", "pt-br": "Abstratamente significativa"}'),
    ROW('ads_twi_piano', '{"es": "Piano Averiado", "pt-br": "Piano Quebrado"}', '{"es": "Disfruta de la ''Saga Crepúsculo: Luna Nueva''", "pt-br": "Quem tocou por último?"}'),
    ROW('ads_twi_roses', '{"es": "Ramillete de Rosas", "pt-br": "Rosas Eternas"}', '{"es": "Disfruta de la ''Saga Crepúsculo: Luna Nueva''", "pt-br": "Como todo amor deve ser"}'),
    ROW('ads_twi_table', '{"es": "Mesa, Pastel y Regalos", "pt-br": "Mesa de Aniversário"}', '{"es": "Disfruta de la ''Saga Crepúsculo: Luna Nueva''", "pt-br": "Com quem será? Com quem será?"}'),
    ROW('ads_twi_toolbx', '{"es": "Caja de Herramientas", "pt-br": "Caixa de Ferramentas"}', '{"es": "Disfruta de la ''Saga Crepúsculo: Luna Nueva''", "pt-br": "Organize tudo depois de usar"}'),
    ROW('ads_twi_tower', '{"es": "Torre del Reloj", "pt-br": "Torre com Relógio"}', '{"es": "Disfruta de la ''Saga Crepúsculo: Luna Nueva''", "pt-br": "Ao meio-dia, com o sol a pino, Edward vai aparecer"}'),
    ROW('ads_twi_trophy', '{"es": "Trofeo Lobo", "pt-br": "Troféu Twilight"}', '{"es": "Disfruta de la ''Saga Crepúsculo: Luna Nueva''", "pt-br": "Para O Maior Fã de Lua Nova do Habbo Hotel"}'),
    ROW('ads_twi_windw', '{"es": "Ventana con velas", "pt-br": "Janela Luz de Velas"}', '{"es": "Disfruta de la ''Saga Crepúsculo: Luna Nueva''", "pt-br": "Veja o céu de crepúsculo lá fora"}'),
    ROW('ads_veet', '{"es": "Póster Veet", "pt-br": "ads_veet name"}', '{"es": "¡Depilación al poder!", "pt-br": "ads_veet desc"}'),
    ROW('ads_wwe_poster', '{"es": "ads_wwe_poster name", "pt-br": "ads_wwe_poster name"}', '{"es": "ads_wwe_poster text", "pt-br": "ads_wwe_poster text"}'),
    ROW('arabian_bigtb', '{"es": "Mesa Árabe", "pt-br": "Mesa de Jantar Marroquina"}', '{"es": "¡Hermoso mosaico!", "pt-br": "Jantares com um ar de riqueza"}'),
    ROW('arabian_chair', '{"es": "Silla Árabe", "pt-br": "Cadeira Marroquina"}', '{"es": "Descanso Oriental", "pt-br": "Conforto e bom gosto"}'),
    ROW('arabian_divdr', '{"es": "Divisor Árabe", "pt-br": "Divisor Marroquino"}', '{"es": "¡Ligero como una pluma!", "pt-br": "Separe suas riquezas em ambientes"}'),
    ROW('arabian_pllw', '{"es": "Cojín Árabe", "pt-br": "Almofada Marroquina"}', '{"es": "¡Bien mullido!", "pt-br": "Linda estampa"}'),
    ROW('arabian_rug', '{"es": "Alfombra Árabe", "pt-br": "Tapete Marroquino"}', '{"es": "¿Nos llevará hasta el Lejano Oriente?", "pt-br": "Tapete persa é coisa do passado."}'),
    ROW('arabian_snake', '{"es": "Serpiente", "pt-br": "Serpente na Cesta"}', '{"es": "Inofensiva siempre que seas un buen encantador", "pt-br": "Surpresa!"}'),
    ROW('arabian_swords', '{"es": "Espadas Árabes", "pt-br": "Espadas dos Ancestrais"}', '{"es": "¡Bien afiladas!", "pt-br": "Não brinque com elas"}'),
    ROW('arabian_teamk', '{"es": "Tetera Árabe", "pt-br": "Carrinho de Bebidas"}', '{"es": "¡Desprende un dulce aroma!", "pt-br": "Gente chique é outra coisa"}'),
    ROW('arabian_tetbl', '{"es": "Mesita Árabe", "pt-br": "Mesa de Chá"}', '{"es": "¡Reluciente como ninguna otra!", "pt-br": "Para momentos especiais"}'),
    ROW('arabian_tile', '{"es": "Mármol Blanco", "pt-br": "Piso Alhambra"}', '{"es": "No puede faltar en ningún palacete", "pt-br": "Por onde andam reis e rainhas"}'),
    ROW('arabian_tray1', '{"es": "Bandeja de té", "pt-br": "Guloseimas Marroquinas"}', '{"es": "¡A la menta, por supuesto!", "pt-br": "Tem chá, bolos e frutas"}'),
    ROW('arabian_tray2', '{"es": "Bandeja con velas", "pt-br": "Mesa Decorada"}', '{"es": "¡Luz oriental!", "pt-br": "Sirva um chá com estilo"}'),
    ROW('arabian_tray3', '{"es": "Bandeja de Dulces", "pt-br": "Mesa de Doces Marroquinos"}', '{"es": "¡Para los más golosos!", "pt-br": "Sabores exóticos e deliciosos"}'),
    ROW('arabian_tray4', '{"es": "Bandeja de Frutas", "pt-br": "Mesa com Frutas"}', '{"es": "¡Vitaminas al poder!", "pt-br": "Para uma dieta saudável"}'),
    ROW('arabian_wall', '{"es": "Pared Mosaico", "pt-br": "Parede Alhambra"}', '{"es": "¡No puede faltar en ningún palacete!", "pt-br": "Palácios erguidos, sonhos realizados"}'),
    ROW('arabian_wndw', '{"es": "Ventana Árabe", "pt-br": "Janela Marroquina"}', '{"es": "¡Un poco de luz!", "pt-br": "Deixe o ar entrar"}'),
    ROW('audChr', '{"es": "Silla Audición", "pt-br": "Cadeira de Plateia"}', '{"es": "¡Tú decides quién pasa el corte!", "pt-br": "Gente muito animada senta aqui."}'),
    ROW('bar_armas', '{"es": "Barril Despensa", "pt-br": "Tonel Frigobar"}', '{"es": "Madera noble, que conserva y distingue", "pt-br": "Um tonel de risadas e um bom tema para debates"}'),
    ROW('bar_basic', '{"es": "Pura minibar", "pt-br": "Frigobar Pura"}', '{"es": "Más que necesario", "pt-br": "Frigobar Pura Série 300"}'),
    ROW('bar_chair_armas', '{"es": "El barril-taburete", "pt-br": "Tonel Banqueta"}', '{"es": "Reciclar es bueno, ¿verdad?", "pt-br": "O que há de mais moderno em móvel reciclado"}'),
    ROW('bar_polyfon', '{"es": "Minibar", "pt-br": "Frigobar"}', '{"es": "Se ve pero no se toca", "pt-br": "Um toque moderno e metálico"}'),
    ROW('barchair_silo', '{"es": "Taburete", "pt-br": "Banqueta de Bar Cinza"}', '{"es": "Práctico y conveniente", "pt-br": "Prático e conveniente"}'),
    ROW('barchair_silo*2', '{"es": "Taburete Negro", "pt-br": "Banqueta de Bar Preta"}', '{"es": "Práctico y conveniente", "pt-br": "Prático e conveniente"}'),
    ROW('barchair_silo*3', '{"es": "Taburete Blanco", "pt-br": "Banqueta de Bar Branca"}', '{"es": "Práctico y conveniente", "pt-br": "Prático e conveniente"}'),
    ROW('barchair_silo*4', '{"es": "Taburete Beige", "pt-br": "Banqueta de Bar Bege"}', '{"es": "Práctico y conveniente", "pt-br": "Prático e conveniente"}'),
    ROW('barchair_silo*5', '{"es": "Taburete Rosa", "pt-br": "Banqueta de Bar Rosa"}', '{"es": "Práctico y conveniente", "pt-br": "Prático e conveniente"}'),
    ROW('barchair_silo*6', '{"es": "Taburete Azul", "pt-br": "Banqueta de Bar Azul"}', '{"es": "Práctico y conveniente", "pt-br": "Prático e conveniente"}'),
    ROW('barchair_silo*7', '{"es": "Taburete Verde", "pt-br": "Banqueta de Bar Verde"}', '{"es": "Práctico y conveniente", "pt-br": "Prático e conveniente"}'),
    ROW('barchair_silo*8', '{"es": "Taburete Amarillo", "pt-br": "Banqueta de Bar Amarela"}', '{"es": "Práctico y conveniente", "pt-br": "Prático e conveniente"}'),
    ROW('barchair_silo*9', '{"es": "Taburete Rojo", "pt-br": "Banqueta de Bar Vermelha"}', '{"es": "Práctico y conveniente", "pt-br": "Prático e conveniente"}'),
    ROW('bardesk_polyfon', '{"es": "Barra Moda", "pt-br": "Mode Bar/Balcão"}', '{"es": "Para subir, subir...", "pt-br": "Para empilhar legal!"}'),
    ROW('bardesk_polyfon*2', '{"es": "Barra Mode Negra", "pt-br": "Porta Balcão Preta"}', '{"es": "Para subir, subir...", "pt-br": "Combina com tudo"}'),
    ROW('bardesk_polyfon*3', '{"es": "Barra Mode Blanca", "pt-br": "Balcão Mode"}', '{"es": "Para subir, subir...", "pt-br": "Cor branco, para empilhar"}'),
    ROW('bardesk_polyfon*4', '{"es": "Barra Mode Beige", "pt-br": "Balcão Mode"}', '{"es": "Para subir, subir...", "pt-br": "Cor bege, para empilhar"}'),
    ROW('bardesk_polyfon*5', '{"es": "Barra Rosa", "pt-br": "Balcão Candy"}', '{"es": "¡Prohibido!", "pt-br": "Balcão Candy (5)"}'),
    ROW('bardesk_polyfon*6', '{"es": "Barra Mode Azul", "pt-br": "Azul Balcão Mode"}', '{"es": "Para subir, subir...", "pt-br": "Azul Balcão Mode"}'),
    ROW('bardesk_polyfon*7', '{"es": "Barra Mode Verde", "pt-br": "Verde Balcão Mode"}', '{"es": "Barra Mode Verde", "pt-br": "Verde Balcão Mode"}'),
    ROW('bardesk_polyfon*8', '{"es": "Barra Mode Amarilla", "pt-br": "Amarelo Balcão Mode"}', '{"es": "Para subir, subir...", "pt-br": "Amarelo Balcão Mode"}'),
    ROW('bardesk_polyfon*9', '{"es": "Barra Mode Roja", "pt-br": "Balcão Vermelho"}', '{"es": "Para subir, subir...", "pt-br": "Coleção Mode Natal 2007"}'),
    ROW('bardeskcorner_polyfon', '{"es": "Escritorio", "pt-br": "Mode Cantos"}', '{"es": "Disfruta de la comodidad de su forma", "pt-br": "Canto de Balcão Mode"}'),
    ROW('bardeskcorner_polyfon*12', '{"es": "Esquina Mode", "pt-br": "Canto de balcão"}', '{"es": "Disfruta de la comodidad de su forma", "pt-br": "Coloque-o em qualquer canto"}'),
    ROW('bardeskcorner_polyfon*13', '{"es": "Escritorio de rincón", "pt-br": "Canto de balcão"}', '{"es": "Disfruta de la comodidad de su forma", "pt-br": "Coloque-o em qualquer canto"}'),
    ROW('bardeskcorner_polyfon*2', '{"es": "Esquina Mode Negra", "pt-br": "Canto de Balcão Preto"}', '{"es": "Disfruta de la comodidad de su forma", "pt-br": "Monte espaços reservados"}'),
    ROW('bardeskcorner_polyfon*3', '{"es": "Escritorio Blanco", "pt-br": "Canto de Balcão Branco"}', '{"es": "Disfruta de la comodidad de su forma", "pt-br": "Canto de Balcão Branco"}'),
    ROW('bardeskcorner_polyfon*4', '{"es": "Esquina Mode Beige", "pt-br": "Canto de Balcão Bege"}', '{"es": "Disfruta de la comodidad de su forma", "pt-br": "Canto de Balcão Bege"}'),
    ROW('bardeskcorner_polyfon*5', '{"es": "Esquina Mode Rosa", "pt-br": "Pacote Candy Balcão"}', '{"es": "¡A la última moda!", "pt-br": "Canto de Balcão Candy"}'),
    ROW('bardeskcorner_polyfon*6', '{"es": "Esquina Mode Azul", "pt-br": "Azul Canto de Balcão Mode"}', '{"es": "Disfruta de la comodidad de su forma", "pt-br": "Azul Canto de Balcão Mode"}'),
    ROW('bardeskcorner_polyfon*7', '{"es": "Escritorio Verde", "pt-br": "Verde Canto de Balcão Mode"}', '{"es": "Disfruta de la comodidad de su forma", "pt-br": "Verde Canto de Balcão Mode"}'),
    ROW('bardeskcorner_polyfon*8', '{"es": "Esquina Mode Amarilla", "pt-br": "Amarelo Canto de Balcão Mode"}', '{"es": "Disfruta de la comodidad de su forma", "pt-br": "Amarelo Canto de Balcão Mode"}'),
    ROW('bardeskcorner_polyfon*9', '{"es": "Escritorio Rojo", "pt-br": "Canto de Balcão Vermelho"}', '{"es": "Disfruta de la comodidad de su forma", "pt-br": "Coleção Mode Natal 2007"}'),
    ROW('barrier*1', '{"es": "Barrera Amarilla", "pt-br": "Barreira Amarela"}', '{"es": "¡No pasar!", "pt-br": "Não ultrapasse."}'),
    ROW('barrier*2', '{"es": "Barrera Blanca", "pt-br": "Barreira Branca"}', '{"es": "¡No pasar!", "pt-br": "Não ultrapasse."}'),
    ROW('barrier*3', '{"es": "Barrera Roja", "pt-br": "Barreira"}', '{"es": "¡No pasar!", "pt-br": "Não ultrapasse o cavalete."}'),
    ROW('bartable_armas', '{"es": "Mesa", "pt-br": "Balcão de bar"}', '{"es": "Para invitados sorpresa", "pt-br": "Mesa em Estilo de Bar - essencial para convidados extras"}'),
    ROW('basket', '{"es": "Cesta de sorpresas", "pt-br": "Cesta de Ovos"}', '{"es": "Lo necesario para Pascua", "pt-br": "Um novo motivo para celebrar a Páscoa"}'),
    ROW('bath', '{"es": "La bañera", "pt-br": "Banho de Espuma"}', '{"es": "Zambúllete en un relajante baño de burbujas", "pt-br": "O que há de mais moderno em conforto"}'),
    ROW('beanstalk', '{"es": "Judía Mágica", "pt-br": "Pé-de-Feijão"}', '{"es": "Hasta el infinito y más allá...", "pt-br": "Quanto mais alto se vai, maior a queda"}'),
    ROW('bed_armas_one', '{"es": "Cama", "pt-br": "Cama de Solteiro"}', '{"es": "Una cama para soñar", "pt-br": "Charme rústico para uma pessoa"}'),
    ROW('bed_armas_two', '{"es": "Cama doble", "pt-br": "Cama de Casal"}', '{"es": "Cama grande, cómoda y confortable", "pt-br": "O conforto de uma cama king-size em madeira de pinho"}'),
    ROW('bed_budget', '{"es": "Cama doble azul", "pt-br": "bed_budget"}', '{"es": "Descanso doble para el Habbo", "pt-br": "Bons sonhos."}'),
    ROW('bed_budget*1', '{"es": "Cama Doble Aguamarina", "pt-br": "Cama de Casal Aquamarine"}', '{"es": "Descanso doble para el Habbo", "pt-br": "Conforto para duas pessoas."}'),
    ROW('bed_budget*2', '{"es": "Cama Doble Rosa", "pt-br": "Cama de Casal Rosa"}', '{"es": "Comodidad total", "pt-br": "Maciez na cor mais fofa."}'),
    ROW('bed_budget*3', '{"es": "Cama Doble Negra", "pt-br": "Cama de Casal Preta"}', '{"es": "A descansar...", "pt-br": "Descanse numa cama de cor neutra."}'),
    ROW('bed_budget*4', '{"es": "Cama Doble Blanca", "pt-br": "Cama de Casal Branca"}', '{"es": "La armonía es necesaria", "pt-br": "Muito conforto"}'),
    ROW('bed_budget*5', '{"es": "Cama Doble Beige", "pt-br": "Cama de Casal Bege"}', '{"es": "A descansar...", "pt-br": "Descanse seus pixels."}'),
    ROW('bed_budget*6', '{"es": "Cama Doble Azul", "pt-br": "Cama de Casal Azul"}', '{"es": "Dulces sueños", "pt-br": "Conforto é o que há"}'),
    ROW('bed_budget*7', '{"es": "Cama Doble Verde", "pt-br": "Cama de Casal Pura Verde"}', '{"es": "¡La mar de cómoda!", "pt-br": "Bons sonhos"}'),
    ROW('bed_budget*8', '{"es": "Cama Doble Amarilla", "pt-br": "Cama de Casal Amarela"}', '{"es": "¡Doble comfort!", "pt-br": "Na bela cor amarela."}'),
    ROW('bed_budget*9', '{"es": "Cama Doble Roja", "pt-br": "Cama de Casal Vermelha"}', '{"es": "A descansar...", "pt-br": "Na cor do amor. Para casais apaixonados descansarem."}'),
    ROW('bed_budget_one', '{"es": "Cama azul", "pt-br": "Cama de Solteiro Simples"}', '{"es": "Descansa...", "pt-br": "Na bela cor azul claro."}'),
    ROW('bed_budget_one*1', '{"es": "Cama Aguamarina", "pt-br": "Cama de Solteiro"}', '{"es": "Descansa...", "pt-br": "Descanse com conforto."}'),
    ROW('bed_budget_one*2', '{"es": "Cama Rosa", "pt-br": "Cama de Solteiro"}', '{"es": "Descanso total", "pt-br": "Na bela cor rosa."}'),
    ROW('bed_budget_one*3', '{"es": "Cama Negra", "pt-br": "Cama de Solteiro"}', '{"es": "Descansa...", "pt-br": "Na bela cor preta."}'),
    ROW('bed_budget_one*4', '{"es": "Cama Blanca", "pt-br": "Cama de Solteiro"}', '{"es": "Descansa...", "pt-br": "Na bela cor branca."}'),
    ROW('bed_budget_one*5', '{"es": "Cama Beige", "pt-br": "Cama de Solteiro"}', '{"es": "Comodidad total", "pt-br": "Na bela cor bege."}'),
    ROW('bed_budget_one*6', '{"es": "Cama Azul", "pt-br": "Cama de Solteiro"}', '{"es": "Que duermas bien...", "pt-br": "Na bela cor azul."}'),
    ROW('bed_budget_one*7', '{"es": "Cama Verde", "pt-br": "Cama Simples Pura Verde"}', '{"es": "¡De lo más cómoda!", "pt-br": "Tudo o que você precisa para uma boa noite de sono"}'),
    ROW('bed_budget_one*8', '{"es": "Cama Amarilla", "pt-br": "Cama de Solteiro"}', '{"es": "¡Pura comodidad!", "pt-br": "Na bela cor amarela."}'),
    ROW('bed_budget_one*9', '{"es": "Cama Roja", "pt-br": "Cama de Solteiro"}', '{"es": "Toca descansar", "pt-br": "Na bela cor vermelha."}'),
    ROW('bed_budgetb', '{"es": "Cama doble", "pt-br": "Cama de Casal Simples"}', '{"es": "La armonía es necesaria", "pt-br": "Doces sonhos a dois"}'),
    ROW('bed_budgetb_one', '{"es": "Cama", "pt-br": "Cama de Solteiro Simples"}', '{"es": "Descansa...", "pt-br": "Tudo o que você precisa para uma boa noite de sono"}'),
    ROW('bed_polyfon', '{"es": "Cama doble", "pt-br": "Cama de Casal"}', '{"es": "No me gustan los despertadores", "pt-br": "Tenha o seu próprio espaço para se espreguiçar"}'),
    ROW('bed_polyfon*2', '{"es": "Cama Doble Negra", "pt-br": "Cama de Casal Mode Preta"}', '{"es": "No me gustan los despertadores", "pt-br": "O preto é a cor da estação"}'),
    ROW('bed_polyfon*3', '{"es": "Cama Doble Blanca", "pt-br": "Cama de Casal"}', '{"es": "No me gustan los despertadores", "pt-br": "Cama de Casal Branco"}'),
    ROW('bed_polyfon*4', '{"es": "Cama Doble Beige", "pt-br": "Cama de Casal"}', '{"es": "No me gustan los despertadores", "pt-br": "Cama de Casal Bege"}'),
    ROW('bed_polyfon*6', '{"es": "Cama Doble Azul", "pt-br": "Azul Mode Cama de Casal"}', '{"es": "No me gustan los despertadores", "pt-br": "Azul Mode Cama de Casal"}'),
    ROW('bed_polyfon*7', '{"es": "Cama Doble Verde", "pt-br": "Verde Mode Cama de Casal"}', '{"es": "No me gustan los despertadores", "pt-br": "Verde Mode Cama de Casal"}'),
    ROW('bed_polyfon*8', '{"es": "Cama Doble Amarilla", "pt-br": "Amarelo Mode Cama de Casal"}', '{"es": "No me gustan los despertadores", "pt-br": "Amarelo Mode Cama de Casal"}'),
    ROW('bed_polyfon*9', '{"es": "Cama Doble Roja", "pt-br": "Cama de Casal Vermelho"}', '{"es": "No me gustan los despertadores", "pt-br": "Coleção Mode Natal 2007"}'),
    ROW('bed_polyfon_girl', '{"es": "Cama doble", "pt-br": "Cama de Casal"}', '{"es": "¿Rosa es el color de los sueños?", "pt-br": "Durma confortavelmente em um mundo cor-de-rosa"}'),
    ROW('bed_polyfon_girl_one', '{"es": "Cama", "pt-br": "Cama de Solteiro"}', '{"es": "¿Rosa es el color del sueño?", "pt-br": "Durma confortavelmente em um mundo cor-de-rosa"}'),
    ROW('bed_polyfon_one', '{"es": "Cama", "pt-br": "Cama de Solteiro"}', '{"es": "Una camita artística", "pt-br": "O berço da arte"}'),
    ROW('bed_polyfon_one*2', '{"es": "Cama Mode Negra", "pt-br": "Cama de Solteiro Mode"}', '{"es": "Una camita artística", "pt-br": "Durma bem"}'),
    ROW('bed_polyfon_one*3', '{"es": "Cama Blanca", "pt-br": "Cama Simples"}', '{"es": "Una camita artística", "pt-br": "Cama Simples cor Branco"}'),
    ROW('bed_polyfon_one*4', '{"es": "Cama Beige", "pt-br": "Cama Simples"}', '{"es": "Una camita artística", "pt-br": "Cama Simples cor Bege"}'),
    ROW('bed_polyfon_one*6', '{"es": "Cama Azul", "pt-br": "Azul Mode Cama Simples"}', '{"es": "Una camita artística", "pt-br": "Azul Mode Cama Simples"}'),
    ROW('bed_polyfon_one*7', '{"es": "Cama Verde", "pt-br": "Verde Mode Cama Simples"}', '{"es": "Una camita artística", "pt-br": "Verde Mode Cama Simples"}'),
    ROW('bed_polyfon_one*8', '{"es": "Cama Amarilla", "pt-br": "Amarelo Mode Cama Simples"}', '{"es": "Una camita artística", "pt-br": "Amarelo Mode Cama Simples"}'),
    ROW('bed_polyfon_one*9', '{"es": "Cama Roja", "pt-br": "Cama Simples Vermelho"}', '{"es": "Una camita artística", "pt-br": "Coleção Mode Natal 2007"}'),
    ROW('bed_silo_one', '{"es": "Cama", "pt-br": "Cama de Solteiro"}', '{"es": "Para la siesta", "pt-br": "Completa e simples"}'),
    ROW('bed_silo_two', '{"es": "Cama doble", "pt-br": "Cama de Casal"}', '{"es": "¿Te gustan los grandes espacios?", "pt-br": "Completa e simples para dois"}'),
    ROW('bed_trad', '{"es": "Cama doble", "pt-br": "Cama de Casal Simples"}', '{"es": "Solo o a dúo...", "pt-br": "Doces sonhos a dois"}'),
    ROW('bed_trad_one', '{"es": "Cama", "pt-br": "Cama de Solteiro Simples"}', '{"es": "Buenas nochessss", "pt-br": "Tudo o que você precisa para uma boa noite de sono"}'),
    ROW('bench_armas', '{"es": "Banco", "pt-br": "Banco"}', '{"es": "Un banco para banquetes", "pt-br": "Para completar o conjunto de jantar"}'),
    ROW('bench_lego', '{"es": "Banquillo de Equipo", "pt-br": "Banco Esportivo"}', '{"es": "Para los suplentes", "pt-br": "Para os seus reservas"}'),
    ROW('bench_puffet', '{"es": "bench_puffet", "pt-br": "bench_puffet"}', '{"es": "bench_puffet", "pt-br": "bench_puffet"}'),
    ROW('birdie', '{"es": "El mini pollito", "pt-br": "Ovo Rachado"}', '{"es": "Pío, pío", "pt-br": "Bonito e barato (!)"}'),
    ROW('bling_bed', '{"es": "Cama Púrpura", "pt-br": "Cama púrpura do amor"}', '{"es": "Esos focos me van a dejar ciego...", "pt-br": "Macia, quentinha, confortável."}'),
    ROW('bling_cabinet', '{"es": "Armario Baño", "pt-br": "Gabinete"}', '{"es": "Todo bien ordenado", "pt-br": "Tudo organizado na casa de banho"}'),
    ROW('bling_chair_a', '{"es": "Silla San Valentín Roja", "pt-br": "Suave Encosto"}', '{"es": "Rodéate de amor", "pt-br": "Paz na correria. Na cor vermelha."}'),
    ROW('bling_chair_b', '{"es": "Silla San Valentín Azul", "pt-br": "Suave Encosto"}', '{"es": "Rodéate de amor", "pt-br": "Paz na correria. Na cor azul."}'),
    ROW('bling_chair_c', '{"es": "Silla San Valentín Violeta", "pt-br": "Suave Recosto"}', '{"es": "Rodéate de amor", "pt-br": "Pausa com estilo, descanso na medida."}'),
    ROW('bling_fridge', '{"es": "Frigorífico Púrpura", "pt-br": "Violeta Geladeira"}', '{"es": "Para conservar el amor en buen estado", "pt-br": "Personalidade na arte de refrigerar."}'),
    ROW('bling_fridge_restricted', '{"es": "No usar este frigorífico", "pt-br": "Champanhe Púrpura"}', '{"es": "bling_fridge_restricted desc", "pt-br": "A melhor safra de bolhas do Hotel."}'),
    ROW('bling_pool', '{"es": "Piscina-Jacuzzi", "pt-br": "Jacuzzi"}', '{"es": "¿Acaso puedes resistirte a probarla?", "pt-br": "Relaxe com estilo, um banho de sofisticação."}'),
    ROW('bling_shwr', '{"es": "Ducha", "pt-br": "Banho de Estrela"}', '{"es": "El agua sale a la temperatura ideal", "pt-br": "O bem-estar de uma ducha gorducha."}'),
    ROW('bling_sink', '{"es": "Lavabo de Mármol", "pt-br": "Pia de luxo"}', '{"es": "Limpio y perfumado", "pt-br": "Mantenha suas mãos limpas, doçura"}'),
    ROW('bling_sofa', '{"es": "Sofá de Cuero", "pt-br": "Sofá de couro"}', '{"es": "Comodidad absoluta", "pt-br": "As metades da laranja, dois amantes, dois irmãos."}'),
    ROW('bling_toilet', '{"es": "Wáter de Oro", "pt-br": "Privada de ouro"}', '{"es": "El último grito en baños románticos", "pt-br": "Para nobres traseiros"}'),
    ROW('bolly_corner', '{"es": "Esquina Bollywood", "pt-br": "Mini Balcão Bolly"}', '{"es": "¡Desprende amor!", "pt-br": "Para arrendondar os cantos"}'),
    ROW('bolly_desk', '{"es": "Barra Bollywood", "pt-br": "Balcão Bolly"}', '{"es": "¡Desprende amor!", "pt-br": "Para dividir o cenário"}'),
    ROW('bolly_drapea', '{"es": "Cortina Rosa", "pt-br": "Cortina Rosa"}', '{"es": "¡Desprende amor!", "pt-br": "Para camarins"}'),
    ROW('bolly_drapeb', '{"es": "Cortina Verde", "pt-br": "Cortina Verde"}', '{"es": "¡Desprende amor!", "pt-br": "Para camarins"}'),
    ROW('bolly_drapec', '{"es": "Cortina Amarilla", "pt-br": "Cortina Amarela"}', '{"es": "¡Desprende amor!", "pt-br": "Para camarins"}'),
    ROW('bolly_fountain', '{"es": "Fontana Bollywood", "pt-br": "Fonte Luminosa Bolly"}', '{"es": "Glub, glub, glub...", "pt-br": "Jorrando alegria"}'),
    ROW('bolly_lamp', '{"es": "Lámpara Bollywood", "pt-br": "Lustre Bolly"}', '{"es": "¡Desprende amor!", "pt-br": "Elegância de cinema"}'),
    ROW('bolly_lotus_pool', '{"es": "Flor de Loto", "pt-br": "Fonte de Lótus"}', '{"es": "¡Desprende amor!", "pt-br": "Muita energia positiva"}'),
    ROW('bolly_monkey_lamp', '{"es": "Lámpara Mono", "pt-br": "Lâmpada Macaco"}', '{"es": "¡Desprende amor!", "pt-br": "Não faça cócegas!"}'),
    ROW('bolly_palm', '{"es": "Palmera", "pt-br": "Palmeira da Índia"}', '{"es": "¡Verde y lustrosa!", "pt-br": "Sempre verde"}'),
    ROW('bolly_petals', '{"es": "Pétalos", "pt-br": "Chuva de Pétalas"}', '{"es": "¡Desprenden amor!", "pt-br": "Caindo sem parar"}'),
    ROW('bolly_phant', '{"es": "Elefante Bollywood", "pt-br": "Elefante Mordomo"}', '{"es": "¿Qué te apetece?", "pt-br": "Não alimente os animais"}'),
    ROW('bolly_pillow', '{"es": "Almohadón Bollywood", "pt-br": "Almofada Bolly"}', '{"es": "¡Desprende amor!", "pt-br": "Para estrelas de cinema"}'),
    ROW('bolly_swing', '{"es": "Columpio", "pt-br": "Balanço"}', '{"es": "¡Que te empuje el amor!", "pt-br": "Dá uma empurradinha aqui"}'),
    ROW('bolly_table', '{"es": "Mesa Bollywood", "pt-br": "Mesa Bolly"}', '{"es": "¡Desprende amor!", "pt-br": "Exótica e original"}'),
    ROW('bolly_tile1', '{"es": "Alfombra Bollywood 1", "pt-br": "Piso Bolly Losango"}', '{"es": "¡Camino al amor!", "pt-br": "Um toque indiano"}'),
    ROW('bolly_tile2', '{"es": "Alfombra Bollywood 2", "pt-br": "Piso Bolly Bolinhas"}', '{"es": "¡Camino al amor!", "pt-br": "Mantenha os pés no chão"}'),
    ROW('bolly_tree', '{"es": "Árbol Bollywood", "pt-br": "Palmeira Bolly"}', '{"es": "¡Da una sombra romántica!", "pt-br": "Quer sombra e água fresca?"}'),
    ROW('bolly_vase', '{"es": "Jarrón Bollywood", "pt-br": "Vaso de Flores"}', '{"es": "¡Desprende amor!", "pt-br": "Uma flor para uma flor"}'),
    ROW('bolly_wdw_wd', '{"es": "Ventana Bollywood", "pt-br": "Janela Indiana"}', '{"es": "Contempla una escena bucólica", "pt-br": "Um toque das terras distantes"}'),
    ROW('bottle', '{"es": "Botella", "pt-br": "Garrafa Vazia Giratória"}', '{"es": "Gira en busca de la perfección", "pt-br": "Para jogos interessantes!"}'),
    ROW('bump_lights', '{"es": "Semáforo Carreras", "pt-br": "Semáforo"}', '{"es": "3, 2, 1... ¡ya!", "pt-br": "Não passe no vermelho hein?"}'),
    ROW('bump_road', '{"es": "Pista de Carreras", "pt-br": "Pista de Asfalto"}', '{"es": "¿Listo para ganar?", "pt-br": "Sem buracos no caminho"}'),
    ROW('bump_signs', '{"es": "Señales", "pt-br": "Placa de trânsito"}', '{"es": "A ver, a ver, qué dicen...", "pt-br": "Respeite os sinais de trânsito."}'),
    ROW('bump_tires', '{"es": "Neumáticos", "pt-br": "Pneus"}', '{"es": "Resisten cualquier impacto", "pt-br": "Sempre é bom ter um reserva"}'),
    ROW('bump_tottero', '{"es": "Cono", "pt-br": "Cone de Segurança"}', '{"es": "¡Ojo con pisarlo!", "pt-br": "Sempre útil"}'),
    ROW('bunny', '{"es": "Conejo de Pascua", "pt-br": "Coelho Fofinho"}', '{"es": "Achuchable", "pt-br": "Para você cuidar e abraçar"}'),
    ROW('byesw_hand', '{"es": "Estatua Habbo Mano", "pt-br": "Assento Mão Gigante"}', '{"es": "¡En su memoria!", "pt-br": "Em memória daquela que sempre cuidou de nossos Mobis"}'),
    ROW('byesw_hotel', '{"es": "Vista Previa", "pt-br": "Maquete do Hotel"}', '{"es": "¡Así era antes!", "pt-br": "Em memória dos bons tempos da arquitetura de pixels"}'),
    ROW('byesw_loadscreen', '{"es": "Pantalla Memorial", "pt-br": "Tela Carregando"}', '{"es": "¡Adiós al Antiguo Habbo!", "pt-br": "Sempre dava um friozinho na barriga quando nessa tela aparecia a palavra Carregando..."}'),
    ROW('calippo', '{"es": "Heladera Calippo", "pt-br": "Calippo icecream machine"}', '{"es": "¡El mejor de los helados!", "pt-br": "Basic model"}'),
    ROW('camera', '{"es": "La Cámara", "pt-br": "Câmera"}', '{"es": "Pa-ta-ta, Pa-ta-ta", "pt-br": "Sorria!"}'),
    ROW('campfire', '{"es": "Fuego", "pt-br": "Fogueira"}', '{"es": "¡A ver si entramos en calor!", "pt-br": "Quer peixe frito?"}'),
    ROW('carpet_armas', '{"es": "Alfombra artesanal", "pt-br": "Tapete Artesanal"}', '{"es": "Da a tu Sala algo de calor", "pt-br": "Proporciona calor instantâneo"}'),
    ROW('carpet_legocourt', '{"es": "El Campo", "pt-br": "Quadra de Basquete"}', '{"es": "Ataque o defensa", "pt-br": "Demarque sua área para enterrar a bola"}'),
    ROW('carpet_polar', '{"es": "Falso oso", "pt-br": "Tapete Sintético"}', '{"es": "Para empezar a hacer el oso", "pt-br": "Para ficar aconchegante"}'),
    ROW('carpet_polar*1', '{"es": "Falso oso", "pt-br": "Tapete de Urso Rosa"}', '{"es": "Chulo", "pt-br": "Fofo"}'),
    ROW('carpet_polar*2', '{"es": "Falso oso", "pt-br": "Tapete de Urso Azul"}', '{"es": "Para empezar a hacer el oso", "pt-br": "Aconchego em um tapete de urso fashion"}'),
    ROW('carpet_polar*3', '{"es": "Falso oso", "pt-br": "Tapete de Urso Amarelo"}', '{"es": "Para empezar a hacer el oso", "pt-br": "Aconchego em um tapete de urso fashion"}'),
    ROW('carpet_polar*4', '{"es": "Falso oso", "pt-br": "Tapete de Urso Verde"}', '{"es": "Para empezar a hacer el oso", "pt-br": "Aconchego em um tapete de urso fashion"}'),
    ROW('carpet_soft', '{"es": "Alfombra de lana blanda", "pt-br": "Tapete de Lã Felpudo"}', '{"es": "Alfombra de lana blanda", "pt-br": "Tapete de Lã Felpudo"}'),
    ROW('carpet_soft*1', '{"es": "Alfombra de lana blanda", "pt-br": "Tapete de Lã Felpudo"}', '{"es": "Alfombra de lana blanda", "pt-br": "Tapete de Lã Felpudo"}'),
    ROW('carpet_soft*2', '{"es": "Alfombra de lana blanda", "pt-br": "Tapete de Lã Felpudo"}', '{"es": "Alfombra de lana blanda", "pt-br": "Tapete de Lã Felpudo"}'),
    ROW('carpet_soft*3', '{"es": "Alfombra de lana blanda", "pt-br": "Tapete de Lã Felpudo"}', '{"es": "Alfombra de lana blanda", "pt-br": "Tapete de Lã Felpudo"}'),
    ROW('carpet_soft*4', '{"es": "Alfombra de lana blanda", "pt-br": "Tapete de Lã Felpudo"}', '{"es": "Alfombra de lana blanda", "pt-br": "Tapete de Lã Felpudo"}'),
    ROW('carpet_soft*5', '{"es": "Alfombra de lana blanda", "pt-br": "Tapete de Lã Felpudo"}', '{"es": "Alfombra de lana blanda", "pt-br": "Tapete de Lã Felpudo"}'),
    ROW('carpet_soft*6', '{"es": "Alfombra de lana blanda", "pt-br": "Tapete de Lã Felpudo"}', '{"es": "Alfombra de lana blanda", "pt-br": "Tapete de Lã Felpudo"}'),
    ROW('carpet_soft_tut', '{"es": "Felpudo de Bienvenida", "pt-br": "Sinta-se em Casa"}', '{"es": "Mi casa, tu casa", "pt-br": "O Habbo Hotel te dá as boas-vindas"}'),
    ROW('carpet_standard', '{"es": "Alfombra", "pt-br": "Tapete de chão"}', '{"es": "Disponible en varios colores", "pt-br": "Disponível em várias cores"}'),
    ROW('carpet_standard*1', '{"es": "Alfombra", "pt-br": "Tapete de Chão Largo"}', '{"es": "Disponible en varios colores", "pt-br": "Disponível em várias cores"}'),
    ROW('carpet_standard*10', '{"es": "Alfombra", "pt-br": "Tapete de Chão Largo"}', '{"es": "Disponible en varios colores", "pt-br": "Disponível em várias cores"}'),
    ROW('carpet_standard*11', '{"es": "Alfombra", "pt-br": "Tapete de Chão Largo"}', '{"es": "Disponible en varios colores", "pt-br": "Disponível em várias cores"}'),
    ROW('carpet_standard*2', '{"es": "Alfombra", "pt-br": "Tapete de Chão Largo"}', '{"es": "Disponible en varios colores", "pt-br": "Disponível em várias cores"}'),
    ROW('carpet_standard*3', '{"es": "Alfombra", "pt-br": "Tapete de Chão Largo"}', '{"es": "Disponible en varios colores", "pt-br": "Disponível em várias cores"}'),
    ROW('carpet_standard*4', '{"es": "Alfombra", "pt-br": "Tapete de Chão Largo"}', '{"es": "Disponible en varios colores", "pt-br": "Disponível em várias cores"}'),
    ROW('carpet_standard*5', '{"es": "Alfombra", "pt-br": "Tapete de Chão Largo"}', '{"es": "Disponible en varios colores", "pt-br": "Disponível em várias cores"}'),
    ROW('carpet_standard*6', '{"es": "Alfombra", "pt-br": "Tapete de Chão Largo"}', '{"es": "Disponible en varios colores", "pt-br": "Disponível em várias cores"}'),
    ROW('carpet_standard*7', '{"es": "Alfombra", "pt-br": "Tapete de Chão Largo"}', '{"es": "Disponible en varios colores", "pt-br": "Disponível em várias cores"}'),
    ROW('carpet_standard*8', '{"es": "Alfombra", "pt-br": "Tapete de Chão Largo"}', '{"es": "Disponible en varios colores", "pt-br": "Disponível em várias cores"}'),
    ROW('carpet_standard*9', '{"es": "Alfombra", "pt-br": "Tapete de Chão Largo"}', '{"es": "Disponible en varios colores", "pt-br": "Disponível em várias cores"}'),
    ROW('carpet_valentine', '{"es": "Alfombra Roja", "pt-br": "Tapete do Amor"}', '{"es": "Para una aparición estelar", "pt-br": "Pisa em quem só te dá carinho"}'),
    ROW('chair_basic*1', '{"es": "Silla Huevo Aguamarina", "pt-br": "Cadeira Pura Aquamarine"}', '{"es": "Diseño revolucionario", "pt-br": "Conforto Ovíparo."}'),
    ROW('chair_basic*2', '{"es": "Silla Huevo Rosa", "pt-br": "Cadeira Seda Teens"}', '{"es": "¡Para que encajes bien en todas partes!", "pt-br": "Para aquelas que gostam de brilhar!"}'),
    ROW('chair_basic*3', '{"es": "Silla Huevo Negra", "pt-br": "Cadeira Pura Preta"}', '{"es": "¡Para que encajes bien en todas partes!", "pt-br": "Conforto para Habbos obscuros"}'),
    ROW('chair_basic*4', '{"es": "Silla Huevo Blanca", "pt-br": "Cadeira Pura Branca"}', '{"es": "¡Para que encajes bien en todas partes!", "pt-br": "Branca como um ovo"}'),
    ROW('chair_basic*5', '{"es": "Silla Huevo Beige", "pt-br": "Cadeira Pura"}', '{"es": "Diseño revolucionario", "pt-br": "Conforto Ovíparo"}'),
    ROW('chair_basic*6', '{"es": "Silla Huevo Azul", "pt-br": "Cadeira Pura Azul"}', '{"es": "¡Diseño ideal!", "pt-br": "Conforto Ovíparo"}'),
    ROW('chair_basic*7', '{"es": "Silla Huevo Verde", "pt-br": "Cadeira Botânica"}', '{"es": "¡Para que encajes bien en todas partes!", "pt-br": "Conforto Ovíparo"}'),
    ROW('chair_basic*8', '{"es": "Silla Huevo Amarilla", "pt-br": "Cadeira Ovo"}', '{"es": "¡Diseño espectacular!", "pt-br": "Conforto Ovíparo"}'),
    ROW('chair_basic*9', '{"es": "Silla Huevo Roja", "pt-br": "Cadeira Aqua"}', '{"es": "¡Para que encajes bien en todas partes!", "pt-br": "Conforto Ovíparo"}'),
    ROW('chair_china', '{"es": "Silla China", "pt-br": "Cadeira de Bambu"}', '{"es": "La elegante belleza de la tradición", "pt-br": "Elegante mesmo."}'),
    ROW('chair_norja', '{"es": "Silla", "pt-br": "Cadeira Iced Bege"}', '{"es": "Lisa y graciosa", "pt-br": "Estilo e elegância para todos os gostos"}'),
    ROW('chair_norja*2', '{"es": "Silla Negra", "pt-br": "Cadeira Iced Preta"}', '{"es": "Pura elegancia", "pt-br": "Fino e elegante"}'),
    ROW('chair_norja*3', '{"es": "Silla Blanca", "pt-br": "Cadeira Iced Branca"}', '{"es": "Pura elegancia", "pt-br": "Fino e elegante"}'),
    ROW('chair_norja*4', '{"es": "Silla Aguamarina", "pt-br": "Cadeira Iced Água-marinha"}', '{"es": "Pura elegancia", "pt-br": "Fino e elegante"}'),
    ROW('chair_norja*5', '{"es": "Silla Rosa", "pt-br": "Cadeira Iced Rosa"}', '{"es": "Pura elegancia", "pt-br": "Fino e elegante"}'),
    ROW('chair_norja*6', '{"es": "Silla Azul", "pt-br": "Cadeira Iced Azul"}', '{"es": "Pura elegancia", "pt-br": "Fino e elegante"}'),
    ROW('chair_norja*7', '{"es": "Silla Verde", "pt-br": "Cadeira Iced Verde"}', '{"es": "Pura elegancia", "pt-br": "Fino e elegante"}'),
    ROW('chair_norja*8', '{"es": "Silla Amarilla", "pt-br": "Cadeira Iced Amarela"}', '{"es": "Pura elegancia", "pt-br": "Fino e elegante"}'),
    ROW('chair_norja*9', '{"es": "Silla Roja", "pt-br": "Cadeira Iced Vermelha"}', '{"es": "Pura elegancia", "pt-br": "Coleção Iced Natal 2007"}'),
    ROW('chair_plasto', '{"es": "Silla", "pt-br": "Cadeira"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('chair_plasto*1', '{"es": "Silla", "pt-br": "Cadeira"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('chair_plasto*10', '{"es": "Silla", "pt-br": "Cadeira"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('chair_plasto*11', '{"es": "Silla", "pt-br": "Cadeira"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('chair_plasto*12', '{"es": "Silla", "pt-br": "Cadeira"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('chair_plasto*13', '{"es": "Silla", "pt-br": "Cadeira"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('chair_plasto*14', '{"es": "HC silla", "pt-br": "HC Cadeira"}', '{"es": "Azul intenso", "pt-br": "Cadeira aquática"}'),
    ROW('chair_plasto*15', '{"es": "Silla", "pt-br": "Cadeira"}', '{"es": "Cool y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('chair_plasto*16', '{"es": "Silla", "pt-br": "Cadeira"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('chair_plasto*2', '{"es": "Silla", "pt-br": "Cadeira"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('chair_plasto*3', '{"es": "Silla", "pt-br": "Cadeira"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('chair_plasto*4', '{"es": "Silla", "pt-br": "Cadeira"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('chair_plasto*5', '{"es": "Silla", "pt-br": "Cadeira"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('chair_plasto*6', '{"es": "Silla", "pt-br": "Cadeira"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('chair_plasto*7', '{"es": "Silla", "pt-br": "Cadeira"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('chair_plasto*8', '{"es": "Silla", "pt-br": "Cadeira"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('chair_plasto*9', '{"es": "Silla", "pt-br": "Cadeira"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('chair_plasty', '{"es": "Pod", "pt-br": "Pufe"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('chair_plasty*1', '{"es": "Pod", "pt-br": "Pufe"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('chair_plasty*10', '{"es": "Pod", "pt-br": "Pufe"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('chair_plasty*11', '{"es": "Pod", "pt-br": "Pufe"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('chair_plasty*2', '{"es": "Pod", "pt-br": "Pufe"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('chair_plasty*3', '{"es": "Pod", "pt-br": "Pufe"}', '{"es": "Una forma original y muy confortable", "pt-br": "Moderna peça de plástico"}'),
    ROW('chair_plasty*4', '{"es": "Pod", "pt-br": "Pufe"}', '{"es": "¡El Pod de Santa Rasta!", "pt-br": "Moderna peça de plástico"}')) AS tr (classname, names, descs)
  ON f.`classname` = tr.classname
SET f.`localized_name` = CAST(tr.names AS JSON), f.`localized_desc` = CAST(tr.descs AS JSON);
UPDATE `definition_furniture` f JOIN (VALUES
    ROW('chair_plasty*5', '{"es": "Pod", "pt-br": "Pufe"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('chair_plasty*6', '{"es": "Pod", "pt-br": "Pufe"}', '{"es": "¡El Pod de Juan Bling!", "pt-br": "Moderna peça de plástico"}'),
    ROW('chair_plasty*7', '{"es": "Pod", "pt-br": "Pufe"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('chair_plasty*8', '{"es": "Pod", "pt-br": "Pufe"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('chair_plasty*9', '{"es": "Pod", "pt-br": "Pufe"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('chair_polyfon', '{"es": "Silla de comedor", "pt-br": "Cadeira de Jantar"}', '{"es": "Un metal noble y agradable", "pt-br": "Uma experiência metálica no jantar"}'),
    ROW('chair_silo', '{"es": "Silla", "pt-br": "Cadeira de Jantar"}', '{"es": "El trono de los amigos", "pt-br": "Mantenha a simplicidade"}'),
    ROW('chair_silo*2', '{"es": "Silla Negra", "pt-br": "Preto Cadeira Jantar Silo"}', '{"es": "El trono de los amigos", "pt-br": "Preto Cadeira Jantar Silo"}'),
    ROW('chair_silo*3', '{"es": "Silla Blanca", "pt-br": "Cadeira de Jantar"}', '{"es": "El trono de los amigos", "pt-br": "Simples e prática"}'),
    ROW('chair_silo*4', '{"es": "Silla Beige", "pt-br": "Bege Cadeira Jantar Silo"}', '{"es": "El trono de los amigos", "pt-br": "Bege Cadeira Jantar Silo"}'),
    ROW('chair_silo*5', '{"es": "Silla Rosa", "pt-br": "Cadeira de Jantar Rosa"}', '{"es": "El trono de los amigos", "pt-br": "Sinta o poder do rosa"}'),
    ROW('chair_silo*6', '{"es": "Silla Azul", "pt-br": "Azul Cadeira Jantar Silo"}', '{"es": "El trono de los amigos", "pt-br": "Azul Cadeira Jantar Silo"}'),
    ROW('chair_silo*7', '{"es": "Silla Verde", "pt-br": "Verde Cadeira Jantar Silo"}', '{"es": "El trono de los amigos", "pt-br": "Verde Cadeira Jantar Silo"}'),
    ROW('chair_silo*8', '{"es": "Silla Amarilla", "pt-br": "Amarelo Cadeira Jantar Silo"}', '{"es": "El trono de los amigos", "pt-br": "Amarelo Cadeira Jantar Silo"}'),
    ROW('chair_silo*9', '{"es": "Silla Roja", "pt-br": "Vermelho Cadeira Jantar Silo"}', '{"es": "El trono de los amigos", "pt-br": "Vermelho Cadeira Jantar Silo"}'),
    ROW('china_light', '{"es": "Farolillo chino", "pt-br": "Luz da China"}', '{"es": "¡Luz estilo oriental!", "pt-br": "Iluminação do Oriente"}'),
    ROW('china_moongt', '{"es": "Pasadizo chino", "pt-br": "Portão Lua Cheia"}', '{"es": "¡Deja atrás los malos espíritus!", "pt-br": "Não precisa fazer esforço pra passar"}'),
    ROW('china_ox', '{"es": "Estatua Buey", "pt-br": "Estátua Ox"}', '{"es": "¡Huele a sabiduría!", "pt-br": "A culpa não foi da Ângela!"}'),
    ROW('china_plmTree', '{"es": "Árbol Chino", "pt-br": "Árvore da Vida"}', '{"es": "Florece en primavera", "pt-br": "Um negócio da China, uma chance única."}'),
    ROW('china_pstr1', '{"es": "Buey", "pt-br": "Cartaz de Boi"}', '{"es": "¡Feliz Año Nuevo Chino!", "pt-br": "Para um ano novo próspero"}'),
    ROW('china_pstr2', '{"es": "Dragón", "pt-br": "Cartaz de Dragão"}', '{"es": "¡Feliz Año Nuevo Chino!", "pt-br": "Não tenha medo"}'),
    ROW('china_pstr3', '{"es": "Póster Chino 3", "pt-br": "Pôster de Tigre"}', '{"es": "2010. El año del tigre", "pt-br": "Controle o tigre em você."}'),
    ROW('china_shelve', '{"es": "Estantería China", "pt-br": "Estante de Bambu"}', '{"es": "Para guardar los tesoros de la mente", "pt-br": "Riqueza Mental"}'),
    ROW('china_table', '{"es": "Mesa China", "pt-br": "Mesa de Bambu"}', '{"es": "Exótica y al alcance de todos", "pt-br": "Clássico"}'),
    ROW('china_tiger', '{"es": "Estatua Tigre", "pt-br": "Estátua de Tigre"}', '{"es": "2010. El año del tigre", "pt-br": "Modere o temperamento, cuidado com impulsos."}'),
    ROW('china_tigrSeat', '{"es": "Asiento Tigre", "pt-br": "Tigre Sentado"}', '{"es": "2010. El año del tigre", "pt-br": "Agressividade controlada. Observação e paciência."}'),
    ROW('christmas_poop', '{"es": "Truño", "pt-br": "Adubo Milagroso"}', '{"es": "¡Ojo que casi lo pisas!", "pt-br": "Tudo na fertilidade e abundância"}'),
    ROW('christmas_reindeer', '{"es": "Reno", "pt-br": "Rena"}', '{"es": "El más rápido sobre ruedas", "pt-br": "Tão viva que quase parece voar"}'),
    ROW('christmas_sleigh', '{"es": "Trineo", "pt-br": "Trenó"}', '{"es": "¿Cabrán en él todos los regalos?", "pt-br": "Meio de transporte, quase inteiro."}'),
    ROW('clrack', '{"es": "Vestuario y Perchas", "pt-br": "Cabide"}', '{"es": "¿No sabes qué ponerte?", "pt-br": "Escolha seu melhor visual"}'),
    ROW('club_sofa', '{"es": "El Club Sofa", "pt-br": "Clube Sofá"}', '{"es": "Posaderas HC", "pt-br": "Sofá do clube"}'),
    ROW('cn_lamp', '{"es": "Linterna China", "pt-br": "Lanterna Chinesa"}', '{"es": "¡Una luz suave para tu Sala!", "pt-br": "Suave atmosfera luminosa"}'),
    ROW('cn_sofa', '{"es": "Sofá Chino Lacado", "pt-br": "Sofá de Bambu"}', '{"es": "La belleza elegante de la tradición", "pt-br": "Elegância tradicional"}'),
    ROW('corner_china', '{"es": "Dragón Tatuado", "pt-br": "Canto do Dragão"}', '{"es": "Calefacción incorporada", "pt-br": "À prova de fogo"}'),
    ROW('couch_norja', '{"es": "Banco", "pt-br": "Banco Iced Bege"}', '{"es": "Pruébalo con tu amigo/a", "pt-br": "Para dois com conforto"}'),
    ROW('couch_norja*2', '{"es": "Banco Negro", "pt-br": "Banco Iced Preto"}', '{"es": "Pruébalo con tu amigo/a", "pt-br": "Confortável para dois"}'),
    ROW('couch_norja*3', '{"es": "Banco Blanco", "pt-br": "Banco Iced Branco"}', '{"es": "Pruébalo con tu amigo/a", "pt-br": "Confortável para dois"}'),
    ROW('couch_norja*4', '{"es": "Banco Aguamarina", "pt-br": "Banco Iced Água-marinha"}', '{"es": "Pruébalo con tu amigo/a", "pt-br": "Confortável para dois"}'),
    ROW('couch_norja*5', '{"es": "Banco Rosa", "pt-br": "Banco Iced Rosa"}', '{"es": "Pruébalo con tu amigo/a", "pt-br": "Confortável para dois"}'),
    ROW('couch_norja*6', '{"es": "Banco Azul", "pt-br": "Banco Iced Azul"}', '{"es": "Pruébalo con tu amigo/a", "pt-br": "Confortável para dois"}'),
    ROW('couch_norja*7', '{"es": "Banco Verde", "pt-br": "Banco Iced Verde"}', '{"es": "Pruébalo con tu amigo/a", "pt-br": "Confortável para dois"}'),
    ROW('couch_norja*8', '{"es": "Banco Amarillo", "pt-br": "Banco Iced Amarelo"}', '{"es": "Pruébalo con tu amigo/a", "pt-br": "Confortável para dois"}'),
    ROW('couch_norja*9', '{"es": "Banco Rojo", "pt-br": "Banco Iced Vermelho"}', '{"es": "Pruébalo con tu amigo/a", "pt-br": "Coleção Iced Natal 2007"}'),
    ROW('country_corner', '{"es": "Esquina Riachuelo", "pt-br": "Curva de riacho"}', '{"es": "Va cargado de agua", "pt-br": "É melhor se segurar nas curvas"}'),
    ROW('country_ditch', '{"es": "Riachuelo", "pt-br": "Riacho"}', '{"es": "No es que venga cargado de agua, no...", "pt-br": "Uma pescaria é sempre bem-vinda"}'),
    ROW('country_fnc1', '{"es": "Valla Campo", "pt-br": "Cerca de madeira"}', '{"es": "De madera de la buena", "pt-br": "Ecologicamente correta"}'),
    ROW('country_fnc2', '{"es": "Muro Piedra", "pt-br": "Muro de pedra"}', '{"es": "Resistente al máximo", "pt-br": "Firme como uma rocha"}'),
    ROW('country_fnc3', '{"es": "Esquina Piedra", "pt-br": "Canto de pedra"}', '{"es": "Bien robusta", "pt-br": "Uma pausa para pensar"}'),
    ROW('country_forestwall', '{"es": "Bosquecillo", "pt-br": "Parede Florestal"}', '{"es": "¡Apenas se ve la luz!", "pt-br": "Mata adentro"}'),
    ROW('country_fp', '{"es": "Chimenea Campestre", "pt-br": "Lareira"}', '{"es": "¡Que calorcito!", "pt-br": "Quero que você me aqueça nesse inverno..."}'),
    ROW('country_gate', '{"es": "Puerta Redil", "pt-br": "Porteira"}', '{"es": "Típicamente campestre", "pt-br": "Abre a porta, Mariquinha"}'),
    ROW('country_grass', '{"es": "Césped Campestre", "pt-br": "Grama"}', '{"es": "¡Bien mullidito!", "pt-br": "Verdinha e macia"}'),
    ROW('country_lantern', '{"es": "Candil Campestre", "pt-br": "Lampião de fazenda"}', '{"es": "Viene muy bien para las noches oscuras", "pt-br": "Movido a querosene"}'),
    ROW('country_log', '{"es": "Tronco", "pt-br": "Tronco de árvore"}', '{"es": "Duro y resistente", "pt-br": "Conte histórias de terror"}'),
    ROW('country_patio', '{"es": "Tablón", "pt-br": "Ponte de madeira"}', '{"es": "De buena madera", "pt-br": "Não é aquela do rio que cai"}'),
    ROW('country_rain', '{"es": "Lluvia", "pt-br": "Chuva"}', '{"es": "Imposible no mojarse", "pt-br": "Nem precisa pedir pra São Pedro"}'),
    ROW('country_rbw', '{"es": "Arcoiris", "pt-br": "Arco-íris"}', '{"es": "Un espectáculo de color", "pt-br": "No final, tem um pote de ouro!"}'),
    ROW('country_scarecrow', '{"es": "Espantapájaros", "pt-br": "Espantalho"}', '{"es": "¡Tiene un aspecto terrible!", "pt-br": "Irmão-gêmeo da Acapulca"}'),
    ROW('country_soil', '{"es": "Terrenito", "pt-br": "Plantação"}', '{"es": "¿Qué piensas plantar en él?", "pt-br": "Solo fértil e novo"}'),
    ROW('country_stage', '{"es": "Palé", "pt-br": "Palco de quermesse"}', '{"es": "¡Yeehaw!", "pt-br": "Dá uma subidinha"}'),
    ROW('country_trctr', '{"es": "Tractor", "pt-br": "Trator"}', '{"es": "Una pieza indispensable para el campo", "pt-br": "Arando o terreno de pixels"}'),
    ROW('country_wall', '{"es": "Pared Cuadra", "pt-br": "Parede de fazenda"}', '{"es": "De estilo decimonónico", "pt-br": "Estilo rural no seu Quarto"}'),
    ROW('country_well', '{"es": "Pozo", "pt-br": "Poço dos Desejos"}', '{"es": "Contiene agua muy fresquita", "pt-br": "Eu desejo ter um desses!"}'),
    ROW('country_wheat', '{"es": "Trigo", "pt-br": "Plantação de trigo"}', '{"es": "¡Hay que ver cómo ha crecido!", "pt-br": "Seu pão quentinho nasce aqui"}'),
    ROW('crystal_patch', '{"es": "Cristalina", "pt-br": "Piso de Cristal"}', '{"es": "¿Ahuyentarás así a los superhéroes?", "pt-br": "Pra iluminar o chão em que pisa"}'),
    ROW('deadduck', '{"es": "Papá Pato de Halloween", "pt-br": "Pato Morto"}', '{"es": "Se parecen mucho en la familia", "pt-br": "É tudo brincadeirinha."}'),
    ROW('deadduck2', '{"es": "Mamá Pato de Habboween", "pt-br": "Pato Morto 2"}', '{"es": "Se parecen mucho en la familia", "pt-br": "Alguém se esqueceu de me alimentar..."}'),
    ROW('deadduck3', '{"es": "Hijo Pato de Habboween", "pt-br": "Pato Morto 3"}', '{"es": "Se parecen mucho en la familia", "pt-br": "Com um toque de ectoplasma"}'),
    ROW('det_bhole', '{"es": "Rotura", "pt-br": "Buraco de Investigação"}', '{"es": "¿Quién habrá sido?", "pt-br": "Não chegue muito perto"}'),
    ROW('det_body', '{"es": "Contorno de tiza", "pt-br": "Silhueta de Corpo"}', '{"es": "El cuerpo del crimen", "pt-br": "Não pise aqui"}'),
    ROW('det_divider', '{"es": "Cinta policial", "pt-br": "Divisória contra Bobba"}', '{"es": "¡No pasar!", "pt-br": "Não ultrapasse o limite"}'),
    ROW('dimmer_buttn', '{"es": "Interruptor 1", "pt-br": "Botão Regulador"}', '{"es": "¡Da la luz!", "pt-br": "Todo mundo devia ter um desses"}'),
    ROW('dimmer_fuse2', '{"es": "Caja Fusibles 1", "pt-br": "Controlador de Luzes"}', '{"es": "¡Da la luz!", "pt-br": "Apenas aperte o botão"}'),
    ROW('dimmer_fuse6', '{"es": "Caja Fusibles 2", "pt-br": "Caixa de Fusíveis"}', '{"es": "¡Da la luz!", "pt-br": "A luz sempre acaba na hora do jogo"}'),
    ROW('dimmer_swtch', '{"es": "Interruptor 2", "pt-br": "Tomada Clássica"}', '{"es": "¡Da la luz!", "pt-br": "Sempre bom economizar energia"}'),
    ROW('diner_bardesk*1', '{"es": "Barra Diner Aqua", "pt-br": "Balcão Retrô Aquamarine"}', '{"es": "Los productos más frescos, aquí", "pt-br": "Separando a clientela"}'),
    ROW('diner_bardesk*2', '{"es": "Barra Diner Rosa", "pt-br": "Balcão Retrô Rosa"}', '{"es": "¡Dura y resistente!", "pt-br": "Separando a clientela"}'),
    ROW('diner_bardesk*3', '{"es": "Barra Diner Negra", "pt-br": "Balcão Retrô Preto"}', '{"es": "¡Dura y resistente!", "pt-br": "Separando a clientela"}'),
    ROW('diner_bardesk*4', '{"es": "Barra Diner Blanca", "pt-br": "Divisória Retrô Branca"}', '{"es": "Los productos más frescos, aquí", "pt-br": "Clientes de um lado, garçonetes do outro"}'),
    ROW('diner_bardesk*5', '{"es": "Barra Diner Beige", "pt-br": "Bar Bege"}', '{"es": "Los productos más frescos, aquí", "pt-br": "Separando a clientela"}'),
    ROW('diner_bardesk*6', '{"es": "Barra Diner Azul", "pt-br": "Balcão Retrô Azul"}', '{"es": "Los productos más frescos, aquí", "pt-br": "Separando a clientela"}'),
    ROW('diner_bardesk*7', '{"es": "Barra Diner Verde", "pt-br": "Balcão Retrô Verde"}', '{"es": "¡Dura y resistente!", "pt-br": "Separando a clientela"}'),
    ROW('diner_bardesk*8', '{"es": "Barra Diner Amarilla", "pt-br": "Balcão Retrô Amarelo"}', '{"es": "Los productos más frescos, aquí", "pt-br": "Separando a clientela"}'),
    ROW('diner_bardesk*9', '{"es": "Barra Diner Roja", "pt-br": "Balcão Retrô Vermelho"}', '{"es": "¡Dura y resistente!", "pt-br": "Separando a clientela"}'),
    ROW('diner_bardesk_corner*1', '{"es": "Esquina Diner Aqua", "pt-br": "Canto de Balcão retrô Aquamarine"}', '{"es": "Los productos más frescos, aquí", "pt-br": "Cuidado com os cantos pontiagudos"}'),
    ROW('diner_bardesk_corner*2', '{"es": "Esquina Diner Rosa", "pt-br": "Canto de Balcão Retrô Rosa"}', '{"es": "Los productos más frescos, aquí", "pt-br": "Cuidado com os cantos pontiagudos"}'),
    ROW('diner_bardesk_corner*3', '{"es": "Esquina Diner Negra", "pt-br": "Canto de Balcão Retrô Preto"}', '{"es": "Los productos más frescos, aquí", "pt-br": "Cuidado com os cantos pontiagudos"}'),
    ROW('diner_bardesk_corner*4', '{"es": "Esquina Diner Blanca", "pt-br": "Canto de Balcão Retrô Branco"}', '{"es": "Los productos más frescos, aquí", "pt-br": "Para montar o seu restaurante retrô"}'),
    ROW('diner_bardesk_corner*5', '{"es": "Esquina Diner Beige", "pt-br": "Canto Bege"}', '{"es": "Los productos más frescos, aquí", "pt-br": "Cuidado com os cantos pontiagudos"}'),
    ROW('diner_bardesk_corner*6', '{"es": "Esquina Diner Azul", "pt-br": "Canto de Balcão Retrô Azul"}', '{"es": "Los productos más frescos, aquí", "pt-br": "Cuidado com os cantos pontiagudos"}'),
    ROW('diner_bardesk_corner*7', '{"es": "Esquina Diner Verde", "pt-br": "Canto de Balcão Retrô Verde"}', '{"es": "Los productos más frescos, aquí", "pt-br": "Cuidado com os cantos pontiagudos"}'),
    ROW('diner_bardesk_corner*8', '{"es": "Esquina Diner Amarilla", "pt-br": "Canto de Balcão Retrô Amarelo"}', '{"es": "Los productos más frescos, aquí", "pt-br": "Cuidado com os cantos pontiagudos"}'),
    ROW('diner_bardesk_corner*9', '{"es": "Esquina Diner Roja", "pt-br": "Canto de Balcão Retrô Vermelho"}', '{"es": "Los productos más frescos, aquí", "pt-br": "Cuidado com os cantos pontiagudos"}'),
    ROW('diner_bardesk_gate*1', '{"es": "Trampa Diner Aqua", "pt-br": "Portão Retrô Aquamarine"}', '{"es": "Pasa antes de que se cierre", "pt-br": "Um de cada vez"}'),
    ROW('diner_bardesk_gate*2', '{"es": "Trampa Diner Rosa", "pt-br": "Portão Retrô Rosa"}', '{"es": "Pasa antes de que se cierre", "pt-br": "Um de cada vez"}'),
    ROW('diner_bardesk_gate*3', '{"es": "Trampa Diner Negra", "pt-br": "Portão Retrô Preto"}', '{"es": "Pasa antes de que se cierre", "pt-br": "Um de cada vez"}'),
    ROW('diner_bardesk_gate*4', '{"es": "Trampa Diner Blanca", "pt-br": "Portão Restaurante Retrô Branco"}', '{"es": "Pasa antes de que se cierre", "pt-br": "Um milkshake na mesa 10, saindo"}'),
    ROW('diner_bardesk_gate*5', '{"es": "Trampa Diner Beige", "pt-br": "Portão Bege"}', '{"es": "Pasa antes de que se cierre", "pt-br": "Um de cada vez"}'),
    ROW('diner_bardesk_gate*6', '{"es": "Trampa Diner Azul", "pt-br": "Portão Retrô Azul"}', '{"es": "¡Dura y resistente!", "pt-br": "Um de cada vez"}'),
    ROW('diner_bardesk_gate*7', '{"es": "Trampa Diner Verde", "pt-br": "Portão Retrô Verde"}', '{"es": "Pasa antes de que se cierre", "pt-br": "Um de cada vez"}'),
    ROW('diner_bardesk_gate*8', '{"es": "Trampa Diner Amarilla", "pt-br": "Portão Retrô Amarelo"}', '{"es": "Pasa antes de que se cierre", "pt-br": "Um de cada vez"}'),
    ROW('diner_bardesk_gate*9', '{"es": "Trampa Diner Roja", "pt-br": "Portão Retrô Vermelho"}', '{"es": "Pasa antes de que se cierre", "pt-br": "Um de cada vez"}'),
    ROW('diner_cashreg*1', '{"es": "Caja Registradora Aqua", "pt-br": "Caixa Registradora Aquamarine"}', '{"es": "¿Tienes cambio?", "pt-br": "De olho no troco"}'),
    ROW('diner_cashreg*2', '{"es": "Caja Registradora Rosa", "pt-br": "Caixa Registradora Rosa"}', '{"es": "¿Tienes cambio?", "pt-br": "De olho no troco"}'),
    ROW('diner_cashreg*3', '{"es": "Caja Registradora Negra", "pt-br": "Caixa Registradora Preta"}', '{"es": "¿Tienes cambio?", "pt-br": "De olho no troco"}'),
    ROW('diner_cashreg*4', '{"es": "Caja Registradora Blanca", "pt-br": "Caixa Registradora Branca"}', '{"es": "¿Tienes cambio?", "pt-br": "Dinheiro na mão é vendaval."}'),
    ROW('diner_cashreg*5', '{"es": "Caja Registradora Beige", "pt-br": "Caixa Registradora Bege"}', '{"es": "¡Guarda un tesoro!", "pt-br": "De olho no troco"}'),
    ROW('diner_cashreg*6', '{"es": "Caja Registradora Azul", "pt-br": "Caixa Registradora Azul"}', '{"es": "¿Tienes cambio?", "pt-br": "De olho no troco"}'),
    ROW('diner_cashreg*7', '{"es": "Caja Registradora Verde", "pt-br": "Caixa Registradora Verde"}', '{"es": "¿Tienes cambio?", "pt-br": "De olho no troco"}'),
    ROW('diner_cashreg*8', '{"es": "Caja Registradora Amarilla", "pt-br": "Caixa Registradora Amarela"}', '{"es": "¿Tienes cambio?", "pt-br": "Da cor da sorte"}'),
    ROW('diner_cashreg*9', '{"es": "Caja Registradora Roja", "pt-br": "Caixa Registradora Vermelha"}', '{"es": "¿Tienes cambio?", "pt-br": "De olho no troco"}'),
    ROW('diner_chair*1', '{"es": "Taburete Diner Aqua", "pt-br": "Cadeira Retrô Aquamarine"}', '{"es": "¡La mar de confortable!", "pt-br": "Sente no clima de antigamente"}'),
    ROW('diner_chair*2', '{"es": "Taburete Diner Rosa", "pt-br": "Cadeira Retrô Rosa"}', '{"es": "¡La mar de confortable!", "pt-br": "Sente no clima de antigamente"}'),
    ROW('diner_chair*3', '{"es": "Taburete Diner Negro", "pt-br": "Cadeira Retrô Preta"}', '{"es": "¡La mar de confortable!", "pt-br": "Sente no clima de antigamente"}'),
    ROW('diner_chair*4', '{"es": "Taburete Diner Blanco", "pt-br": "Banco Restaurante Retrô Branco"}', '{"es": "¡La mar de confortable!", "pt-br": "Todo o estilo dos anos 50"}'),
    ROW('diner_chair*5', '{"es": "Taburete Diner Beige", "pt-br": "Banco Bege"}', '{"es": "La mar de confortable", "pt-br": "Sente no clima de antigamente"}'),
    ROW('diner_chair*6', '{"es": "Taburete Diner Azul", "pt-br": "Cadeira Retrô Azul"}', '{"es": "¡La mar de confortable!", "pt-br": "Sente no clima de antigamente"}'),
    ROW('diner_chair*7', '{"es": "Taburete Diner Verde", "pt-br": "Cadeira Retrô Verde"}', '{"es": "¡La mar de confortable!", "pt-br": "Sente no clima de antigamente"}'),
    ROW('diner_chair*8', '{"es": "Taburete Diner Amarilla", "pt-br": "Cadeira Retrô Amarela"}', '{"es": "¡La mar de confortable!", "pt-br": "Sente no clima de antigamente"}'),
    ROW('diner_chair*9', '{"es": "Taburete Diner Rojo", "pt-br": "Cadeira Retrô Vermelha"}', '{"es": "¡La mar de confortable!", "pt-br": "Sente no clima de antigamente"}'),
    ROW('diner_gumvendor*1', '{"es": "Máquina de Chicles Aqua", "pt-br": "Máquina de Chicletes Aquamarine"}', '{"es": "¡Los mejores chicles de Habbo!", "pt-br": "Depois tem que escovar os dentes!"}'),
    ROW('diner_gumvendor*2', '{"es": "Máquina de chicles Rosa", "pt-br": "Máquina de Chicletes Rosa"}', '{"es": "¡Los mejores chicles de Habbo!", "pt-br": "Depois tem que escovar os dentes!"}'),
    ROW('diner_gumvendor*3', '{"es": "Máquina de chicles Negra", "pt-br": "Máquina de Chicletes Preta"}', '{"es": "¡El más dulce de los inventos!", "pt-br": "Depois tem que escovar os dentes!"}'),
    ROW('diner_gumvendor*4', '{"es": "Máquina de chicles Blanca", "pt-br": "Máquina de Chicletes Branca"}', '{"es": "¡Los mejores chicles de Habbo!", "pt-br": "Depois do doce, escove os dentes,"}'),
    ROW('diner_gumvendor*5', '{"es": "Máquina de chicles Beige", "pt-br": "Máqui na de Chiclete Bege"}', '{"es": "¡Los mejores chicles de Habbo!", "pt-br": "Depois tem que escovar os dentes!"}'),
    ROW('diner_gumvendor*6', '{"es": "Máquina de chicles Azul", "pt-br": "Máquina de Chicletes Azul"}', '{"es": "¡El más dulce de los inventos!", "pt-br": "Depois tem que escovar os dentes!"}'),
    ROW('diner_gumvendor*7', '{"es": "Máquina de chicles Verde", "pt-br": "Máquina de Chicletes Verde"}', '{"es": "¡El más dulce de los inventos!", "pt-br": "Depois tem que escovar os dentes!"}'),
    ROW('diner_gumvendor*8', '{"es": "Máquina de chicles Amarilla", "pt-br": "Máquina de Chicletes Amarela"}', '{"es": "¡Los mejores chicles de Habbo!", "pt-br": "Quer uma bala de limão?"}'),
    ROW('diner_gumvendor*9', '{"es": "Máquina de chicles Roja", "pt-br": "Máquina de Chicletes Vermelha"}', '{"es": "¡El más dulce de los inventos!", "pt-br": "Depois tem que escovar os dentes!"}'),
    ROW('diner_poster', '{"es": "Póster Diner", "pt-br": "Pôster Retrô"}', '{"es": "Los mayores manjares, aquí", "pt-br": "Estilo que sobrevive ao tempo"}'),
    ROW('diner_rug', '{"es": "Alfombra Diner", "pt-br": "Piso Retrô"}', '{"es": "¡Por un mundo ajedrezado!", "pt-br": "Dá até pra jogar damas"}'),
    ROW('diner_shaker', '{"es": "Batidora", "pt-br": "Máquina de Milk-Shake"}', '{"es": "¡Hace las bebidas más cremosas de Habbo!", "pt-br": "Sempre uma boa pedida"}'),
    ROW('diner_sofa_1*1', '{"es": "Sofá Diner Aqua", "pt-br": "Sofá Retrô Aquamarine"}', '{"es": "¡Mullido y confortable!", "pt-br": "Aconchego de pixels"}'),
    ROW('diner_sofa_1*2', '{"es": "Sofá Diner Rosa", "pt-br": "Sofá Retrô Rosa"}', '{"es": "¡Mullido y confortable!", "pt-br": "Aconchego de pixels"}'),
    ROW('diner_sofa_1*3', '{"es": "Sofá Diner Negro", "pt-br": "Sofá Retrô Preto"}', '{"es": "¡Mullido y confortable!", "pt-br": "Aconchego de pixels"}'),
    ROW('diner_sofa_1*4', '{"es": "Sofá Diner Blanco", "pt-br": "Sofá Retrô Branco 1"}', '{"es": "¡Mullido y confortable!", "pt-br": "Confortável e cheio de estilo"}'),
    ROW('diner_sofa_1*5', '{"es": "Sofá Diner Beige", "pt-br": "Sofa Bege 1"}', '{"es": "¡Mullido y confortable!", "pt-br": "Aconchego de pixels"}'),
    ROW('diner_sofa_1*6', '{"es": "Sofá Diner Azul", "pt-br": "Sofá Retrô Azul"}', '{"es": "¡Mullido y confortable!", "pt-br": "Aconchego de pixels"}'),
    ROW('diner_sofa_1*7', '{"es": "Sofá Diner Verde", "pt-br": "Sofá Retrô Verde"}', '{"es": "¡Mullido y confortable!", "pt-br": "Aconchego de pixels"}'),
    ROW('diner_sofa_1*8', '{"es": "Sofá Diner Amarillo", "pt-br": "Sofá Retrô Amarelo"}', '{"es": "¡Mullido y confortable!", "pt-br": "Aconchego de pixels"}'),
    ROW('diner_sofa_1*9', '{"es": "Sofá Diner Rojo", "pt-br": "Sofá Retrô Vermelho"}', '{"es": "¡Mullido y confortable!", "pt-br": "Aconchego de pixels"}'),
    ROW('diner_sofa_2*1', '{"es": "Sofá Diner Aqua", "pt-br": "Sofá Retrô Aquamarine"}', '{"es": "¡Mullido y confortable!", "pt-br": "Aconchego de pixels"}'),
    ROW('diner_sofa_2*2', '{"es": "Sofá Diner Rosa", "pt-br": "Sofá Retrô Rosa"}', '{"es": "¡Mullido y confortable!", "pt-br": "Aconchego de pixels"}'),
    ROW('diner_sofa_2*3', '{"es": "Sofá Diner Negro 2", "pt-br": "Sofá Retrô Preto"}', '{"es": "¡Mullido y comfortable!", "pt-br": "Aconchego de pixels"}'),
    ROW('diner_sofa_2*4', '{"es": "Sofá Diner Blanco", "pt-br": "Sofá Retrô Branco"}', '{"es": "¡Mullido y confortable!", "pt-br": "O tempo todo na moda"}'),
    ROW('diner_sofa_2*5', '{"es": "Sofá Diner Beige", "pt-br": "Sofá Bege 2"}', '{"es": "¡Mullido y confortable!", "pt-br": "Aconchego de pixels"}'),
    ROW('diner_sofa_2*6', '{"es": "Sofá Diner Azul 2", "pt-br": "Sofá Retrô Azul"}', '{"es": "¡Mullido y confortable!", "pt-br": "Aconchego de pixels"}'),
    ROW('diner_sofa_2*7', '{"es": "Sofá Diner Verde 2", "pt-br": "Sofá Retrô Verde"}', '{"es": "¡Mullido y confortable!", "pt-br": "Aconchego de pixels"}'),
    ROW('diner_sofa_2*8', '{"es": "Sofá Diner Amarillo", "pt-br": "Sofá Retrô Amarelo"}', '{"es": "¡Mullido y confortable!", "pt-br": "O tempo todo na moda"}'),
    ROW('diner_sofa_2*9', '{"es": "Sofá Diner Rojo 2", "pt-br": "Sofá Retrô Vermelho"}', '{"es": "¡Mullido y confortable!", "pt-br": "Aconchego de pixels"}'),
    ROW('diner_table_1*1', '{"es": "Mesa Diner Aqua", "pt-br": "Mesa Retrô Aquamarine"}', '{"es": "¿Listo para una buena comilona?", "pt-br": "Não faça sujeira"}'),
    ROW('diner_table_1*2', '{"es": "Mesa Diner Rosa", "pt-br": "Mesa Retrô Rosa"}', '{"es": "¿Listo para una buena comilona?", "pt-br": "Não faça sujeira"}'),
    ROW('diner_table_1*3', '{"es": "Mesa Diner Negra", "pt-br": "Mesa Retrô Preta"}', '{"es": "¿Listo para una buena comilona?", "pt-br": "Não faça sujeira"}'),
    ROW('diner_table_1*4', '{"es": "Mesa Diner Blanca", "pt-br": "Mesa Retrô Pequena Branca"}', '{"es": "¿Listo para una buena comilona?", "pt-br": "Uma nova onda retrô no Hotel"}'),
    ROW('diner_table_1*5', '{"es": "Mesa Diner Beige", "pt-br": "Mesa Bege"}', '{"es": "¿Listo para una buena comilona?", "pt-br": "Não faça sujeira"}'),
    ROW('diner_table_1*6', '{"es": "Mesa Diner Azul", "pt-br": "Mesa Retrô Azul"}', '{"es": "¿Listo para una buena comilona?", "pt-br": "Não faça sujeira"}'),
    ROW('diner_table_1*7', '{"es": "Mesa Diner Verde", "pt-br": "Mesa Retrô Verde"}', '{"es": "¿Listo para una buena comilona?", "pt-br": "Não faça sujeira"}'),
    ROW('diner_table_1*8', '{"es": "Mesa Diner Amarilla", "pt-br": "Mesa Retrô Amarela"}', '{"es": "¿Listo para una buena comilona?", "pt-br": "Não faça sujeira"}'),
    ROW('diner_table_1*9', '{"es": "Mesa Diner Roja", "pt-br": "Mesa Retrô Vermelha"}', '{"es": "¿Listo para una buena comilona?", "pt-br": "Não faça sujeira"}'),
    ROW('diner_table_2*1', '{"es": "Gran Mesa Diner Aqua", "pt-br": "Mesa Retrô Aquamarine"}', '{"es": "¡Para reuniones familiares!", "pt-br": "Não faça sujeira"}'),
    ROW('diner_table_2*2', '{"es": "Gran Mesa Diner Rosa", "pt-br": "Mesa Retrô Rosa"}', '{"es": "¡Para reuniones familiares!", "pt-br": "Não faça sujeira"}'),
    ROW('diner_table_2*3', '{"es": "Gran Mesa Diner Negra", "pt-br": "Mesa Retrô Preta"}', '{"es": "¡Para reuniones familiares!", "pt-br": "Não faça sujeira"}'),
    ROW('diner_table_2*4', '{"es": "Gran Mesa Diner Blanca", "pt-br": "Mesa Retrô Grande Branca"}', '{"es": "¡Para reuniones familiares!", "pt-br": "Convide os amigos para um lanche!"}'),
    ROW('diner_table_2*5', '{"es": "Gran Mesa Diner Beige", "pt-br": "Mesa Bege"}', '{"es": "¡Para reuniones familiares!", "pt-br": "Não faça sujeira"}'),
    ROW('diner_table_2*6', '{"es": "Gran Mesa Diner Azul", "pt-br": "Mesa Retrô Azul"}', '{"es": "¡Para reuniones familiares!", "pt-br": "Não faça sujeira"}'),
    ROW('diner_table_2*7', '{"es": "Gran Mesa Diner Verde", "pt-br": "Mesa Retrô Verde"}', '{"es": "¡Para reuniones familiares!", "pt-br": "Não faça sujeira"}'),
    ROW('diner_table_2*8', '{"es": "Gran Mesa Diner Amarilla", "pt-br": "Mesa Retrô Amarela"}', '{"es": "¡Para reuniones familiares!", "pt-br": "Não faça sujeira"}'),
    ROW('diner_table_2*9', '{"es": "Gran Mesa Diner Roja", "pt-br": "Mesa Retrô Vermelha"}', '{"es": "¡Para reuniones familiares!", "pt-br": "Não faça sujeira"}'),
    ROW('diner_tray_0', '{"es": "Plato Diner", "pt-br": "Comida Retrô"}', '{"es": "¿Y la comida?", "pt-br": "Tudo em pratos limpos"}'),
    ROW('diner_tray_1', '{"es": "Hamburguesa Diner", "pt-br": "Comida Retrô"}', '{"es": "¡Y que no falten las patatas!", "pt-br": "Tudo em pratos limpos"}'),
    ROW('diner_tray_2', '{"es": "Filete Diner", "pt-br": "Comida Retrô"}', '{"es": "¡Hecho a la piedra!", "pt-br": "Tudo em pratos limpos"}'),
    ROW('diner_tray_3', '{"es": "Spaghetti Diner", "pt-br": "Comida Retrô"}', '{"es": "¡Ojo con el tomate!", "pt-br": "Tudo em pratos limpos"}'),
    ROW('diner_tray_4', '{"es": "Tortitas Diner", "pt-br": "Comida Retrô"}', '{"es": "¡Recién hechas!", "pt-br": "Tudo em pratos limpos"}'),
    ROW('diner_tray_5', '{"es": "Huevos Diner", "pt-br": "Comida Retrô"}', '{"es": "¡Para estómagos resistentes!", "pt-br": "Tudo em pratos limpos"}'),
    ROW('diner_tray_6', '{"es": "Batidos Diner", "pt-br": "Comida Retrô"}', '{"es": "¡Pura cremosidad!", "pt-br": "Tudo em pratos limpos"}'),
    ROW('diner_tray_7', '{"es": "Salsas Diner", "pt-br": "Comida Retrô"}', '{"es": "Tú eliges: ketchup, mostaza, mahonesa...", "pt-br": "Tudo em pratos limpos"}'),
    ROW('diner_walltable', '{"es": "Mesilla Diner", "pt-br": "Mesa Suspensa Retrô"}', '{"es": "¡Ideal para cualquier rincón!", "pt-br": "Praticidade é tudo!"}'),
    ROW('divider_arm1', '{"es": "Esquina", "pt-br": "Base de Canto Rústica"}', '{"es": "Madera sólida", "pt-br": "Base de Canto"}'),
    ROW('divider_arm2', '{"es": "Un divisor", "pt-br": "Pacote Rústico Divisória"}', '{"es": "Con estilo", "pt-br": "Divisória"}'),
    ROW('divider_arm3', '{"es": "Portón", "pt-br": "Porta (com trinco)"}', '{"es": "¿Quién es?", "pt-br": "Alguém em casa?"}'),
    ROW('divider_nor1', '{"es": "Mini divisor", "pt-br": "Canto Iced Bege"}', '{"es": "Divide para reunir", "pt-br": "Canto *macio*"}'),
    ROW('divider_nor1*2', '{"es": "Mini divisor Negro", "pt-br": "Canto Iced Preto"}', '{"es": "Divide para reunir", "pt-br": "Preto Canto Gelo"}'),
    ROW('divider_nor1*3', '{"es": "Mini divisor Blanco", "pt-br": "Canto Iced Branco"}', '{"es": "Divide para reunir", "pt-br": "Branco Canto Gelo"}'),
    ROW('divider_nor1*4', '{"es": "Mini divisor Aguamarina", "pt-br": "Canto Iced Água-marinha"}', '{"es": "Divide para reunir", "pt-br": "Aquamarine Canto Gelo"}'),
    ROW('divider_nor1*5', '{"es": "Mini divisor Rosa", "pt-br": "Canto Iced Rosa"}', '{"es": "Divide para reunir", "pt-br": "Rosa Canto Gelo"}'),
    ROW('divider_nor1*6', '{"es": "Mini divisor Azul", "pt-br": "Canto Iced Azul"}', '{"es": "Divide para reunir", "pt-br": "Azul Canto Gelo"}'),
    ROW('divider_nor1*7', '{"es": "Mini divisor Verde", "pt-br": "Canto Iced Verde"}', '{"es": "Divide para reunir", "pt-br": "Verde Canto Gelo"}'),
    ROW('divider_nor1*8', '{"es": "Mini divisor Amarillo", "pt-br": "Canto Iced Amarelo"}', '{"es": "Divide para reunir", "pt-br": "Amarelo Canto Gelo"}'),
    ROW('divider_nor1*9', '{"es": "Mini divisor Rojo", "pt-br": "Canto Iced Vermelho"}', '{"es": "Divide para reunir", "pt-br": "Coleção Iced Natal 2007"}'),
    ROW('divider_nor2', '{"es": "Un divisor", "pt-br": "Pacote Iced Divisórias"}', '{"es": "Con estilo", "pt-br": "Divisória Iced Bege"}'),
    ROW('divider_nor2*2', '{"es": "Divisor Negro", "pt-br": "Divisória Iced Preta"}', '{"es": "Con estilo", "pt-br": "Preto Mesa de Bar Iced"}'),
    ROW('divider_nor2*3', '{"es": "Divisor Blanco", "pt-br": "Divisória Iced Branca"}', '{"es": "Con estilo", "pt-br": "Branco Mesa de Bar Iced"}'),
    ROW('divider_nor2*4', '{"es": "Divisor Aguamarina", "pt-br": "Divisória Iced Água-marinha"}', '{"es": "Con estilo", "pt-br": "Aquamarine Mesa de Bar Iced"}'),
    ROW('divider_nor2*5', '{"es": "Divisor Rosa", "pt-br": "Divisória Iced Rosa"}', '{"es": "Con estilo", "pt-br": "Rosa Mesa de Bar Iced"}'),
    ROW('divider_nor2*6', '{"es": "Divisor Azul", "pt-br": "Divisória Iced Azul"}', '{"es": "Con estilo", "pt-br": "Azul Mesa de Bar Iced"}'),
    ROW('divider_nor2*7', '{"es": "Divisor Verde", "pt-br": "Divisória Iced Verde"}', '{"es": "Con estilo", "pt-br": "Verde Mesa de Bar Iced"}'),
    ROW('divider_nor2*8', '{"es": "Divisor Amarillo", "pt-br": "Divisória Iced Amarela"}', '{"es": "Con estilo", "pt-br": "Amarelo Mesa de Bar Iced"}'),
    ROW('divider_nor2*9', '{"es": "Divisor Rojo", "pt-br": "Divisória Iced Vermelha"}', '{"es": "Con estilo", "pt-br": "Coleção Iced Natal 2007"}'),
    ROW('divider_nor3', '{"es": "La Puerta", "pt-br": "Portão Iced Bege"}', '{"es": "A puerta cerrada o abierta", "pt-br": "Entre..."}'),
    ROW('divider_nor3*2', '{"es": "La Puerta Negra", "pt-br": "Portão Iced Preto"}', '{"es": "A puerta cerrada o abierta", "pt-br": "Preto Portão Iced"}'),
    ROW('divider_nor3*3', '{"es": "La Puerta Blanca", "pt-br": "Branco Portão Iced"}', '{"es": "A puerta cerrada o abierta", "pt-br": "Portão Iced Branco"}'),
    ROW('divider_nor3*4', '{"es": "La Puerta Aguamarina", "pt-br": "Portão Iced Água-marinha"}', '{"es": "A puerta cerrada o abierta", "pt-br": "Aquamarine Portão Iced"}'),
    ROW('divider_nor3*5', '{"es": "La Puerta Rosa", "pt-br": "Portão Iced Rosa"}', '{"es": "A puerta cerrada o abierta", "pt-br": "Rosa Portão Iced"}'),
    ROW('divider_nor3*6', '{"es": "La Puerta Azul", "pt-br": "Portão Iced Azul"}', '{"es": "A puerta cerrada o abierta", "pt-br": "Azul Portão Iced"}'),
    ROW('divider_nor3*7', '{"es": "La Puerta Verde", "pt-br": "Portão Iced Verde"}', '{"es": "A puerta cerrada o abierta", "pt-br": "Verde Portão Iced"}'),
    ROW('divider_nor3*8', '{"es": "La Puerta Amarilla", "pt-br": "Portão Iced Amarelo"}', '{"es": "A puerta cerrada o abierta", "pt-br": "Amarelo Portão Iced"}'),
    ROW('divider_nor3*9', '{"es": "La Puerta Roja", "pt-br": "Portão Iced Vermelho"}', '{"es": "A puerta cerrada o abierta", "pt-br": "Coleção Iced Natal 2007"}'),
    ROW('divider_nor4', '{"es": "Barra Clásica", "pt-br": "Persiana Iced Bege"}', '{"es": "¡Prohibido!", "pt-br": "De enrolar."}'),
    ROW('divider_nor4*2', '{"es": "Barra Negra", "pt-br": "Persiana Iced Preta"}', '{"es": "¡Prohibido!", "pt-br": "De enrolar."}'),
    ROW('divider_nor4*3', '{"es": "Barra Blanca", "pt-br": "Persiana Iced Branca"}', '{"es": "¡Prohibido!", "pt-br": "De enrolar."}'),
    ROW('divider_nor4*4', '{"es": "Barra Marina", "pt-br": "Persiana Iced Água-marinha"}', '{"es": "¡Prohibido!", "pt-br": "De enrolar."}'),
    ROW('divider_nor4*5', '{"es": "Barra Rosa", "pt-br": "Persiana Iced Rosa"}', '{"es": "¡Prohibido!", "pt-br": "De enrolar."}'),
    ROW('divider_nor4*6', '{"es": "Barra Azul", "pt-br": "Persiana Iced Azul"}', '{"es": "¡Prohibido!", "pt-br": "De enrolar."}'),
    ROW('divider_nor4*7', '{"es": "Barra Verde", "pt-br": "Persiana Iced Verde"}', '{"es": "¡Prohibido!", "pt-br": "De enrolar."}'),
    ROW('divider_nor4*8', '{"es": "Barra Piramidal", "pt-br": "Persiana Iced Amarela"}', '{"es": "¡Prohibido!", "pt-br": "De enrolar."}'),
    ROW('divider_nor4*9', '{"es": "Barra Roja", "pt-br": "Persiana Iced Vermelha"}', '{"es": "¡Prohibido!", "pt-br": "De enrolar."}'),
    ROW('divider_nor5', '{"es": "Ángulo Iced Clásico", "pt-br": "Ângulo Iced Bege"}', '{"es": "El camino tuerce aquí", "pt-br": "Dobre no meio, faça ambientes."}'),
    ROW('divider_nor5*2', '{"es": "Ángulo Iced Negro", "pt-br": "Ângulo Iced Preto"}', '{"es": "Ángulo Iced", "pt-br": "Monte e fique diferente"}'),
    ROW('divider_nor5*3', '{"es": "Ángulo Iced Blanco", "pt-br": "Ângulo Iced Branco"}', '{"es": "El camino tuerce aquí", "pt-br": "Monte e fique diferente"}'),
    ROW('divider_nor5*4', '{"es": "Ángulo Iced Marino", "pt-br": "Ângulo Iced Água-marinha"}', '{"es": "El camino tuerce aquí", "pt-br": "Monte e fique diferente"}'),
    ROW('divider_nor5*5', '{"es": "Ángulo Iced Rosa", "pt-br": "Ângulo Iced Rosa"}', '{"es": "El camino tuerce aquí", "pt-br": "Monte e fique diferente"}'),
    ROW('divider_nor5*6', '{"es": "Ángulo Iced Azul", "pt-br": "Ângulo Iced Azul"}', '{"es": "El camino tuerce aquí", "pt-br": "Monte e fique diferente"}'),
    ROW('divider_nor5*7', '{"es": "Ángulo Iced Verde", "pt-br": "Ângulo Iced Verde"}', '{"es": "El camino tuerce aquí", "pt-br": "Monte e fique diferente"}'),
    ROW('divider_nor5*8', '{"es": "Ángulo Iced Amarillo", "pt-br": "Ângulo Iced Amarelo"}', '{"es": "El camino tuerce aquí", "pt-br": "Monte e fique diferente"}'),
    ROW('divider_nor5*9', '{"es": "Ángulo Iced Rojo", "pt-br": "Ângulo Iced Vermelho"}', '{"es": "El camino tuerce aquí", "pt-br": "Monte e fique diferente"}'),
    ROW('divider_poly3', '{"es": "La Trampa", "pt-br": "Portão Mode Água-marinha"}', '{"es": "¡No caigas en ella!", "pt-br": "Para bares e o que vier na cabeça"}'),
    ROW('divider_poly3*2', '{"es": "La Trampa Negra", "pt-br": "Portão Mode Preto"}', '{"es": "¡No caigas en ella!", "pt-br": "Preto Portão Balcão Mode"}'),
    ROW('divider_poly3*3', '{"es": "La Trampa Blanca", "pt-br": "Portão Mode Branco"}', '{"es": "¡No caigas en ella!", "pt-br": "Branco Portão Balcão Mode"}'),
    ROW('divider_poly3*4', '{"es": "La Trampa Beige", "pt-br": "Portão Mode Bege"}', '{"es": "¡No caigas en ella!", "pt-br": "Bege Portão Balcão Mode"}'),
    ROW('divider_poly3*5', '{"es": "La Trampa Rosa", "pt-br": "Portão Mode Rosa"}', '{"es": "¡No caigas en ella!", "pt-br": "Candy Portão Balcão"}'),
    ROW('divider_poly3*6', '{"es": "La Trampa Azul", "pt-br": "Portão Mode Azul"}', '{"es": "¡No caigas en ella!", "pt-br": "Azul Portão Balcão Mode"}'),
    ROW('divider_poly3*7', '{"es": "La Trampa Verde", "pt-br": "Portão Mode Verde"}', '{"es": "¡No caigas en ella!", "pt-br": "Verde Portão Balcão Mode"}'),
    ROW('divider_poly3*8', '{"es": "La Trampa Amarilla", "pt-br": "Portão Mode Amarelo"}', '{"es": "¡No caigas en ella!", "pt-br": "Amarelo Portão Balcão Mode"}'),
    ROW('divider_poly3*9', '{"es": "La Trampa Roja", "pt-br": "Portão Mode Vermelho"}', '{"es": "¡No caigas en ella!", "pt-br": "Coleção Mode Natal 2007"}'),
    ROW('divider_silo1', '{"es": "Esquina Área", "pt-br": "Prateleira Area"}', '{"es": "Un toque chic", "pt-br": "Prateleiras de Canto Area"}'),
    ROW('divider_silo1*2', '{"es": "La Esquina Negra", "pt-br": "Preto Estante de Canto Area"}', '{"es": "Girando 90º", "pt-br": "Preto Estante de Canto"}'),
    ROW('divider_silo1*3', '{"es": "La Esquina Blanca", "pt-br": "Estante de Canto Area Branca"}', '{"es": "Girando 90º", "pt-br": "Dá paz e tranquilidade"}'),
    ROW('divider_silo1*4', '{"es": "La Esquina Beige", "pt-br": "Bege Estante de Canto Area"}', '{"es": "Girando 90º", "pt-br": "Bege Estante de Canto Area"}'),
    ROW('divider_silo1*5', '{"es": "La Esquina Rosa", "pt-br": "Estante de Canto Rosa"}', '{"es": "Girando 90º", "pt-br": "Estante de Canto Rosa"}'),
    ROW('divider_silo1*6', '{"es": "La Esquina Azul", "pt-br": "Azul Estante de Canto Area"}', '{"es": "Girando 90º", "pt-br": "Azul Estante de Canto Area"}'),
    ROW('divider_silo1*7', '{"es": "La Esquina Verde", "pt-br": "Verde Estante de Canto Area"}', '{"es": "Girando 90º", "pt-br": "Verde Estante de Canto Area"}'),
    ROW('divider_silo1*8', '{"es": "La Esquina Amarilla", "pt-br": "Amarelo Estante de Canto Area"}', '{"es": "Girando 90º", "pt-br": "Amarelo Estante de Canto Area"}'),
    ROW('divider_silo1*9', '{"es": "La Esquina Roja", "pt-br": "Vermelho Area Mesa Pequena"}', '{"es": "Girando 90º", "pt-br": "Vermelho Area Mesa Pequena"}'),
    ROW('divider_silo2', '{"es": "El Divisor", "pt-br": "Pacote Area Telas"}', '{"es": "Divide con estilo", "pt-br": "Cinco Telas Area"}'),
    ROW('divider_silo3', '{"es": "La Puerta", "pt-br": "Porta (com trinco)"}', '{"es": "Que pase el siguiente", "pt-br": "Para festas e eventos"}'),
    ROW('divider_silo3*2', '{"es": "La Puerta", "pt-br": "Preto Portão Area"}', '{"es": "Que pase el siguiente", "pt-br": "Preto Portão Area"}'),
    ROW('divider_silo3*3', '{"es": "La Puerta Blanca", "pt-br": "Portão Area Branco"}', '{"es": "Que pase el siguiente", "pt-br": "Controle o acesso"}'),
    ROW('divider_silo3*4', '{"es": "La Puerta Beige", "pt-br": "Bege Portão Area"}', '{"es": "Que pase el siguiente", "pt-br": "Bege Portão Area"}'),
    ROW('divider_silo3*5', '{"es": "La Puerta Rosa", "pt-br": "Portão Area Rosa"}', '{"es": "Que pase el siguiente", "pt-br": "Portão Area Rosa"}'),
    ROW('divider_silo3*6', '{"es": "La Puerta Azul", "pt-br": "Azul Portão Area"}', '{"es": "Que pase el siguiente", "pt-br": "Azul Portão Area"}'),
    ROW('divider_silo3*7', '{"es": "La Puerta Verde", "pt-br": "Verde Portão Area"}', '{"es": "Que pase el siguiente", "pt-br": "Verde Portão Area"}'),
    ROW('divider_silo3*8', '{"es": "La Puerta Amarilla", "pt-br": "Amarelo Portão Area"}', '{"es": "Que pase el siguiente", "pt-br": "Amarelo Portão Area"}'),
    ROW('divider_silo3*9', '{"es": "La Puerta Roja", "pt-br": "Vermelho Portão Area"}', '{"es": "Que pase el siguiente", "pt-br": "Vermelho Portão Area"}'),
    ROW('djesko_turntable', '{"es": "Habbo Tocadiscos", "pt-br": "PickUp DJ"}', '{"es": "Música al estilo retro", "pt-br": "Os maiores DJs do Habbo já tocaram aqui"}'),
    ROW('door', '{"es": "El Británico", "pt-br": "Cabine"}', '{"es": "Viaja con estilo", "pt-br": "Quem está na linha?"}'),
    ROW('doorB', '{"es": "El Guardarropa", "pt-br": "Armário"}', '{"es": "Descubre otros mundos", "pt-br": "Esconde um outro mundo"}'),
    ROW('doorC', '{"es": "Portaloo", "pt-br": "Banheiro Público"}', '{"es": "Éste te transportará", "pt-br": "Com pressa?"}'),
    ROW('doorD', '{"es": "Imperiales", "pt-br": "Teleporte Imperial"}', '{"es": "Viaja al estilo imperial", "pt-br": "Vamos nessa, imperador!"}'),
    ROW('doormat_love', '{"es": "Felpudo", "pt-br": "Tapete de Boas-Vindas"}', '{"es": "Disfruta del Habbo estilo", "pt-br": "Dê boas-vindas aos Habbos com estilo"}'),
    ROW('doormat_plain', '{"es": "Felpudo", "pt-br": "Tapete de Boas-Vindas"}', '{"es": "Disponible en varios colores", "pt-br": "Disponível em várias cores"}'),
    ROW('doormat_plain*1', '{"es": "Felpudo", "pt-br": "Tapete de Boas-Vindas"}', '{"es": "Disponible en varios colores", "pt-br": "Disponível em várias cores"}'),
    ROW('doormat_plain*2', '{"es": "Felpudo", "pt-br": "Tapete de Boas-Vindas"}', '{"es": "Disponible en varios colores", "pt-br": "Disponível em várias cores"}'),
    ROW('doormat_plain*3', '{"es": "Felpudo", "pt-br": "Tapete de Boas-Vindas"}', '{"es": "Disponible en varios colores", "pt-br": "Disponível em várias cores"}'),
    ROW('doormat_plain*4', '{"es": "Felpudo", "pt-br": "Tapete de Boas-Vindas"}', '{"es": "Disponible en varios colores", "pt-br": "Disponível em várias cores"}'),
    ROW('doormat_plain*5', '{"es": "Felpudo", "pt-br": "Tapete de Boas-Vindas"}', '{"es": "Disponible en varios colores", "pt-br": "Disponível em várias cores"}'),
    ROW('doormat_plain*6', '{"es": "Felpudo", "pt-br": "Tapete de Boas-Vindas"}', '{"es": "Disponible en varios colores", "pt-br": "Disponível em várias cores"}'),
    ROW('drinks', '{"es": "Latas Vacías", "pt-br": "Latas Vazias"}', '{"es": "¡Cuánto ruido!", "pt-br": "Você também é desleixado?"}'),
    ROW('duck', '{"es": "El Pato", "pt-br": "Pato de Borracha"}', '{"es": "Mira qué ojitos", "pt-br": "Essencial para os banhistas"}'),
    ROW('easterduck', '{"es": "El Conejo", "pt-br": "Pato de Páscoa"}', '{"es": "Cómo molo", "pt-br": "Pato aqui Pato acolá"}'),
    ROW('easy_bowl2', '{"es": "easy_bowl2", "pt-br": "easy_bowl2"}', '{"es": "old campaign product desc", "pt-br": "old campaign product desc"}'),
    ROW('easy_carpet', '{"es": "easy_carpet name", "pt-br": "easy_carpet name"}', '{"es": "easy_carpet text", "pt-br": "easy_carpet text"}'),
    ROW('easy_poster', '{"es": "easy_poster", "pt-br": "easy_poster"}', '{"es": "Easy mac promoción", "pt-br": "Easy mac promotion"}'),
    ROW('eco_cactus1', '{"es": "Cáctus Ecotron 1", "pt-br": "Eco Cacto"}', '{"es": "¡Necesita mucho sol!", "pt-br": "Pra preservar a água do planeta"}'),
    ROW('eco_cactus2', '{"es": "Cáctus Ecotron 2", "pt-br": "Eco Cacto"}', '{"es": "¡Necesita mucho sol!", "pt-br": "Pra preservar a água do planeta"}'),
    ROW('eco_cactus3', '{"es": "Cáctus Ecotron 3", "pt-br": "Eco Cacto"}', '{"es": "¡Necesita mucho sol!", "pt-br": "Pra preservar a água do planeta"}'),
    ROW('eco_chair1', '{"es": "Taburete Ecotron 1", "pt-br": "Eco Banqueta"}', '{"es": "Diseño verdoso", "pt-br": "Conforto reciclado"}'),
    ROW('eco_chair2', '{"es": "Taburete Ecotron 2", "pt-br": "Eco Banqueta"}', '{"es": "Diseño marrón", "pt-br": "Conforto reciclado"}'),
    ROW('eco_chair3', '{"es": "Taburete Ecotron 3", "pt-br": "Eco Banqueta"}', '{"es": "Diseño blanquinegro", "pt-br": "Conforto reciclado"}'),
    ROW('eco_curtains1', '{"es": "Cortinas Ecotron 1", "pt-br": "Eco Cortina"}', '{"es": "¡Un toque de reciclaje para tu Sala!", "pt-br": "Bloqueia os raios solares."}'),
    ROW('eco_curtains2', '{"es": "Cortinas Ecotron 2", "pt-br": "Eco Cortina"}', '{"es": "¡Un toque de reciclaje para tu Sala!", "pt-br": "Bloqueia os raios solares."}'),
    ROW('eco_curtains3', '{"es": "Cortinas Ecotron 3", "pt-br": "Eco Cortina"}', '{"es": "¡Un toque de reciclaje para tu Sala!", "pt-br": "Bloqueia os raios solares."}'),
    ROW('eco_fruits1', '{"es": "Bandeja de Frutas 1", "pt-br": "Cesto de Frutas"}', '{"es": "¡Del árbol a tu mesa pixelada!", "pt-br": "Alimento saudável"}'),
    ROW('eco_fruits2', '{"es": "Bandeja de Frutas 2", "pt-br": "Cesto de Frutas"}', '{"es": "¡Del árbol a tu mesa pixelada!", "pt-br": "Alimento saudável"}'),
    ROW('eco_fruits3', '{"es": "Bandeja de Frutas 3", "pt-br": "Cesto de Frutas"}', '{"es": "¡Del árbol a tu mesa pixelada!", "pt-br": "Alimento saudável"}'),
    ROW('eco_lamp1', '{"es": "Lámpara Ecotron 1", "pt-br": "Eco Lâmpada"}', '{"es": "¡Con bombilla de bajo consumo!", "pt-br": "Corrente de energia solar"}'),
    ROW('eco_lamp2', '{"es": "Lámpara Ecotron 2", "pt-br": "Eco Lâmpada"}', '{"es": "¡Con bombilla de bajo consumo!", "pt-br": "Corrente de energia solar"}'),
    ROW('eco_lamp3', '{"es": "Lámpara Ecotron 3", "pt-br": "Eco Lâmpada"}', '{"es": "¡Con bombilla de bajo consumo!", "pt-br": "Corrente de energia solar"}'),
    ROW('eco_light1', '{"es": "Luz Ecotron 1", "pt-br": "Eco Lustre"}', '{"es": "¡Con bombilla de bajo consumo!", "pt-br": "Requinte e ecologia"}'),
    ROW('eco_light2', '{"es": "Luz Ecotron 2", "pt-br": "Eco Lustre"}', '{"es": "¡Con bombilla de bajo consumo!", "pt-br": "Requinte e ecologia"}'),
    ROW('eco_light3', '{"es": "Luz Ecotron 3", "pt-br": "Eco Lustre"}', '{"es": "¡Con bombilla de bajo consumo!", "pt-br": "Requinte e ecologia"}'),
    ROW('eco_mush1', '{"es": "Setón", "pt-br": "Champignon Gigante"}', '{"es": "Menudo ejemplar", "pt-br": "Casinha de chuva"}'),
    ROW('eco_mush2', '{"es": "Amanita", "pt-br": "Amanita Venenosa"}', '{"es": "Veneno puro", "pt-br": "Um cogumelo raríssimo"}'),
    ROW('eco_sofa1', '{"es": "Sillón Ecotron 1", "pt-br": "Eco Cadeira"}', '{"es": "¡Relájate!", "pt-br": "Descanse sem medo"}'),
    ROW('eco_sofa2', '{"es": "Sillón Ecotron 2", "pt-br": "Eco Cadeira"}', '{"es": "¡Relájate!", "pt-br": "Descanse sem medo"}'),
    ROW('eco_sofa3', '{"es": "Sillón Ecotron 3", "pt-br": "Eco Cadeira"}', '{"es": "¡Relájate!", "pt-br": "Descanse sem medo"}'),
    ROW('eco_table1', '{"es": "Mesa Ecotron 1", "pt-br": "Eco Mesa de Café"}', '{"es": "¡Madera reciclada!", "pt-br": "Pode se servir"}'),
    ROW('eco_table2', '{"es": "Mesa Ecotron 2", "pt-br": "Eco Mesa de Café"}', '{"es": "¡Madera reciclada!", "pt-br": "Pode se servir"}'),
    ROW('eco_table3', '{"es": "Mesa Ecotron 3", "pt-br": "Eco Mesa de Café"}', '{"es": "¡Madera reciclada!", "pt-br": "Pode se servir"}'),
    ROW('eco_tree1', '{"es": "Naranjo", "pt-br": "Pé de Laranja"}', '{"es": "¡Qué buena pinta!", "pt-br": "Deliciosos sucos"}'),
    ROW('eco_tree2', '{"es": "Peral", "pt-br": "Pé de Pêra"}', '{"es": "¡Qué buena pinta!", "pt-br": "Quer uma mordida?"}'),
    ROW('ecotron_box', '{"es": "Caja Misteriosa", "pt-br": "Caixa Surpresa"}', '{"es": "Este objeto es 100 % reciclado.", "pt-br": "O item está 100 % reciclado."}'),
    ROW('edice', '{"es": "Holodado", "pt-br": "Holo-Dado"}', '{"es": "Tu número de la suerte es...", "pt-br": "Qual é o seu número de sorte?"}'),
    ROW('edicehc', '{"es": "El Super Dado", "pt-br": "Porta Dados"}', '{"es": "Busca tu número", "pt-br": "Clique para girar."}'),
    ROW('env_bushes', '{"es": "Arbusto Divisor", "pt-br": "Divisória de Arbusto"}', '{"es": "¡Impenetrable!", "pt-br": "Cuidado com os espinhos"}'),
    ROW('env_bushes_gate', '{"es": "Arbusto Puerta", "pt-br": "Portão de Arbusto"}', '{"es": "Agáchate un poco para pasar...", "pt-br": "Abre-te Sésamo!"}'),
    ROW('env_grass', '{"es": "Césped Pascua", "pt-br": "Piso de Grama"}', '{"es": "¡Frondoso a más no poder!", "pt-br": "A do vizinho é sempre mais verde"}'),
    ROW('env_telep', '{"es": "Teleport Campestre", "pt-br": "Casinha Teleporte"}', '{"es": "¡Para viajar por el campo!", "pt-br": "Se estiver apertado, espera na fila"}'),
    ROW('env_tree2', '{"es": "Arbolito 2", "pt-br": "Banqueta de Pinheiro"}', '{"es": "¡Viva la naturaleza!", "pt-br": "Fruto do reflorestamento"}'),
    ROW('env_tree3', '{"es": "Arbolito 3", "pt-br": "Banqueta de Eucalipto"}', '{"es": "¡Viva la naturaleza!", "pt-br": "Sente esse aroma!"}'),
    ROW('env_tree4', '{"es": "Árbol de la Amistad", "pt-br": "Árvore"}', '{"es": "¡Cuida la naturaleza junto a un buen amig@!", "pt-br": "O melhor presente para o melhor amigo"}'),
    ROW('exe_artlamp', '{"es": "Lámpara para Ejecutivos", "pt-br": "Lâmpada Executiva Artística"}', '{"es": "¡Un toque chic para tu despacho!", "pt-br": "Cultura e negócios podem andar juntos"}'),
    ROW('exe_bardesk', '{"es": "Barra Ejecutiva", "pt-br": "Bar Balcão"}', '{"es": "¡Un salto de calidad!", "pt-br": "Para bares e o que vier na cabeça"}'),
    ROW('exe_chair', '{"es": "Silla Ejecutiva", "pt-br": "Poltrona Empresário"}', '{"es": "¡Tomando grandes decisiones!", "pt-br": "Esperando o cafezinho."}'),
    ROW('exe_chair2', '{"es": "Sillón Presidente", "pt-br": "Cadeira Patrão"}', '{"es": "¡Tomando decisiones!", "pt-br": "Não me venha pedir aumento."}'),
    ROW('exe_corner', '{"es": "Esquina Ejecutiva", "pt-br": "Canto de Balcão"}', '{"es": "¡De gran calidad!", "pt-br": "Canto do Bar Balcão"}'),
    ROW('exe_cubelight', '{"es": "Cubo Ejecutivo", "pt-br": "Lâmpada de Cubo Executiva"}', '{"es": "¡Míralo en profundidad!", "pt-br": "Coisa do Picasso..."}'),
    ROW('exe_drinks', '{"es": "Bandeja de Ejecutivos", "pt-br": "Drinques"}', '{"es": "¡Para un merecido descanso!", "pt-br": "Suco de laranja e guaraná?"}'),
    ROW('exe_elevator', '{"es": "Ascensor Executive", "pt-br": "Elevador Executivo"}', '{"es": "¿Qué habrá en las otras Plantas?", "pt-br": "Para chegar rapidamente ao topo"}'),
    ROW('exe_gate', '{"es": "Trampa Ejecutiva", "pt-br": "Portão Executivo"}', '{"es": "¡No te quedes en medio!", "pt-br": "Toc toc?"}'),
    ROW('exe_globe', '{"es": "Globo Terráqueo Ejecutivo", "pt-br": "Globo"}', '{"es": "¡Tú decides a dónde quieres viajar!", "pt-br": "O mundo no seu Quarto"}'),
    ROW('exe_light', '{"es": "Foco Ejecutivo", "pt-br": "Lustre Executivo"}', '{"es": "¡Arrojan luz positiva sobre los números!", "pt-br": "O poder sobre a sua cabeça"}'),
    ROW('exe_map', '{"es": "Mapa", "pt-br": "Mapa Executivo"}', '{"es": "¡Señala tu país de origen!", "pt-br": "Não é você que quer dominar o mundo?"}'),
    ROW('exe_plant', '{"es": "Árbol Wall Street", "pt-br": "Planta Lendária"}', '{"es": "A la sombra de grandes decisiones", "pt-br": "Direto do abismo de pixels"}'),
    ROW('exe_rug', '{"es": "Alfombra Ejecutiva", "pt-br": "Carpete Empresário"}', '{"es": "¡Huele a grandes decisiones!", "pt-br": "Forra o chão do escritório. Admite tapete por cima."}'),
    ROW('exe_s_table', '{"es": "Mesita Ejecutiva", "pt-br": "Mesinha Empresário"}', '{"es": "¡De formas perfectas y elegantes!", "pt-br": "Vidro Temperado."}'),
    ROW('exe_sofa', '{"es": "Sofá Ejecutivo", "pt-br": "Sofá Empresário"}', '{"es": "¡Para grandes negociadores!", "pt-br": "Para espera de clientes ou soneca depois do almoço."}'),
    ROW('exe_table', '{"es": "Mesa de Ejecutivo", "pt-br": "Mesa Empresário"}', '{"es": "¡Repleta de documentos ''Top Secret''!", "pt-br": "Chamadas, documentos, recados, gavetas, gavetas..."}'),
    ROW('exe_wfall', '{"es": "Cascada Ejecutiva", "pt-br": "Cascata Executiva"}', '{"es": "¡Un toque único para tu despacho!", "pt-br": "Ponha uma queda d''água no seu Quarto!"}'),
    ROW('fireplace_armas', '{"es": "La chimenea", "pt-br": "Lareira"}', '{"es": "Para calentar tu Sala o incendiar los corazones", "pt-br": "Fogo autêntico e de verdade"}'),
    ROW('fireplace_polyfon', '{"es": "El Horno", "pt-br": "Lareira"}', '{"es": "Todo se cuece aquí dentro", "pt-br": "Moderno, em aço inoxidável"}'),
    ROW('flag_algeria', '{"es": "Argelia", "pt-br": "Bandeira da Argélia"}', '{"es": "Ondea tu bandera", "pt-br": "Agite-a com orgulho"}'),
    ROW('flag_argentina', '{"es": "Bandera Argentina", "pt-br": "Bandeira da Argentina"}', '{"es": "Ondea tu bandera", "pt-br": "Agite-a com orgulho"}'),
    ROW('flag_belgium', '{"es": "Bélgica", "pt-br": "Bandeira da Bélgica"}', '{"es": "¡Ondea tu bandera!", "pt-br": "Agite-a com orgulho"}'),
    ROW('flag_chile', '{"es": "Chile", "pt-br": "Bandeira do Chile"}', '{"es": "Ondea tu bandera", "pt-br": "Agite-a com orgulho"}'),
    ROW('flag_columbia', '{"es": "Colombia", "pt-br": "Bandeira da Colômbia"}', '{"es": "Ondea tu bandera", "pt-br": "Agite-a com orgulho"}'),
    ROW('flag_dominicanrepublic', '{"es": "República Dominicana", "pt-br": "Bandeira da Rep. Dominicana"}', '{"es": "Ondea tu bandera", "pt-br": "Agite-a com orgulho"}'),
    ROW('flag_ecuador', '{"es": "Ecuador", "pt-br": "Bandeira do Equador"}', '{"es": "Ondea tu bandera", "pt-br": "Agite-a com orgulho"}'),
    ROW('flag_greece', '{"es": "Grecia", "pt-br": "Bandeira da Grécia"}', '{"es": "Ondea tu bandera", "pt-br": "Agite-a com orgulho"}'),
    ROW('flag_malaysia', '{"es": "Malasia", "pt-br": "Bandeira da Malásia"}', '{"es": "Ondea tu bandera", "pt-br": "Agite-a com orgulho"}'),
    ROW('flag_mexico', '{"es": "México", "pt-br": "Bandeira do México"}', '{"es": "Ondea tu bandera", "pt-br": "Bandeira do México"}'),
    ROW('flag_morocco', '{"es": "Marruecos", "pt-br": "Bandeira do Marrocos"}', '{"es": "Ondea tu bandera", "pt-br": "Agite-a com orgulho"}'),
    ROW('flag_newzealand', '{"es": "Nueva Zelanda", "pt-br": "Bandeira da Nova Zelândia"}', '{"es": "Ondea tu bandera", "pt-br": "Agite-a com orgulho"}'),
    ROW('flag_norway', '{"es": "Noruega", "pt-br": "Bandeira da Noruega"}', '{"es": "¡Ondea tu bandera!", "pt-br": "Agite-a com orgulho"}'),
    ROW('flag_panama', '{"es": "Panamá", "pt-br": "Bandeira do Panamá"}', '{"es": "Ondea tu bandera", "pt-br": "Agite-a com orgulho"}'),
    ROW('flag_peru', '{"es": "Perú", "pt-br": "Bandeira do Peru"}', '{"es": "Ondea tu bandera con orgullo", "pt-br": "Agite-a com orgulho"}'),
    ROW('flag_philippines', '{"es": "Filipinas", "pt-br": "Bandeira da Filipinas"}', '{"es": "Ondea tu bandera", "pt-br": "Agite-a com orgulho"}'),
    ROW('flag_portugal', '{"es": "Portugal", "pt-br": "Bandeira de Portugal"}', '{"es": "¡Ondea tu bandera!", "pt-br": "Agite-a com orgulho"}'),
    ROW('flag_singapore', '{"es": "Singapur", "pt-br": "Bandeira de Singapura"}', '{"es": "Ondea tu bandera", "pt-br": "Agite-a com orgulho"}'),
    ROW('flag_tunisia', '{"es": "Túnez", "pt-br": "Bandeira da Tunísia"}', '{"es": "Ondea tu bandera", "pt-br": "Agite-a com orgulho"}'),
    ROW('flag_turkey', '{"es": "Turquía", "pt-br": "Bandeira da Turquia"}', '{"es": "Ondea tu bandera", "pt-br": "Agite-a com orgulho"}'),
    ROW('flag_venezl', '{"es": "Venezuela", "pt-br": "Bandeira da Venezuela"}', '{"es": "Ondea tu bandera", "pt-br": "Agite-a com orgulho"}'),
    ROW('floor', '{}', '{}'),
    ROW('floortile', '{"es": "Baldosa", "pt-br": "Ladrilhos"}', '{"es": "Disponible en varios colores", "pt-br": "Em Várias Cores"}'),
    ROW('footylamp', '{"es": "Balón de Fútbol", "pt-br": "Bola de Futebol Habbo"}', '{"es": "Sólo para grandes goleadores", "pt-br": "Para as maiores pontuações"}'),
    ROW('footylamp_campaign', '{"es": "Balón de Fútbol", "pt-br": "footylamp_campaign"}', '{"es": "Sólo para grandes goleadores"}'),
    ROW('fortune', '{"es": "La Calavera de Cristal", "pt-br": "Bola de Cristal"}', '{"es": "¿Quieres conocer tu futuro?", "pt-br": "Sorte ou azar no amor? Descubra o futuro! :o"}'),
    ROW('fridge', '{"es": "Nevera Pura", "pt-br": "Geladeira Pura"}', '{"es": "Siempre tendrás agua fresca", "pt-br": "Relaxe com um drinque ou um petisco gelado"}'),
    ROW('fx_bubble', '{"es": "Bubbles", "pt-br": "Bubbles"}', '{"es": "Pop!", "pt-br": "Pop!"}'),
    ROW('fx_explosion', '{"es": "Explosion", "pt-br": "Explosion"}', '{"es": "This rental will last for 1 hour.", "pt-br": "This rental will last for 1 hour."}'),
    ROW('fx_flare', '{"es": "Flare", "pt-br": "Flare"}', '{"es": "This rental will last for 1 hour.", "pt-br": "This rental will last for 1 hour."}'),
    ROW('garden_flo1', '{"es": "Pasión azul", "pt-br": "Paixonite Azul"}', '{"es": "Decora tu jardín... con una pizca de suerte", "pt-br": "A paixão é azulzinha."}'),
    ROW('garden_flo2', '{"es": "Placer Amarillo", "pt-br": "Amarelo delirante"}', '{"es": "Decora tu jardín", "pt-br": "Delírios juvenis nos seus jardins."}'),
    ROW('garden_flo3', '{"es": "Explosión rosa", "pt-br": "Rosa cor-de-rosa"}', '{"es": "Decora tu jardín", "pt-br": "Uma pandemia de doçura."}'),
    ROW('garden_flolamp', '{"es": "Lámpara Flor", "pt-br": "Lâmpada Flor"}', '{"es": "Decora tu jardín", "pt-br": "É tempo de fazer a luz desabrochar."}'),
    ROW('garden_flytrap', '{"es": "Teleport Carnívoro", "pt-br": "Teleporte Mordida"}', '{"es": "Decora tu jardín", "pt-br": "Nhac! E você vai pra outro lugar."}'),
    ROW('garden_jungle', '{"es": "Mini Jungla", "pt-br": "Matinho de jardim"}', '{"es": "Decora tu jardín", "pt-br": "Eu acho que vi um gnomo."}'),
    ROW('garden_jyrki', '{"es": "Flor Estrella", "pt-br": "Flor Estrela"}', '{"es": "Decora tu jardín", "pt-br": "Uma constelação no seu jardim."}'),
    ROW('garden_leaves', '{"es": "Hojas", "pt-br": "Vitória Régia"}', '{"es": "Decora tu jardín", "pt-br": "Casa de sapos - ou príncipes."}'),
    ROW('garden_lupin1', '{"es": "Lupino Amarillo", "pt-br": "Tremoceiro Laranja"}', '{"es": "Decora tu jardín", "pt-br": "Um jardim só é feliz com um desses."}'),
    ROW('garden_lupin2', '{"es": "Lupino Azul", "pt-br": "Tremoceiro Azul Celeste"}', '{"es": "Decora tu jardín", "pt-br": "Um pedaço do céu no seu jardim."}'),
    ROW('garden_lupin3', '{"es": "Lupino Rojo", "pt-br": "Tremoceiro Vermelho"}', '{"es": "Decora tu jardín", "pt-br": "A paixão nasce dessas plantas."}'),
    ROW('garden_lupin4', '{"es": "Lupino Blanco", "pt-br": "Tremoceiro Branco Gelo"}', '{"es": "Decora tu jardín", "pt-br": "Nevou aqui ou é impressão minha?"}'),
    ROW('garden_lupin5', '{"es": "Lupino Púrpura", "pt-br": "Tremoceiro Púrpura"}', '{"es": "Decora tu jardín", "pt-br": "A natureza em sua forma mais púrpura."}'),
    ROW('garden_mursu', '{"es": "Junco Rosa", "pt-br": "Taboa Rosa"}', '{"es": "Decora tu jardín", "pt-br": "Faz a diferença no seu ecossistema."}'),
    ROW('garden_mursu2', '{"es": "Junco Rojo", "pt-br": "Taboa Vermelha"}', '{"es": "Decora tu jardín", "pt-br": "Enriquece o ambiente ao colorir a flora."}'),
    ROW('garden_mursu3', '{"es": "Junco Dorado", "pt-br": "Taboa Dourada"}', '{"es": "Decora tu jardín", "pt-br": "Conheça sua nova planta premiada."}'),
    ROW('garden_mursu4', '{"es": "Junco Blanco", "pt-br": "Taboa Branca"}', '{"es": "Decora tu jardín", "pt-br": "Nada mais sofisticado que o clássico."}'),
    ROW('garden_orchtree', '{"es": "Árbol Orquídea", "pt-br": "Pé de Orquídea"}', '{"es": "Decora tu jardín", "pt-br": "Do gênero Bauhinia, árvore mais bela não há."}'),
    ROW('garden_seed', '{"es": "Violeta", "pt-br": "Semente"}', '{"es": "Por fin ha florecido", "pt-br": "Com carinho, coisas boas desabrocham."}'),
    ROW('garden_staringbush', '{"es": "Arbusto Ojeador", "pt-br": "Arbusto Bizarro"}', '{"es": "Decora tu jardín", "pt-br": "Sensação de estar sendo observado."}'),
    ROW('garden_volcano', '{"es": "Planta Volcán", "pt-br": "Planta Vulcão"}', '{"es": "Expulsa un líquido viscoso", "pt-br": "Expelindo gases tóxicos, cuidado!"}'),
    ROW('garden_wall', '{"es": "Parra", "pt-br": "Videira iluminada"}', '{"es": "Decora tu jardín", "pt-br": "Uva elétrica vem daqui."}'),
    ROW('giftflowers', '{"es": "Cesta", "pt-br": "Flores Exóticas"}', '{"es": "Para decorar, ofrecer...", "pt-br": "Vivas por um longo tempo"}'),
    ROW('glass_chair', '{"es": "Butaca Cristal", "pt-br": "Cadeira Cristal"}', '{"es": "¡Frágil!", "pt-br": "Cristalina e elegante."}'),
    ROW('glass_chair*2', '{"es": "Butaca Cristal", "pt-br": "Cadeira Vidro"}', '{"es": "¡Frágil!", "pt-br": "Translúcido"}'),
    ROW('glass_chair*3', '{"es": "Butaca Cristal", "pt-br": "Cadeira Vidro"}', '{"es": "¡Frágil!", "pt-br": "Translúcido"}'),
    ROW('glass_chair*4', '{"es": "Silla Cristal", "pt-br": "Cadeira Cristal"}', '{"es": "¡Frágil!", "pt-br": "Translúcido"}'),
    ROW('glass_chair*5', '{"es": "Butaca Cristal", "pt-br": "Cadeira Vidro"}', '{"es": "¡Frágil!", "pt-br": "Translúcido"}'),
    ROW('glass_chair*6', '{"es": "Butaca Cristal", "pt-br": "Cadeira Cristal"}', '{"es": "¡Frágil!", "pt-br": "Cristalina e elegante."}'),
    ROW('glass_chair*7', '{"es": "Butaca Cristal", "pt-br": "Cadeira Cristal"}', '{"es": "¡Frágil!", "pt-br": "Cristalina e elegante."}'),
    ROW('glass_chair*8', '{"es": "Butaca Cristal", "pt-br": "Cadeira Cristal"}', '{"es": "¡Frágil!", "pt-br": "Cristalina e elegante."}'),
    ROW('glass_chair*9', '{"es": "Butaca Cristal", "pt-br": "Cadeira Cristal"}', '{"es": "¡Frágil!", "pt-br": "Cristalina e elegante."}'),
    ROW('glass_shelf', '{"es": "Estantería Cristal", "pt-br": "Estante Cristal"}', '{"es": "Tus libros, suspendidos en el aire", "pt-br": "Translúcido"}'),
    ROW('glass_sofa', '{"es": "Sofá Cristal", "pt-br": "Sofá Cristal"}', '{"es": "Visible desde cualquier ángulo", "pt-br": "Conforto translúcido."}'),
    ROW('glass_sofa*2', '{"es": "Sofá Cristal", "pt-br": "Sofá Vidro"}', '{"es": "Visible desde cualquier ángulo", "pt-br": "Translúcido"}'),
    ROW('glass_sofa*3', '{"es": "Sofá Cristal", "pt-br": "Sofá Vidro"}', '{"es": "Visible desde cualquier ángulo", "pt-br": "Translúcido"}'),
    ROW('glass_sofa*4', '{"es": "Banco Cristal", "pt-br": "Sofá Cristal"}', '{"es": "Visible desde cualquier ángulo", "pt-br": "Translúcido"}'),
    ROW('glass_sofa*5', '{"es": "Sofá Cristal", "pt-br": "Sofá Vidro"}', '{"es": "Visible desde cualquier ángulo", "pt-br": "Translúcido"}'),
    ROW('glass_sofa*6', '{"es": "Sofá Cristal", "pt-br": "Sofá Cristal"}', '{"es": "Visible desde cualquier ángulo", "pt-br": "Conforto translúcido."}'),
    ROW('glass_sofa*7', '{"es": "Sofá Cristal Verde", "pt-br": "Sofá Cristal"}', '{"es": "Visible desde cualquier ángulo", "pt-br": "Conforto translúcido."}')) AS tr (classname, names, descs)
  ON f.`classname` = tr.classname
SET f.`localized_name` = CAST(tr.names AS JSON), f.`localized_desc` = CAST(tr.descs AS JSON);
UPDATE `definition_furniture` f JOIN (VALUES
    ROW('glass_sofa*8', '{"es": "Sofá Cristal", "pt-br": "Sofá Cristal"}', '{"es": "Visible desde cualquier ángulo", "pt-br": "Conforto translúcido."}'),
    ROW('glass_sofa*9', '{"es": "Sofá Cristal", "pt-br": "Sofá Cristal"}', '{"es": "Visible desde cualquier ángulo", "pt-br": "Conforto translúcido."}'),
    ROW('glass_stool', '{"es": "Taburete Cristal", "pt-br": "Banqueta Cristal"}', '{"es": "Miniatura transparente", "pt-br": "Translucidamente chique."}'),
    ROW('glass_stool*2', '{"es": "Taburete Cristal", "pt-br": "Banco Vidro"}', '{"es": "Miniatura transparente", "pt-br": "Translúcido"}'),
    ROW('glass_stool*3', '{"es": "Taburete Cristal", "pt-br": "Banco Vidro"}', '{"es": "Miniatura transparente", "pt-br": "Translúcido"}'),
    ROW('glass_stool*4', '{"es": "Taburete Cristal", "pt-br": "Banco Cristal"}', '{"es": "Miniatura transparente", "pt-br": "Translúcido"}'),
    ROW('glass_stool*5', '{"es": "Taburete Cristal", "pt-br": "Banco Vidro"}', '{"es": "Miniatura transparente", "pt-br": "Translúcido"}'),
    ROW('glass_stool*6', '{"es": "Taburete Cristal", "pt-br": "Banqueta Cristal"}', '{"es": "Miniatura transparente", "pt-br": "Translucidamente chique."}'),
    ROW('glass_stool*7', '{"es": "Taburete Cristal Verde", "pt-br": "Banqueta Cristal"}', '{"es": "Miniatura transparente", "pt-br": "Translucidamente chique."}'),
    ROW('glass_stool*8', '{"es": "Taburete Cristal", "pt-br": "Banqueta Cristal"}', '{"es": "Miniatura transparente", "pt-br": "Translucidamente chique."}'),
    ROW('glass_stool*9', '{"es": "Taburete Cristal", "pt-br": "Banqueta Cristal"}', '{"es": "Miniatura transparente", "pt-br": "Translucidamente chique."}'),
    ROW('glass_table', '{"es": "Mesa Cristal", "pt-br": "Mesinha Cristal"}', '{"es": "¿Es cristal o es hielo?", "pt-br": "Cinderela morre de inveja."}'),
    ROW('glass_table*2', '{"es": "Mesa Cristal", "pt-br": "Mesa Vidro"}', '{"es": "¿Es cristal o es hielo?", "pt-br": "Translúcido"}'),
    ROW('glass_table*3', '{"es": "Mesa Cristal", "pt-br": "Mesa Vidro"}', '{"es": "¿Es cristal o es hielo?", "pt-br": "Translúcido"}'),
    ROW('glass_table*4', '{"es": "Mesa Cristal", "pt-br": "Mesa Cristal"}', '{"es": "¿Es cristal o es hielo?", "pt-br": "Translúcido"}'),
    ROW('glass_table*5', '{"es": "Mesa Cristal", "pt-br": "Mesa Vidro"}', '{"es": "¿Es cristal o es hielo?", "pt-br": "Translúcido"}'),
    ROW('glass_table*6', '{"es": "Mesa Cristal", "pt-br": "Mesinha Cristal"}', '{"es": "¿Es cristal o es hielo?", "pt-br": "Cinderela morre de inveja."}'),
    ROW('glass_table*7', '{"es": "Mesa Cristal", "pt-br": "Mesinha Cristal"}', '{"es": "¿Es cristal o es hielo?", "pt-br": "Cinderela morre de inveja."}'),
    ROW('glass_table*8', '{"es": "Mesa Cristal", "pt-br": "Mesinha Cristal"}', '{"es": "¿Es cristal o es hielo?", "pt-br": "Cinderela morre de inveja."}'),
    ROW('glass_table*9', '{"es": "Mesa Cristal", "pt-br": "Mesinha Cristal"}', '{"es": "¿Es cristal o es hielo?", "pt-br": "Cinderela morre de inveja."}'),
    ROW('goodie1', '{"es": "El Hombre de Mazapán", "pt-br": "Homem de Maçapão"}', '{"es": "Delicia crujiente para Perros (Energía Baja)", "pt-br": "Crocante canino (Energia Baixa)"}'),
    ROW('goodie1*1', '{"es": "El Hombre de Mazapán", "pt-br": "Boneco de Marzipã"}', '{"es": "Delicia crujiente para Perros (Energía Baja)", "pt-br": "Delícia crocante (100 pontos de energia)"}'),
    ROW('goodie1*2', '{"es": "El Hombre de Mazapán", "pt-br": "Boneco de Marzipã"}', '{"es": "Delicia crujiente para Perros (Energía Baja)", "pt-br": "Delícia crocante (100 pontos de energia)"}'),
    ROW('goodie2', '{"es": "El Ratón de chocolate", "pt-br": "Rato de Chocolate"}', '{"es": "Para Mascotas Gourmet (Energía Baja)", "pt-br": "Gourmet para Mascotes (Energia Baixa)"}'),
    ROW('goth_table', '{"es": "Mesa Gótica", "pt-br": "Mesa Gótica"}', '{"es": "El lado oscuro de Habbo", "pt-br": "Lembranças de Notre-Dame"}'),
    ROW('gothgate', '{"es": "Pórtico Gótico", "pt-br": "Portão Gótico"}', '{"es": "El lado oscuro de Habbo", "pt-br": "Lembranças de Notre-Dame"}'),
    ROW('gothic_carpet', '{"es": "Camino de adoquines", "pt-br": "Paralelepípedo"}', '{"es": "El lugar menos transitado", "pt-br": "O caminho não trilhado"}'),
    ROW('gothic_carpet2', '{"es": "Desagüe Gótico", "pt-br": "Alçapão"}', '{"es": "¿Qué habrá debajo?", "pt-br": "Não queira saber o que há embaixo"}'),
    ROW('gothic_chair', '{}', '{}'),
    ROW('gothic_chair*1', '{"es": "Silla Gótica rosa", "pt-br": "Trono Gótico"}', '{"es": "El lado oscuro de Habbo", "pt-br": "Lembranças de Notre-Dame"}'),
    ROW('gothic_chair*2', '{"es": "Silla Gótica azafrán", "pt-br": "Trono Gótico"}', '{"es": "El lado oscuro de Habbo", "pt-br": "Lembranças de Notre-Dame"}'),
    ROW('gothic_chair*3', '{"es": "Trono Gótico rojo", "pt-br": "Trono Gótico"}', '{"es": "El lado oscuro de Habbo", "pt-br": "Lembranças de Notre-Dame"}'),
    ROW('gothic_chair*4', '{"es": "Trono Gótico negro", "pt-br": "Trono Gótico"}', '{"es": "El lado oscuro de Habbo", "pt-br": "O lado negro do Habbo"}'),
    ROW('gothic_chair*5', '{"es": "Trono Gótico verde", "pt-br": "Trono Gótico"}', '{"es": "El lado oscuro de Habbo", "pt-br": "O lado negro do Habbo"}'),
    ROW('gothic_chair*6', '{"es": "Silla Gótica azul", "pt-br": "Trono Gótico"}', '{"es": "El lado oscuro de Habbo", "pt-br": "O lado negro do Habbo"}'),
    ROW('gothic_sofa', '{}', '{}'),
    ROW('gothic_sofa*1', '{"es": "Sofá Gótico rosa", "pt-br": "Sofá Gótico"}', '{"es": "El lado oscuro de Habbo", "pt-br": "Lembranças de Notre-Dame"}'),
    ROW('gothic_sofa*2', '{"es": "Sofá Gótico azafrán", "pt-br": "Sofá Gótico"}', '{"es": "El lado oscuro de Habbo", "pt-br": "Lembranças de Notre-Dame"}'),
    ROW('gothic_sofa*3', '{"es": "Sofá Gótico rojo", "pt-br": "Sofá Gótico"}', '{"es": "El lado oscuro de Habbo", "pt-br": "Lembranças de Notre-Dame"}'),
    ROW('gothic_sofa*4', '{"es": "Sofá Gótico negro", "pt-br": "Sofá Gótico"}', '{"es": "El lado oscuro de Habbo", "pt-br": "Lembranças de Notre-Dame"}'),
    ROW('gothic_sofa*5', '{"es": "Sofá Gótico verde", "pt-br": "Sofá Gótico"}', '{"es": "El lado oscuro de Habbo", "pt-br": "Lembranças de Notre-Dame"}'),
    ROW('gothic_sofa*6', '{"es": "Sofá Gótico azul", "pt-br": "Sofá Gótico"}', '{"es": "El lado oscuro de Habbo", "pt-br": "Lembranças de Notre-Dame"}'),
    ROW('gothic_stool', '{}', '{}'),
    ROW('gothic_stool*1', '{"es": "Taburete Gótico rosa", "pt-br": "Cadeira Gótica"}', '{"es": "El lado oscuro de Habbo", "pt-br": "Lembranças de Notre-Dame"}'),
    ROW('gothic_stool*2', '{"es": "Taburete Gótico azafrán", "pt-br": "Cadeira Gótica"}', '{"es": "El lado oscuro de Habbo", "pt-br": "Lembranças de Notre-Dame"}'),
    ROW('gothic_stool*3', '{"es": "Taburete Gótico rojo", "pt-br": "Cadeira Gótica"}', '{"es": "El lado oscuro de Habbo", "pt-br": "Lembranças de Notre-Dame"}'),
    ROW('gothic_stool*4', '{"es": "Silla Gótica negra", "pt-br": "Cadeira Gótica"}', '{"es": "El lado oscuro de Habbo", "pt-br": "Lembranças de Notre-Dame"}'),
    ROW('gothic_stool*5', '{"es": "Taburete Gótico verde", "pt-br": "Cadeira Gótica"}', '{"es": "El lado oscuro de Habbo", "pt-br": "Lembranças de Notre-Dame"}'),
    ROW('gothic_stool*6', '{"es": "Taburete Gótico azul", "pt-br": "Cadeira Gótica"}', '{"es": "El lado oscuro de Habbo", "pt-br": "Lembranças de Notre-Dame"}'),
    ROW('gothiccandelabra', '{"es": "Candelabro Gótico", "pt-br": "Candelabro Gótico"}', '{"es": "El lado oscuro de Habbo", "pt-br": "Lembranças de Notre-Dame"}'),
    ROW('gothicfountain', '{"es": "Fontana Gótica", "pt-br": "Fonte Gótica"}', '{"es": "El lado oscuro de Habbo", "pt-br": "O lado negro do Habbo"}'),
    ROW('gothicfountain2', '{"es": "Fontana Gótica", "pt-br": "Fonte Gótica"}', '{"es": "El lado oscuro de Habbo", "pt-br": "O lado negro do Habbo"}'),
    ROW('gothrailing', '{"es": "Verja Gótica", "pt-br": "Grade Gótica"}', '{"es": "El lado oscuro de Habbo", "pt-br": "Lembranças de Notre-Dame"}'),
    ROW('grand_piano*1', '{"es": "Piano Rosa", "pt-br": "Rosa Grand Piano"}', '{"es": "¡Tócala otra vez!", "pt-br": "Lírico e clássico"}'),
    ROW('grand_piano*2', '{"es": "Piano de cola lima", "pt-br": "Piano de Cauda"}', '{"es": "Música celestial", "pt-br": "Verde-limão é um luxo."}'),
    ROW('grand_piano*3', '{"es": "Piano Turquesa", "pt-br": "Piano de Cauda"}', '{"es": "¡Tócala otra vez!", "pt-br": "Piano de Cauda"}'),
    ROW('grand_piano*4', '{"es": "Piano Dorado", "pt-br": "Piano de Cauda"}', '{"es": "Música celestial", "pt-br": "Para grandes concertos!"}'),
    ROW('grand_piano*5', '{"es": "Piano de cola Negro", "pt-br": "Piano de Cauda"}', '{"es": "De ébano y marfil", "pt-br": "Estilo Cinza"}'),
    ROW('greek_block', '{"es": "Almohada Griega", "pt-br": "Bloco Grego"}', '{"es": "¿Confortable? Va a ser que no...", "pt-br": "Do mais puro mármore"}'),
    ROW('greek_corner', '{"es": "Esquina Griega", "pt-br": "Canto de Pilar Grego"}', '{"es": "¡Bien robusta!", "pt-br": "Imponência em qualquer Partenom"}'),
    ROW('greek_gate', '{"es": "Portón griego", "pt-br": "Portão do Olimpo"}', '{"es": "¡Adelante!", "pt-br": "Que entrem os Deuses Antigos"}'),
    ROW('greek_pillars', '{"es": "Pilares Griegos", "pt-br": "Pilares Gregos"}', '{"es": "¡Bien robustos!", "pt-br": "Monte seu próprio Partenom"}'),
    ROW('greek_seat', '{"es": "Asiento Griego", "pt-br": "Assento Grego"}', '{"es": "Un poco duro sí que es", "pt-br": "Observe a arena"}'),
    ROW('greektrophy*1', '{"es": "Trofeo griego", "pt-br": "Troféu Grego"}', '{"es": "Oro resplandeciente", "pt-br": "Ouro deslumbrante"}'),
    ROW('greektrophy*2', '{"es": "Trofeo griego", "pt-br": "Troféu Grego"}', '{"es": "Plata brillante", "pt-br": "Prata brilhante"}'),
    ROW('greektrophy*3', '{"es": "Trofeo griego", "pt-br": "Troféu Grego"}', '{"es": "Bronce impresionante", "pt-br": "Bronze de tirar o fôlego"}'),
    ROW('grunge_barrel', '{"es": "Barril Grunge", "pt-br": "Barril Grunge"}', '{"es": "¡Para calentarse las manos!", "pt-br": "Adivinha o que tem dentro?"}'),
    ROW('grunge_bench', '{"es": "Banco Grunge", "pt-br": "Banco Grunge"}', '{"es": "¡No parece muy seguro!", "pt-br": "Já cansou?"}'),
    ROW('grunge_candle', '{"es": "Vela Grunge", "pt-br": "Vela Grunge"}', '{"es": "¡Luz para los tiempos duros!", "pt-br": "Iluminando caminhos escuros"}'),
    ROW('grunge_chair', '{"es": "Silla Grunge", "pt-br": "Cadeira Grunge"}', '{"es": "Silla alternativa para gente alternativa", "pt-br": "Cadeira alternativa para pessoas alternativas"}'),
    ROW('grunge_mattress', '{"es": "Colchón", "pt-br": "Colchão Grunge"}', '{"es": "¿Tendrá un tesoro debajo?", "pt-br": "Ninguém vai dormir no chão"}'),
    ROW('grunge_radiator', '{"es": "Radiador", "pt-br": "Aquecedor Grunge"}', '{"es": "Calefacción alternativa para gente alternativa", "pt-br": "Nunca está quente o bastante"}'),
    ROW('grunge_shelf', '{"es": "Estantería Grunge", "pt-br": "Prateleira Grunge"}', '{"es": "Estantería alternativa para gente alternativa", "pt-br": "Guarda tudo aqui"}'),
    ROW('grunge_sign', '{"es": "Señal Grunge", "pt-br": "Sinal Grunge"}', '{"es": "Decoración alternativa para gente alternativa", "pt-br": "Igual a você, diferente"}'),
    ROW('grunge_table', '{"es": "Mesa Grunge", "pt-br": "Mesa Grunge"}', '{"es": "Mesa alternativa para gente alternativa", "pt-br": "Cabe uma banda inteira"}'),
    ROW('guitar_skull', '{"es": "Guitarra Calavérica", "pt-br": "Guitarra Cavérica"}', '{"es": "Simplemente... ¡hardcore!", "pt-br": "Acordes Finados."}'),
    ROW('guitar_v', '{"es": "Guitarra Eléctrica", "pt-br": "Guitarra Elétrica"}', '{"es": "¡Sonido brutal!", "pt-br": "Acordes massacrantes"}'),
    ROW('habbocake', '{"es": "El Pastel", "pt-br": "Bolo"}', '{"es": "¿Tienes hambre?", "pt-br": "Deixe uma fatia para mim!"}'),
    ROW('habboween_crypt', '{"es": "Cripta", "pt-br": "Cripta"}', '{"es": "¡Descanso eterno... y cómodo!", "pt-br": "Contém finado"}'),
    ROW('habboween_grass', '{"es": "Césped Habboween", "pt-br": "Solo Profano"}', '{"es": "¡El más podrido del mundo!", "pt-br": "Buscando a redenção musical"}'),
    ROW('habbowheel', '{"es": "Habboruleta", "pt-br": "Roda da Fortuna"}', '{"es": "Prueba fortuna", "pt-br": "A sorte está lançada"}'),
    ROW('habbowood_chair', '{"es": "Silla del Director", "pt-br": "Cadeira Diretor"}', '{"es": "Y... ¡corten!", "pt-br": "Ação"}'),
    ROW('habw_mirror', '{"es": "Espejo Habbowood", "pt-br": "Espelho de Camarim"}', '{"es": "¡Qué bien me veo!", "pt-br": "Capriche na maquiagem."}'),
    ROW('hal_cauldron', '{"es": "Caldero de Habboween", "pt-br": "Caldeirão Sinistro"}', '{"es": "¡Prepara tus mejores brebajes!", "pt-br": "Receita de pata de aranha e tripa de rato"}'),
    ROW('hal_grave', '{"es": "Tumba", "pt-br": "Tumba"}', '{"es": "¡Ocupada por un espíritu!", "pt-br": "Tumbalaca tumbalaca tumbata"}'),
    ROW('ham', '{"es": "El jamón", "pt-br": "Presunto Defumado"}', '{"es": "Es la hora de tapear", "pt-br": "Coma com apetite"}'),
    ROW('ham2', '{"es": "El hueso de jamón", "pt-br": "Presunto Devorado"}', '{"es": "Pero... aquí falta algo", "pt-br": "Você chegou tarde demais"}'),
    ROW('hc2_armchair', '{"es": "Sillón de cuero", "pt-br": "Cadeira com braços"}', '{"es": "Relax con estilo", "pt-br": "Relaxe com estilo"}'),
    ROW('hc2_barchair', '{"es": "Taburete de cuero", "pt-br": "Banqueta de líder"}', '{"es": "En las alturas", "pt-br": "Sente no topo"}'),
    ROW('hc2_biglamp', '{"es": "Lámpara de diseño", "pt-br": "Lâmpada Preta"}', '{"es": "Luz sofisticada", "pt-br": "Iluminação sofisticada"}'),
    ROW('hc2_carpet', '{"es": "Alfombra de diseño", "pt-br": "Tapete da moda"}', '{"es": "Confort absoluto", "pt-br": "Conforto de luxo"}'),
    ROW('hc2_cart', '{"es": "Minibar de diseño", "pt-br": "Carrinho HC"}', '{"es": "Camarero no incluido", "pt-br": "Mimos sobre rodas."}'),
    ROW('hc2_coffee', '{"es": "Máquina de café", "pt-br": "Máquina de Expresso"}', '{"es": "Lo mejor de las mañanas", "pt-br": "O legítimo italiano é escrito com s mesmo."}'),
    ROW('hc2_divider', '{"es": "Divisor de diseño", "pt-br": "Divisória preta"}', '{"es": "Divide con glamour", "pt-br": "Divide o mundo"}'),
    ROW('hc2_dvn', '{"es": "Diván de cuero", "pt-br": "Divisória de couro"}', '{"es": "Adiós al estrés", "pt-br": "Maleável"}'),
    ROW('hc2_frplc', '{"es": "Chimenea de diseño", "pt-br": "Lareira Suave"}', '{"es": "¡Qué buena temperatura!", "pt-br": "A chama do sucesso"}'),
    ROW('hc2_sofa', '{"es": "Sofá de cuero", "pt-br": "Sofá de líder"}', '{"es": "Asientos con estilo", "pt-br": "Sente com estilo"}'),
    ROW('hc2_sofatbl', '{"es": "Mesa de Cristal", "pt-br": "Mesa de vidro"}', '{"es": "Pura sofisticación", "pt-br": "Tenha status"}'),
    ROW('hc2_vase', '{"es": "Jarrón de diseño", "pt-br": "Vaso preto"}', '{"es": "La elegancia, ante ti", "pt-br": "Formas elegantes"}'),
    ROW('hc3_bard', '{"es": "Barra HC", "pt-br": "Balcão HC"}', '{"es": "El lujo ya está aquí", "pt-br": "Crie uma loja"}'),
    ROW('hc3_dc', '{"es": "Diván HC", "pt-br": "Divã HC"}', '{"es": "Vive la vida", "pt-br": "Para sentar e desabafar."}'),
    ROW('hc3_divider', '{"es": "Divisor HC", "pt-br": "Divisória HC"}', '{"es": "Administra tu espacio", "pt-br": "Gerencie seu espaço"}'),
    ROW('hc3_hugelamp', '{"es": "Lámpara HC", "pt-br": "Lâmpada HC"}', '{"es": "Menuda iluminación", "pt-br": "Traz um enorme status"}'),
    ROW('hc3_light', '{"es": "Fluorescente HC", "pt-br": "Luz HC"}', '{"es": "Sé el centro de atención", "pt-br": "Fique sempre no foco"}'),
    ROW('hc3_shelf', '{"es": "Mueble HC", "pt-br": "Prateleiras HC"}', '{"es": "Guarda tus tesoros", "pt-br": "Guarde seus prêmios"}'),
    ROW('hc3_sofa', '{"es": "Sofá HC", "pt-br": "Sofá HC"}', '{"es": "Puro comfort", "pt-br": "Conforto com estilo"}'),
    ROW('hc3_stool', '{"es": "Taburete HC", "pt-br": "Banqueta HC"}', '{"es": "Alto y confortable", "pt-br": "Alto e avante"}'),
    ROW('hc3_table', '{"es": "Mesa HC", "pt-br": "Mesa de Café HC"}', '{"es": "Para reuniones sociales", "pt-br": "Para eventos sociais"}'),
    ROW('hc3_walldeco', '{"es": "Cuadro HC", "pt-br": "Arte de Muro HC"}', '{"es": "Viva el cubismo", "pt-br": "O Cubismo está vivo"}'),
    ROW('hc_bkshlf', '{"es": "Estantería Medieval", "pt-br": "Estante Medieval"}', '{"es": "¡Vamos a estudiar!", "pt-br": "Armadura dos Templários"}'),
    ROW('hc_btlr', '{"es": "Mayordomo electrónico", "pt-br": "Mordomo Elétrico"}', '{"es": "¡Esto es vida!", "pt-br": "O outro Mordomo é bem mais legal"}'),
    ROW('hc_chr', '{"es": "Silla Majestic", "pt-br": "Cadeira Majestosa"}', '{"es": "Comodidad real", "pt-br": "Conforto real"}'),
    ROW('hc_crpt', '{"es": "Alfombra Persa", "pt-br": "Tapete Persa"}', '{"es": "El último grito en artesanía", "pt-br": "Feito à mão"}'),
    ROW('hc_crtn', '{"es": "Cortinas de antaño", "pt-br": "Cortina Vitoriana"}', '{"es": "¡Un poco de privacidad!", "pt-br": "Extrema privacidade"}'),
    ROW('hc_djset', '{"es": "El Gramófono", "pt-br": "Gramofone"}', '{"es": "Una buena sesión de música", "pt-br": "Da época do guaraná de rolha"}'),
    ROW('hc_dsk', '{"es": "Escritorio HC", "pt-br": "Escrivaninha"}', '{"es": "Para grandes estudiantes", "pt-br": "Para Habbo estudantes"}'),
    ROW('hc_frplc', '{"es": "La Gran Chimenea", "pt-br": "Fornalha"}', '{"es": "¡Te vas a asar!", "pt-br": "Tão quente! Para Habbo siderurgia."}'),
    ROW('hc_lmp', '{"es": "Lámpara HC", "pt-br": "Lampião"}', '{"es": "Luz potente", "pt-br": "Fique sempre iluminado"}'),
    ROW('hc_lmpst', '{"es": "Farola victoriana", "pt-br": "Poste Vitoriano"}', '{"es": "¡Por una atmósfera sombría!", "pt-br": "Da época da Revolução Industrial"}'),
    ROW('hc_machine', '{"es": "La Máquina", "pt-br": "Máquina Científica"}', '{"es": "Para científicos algo alocados", "pt-br": "Invenção incompreensível!"}'),
    ROW('hc_rllr', '{"es": "Rollers HC", "pt-br": "Roller HC"}', '{"es": "El transporte más moderno", "pt-br": "Transporte de primeira classe"}'),
    ROW('hc_rntgn', '{"es": "Pantalla de rayos X", "pt-br": "Raio-X"}', '{"es": "¡Estás en los huesos! ;)", "pt-br": "Acredite ou não!"}'),
    ROW('hc_tbl', '{"es": "Mesa Nórdica", "pt-br": "Mesa Nórdica"}', '{"es": "Perfecta para grandes banquetes", "pt-br": "Perfeita para banquetes"}'),
    ROW('hc_trll', '{"es": "Carrito", "pt-br": "Carrinho de Bebidas"}', '{"es": "Para cenas copiosas", "pt-br": "Só para jantares chiques"}'),
    ROW('hc_tv', '{"es": "Set de TV", "pt-br": "Estúdio de TV"}', '{"es": "¿Plasma? ¡Mejor un HC!", "pt-br": "Esqueça a TV plasma"}'),
    ROW('hc_wall_lamp', '{"es": "Lámpara Retro", "pt-br": "Lâmpada Retrô"}', '{"es": "¡Muy chic!", "pt-br": "Chiquérrimo!"}'),
    ROW('hcamme', '{"es": "El Jacuzzy", "pt-br": "Banheira Master"}', '{"es": "Tómate un respiro", "pt-br": "É hora de se molhar"}'),
    ROW('hcc_chair', '{"es": "Silla HC", "pt-br": "Cadeira do Clube"}', '{"es": "Un asiento de lujo", "pt-br": "Nada menos que o máximo."}'),
    ROW('hcc_crnr', '{"es": "Esquina HC", "pt-br": "Canto de Vidro"}', '{"es": "Un detalle de calidad", "pt-br": "Nada menos que o máximo."}'),
    ROW('hcc_dvdr', '{"es": "Separador HC", "pt-br": "Divisória de Vidro"}', '{"es": "Crea un ambiente especial", "pt-br": "Nada menos que o máximo."}'),
    ROW('hcc_minibar', '{"es": "Mini Minibar HC", "pt-br": "Frigobar do Clube"}', '{"es": "Todo fresco en verano o invierno", "pt-br": "Nada menos que o máximo."}'),
    ROW('hcc_shelf', '{"es": "Estante HC", "pt-br": "Estante de Livros"}', '{"es": "Sostén todo con glamour", "pt-br": "Nada menos que o máximo."}'),
    ROW('hcc_sofa', '{"es": "Sofá Especial HC", "pt-br": "Sofá do Clube"}', '{"es": "Tapizado relajante", "pt-br": "Nada menos que o máximo."}'),
    ROW('hcc_sofachair', '{"es": "Gran Sofá HC", "pt-br": "Poltrona Reclinável"}', '{"es": "Para los más comodones", "pt-br": "Nada menos que o máximo."}'),
    ROW('hcc_stool', '{"es": "Taburete HC", "pt-br": "Banqueta do Clube"}', '{"es": "Siéntate a tus anchas", "pt-br": "Nada menos que o máximo."}'),
    ROW('hcc_table', '{"es": "Mesa HC", "pt-br": "Mesa Tampo de Vidro"}', '{"es": "¡Pon la mesa!", "pt-br": "Nada menos que o máximo."}'),
    ROW('hcsohva', '{"es": "Sofá Trono HC", "pt-br": "Sofá-Trono"}', '{"es": "¿Te sientes en una nube?", "pt-br": "Para traseiros reais..."}'),
    ROW('heart', '{"es": "El Corazón Gigante", "pt-br": "Coração Gigante"}', '{"es": "Latidos volumen brutal", "pt-br": "Cheio de Amor"}'),
    ROW('heartsofa', '{"es": "Sofa de Corazón", "pt-br": "Sofá-Coração"}', '{"es": "Para sentarse en compañía", "pt-br": "Perfeito para abraços apaixonados"}'),
    ROW('hockey_light', '{"es": "Alarma", "pt-br": "Alarme"}', '{"es": "Se ilumina cuando empieza la competición", "pt-br": "Desligue."}'),
    ROW('hockey_score', '{"es": "El Contador", "pt-br": "Placar"}', '{"es": "Verde cuenta, Rojo anula", "pt-br": "...para registrar os seus pontos"}'),
    ROW('hologram', '{"es": "Holoboy", "pt-br": "Holo Mano"}', '{"es": "Tantas vueltas", "pt-br": "Como se fosse mágica..."}'),
    ROW('house', '{"es": "La Casita de Chocolate", "pt-br": "Casa de Chocolate"}', '{"es": "Ñam, ñam", "pt-br": "Boa o suficiente para comer"}'),
    ROW('house2', '{"es": "Era la casa de chocolate", "pt-br": "Casa de Chocolate Mordida"}', '{"es": "La puerta estaba riquísima", "pt-br": "Boa o suficiente para comer"}'),
    ROW('hrella_poster_1', '{"es": "Escotilla", "pt-br": "Janela Habborella"}', '{"es": "Ilumina tu camarote", "pt-br": "Que bela vista, não?"}'),
    ROW('hrella_poster_2', '{"es": "Salvavidas", "pt-br": "Salva-Vidas"}', '{"es": "Para los momentos de apuro en el Lido", "pt-br": "Não saia de casa sem ele"}'),
    ROW('hrella_poster_3', '{"es": "Ancla", "pt-br": "Âncora"}', '{"es": "¡Evita ir a la deriva!", "pt-br": "Terra à vista!"}'),
    ROW('hw08_xray', '{"es": "Luz de Rayos X", "pt-br": "Raio-X"}', '{"es": "Para ver cualquier fractura", "pt-br": "Diga xiiiiiis"}'),
    ROW('hw_08_xray', '{"es": "Panel de Rayos X", "pt-br": "Raio-X"}', '{"es": "¿Algún hueso roto?", "pt-br": "Diga xiiiiiis"}'),
    ROW('hween08_bath', '{"es": "Bañera sangrienta", "pt-br": "Banheira de Sangue"}', '{"es": "No olvides frotar detrás de las orejas", "pt-br": "Um banho de espuma vermelha"}'),
    ROW('hween08_bath2', '{"es": "Bañera babosa", "pt-br": "Banheira de Limo"}', '{"es": "No olvides frotar detrás de las orejas", "pt-br": "Lave atrás da orelha"}'),
    ROW('hween08_bbag', '{"es": "Funda mortuoria", "pt-br": "Saco de defunto"}', '{"es": "El que entra.... ¡no sale!", "pt-br": "Ui, que medo"}'),
    ROW('hween08_bed', '{"es": "Cama sangrienta", "pt-br": "Cama Ensanguentada"}', '{"es": "El terror se huele en el ambiente...", "pt-br": "Só umas manchinhas..."}'),
    ROW('hween08_bed2', '{"es": "Cama pegajosa", "pt-br": "Cama Pegajosa"}', '{"es": "¡Es difícil levantarse de ella!", "pt-br": "Você se atreve?"}'),
    ROW('hween08_bio', '{"es": "Alerta biológica", "pt-br": "Aviso BioContaminação"}', '{"es": "¡Cuidado con contaminarte!", "pt-br": "Faça o piquenique longe daqui"}'),
    ROW('hween08_curtain', '{"es": "Cortina sangrienta", "pt-br": "Cortina de Sangue"}', '{"es": "¿Quién se esconde detrás?", "pt-br": "Para grandes espetáculos"}'),
    ROW('hween08_curtain2', '{"es": "Cortina babosa", "pt-br": "Cortina de Limo"}', '{"es": "¿Quién se esconde detrás?", "pt-br": "O que tem atrás dela?"}'),
    ROW('hween08_defibs', '{"es": "Desfibrilador sangriento", "pt-br": "Desfibrilador Ensanguentado"}', '{"es": "Uno, dos, tres... ¡Revive!", "pt-br": "Afaste os curiosos"}'),
    ROW('hween08_defibs2', '{"es": "Desfibrilador pegajoso", "pt-br": "Desfibrilador Pegajoso"}', '{"es": "Uno, dos tres... ¡Revive!", "pt-br": "Tenha sempre um na bolsa"}'),
    ROW('hween08_manhole', '{"es": "Alcantarilla", "pt-br": "Bueiro"}', '{"es": "A ver quién es el guapo que baja...", "pt-br": "Eca, que cheiro"}'),
    ROW('hween08_rad', '{"es": "Alerta Radioactiva", "pt-br": "Quarto Radioativo"}', '{"es": "Estás entrando en una zona peligrosa...", "pt-br": "Conselho: não se aproxime"}'),
    ROW('hween08_sink', '{"es": "Lavabo sangriento", "pt-br": "Pia de Sangue"}', '{"es": "¡No veas cómo queda tras las operaciones!", "pt-br": "Lave bem as mãos"}'),
    ROW('hween08_sink2', '{"es": "Lavabo baboso", "pt-br": "Pia de Limo"}', '{"es": "Para dejar las manos bien ¿limpias?", "pt-br": "Escovaria os dentes aí?"}'),
    ROW('hween08_trll', '{"es": "Mesa de instrumentos", "pt-br": "Mesa de instrumentos"}', '{"es": "¡Para todo tipo de operaciones!", "pt-br": "Passa o bisturi"}'),
    ROW('hween08_wndw', '{"es": "Ventana Habboween Grande", "pt-br": "Janela Quebrada"}', '{"es": "Mejor no la cierres del todo...", "pt-br": "Mantenha fechada"}'),
    ROW('hween08_wndwb', '{"es": "Ventana Habboween Pequeña", "pt-br": "Janela Quebrada"}', '{"es": "Será mejor no acercarse a ella", "pt-br": "Mantenha fechada"}'),
    ROW('hween09_chair', '{"es": "Silla Habboween", "pt-br": "Cadeira relíquia"}', '{"es": "¿Te atrapará?", "pt-br": "Assento de palha. Beleza e requinte"}'),
    ROW('hween09_chandelier', '{"es": "Lámpara Habboween", "pt-br": "Lustre de velas"}', '{"es": "No te quedes a oscuras", "pt-br": "Iluminação à moda antiga"}'),
    ROW('hween09_crnr1', '{"es": "Esquina Mansión", "pt-br": "Canto empoeirado"}', '{"es": "Muchos secretos deben esconder estas paredes", "pt-br": "Um bom lugar para assombrações"}'),
    ROW('hween09_curt', '{"es": "Cortinas Habboween", "pt-br": "Cortina rasgada"}', '{"es": "¿Que habrá detrás?", "pt-br": "O que aconteceu aqui?"}'),
    ROW('hween09_floor', '{"es": "Suelo de madera", "pt-br": "Assoalho de madeira"}', '{"es": "¿Repleto de termitas?", "pt-br": "Ele range a cada passo"}'),
    ROW('hween09_ghost', '{"es": "Fantasma", "pt-br": "Fantasma"}', '{"es": "¿No querías compañía?", "pt-br": "Palidamente plácido"}'),
    ROW('hween09_hatch', '{"es": "Trampilla", "pt-br": "Alçapão antigo"}', '{"es": "¿Te atreves a bajar al sótano?", "pt-br": "Cria coragem pra entrar"}'),
    ROW('hween09_jar', '{"es": "Tarro Habboween", "pt-br": "Formol"}', '{"es": "Para guardar tesoros", "pt-br": "O que você quer conservar aqui?"}'),
    ROW('hween09_mirror', '{"es": "Espejo Antiguo", "pt-br": "Espelho assombrado"}', '{"es": "¿Se reflejarán en él los fantasmas?", "pt-br": "O que é isso atrás do seu reflexo?"}'),
    ROW('hween09_organ', '{"es": "Órgano del Terror", "pt-br": "Órgão"}', '{"es": "Lo mejor para poner los Píxeles de punta", "pt-br": "Está tocando a nossa música..."}'),
    ROW('hween09_paint', '{"es": "Cuadro Habboween", "pt-br": "Pintura assombrada"}', '{"es": "¿Quién da más miedo?", "pt-br": "Não encare!"}'),
    ROW('hween09_stonewall', '{"es": "Muro Mansión", "pt-br": "Parede de pedras"}', '{"es": "De piedra añeja", "pt-br": "Só um fantasma para atravessá-la"}'),
    ROW('hween09_table', '{"es": "Mesa Habboween", "pt-br": "Mesa de banquete"}', '{"es": "¡Fantasmas, a comeeeer!", "pt-br": "Lembre-se sempre dos bons modos à mesa"}'),
    ROW('hween09_treewall', '{"es": "Arboleda Habboween", "pt-br": "Floresta morta"}', '{"es": "¿Te atreves a entrar en el bosque?", "pt-br": "A clorofila foi embora"}'),
    ROW('hween09_tv', '{"es": "TV sin color", "pt-br": "Televisão antiga"}', '{"es": "Sólo sintoniza programas terroríficos", "pt-br": "Cuidado com o que pode sair dela"}'),
    ROW('hween09_wall1', '{"es": "Pared Mansión", "pt-br": "Parede antiga"}', '{"es": "Muchos secretos deben esconder estas paredes", "pt-br": "Cuidado! As paredes têm ouvidos"}'),
    ROW('hween09_win', '{"es": "Ventana Habboween", "pt-br": "Janela Sombria"}', '{"es": "¿El miedo atenaza tus Píxeles?", "pt-br": "Abra a janela e deixe o medo entrar"}'),
    ROW('hyacinth1', '{"es": "Jacintos Rojos", "pt-br": "Jacinto Rosa"}', '{"es": "Flores de Pascua", "pt-br": "Uma bela flor"}'),
    ROW('hyacinth2', '{"es": "Jacintos Azules", "pt-br": "Jacinto Azul"}', '{"es": "Flores en invierno", "pt-br": "Uma bela flor"}'),
    ROW('igor_seat', '{"es": "Sillón Supervillano", "pt-br": "Massageador de Vértebras"}', '{"es": "¿Qué maldad se te ha ocurrido?", "pt-br": "Silêncio. Gênio pensando"}'),
    ROW('industrialfan', '{"es": "Turbina industrial", "pt-br": "Turbina Industrial"}', '{"es": "¡Pura potencia!", "pt-br": "Turbina Industrial"}'),
    ROW('joulutahti', '{"es": "Flor de Pascua", "pt-br": "Poinsétia"}', '{"es": "¡Qué bien huele!", "pt-br": "Dá paz ao ambiente..."}'),
    ROW('jp_bamboo', '{"es": "Bamboo", "pt-br": "Bambu"}', '{"es": "¡No te pierdas!", "pt-br": "Cuidado com as cobras"}'),
    ROW('jp_drawer', '{"es": "Cajón", "pt-br": "Caixa Japonesa"}', '{"es": "¡Para guardar tus tesoros!", "pt-br": "Guarde suas riquezas"}'),
    ROW('jp_irori', '{"es": "Cocina japonesa", "pt-br": "Irori"}', '{"es": "¡Sashimi en su punto!", "pt-br": "Tradicional. Aquece o rango."}'),
    ROW('jp_katana1', '{"es": "Katana Roja", "pt-br": "Katana Vermelha"}', '{"es": "¡Bien afilada!", "pt-br": "Símbolo dos antigos Samurais"}'),
    ROW('jp_katana2', '{"es": "Katana Azul", "pt-br": "Katana Azul"}', '{"es": "¡Bien afilada!", "pt-br": "Símbolo dos antigos Samurais"}'),
    ROW('jp_katana3', '{"es": "Katana Verde", "pt-br": "Katana Verde"}', '{"es": "¡Bien afilada!", "pt-br": "Símbolo dos antigos Samurais"}'),
    ROW('jp_lantern', '{"es": "Linterna", "pt-br": "Lanterna Japonesa"}', '{"es": "¡No te quedes a oscuras!", "pt-br": "Jardins Imperiais em seu Quarto"}'),
    ROW('jp_ninjastars', '{"es": "Estrella Ninja", "pt-br": "Shuriken"}', '{"es": "Ehhh, ¡que esto no es un frisbee!", "pt-br": "Cheia de mitos e lendas."}'),
    ROW('jp_pillow', '{"es": "Cojín Japonés", "pt-br": "Almofada Japonesa"}', '{"es": "¡Esto sí que es comodidad!", "pt-br": "Senta de perna cruzada"}'),
    ROW('jp_rare', '{"es": "Fuente de Bambú", "pt-br": "Odoshi"}', '{"es": "¡Sonido japón!", "pt-br": "A paciência na espera..."}'),
    ROW('jp_sheet1', '{"es": "Caligrafía Japonesa", "pt-br": "Quadro Ziritsu"}', '{"es": "Está bien claro, ¿no?", "pt-br": "Sabedoria Oriental"}'),
    ROW('jp_sheet2', '{"es": "Caligrafía Japonesa II", "pt-br": "Quadro Ninja"}', '{"es": "Está bien claro, ¿no?", "pt-br": "Sabedoria Oriental"}'),
    ROW('jp_sheet3', '{"es": "Paisaje Japonés", "pt-br": "Quadro Hokusai"}', '{"es": "Está bien claro, ¿no?", "pt-br": "Sabedoria Oriental"}'),
    ROW('jp_table', '{"es": "Mesita japonesa", "pt-br": "Mesa Japonesa"}', '{"es": "¡Repleta de manjares!", "pt-br": "Pratos tradicionais"}'),
    ROW('jp_tatami', '{"es": "Pequeño Tatami", "pt-br": "Tatame Pequeno"}', '{"es": "¡Zapatos fuera!", "pt-br": "Tire os sapatos"}'),
    ROW('jp_tatami2', '{"es": "Gran Tatami", "pt-br": "Tatame"}', '{"es": "¡Zapatos fuera!", "pt-br": "Tire os sapatos"}'),
    ROW('jp_teamaker', '{"es": "Máquina de Té", "pt-br": "Casinha de Chá Japonesa"}', '{"es": "Aroma japonés", "pt-br": "Chá quentinho a toda hora"}'),
    ROW('jp_tray1', '{"es": "Sushi Maguro", "pt-br": "Mesinha com Salmão"}', '{"es": "¡Delicioso sushi de atún!", "pt-br": "Delicioso prato oriental"}'),
    ROW('jp_tray2', '{"es": "Sushi Ika", "pt-br": "Mesinha com Arroz"}', '{"es": "¡Delicioso sushi de calamar!", "pt-br": "Delicioso prato oriental"}'),
    ROW('jp_tray3', '{"es": "Sushi Ikura", "pt-br": "Mesinha com Sushi"}', '{"es": "¡Delicioso sushi de caviar!", "pt-br": "Delicioso prato oriental"}'),
    ROW('jp_tray4', '{"es": "Sushi Uni", "pt-br": "Mesinha com Tempura"}', '{"es": "¡Delicioso sushi de erizo de mar!", "pt-br": "Delicioso prato oriental"}'),
    ROW('jp_tray5', '{"es": "Sushi Tamago", "pt-br": "Mesinha com Yakimeshi"}', '{"es": "¡Delicioso sushi de tortilla francesa!", "pt-br": "Delicioso prato oriental"}'),
    ROW('jp_tray6', '{"es": "Sushi Kohada", "pt-br": "Mesinha com Sashimi"}', '{"es": "¡Delicioso sushi de caballa!", "pt-br": "Delicioso prato oriental"}'),
    ROW('jukebox*1', '{"es": "Jukebox", "pt-br": "Jukebox"}', '{"es": "¡Para tus días más felices!", "pt-br": "Tuas Trax tocam aqui."}'),
    ROW('jukebox_ptv*1', '{"es": "Jukebox Pacha TV", "pt-br": "Jukebox Pacha TV"}', '{"es": "Jukebox Pacha TV", "pt-br": "Jukebox Pacha TV"}'),
    ROW('kinkysofa', '{"es": "kinkysofa", "pt-br": "kinkysofa"}', '{"es": "En ningún otro lugar estarás tan cómodo"}'),
    ROW('ktchn_bBlock', '{"es": "Mesa carnicero", "pt-br": "Mesa para Cortes"}', '{"es": "¡Marchando unos filetes!", "pt-br": "Sempre com um adulto por perto, é claro"}'),
    ROW('ktchn_cornr', '{"es": "Esquina Cocina", "pt-br": "Canto de Cozinha"}', '{"es": "¡El algodón no engaña!", "pt-br": "Arredondado para facilitar a limpeza"}'),
    ROW('ktchn_countr_1', '{"es": "Encimera", "pt-br": "Balcão Quadrado"}', '{"es": "¡Impoluta!", "pt-br": "Tudo organizado"}'),
    ROW('ktchn_countr_2', '{"es": "Gran Encimera", "pt-br": "Balcão Retângulo"}', '{"es": "¡Impoluta!", "pt-br": "Me convida pro jantar, tá?"}'),
    ROW('ktchn_desk', '{"es": "Mesita accesorios", "pt-br": "Mesinha com prateleira"}', '{"es": "¡Todo lo necesario para conocer, en ella!", "pt-br": "Arrumação é a palavra de ordem"}'),
    ROW('ktchn_dvdr', '{"es": "Divisor Cocina", "pt-br": "Divisória de Cozinha"}', '{"es": "¡El algodón no engaña!", "pt-br": "Eu já disse pra não falar de boca cheia, menino"}'),
    ROW('ktchn_fridge', '{"es": "Súper frigorífico", "pt-br": "Freezer"}', '{"es": "¡No hace escarcha!", "pt-br": "Onde eu guardo meu queijo, cenoura, peixe, tudo frequinho!"}'),
    ROW('ktchn_gate', '{"es": "Puerta Cocina", "pt-br": "Porta da Cozinha"}', '{"es": "Cuidado que voyyyyy", "pt-br": "Daqui só sai coisa boa"}'),
    ROW('ktchn_hlthNut', '{"es": "Franz", "pt-br": "Franz"}', '{"es": "Corre, Habbo, corre...", "pt-br": "Saúde é o que interessa, o resto não tem pressa"}'),
    ROW('ktchn_inspctr', '{"es": "Hans", "pt-br": "Inspetor Sanitário"}', '{"es": "Inspector de Sanidad", "pt-br": "Melhor que um amigo imaginário"}'),
    ROW('ktchn_knives', '{"es": "Cuchillos", "pt-br": "Faqueiro"}', '{"es": "¡Bien afilados!", "pt-br": "Uma para cada tipo de comida"}'),
    ROW('ktchn_light', '{"es": "Fluorescentes", "pt-br": "Luz de Cozinha"}', '{"es": "¡Luz artificial!", "pt-br": "Nada de cozinhar no escuro"}'),
    ROW('ktchn_oven', '{"es": "Horno Cocina", "pt-br": "Forno Embutido"}', '{"es": "¡De él salen unos platos suculentos!", "pt-br": "Fica de olho no suflê"}'),
    ROW('ktchn_plates', '{"es": "Platos Sucios", "pt-br": "Pratos prontos"}', '{"es": "Alguien va a tener que fregarlos...", "pt-br": "A praticidade da vida moderna"}'),
    ROW('ktchn_pots', '{"es": "Sartenes y cacerolas", "pt-br": "Panelas"}', '{"es": "¡Indispensables en toda cocina!", "pt-br": "A mais velha é que faz comida boa"}'),
    ROW('ktchn_sink', '{"es": "Fregadero", "pt-br": "Pia de Alumínio"}', '{"es": "¿Algo que lavar?", "pt-br": "Dura uma eternidade - e é fácil de limpar!"}'),
    ROW('ktchn_stove', '{"es": "Cocina", "pt-br": "Fogão"}', '{"es": "¡A cocinar!", "pt-br": "Não importa quantas bocas, o 4Queijos sempre come mais"}'),
    ROW('ktchn_trash', '{"es": "Cubo de la Basura", "pt-br": "Lixeira"}', '{"es": "¡Puajjj, que mal huele!", "pt-br": "Quem vai levar o lixo pra fora?"}'),
    ROW('ktchn_wall', '{"es": "Pared Cocina", "pt-br": "Parede de Cozinha"}', '{"es": "¡El algodon no engaña!", "pt-br": "Mantenha sempre limpa"}'),
    ROW('lamp2_armas', '{"es": "Velas", "pt-br": "Vela Rústica"}', '{"es": "De cera y quizás ¿algo de encanto?", "pt-br": "À meia luz, rusticamente."}'),
    ROW('lamp_armas', '{"es": "Lámpara", "pt-br": "Lâmpada de Mesa"}', '{"es": "¡Cuánta luz!", "pt-br": "Luz ambiente é essencial"}'),
    ROW('lamp_basic', '{"es": "Lámpara Pura", "pt-br": "Lâmpada Pura"}', '{"es": "Una lámpara sofisticada", "pt-br": "Anime o ambiente com esta luz sofisticada"}'),
    ROW('landscape', '{}', '{}'),
    ROW('laptopdesk', '{"es": "Mesa para informáticos", "pt-br": "Mesinha de Laptop"}', '{"es": "Portátil, Impresora, Cds... ¡Cuenta con lo necesario!", "pt-br": "Onde foi que eu deixei aquele papel?"}'),
    ROW('lc_anemone', '{"es": "Anémona", "pt-br": "Anêmona"}', '{"es": "Se mueve al ritmo de las corrientes", "pt-br": "Um bonsai marinho"}'),
    ROW('lc_chair', '{"es": "Silla", "pt-br": "Cadeira Perdida"}', '{"es": "Madera pasada por agua", "pt-br": "Mas essa é minha"}'),
    ROW('lc_coral_divider_hi', '{"es": "Gran Coral", "pt-br": "Divisória Alta de Corais"}', '{"es": "¿Esconderá alguna perla?", "pt-br": "Quem dera ser um peixe..."}'),
    ROW('lc_coral_divider_low', '{"es": "Coral", "pt-br": "Divisória Baixa de Corais"}', '{"es": "¿Esconderá alguna perla?", "pt-br": "...para enfeitar de corais tua cintura"}'),
    ROW('lc_corner', '{"es": "Esquina", "pt-br": "Canto Perdido"}', '{"es": "Madera pasada por agua", "pt-br": "Não confunda com perdido no canto"}'),
    ROW('lc_crab1', '{"es": "Guarida cangrejo", "pt-br": "Toca do Siri"}', '{"es": "Píllalo antes de que se esconda", "pt-br": "Cuidado onde metes a mão"}'),
    ROW('lc_crab2', '{"es": "Cangrejo", "pt-br": "Saulo Siri"}', '{"es": "¡Ojo con sus pinzas!", "pt-br": "Kani de primeira"}'),
    ROW('lc_desk', '{"es": "Barra", "pt-br": "Balcão Perdido"}', '{"es": "Madera pasada por agua", "pt-br": "Comece a achar suas coisas"}'),
    ROW('lc_glass_floor', '{"es": "Mini acuario", "pt-br": "Chão de Vidro"}', '{"es": "A miles de metros de profundidad", "pt-br": "Vamos pular, vamos pular, vamos pulaaar"}'),
    ROW('lc_medusa1', '{"es": "Gran Medusa", "pt-br": "Lâmpada de Medusa Grande"}', '{"es": "¡Picores al instante!", "pt-br": "Olha o veneno aí"}'),
    ROW('lc_medusa2', '{"es": "Medusa", "pt-br": "Lâmpada de Medusa"}', '{"es": "¡Picores al instante!", "pt-br": "Iluminando grandes profundidades"}'),
    ROW('lc_stool', '{"es": "Taburete", "pt-br": "Banqueta Perdida"}', '{"es": "Madera pasada por agua", "pt-br": "Tão pequenina..."}'),
    ROW('lc_table', '{"es": "Mesa", "pt-br": "Mesa Perdida"}', '{"es": "Madera pasada por agua", "pt-br": "Mas aqui a galera se encontra"}'),
    ROW('lc_tile1', '{"es": "Bloques", "pt-br": "Piso de Mármore Perdido"}', '{"es": "¡Has tocado fondo!", "pt-br": "Grandes descobertas"}'),
    ROW('lc_tile2', '{"es": "Piedras", "pt-br": "Piso de Pedra Polida"}', '{"es": "¡Has tocado fondo!", "pt-br": "Uma massoterapia nos pés"}'),
    ROW('lc_tubes_corners', '{"es": "Esquina Tubo", "pt-br": "Tubo de Esquina"}', '{"es": "¡En el fondo del mar!", "pt-br": "Mas esse aqui é de família"}'),
    ROW('lc_tubes_straight', '{"es": "Barra Tubo", "pt-br": "Tubo Perdido"}', '{"es": "¡En el fondo del mar!", "pt-br": "Siga em frente"}'),
    ROW('lc_wall1', '{"es": "Roca", "pt-br": "Muro de Rocha"}', '{"es": "¡Siempre bajo el mar!", "pt-br": "Firme e resistente"}'),
    ROW('lc_wall2', '{"es": "Roca con algas", "pt-br": "Muro de Corais"}', '{"es": "¡Siempre bajo el mar!", "pt-br": "Um toque de personalidade"}'),
    ROW('lc_window1', '{"es": "Ventana de Madera", "pt-br": "Janela Perdida"}', '{"es": "Madera pasada por agua", "pt-br": "Espia para se achar"}'),
    ROW('lc_window2', '{"es": "Ventana Acuario", "pt-br": "Janela de Aquário"}', '{"es": "A ver qué hay detrás...", "pt-br": "O mundo marinho pertinho"}'),
    ROW('legotrophy', '{"es": "Trofeo Baloncesto", "pt-br": "Troféu de Basquete"}', '{"es": "Sólo para los Habbos atletas", "pt-br": "Para a equipe vencedora"}'),
    ROW('lostc_merdragon', '{"es": "Leviathan", "pt-br": "Leviatã"}', '{"es": "Enero 2009", "pt-br": "Colecionável Janeiro/2009"}'),
    ROW('lostc_octopus', '{"es": "Kraken", "pt-br": "Kraken"}', '{"es": "Que no te dé el abrazo del... ¡pulpo!", "pt-br": "Colecionável Fevereiro/2009"}'),
    ROW('lostc_teleport', '{"es": "Architeuthis", "pt-br": "Architeuthis"}', '{"es": "El teletransporte del futuro va por mar", "pt-br": "Seja abduzido por tentáculos."}'),
    ROW('lt_bughill', '{"es": "Hormiguero", "pt-br": "Formigueiro"}', '{"es": "¡Sus habitantes son todas carnívoras!", "pt-br": "Ai ai ai meu pé!"}'),
    ROW('lt_gate', '{"es": "Portón de la Tribu", "pt-br": "Portão de Pedra"}', '{"es": "¿Te atreves a traspasarlo?", "pt-br": "Pedras precisamente encaixadas"}'),
    ROW('lt_jngl_wall', '{"es": "Súper Jungla", "pt-br": "Floresta Sombria"}', '{"es": "¿Te atreves a penetrar en ella?", "pt-br": "Não ande por aí sozinho"}'),
    ROW('lt_lava', '{"es": "Lava", "pt-br": "Rio de Lava"}', '{"es": "Ojo que quema", "pt-br": "Lava borbulhando"}'),
    ROW('lt_lavac', '{"es": "Esquina Lava", "pt-br": "Canteiro de Lava"}', '{"es": "Ojo que quema", "pt-br": "Lava borbulhando"}'),
    ROW('lt_patch', '{"es": "Terreno Perdido", "pt-br": "Trilha de Pedras"}', '{"es": "Con pinta de legendario...", "pt-br": "Levando à Tribo Perdida"}'),
    ROW('lt_spider', '{"es": "Tarántula", "pt-br": "Tarântulas"}', '{"es": "¡Su picadura es mortífera!", "pt-br": "A mordida dói. Dói muito."}'),
    ROW('lt_stage1', '{"es": "Terrazón", "pt-br": "Monte de grama"}', '{"es": "¿Te sientes elevado?", "pt-br": "Para ficar um pouco mais alto"}'),
    ROW('lt_stage2', '{"es": "Gran Terrazón", "pt-br": "Monte de grama grande"}', '{"es": "¿Te sientes elevado?", "pt-br": "Que tal um discurso para sua Tribo?"}'),
    ROW('lt_statue', '{"es": "Estatua de Piedra", "pt-br": "Estátua Ancestral"}', '{"es": "¡A que da miedo!", "pt-br": "Arte da Tribo Habbo"}'),
    ROW('lt_stone1', '{"es": "Esquina Jeroglífico", "pt-br": "Canteiro de Pedras"}', '{"es": "¿Pero tú entiendes algo?", "pt-br": "Divide até famílias"}'),
    ROW('lt_stone2', '{"es": "Murete Jeroglífico", "pt-br": "Divisória de Pedra"}', '{"es": "¿Pero tú entiendes algo?", "pt-br": "Divide até famílias"}'),
    ROW('lt_wall', '{"es": "Gran Muro Tribal", "pt-br": "Parede de Pedra"}', '{"es": "¿Podrás escalarlo?", "pt-br": "Duuura!"}'),
    ROW('marquee*1', '{"es": "Toldo Rosa", "pt-br": "Toldo Rosa"}', '{"es": "¡Una puerta y una sombrilla juntas!", "pt-br": "Porta e sombra ao mesmo tempo!"}'),
    ROW('marquee*10', '{"es": "Toldo Blanco", "pt-br": "Toldo Branco"}', '{"es": "¡Una puerta y una sombrilla juntas!", "pt-br": "Porta e sombra ao mesmo tempo!"}'),
    ROW('marquee*2', '{"es": "Toldo Rojo", "pt-br": "Toldo Vermelho"}', '{"es": "¡Una puerta y una sombrilla juntas!", "pt-br": "Porta e sombra ao mesmo tempo!"}'),
    ROW('marquee*3', '{"es": "Toldo Aqua", "pt-br": "Toldo Azul-piscina"}', '{"es": "¡Una puerta y una sombrilla juntas!", "pt-br": "Porta e sombra ao mesmo tempo!"}'),
    ROW('marquee*4', '{"es": "Toldo Amarillo", "pt-br": "Toldo Amarelo"}', '{"es": "¡Una puerta y una sombrilla juntas!", "pt-br": "Porta e sombra ao mesmo tempo!"}'),
    ROW('marquee*5', '{"es": "Toldo Grafito", "pt-br": "Toldo Grafite"}', '{"es": "¡Una puerta y una sombrilla juntas!", "pt-br": "Porta e sombra ao mesmo tempo!"}'),
    ROW('marquee*6', '{"es": "Toldo Azul", "pt-br": "Toldo Azul"}', '{"es": "¡Una puerta y una sombrilla juntas!", "pt-br": "Porta e sombra ao mesmo tempo!"}'),
    ROW('marquee*7', '{"es": "Toldo Púrpura", "pt-br": "Toldo Violeta"}', '{"es": "¡Una puerta y una sombrilla juntas!", "pt-br": "Porta e sombra ao mesmo tempo!"}'),
    ROW('marquee*8', '{"es": "Toldo Ultramarino", "pt-br": "Toldo Azul-marinho"}', '{"es": "¡Una puerta y una sombrilla juntas!", "pt-br": "Porta e sombra ao mesmo tempo!"}'),
    ROW('marquee*9', '{"es": "Toldo Verde", "pt-br": "Toldo Verde"}', '{"es": "¡Una puerta y una sombrilla juntas!", "pt-br": "Porta e sombra ao mesmo tempo!"}'),
    ROW('marsrug', '{"es": "Alfombra de Marte", "pt-br": "Piso de Marte"}', '{"es": "¡Hemos llegado al planeta rojo!", "pt-br": "Aterrisse no Planeta Vermelho"}'),
    ROW('md_can', '{"es": "Lata Bubble Juice", "pt-br": "Pôster Bolhas Verdes"}', '{"es": "Suficiente refresco para toda una tarde", "pt-br": "Sapos e bolhas para seu Quarto"}'),
    ROW('md_limukaappi', '{"es": "Máquina de Habbo Cola", "pt-br": "Habbo Máquina de Refrigerante"}', '{"es": "¡Una refrescante y chispeante bebida pixelada!", "pt-br": "Um refrigerante refrescante e cheio de gás!"}'),
    ROW('md_logo_wall', '{"es": "Logo Bubble Juice", "pt-br": "Bubble Juice"}', '{"es": "Una pared burbujeante", "pt-br": "Verde limão comanda"}'),
    ROW('md_rug', '{"es": "Suelo Bubble Juice", "pt-br": "Tapete Bolhas Verdes"}', '{"es": "Burbujas bajo tus pisadas", "pt-br": "Bolhas sobre seus pés"}'),
    ROW('md_sofa', '{"es": "Mountaindew-Sofa", "pt-br": "MD Sofá"}', '{"es": "Lokoisa sohva kahdelle", "pt-br": "Um excelente reclinador"}'),
    ROW('menorah', '{"es": "Menorah", "pt-br": "Menorá"}', '{"es": "Empieza la fiesta de las luces", "pt-br": "Candelabro de Sete Pontas"}'),
    ROW('merger_chest', '{"es": "Baúl de Viaje", "pt-br": "Baú de Viagem"}', '{"es": "¡El baúl de las sorpresas!", "pt-br": "merger_chest desc"}'),
    ROW('mocchamaster', '{"es": "La Cafetera", "pt-br": "Chocomaster"}', '{"es": "¿Solo o con leche?", "pt-br": "Acorde e sinta o perfume!"}'),
    ROW('nest', '{"es": "Cesta", "pt-br": "Cesta"}', '{"es": "Para dormir", "pt-br": "Já é noite"}'),
    ROW('nest_basket', '{"es": "Cesta Paja y Cojín", "pt-br": "Caixa de Mascote"}', '{"es": "La más cómoda del mercado", "pt-br": "Linda caixa de madeira para seu pet! ;D"}'),
    ROW('nest_dirt', '{"es": "Chiquero", "pt-br": "Cama de lama"}', '{"es": "Aunque no lo creas, al Jabalí le encanta estar aquí", "pt-br": "Galhos e lama, a melhor cama para o seu porquinho."}'),
    ROW('nest_ice', '{"es": "Hogar Helado", "pt-br": "Cama para Urso"}', '{"es": "El rincón ideal para el Oso", "pt-br": "Gelaaaaaaaada"}'),
    ROW('nest_nails', '{"es": "Cesta Faquir", "pt-br": "Cama de Pregos"}', '{"es": "Para Mascotas de piel dura...", "pt-br": "Para mascotes radicais, camas radicais!"}'),
    ROW('nest_nest', '{"es": "Cesta Paja y Piedras", "pt-br": "Cesto de ninho"}', '{"es": "Hogar, dulce hogar", "pt-br": "Aconchego e proteção no lugar certo."}'),
    ROW('nest_plow_blu', '{"es": "Cojín Mascota Azul", "pt-br": "Almofada Azul"}', '{"es": "Comodidad para tu Mascota", "pt-br": "Para dar uma aliviada"}'),
    ROW('nest_plow_bro', '{"es": "Cojín Mascota Marrón", "pt-br": "Cama para Mascote"}', '{"es": "Comodidad para tu Mascota", "pt-br": "Fofiiiinha!"}'),
    ROW('nest_plow_red', '{"es": "Cojín Mascota Rojo", "pt-br": "Almofada Vermelha"}', '{"es": "Comodidad para tu Mascota", "pt-br": "Mascotes merecem descanso"}'),
    ROW('nest_plow_reg', '{"es": "Cojín Mascota Blanco", "pt-br": "Cama para Mascote"}', '{"es": "Comodidad para tu Mascota", "pt-br": "Fofiiiinha!"}'),
    ROW('nest_plow_skl', '{"es": "Cojín Mascota Calavera", "pt-br": "Almofada Caveira"}', '{"es": "Descanso infernal", "pt-br": "Moderna, descolada e bonita. Seu Mascote vai se sentir o máximo!"}'),
    ROW('nest_snug_bla', '{"es": "Cesta Mascota Negra", "pt-br": "Cama Preta"}', '{"es": "¡Un hogar para tu Mascota!", "pt-br": "Para seu mascote dormir confortavelmente."}'),
    ROW('nest_snug_blu', '{"es": "Cesta Mascota Azul", "pt-br": "Cama para Mascote"}', '{"es": "¡Un hogar para tu Mascota!", "pt-br": "Fofiiiinha!"}'),
    ROW('nest_snug_grn', '{"es": "Cesta Mascota Verde", "pt-br": "Assento Verde"}', '{"es": "¡Un hogar para tu Mascota!", "pt-br": "Para seu Mascote descansar"}'),
    ROW('nest_snug_prp', '{"es": "Cesta Mascota Púrpura", "pt-br": "Cama Roxa"}', '{"es": "¡Un hogar para tu Mascota!", "pt-br": "Para seu mascote dormir confortavelmente."}'),
    ROW('nest_snug_red', '{"es": "Cesta Mascota Roja", "pt-br": "Cama para Mascote"}', '{"es": "¡Un hogar para tu Mascota!", "pt-br": "Fofiiiinha!"}'),
    ROW('nest_snug_yel', '{"es": "Cesta Mascota Amarilla", "pt-br": "Assento Laranja"}', '{"es": "¡Un hogar para tu Mascota!", "pt-br": "Mascotes precisam descansar"}'),
    ROW('netari_carpet', '{"es": "netari_carpet", "pt-br": "netari_carpet"}', '{"es": "Netari branded skull description", "pt-br": "Netari branded skull description"}'),
    ROW('netari_poster', '{"es": "netari_poster", "pt-br": "netari_poster"}', '{"es": "Netari promotion text here", "pt-br": "Netari promotion text here"}'),
    ROW('noob_chair*1', '{"es": "Habbo Silla", "pt-br": "Cadeira Novato"}', '{"es": "Ligera, práctica y amarilla", "pt-br": "Leve, prática e amarela."}'),
    ROW('noob_chair*2', '{"es": "Habbo Silla", "pt-br": "Cadeira Novato"}', '{"es": "Ligera, práctica y con oscuras rayas azules", "pt-br": "Leve, prática, com listras azul-escuras."}'),
    ROW('noob_chair*3', '{"es": "Habbo Silla", "pt-br": "Cadeira Novato"}', '{"es": "Ligera, práctica y con rayas rojas", "pt-br": "Leve, prática, com listras vermelhas."}'),
    ROW('noob_chair*4', '{"es": "Habbo Silla", "pt-br": "Cadeira Novato"}', '{"es": "Ligera, práctica y con rayas azules", "pt-br": "Leve, prática, com listras azul-claras."}'),
    ROW('noob_chair*5', '{"es": "Habbo silla", "pt-br": "Cadeira Novato"}', '{"es": "Ligera, práctica y con rayas rosas", "pt-br": "Leve, prática e cor-de-rosa!"}'),
    ROW('noob_chair*6', '{"es": "Habbo Silla", "pt-br": "Cadeira Novato"}', '{"es": "Ligera, práctica y amarilla", "pt-br": "Leve, prática e amarela."}'),
    ROW('noob_chair_tradeable*1', '{"es": "Habbo Silla", "pt-br": "Cadeira Novato"}', '{"es": "Ligera y práctica", "pt-br": "Agora negociável"}'),
    ROW('noob_chair_tradeable*2', '{"es": "Habbo Silla", "pt-br": "Cadeira Novato"}', '{"es": "Ligera y práctica", "pt-br": "Agora negociável"}'),
    ROW('noob_chair_tradeable*3', '{"es": "Habbo Silla", "pt-br": "Cadeira Novato"}', '{"es": "Ligera y práctica", "pt-br": "Agora negociável"}'),
    ROW('noob_chair_tradeable*4', '{"es": "Habbo Silla", "pt-br": "Cadeira Novato"}', '{"es": "Ligera y práctica", "pt-br": "Agora negociável"}'),
    ROW('noob_chair_tradeable*5', '{"es": "Habbo Silla", "pt-br": "Cadeira Novato"}', '{"es": "Ligera y práctica", "pt-br": "Agora negociável"}'),
    ROW('noob_chair_tradeable*6', '{"es": "Habbo Silla", "pt-br": "Cadeira Novato"}', '{"es": "Set del principiante", "pt-br": "Agora negociável"}'),
    ROW('noob_lamp*1', '{"es": "Habbo Lámpara", "pt-br": "Luminária Novato"}', '{"es": "Una luz donde quiera que estés", "pt-br": "Tenha luz onde quiser (amarela)"}'),
    ROW('noob_lamp*2', '{"es": "Habbo Lámpara", "pt-br": "Luminária Novato"}', '{"es": "Una luz donde quiera que estés", "pt-br": "Tenha luz onde quiser (azul-escura)"}'),
    ROW('noob_lamp*3', '{"es": "Habbo Lámpara", "pt-br": "Luminária Novato"}', '{"es": "Una luz donde quiera que estés", "pt-br": "Tenha luz onde quiser (beringela)"}'),
    ROW('noob_lamp*4', '{"es": "Habbo Lámpara", "pt-br": "Luminária Novato"}', '{"es": "Una luz donde quiera que estés", "pt-br": "Tenha luz onde quiser (azul-clara)"}'),
    ROW('noob_lamp*5', '{"es": "Habbo Lámpara", "pt-br": "Luminária Novato"}', '{"es": "Una luz donde quiera que estés", "pt-br": "Tenha luz onde quiser (cor-de-rosa)"}'),
    ROW('noob_lamp*6', '{"es": "Habbo Lámpara", "pt-br": "Luminária Novato"}', '{"es": "Una luz donde quiera que estés", "pt-br": "Tenha luz onde quiser (amarelo-canário)"}'),
    ROW('noob_lamp_tradeable*1', '{"es": "Habbo Lámpara", "pt-br": "Luminária Novato"}', '{"es": "Una luz donde quiera que estés", "pt-br": "Agora negociável"}'),
    ROW('noob_lamp_tradeable*2', '{"es": "Habbo Lámpara", "pt-br": "Luminária Novato"}', '{"es": "Una luz donde quiera que estés", "pt-br": "Agora negociável"}'),
    ROW('noob_lamp_tradeable*3', '{"es": "Habbo Lámpara", "pt-br": "Luminária Novato"}', '{"es": "Una luz donde quiera que estés", "pt-br": "Agora negociável"}'),
    ROW('noob_lamp_tradeable*4', '{"es": "Habbo Lámpara", "pt-br": "Luminária Novato"}', '{"es": "Una luz donde quiera que estés", "pt-br": "Agora negociável"}'),
    ROW('noob_lamp_tradeable*5', '{"es": "Habbo Lámpara", "pt-br": "Luminária Novato"}', '{"es": "Una luz donde quiera que estés", "pt-br": "Agora negociável"}'),
    ROW('noob_lamp_tradeable*6', '{"es": "Habbo Lámpara", "pt-br": "Luminária Novato"}', '{"es": "Una luz donde quiera que estés", "pt-br": "Agora negociável"}'),
    ROW('noob_plant', '{"es": "Bambú", "pt-br": "Planta Novato"}', '{"es": "¡Para alegrar tu nueva Sala!", "pt-br": "A primeira plantinha a gente nunca esquece."}'),
    ROW('noob_rug*1', '{"es": "Habbo Alfombra", "pt-br": "Tapete Novato"}', '{"es": "Bonita y sencilla alfombra de bordes naranjas", "pt-br": "Belo tapete com bordas laranja"}'),
    ROW('noob_rug*2', '{"es": "Habbo Alfombra", "pt-br": "Tapete Novato"}', '{"es": "Bonita y sencilla alfombra de bordes azules", "pt-br": "Belo tapete com bordas azuis"}'),
    ROW('noob_rug*3', '{"es": "Habbo Alfombra", "pt-br": "Tapete Novato"}', '{"es": "Bonita y sencilla alfombra de bordes berenjenas", "pt-br": "Belo tapete com bordas beringela"}'),
    ROW('noob_rug*4', '{"es": "Habbo Alfombra", "pt-br": "Tapete Novato"}', '{"es": "Bonita y sencilla alfombra de bordes celestes", "pt-br": "Belo tapete com bordas azul-claro"}'),
    ROW('noob_rug*5', '{"es": "Habbo Alfombra", "pt-br": "Tapete Novato"}', '{"es": "Bonita y sencilla alfombra de bordes rosas", "pt-br": "Belo tapete com bordas cor-de-rosa"}'),
    ROW('noob_rug*6', '{"es": "Habbo Alfombra", "pt-br": "Tapete Novato"}', '{"es": "Bonita y sencilla alfombra de bordes verdes", "pt-br": "Belo tapete com bordas verdes"}'),
    ROW('noob_rug_tradeable*1', '{"es": "Habbo Alfombra", "pt-br": "Tapete Novato"}', '{"es": "Bonita y sencilla", "pt-br": "Agora negociável"}'),
    ROW('noob_rug_tradeable*2', '{"es": "Habbo Alfombra", "pt-br": "Tapete Novato"}', '{"es": "Bonita y sencilla", "pt-br": "Agora negociável"}'),
    ROW('noob_rug_tradeable*3', '{"es": "Habbo Alfombra", "pt-br": "Tapete Novato"}', '{"es": "Bonita y sencilla", "pt-br": "Agora negociável"}'),
    ROW('noob_rug_tradeable*4', '{"es": "Habbo Alfombra", "pt-br": "Tapete Novato"}', '{"es": "Bonita y sencilla", "pt-br": "Agora negociável"}'),
    ROW('noob_rug_tradeable*5', '{"es": "Habbo Alfombra", "pt-br": "Tapete Novato"}', '{"es": "Bonita y sencilla", "pt-br": "Agora negociável"}'),
    ROW('noob_rug_tradeable*6', '{"es": "Habbo Alfombra", "pt-br": "Tapete Novato"}', '{"es": "Bonita y sencilla", "pt-br": "Agora negociável"}'),
    ROW('noob_stool*1', '{"es": "Habbo Taburete", "pt-br": "Banqueta Novato"}', '{"es": "Se guarda en cualquier parte", "pt-br": "Suporto grandes pesos (laranja)"}'),
    ROW('noob_stool*2', '{"es": "Habbo Taburete", "pt-br": "Banqueta Novato"}', '{"es": "Se guarda en cualquier parte", "pt-br": "Suporto grandes pesos (azul-escuro)"}'),
    ROW('noob_stool*3', '{"es": "Habbo Taburete", "pt-br": "Banqueta Novato"}', '{"es": "Se guarda en cualquier parte", "pt-br": "Suporto grandes pesos (beringela)"}'),
    ROW('noob_stool*4', '{"es": "Habbo Taburete", "pt-br": "Banqueta Novato"}', '{"es": "Se guarda en cualquier parte", "pt-br": "Suporto grandes pesos (azul claro)"}'),
    ROW('noob_stool*5', '{"es": "Habbo Taburete", "pt-br": "Banqueta Novato"}', '{"es": "Se guarda en cualquier parte", "pt-br": "Suporto grandes pesos (rosa)"}'),
    ROW('noob_stool*6', '{"es": "Habbo Taburete", "pt-br": "Banqueta Novato"}', '{"es": "Se guarda en cualquier parte", "pt-br": "Suporto grandes pesos (verde)"}'),
    ROW('noob_table*1', '{"es": "Habbo Mesa", "pt-br": "Mesa Novato"}', '{"es": "Ligera, práctica y naranja", "pt-br": "Leve, prática e laranja"}'),
    ROW('noob_table*2', '{"es": "Habbo Mesa", "pt-br": "Mesa Novato"}', '{"es": "Ligera, práctica y azul", "pt-br": "Leve, prática e azul-escuro"}'),
    ROW('noob_table*3', '{"es": "Habbo Mesa", "pt-br": "Mesa Novato"}', '{"es": "Ligera, práctica y berenjena", "pt-br": "Leve, prática e beringela"}'),
    ROW('noob_table*4', '{"es": "Habbo Mesa", "pt-br": "Mesa Novato"}', '{"es": "Ligera, práctica y celeste", "pt-br": "Leve, prática e azul-claro"}'),
    ROW('noob_table*5', '{"es": "Habbo Mesa", "pt-br": "Mesa Novato"}', '{"es": "Ligera, práctica y rosa", "pt-br": "Leve, prática e amarela"}'),
    ROW('noob_table*6', '{"es": "Habbo Mesa", "pt-br": "Mesa Novato"}', '{"es": "Ligera, práctica y verde", "pt-br": "Leve, prática e verde"}'),
    ROW('noob_window_double', '{"es": "Ventana", "pt-br": "Janela"}', '{"es": "Ventana", "pt-br": "Janela"}'),
    ROW('noticeboard', '{"es": "Panel de Notas", "pt-br": "Quadro de Recados"}', '{"es": "¿Algo que anunciar?", "pt-br": "Anota direito pra não esquecer"}'),
    ROW('nouvelle_trax', '{"es": "nouvelle_trax", "pt-br": "Coletânea Trax"}', '{"pt-br": "Deixe-se levar pela música"}'),
    ROW('one_way_door*1', '{"es": "Puerta Transpixelar Azul", "pt-br": "Portão Sentido Único"}', '{"es": "¡No hay vuelta atrás!", "pt-br": "Siga em frente"}'),
    ROW('one_way_door*2', '{"es": "Puerta Transpixelar Negra", "pt-br": "Porta Preta Sentido Único"}', '{"es": "¡No hay vuelta atrás!", "pt-br": "Siga por aqui e seja feliz"}'),
    ROW('one_way_door*3', '{"es": "Puerta Transpixelar Blanca", "pt-br": "Porta Branca Sentido Único"}', '{"es": "¡No hay vuelta atrás!", "pt-br": "Siga em frente"}'),
    ROW('one_way_door*4', '{"es": "Puerta Transpixelar Beige", "pt-br": "Portão Sentido Único"}', '{"es": "¡No hay vuelta atrás!", "pt-br": "Siga em frente"}'),
    ROW('one_way_door*5', '{"es": "Puerta Transpixelar Rosa", "pt-br": "Portão Sentido Único"}', '{"es": "Puerta de seguridad", "pt-br": "Siga sempre em frente"}'),
    ROW('one_way_door*6', '{"es": "Puerta Transpixelar Ultramarina", "pt-br": "Portão Sentido Único"}', '{"es": "¡No hay vuelta atrás!", "pt-br": "Siga sempre em frente"}'),
    ROW('one_way_door*7', '{"es": "Puerta Transpixelar Verde", "pt-br": "Portão Sentido Único"}', '{"es": "¡Pasa a otra dimensión!", "pt-br": "Siga em frente"}'),
    ROW('one_way_door*8', '{"es": "Puerta Transpixelar Amarilla", "pt-br": "Porta Amarela Sentido Único"}', '{"es": "¡En busca de ''El Dorado''!", "pt-br": "Siga por aqui e seja feliz"}'),
    ROW('one_way_door*9', '{"es": "Puerta Transpixelar Roja", "pt-br": "Porta Vermelha Sentido Único"}', '{"es": "¡No hay vuelta atrás!", "pt-br": "Vá por ali"}'),
    ROW('org_chairblk', '{"es": "Silla Organo Gris", "pt-br": "Cadeira Organo Preta"}', '{"es": "Toma asiento", "pt-br": "Sente-se e fique à vontade"}'),
    ROW('org_chairpnk', '{"es": "Silla Organo Rosa", "pt-br": "Cadeira Organo Rosa"}', '{"es": "Toma asiento", "pt-br": "Sente-se e fique à vontade"}'),
    ROW('org_chrblk', '{"es": "Armario Organo Negro", "pt-br": "Poltrona Organo Preta"}', '{"es": "Un diseño revolucionario"}'),
    ROW('org_chrpnk', '{"es": "Armario Organo Rosa", "pt-br": "Poltrona Organo Rosa"}', '{"es": "Un diseño revolucionario"}'),
    ROW('org_lampblk', '{"es": "Lámpara Organo Negra", "pt-br": "Luminária Organo Preta"}', '{"es": "Un toque de color para tus salas", "pt-br": "Um toque de cor ao seu quarto"}'),
    ROW('org_lamppnk', '{"es": "Lámpara  Rosa", "pt-br": "Luminária Organo pink"}', '{"es": "Un toque de color para tus salas", "pt-br": "Um toque de cor aos seus quartos"}'),
    ROW('org_table', '{"es": "Mesa de Café Organo Rosa", "pt-br": "Mesa Organo Rosa"}', '{"es": "Listo para ser utilizado", "pt-br": "Já pode usar"}'),
    ROW('org_tblblk', '{"es": "Mesita Multiforme Negra", "pt-br": "Mesa Organo preta"}', '{"es": "¿Listo para cenar?", "pt-br": "Preparados para o jantar?"}'),
    ROW('org_tblpnk', '{"es": "Mesita Multiforme Rosa", "pt-br": "Mesa Organo pink"}', '{"es": "¿Listo para cenar?", "pt-br": "Preparados para o jantar?"}'),
    ROW('party_ball', '{"es": "Bola Discotequera", "pt-br": "Globo de Espelhos"}', '{"es": "¡Dale brillo a tu Club!", "pt-br": "E tudo foi girando, girando..."}'),
    ROW('party_barcorn', '{"es": "Esquina Neón", "pt-br": "Canto de Balcão Neon"}', '{"es": "¡Para los mejores clubs!", "pt-br": "Só não vale ficar no canto da balada"}'),
    ROW('party_bardesk', '{"es": "Barra Neón", "pt-br": "Balcão Neon"}', '{"es": "¡Para los mejores clubs!", "pt-br": "Chega mais, faz um pedido"}'),
    ROW('party_beamer', '{"es": "Rayos Neón", "pt-br": "Projetor Neon"}', '{"es": "¡Menuda fiesta!", "pt-br": "Para danças agitadas"}'),
    ROW('party_block', '{"es": "Mini-escenario", "pt-br": "Bloco Pequeno de Piso de Festa"}', '{"es": "¡Da la nota!", "pt-br": "Dá pra deslizar"}'),
    ROW('party_block2', '{"es": "Escenario", "pt-br": "Bloco Grande de Piso de Festa"}', '{"es": "¡Da la nota!", "pt-br": "Não vá escorregar"}'),
    ROW('party_chair', '{"es": "Taburete Neón", "pt-br": "Cadeira Neon"}', '{"es": "Un toque chic para tu Club", "pt-br": "Para recarregar as energias"}'),
    ROW('party_discol', '{"es": "Foco triple", "pt-br": "Canhão de Luzes"}', '{"es": "Para los mejores conciertos", "pt-br": "Psicodelia pura"}'),
    ROW('party_djtable', '{"es": "Mesa de mezclas", "pt-br": "Picape de DJ"}', '{"es": "¡Para tus grandes temazos!", "pt-br": "Scratch e remix"}'),
    ROW('party_floor', '{"es": "Pista de Baile", "pt-br": "Piso de Balada"}', '{"es": "¡Necesaria en todo Club que se precie!", "pt-br": "Quero no meu Quarto!"}'),
    ROW('party_lantern', '{"es": "Farol de fiesta", "pt-br": "Lanterna de Festa"}', '{"es": "¡No te quedes a oscuras!", "pt-br": "A luz que te guia na noite"}'),
    ROW('party_led', '{"es": "Luces Neón", "pt-br": "Luzes na parede"}', '{"es": "¡Animan cualquier fiesta!", "pt-br": "Indispensável"}'),
    ROW('party_lights', '{"es": "Luces de Cotillón", "pt-br": "Luzes Festivas"}', '{"es": "La luz brillante atrae a l@s Habbos nocturn@s", "pt-br": "Iluminação especial para eventos especiais"}'),
    ROW('party_mic', '{"es": "Micrófono", "pt-br": "Microfone"}', '{"es": "¡Para oirte mejorrrr!", "pt-br": "Essa platéia maravilhosa..."}'),
    ROW('party_neon1', '{"es": "Flechas", "pt-br": "Seta Neon Esquerda"}', '{"es": "¿La salida? Quizá a la izquierda", "pt-br": "Seguindo a pista"}'),
    ROW('party_neon2', '{"es": "Flechas", "pt-br": "Seta Neon Direita"}', '{"es": "¿La salida? Quizá hacia la derecha...", "pt-br": "Pode confiar na seta"}')) AS tr (classname, names, descs)
  ON f.`classname` = tr.classname
SET f.`localized_name` = CAST(tr.names AS JSON), f.`localized_desc` = CAST(tr.descs AS JSON);
UPDATE `definition_furniture` f JOIN (VALUES
    ROW('party_neon3', '{"es": "Flamenco Neón", "pt-br": "Luz de flamingo"}', '{"es": "¡Para Clubes selectos!", "pt-br": "Elegância na parede"}'),
    ROW('party_neon4', '{"es": "Calavera Neón", "pt-br": "Caveira Neon"}', '{"es": "¡Para Clubs que pone los pelos de punta!", "pt-br": "Esqueleto colorido"}'),
    ROW('party_neon5', '{"es": "Corazón Neón", "pt-br": "Coração na Parede"}', '{"es": "¿Va de romántico tu Club?", "pt-br": "Uma festa romântica"}'),
    ROW('party_ravel', '{"es": "Láser Neón", "pt-br": "Laser Neon"}', '{"es": "¡Menuda fiesta!", "pt-br": "Uma luz do bem"}'),
    ROW('party_seat', '{"es": "Silla Neón", "pt-br": "Cadeirinha Neon"}', '{"es": "¡Bien cómodo para ver el espectáculo!", "pt-br": "Sentiremos sua falta na pista"}'),
    ROW('party_shelf', '{"es": "Expositor de bebidas", "pt-br": "Bar Shelf"}', '{"es": "¡Para tus mejores cockteles!", "pt-br": "Cadê os livros que estavam aqui?"}'),
    ROW('party_table', '{"es": "Mesita Neón", "pt-br": "Mesa Neon"}', '{"es": "Un toque chic para tu Club", "pt-br": "Cabe tudo o que precisa"}'),
    ROW('party_tray', '{"es": "Bandeja Neón", "pt-br": "Prateleira Neon"}', '{"es": "¡Para preparar los mejores cockteles sin alcohol!", "pt-br": "Tenha várias"}'),
    ROW('party_tube_bubble', '{"es": "Lámpara burbujeante", "pt-br": "Tubo de Bolhas"}', '{"es": "¡No podrás hacer otra cosa si la miras!", "pt-br": "Eu podia ficar olhando pra sempre"}'),
    ROW('party_tube_lava', '{"es": "Lámpara de Lava", "pt-br": "Tubo de Lava"}', '{"es": "¡Un toque chic!", "pt-br": "Está esquentando ou sou apenas eu?"}'),
    ROW('party_wc_boy', '{"es": "WC", "pt-br": "Banheiro dos Meninos"}', '{"es": "¡Adelante caballeros!", "pt-br": "WC = Wanderley Cardoso"}'),
    ROW('party_wc_girl', '{"es": "WC", "pt-br": "Banheiro das Meninas"}', '{"es": "¡Adelante señoras!", "pt-br": "Muito respeito aqui"}'),
    ROW('penguin_ballet', '{"es": "Pingüino Bailarina", "pt-br": "Pingüina Bailarina"}', '{"es": "¿Quieres ser mi pareja?", "pt-br": "Pulinho pra cá e pra lá"}'),
    ROW('penguin_basic', '{"es": "Pingüino", "pt-br": "Pingüim Básico"}', '{"es": "¿Dónde está el pescado?", "pt-br": "Original de fábrica"}'),
    ROW('penguin_boxer', '{"es": "Pingüino Boxeador", "pt-br": "Pingüim Boxeador"}', '{"es": "Observa mi juego de piernas", "pt-br": "Vai encarar?"}'),
    ROW('penguin_bunny', '{"es": "Pingüino Conejo", "pt-br": "Pingüim Coelho"}', '{"es": "Me encanta la zanahoria", "pt-br": "Em que mês estamos?"}'),
    ROW('penguin_clown', '{"es": "Pingüino Payaso", "pt-br": "Pingüim Palhaço"}', '{"es": "¿Cómo están ustedessss?", "pt-br": "Agora conta a do papagaio..."}'),
    ROW('penguin_cowboy', '{"es": "Pingüino Cowboy", "pt-br": "Pingüim Cáuboi"}', '{"es": "¡El más duro del Ártico!", "pt-br": "Rodeio de leão-marinho"}'),
    ROW('penguin_elf', '{"es": "Pingüino Santa", "pt-br": "Pingüim Noel"}', '{"es": "¿Los regalos? ¿Qué regalos?", "pt-br": "O mundo mágico é aqui"}'),
    ROW('penguin_glow', '{"es": "Pingüino Radioactivo", "pt-br": "Pingüim Fluorescente"}', '{"es": "El pescado estaba contaminado :(", "pt-br": "Brilha no escuro!"}'),
    ROW('penguin_hunchback', '{"es": "Pingüino Jorobado", "pt-br": "Pingüim Corcunda"}', '{"es": "¡Tolon, tolón! ¡Tolón, tolón!", "pt-br": "Ah, Esmeralda..."}'),
    ROW('penguin_icehockey', '{"es": "Pingüino Hockey", "pt-br": "Pingüim de Hóquei"}', '{"es": "¡Voy a meter un golazo!", "pt-br": "Opa, deslizando..."}'),
    ROW('penguin_infected', '{"es": "Pingüino Griposo", "pt-br": "Pingüim Infectado"}', '{"es": "A mis brazos, Habbo", "pt-br": "Não chegue muito perto"}'),
    ROW('penguin_magician', '{"es": "Pingüino Abracadabra", "pt-br": "Pingüim Mágico"}', '{"es": "Esto es sólo tu imaginación", "pt-br": "Cadê o coelho que estava aqui?"}'),
    ROW('penguin_musketeer', '{"es": "Pingüino Mosquetero", "pt-br": "Pinqüim Mosqueteiro"}', '{"es": "¡Todos para uno y luego ya veremos!", "pt-br": "Um por todos e... esqueci o texto"}'),
    ROW('penguin_ninja', '{"es": "Pingüino Ninja", "pt-br": "Pinqüim Ninja"}', '{"es": "¡Shinobi! ¡Noba! ¡Que cada día te quiero más!", "pt-br": "Fique frio, cara"}'),
    ROW('penguin_pilot', '{"es": "Pingüino Piloto", "pt-br": "Pingüim Piloto"}', '{"es": "¡Volando voy! ¡Volando vengo!", "pt-br": "Afinal, é uma ave, né?"}'),
    ROW('penguin_pirate', '{"es": "Pingüino Pirata", "pt-br": "Pingüim Pirata"}', '{"es": "Navegaré los siete mares por ti, pirata", "pt-br": "Olho de vidro, bico de madeira"}'),
    ROW('penguin_punk', '{"es": "Pingüino Punk", "pt-br": "Pingüim Punk"}', '{"es": "El Punk no ha muerto, estaba de parranda", "pt-br": "Um ícone da moda e da música"}'),
    ROW('penguin_robot', '{"es": "Pingüino Robot", "pt-br": "Pingüim Robô"}', '{"es": "Soy primo de C3PO", "pt-br": "Come sardinha ainda na lata"}'),
    ROW('penguin_rock', '{"es": "Pingüino Rock", "pt-br": "Pingüim de Balada"}', '{"es": "It''s only Rock''n''Roll y soy el number one!", "pt-br": "Estilo é o que não falta"}'),
    ROW('penguin_skater', '{"es": "Pingüino Skater", "pt-br": "Pingüim Skatista"}', '{"es": "¡Atención a mi triplehalfpipe360! ¡Sin dientes!", "pt-br": "Equilíbrio é fundamental"}'),
    ROW('penguin_ski', '{"es": "Pingüino Esquiador", "pt-br": "Pingüinha de Esqui"}', '{"es": "El auténtico monstruo de las nieves", "pt-br": "Urru, sem as mãos!"}'),
    ROW('penguin_suit', '{"es": "Pingüino Pingüino", "pt-br": "Pingüim Executivo"}', '{"es": "Los viernes visto de ''sport''", "pt-br": "Fazemos qualquer negócio!"}'),
    ROW('penguin_sumo', '{"es": "Pingüino Sumo", "pt-br": "Pingüim Sumô"}', '{"es": "Estoy que me salgo por todos los lados", "pt-br": "Truques milenares"}'),
    ROW('penguin_super', '{"es": "Superpingüino", "pt-br": "Super Pingüim"}', '{"es": "¿Soy un pájaro? ¿Soy un avión? ¡Estoy confundido!", "pt-br": "É um pássaro? É..."}'),
    ROW('penguin_swim', '{"es": "Pingüino Bañista", "pt-br": "Pingüim Nadador"}', '{"es": "Yo también tengo 8 oros olímpicos", "pt-br": "Mergulhar é o forte dele"}'),
    ROW('penguin_wrestler', '{"es": "Pingüino Lucha Libre", "pt-br": "Pingüim Lutador"}', '{"es": "¡No pruebes esto en casa!", "pt-br": "É só auto-defesa, juro"}'),
    ROW('petfood1', '{"es": "Hueso botánico", "pt-br": "Ossos"}', '{"es": "Energía natural para los aulladores (Energía Media)", "pt-br": "Nutrição natural para o latido animal (550 nutrientes)"}'),
    ROW('petfood10', '{"es": "Tarta Navideña", "pt-br": "Bolo de Natal"}', '{"es": "Para momentos especiales (Energía Alta)", "pt-br": "Para momentos especiais (825 nutrientes)"}'),
    ROW('petfood11', '{"es": "Cartas", "pt-br": "Correspondências"}', '{"es": "Probablemente, una pila de billetes (Energía Baja)", "pt-br": "Uma pilha de contas, provavelmente (130 nutrientes)"}'),
    ROW('petfood12', '{"es": "Libros", "pt-br": "Livros"}', '{"es": "La cultura del buen comer (Energía Baja)", "pt-br": "Cultura alimenta (100 nutrientes)"}'),
    ROW('petfood13', '{"es": "Mando TV", "pt-br": "Controle remoto"}', '{"es": "¡Uff! ¡Ahora no podré cambiar de canal! (Energía Baja)", "pt-br": "Ai! Agora não posso mudar o canal! (120 nutrientes)"}'),
    ROW('petfood2', '{"es": "Sardinas", "pt-br": "Sardinhas"}', '{"es": "Caza fresca de ayer (Energía Media)", "pt-br": "Caça fresquinha de ontem (400 nutrientes)"}'),
    ROW('petfood3', '{"es": "Col", "pt-br": "Repolho"}', '{"es": "Comida saludable para tus mascotas (Energí Baja)", "pt-br": "Comida saudável para mascotes (325 nutrientes)"}'),
    ROW('petfood4', '{"es": "Huesitos", "pt-br": "Bistecas"}', '{"es": "¡Carne y hueso! (Energía Media)", "pt-br": "Carne e osso! (550 nutrientes)"}'),
    ROW('petfood5', '{"es": "Manzanas Verdes", "pt-br": "Maçãs Verdes"}', '{"es": "Repleto de vitamina G (Energía Baja)", "pt-br": "Cheio de Vitamina G (350 nutrientes)"}'),
    ROW('petfood6', '{"es": "Manzanas Rojas", "pt-br": "Maçãs Vermelhas"}', '{"es": "¡Repleto de vitamina R! (Energía Baja)", "pt-br": "Cheio de Vitamina R (350 nutrientes)"}'),
    ROW('petfood7', '{"es": "Fruta Congelada", "pt-br": "Maçãs Congeladas"}', '{"es": "Repleto de vitaminas I (Energía Media)", "pt-br": "Cheio de Vitamina I (750 nutrientes)"}'),
    ROW('petfood8', '{"es": "Pollo", "pt-br": "Frango"}', '{"es": "Todo sabe a Pollo (Energía Alta)", "pt-br": "Tudo tem gosto de frango assado (825 nutrientes)"}'),
    ROW('petfood9', '{"es": "Salmón", "pt-br": "Salmão"}', '{"es": "Salmón fresco de las montañas (Energía Súper)", "pt-br": "Salmão fresco das montanhas (1375 nutrientes)"}'),
    ROW('photo', '{"es": "Foto", "pt-br": "Foto"}', '{"es": "Foto de Habbo", "pt-br": "Foto de um Habbo"}'),
    ROW('pillar*0', '{"es": "Pilar Clásico Dórico", "pt-br": "Pilar Clássico"}', '{"es": "Antiguo y majestuoso", "pt-br": "Antiguidade e pompa"}'),
    ROW('pillar*1', '{"es": "Pilar Dórico de Mármol", "pt-br": "Pilar de São Valentim"}', '{"es": "Antiguo y majestuoso", "pt-br": "Uma chinfra para seu Quarto. Use-o também para empilhar Mobis."}'),
    ROW('pillar*2', '{"es": "Pilar Dórico de Madera", "pt-br": "Novo Pilar de Madeira"}', '{"es": "Antiguo y majestuoso", "pt-br": "Clássico reciclado"}'),
    ROW('pillar*3', '{"es": "Pilar Dórico Azul", "pt-br": "Pilar Azul"}', '{"es": "Antiguo y majestuoso", "pt-br": "Antiguidade e pompa"}'),
    ROW('pillar*4', '{"es": "Pilar Dórico de Grafito", "pt-br": "Pilar Grafite"}', '{"es": "Antiguo y majestuoso", "pt-br": "Antigüidade e pompa"}'),
    ROW('pillar*5', '{"es": "Pilar Dórico Verde", "pt-br": "Pilar Verde"}', '{"es": "Antiguo y majestuoso", "pt-br": "Antiguidade e pompa"}'),
    ROW('pillar*6', '{"es": "Pilar Dórico de Barro", "pt-br": "Pilar Terracota"}', '{"es": "Antiguo y majestuoso", "pt-br": "Antigüidade e pompa"}'),
    ROW('pillar*7', '{"es": "Pilar Dórico de Oro", "pt-br": "Pilar Dourado"}', '{"es": "Antiguo y majestuoso", "pt-br": "Antiguidade e pompa"}'),
    ROW('pillar*8', '{"es": "Pilar Dórico Verde Oliva", "pt-br": "Pilar Verde-oliva"}', '{"es": "Antiguo y majestuoso", "pt-br": "Antiguidade e pompa"}'),
    ROW('pillar*9', '{"es": "Roca Dórica", "pt-br": "Pilar de Pedra"}', '{"es": "Antiguo y majestuoso", "pt-br": "Antiguidade e pompa"}'),
    ROW('pillow*0', '{"es": "Almohadón Blanco", "pt-br": "Almofada com Laço Branco"}', '{"es": "Enorme, suave y acolchado", "pt-br": "Conforto para quem se contenta com pouco!"}'),
    ROW('pillow*1', '{"es": "Almohadón Rosa", "pt-br": "Almofada Macia Rosa"}', '{"es": "Enorme, suave y acolchado", "pt-br": "Grande, leve e muito macia"}'),
    ROW('pillow*2', '{"es": "Almohadón Rojo", "pt-br": "Almofada de Seda Vermelha"}', '{"es": "Enorme, suave y acolchado", "pt-br": "Grande, leve e muito macia"}'),
    ROW('pillow*3', '{"es": "Almohadón Turquesa", "pt-br": "Almofada de Cetim Turquesa"}', '{"es": "Enorme, suave y acolchado", "pt-br": "Grande, leve e muito macia"}'),
    ROW('pillow*4', '{"es": "Almohadón Dorado", "pt-br": "Almofada de Penas Dourada"}', '{"es": "Enorme, suave y acolchado", "pt-br": "Grande, leve e muito macia"}'),
    ROW('pillow*5', '{"es": "Almohadón Verde Oscuro", "pt-br": "Almofada de Couro Preto"}', '{"es": "Enorme, suave y acolchado", "pt-br": "Grande, leve e muito macia"}'),
    ROW('pillow*6', '{"es": "Almohadón Azul", "pt-br": "Almofada de Algodão Azul"}', '{"es": "Enorme, suave y acolchado", "pt-br": "Grande, leve e muito macia"}'),
    ROW('pillow*7', '{"es": "Almohadón Púrpura", "pt-br": "Almofada de Seda da Bonnie"}', '{"es": "Enorme, suave y acolchado", "pt-br": "Somente para Habbo Astros"}'),
    ROW('pillow*8', '{"es": "Almohadón Marino", "pt-br": "Almofada de Fios Azul-marinho"}', '{"es": "Enorme, suave y acolchado", "pt-br": "Grande, leve e muito macia"}'),
    ROW('pillow*9', '{"es": "Almohadón Verde", "pt-br": "Almofada de Lã Verde"}', '{"es": "Enorme, suave y acolchado", "pt-br": "Grande, leve e muito macia"}'),
    ROW('pix_asteroid', '{"es": "Asteroide", "pt-br": "Asteroide"}', '{"es": "¡No choques con él!", "pt-br": "Rota errante"}'),
    ROW('pizza', '{"es": "Pizza", "pt-br": "Caixa de Pizza"}', '{"es": "¿Y si hacemos limpieza?", "pt-br": "Habbo porcalhão"}'),
    ROW('planet_of_love', '{"es": "El Planeta del Amor", "pt-br": "Planeta do Amor"}', '{"es": "¡Repleto de buenos sentimientos!", "pt-br": "Sempre um bom sentimento"}'),
    ROW('plant_big_cactus', '{"es": "Cáctus", "pt-br": "Cacto Maduro"}', '{"es": "Necesita luz y calor", "pt-br": "O monstro está escondido!  Shhhh..."}'),
    ROW('plant_bonsai', '{"es": "Bonsai", "pt-br": "Bonsai"}', '{"es": "Árbol diminuto", "pt-br": "Garantia de vida longa"}'),
    ROW('plant_bulrush', '{"es": "Junco", "pt-br": "Taboa"}', '{"es": "Ideal para la orilla de un río", "pt-br": "Estou ótima. No pântano."}'),
    ROW('plant_cruddy', '{"es": "Aloe Vera", "pt-br": "Aloe Vera"}', '{"es": "Viva el verde", "pt-br": "A planta dos Deuses. ;-)"}'),
    ROW('plant_fruittree', '{"es": "Árbol frutal", "pt-br": "Árvore Frutífera"}', '{"es": "El más dulce", "pt-br": "Boa colheita e frutas doces"}'),
    ROW('plant_maze', '{"es": "Seto", "pt-br": "Arbusto Labirinto"}', '{"es": "¡A ver si lo saltas!", "pt-br": "Cuidado com o Minotauro"}'),
    ROW('plant_maze_snow', '{"es": "Setos Nevados", "pt-br": "Arbustos Nevados (5)"}', '{"es": "¡Laberintos bajo cero!", "pt-br": "Cinco Arbustos Nevados."}'),
    ROW('plant_mazegate', '{"es": "Puertas del Seto Nevadas", "pt-br": "Portão de Arbusto"}', '{"es": "¡A ver si la abres!", "pt-br": "Densos labirintos"}'),
    ROW('plant_mazegate_snow', '{"es": "Puerta del Seto Nevada", "pt-br": "Arbusto-Portão Nevado"}', '{"es": "¡Laberintos bajo cero!", "pt-br": "Companhia A. Bonada. Todos os direitos reservados."}'),
    ROW('plant_pineapple', '{"es": "Piña", "pt-br": "Pé de Abacaxi"}', '{"es": "Necesita amor para crecer", "pt-br": "Coroa na cabeça"}'),
    ROW('plant_rose', '{"es": "Rosas", "pt-br": "Rosas"}', '{"es": "Siempre de moda", "pt-br": "Estilo e elegância"}'),
    ROW('plant_small_cactus', '{"es": "Cactusín", "pt-br": "Cacto Pequeno"}', '{"es": "Púas pequeñas para un dolor intenso", "pt-br": "Muito menos água do que no mundo real"}'),
    ROW('plant_sunflower', '{"es": "Girasol", "pt-br": "Girassol"}', '{"es": "Busco un sol", "pt-br": "Para Habbos felizes"}'),
    ROW('plant_valentinerose*1', '{"es": "Rosa San Valentín Roja", "pt-br": "Rosa Vermelha"}', '{"es": "Por un amor verdadero", "pt-br": "É o amooooor..."}'),
    ROW('plant_valentinerose*2', '{"es": "Rosa de San Valentín Blanca", "pt-br": "Rosa Branca"}', '{"es": "Para un amor secreto", "pt-br": "É o amooooor..."}'),
    ROW('plant_valentinerose*3', '{"es": "Rosa de San Valentín Amarilla", "pt-br": "Rosa Amarela"}', '{"es": "La más apasionada", "pt-br": "É o amooooor..."}'),
    ROW('plant_valentinerose*4', '{"es": "Rosa de San Valentín rosa", "pt-br": "Rosa Lilás"}', '{"es": "Un guiño para tu pareja", "pt-br": "É o amooooor..."}'),
    ROW('plant_valentinerose*5', '{"es": "Rosa de San Valentín Púrpura", "pt-br": "Rosa Roxa"}', '{"es": "Para ese píxel tan especial", "pt-br": "É o amooooor..."}'),
    ROW('plant_yukka', '{"es": "Yucca", "pt-br": "Iúca"}', '{"es": "Oxigénate", "pt-br": "Fácil de cuidar"}'),
    ROW('post.it', '{"es": "Notas adhesivas", "pt-br": "Caixa de adesivos"}', '{"es": "Pega tu vida", "pt-br": "Caixa de adesivos"}'),
    ROW('post.it.vd', '{"es": "Notas de corazón", "pt-br": "Corações adesivos"}', '{"es": "Pega tu amor...", "pt-br": "Corações adesivos"}'),
    ROW('post_it', '{"es": "Pack de notas", "pt-br": "Post-it"}', '{"es": "Pégalas en tu pared", "pt-br": "Coloque na sua parede!"}'),
    ROW('post_it_vd', '{"es": "Notas de corazón", "pt-br": "Corações adesivos"}', '{"es": "Pega tu amor...", "pt-br": "Corações adesivos"}'),
    ROW('poster', '{}', '{}'),
    ROW('present_gen', '{"es": "Regaloooooo", "pt-br": "Presente"}', '{"es": "¿Qué será?", "pt-br": "O que tem dentro?"}'),
    ROW('present_gen1', '{"es": "Regaloooooo", "pt-br": "Presente"}', '{"es": "¿Qué será?", "pt-br": "O que tem dentro?"}'),
    ROW('present_gen2', '{"es": "Regaloooooo", "pt-br": "Presente"}', '{"es": "¿Qué será?", "pt-br": "O que tem dentro?"}'),
    ROW('present_gen3', '{"es": "Regaloooooo", "pt-br": "Presente"}', '{"es": "¿Qué será?", "pt-br": "O que tem dentro?"}'),
    ROW('present_gen4', '{"es": "Regaloooooo", "pt-br": "Presente"}', '{"es": "¿Qué será?", "pt-br": "O que tem dentro?"}'),
    ROW('present_gen5', '{"es": "Regaloooooo", "pt-br": "Presente"}', '{"es": "¿Qué será?", "pt-br": "O que tem dentro?"}'),
    ROW('present_gen6', '{"es": "Regaloooooo", "pt-br": "Presente"}', '{"es": "¿Qué será?", "pt-br": "O que tem dentro?"}'),
    ROW('present_wrap*1', '{"es": "Regalo envuelto", "pt-br": "Presente"}', '{"es": "¡Una presentación espectacular!", "pt-br": "O que tem dentro?"}'),
    ROW('present_wrap*10', '{"es": "Regalo envuelto", "pt-br": "Presente"}', '{"es": "¡Una presentación espectacular!", "pt-br": "O que tem dentro?"}'),
    ROW('present_wrap*2', '{"es": "Regalo envuelto", "pt-br": "Presente"}', '{"es": "¡Una presentación espectacular!", "pt-br": "O que tem dentro?"}'),
    ROW('present_wrap*3', '{"es": "Raro Caja HC", "pt-br": "Presente"}', '{"es": "Ooh, ¿qué podrá ser?", "pt-br": "O que tem dentro?"}'),
    ROW('present_wrap*4', '{"es": "Regalo envuelto", "pt-br": "Presente"}', '{"es": "¡Una presentación espectacular!", "pt-br": "O que tem dentro?"}'),
    ROW('present_wrap*5', '{"es": "Regalo envuelto", "pt-br": "Presente"}', '{"es": "¡Una presentación espectacular!", "pt-br": "O que tem dentro?"}'),
    ROW('present_wrap*6', '{"es": "Regalo envuelto", "pt-br": "Presente"}', '{"es": "¡Una presentación espectacular!", "pt-br": "O que tem dentro?"}'),
    ROW('present_wrap*7', '{"es": "Regalo envuelto", "pt-br": "Presente"}', '{"es": "¡Una presentación espectacular!", "pt-br": "O que tem dentro?"}'),
    ROW('present_wrap*8', '{"es": "Regalo envuelto", "pt-br": "Presente"}', '{"es": "¡Una presentación espectacular!", "pt-br": "O que tem dentro?"}'),
    ROW('present_wrap*9', '{"es": "Regalo envuelto", "pt-br": "Presente"}', '{"es": "¡Una presentación espectacular!", "pt-br": "O que tem dentro?"}'),
    ROW('prize1', '{"es": "Trofeo de Oro", "pt-br": "Troféu de Ouro"}', '{"es": "Síii, el número uno", "pt-br": "Brilho vistoso"}'),
    ROW('prize2', '{"es": "Trofeo de Plata", "pt-br": "Troféu de Prata"}', '{"es": "Brillo traído de la luna", "pt-br": "Agradavelmente brilhante"}'),
    ROW('prize3', '{"es": "Trofeo de Bronce", "pt-br": "Troféu de Bronze"}', '{"es": "El último metal precioso", "pt-br": "Um prêmio da pesada"}'),
    ROW('prizetrophy*1', '{"es": "Trofeo Clásico", "pt-br": "Troféu Clássico"}', '{"es": "Oro campeón", "pt-br": "Ouro reluzente"}'),
    ROW('prizetrophy*2', '{"es": "Trofeo Clásico", "pt-br": "Troféu Clássico"}', '{"es": "Plata nocturna", "pt-br": "Prata reluzente"}'),
    ROW('prizetrophy*3', '{"es": "Trofeo Clásico", "pt-br": "Troféu Clássico"}', '{"es": "Bronce prehistórico", "pt-br": "Bronze sensacional"}'),
    ROW('prizetrophy2*1', '{"es": "Trofeo Pato", "pt-br": "Troféu de Pato"}', '{"es": "Número uno", "pt-br": "Ouro reluzente"}'),
    ROW('prizetrophy2*2', '{"es": "Trofeo Pato", "pt-br": "Troféu de Pato"}', '{"es": "Número dos", "pt-br": "Prata reluzente"}'),
    ROW('prizetrophy2*3', '{"es": "Trofeo Pato", "pt-br": "Troféu de Pato"}', '{"es": "Número tres", "pt-br": "Patinho bronzeado, uau"}'),
    ROW('prizetrophy3*1', '{"es": "Trofeo Globo", "pt-br": "Troféu de Globo"}', '{"es": "Oro de quilates insospechados", "pt-br": "Ouro reluzente"}'),
    ROW('prizetrophy3*2', '{"es": "Trofeo Globo", "pt-br": "Troféu de Globo"}', '{"es": "Plata de ley insospechada", "pt-br": "Prata reluzente"}'),
    ROW('prizetrophy3*3', '{"es": "Trofeo Globo", "pt-br": "Troféu de Globo"}', '{"es": "Bronce de calidad insospechada", "pt-br": "Bronze sensacional"}'),
    ROW('prizetrophy4*1', '{"es": "Trofeo Pez", "pt-br": "Troféu de Peixe"}', '{"es": "Número uno", "pt-br": "Ouro reluzente"}'),
    ROW('prizetrophy4*2', '{"es": "Trofeo Pez", "pt-br": "Troféu de Peixe"}', '{"es": "Número dos", "pt-br": "Prata reluzente"}'),
    ROW('prizetrophy4*3', '{"es": "Trofeo Pez", "pt-br": "Troféu de Peixe"}', '{"es": "Número tres", "pt-br": "Bronze sensacional"}'),
    ROW('prizetrophy5*1', '{"es": "Trofeo Dúo", "pt-br": "Troféu para Duplas"}', '{"es": "Oro Solar", "pt-br": "A amizade é o bem mais valioso de nossas vidas ;)"}'),
    ROW('prizetrophy5*2', '{"es": "Trofeo Dúo", "pt-br": "Troféu para Duplas"}', '{"es": "Plata Lunar", "pt-br": "A amizade reluz e alegra a sua vida"}'),
    ROW('prizetrophy5*3', '{"es": "Trofeo Dúo", "pt-br": "Troféu para Duplas"}', '{"es": "Bronce Terráqueo", "pt-br": "Mostre o carinho pelos verdadeiros amigos"}'),
    ROW('prizetrophy6*1', '{"es": "Trofeo Campeón", "pt-br": "Troféu para Campeões"}', '{"es": "Oro Solarr", "pt-br": "Ouro reluzente"}'),
    ROW('prizetrophy6*2', '{"es": "Trofeo Campeón", "pt-br": "Troféu para Campeões"}', '{"es": "Plata Lunar", "pt-br": "Prata reluzente"}'),
    ROW('prizetrophy6*3', '{"es": "Trofeo Campeón", "pt-br": "Troféu para Campeões"}', '{"es": "Bronce Terráqueo", "pt-br": "Bronze sensacional"}'),
    ROW('prizetrophy7*1', '{"es": "Habbo Trofeo Oro", "pt-br": "Troféu Habbo"}', '{"es": "Oro mítico", "pt-br": "Ouro reluzente"}'),
    ROW('prizetrophy7*2', '{"es": "Habbo Trofeo Plata", "pt-br": "Troféu Habbo"}', '{"es": "Plata legendaria", "pt-br": "Prata reluzente"}'),
    ROW('prizetrophy7*3', '{"es": "Habbo Trofeo Bronce", "pt-br": "Troféu Habbo"}', '{"es": "Bronce histórico", "pt-br": "Bronze reluzente"}'),
    ROW('prizetrophy8*1', '{"es": "Trofeo Dúo Burbujas", "pt-br": "Troféu Campeão"}', '{"es": "Glittery gold", "pt-br": "Orgulho de ser campeão"}'),
    ROW('prizetrophy9*1', '{"es": "Trofeo Campeón", "pt-br": "Troféu Campeão 2"}', '{"es": "Oro brillante", "pt-br": "Orgulho de ser campeão"}'),
    ROW('prizetrophy_cool', '{"es": "Trofeo Ángel", "pt-br": "Troféu Frio"}', '{"es": "¡En ti puedo confiar!", "pt-br": "Geladinho e refrescante, para amigos assim."}'),
    ROW('prizetrophy_hot', '{"es": "Trofeo Diablo", "pt-br": "Troféu Quente"}', '{"es": "¡Contigo siempre me divierto!", "pt-br": "Para amigos incendiários de corações."}'),
    ROW('prizetrophy_nrj', '{"es": "Trofeo NRJ", "pt-br": "Troféu NRJ"}', '{"es": "¡Vive cantando!", "pt-br": "prizetrophy_nrj desc"}'),
    ROW('prizetrophy_thumb', '{"es": "Trofeo Pulgares Arriba", "pt-br": "Troféu Joinha"}', '{"es": "¿Todo ok?", "pt-br": "Apenas para os Habbos mais joiados do Hotel."}'),
    ROW('pudding', '{"es": "El Pastel de Navidad", "pt-br": "Panetone"}', '{"es": "Ñam, ñam", "pt-br": "Com pedaços de chocolate."}'),
    ROW('pumpkin', '{"es": "La Calabaza", "pt-br": "Lâmpada de Abóbora"}', '{"es": "Esa sonrisa terrible", "pt-br": "Para um brilho fantasmagórico"}'),
    ROW('pura_mdl1*1', '{"es": "Sillón Pura Aguamarina", "pt-br": "Módulo Pura Aquamarine"}', '{"es": "¡Qué estilazo!", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl1*2', '{"es": "Sillón Pura Rosa", "pt-br": "Módulo Pura Rosa"}', '{"es": "¡Qué estilazo!", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl1*3', '{"es": "Sillón Pura negro", "pt-br": "Módulo Pura Preto"}', '{"es": "¡Pura comodidad!", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl1*4', '{"es": "Sillón Pura blanco", "pt-br": "Módulo Pura Branco"}', '{"es": "¡Pura comodidad!", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl1*5', '{"es": "Sillón Pura Beige", "pt-br": "Módulo Pura Bege"}', '{"es": "¡Qué estilazo!", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl1*6', '{"es": "Sillón Pura Azul", "pt-br": "Módulo Pura Azul"}', '{"es": "Cómodo, ¡al menos para un brazo!", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl1*7', '{"es": "Silla Pura Verde", "pt-br": "Módulo Pura Verde"}', '{"es": "¡Diseño revolucionario!", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl1*8', '{"es": "Sillón Pura Amarillo", "pt-br": "Módulo Pura Amarelo"}', '{"es": "¡Diseño espectacular!", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl1*9', '{"es": "Sillón Pura Rojo", "pt-br": "Módulo Pura Vermelho"}', '{"es": "¡Diseño espectacular!", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl2*1', '{"es": "Sillón Pura Aguamarina", "pt-br": "Módulo Pura Aquamarine"}', '{"es": "¡Qué estilazo!", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl2*2', '{"es": "Sillón Pura Rosa", "pt-br": "Módulo Pura Rosa"}', '{"es": "¡Qué estilazo!", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl2*3', '{"es": "Sillón Pura negro", "pt-br": "Módulo Pura Preto"}', '{"es": "¡Pura comodidad!", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl2*4', '{"es": "Sillón Pura blanco", "pt-br": "Módulo Pura Branco"}', '{"es": "¡Pura comodidad!", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl2*5', '{"es": "Sillón Pura Beige", "pt-br": "Módulo Pura Bege"}', '{"pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl2*6', '{"es": "Sillón Pura Azul", "pt-br": "Módulo Pura Azul"}', '{"es": "Cómodo, ¡al menos para un brazo!", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl2*7', '{"es": "Silla Pura Verde", "pt-br": "Módulo Pura Verde"}', '{"es": "¡Diseño revolucionario!", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl2*8', '{"es": "Sillón Pura Amarillo", "pt-br": "Módulo Pura Amarelo"}', '{"es": "¡Diseño espectacular!", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl2*9', '{"es": "Sillón Pura Rojo", "pt-br": "Módulo Pura Vermelho"}', '{"es": "¡Diseño espectacular!", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl3*1', '{"es": "Silla Pura Aguamarina", "pt-br": "Módulo Pura Aquamarine"}', '{"es": "¿Te gusta?", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl3*2', '{"es": "Silla Pura Rosa", "pt-br": "Módulo Pura Rosa"}', '{"es": "Menos para apoyar los brazos, ¡lo tiene todo!", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl3*3', '{"es": "Silla Pura Negra", "pt-br": "Módulo Pura Preto"}', '{"es": "Menos para apoyar los brazos, ¡lo tiene todo!", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl3*4', '{"es": "Silla Pura Blanca", "pt-br": "Módulo Pura Branco"}', '{"es": "Menos para apoyar los brazos, ¡ lo tiene todo!", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl3*5', '{"es": "Silla Pura Beige", "pt-br": "Módulo Pura Bege"}', '{"es": "¿Te gusta?", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl3*6', '{"es": "Silla Pura Azul", "pt-br": "Módulo Pura Azul"}', '{"es": "¡De diseño!", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl3*7', '{"es": "Silla Pura Verde", "pt-br": "Módulo Pura Verde"}', '{"es": "Menos para apoyar los brazos, ¡lo tiene todo!", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl3*8', '{"es": "Silla Pura Amarilla", "pt-br": "Módulo Pura Amarelo"}', '{"es": "¡Diseño espectacular!", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl3*9', '{"es": "Silla Pura Roja", "pt-br": "Módulo Pura Vermelho"}', '{"es": "Menos para apoyar los brazos, ¡lo tiene todo!", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl4*1', '{"es": "Sillón Esquina Aguamarina", "pt-br": "Módulo Pura Aquamarine"}', '{"es": "Aprovechando cualquier espacio", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl4*2', '{"es": "Sillón Esquina Rosa", "pt-br": "Módulo Pura Rosa"}', '{"es": "¡Comodidad por las esquinas!", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl4*3', '{"es": "Sillón Esquina Negro", "pt-br": "Módulo Pura Preto"}', '{"es": "¡Comodidad por las esquinas!", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl4*4', '{"es": "Sillón Esquina Blanco", "pt-br": "Módulo Pura Branco"}', '{"es": "¡Comodidad por las esquinas!", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl4*5', '{"es": "Sillón Esquina Beige", "pt-br": "Módulo Pura Bege"}', '{"es": "Hay que aprovechar todo el espacio", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl4*6', '{"es": "Sillón Esquina Azul", "pt-br": "Módulo Pura Azul"}', '{"es": "¡Diseño exclusivo!", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl4*7', '{"es": "Sillón Esquina Verde", "pt-br": "Módulo Pura Verde"}', '{"es": "¡Comodidad por las esquinas!", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl4*8', '{"es": "Sillón Esquina Amarillo", "pt-br": "Módulo Pura Amarelo"}', '{"es": "La comodidad hecha Pura", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl4*9', '{"es": "Sillón Esquina Rojo", "pt-br": "Módulo Pura Vermelho"}', '{"es": "¡Comodidad por las esquinas!", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl5*1', '{"es": "Butaca Pura Aguamarina", "pt-br": "Módulo Pura Aquamarine"}', '{"es": "Comodidad a tope", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl5*2', '{"es": "Butaca Pura Rosa", "pt-br": "Módulo Pura Rosa"}', '{"es": "¡Cabe en cualquier sitio!", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl5*3', '{"es": "Butaca Pura Negra", "pt-br": "Módulo Pura Preto"}', '{"es": "¡Cabe en cualquier sitio!", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl5*4', '{"es": "Butaca Pura Blanca", "pt-br": "Módulo Pura Branco"}', '{"es": "¡Cabe en cualquier sitio!", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl5*5', '{"es": "Butaca Pura Beige", "pt-br": "Módulo Pura Bege"}', '{"es": "Un acabado ideal", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl5*6', '{"es": "Butaca Pura Azul", "pt-br": "Módulo Pura Azul"}', '{"es": "¡Diseño exclusivo!", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl5*7', '{"es": "Butaca Pura Verde", "pt-br": "Módulo Pura Verde"}', '{"es": "¡Cabe en cualquier sitio!", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl5*8', '{"es": "Butaca Pura amarilla", "pt-br": "Módulo Pura Amarelo"}', '{"es": "¡Diseño espectacular!", "pt-br": "Monte seu espaço"}'),
    ROW('pura_mdl5*9', '{"es": "Butaca Pura Roja", "pt-br": "Módulo Pura Vermelho"}', '{"es": "¡Cabe en cualquier sitio!", "pt-br": "Monte seu espaço"}'),
    ROW('queue_tile1*0', '{"es": "Habbo Roller Blanco", "pt-br": "Habbo Roller Branco"}', '{"es": "El poder del movimiento", "pt-br": "O poder do movimento"}'),
    ROW('queue_tile1*1', '{"es": "Habbo Roller Rosa", "pt-br": "Habbo Roller Rosa"}', '{"es": "El poder del movimiento", "pt-br": "O poder do movimento"}'),
    ROW('queue_tile1*2', '{"es": "Habbo Roller Rojo", "pt-br": "Habbo Roller Vermelho"}', '{"es": "El poder del movimiento", "pt-br": "Pacote com 5 Rollers"}'),
    ROW('queue_tile1*3', '{"es": "Habbo Roller Aqua", "pt-br": "Habbo Roller Azul-piscina"}', '{"es": "El poder del movimiento", "pt-br": "O poder do movimento"}'),
    ROW('queue_tile1*4', '{"es": "Habbo Roller Oro", "pt-br": "Habbo Roller Dourado"}', '{"es": "El poder del movimiento", "pt-br": "O poder do movimento"}'),
    ROW('queue_tile1*5', '{"es": "Habbo Roller Negro", "pt-br": "Habbo Roller Preto"}', '{"es": "El poder del movimiento", "pt-br": "O poder do movimento"}'),
    ROW('queue_tile1*6', '{"es": "Habbo Roller Azul", "pt-br": "Habbo Roller Azul"}', '{"es": "El poder del movimiento", "pt-br": "O poder do movimento"}'),
    ROW('queue_tile1*7', '{"es": "Habbo Roller Púrpura", "pt-br": "Habbo Roller Roxo"}', '{"es": "El poder del movimiento", "pt-br": "O poder do movimento"}'),
    ROW('queue_tile1*8', '{"es": "Habbo Roller Ultramarino", "pt-br": "Habbo Roller Azul-marinho"}', '{"es": "El poder del movimiento", "pt-br": "O poder do movimento"}'),
    ROW('queue_tile1*9', '{"es": "Habbo Roller Verde", "pt-br": "Habbo Roller Verde"}', '{"es": "El poder del movimiento", "pt-br": "O poder do movimento"}'),
    ROW('rare_beehive_bulb', '{"es": "Luciérnaga Azul", "pt-br": "Lâmpada de Âmbar Azul"}', '{"es": "Luz y color en tu Sala...", "pt-br": "Um brilho cor de mel"}'),
    ROW('rare_beehive_bulb*1', '{"es": "Luciérnaga Roja", "pt-br": "Lâmpada de Âmbar Vermelha"}', '{"es": "Luz y color en tu Sala...", "pt-br": "Um brilho cor de mel"}'),
    ROW('rare_beehive_bulb*2', '{"es": "Luciérnaga Amarilla", "pt-br": "Lâmpada de Âmbar Amarela"}', '{"es": "La mejor luz posible", "pt-br": "Um brilho cor de mel"}'),
    ROW('rare_daffodil_rug', '{"es": "El Césped", "pt-br": "Canteiro de Pétalas"}', '{"es": "Para pisar descalzos", "pt-br": "Um pouco de natureza entre quatro paredes..."}'),
    ROW('rare_dragonlamp*0', '{"es": "Dragón de Fuego", "pt-br": "Lâmpada do Dragão de Fogo"}', '{"es": "Llamaradas inquietantes", "pt-br": "Abrasadora e assustadora!"}'),
    ROW('rare_dragonlamp*1', '{"es": "Dragón de Fuego Azul", "pt-br": "Lâmpada do Dragão Azul"}', '{"es": "Llamaradas inquietantes", "pt-br": "Abrasadora e assustadora!"}'),
    ROW('rare_dragonlamp*2', '{"es": "Dragón de Fuego Jade", "pt-br": "Lâmpada do Dragão de Jade"}', '{"es": "Llamaradas inquietantes", "pt-br": "Pedra ornamental, elegantes chamas"}'),
    ROW('rare_dragonlamp*3', '{"es": "Dragón de Fuego Plata", "pt-br": "Lâmpada do Dragão de Prata"}', '{"es": "Llamaradas inquietantes", "pt-br": "Abrasador e assustador!"}'),
    ROW('rare_dragonlamp*4', '{"es": "Dragón de Fuego Negro", "pt-br": "Lâmpada do Dragão da Escuridão"}', '{"es": "Llamaradas inquietantes", "pt-br": "Rituais de Zumbi!"}'),
    ROW('rare_dragonlamp*5', '{"es": "Dragón de Fuego Musgo", "pt-br": "Lâmpada do Dragão da Floresta"}', '{"es": "Llamaradas inquietantes", "pt-br": "Abrasador e assustador"}'),
    ROW('rare_dragonlamp*6', '{"es": "Dragón de Fuego Oro", "pt-br": "Dragão Dourado"}', '{"es": "Llamaradas inquietantes", "pt-br": "Abrasador e assustador!"}'),
    ROW('rare_dragonlamp*7', '{"es": "Dragón de Fuego Cielo", "pt-br": "Lâmpada do Dragão Alado"}', '{"es": "Llamaradas inquietantes", "pt-br": "Abrasadora e assustadora!"}'),
    ROW('rare_dragonlamp*8', '{"es": "Dragón de Fuego Bronce", "pt-br": "Dragão de Bronze"}', '{"es": "Llamaradas inquietantes", "pt-br": "Abrasador e assustador!"}'),
    ROW('rare_dragonlamp*9', '{"es": "Dragón Púrpura", "pt-br": "Lâmpada do Dragão Roxa"}', '{"es": "Llamaradas inquietantes", "pt-br": "Abrasadora e assustadora!"}'),
    ROW('rare_elephant_statue', '{"es": "Mamut de Oro", "pt-br": "Elefante Dourado"}', '{"es": "Todo un Símbolo", "pt-br": "Diga oi para a Lela"}'),
    ROW('rare_elephant_statue*1', '{"es": "Mamut de Plata", "pt-br": "Elefante Prateado"}', '{"es": "Todo un Símbolo", "pt-br": "Diga oi para a Lela"}'),
    ROW('rare_elephant_statue*2', '{"es": "Mamut de Bronce", "pt-br": "Elefante de Bronze"}', '{"es": "Todo un Símbolo", "pt-br": "Diga oi para a Lela"}'),
    ROW('rare_fan*0', '{"es": "La Ventolera Roja", "pt-br": "Ventilador Vermelho"}', '{"es": "Disfruta de la fresca brisa", "pt-br": "Para o calor ficar bem longe!"}'),
    ROW('rare_fan*1', '{"es": "La Ventolera Azul", "pt-br": "Ventilador Azul"}', '{"es": "Disfruta de la fresca brisa", "pt-br": "Para o calor ficar bem longe!"}'),
    ROW('rare_fan*2', '{"es": "La Ventolera Verde", "pt-br": "Ventilador Verde"}', '{"es": "Disfruta de la fresca brisa", "pt-br": "Para o calor ficar bem longe!"}'),
    ROW('rare_fan*3', '{"es": "La Ventolera Púrpura", "pt-br": "Ventilador Roxo"}', '{"es": "Disfruta de la fresca brisa", "pt-br": "Para o calor ficar bem longe!"}'),
    ROW('rare_fan*4', '{"es": "La Ventolera Rosa", "pt-br": "Ventilador Rosa"}', '{"es": "Disfruta de la brisa del amor", "pt-br": "Para o calor ficar bem longe!"}'),
    ROW('rare_fan*5', '{"es": "La Ventolera Amarilla", "pt-br": "Ventilador Amarelo"}', '{"es": "Disfruta de la fresca brisa", "pt-br": "Para o calor ficar bem longe!"}'),
    ROW('rare_fan*6', '{"es": "La Ventolera Ocre", "pt-br": "Ventilador Terra"}', '{"es": "Disfruta de la fresca brisa", "pt-br": "Refrescando os mais divos"}'),
    ROW('rare_fan*7', '{"es": "La Ventolera Bombón", "pt-br": "Ventilador Marrom"}', '{"es": "Disfruta de la fresca brisa", "pt-br": "Para o calor ficar bem longe!"}'),
    ROW('rare_fan*8', '{"es": "La Ventolera Turquesa", "pt-br": "Ventilador Turquesa"}', '{"es": "Disfruta de la fresca brisa", "pt-br": "Para o calor ficar bem longe!"}'),
    ROW('rare_fan*9', '{"es": "La Ventolera Fucsia", "pt-br": "Ventilador Fúcsia"}', '{"es": "Disfruta de la fresca brisa", "pt-br": "Para o calor ficar bem longe!"}'),
    ROW('rare_fountain', '{"es": "La Fontana Roja", "pt-br": "Chafariz Vermelho"}', '{"es": "Agua pura y cristalina", "pt-br": "Para nossos amigos com penas"}'),
    ROW('rare_fountain*1', '{"es": "La Fontana Gris", "pt-br": "Chafariz Cinza"}', '{"es": "Sencillamente espectacular", "pt-br": "Para nossos amigos com penas"}'),
    ROW('rare_fountain*2', '{"es": "La Fontana Mágica", "pt-br": "Chafariz Verde"}', '{"es": "¡Todo un Oasis!", "pt-br": "Para nossos amigos com penas"}'),
    ROW('rare_fountain*3', '{"es": "La Fontana Mágica", "pt-br": "Chafariz Azul"}', '{"es": "¡Todo un Oasis!", "pt-br": "Para nossos amigos com penas"}'),
    ROW('rare_globe', '{"es": "La Bola de Nieve", "pt-br": "Globo de Neve"}', '{"es": "Agítala, pero que no se te caiga.", "pt-br": "Está tudo branco..."}'),
    ROW('rare_hammock', '{"es": "Hamaca zZz", "pt-br": "Rede"}', '{"es": "Tus párpados pesan", "pt-br": "Cama ecológica"}'),
    ROW('rare_icecream*0', '{"es": "La Heladera Roja", "pt-br": "Sorveteira Vermelha"}', '{"es": "¡La fresa virtual mola!", "pt-br": "O seu sorvete virtual!"}'),
    ROW('rare_icecream*1', '{"es": "La Heladera Azul", "pt-br": "Sorveteira Azul"}', '{"es": "¡La vainilla virtual mola!", "pt-br": "O seu sorvete virtual!"}'),
    ROW('rare_icecream*2', '{"es": "La Heladera Verde", "pt-br": "Sorveteira Verde"}', '{"es": "¡La vainilla virtual mola!", "pt-br": "O seu sorvete virtual!"}'),
    ROW('rare_icecream*3', '{"es": "La Heladera Púrpura", "pt-br": "Sorveteira Roxa"}', '{"es": "¡La vainilla virtual mola!", "pt-br": "O seu sorvete virtual!"}'),
    ROW('rare_icecream*4', '{"es": "La Heladera Rosa", "pt-br": "Sorveteira Rosa"}', '{"es": "¡La vainilla virtual mola!", "pt-br": "O seu sorvete virtual!"}'),
    ROW('rare_icecream*5', '{"es": "La Heladera Dorada", "pt-br": "Sorveteira Dourada"}', '{"es": "¡La vainilla virtual mola!", "pt-br": "O seu sorvete virtual!"}'),
    ROW('rare_icecream*6', '{"es": "La Heladera Ocre", "pt-br": "Sorveteira Terra"}', '{"es": "¡La vainilla virtual mola!", "pt-br": "O seu sorvete virtual!"}'),
    ROW('rare_icecream*7', '{"es": "La Heladera Bombón", "pt-br": "Sorveteira Chocolate"}', '{"es": "¡El chocolate virtual sí que mola!", "pt-br": "O seu sorvete virtual!"}'),
    ROW('rare_icecream*8', '{"es": "La Heladera Aqua", "pt-br": "Sorveteira Azul-piscina"}', '{"es": "¡La vainilla virtual mola!", "pt-br": "O seu sorvete de chocolate virtual!"}'),
    ROW('rare_icecream*9', '{"es": "La Heladera Fucsia", "pt-br": "Sorveteira Flor"}', '{"es": "¡La vainilla virtual mola!", "pt-br": "O seu sorvete virtual!"}'),
    ROW('rare_icecream_campaign', '{"es": "Heladera Blanca", "pt-br": "Rare icecream white"}', '{"es": "Imprescindible en verano", "pt-br": "Basic model"}'),
    ROW('rare_ironmaiden', '{"es": "Teleport Iron Maiden", "pt-br": "Teleporte Macabro"}', '{"es": "Un viaje por el heavy metal", "pt-br": "Quem tem coragem, entra. Quem tem sorte, sai."}'),
    ROW('rare_mmmth', '{"es": "Sofá Mamut", "pt-br": "Sofá Mamute"}', '{"es": "Comodidad a la antigua usanza", "pt-br": "Descanse num pedaço quase vivo da pré-história."}'),
    ROW('rare_mnstr', '{"es": "Habbolus Venenosus", "pt-br": "Papa-Pixel"}', '{"es": "Yo no me acercaría...", "pt-br": "Mantenha distância"}'),
    ROW('rare_moonrug', '{"es": "Alfombra Lunar", "pt-br": "Tapete de Lua"}', '{"es": "¡Construida en 1969!", "pt-br": "Astronauta Libertário"}'),
    ROW('rare_parasol', '{"es": "Sombrilla Rosa", "pt-br": "Parasol"}', '{"es": "¡Sol! ¡Sol! ¡Sol!", "pt-br": "Zon! Zon! Zon!"}'),
    ROW('rare_parasol*0', '{"es": "La Sombrilla Verde", "pt-br": "Guarda-sol Verde"}', '{"es": "Bajo la sombra cuando hace sol...", "pt-br": "Bloqueie os raios UV!"}'),
    ROW('rare_parasol*1', '{"es": "La Sombrilla Amarilla", "pt-br": "Guarda-sol Squabble"}', '{"es": "Bajo la sombra cuando hace sol", "pt-br": "Protege a pele dos Habbo Astros."}'),
    ROW('rare_parasol*2', '{"es": "Parasol Naranja", "pt-br": "Guarda-sol Laranja"}', '{"es": "Bronceado con protección", "pt-br": "Bloqueie os raios UV!"}'),
    ROW('rare_parasol*3', '{"es": "Parasol Violeta", "pt-br": "Guarda-sol violeta"}', '{"es": "Vacaciones de verano", "pt-br": "Bloqueia os raios UV!"}'),
    ROW('rare_snowrug', '{"es": "Alfombra de nieve", "pt-br": "Neve"}', '{"es": "Tu propio paisaje congelado", "pt-br": "Terra gelada"}'),
    ROW('rare_stand', '{"es": "El Hablador", "pt-br": "Palanque do Professor"}', '{"es": "Querid@s Habbos...", "pt-br": "Suba e fale!"}'),
    ROW('rare_trex', '{"es": "T-Rex", "pt-br": "Raro Rex"}', '{"es": "La criatura más feroz, en Habbo", "pt-br": "O maior Raro de todos os tempos. Grande e grandioso."}'),
    ROW('rare_vdoll', '{"es": "Muñeco Voodoo", "pt-br": "Boneco de Vodu"}', '{"es": "Mientras no te represente a ti...", "pt-br": "Uma espetadinha aqui, uma agulhada ali."}'),
    ROW('rare_xmas_screen', '{"es": "¿Adivinas quien soy?", "pt-br": "Saudações da Lapônia"}', '{"es": "Ho Ho Ho!", "pt-br": "Ho Ho Ho!"}'),
    ROW('rcandle', '{"es": "Velas Rojas", "pt-br": "Vela vermelha"}', '{"es": "La luz más navideña", "pt-br": "Vela de Natal"}'),
    ROW('rcandleset', '{"es": "Las Velas Rojas", "pt-br": "Velas Vermelhas"}', '{"es": "Amor, amor...", "pt-br": "Simplicidade com estilo"}'),
    ROW('rclr_chair', '{"es": "Silla Palmera", "pt-br": "Cadeira Coqueiro"}', '{"es": "Un merecido descanso", "pt-br": "O côco vai cair no côco"}'),
    ROW('rclr_garden', '{"es": "Jardín Acuático", "pt-br": "Jardim Aquático"}', '{"es": "¡Hay que ver cómo luce!", "pt-br": "Oxigenação automática"}'),
    ROW('rclr_lamp', '{"es": "Lámpara Lunar", "pt-br": "Lâmpada de Lua"}', '{"es": "Ilumina tu Sala", "pt-br": "Iluminação natural e reciclada"}'),
    ROW('rclr_sofa', '{"es": "Sofá Polar", "pt-br": "Sofá Polar"}', '{"es": "Acoge y abriga por igual", "pt-br": "Juntando esquenta"}'),
    ROW('red_tv', '{"es": "Mini TV", "pt-br": "TV Portátil"}', '{"es": "¿Por qué perderte tus programas favoritos?", "pt-br": "Não perca a programação"}'),
    ROW('redhologram', '{"es": "Holo Angel", "pt-br": "Holo Mina"}', '{"es": "Gira, amor, gira", "pt-br": "Holograma é tudo."}'),
    ROW('rela_candle1', '{"es": "Velita Relax 1", "pt-br": "Vela Branca"}', '{"es": "¿Sientes cómo se duermen tus Píxeles?", "pt-br": "Um clássico da iluminação"}'),
    ROW('rela_candle2', '{"es": "Velita Relax 2", "pt-br": "Vela Vermelha"}', '{"es": "¿Sientes cómo se duermen tus Píxeles?", "pt-br": "Para ocasiões especiais"}'),
    ROW('rela_candle3', '{"es": "Velita Relax 3", "pt-br": "Vela Violeta"}', '{"es": "¿Sientes cómo se duermen tus Píxeles?", "pt-br": "No que você está pensando?"}'),
    ROW('rela_candles1', '{"es": "Velitas Relax 1", "pt-br": "Velas Brancas"}', '{"es": "¿Sientes cómo se duermen tus Píxeles?", "pt-br": "São também perfumadas!"}'),
    ROW('rela_candles2', '{"es": "Velitas Relax 2", "pt-br": "Velas Vermelhas"}', '{"es": "¿Sientes cómo se duermen tus Píxeles?", "pt-br": "Dão cor a qualquer ambiente"}'),
    ROW('rela_candles3', '{"es": "Velitas Relax 3", "pt-br": "Velas Violetas"}', '{"es": "¿Sientes cómo se duermen tus Píxeles?", "pt-br": "Um trava-língua de beleza"}'),
    ROW('rela_hchair', '{"es": "Hamaca Relax", "pt-br": "Cadeira de Cafofo"}', '{"es": "¿Sientes cómo se duermen tus Píxeles?", "pt-br": "Um ninho de aconchego"}'),
    ROW('rela_orchid', '{"es": "Orquídea", "pt-br": "Orquídea"}', '{"es": "Impide que tu aura se desequilibre", "pt-br": "A mais rara flor dos pixels"}'),
    ROW('rela_plant', '{"es": "Planta Relax", "pt-br": "Planta do Sossego"}', '{"es": "Mírala fijamente e igual te duermes...", "pt-br": "A paz repousa entre suas folhas"}'),
    ROW('rela_rock', '{"es": "Asiento Roca", "pt-br": "Rochas da Permanência"}', '{"es": "¿Sientes cómo se duermen tus Píxeles?", "pt-br": "Porque o que é pra sempre não acaba nunca"}'),
    ROW('rela_stick', '{"es": "Plantita Relax", "pt-br": "Espeto da Temperança"}', '{"es": "¿Sientes cómo se duermen tus Píxeles?", "pt-br": "Para ficar calminho"}'),
    ROW('rela_stone', '{"es": "Piedra Relax", "pt-br": "Pedras de Estimação"}', '{"es": "¿Sientes cómo se duermen tus Píxeles?", "pt-br": "No meio do caminho havia uma"}'),
    ROW('rela_wall', '{"es": "Pared Relajante", "pt-br": "Parede de Vidro"}', '{"es": "¡Llena de cuadritos!", "pt-br": "Transparente como alma deve ser"}'),
    ROW('rom_lamp', '{"es": "Lámpara de Cristal", "pt-br": "Lâmpada de Cristal"}', '{"es": "Una pizca de luz", "pt-br": "Ilumine com classe"}'),
    ROW('romantique_chair*1', '{"es": "Silla Romántica Rosa", "pt-br": "Rosa Poltrona"}', '{"es": "¡Que viva el amor!", "pt-br": "Salões e Valsas"}'),
    ROW('romantique_chair*2', '{"es": "Silla romántica Lima", "pt-br": "Cadeira Verde-Limão"}', '{"es": "La espiral del amor", "pt-br": "Estilo e Classe."}'),
    ROW('romantique_chair*3', '{"es": "Silla Romántica", "pt-br": "Cadeira Turquesa"}', '{"es": "Para tu pieza maestra", "pt-br": "Estilo e Classe"}'),
    ROW('romantique_chair*4', '{"es": "Silla romántica Dorada", "pt-br": "Verde Limão Poltrona"}', '{"es": "En el amor, ante todo, ¡comodidad!", "pt-br": "Salões e Valsas"}'),
    ROW('romantique_chair*5', '{"es": "Silla de piano Gris", "pt-br": "Poltrona Romântica Cinza"}', '{"es": "La espiral del amor", "pt-br": "Salões e Valsas"}'),
    ROW('romantique_clock', '{"es": "Reloj Antiguo", "pt-br": "Relógio Romântico"}', '{"es": "Sigue dando la hora correcta varios siglos después", "pt-br": "Um toque clássico e chique"}'),
    ROW('romantique_divan*1', '{"es": "Divan Romántico Rosa", "pt-br": "Rosa Chez Lounge"}', '{"es": "¡La elegancia Furnificada!", "pt-br": "Só vou encostar aqui um pouco e..."}'),
    ROW('romantique_divan*2', '{"es": "Diván Lima", "pt-br": "Divã Verde-Limão"}', '{"es": "¡Es hora de relajarse!", "pt-br": "Estilo e Classe"}'),
    ROW('romantique_divan*3', '{"es": "Diván Turquesa", "pt-br": "Divã Turquesa"}', '{"es": "¡Es hora de relajarse!", "pt-br": "Estilo e Classe"}'),
    ROW('romantique_divan*4', '{"es": "Diván romántico Dorado", "pt-br": "Verde Limão Chez Lounge"}', '{"es": "En el amor, ante todo, comodidad", "pt-br": "Só vou encostar aqui um pouco e..."}'),
    ROW('romantique_divan*5', '{"es": "Diván Gris", "pt-br": "Cinza Chez Lounge"}', '{"es": "¡Es hora de relajarse!", "pt-br": "Só vou encostar aqui um pouco e..."}'),
    ROW('romantique_divider*1', '{"es": "Divisor romántico rosa", "pt-br": "Rosa Biombo"}', '{"es": "¡Viva el amor!", "pt-br": "Vamos por partes"}'),
    ROW('romantique_divider*2', '{"es": "Divisor Lima", "pt-br": "Biombo Verde-Limão"}', '{"es": "¡Cada cosa en su sitio!", "pt-br": "Estilo e Classe"}'),
    ROW('romantique_divider*3', '{"es": "Divisor Turquesa", "pt-br": "Biombo Turquesa"}', '{"es": "¡Cada cosa en su sitio!", "pt-br": "Estilo e Classe"}'),
    ROW('romantique_divider*4', '{"es": "Divisor romántico Dorado", "pt-br": "Biombo Ocre"}', '{"es": "¡Cada cosa en su sitio!", "pt-br": "Mantendo as coisas separadas"}'),
    ROW('romantique_mirrortabl', '{"es": "Tocador", "pt-br": "Espelho Lendário"}', '{"es": "¿Nos preparamos para la gran cita?", "pt-br": "Uma lenda urbana em seu Quarto"}'),
    ROW('romantique_pianochair*1', '{"es": "Silla de Piano rosa", "pt-br": "Banco Clássico"}', '{"es": "¡Comodidad para la música!", "pt-br": "Banco Clássico"}'),
    ROW('romantique_pianochair*2', '{"es": "Piano de cola Lima", "pt-br": "Banco Clássico"}', '{"es": "Para tus piezas maestras", "pt-br": "Estilo verde limão"}'),
    ROW('romantique_pianochair*3', '{"es": "Silla de piano Turquesa", "pt-br": "Banco Clássico"}', '{"es": "Para tus piezas maestras", "pt-br": "Estilo Turquesa"}'),
    ROW('romantique_pianochair*4', '{"es": "Silla de Piano dorada", "pt-br": "Banco dourado"}', '{"es": "En el amor, ante todo, ¡comodidad!", "pt-br": "Obras primas"}'),
    ROW('romantique_pianochair*5', '{"es": "Silla de piano gris", "pt-br": "Cinza Piano Stool"}', '{"es": "Para tus piezas maestras", "pt-br": "Obras primas"}'),
    ROW('romantique_smalltabl*1', '{"es": "Mesita Rosa", "pt-br": "Mesinha Rosa"}', '{"es": "¡Viva el amor!", "pt-br": "Todos precisam de uma Mesinha..."}'),
    ROW('romantique_smalltabl*2', '{"es": "Mesita Lima", "pt-br": "Mesinha Verde-Limão"}', '{"es": "Toda bandeja necesita una mesa", "pt-br": "Estilo e Classe"}'),
    ROW('romantique_smalltabl*3', '{"es": "Mesita Turquesa", "pt-br": "Mesinha Turquesa"}', '{"es": "Toda bandeja necesita una mesa", "pt-br": "Estilo e Classe"}'),
    ROW('romantique_smalltabl*4', '{"es": "Mesita romántica Dorada", "pt-br": "Verde Limão Bandeja"}', '{"es": "¡Desprende puro amor!", "pt-br": "Servindo com glamour, toujours"}'),
    ROW('romantique_smalltabl*5', '{"es": "Mesita romántica Gris", "pt-br": "Cinza Bandeja"}', '{"es": "¡Desprende puro amor!", "pt-br": "Servindo com glamour, toujours"}'),
    ROW('romantique_tray1', '{"es": "Bandeja de Desayuno", "pt-br": "Bandeja Café"}', '{"es": "¡Empieza el día con energía!", "pt-br": "Estilo e Classe"}'),
    ROW('romantique_tray2', '{"es": "Bandeja para Gourmets", "pt-br": "Bandeja Caviar"}', '{"es": "¡Date un capricho!", "pt-br": "Estilo e Classe"}'),
    ROW('roomdimmer', '{"es": "Regulador de Luces", "pt-br": "Regulador de Luzes"}', '{"es": "Cambia el tono de luz a tu voluntad", "pt-br": "Crie novos ambientes em um mesmo Quarto"}'),
    ROW('rope_divider', '{"es": "Cordón HC", "pt-br": "Corda de Isolamento"}', '{"es": "¡El cuarto de Ember! El quinto es la propia Pared", "pt-br": "VIPs desse lado"}'),
    ROW('rubberchair*1', '{"es": "Silla Hinchable Azul", "pt-br": "Cadeira Ego Inflável HC"}', '{"es": "Suave e impermeable", "pt-br": "Egos inflados. Artistas feitos de ar. Exclusivo HC."}'),
    ROW('rubberchair*2', '{"es": "Silla Hinchable Rosa", "pt-br": "Cadeira Ego Inflável"}', '{"es": "Suave e impermeable", "pt-br": "Egos grandes. Artistas de feitos ar."}'),
    ROW('rubberchair*3', '{"es": "Silla Hinchable Naranja", "pt-br": "Cadeira Inflável"}', '{"es": "Suave e impermeable", "pt-br": "Rafting, corredeiras, botes e sala de estar."}'),
    ROW('rubberchair*4', '{"es": "Silla Hinchable Océano", "pt-br": "Cadeira Ego Inflável"}', '{"es": "Suave e impermeable", "pt-br": "Egos inflados. Artistas feitos de ar."}'),
    ROW('rubberchair*5', '{"es": "Silla Hinchable Lima", "pt-br": "Cadeira Ego Inflável"}', '{"es": "Suave e impermeable", "pt-br": "Egos grandes. Artistas de feitos ar."}'),
    ROW('rubberchair*6', '{"es": "Silla Hinchable Violeta", "pt-br": "Cadeira Ego Inflável"}', '{"es": "Suave e impermeable", "pt-br": "Egos inflados. Artistas feitos de ar."}'),
    ROW('rubberchair*7', '{"es": "Silla Hinchable Blanca", "pt-br": "Cadeira Ego Inflável"}', '{"es": "Suave e impermeable", "pt-br": "Egos inflados. Artistas feitos de ar."}'),
    ROW('rubberchair*8', '{"es": "Silla Hinchable negra", "pt-br": "Cadeira Ego RockStar"}', '{"es": "Suave e impermeable", "pt-br": "Para aqueles que tem rock em seu sangue!"}'),
    ROW('runway_bench', '{"es": "Cómoda Runway", "pt-br": "Banco Esbelto"}', '{"es": "Menudo estilazo", "pt-br": "Um toque das passarelas para o seu Quarto."}'),
    ROW('runway_bigchr_1', '{"es": "Sillón Runway 1", "pt-br": "Poltrona Azul"}', '{"es": "Menudo estilazo", "pt-br": "Um cantinho fashion para descansar."}'),
    ROW('runway_bigchr_2', '{"es": "Sillón Runway 2", "pt-br": "Poltrona Vermelha"}', '{"es": "Menudo estilazo", "pt-br": "Um cantinho fashion para descansar."}'),
    ROW('runway_bigchr_3', '{"es": "Sillón Runway 3", "pt-br": "Poltrona Marrom"}', '{"es": "Menudo estilazo", "pt-br": "Um cantinho fashion para descansar."}'),
    ROW('runway_bigchr_4', '{"es": "Sillón Runway 4", "pt-br": "Poltrona Violeta"}', '{"es": "Menudo estilazo", "pt-br": "Um cantinho fashion para descansar."}'),
    ROW('runway_bigchr_5', '{"es": "Sillón Runway 5", "pt-br": "Poltrona Preta"}', '{"es": "Menudo estilazo", "pt-br": "Um cantinho fashion para descansar."}'),
    ROW('runway_block_1', '{"es": "Escenario Runway 1", "pt-br": "Passarela de Desfile"}', '{"es": "Menudo estilazo", "pt-br": "Mostre seu talento de modelo!"}'),
    ROW('runway_block_2', '{"es": "Escenario Runway 2", "pt-br": "Passarela de Desfile Escura"}', '{"es": "Menudo estilazo", "pt-br": "Um efeito sobre a passarela sempre gera bafafá."}'),
    ROW('runway_chair_1', '{"es": "Silla de Peluquería Negra", "pt-br": "Cadeira de Salão Preta"}', '{"es": "¡Qué guap@ te van a dejar!", "pt-br": "Barba e cabelo completos."}'),
    ROW('runway_chair_2', '{"es": "Silla de Peluquería Rosa", "pt-br": "Cadeira de Salão Vermelha"}', '{"es": "¡Qué guap@ te van a dejar!", "pt-br": "Sente e relaxe. A beleza espera vem até você."}'),
    ROW('runway_display', '{"es": "Expositor de Peluquería", "pt-br": "Expositor"}', '{"es": "Los mejores productos a tu alcance", "pt-br": "Fica tudo à mostra, organizadinho."}'),
    ROW('runway_dvdr', '{"es": "Espejo de Peluquería", "pt-br": "Penteadeira"}', '{"es": "¡Qué guap@ te van a dejar!", "pt-br": "Passe sempre por ela antes de sair por aí."}'),
    ROW('runway_fabric', '{"es": "Rollos de Telas", "pt-br": "Rolos de Tecido"}', '{"es": "Seda natural, como poco", "pt-br": "Estoque de lindos visuais."}'),
    ROW('runway_head', '{"es": "Busto de Peluquería", "pt-br": "Cabeça"}', '{"es": "Imagina que el peinado te queda así de bien", "pt-br": "Pra quem vive perdendo a sua..."}'),
    ROW('runway_manqn_1', '{"es": "Maniquí Caballero", "pt-br": "Manequim Menino"}', '{"es": "Todo te sienta como un guante", "pt-br": "Ele suporta agulhadas sem reclamar."}'),
    ROW('runway_manqn_2', '{"es": "Maniquí Señora", "pt-br": "Manequim Menina"}', '{"es": "Todo te sienta como un guante", "pt-br": "Parada, calada, misteriosa."}'),
    ROW('runway_shelf', '{"es": "Utensilios de Peluquería", "pt-br": "Balcão de Beleza"}', '{"es": "¡Qué guap@ te van a dejar!", "pt-br": "Tudo o que você precisa para melhorar seu visual."}'),
    ROW('runway_stool', '{"es": "Taburete para diseñadores", "pt-br": "Banqueta Fashion"}', '{"es": "No veas cómo brotan las ideas", "pt-br": "Onde os criadores de moda descansam."}'),
    ROW('runway_table_1', '{"es": "Mesa de diseñador", "pt-br": "Mesa de Criação"}', '{"es": "No veas cómo brotan las ideas", "pt-br": "É aqui onde a magia começa."}'),
    ROW('runway_table_2', '{"es": "Máquina de coser", "pt-br": "Máquina de Costura"}', '{"es": "Aguja e hilo, todo es ponerse", "pt-br": "Não se esqueça de usal dedal."}'),
    ROW('safe_silo', '{"es": "Caja Fuerte", "pt-br": "Cofre Frigobar"}', '{"es": "Tu bebida, a salvo", "pt-br": "Anti assaltantes de geladeira"}'),
    ROW('safe_silo*2', '{"es": "Caja Fuerte Negra", "pt-br": "Cofre Frigobar Preto"}', '{"es": "Tu bebida, a salvo", "pt-br": "Anti assaltantes de geladeira"}'),
    ROW('safe_silo*3', '{"es": "Caja Fuerte Blanca", "pt-br": "Cofre Branco"}', '{"es": "Tu bebida, a salvo", "pt-br": "Anti assaltantes de geladeira"}'),
    ROW('safe_silo*4', '{"es": "Caja Fuerte Beige", "pt-br": "Bege Cofre Frigobar"}', '{"es": "Tu bebida, a salvo", "pt-br": "Anti assaltantes de geladeira"}'),
    ROW('safe_silo*5', '{"es": "Caja Fuerte Rosa", "pt-br": "Cofre Rosa"}', '{"es": "Tu bebida, a salvo", "pt-br": "É prático, é útil, é rosa!"}'),
    ROW('safe_silo*6', '{"es": "Caja Fuerte Azul", "pt-br": "Azul Cofre Frigobar"}', '{"es": "Tu bebida, a salvo", "pt-br": "Anti assaltantes de geladeira"}'),
    ROW('safe_silo*7', '{"es": "Caja Fuerte Verde", "pt-br": "Verde Cofre Frigobar"}', '{"es": "Tu bebida, a salvo", "pt-br": "Anti assaltantes de geladeira"}'),
    ROW('safe_silo*8', '{"es": "Caja Fuerte Amarilla", "pt-br": "Amarelo Cofre Frigobar"}', '{"es": "¡Tus tesoros, a salvo!", "pt-br": "Anti assaltantes de geladeira"}'),
    ROW('safe_silo*9', '{"es": "Caja Fuerte Roja", "pt-br": "Cofre Frigobar Vermelho"}', '{"es": "Tu bebida, a salvo", "pt-br": "Guarde suas riquezas e sanduíches"}'),
    ROW('safe_silo_pb', '{"es": "postbank Safe", "pt-br": "postbank Safe"}', '{"es": "Totally shatter-proof!", "pt-br": "Totally shatter-proof!"}'),
    ROW('samovar', '{"es": "Samovar Ruso", "pt-br": "Samovar Russo"}', '{"es": "Para una buena taza de té", "pt-br": "Clique para tomar um chá refrescante"}'),
    ROW('sand_cstl_gate', '{"es": "Puerta de Arena", "pt-br": "Portão Castelo de Areia"}', '{"es": "¡Entra sin llamar!", "pt-br": "Muralha da China"}'),
    ROW('sand_cstl_twr', '{"es": "Torre de Arena", "pt-br": "Torre Castelo de Areia"}', '{"es": "Desde aquí se ve toda la playa", "pt-br": "Jogue suas tranças, Rapunzel"}'),
    ROW('sand_cstl_wall', '{"es": "Pared de Arena", "pt-br": "Muro Castelo de Areia"}', '{"es": "Pero no se puede atravesar", "pt-br": "Protege da onda do mar"}'),
    ROW('sandrug', '{"es": "Alfombra Playa", "pt-br": "Areia"}', '{"es": "Paraíso a pie de mar", "pt-br": "Tropical e paradisíaco"}'),
    ROW('saturn', '{"es": "Saturno", "pt-br": "Saturno"}', '{"es": "¡Cómo brillan sus anillos!", "pt-br": "O gigante planeta dos anéis"}'),
    ROW('scifidoor*1', '{"es": "Puerta Espacial Rosa", "pt-br": "Porta Nave Espacial Rosa"}', '{"es": "Alta Tecnología Galáctica", "pt-br": "É de outro mundo!"}'),
    ROW('scifidoor*10', '{"es": "Puerta Espacial Violeta", "pt-br": "Porta Nave Espacial Violeta"}', '{"es": "Alta Tecnología Galáctica", "pt-br": "É de outro mundo!"}'),
    ROW('scifidoor*2', '{"es": "Puerta Espacial Dorada", "pt-br": "Porta Nave Espacial Amarela"}', '{"es": "Alta Tecnología Galáctica", "pt-br": "É de outro mundo!"}'),
    ROW('scifidoor*3', '{"es": "Puerta Espacial Celeste", "pt-br": "Porta Nave Espacial Azul-clara"}', '{"es": "Alta Tecnología Galáctica", "pt-br": "É de outro mundo!"}'),
    ROW('scifidoor*4', '{"es": "Puerta Espacial Jade", "pt-br": "Porta Nave Espacial Jade"}', '{"es": "Alta Tecnología Galáctica", "pt-br": "É de outro mundo!"}'),
    ROW('scifidoor*5', '{"es": "Puerta Espacial Blanca", "pt-br": "Porta Nave Espacial Branca"}', '{"es": "Alta Tecnología Galáctica", "pt-br": "É de outro mundo!"}'),
    ROW('scifidoor*6', '{"es": "Puerta Espacial Negra", "pt-br": "Monolito Negro"}', '{"es": "Alta Tecnología Galáctica", "pt-br": "É de outro mundo!"}'),
    ROW('scifidoor*7', '{"es": "Puerta Espacial Nube", "pt-br": "Porta Nave Espacial Azul-piscina"}', '{"es": "Alta Tecnología Galáctica", "pt-br": "É de outro mundo!"}'),
    ROW('scifidoor*8', '{"es": "Puerta Espacial Púrpura", "pt-br": "Porta Nave Espacial Roxa"}', '{"es": "Alta Tecnología Galáctica", "pt-br": "É de outro mundo!"}'),
    ROW('scifidoor*9', '{"es": "Puerta Espacial Azul", "pt-br": "Porta Nave Espacial Azul"}', '{"es": "Alta Tecnología Galáctica", "pt-br": "É de outro mundo!"}'),
    ROW('scifiport*0', '{"es": "Puerta Láser Roja", "pt-br": "Porta Sci-Fi Vermelha"}', '{"es": "Rayo energético ¡Da calambre!", "pt-br": "Feixes elétricos. Proibida a entrada!"}'),
    ROW('scifiport*1', '{"es": "Puerta Láser Amarilla", "pt-br": "Porta Sci-Fi Amarela"}', '{"es": "Rayo energético ¡Da calambre!", "pt-br": "Feixes elétricos. Proibida a entrada!"}'),
    ROW('scifiport*2', '{"es": "Puerta Láser Azul", "pt-br": "Porta Sci-Fi Azul"}', '{"es": "Rayo energético ¡Da calambre!", "pt-br": "Feixes elétricos. Proibida a entrada!"}'),
    ROW('scifiport*3', '{"es": "Puerta Láser Jade", "pt-br": "Porta Sci-Fi Jade"}', '{"es": "Rayo energético ¡Da calambre!", "pt-br": "Feixes elétricos. Proibida a entrada!"}'),
    ROW('scifiport*4', '{"es": "Puerta Láser Rosa", "pt-br": "Porta Sci-Fi Rosa"}', '{"es": "Rayo energético ¡Da calambre!", "pt-br": "Feixes elétricos. Proibida a entrada!"}'),
    ROW('scifiport*5', '{"es": "Puerta Láser Negra", "pt-br": "Porta Sci-Fi Preta"}', '{"es": "Rayo energético ¡Da calambre!", "pt-br": "Feixes elétricos. Proibida a entrada!"}'),
    ROW('scifiport*6', '{"es": "Puerta Láser Blanca", "pt-br": "Porta Sci-Fi Branca"}', '{"es": "Rayo energético ¡Da calambre!", "pt-br": "Feixes elétricos. Proibida a entrada!"}'),
    ROW('scifiport*7', '{"es": "Puerta Láser Turquesa", "pt-br": "Porta Sci-Fi Turquesa"}', '{"es": "Rayo energético ¡Da calambre!", "pt-br": "Feixes elétricos. Proibida a entrada!"}'),
    ROW('scifiport*8', '{"es": "Puerta Láser Púrpura", "pt-br": "Porta Sci-Fi Roxa"}', '{"es": "Rayo energético ¡Da calambre!", "pt-br": "Feixes elétricos. Proibida a entrada!"}'),
    ROW('scifiport*9', '{"es": "Puerta Láser Violeta", "pt-br": "Porta Sci-Fi Violeta"}', '{"es": "Rayo energético ¡Da calambre!", "pt-br": "Feixes elétricos. Proibida a entrada!"}'),
    ROW('scifirocket*0', '{"es": "La Humareda Roja", "pt-br": "Máquina de Fumaça"}', '{"es": "Nubes bajo tu techo", "pt-br": "Para as festas de Ano Novo. Bem vindo 2007."}'),
    ROW('scifirocket*1', '{"es": "La Humareda Amarilla", "pt-br": "Máquina de Fumaça Amarela"}', '{"es": "Nubes bajo tu techo", "pt-br": "Alta tecnologia com um toque do passado"}'),
    ROW('scifirocket*2', '{"es": "La Humareda Azul", "pt-br": "Máquina de Fumaça Azul"}', '{"es": "Nubes bajo tu techo", "pt-br": "Alta tecnologia com um toque do passado"}'),
    ROW('scifirocket*3', '{"es": "La Humareda Jade", "pt-br": "Máquina de Fumaça Jade"}', '{"es": "Nubes bajo tu techo", "pt-br": "Alta tecnologia com um toque do passado"}'),
    ROW('scifirocket*4', '{"es": "La Humareda Rosa", "pt-br": "Máquina de Fumaça Rosa"}', '{"es": "Nubes bajo tu techo", "pt-br": "Alta tecnologia com um toque do passado"}'),
    ROW('scifirocket*5', '{"es": "La Humareda Negra", "pt-br": "Máquina de Fumaça Preta"}', '{"es": "Nubes bajo tu techo", "pt-br": "Alta tecnologia com um toque do passado"}'),
    ROW('scifirocket*6', '{"es": "La Humareda Blanca", "pt-br": "Máquina de Fumaça Branca"}', '{"es": "Nubes bajo tu techo", "pt-br": "Alta tecnologia com um toque do passado"}'),
    ROW('scifirocket*7', '{"es": "La Humareda Turquesa", "pt-br": "Máquina de Fumaça Azul-piscina"}', '{"es": "Nubes bajo tu techo", "pt-br": "Alta tecnologia com um toque do passado"}'),
    ROW('scifirocket*8', '{"es": "La Humareda Púrpura", "pt-br": "Máquina de Fumaça Roxa"}', '{"es": "Nubes bajo tu techo", "pt-br": "Alta tecnologia com um toque do passado"}'),
    ROW('scifirocket*9', '{"es": "La Humareda Violeta", "pt-br": "Máquina de Fumaça Violeta"}', '{"es": "Nubes bajo tu techo", "pt-br": "Alta tecnologia com um toque do passado"}'),
    ROW('sf_floor', '{"es": "Suelo Cósmico", "pt-br": "Piso Transparente"}', '{"es": "¡Mira las estrellas a tus pìes!", "pt-br": "Dá pra ver as engrenagens, ó.."}'),
    ROW('sf_gate', '{"es": "Puerta Espacial", "pt-br": "Portão Estelar"}', '{"es": "¡Entra en la Nave!", "pt-br": "Atravessando estrelas"}'),
    ROW('sf_mbar', '{"es": "Astro-Bar", "pt-br": "Astrobar"}', '{"es": "Sírvete tú mismo", "pt-br": "Bebida galática"}'),
    ROW('sf_pod', '{"es": "Cama Criogénica", "pt-br": "Câmara Criogênica"}', '{"es": "¡Descansa mientras viajas a Píxeles luz de distancia!", "pt-br": "Conserva os Habbos congelados para sempre"}'),
    ROW('sf_roller', '{"es": "Roller Galáctico", "pt-br": "Roller do Espaço"}', '{"es": "Deslízate por tu Nave", "pt-br": "Sempre em movimento"}'),
    ROW('sf_roof', '{"es": "Techo de la Nave", "pt-br": "Teto de Nave"}', '{"es": "¡No puede faltarte!", "pt-br": "Quem diria, hem?"}'),
    ROW('sf_stick', '{"es": "Vara láser", "pt-br": "Bastão de Laser"}', '{"es": "¿Sientes la fuerza?", "pt-br": "Que a força esteja com você"}'),
    ROW('sf_tele', '{"es": "Telespacio", "pt-br": "Teleporte Espacial"}', '{"es": "El espacio te espera", "pt-br": "Materialize-se numa outra galáxia"}'),
    ROW('sf_wall', '{"es": "Pared de la nave", "pt-br": "Parede"}', '{"es": "Si se agrieta, estás perdido en el espacio", "pt-br": "Parede"}'),
    ROW('sf_wall2', '{"es": "Paredes de la Nave", "pt-br": "Parede de Nave"}', '{"es": "Recubre tu Nave", "pt-br": "Cuidado onde encosta"}'),
    ROW('sf_wall3', '{"es": "Esquina de la Nave", "pt-br": "Canto de Nave"}', '{"es": "¡Qué detallazo!", "pt-br": "A parte que dá mais trabalho pra limpar"}'),
    ROW('sf_window', '{"es": "Ventana Galáctica", "pt-br": "Janela Sci-Fi"}', '{"es": "¡Cuélgate de  las estrellas!", "pt-br": "Para ver o céu estrelado"}'),
    ROW('shelves_armas', '{"es": "Estante para libros", "pt-br": "Estante de Livros"}', '{"es": "Coloca lo que quieras", "pt-br": "Para contar histórias ao lado da lareira"}')) AS tr (classname, names, descs)
  ON f.`classname` = tr.classname
SET f.`localized_name` = CAST(tr.names AS JSON), f.`localized_desc` = CAST(tr.descs AS JSON);
UPDATE `definition_furniture` f JOIN (VALUES
    ROW('shelves_basic', '{"es": "Estanteria", "pt-br": "Prateleiras pura"}', '{"es": "Grande y de diseño", "pt-br": "Prateleiras pura série 404"}'),
    ROW('shelves_norja', '{"es": "Estanteria", "pt-br": "Estante"}', '{"es": "Para colocar tus emociones", "pt-br": "Para itens decorativos"}'),
    ROW('shelves_norja*2', '{"es": "Librería Negra", "pt-br": "Estante Preta"}', '{"es": "¿Has leído todos?", "pt-br": "Livro deixa inteligente"}'),
    ROW('shelves_norja*3', '{"es": "Librería Blanca", "pt-br": "Estrante Branca"}', '{"es": "¿Has leído todos?", "pt-br": "Livro deixa inteligente"}'),
    ROW('shelves_norja*4', '{"es": "Librería Marina", "pt-br": "Estante Águamarinha"}', '{"es": "¿Has leído todos?", "pt-br": "Livro deixa inteligente"}'),
    ROW('shelves_norja*5', '{"es": "Librería Rosa", "pt-br": "Estante Rosa"}', '{"es": "¿Has leído todos?", "pt-br": "Livro deixa inteligente"}'),
    ROW('shelves_norja*6', '{"es": "Librería Azul", "pt-br": "Estante Azul"}', '{"es": "¿Has leído todos?", "pt-br": "Livro deixa inteligente"}'),
    ROW('shelves_norja*7', '{"es": "Librería Verde", "pt-br": "Estante Verde"}', '{"es": "¿Has leído todos?", "pt-br": "Livro deixa inteligente"}'),
    ROW('shelves_norja*8', '{"es": "Librería Amarilla", "pt-br": "Estante Amarela"}', '{"es": "¿Has leído todos?", "pt-br": "Livro deixa inteligente"}'),
    ROW('shelves_norja*9', '{"es": "Librería Roja", "pt-br": "Estante Vermelha"}', '{"es": "¿Has leído todos?", "pt-br": "Livro deixa inteligente"}'),
    ROW('shelves_polyfon', '{"es": "Estanteria", "pt-br": "Estante de Livros"}', '{"es": "Muy Design :)", "pt-br": "Para decoração artística"}'),
    ROW('shelves_silo', '{"es": "Estanteria", "pt-br": "Estante de Livros"}', '{"es": "Coloca tus cosas y más...", "pt-br": "Para itens decorativos e livros em art deco"}'),
    ROW('silo_studydesk', '{"es": "Escritorio Area", "pt-br": "Mesa de Estudos"}', '{"es": "Gimnasia mental a tope", "pt-br": "Dedique-se. Tire 10 na prova."}'),
    ROW('sink', '{"es": "Lavabo", "pt-br": "Pia"}', '{"es": "La factura del agua, ¿ahora quién la paga?", "pt-br": "Água quente e fria de graça"}'),
    ROW('skullcandle', '{"es": "Calavera", "pt-br": "Castiçal de Crânio"}', '{"es": "Luz y sustos", "pt-br": "Coitado do Mateus..."}'),
    ROW('sleepingbag*1', '{"es": "Saco de dormir Rojo", "pt-br": "Saco de Dormir Vermelho"}', '{"es": "Duerme bajo las estrellas", "pt-br": "Sonhos de amor"}'),
    ROW('sleepingbag*10', '{"es": "Saco de dormir Caqui", "pt-br": "Saco de Dormir Cáqui"}', '{"es": "Duerme bajo las estrellas", "pt-br": "Conforto para viajantes"}'),
    ROW('sleepingbag*2', '{"es": "Saco de dormir Lima", "pt-br": "Saco de Dormir Lima"}', '{"es": "Duerme bajo las estrellas", "pt-br": "Conforto para viajantes"}'),
    ROW('sleepingbag*3', '{"es": "Saco de los Linces", "pt-br": "Saco de Dormir Oceano"}', '{"es": "Duerme bajo las estrellas", "pt-br": "Conforto para viajantes"}'),
    ROW('sleepingbag*4', '{"es": "Saco de dormir Verde", "pt-br": "Saco de Dormir Camuflado"}', '{"es": "Duerme bajo las estrellas", "pt-br": "Conforto para viajantes"}'),
    ROW('sleepingbag*5', '{"es": "Saco de dormir Gris", "pt-br": "Saco de Dormir Grafite"}', '{"es": "Duerme bajo las estrellas", "pt-br": "Conforto para viajantes"}'),
    ROW('sleepingbag*6', '{"es": "Saco de dormir Púrpura", "pt-br": "Saco de Dormir Roxo"}', '{"es": "Duerme bajo las estrellas", "pt-br": "Conforto para viajantes"}'),
    ROW('sleepingbag*7', '{"es": "Saco de dormir Naranja", "pt-br": "Saco de Dormir Laranja"}', '{"es": "Duerme bajo las estrellas", "pt-br": "Conforto para viajantes"}'),
    ROW('sleepingbag*8', '{"es": "Saco de dormir Amarillo", "pt-br": "Saco de Dormir Amarelo"}', '{"es": "Duerme bajo las estrellas", "pt-br": "Conforto para viajantes"}'),
    ROW('sleepingbag*9', '{"es": "Saco de dormir Azul", "pt-br": "Saco de Dormir Azul"}', '{"es": "Duerme bajo las estrellas", "pt-br": "Conforto para viajantes"}'),
    ROW('small_chair_armas', '{"es": "Taburete", "pt-br": "Banqueta"}', '{"es": "Estilo nórdico", "pt-br": "A melhor forma do charme rústico"}'),
    ROW('small_table_armas', '{"es": "Mesa", "pt-br": "Mesa de Ocasião"}', '{"es": "¡De lo más resistente!", "pt-br": "Prática e bela"}'),
    ROW('smooth_table_polyfon', '{"es": "Mesa Buena Cena", "pt-br": "Mesa de Jantar Grande"}', '{"es": "Para las grandes ocasiones", "pt-br": "Para encontros maiores"}'),
    ROW('sob_carpet', '{"es": "Alfombra Streets of Bobba", "pt-br": "Carpete Caveira"}', '{"es": "¡Cuidado no te quemes!", "pt-br": "Para os amantes do rock!"}'),
    ROW('sofa_polyfon', '{"es": "Sofá Mode Doble", "pt-br": "Sofá de dois lugares"}', '{"es": "Comparte tu vida", "pt-br": "Conforto para casais com estilo"}'),
    ROW('sofa_polyfon*2', '{"es": "Sofá Mode Negro", "pt-br": "Sofá Mode Preto"}', '{"es": "Descansa en negro", "pt-br": "Perfeito para receber os amigos"}'),
    ROW('sofa_polyfon*3', '{"es": "Sofá Mode Blanco", "pt-br": "Sofá Branco"}', '{"es": "Descansa en blanco", "pt-br": "Sofá Branco"}'),
    ROW('sofa_polyfon*4', '{"es": "Sofá Mode Beige", "pt-br": "Sofá Bege"}', '{"es": "Descansa en beige", "pt-br": "Sofá Bege"}'),
    ROW('sofa_polyfon*6', '{"es": "Sofá Mode Azul", "pt-br": "Azul Mode Sofá"}', '{"es": "Descansa en azul", "pt-br": "Azul Mode Sofá"}'),
    ROW('sofa_polyfon*7', '{"es": "Sofá Mode Verde", "pt-br": "Verde Mode Sofá"}', '{"es": "Descansa en verde", "pt-br": "Verde Mode Sofá"}'),
    ROW('sofa_polyfon*8', '{"es": "Sofá Mode Amarillo", "pt-br": "Amarelo Mode Sofá"}', '{"es": "Descansa en amarillo", "pt-br": "Amarelo Mode Sofá"}'),
    ROW('sofa_polyfon*9', '{"es": "Sofá Mode Rojo", "pt-br": "Sofá Vermelho"}', '{"es": "Descansa en rojo", "pt-br": "Coleção Mode Natal 2007"}'),
    ROW('sofa_polyfon_girl', '{"es": "Sofa doble", "pt-br": "Sofá de dois lugares"}', '{"es": "Muy romántico", "pt-br": "Cor-de-rosa romântico para dois"}'),
    ROW('sofa_silo', '{"es": "Sofá Área Doble", "pt-br": "Sofá de Dois Lugares"}', '{"es": "Un ''must'' del Hotel", "pt-br": "Conforto moderno e acolchoado"}'),
    ROW('sofa_silo*2', '{"es": "Sofá Area Negro", "pt-br": "Preto Sofá Area"}', '{"es": "Sueños de colores", "pt-br": "Preto Sofá Area"}'),
    ROW('sofa_silo*3', '{"es": "Sofá Area Blanco", "pt-br": "Sofá Area Branco"}', '{"es": "Sueños de colores", "pt-br": "Perfeito para hospitais"}'),
    ROW('sofa_silo*4', '{"es": "Sofá Area Beige", "pt-br": "Bege Sofá Area"}', '{"es": "Sueños de colores", "pt-br": "Bege Sofá Area"}'),
    ROW('sofa_silo*5', '{"es": "Sofá Area Rosa", "pt-br": "Sofá Area Rosa"}', '{"es": "Sueños de colores", "pt-br": "Sofá Area Rosa"}'),
    ROW('sofa_silo*6', '{"es": "Sofá Area Azul", "pt-br": "Azul Sofá Area"}', '{"es": "Sueños de colores", "pt-br": "Azul Sofá Area"}'),
    ROW('sofa_silo*7', '{"es": "Sofá Area Verde", "pt-br": "Verde Sofá Area"}', '{"es": "Sueños de colores", "pt-br": "Verde Sofá Area"}'),
    ROW('sofa_silo*8', '{"es": "Sofá Area Amarillo", "pt-br": "Amarelo Sofá Area"}', '{"es": "Sueños de colores", "pt-br": "Amarelo Sofá Area"}'),
    ROW('sofa_silo*9', '{"es": "Sofá Area Rojo", "pt-br": "Vermelho Sofá Area"}', '{"es": "Sueños de colores", "pt-br": "Vermelho Sofá Area"}'),
    ROW('sofachair_polyfon', '{"es": "Sillón", "pt-br": "Poltrona"}', '{"es": "Estilo ''loft''", "pt-br": "Conforto em estilo moderno"}'),
    ROW('sofachair_polyfon*2', '{"es": "Butaca Mode Negra", "pt-br": "Poltrona Preta"}', '{"es": "Una plaza, un paraíso", "pt-br": "Confortável e elegante"}'),
    ROW('sofachair_polyfon*3', '{"es": "Butaca Mode Blanca", "pt-br": "Poltrona Mode Branca"}', '{"es": "Una plaza, un paraíso", "pt-br": "Tire os pés daí, agora!"}'),
    ROW('sofachair_polyfon*4', '{"es": "Butaca Mode Beige", "pt-br": "Poltrona Bege"}', '{"es": "Una plaza, un paraíso", "pt-br": "Poltrona Mode Bege"}'),
    ROW('sofachair_polyfon*6', '{"es": "Butaca Mode Azul", "pt-br": "Poltrona Mode Azul"}', '{"es": "Una plaza, un paraíso", "pt-br": "Poltrona Mode Azul"}'),
    ROW('sofachair_polyfon*7', '{"es": "Butaca Mode Verde", "pt-br": "Verde Poltrona Mode"}', '{"es": "Una plaza, un paraíso", "pt-br": "Verde Poltrona Mode"}'),
    ROW('sofachair_polyfon*8', '{"es": "Butaca Mode Amarilla", "pt-br": "Amarelo Poltrona Mode"}', '{"es": "Una plaza, un paraíso", "pt-br": "Amarelo Poltrona Mode"}'),
    ROW('sofachair_polyfon*9', '{"es": "Butaca Mode Roja", "pt-br": "Poltrona Vermelho"}', '{"es": "Una plaza, un paraíso", "pt-br": "Coleção Mode Natal 2007"}'),
    ROW('sofachair_polyfon_girl', '{"es": "Butaca Mode Rosa", "pt-br": "Poltrona"}', '{"es": "Hay que relajarse", "pt-br": "Pense rosa"}'),
    ROW('sofachair_silo', '{"es": "Sillón", "pt-br": "Poltrona"}', '{"es": "Confortable...", "pt-br": "Grande, mas vale à pena"}'),
    ROW('sofachair_silo*2', '{"es": "Butaca Area Negra", "pt-br": "Preto Poltrona Area"}', '{"es": "Un Habbo, una plaza", "pt-br": "Preto Poltrona Area"}'),
    ROW('sofachair_silo*3', '{"es": "Butaca Area Blanca", "pt-br": "Poltrona Area Branca"}', '{"es": "Un Habbo, una plaza", "pt-br": "Relaxe e descanse, o médico já vem"}'),
    ROW('sofachair_silo*4', '{"es": "Butaca Area Beige", "pt-br": "Bege Poltrona Area"}', '{"es": "Un Habbo, una plaza", "pt-br": "Bege Poltrona Area"}'),
    ROW('sofachair_silo*5', '{"es": "Butaca Area Rosa", "pt-br": "Poltrona Area Rosa"}', '{"es": "Un Habbo, una plaza", "pt-br": "Poltrona Area Rosa"}'),
    ROW('sofachair_silo*6', '{"es": "Butaca Area Azul", "pt-br": "Azul Poltrona Area"}', '{"es": "Un Habbo, una plaza", "pt-br": "Azul Poltrona Area"}'),
    ROW('sofachair_silo*7', '{"es": "Butaca Area Verde", "pt-br": "Verde Poltrona Area"}', '{"es": "Un Habbo, una plaza", "pt-br": "Verde Poltrona Area"}'),
    ROW('sofachair_silo*8', '{"es": "Sillón Área Amarillo", "pt-br": "Amarelo Poltrona Area"}', '{"es": "Un Habbo, una plaza", "pt-br": "Amarelo Poltrona Area"}'),
    ROW('sofachair_silo*9', '{"es": "Butaca Area Roja", "pt-br": "Vermelho Poltrona Area"}', '{"es": "Un Habbo, una plaza", "pt-br": "Vermelho Poltrona Area"}'),
    ROW('soft_jaggara_norja', '{"es": "Taburete", "pt-br": "Sofá-cadeira Fashion"}', '{"es": "Tu trono de cada día", "pt-br": "Sofá-cadeira macio e fashion"}'),
    ROW('soft_sofa_norja', '{"es": "Sofá", "pt-br": "Sofá Iced Bege"}', '{"es": "A descansar", "pt-br": "Beeem *macio*"}'),
    ROW('soft_sofa_norja*2', '{"es": "Sofá Iced Negro", "pt-br": "Sofá Iced Preto"}', '{"es": "Siéntate en pareja", "pt-br": "Preto Sofá Iced"}'),
    ROW('soft_sofa_norja*3', '{"es": "Sofá Iced Blanco", "pt-br": "Sofá Iced Branco"}', '{"es": "Siéntate en pareja", "pt-br": "Branco Sofá Iced"}'),
    ROW('soft_sofa_norja*4', '{"es": "Sofá Iced Marino", "pt-br": "Sofá Iced Água-marinha"}', '{"es": "Siéntate en pareja", "pt-br": "Aquamarine Sofá Iced"}'),
    ROW('soft_sofa_norja*5', '{"es": "Sofá Iced Rosa", "pt-br": "Sofá Iced Rosa"}', '{"es": "Siéntate en pareja", "pt-br": "Rosa Sofá Iced"}'),
    ROW('soft_sofa_norja*6', '{"es": "Sofá Iced Azul", "pt-br": "Sofá Iced Azul"}', '{"es": "Siéntate en pareja", "pt-br": "Azul Sofá Iced"}'),
    ROW('soft_sofa_norja*7', '{"es": "Sofá Iced Verde", "pt-br": "Sofá Iced Verde"}', '{"es": "Siéntate en pareja", "pt-br": "Verde Sofá Iced"}'),
    ROW('soft_sofa_norja*8', '{"es": "Sofá Iced Amarillo", "pt-br": "Sofá Iced Amarelo"}', '{"es": "Siéntate en pareja", "pt-br": "Amarelo Sofá Iced"}'),
    ROW('soft_sofa_norja*9', '{"es": "Sofá Iced Rojo", "pt-br": "Sofá Iced Vermelho"}', '{"es": "Siéntate en pareja", "pt-br": "Coleção Iced Natal 2007 *macio*"}'),
    ROW('soft_sofachair_norja', '{"es": "Sofá", "pt-br": "Poltrona Iced Bege"}', '{"es": "Diseñado para el descanso", "pt-br": "Fique *macio*"}'),
    ROW('soft_sofachair_norja*2', '{"es": "Butaca Iced Negra", "pt-br": "Poltrona Iced Preta"}', '{"es": "La más acolchada", "pt-br": "Preto Poltrona Iced"}'),
    ROW('soft_sofachair_norja*3', '{"es": "Butaca Iced Blanca", "pt-br": "Poltrona Iced Branca"}', '{"es": "La más acolchada", "pt-br": "Branco Poltrona Iced"}'),
    ROW('soft_sofachair_norja*4', '{"es": "Butaca Iced Marina", "pt-br": "Poltrona Iced Água-marinha"}', '{"es": "La más acolchada", "pt-br": "Marinho Poltrona Iced"}'),
    ROW('soft_sofachair_norja*5', '{"es": "Butaca Iced Rosa", "pt-br": "Poltrona Iced Rosa"}', '{"es": "La más acolchada", "pt-br": "Rosa Poltrona Iced"}'),
    ROW('soft_sofachair_norja*6', '{"es": "Butaca Iced Azul", "pt-br": "Poltrona Iced Azul"}', '{"es": "La más acolchada", "pt-br": "Azul Poltrona Iced"}'),
    ROW('soft_sofachair_norja*7', '{"es": "Butaca Iced Verde", "pt-br": "Poltrona Iced Verde"}', '{"es": "La más acolchada", "pt-br": "Verde Poltrona Iced"}'),
    ROW('soft_sofachair_norja*8', '{"es": "Butaca Iced Amarilla", "pt-br": "Poltrona Iced Amarela"}', '{"es": "La más acolchada", "pt-br": "Amarelo Poltrona Iced"}'),
    ROW('soft_sofachair_norja*9', '{"es": "Butaca Iced Roja", "pt-br": "Poltrona Iced Vermelha"}', '{"es": "La más acolchada", "pt-br": "Coleção Iced Natal 2007"}'),
    ROW('solarium_norja', '{"es": "Solarium Blanco", "pt-br": "Solário Branco"}', '{"es": "¡Broncea tus píxeles!", "pt-br": "É artificial. Cuide, pra não envelhecer a pele."}'),
    ROW('solarium_norja*1', '{"es": "Solarium Negro", "pt-br": "Preto Solário"}', '{"es": "¡Broncea tus píxeles!", "pt-br": "É artificial. Cuide, pra não envelhescer a pele."}'),
    ROW('solarium_norja*2', '{"es": "Solarium Beige", "pt-br": "Solário Iced"}', '{"es": "¡Broncea tus píxeles!", "pt-br": "É artificial. Cuide, pra não envelhescer a pele."}'),
    ROW('solarium_norja*3', '{"es": "Solarium Marino", "pt-br": "Solário Marinho"}', '{"es": "¡Broncea tus píxeles!", "pt-br": "É artificial. Cuide, pra não envelhescer a pele."}'),
    ROW('solarium_norja*5', '{"es": "Solarium Rosa", "pt-br": "Rosa Solário"}', '{"es": "¡Broncea tus píxeles!", "pt-br": "É artificial. Cuide, pra não envelhescer a pele."}'),
    ROW('solarium_norja*6', '{"es": "Solarium Azul", "pt-br": "Azul Solário"}', '{"es": "¡Broncea tus píxeles!", "pt-br": "É artificial. Cuide, pra não envelhescer a pele."}'),
    ROW('solarium_norja*7', '{"es": "Solarium Verde", "pt-br": "Verde Solário"}', '{"es": "¡Broncea tus píxeles!", "pt-br": "É artificial. Cuide, pra não envelhescer a pele."}'),
    ROW('solarium_norja*8', '{"es": "Solarium Amarillo", "pt-br": "Amarelo Solário"}', '{"es": "¡Broncea tus píxeles!", "pt-br": "É artificial. Cuide, pra não envelhescer a pele."}'),
    ROW('solarium_norja*9', '{"es": "Solarium Rojo", "pt-br": "Solário Vermelho"}', '{"es": "¡Broncea tus píxeles!", "pt-br": "Coleção Iced Natal 2007"}'),
    ROW('song_disk', '{"es": "TraxDisco", "pt-br": "Epic Flail"}', '{"es": "¡Menuda grabación!", "pt-br": "By Habbocalyptica"}'),
    ROW('sound_machine', '{"es": "Traxmachine", "pt-br": "Traxmachine"}', '{"es": "Crea los sonidos más divertidos", "pt-br": "É balada no Habbo"}'),
    ROW('sound_machine*1', '{"es": "Traxmachine Gris", "pt-br": "Traxmachine"}', '{"es": "Música, maestro", "pt-br": "Faz a música e ouve em seu Quarto"}'),
    ROW('sound_machine*2', '{"es": "Traxmachine Océano", "pt-br": "Trax Aquamarine"}', '{"es": "Suena a... ¡Habbo!", "pt-br": "Cor vibrante faz toda a diferença"}'),
    ROW('sound_machine*3', '{"es": "Traxmachine Verde", "pt-br": "Verde Traxmachine"}', '{"es": "¡Crea la mejor música!", "pt-br": "O som do Habbo"}'),
    ROW('sound_machine*4', '{"es": "Traxmachine Azul", "pt-br": "Azul Traxmachine"}', '{"es": "Suena a... ¡Habbo!", "pt-br": "O som do Habbo"}'),
    ROW('sound_machine*5', '{"es": "Traxmachine Marrón", "pt-br": "Marrom Traxmachine"}', '{"es": "Sube el volumen", "pt-br": "O som do Habbo"}'),
    ROW('sound_machine*6', '{"es": "Traxmachine Púrpura", "pt-br": "Trax Monsters Of Habbo"}', '{"es": "Suena a... ¡Habbo!", "pt-br": "Metal não é pizza!"}'),
    ROW('sound_machine*7', '{"es": "Traxmachine Roja", "pt-br": "Vermelho Traxmachine"}', '{"es": "Suena a... ¡Habbo!", "pt-br": "O som do Habbo"}'),
    ROW('sound_machine_pro', '{"es": "Sound Machine Pro", "pt-br": "Sound Machine Pro"}', '{"es": "creating fancy sounds", "pt-br": "creating fancy sounds"}'),
    ROW('sound_set_1', '{"es": "Habbo Sounds 1", "pt-br": "Duck Funk - DJ Fuse"}', '{"es": "¿Te gusta el rock?", "pt-br": "Rock"}'),
    ROW('sound_set_10', '{"es": "Hip Hop Beats 1", "pt-br": "Boy Band"}', '{"es": "Los Hit más modernos", "pt-br": "Super popular."}'),
    ROW('sound_set_11', '{"es": "Dance 4", "pt-br": "Spicey Donna"}', '{"es": "D.I.S.C.O", "pt-br": "Música dance e disco."}'),
    ROW('sound_set_12', '{"es": "Habbo Sounds 2", "pt-br": "Abe Normal"}', '{"es": "¡Sólo para mentes abiertas!", "pt-br": "Sonso aleatórios."}'),
    ROW('sound_set_13', '{"es": "Dance 5", "pt-br": "Café Muzzakh"}', '{"es": "Suave", "pt-br": "Música de elevador."}'),
    ROW('sound_set_14', '{"es": "Hip Hop Beats 2", "pt-br": "Ex da Cameron"}', '{"es": "¡En la onda!", "pt-br": "Batidas loucas."}'),
    ROW('sound_set_15', '{"es": "RnB Grooves 6", "pt-br": "El Generico"}', '{"es": "R&B a tope", "pt-br": "R ''n'' B para massas"}'),
    ROW('sound_set_16', '{"es": "Hip Hop Beats 3", "pt-br": "Ferry Nultado"}', '{"es": "¡Melodias chic!", "pt-br": "Melodia chique."}'),
    ROW('sound_set_17', '{"es": "Groove 3", "pt-br": "Jive Sideburns"}', '{"es": "¡Lo más funky!", "pt-br": "Jive Sideburns"}'),
    ROW('sound_set_18', '{"es": "Groove 4", "pt-br": "Cartucho Samba"}', '{"es": "El suave ritmo brasileño", "pt-br": "Batidas de samba."}'),
    ROW('sound_set_19', '{"es": "Hip Hop Beats 4", "pt-br": "MnM"}', '{"es": "¡Y más funky!", "pt-br": "Batidas com suíngue"}'),
    ROW('sound_set_2', '{"es": "Habbo Sounds 3", "pt-br": "Habbo - DJ Fuse"}', '{"es": "Las clásicas Habbo Melodías", "pt-br": "Melodias Clássicas Habbo"}'),
    ROW('sound_set_20', '{"es": "SFX 2", "pt-br": "Monkey Paradise"}', '{"es": "Sonidos de la jungla más profunda", "pt-br": "Monkey Paradise"}'),
    ROW('sound_set_21', '{"es": "Rock 1", "pt-br": "Snotty Day"}', '{"es": "Al ritmo más brutal", "pt-br": "Snotty Day"}'),
    ROW('sound_set_22', '{"es": "SFX 3", "pt-br": "A Day In Park"}', '{"es": "La naturaleza en estado puro", "pt-br": "A Day In Park"}'),
    ROW('sound_set_23', '{"es": "SFX 4", "pt-br": "Nature Nightlife"}', '{"es": "¡Una de búhos y grillos!", "pt-br": "Nature Nightlife"}'),
    ROW('sound_set_24', '{"es": "Habbo Sounds 4", "pt-br": "Compu"}', '{"es": "Efectos especiales", "pt-br": "Efeitos de computador e digitais."}'),
    ROW('sound_set_25', '{"es": "Dance 1", "pt-br": "Party Pack"}', '{"es": "Sonidos fiesteros", "pt-br": "Party pack"}'),
    ROW('sound_set_26', '{"es": "Groove 1", "pt-br": "Bhangra Mangra"}', '{"es": "Un toque hindú", "pt-br": "Ritmos Indianos."}'),
    ROW('sound_set_27', '{"es": "Groove 2", "pt-br": "Rasta Santa Mix"}', '{"es": "Para los amantes del reggae", "pt-br": "Rastaaaaaaaa"}'),
    ROW('sound_set_28', '{"es": "Rock 2", "pt-br": "Metal"}', '{"es": "¡Demasiado para tus oídos!", "pt-br": "Pesado e sujo"}'),
    ROW('sound_set_29', '{"es": "Dance 2", "pt-br": "Festa no Apê"}', '{"es": "El mejor House electrónico", "pt-br": "Pode aparecer"}'),
    ROW('sound_set_3', '{"es": "Electronic 1", "pt-br": "BattleBall"}', '{"es": "¡Los sonidos más alocados!", "pt-br": "Loucura 8bit"}'),
    ROW('sound_set_30', '{"es": "Instrumental 1", "pt-br": "Cartucho Melódico"}', '{"es": "Para momentos románticos", "pt-br": "Estão tocando a nossa canção..."}'),
    ROW('sound_set_31', '{"es": "Dance 3", "pt-br": "DJ Balada"}', '{"es": "House sin parar", "pt-br": "Solte suas feras"}'),
    ROW('sound_set_32', '{"es": "Instrumental 2", "pt-br": "Pianinho"}', '{"es": "Tu concierto de piano", "pt-br": "Para dedilhar"}'),
    ROW('sound_set_33', '{"es": "Rock 3", "pt-br": "Yngvie Van Halen"}', '{"es": "Guitarreando", "pt-br": "Yngvie Van Halen"}'),
    ROW('sound_set_34', '{"es": "Rock 5", "pt-br": "Rockin'' riffs"}', '{"es": "Sigue a tus ídolos a la guitarra", "pt-br": "Rockin'' riffs"}'),
    ROW('sound_set_35', '{"es": "Dance 6", "pt-br": "Festa Funk"}', '{"es": "Deliciosamente funk", "pt-br": "Cada um no seu quadrado"}'),
    ROW('sound_set_36', '{"es": "Latin 3", "pt-br": "Bom Balanço"}', '{"es": "Para mentes adultas", "pt-br": "O som dos Grandes Conjuntos"}'),
    ROW('sound_set_37', '{"es": "Habbowood", "pt-br": "Música de Cinema"}', '{"es": "Una buena BSO", "pt-br": "Para ouvir comendo pipoca"}'),
    ROW('sound_set_38', '{"es": "Rock 6", "pt-br": "Highway to Habbo"}', '{"es": "Sonidos con mucho peso", "pt-br": "Highway to Habbo"}'),
    ROW('sound_set_39', '{"es": "Rock 7", "pt-br": "Iron Maid"}', '{"es": "¡A gritar!", "pt-br": "Iron Maid"}'),
    ROW('sound_set_4', '{"es": "Ambient 1", "pt-br": "Aventuras dos Pôr do Sol"}', '{"es": "Para los momentos de chill-out", "pt-br": "Relaxando"}'),
    ROW('sound_set_40', '{"es": "Rock 4", "pt-br": "Pixels On The Water"}', '{"es": "De vuelta al rock más puro", "pt-br": "Pixels On The Water"}'),
    ROW('sound_set_41', '{"es": "Rock 8", "pt-br": "Sympathy for the coder"}', '{"es": "Rizando el rizo", "pt-br": "Sympathy for the coder"}'),
    ROW('sound_set_42', '{"es": "Snouthill Horror", "pt-br": "Snouthill Horror"}', '{"es": "Sonido Habboween", "pt-br": "Snouthill Horror"}'),
    ROW('sound_set_43', '{"es": "SFX 1", "pt-br": "Silence of the Moderators"}', '{"es": "¿Miedo? ¡Muajajaja!", "pt-br": "Silence of the Moderators"}'),
    ROW('sound_set_44', '{"es": "Ghost Story", "pt-br": "Ghost Story"}', '{"es": "Para Salas oscuras", "pt-br": "Ghost Story"}'),
    ROW('sound_set_45', '{"es": "Techno 1", "pt-br": "Conexão Berlim"}', '{"es": "¡Los sonidos más contundentes!", "pt-br": "Seleção musical Santa3000"}'),
    ROW('sound_set_46', '{"es": "Club 1", "pt-br": "Sound set 46"}', '{"es": "Hablando de House...", "pt-br": "De bada bada bo!"}'),
    ROW('sound_set_47', '{"es": "Club 2", "pt-br": "Sound set 47"}', '{"es": "Sonidos de Estocolmo", "pt-br": "Storm the UKCharts!"}'),
    ROW('sound_set_48', '{"es": "Club 3", "pt-br": "Sound set 48"}', '{"es": "Sonidos de la vieja escuela", "pt-br": "Sweet party beat"}'),
    ROW('sound_set_49', '{"es": "Club 4", "pt-br": "Sound set 49"}', '{"es": "Verano de 2007", "pt-br": "You will belong"}'),
    ROW('sound_set_5', '{"es": "Ambient 4", "pt-br": "Céu Nublado"}', '{"es": "El lado oscuro de Habbo", "pt-br": "Alma em transe"}'),
    ROW('sound_set_50', '{"es": "Club 5", "pt-br": "Sound set 50"}', '{"es": "¡Dale caña!", "pt-br": "The harder generation"}'),
    ROW('sound_set_51', '{"es": "Club 6", "pt-br": "Sound set 51"}', '{"es": "¡Dale caña!", "pt-br": "Bop to the beat"}'),
    ROW('sound_set_52', '{"es": "Club 7", "pt-br": "Trancehouse"}', '{"es": "¡Dale caña!", "pt-br": "Seleção musical do Santa 3000"}'),
    ROW('sound_set_53', '{"es": "Far West 2", "pt-br": "Habbo Sagrado"}', '{"es": "Ahhh, un viaje musical hasta el Lejano Oriente", "pt-br": "Seleção de Natal 2007"}'),
    ROW('sound_set_54', '{"es": "Sound set 54", "pt-br": "Loops da Rena"}', '{"es": "Tune into Christmas", "pt-br": "Seleção musical Natal 2007"}'),
    ROW('sound_set_55', '{"es": "R&B Grooves 1", "pt-br": "Sound set 55"}', '{"es": "¡Estilo Nelly!", "pt-br": "Can you fill me in?"}'),
    ROW('sound_set_56', '{"es": "R&B Grooves 2", "pt-br": "Sound set 56"}', '{"es": "¡Sonidos deliciosos!", "pt-br": "Get down tonight!"}'),
    ROW('sound_set_57', '{"es": "R&B Grooves 3", "pt-br": "Sound set 57"}', '{"es": "¡Sonidos Black!", "pt-br": "Feel the groove"}'),
    ROW('sound_set_58', '{"es": "R&B Grooves 4", "pt-br": "Sound set 58"}', '{"es": "¡Menudo ritmo!", "pt-br": "Sh-shake it!"}'),
    ROW('sound_set_59', '{"es": "Sound set 59", "pt-br": "Sound set 59"}', '{"es": "Urban break beats", "pt-br": "Urban break beats"}'),
    ROW('sound_set_6', '{"es": "Ambient 3", "pt-br": "Ambiências"}', '{"es": "El mejor sonido ambiente", "pt-br": "Som ambiente"}'),
    ROW('sound_set_60', '{"es": "Latin 1", "pt-br": "Cartucho Amor Latino"}', '{"es": "Hot and romantic", "pt-br": "**Estoy enamorado**"}'),
    ROW('sound_set_61', '{"es": "Latin 2", "pt-br": "Cartucho Habborella"}', '{"es": "BSO del Crucero del Amor", "pt-br": "Perfeito para jantares à luz de velas"}'),
    ROW('sound_set_62', '{"es": "Alhambra 1", "pt-br": "O som do Gênio"}', '{"es": "¡Ecos del Lejano Oriente!", "pt-br": "O cara tem bom gosto"}'),
    ROW('sound_set_63', '{"es": "Alhambra 2", "pt-br": "1001 Noites"}', '{"es": "¡Ecos del Lejano Oriente!", "pt-br": "Mexe a cintura"}'),
    ROW('sound_set_64', '{"es": "Alhambra 3", "pt-br": "Batidas Marroquinas"}', '{"es": "¡Ecos del Lejano Oriente!", "pt-br": "Ninguém fica parado"}'),
    ROW('sound_set_65', '{"es": "Tiki Sound 1", "pt-br": "Sound set 65"}', '{"es": "¡Sonido natural!"}'),
    ROW('sound_set_66', '{"es": "Tiki Sound 2", "pt-br": "Sound set 66"}', '{"es": "¡Sonido natural!"}'),
    ROW('sound_set_67', '{"es": "Tiki Sound 3", "pt-br": "Sound set 67"}', '{"es": "¡Sonido natural!"}'),
    ROW('sound_set_68', '{"es": "Sport 1", "pt-br": "Sound set 68"}', '{"es": "¡Emoción en la grada!"}'),
    ROW('sound_set_69', '{"es": "Sport 2", "pt-br": "Sound set 69"}', '{"es": "¡Emoción en la grada!"}'),
    ROW('sound_set_7', '{"es": "SFX 5", "pt-br": "Som dos Mobis"}', '{"es": "¡Viva los efectos especiales!", "pt-br": "Efeitos Mobísticos"}'),
    ROW('sound_set_70', '{"es": "Sport 3", "pt-br": "Sound set 70"}', '{"es": "¡Emoción en la grada!"}'),
    ROW('sound_set_71', '{"es": "Ártico", "pt-br": "Cartucho Natal 2008"}', '{"es": "¡El sonido más refrescante!", "pt-br": "Bate o sino pequenino..."}'),
    ROW('sound_set_72', '{"es": "Campo", "pt-br": "Sons da roça"}', '{"es": "The working farm", "pt-br": "Para ouvir num momento de paz"}'),
    ROW('sound_set_8', '{"es": "Ambient 2", "pt-br": "Electrônico"}', '{"es": "Pura potencia", "pt-br": "Grooves eletrônicos"}'),
    ROW('sound_set_9', '{"es": "Electronic 2", "pt-br": "Místico"}', '{"es": "Sólo para atrevidos", "pt-br": "Brumas místicas"}'),
    ROW('sporttrack1*1', '{"es": "Pista recta tartán", "pt-br": "Pista de saibro"}', '{"es": "¡Válida para cualquier deporte!", "pt-br": "Para todos os esportes!"}'),
    ROW('sporttrack1*2', '{"es": "Pista recta cemento", "pt-br": "Pista de asfalto"}', '{"es": "¡Válida para cualquier deporte!", "pt-br": "Para todos os esportes!"}'),
    ROW('sporttrack1*3', '{"es": "Pista recta hierba", "pt-br": "Pista de grama"}', '{"es": "¡Válida para cualquier deporte!", "pt-br": "Para todos os esportes!"}'),
    ROW('sporttrack2*1', '{"es": "Córner tartán", "pt-br": "Canto de pista saibro"}', '{"es": "¡Válido para cualquier deporte!", "pt-br": "Para todos os esportes!"}'),
    ROW('sporttrack2*2', '{"es": "Córner cemento", "pt-br": "Canto de pista asfalto"}', '{"es": "¡Válido para cualquier deporte!", "pt-br": "Para todos os esportes!"}'),
    ROW('sporttrack2*3', '{"es": "Córner hierba", "pt-br": "Canto de pista grama"}', '{"es": "¡Válido para cualquier deporte!", "pt-br": "Para todos os esportes!"}'),
    ROW('sporttrack3*1', '{"es": "Meta tartán", "pt-br": "Linha chegada saibro"}', '{"es": "¡Válida para cualquier deporte!", "pt-br": "Para todos os esportes!"}'),
    ROW('sporttrack3*2', '{"es": "Meta cemento", "pt-br": "Linha chegada asfalto"}', '{"es": "¡Válida para cualquier deporte!", "pt-br": "Para todos os esportes!"}'),
    ROW('sporttrack3*3', '{"es": "Meta hierba", "pt-br": "Linha chegada grama"}', '{"es": "¡Válida para cualquier deporte!", "pt-br": "Para todos os esportes!"}'),
    ROW('spotlight', '{"es": "Foco", "pt-br": "Holofote"}', '{"es": "¡Que me achicharro!", "pt-br": "Luz das estrelas"}'),
    ROW('spyro', '{"es": "Huevo de Dragón", "pt-br": "Ovo do Grande Moa"}', '{"es": "¡Aléjate en cuanto se rompa el cascarón!", "pt-br": "O centro da lenda"}'),
    ROW('stand_polyfon_z', '{"es": "Z", "pt-br": "Prateleira"}', '{"es": "¡Zorprendente!", "pt-br": "Em ordem"}'),
    ROW('statue', '{"es": "Cupido, el dios del amor", "pt-br": "Estátua de Cupido"}', '{"es": "Directo al corazón", "pt-br": "Cuidado com as flechas!"}'),
    ROW('summer_chair*1', '{"es": "Tumbona Aguamarina", "pt-br": "Cadeira de Praia"}', '{"es": "¡Para ponerse moreno!", "pt-br": "Conforto ao sol na cor aquamarine"}'),
    ROW('summer_chair*2', '{"es": "Tumbona Rosa", "pt-br": "Cadeira de Praia"}', '{"es": "¡Para ponerse moreno!", "pt-br": "Férias cor rosa"}'),
    ROW('summer_chair*3', '{"es": "Tumbona Negra", "pt-br": "Cadeira de Praia"}', '{"es": "¡Para ponerse moreno!", "pt-br": "Conforto ao sol na cor preta"}'),
    ROW('summer_chair*4', '{"es": "Tumbona Blanca", "pt-br": "Cadeira de Praia"}', '{"es": "¡A ponerse moreno!", "pt-br": "Conforto ao sol na cor branca"}'),
    ROW('summer_chair*5', '{"es": "Tumbona Beige", "pt-br": "Cadeira de Praia"}', '{"es": "¡Para ponerse moreno!", "pt-br": "Conforto ao sol na cor bege"}'),
    ROW('summer_chair*6', '{"es": "Tumbona Azul", "pt-br": "Cadeira de Praia"}', '{"es": "¡Para ponerse moreno!", "pt-br": "Férias cor azul"}'),
    ROW('summer_chair*7', '{"es": "Tumbona Verde", "pt-br": "Cadeira de Praia"}', '{"es": "¡A ponerse moreno!", "pt-br": "Férias cor verde"}'),
    ROW('summer_chair*8', '{"es": "Tumbona Amarilla", "pt-br": "Cadeira de Praia"}', '{"es": "¡A ponerse moreno!", "pt-br": "Férias cor amarelo"}'),
    ROW('summer_chair*9', '{"es": "Tumbona Roja", "pt-br": "Cadeira de Praia"}', '{"es": "¡Para ponerse moreno!", "pt-br": "Férias cor vermelho"}'),
    ROW('summer_grill*1', '{"es": "Barbacoa Grill Azul", "pt-br": "Churrasqueira"}', '{"es": "La carne, en su punto", "pt-br": "Delícia cor azul"}'),
    ROW('summer_grill*2', '{"es": "Barbacoa Grill Roja", "pt-br": "Churrasqueira"}', '{"es": "La carne, en su punto", "pt-br": "Delícia cor vermelho"}'),
    ROW('summer_grill*3', '{"es": "Barbacoa Grill Verde", "pt-br": "Churrasqueira"}', '{"es": "La carne, en su punto", "pt-br": "Delícia cor verde"}'),
    ROW('summer_grill*4', '{"es": "Barbacoa Grill Gris", "pt-br": "Churrasqueira"}', '{"es": "La carne, en su punto", "pt-br": "Delícia cor amarelo"}'),
    ROW('summer_icebox', '{"es": "Mini Nevera", "pt-br": "Freezer Portátil"}', '{"es": "¡Qué fresquito!", "pt-br": "A parte mais refrescante das férias"}'),
    ROW('summer_pool*1', '{"es": "Piscina Azul", "pt-br": "Piscina"}', '{"es": "¡A refrescarse!", "pt-br": "Cor azul"}'),
    ROW('summer_pool*2', '{"es": "Piscina Roja", "pt-br": "Piscina"}', '{"es": "Sin tiburones a la vista", "pt-br": "Cor vermelho"}'),
    ROW('summer_pool*3', '{"es": "Piscina Verde", "pt-br": "Piscina"}', '{"es": "¡Qué buena idea!", "pt-br": "Cor verde"}'),
    ROW('summer_pool*4', '{"es": "Piscina Amarilla", "pt-br": "Piscina"}', '{"es": "Ehh, que esto no es el baño", "pt-br": "Cor amarelo"}'),
    ROW('summer_raft1', '{"es": "Flotador Rosa", "pt-br": "Bóia de Rafting"}', '{"es": "La Sorpresa del Verano 09", "pt-br": "Segura e firme e vai com Deus!"}'),
    ROW('summer_raft2', '{"es": "Flotador Azul", "pt-br": "Bóia de Rafting Azul"}', '{"es": "La Sorpresa del Verano 09", "pt-br": "Prefere viver com emoção?"}'),
    ROW('svnr_aus', '{"es": "Koala", "pt-br": "Coala Australiano"}', '{"es": "Desde Australia, una criatura peluda y achuchable", "pt-br": "Novembro, 5/6"}'),
    ROW('svnr_de', '{"es": "Gnomo", "pt-br": "Gnomo Alemão"}', '{"es": "Octubre 2008, 4/6", "pt-br": "Outubro, 4/6"}'),
    ROW('svnr_fi', '{"es": "Sauna Finlandesa", "pt-br": "Lareira Finlandesa"}', '{"es": "Diciembre 2008, 6/6", "pt-br": "Colecionável"}'),
    ROW('svnr_it', '{"es": "Góndola", "pt-br": "Gôndola Italiana"}', '{"es": "Para navegar por los canales de Habbo", "pt-br": "Julho, 1/6"}'),
    ROW('svnr_nl', '{"es": "Zueco", "pt-br": "Tamanco Holandês"}', '{"es": "Agosto 2008, 2/6", "pt-br": "Agosto, 2/6"}'),
    ROW('svnr_uk', '{"es": "Big Ben", "pt-br": "Big Ben"}', '{"es": "Ni un segundo de retraso", "pt-br": "Setembro, 3/6"}'),
    ROW('sw_chest', '{"es": "Baúl", "pt-br": "sw_chest"}', '{"es": "¡Qué de recuerdos!", "pt-br": "One size fits all"}'),
    ROW('sw_hole', '{"es": "Boquete", "pt-br": "sw_hole"}', '{"es": "¿Pero qué han hecho con la pared?", "pt-br": "sw_hole"}'),
    ROW('sw_raven', '{"es": "Cuervo", "pt-br": "raven"}', '{"es": "Dicen que doy mala suerte, muajajaja", "pt-br": "spiderwick furniture"}'),
    ROW('sw_stone', '{"es": "Collar de piedra", "pt-br": "sw_stone"}', '{"es": "Toda una obra de arte", "pt-br": "sw_stone"}'),
    ROW('sw_swords', '{"es": "Espadas", "pt-br": "Floretes"}', '{"es": "Como las que usaban en la Edad Media", "pt-br": "Z de Zorro!"}'),
    ROW('sw_table', '{"es": "Escritorio Antiguo", "pt-br": "sw_table"}', '{"es": "Sobre él se redactaron grandes manuscritos", "pt-br": "Where will you go today?"}'),
    ROW('table_armas', '{"es": "Mesa", "pt-br": "Mesa de Jantar"}', '{"es": "Para comer algo", "pt-br": "Para jantares informais"}'),
    ROW('table_norja_med', '{"es": "Mesita", "pt-br": "Mesa para Lanche"}', '{"es": "Para tomar un cafe", "pt-br": "Embutida com elegância"}'),
    ROW('table_norja_med*2', '{"es": "Sobremesa Negra", "pt-br": "Mesa de Café Grande Preto"}', '{"es": "Reuniones a montones", "pt-br": "Grandes Reuniões"}'),
    ROW('table_norja_med*3', '{"es": "Sobremesa Blanca", "pt-br": "Mesa de Café Iced Grande Branca"}', '{"es": "Reuniones a montones", "pt-br": "Grandes Reuniões"}'),
    ROW('table_norja_med*4', '{"es": "Sobremesa Marina", "pt-br": "Mesa de Café Grande Aquamarine"}', '{"es": "Reuniones a montones", "pt-br": "Grandes Reuniões"}'),
    ROW('table_norja_med*5', '{"es": "Sobremesa Rosa", "pt-br": "Mesa de Café Grande Rosa"}', '{"es": "Reuniones a montones", "pt-br": "Grandes Reuniões"}'),
    ROW('table_norja_med*6', '{"es": "Sobremesa Azul", "pt-br": "Mesa de Café Grande Azul"}', '{"es": "Reuniones a montones", "pt-br": "Grandes Reuniões"}'),
    ROW('table_norja_med*7', '{"es": "Sobremesa Verde", "pt-br": "Mesa de Café Grande Verde"}', '{"es": "Reuniones a montones", "pt-br": "Grandes Reuniões"}'),
    ROW('table_norja_med*8', '{"es": "Sobremesa Amarilla", "pt-br": "Mesa de Café Grande Amarelo"}', '{"es": "Reuniones a montones", "pt-br": "Grandes Reuniões"}'),
    ROW('table_norja_med*9', '{"es": "Sobremesa Roja", "pt-br": "Mesa de Café Grande Vermelho"}', '{"es": "Reuniones a montones", "pt-br": "Grandes Reuniões"}'),
    ROW('table_plasto_4leg', '{"es": "Mesa", "pt-br": "Mesa Quatro Pernas"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_4leg*1', '{"es": "Mesa", "pt-br": "Mesa Quatro Pernas"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_4leg*10', '{"es": "Mesa", "pt-br": "Mesa Quatro Pernas"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_4leg*14', '{"es": "Mesa HC", "pt-br": "HC mesa"}', '{"es": "Mesa Aqua", "pt-br": "Mesa aquática"}'),
    ROW('table_plasto_4leg*15', '{"es": "Mesa", "pt-br": "Mesa Quatro Pernas"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_4leg*16', '{"es": "Mesa", "pt-br": "Mesa Quatro Pernas"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_4leg*2', '{"es": "Mesa", "pt-br": "Mesa Quatro Pernas"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_4leg*3', '{"es": "Mesa", "pt-br": "Mesa Quatro Pernas"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_4leg*4', '{"es": "Mesa", "pt-br": "Mesa Quatro Pernas"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_4leg*5', '{"es": "Mesa", "pt-br": "Mesa Quatro Pernas"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_4leg*6', '{"es": "Mesa", "pt-br": "Mesa Quatro Pernas"}', '{"es": "Cool y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_4leg*7', '{"es": "Mesa", "pt-br": "Mesa Quatro Pernas"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_4leg*8', '{"es": "Mesa", "pt-br": "Mesa Quatro Pernas"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_4leg*9', '{"es": "Mesa", "pt-br": "Mesa Quatro Pernas"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_bigsquare', '{"es": "Mesa", "pt-br": "Mesa Quadrada Grande"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_bigsquare*1', '{"es": "Mesa", "pt-br": "Mesa Quadrada Grande"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_bigsquare*14', '{"es": "Mesa", "pt-br": "Mesa Quadrada Grande"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_bigsquare*15', '{"es": "Mesa", "pt-br": "Mesa Quadrada Grande"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_bigsquare*2', '{"es": "Mesa", "pt-br": "Mesa Quadrada Grande"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_bigsquare*3', '{"es": "Mesa", "pt-br": "Mesa Quadrada Grande"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_bigsquare*4', '{"es": "Mesa de comedor", "pt-br": "Mesa Quadrada Grande"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_bigsquare*5', '{"es": "Mesa", "pt-br": "Mesa Quadrada Grande"}', '{"es": "Muy ''Pop''", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_bigsquare*6', '{"es": "Mesa", "pt-br": "Mesa Quadrada Grande"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_bigsquare*7', '{"es": "Mesa", "pt-br": "Mesa Quadrada Grande"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_bigsquare*8', '{"es": "Mesa", "pt-br": "Mesa Quadrada Grande"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_bigsquare*9', '{"es": "Mesa", "pt-br": "Mesa Quadrada Grande"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_round', '{"es": "Mesa", "pt-br": "Mesa Redonda"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_round*1', '{"es": "Mesa", "pt-br": "Mesa Redonda"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_round*14', '{"es": "Mesa", "pt-br": "Mesa Redonda"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_round*15', '{"es": "Mesa", "pt-br": "Mesa Redonda"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_round*2', '{"es": "Mesa", "pt-br": "Mesa Redonda"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_round*3', '{"es": "Mesa", "pt-br": "Mesa Redonda"}', '{"es": "Guay y de plástica", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_round*4', '{"es": "Mesa", "pt-br": "Mesa Redonda"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_round*5', '{"es": "Mesa", "pt-br": "Mesa Redonda"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_round*6', '{"es": "Mesa", "pt-br": "Mesa Redonda"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_round*7', '{"es": "Mesa", "pt-br": "Mesa Redonda"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_round*8', '{"es": "Mesa", "pt-br": "Mesa Redonda"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_round*9', '{"es": "Mesa", "pt-br": "Mesa Redonda"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_square', '{"es": "Mesa", "pt-br": "Mesa Quadrada"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_square*1', '{"es": "Mesa", "pt-br": "Mesa Quadrada"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_square*14', '{"es": "Mesa", "pt-br": "Mesa Quadrada"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_square*15', '{"es": "Mesa", "pt-br": "Mesa Quadrada"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_square*2', '{"es": "Mesa", "pt-br": "Mesa Quadrada"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_square*3', '{"es": "Mesa de comedor", "pt-br": "Mesa Quadrada"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_square*4', '{"es": "Mesa", "pt-br": "Mesa Quadrada"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_square*5', '{"es": "Mesa", "pt-br": "Mesa Quadrada"}', '{"es": "Cool y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_square*6', '{"es": "Mesa", "pt-br": "Mesa Quadrada"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_square*7', '{"es": "Mesa", "pt-br": "Mesa Quadrada"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_square*8', '{"es": "Mesa", "pt-br": "Mesa Quadrada"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_plasto_square*9', '{"es": "Mesa", "pt-br": "Mesa Quadrada"}', '{"es": "Guay y de plástico", "pt-br": "Moderna peça de plástico"}'),
    ROW('table_polyfon', '{"es": "Mesa", "pt-br": "Mesa Grande"}', '{"es": "Para los amigos", "pt-br": "Para encontros maiores"}'),
    ROW('table_polyfon_med', '{"es": "Mesa", "pt-br": "Mesa Grande para Lanche"}', '{"es": "Para los amigos", "pt-br": "Para encontros maiores"}'),
    ROW('table_polyfon_small', '{"es": "Mesita", "pt-br": "Mesa para Lanche"}', '{"es": "Para Habbos sofisticados", "pt-br": "Para servir lanches com estilo"}'),
    ROW('table_silo_med', '{"es": "Mesa", "pt-br": "Mesa para Lanche"}', '{"es": "Siempre necesaria", "pt-br": "Limpeza, harmonia e discrição"}'),
    ROW('table_silo_med*2', '{"es": "Mesa Area Negra", "pt-br": "Preto Mesa de Café Area"}', '{"es": "Café Negro", "pt-br": "Preto Mesa de Café Area"}'),
    ROW('table_silo_med*3', '{"es": "Mesa Area Blanca", "pt-br": "Mesa de Café Area Branca"}', '{"es": "Café Blanco", "pt-br": "Cházinho ou café?"}'),
    ROW('table_silo_med*4', '{"es": "Mesa Area Beige", "pt-br": "Bege Mesa de Café Area"}', '{"es": "Café Beige", "pt-br": "Bege Mesa de Café Area"}'),
    ROW('table_silo_med*5', '{"es": "Mesa Area Rosa", "pt-br": "Mesa de Café Rosa"}', '{"es": "Café Rosa", "pt-br": "Mesa de Café Area Rosa"}'),
    ROW('table_silo_med*6', '{"es": "Mesa Area Azul", "pt-br": "Azul Mesa de Café Area"}', '{"es": "Café Azul", "pt-br": "Azul Mesa de Café Area"}'),
    ROW('table_silo_med*7', '{"es": "Mesa Area Verde", "pt-br": "Verde Mesa de Café Area"}', '{"es": "Café Verde", "pt-br": "Verde Mesa de Café Area"}'),
    ROW('table_silo_med*8', '{"es": "Mesa Area Amarilla", "pt-br": "Amarelo Mesa de Café Area"}', '{"es": "Café Amarillo", "pt-br": "Amarelo Mesa de Café Area"}'),
    ROW('table_silo_med*9', '{"es": "Mesa Area Roja", "pt-br": "Vermelho Mesa de Café Area"}', '{"es": "Café Rojo", "pt-br": "Vermelho Mesa de Café Area"}'),
    ROW('table_silo_small', '{"es": "Mesita", "pt-br": "Mesa de Ocasião"}', '{"es": "Para tapear o jugar...", "pt-br": "Para os momentos casuais"}'),
    ROW('table_silo_small*2', '{"es": "Mesa Area de pie Negra", "pt-br": "Preto Mesa de Ocasião Area"}', '{"es": "Para meriendas rápidas", "pt-br": "Preto Mesa de Ocasião Area"}'),
    ROW('table_silo_small*3', '{"es": "Mesa Area de pie Blanca", "pt-br": "Mesa de Ocasião Branca"}', '{"es": "Para meriendas rápidas", "pt-br": "Receba convidados"}'),
    ROW('table_silo_small*4', '{"es": "Mesa Area de pie Beige", "pt-br": "Bege Mesa de Ocasião Area"}', '{"es": "Para meriendas rápidas", "pt-br": "Bege Mesa de Ocasião Area"}'),
    ROW('table_silo_small*5', '{"es": "Mesa Area de pie Rosa", "pt-br": "Mesa de Ocasião Rosa"}', '{"es": "Para meriendas rápidas", "pt-br": "Mesa de Ocasião Area Rosa"}'),
    ROW('table_silo_small*6', '{"es": "Mesa Area de pìe Azul", "pt-br": "Azul Mesa de Ocasião Area"}', '{"es": "Para meriendas rápidas", "pt-br": "Azul Mesa de Ocasião Area"}'),
    ROW('table_silo_small*7', '{"es": "Mesa Area de pie Verde", "pt-br": "Verde Mesa de Ocasião Area"}', '{"es": "Para meriendas rápidas", "pt-br": "Verde Mesa de Ocasião Area"}'),
    ROW('table_silo_small*8', '{"es": "Mesa Area de pie Amarilla", "pt-br": "Amarelo Mesa de Ocasião Area"}', '{"es": "Para meriendas rápidas", "pt-br": "Amarelo Mesa de Ocasião Area"}'),
    ROW('table_silo_small*9', '{"es": "Mesa Area de pie Roja", "pt-br": "Vermelho Mesa de Ocasião Area"}', '{"es": "Para meriendas rápidas", "pt-br": "Vermelho Mesa de Ocasião Area"}'),
    ROW('tampax_rug', '{"es": "Alfombra Tampax", "pt-br": "tampax_rug"}', '{"es": "¡Para chicas de hoy!", "pt-br": "tampax_rug"}'),
    ROW('tampax_wall', '{"es": "Póster Tampax", "pt-br": "Pôster Tampax"}', '{"es": "¡El de las chicas Tampax!", "pt-br": "Pôster Tampax"}'),
    ROW('teddy_basic', '{"es": "Oso Amoroso", "pt-br": "Teddy Básico"}', '{"es": "Es lo que tiene l''amour...", "pt-br": "O básico todo mundo tem que ter."}'),
    ROW('teddy_bear', '{"es": "Señor Oso", "pt-br": "Ursinho Teddy"}', '{"es": "¡Siempre tan elegante!", "pt-br": "Fofo e estiloso. Conquista qualquer um."}'),
    ROW('teddy_pendergrass', '{"es": "Oso Piloto", "pt-br": "Teddy Piloto"}', '{"es": "Vamos que nos vamos...", "pt-br": "Leva você às alturas sem sair do chão."}'),
    ROW('teddy_pink', '{"es": "Oso Anti-Amoroso", "pt-br": "Teddy Rosa"}', '{"es": "No le gusta ser achuchado. ¡Qué cosas!", "pt-br": "Impossível passar despercebido."}'),
    ROW('teleport_door', '{"es": "Puerta Temporal", "pt-br": "Porta Teleporte"}', '{"es": "¡Nunca se sabe en qué año te detendrás!", "pt-br": "Esconde outra dimensão..."}'),
    ROW('theatre_seat', '{"es": "Butaca Roja", "pt-br": "Poltrona Habbowood"}', '{"es": "¡Comienza el espectáculo!", "pt-br": "Já vem com pipoca e Suco de Bolhas."}'),
    ROW('throne', '{"es": "Trono", "pt-br": "Trono"}', '{"es": "Para Habbos HC", "pt-br": "Somente para Habbos importantes"}'),
    ROW('ticket', '{"es": "Ticket", "pt-br": "Tíquete"}', '{"es": "Ticket para Juegos", "pt-br": "Tíquete para Jogos"}'),
    ROW('tiki_bardesk', '{"es": "Barra Tiki", "pt-br": "Mesa de Palha e Bambu"}', '{"es": "¡Del mejor material!", "pt-br": "Mobis de Bambu são a última moda"}'),
    ROW('tiki_bench', '{"es": "Taburete Tiki", "pt-br": "Banco de Bambu"}', '{"es": "¡La mar de cómodo!", "pt-br": "É super confortável"}'),
    ROW('tiki_bflies', '{"es": "Mariposas", "pt-br": "Borboletas"}', '{"es": "¡De todos los colores!", "pt-br": "A natureza ao seu redor"}'),
    ROW('tiki_corner', '{"es": "Corner Tiki", "pt-br": "Canto de Bambu"}', '{"es": "¡Del mejor material!", "pt-br": "Bambus dobráveis para uma melhor forma"}'),
    ROW('tiki_junglerug', '{"es": "Jungla", "pt-br": "Tapete de Mata Fechada"}', '{"es": "Casi impenetrable", "pt-br": "Bem vindo à selva!"}'),
    ROW('tiki_parasol', '{"es": "Sombrilla Tiki", "pt-br": "Guarda Sol de Bambu"}', '{"es": "¡Protege del sol!", "pt-br": "Simples, estiloso e único"}'),
    ROW('tiki_sand', '{"es": "Arena Tiki", "pt-br": "Areia"}', '{"es": "¡La más fina del mundo!", "pt-br": "Direto da ilha Tiki"}'),
    ROW('tiki_statue', '{"es": "Totem Tiki", "pt-br": "Estátua Selvagem"}', '{"es": "Espíritu impactante", "pt-br": "Proteção contra os maus espíritos"}'),
    ROW('tiki_surfboard', '{"es": "Tabla de surf", "pt-br": "Prancha de Surf"}', '{"es": "¡Súbete a las olas!", "pt-br": "Pegue umas ondas iradas uhu!"}'),
    ROW('tiki_torch', '{"es": "Antorcha Tiki", "pt-br": "Tocha Indígena"}', '{"es": "¡Ilumínanos!", "pt-br": "Limite o seu território!"}'),
    ROW('tiki_toucan', '{"es": "Tucán", "pt-br": "Tucano"}', '{"es": "¡Ojo con los picotazos!", "pt-br": "Cuide bem dele"}'),
    ROW('tiki_tray0', '{"es": "Bandeja Tiki", "pt-br": "Cesta de Palha"}', '{"es": "¡Para rellenar!", "pt-br": "Guarde frutas tropicais"}'),
    ROW('tiki_tray1', '{"es": "Bandeja Tiki", "pt-br": "Cesta com Comidas típicas"}', '{"es": "Repleta de fruta", "pt-br": "Sabores jamais provados"}'),
    ROW('tiki_tray2', '{"es": "Bandeja Tiki", "pt-br": "Cesta de Frutas"}', '{"es": "Repleta de piñas", "pt-br": "Frutas de todos os sabores"}'),
    ROW('tiki_tray3', '{"es": "Bandeja Tiki", "pt-br": "Peixe assado e temperado"}', '{"es": "Repleta de pescado", "pt-br": "Um prato exótico e delicioso"}'),
    ROW('tiki_tray4', '{"es": "Bandeja Tiki", "pt-br": "Cesta com Porco Assado"}', '{"es": "¡Con la cabeza de un cerdo!", "pt-br": "Coitado do porquinho... vai um bacon aí?"}'),
    ROW('tiki_wallplnt', '{"es": "Enredadera", "pt-br": "Planta Selvagem"}', '{"es": "¡La mar de frondosa!", "pt-br": "Só nasce no meio da selva"}'),
    ROW('tiki_waterfall', '{"es": "Cascada", "pt-br": "Cachoeira"}', '{"es": "¡Be water!", "pt-br": "Lave a alma"}'),
    ROW('tile', '{"es": "Alfombra de baño", "pt-br": "Ladrilhos"}', '{"es": "Sécate los pies con glamour", "pt-br": "Em uma variedade de cores"}'),
    ROW('tile_brown', '{"es": "Baldosa Marrón", "pt-br": "Mármore Rosso"}', '{"es": "Ideales para pasear por ellas", "pt-br": "É como andar nas estrelas"}'),
    ROW('tile_marble', '{"es": "Baldosa de Mármol", "pt-br": "Mármore Carrara"}', '{"es": "¡Bien fresquita!", "pt-br": "É como andar nas estrelas"}'),
    ROW('tile_red', '{"es": "Pinturas", "pt-br": "Ladrilhos"}', '{"es": "Sécate los pies con glamour", "pt-br": "Em uma variedade de cores"}'),
    ROW('tile_stella', '{"es": "Baldosa de Estrella", "pt-br": "Ladrilho Stella"}', '{"es": "Una vuelta por el Paseo de la Fama", "pt-br": "Ladrilho de Stella"}'),
    ROW('tile_yell', '{"es": "Alfombra de baño", "pt-br": "Ladrilhos"}', '{"es": "Sécate los pies con glamour", "pt-br": "Em uma variedade de cores"}'),
    ROW('toilet', '{"es": "El Water", "pt-br": "Vaso Sanitário"}', '{"es": "Siéntate en color", "pt-br": "Vaso Sanitário"}'),
    ROW('toilet_red', '{"es": "El Water", "pt-br": "Vaso Sanitário"}', '{"es": "Siéntate en color", "pt-br": "Vaso Sanitário"}'),
    ROW('toilet_yell', '{"es": "El Water", "pt-br": "Vaso Sanitário"}', '{"es": "Siéntate en color", "pt-br": "Vaso Sanitário"}'),
    ROW('torch', '{"es": "Antorcha Gótica", "pt-br": "Tocha"}', '{"es": "El lado oscuro de Habbo", "pt-br": "Cadê meu gárgula?"}'),
    ROW('totem_head', '{"es": "Cabeza Sagrada", "pt-br": "Totem Lendário"}', '{"es": "Tótem: 2/3", "pt-br": "Colecionável de Maio/2009"}'),
    ROW('totem_leg', '{"es": "Fuego Sagrado", "pt-br": "Totem do Fogo"}', '{"es": "Totem 1/3", "pt-br": "A primeira parte do Lendário Totem."}'),
    ROW('totem_planet', '{"es": "Planeta Sagrado", "pt-br": "Planeta Totem"}', '{"es": "Tótem, 3/3", "pt-br": "Lua, Terra ou Sol"}'),
    ROW('toy1', '{"es": "Pelota", "pt-br": "Bola de Borracha"}', '{"es": "Las mascotas son juguetonas", "pt-br": "Um grande pulo de imaginação"}'),
    ROW('toy1*1', '{"es": "Pelota", "pt-br": "Bola de Borracha"}', '{"es": "Las mascotas son juguetonas", "pt-br": "Um grande pulo de imaginação"}'),
    ROW('toy1*2', '{"es": "Pelota", "pt-br": "Bola de Borracha"}', '{"es": "Las mascotas son juguetonas", "pt-br": "Um grande pulo de imaginação"}'),
    ROW('toy1*3', '{"es": "Pelota", "pt-br": "Bola de Borracha"}', '{"es": "Las mascotas son juguetonas", "pt-br": "Um grande pulo de imaginação"}'),
    ROW('toy1*4', '{"es": "Pelota", "pt-br": "Bola de Borracha"}', '{"es": "Las mascotas son juguetonas", "pt-br": "Um grande pulo de imaginação"}'),
    ROW('traffic_light*1', '{"es": "Semáforo Clásico", "pt-br": "Semáforo Clássico"}', '{"es": "Circula con calma", "pt-br": "No vermelho, pare."}'),
    ROW('traffic_light*2', '{"es": "Semáforo Azul", "pt-br": "Semáforo Azul"}', '{"es": "Circula con calma", "pt-br": "No vermelho, pare."}'),
    ROW('traffic_light*3', '{"es": "Semáforo Púrpura", "pt-br": "Semáforo Roxo"}', '{"es": "Circula con calma", "pt-br": "No vermelho, pare."}'),
    ROW('traffic_light*4', '{"es": "Semáforo Amarillo", "pt-br": "Semáforo Amarelo"}', '{"es": "Circula con calma", "pt-br": "No vermelho, pare."}'),
    ROW('traffic_light*5', '{"es": "Semáforo Blanco", "pt-br": "Semáforo Branco"}', '{"es": "Circula con calma", "pt-br": "No vermelho, pare."}'),
    ROW('traffic_light*6', '{"es": "Semáforo Rojo", "pt-br": "Semáforo Vermelho"}', '{"es": "Circula con calma", "pt-br": "No vermelho, pare."}'),
    ROW('transparent_floor', '{"es": "Suelo Brillante", "pt-br": "Piso Transparente"}', '{"es": "Para bailar un vals", "pt-br": "Para um chão de estrelas"}'),
    ROW('traxbronze', '{"es": "traxbronze", "pt-br": "traxbronze"}', '{}'),
    ROW('traxgold', '{"es": "traxgold", "pt-br": "traxgold"}', '{}'),
    ROW('traxsilver', '{"es": "traxsilver", "pt-br": "traxsilver"}', '{}'),
    ROW('tray_cake', '{"es": "Pastel", "pt-br": "Bolo confeitado"}', '{"es": "¡Delicioso!", "pt-br": "No fim da festa, fica com gosto de dedo."}'),
    ROW('tray_champagne', '{"es": "¡Champán!", "pt-br": "Champanhe"}', '{"es": "¿Brindas?", "pt-br": "Suco de bolhas engarrafado. Safra de quatro anos."}'),
    ROW('tray_glasstower', '{"es": "Torre de copas", "pt-br": "Torre de taças"}', '{"es": "¿Qué se celebra?", "pt-br": "As leis da Física na mesa da sua festa."}'),
    ROW('tree1', '{"es": "El Viejo Árbol", "pt-br": "Árvore de Natal antiga"}', '{"es": "Se mantiene como un roble", "pt-br": "Árvore de Natal antiga"}'),
    ROW('tree2', '{"es": "El Árbol", "pt-br": "Árvore de Natal antiga"}', '{"es": "El árbol más legendario", "pt-br": "Árvore de Natal antiga"}'),
    ROW('tree3', '{"es": "El abeto de navidad", "pt-br": "Árvore de Natal"}', '{"es": "¿Todavía no te han dejado ningún regalo?", "pt-br": "Já colocaram os presentes?"}'),
    ROW('tree4', '{"es": "El Arbolito", "pt-br": "Árvore de Natal"}', '{"es": "Es imprescindible", "pt-br": "Já colocaram os presentes?"}'),
    ROW('tree5', '{"es": "El árbol total", "pt-br": "Árvore de Natal 3"}', '{"es": "¿Todavía no te han dejado ningún regalo?", "pt-br": "Já colocaram os presentes?"}'),
    ROW('tree6', '{"es": "Arbolitos chillones", "pt-br": "Poderoso Pinheiro Quantum"}', '{"es": "¡Que brillen tus ojos!", "pt-br": "Natal em nível molecular"}'),
    ROW('tree7', '{"es": "Árboles Nevados", "pt-br": "Pinheiro Nevado"}', '{"es": "¡Es nieve de la buena!", "pt-br": "Companhia A. Bonada. Todos os direitos reservados."}'),
    ROW('triplecandle', '{"es": "Velas eléctricas", "pt-br": "Velas Elétricas"}', '{"es": "No hay que preocuparse de la cera.", "pt-br": "Sem perigo de se queimar com a cera quente"}'),
    ROW('turkey', '{"es": "El pavo de Navidad", "pt-br": "Peru Assado"}', '{"es": "El símbolo de la cena navideña", "pt-br": "Cadê a farofa?"}'),
    ROW('tv_luxus', '{"es": "Televisor", "pt-br": "TV Digital"}', '{"es": "El cine en tu Sala", "pt-br": "Nota Dez em sintonia"}'),
    ROW('typingmachine', '{"es": "La Máquina de escribir", "pt-br": "Máquina de Escrever"}', '{"es": "Famosa e imprescindible", "pt-br": "Escreva o seu bestseller"}'),
    ROW('urban_bench', '{"es": "Banco Urbano", "pt-br": "Banco de Pedra"}', '{"es": "No es el más cómodo", "pt-br": "Comum dos grandes centros urbanos"}'),
    ROW('urban_bench_plain', '{"es": "Banco Urbano 2", "pt-br": "Banco de praça"}', '{"es": "Muy cómodo no parece", "pt-br": "Seu guarda eu não sou vagabundo, eu não sou delinquente..."}'),
    ROW('urban_bin', '{"es": "Contenedor Urbano", "pt-br": "Cesto de Lixo"}', '{"es": "Debe oler muy bien", "pt-br": "Se duvidar, dentro você encontra até dinheiro."}'),
    ROW('urban_blocker', '{"es": "Bloque Hormigón", "pt-br": "Barreira de Pedra"}', '{"es": "Duro y resistente", "pt-br": "Usado em construções civis"}'),
    ROW('urban_bsktbll', '{"es": "Canasta Urbana", "pt-br": "Cesta de Basquete"}', '{"es": "¿Llegas a hacer un mate?", "pt-br": "Já jogou Basquete de Rua?"}'),
    ROW('urban_carsofa', '{"es": "Coche Abandonado", "pt-br": "Táxi"}', '{"es": "Mobiliario curioso", "pt-br": "Acho que esse carro não anda mais..."}'),
    ROW('urban_fence', '{"es": "Valla Urbana", "pt-br": "Grade"}', '{"es": "¿A que no la saltas?", "pt-br": "Não é tão alto, dá pra pular"}'),
    ROW('urban_fence_corner', '{"es": "Esquina Valla Urbana", "pt-br": "Canteiro de Grade"}', '{"es": "¿A que no la saltas?", "pt-br": "Não é tão alto, dá pra pular"}'),
    ROW('urban_lamp', '{"es": "Farola", "pt-br": "Poste de Luz"}', '{"es": "Ilumina las calles de Habbo", "pt-br": "Iluminando a cidade grande do alto."}'),
    ROW('urban_sidewalk', '{"es": "Pavimento", "pt-br": "Calçada"}', '{"es": "Nada como pisar la calle", "pt-br": "Ótimo para manobras de skate"}'),
    ROW('urban_wall', '{"es": "Muro Urbano", "pt-br": "Parede de Tijolos"}', '{"es": "La ciudad en tu Sala", "pt-br": "Do jeitinho da sua cidade"}'),
    ROW('urban_wpost', '{"es": "Boca de Agua", "pt-br": "Hidrante"}', '{"es": "¿Será potable?", "pt-br": "Banheiro de cachorro"}'),
    ROW('val09_floor', '{"es": "Baldosa Pulida", "pt-br": "Piso Polido"}', '{"es": "Dar cera, pulir cera...", "pt-br": "E com cheirinho de limpeza"}'),
    ROW('val09_floor2', '{"es": "Baldosa de Madera", "pt-br": "Piso de Madeira"}', '{"es": "La mejor calefacción para tu casa", "pt-br": "Recriação de um clássico"}'),
    ROW('val09_wall1', '{"es": "Panel Antiruidos", "pt-br": "Isolamento acústico"}', '{"es": "Insonoriza tu Sala", "pt-br": "O melhor da festa é não incomodar os vizinhos"}'),
    ROW('val09_wall2', '{"es": "Pared Empapelada", "pt-br": "Parede de bordados"}', '{"es": "¿Esconde alguna figura ese dibujo?", "pt-br": "Na borda do bordado brota uma imagem genial"}'),
    ROW('val09_wdrobe_b', '{"es": "Puertas Armario Negras", "pt-br": "Roupeiro preto"}', '{"es": "¿Cabrá toda tu ropa?", "pt-br": "Um pretinho básico é obrigatório."}'),
    ROW('val09_wdrobe_g', '{"es": "Puertas Armario Blancas", "pt-br": "Roupeiro de vidro"}', '{"es": "¿Cabrá toda tu ropa?", "pt-br": "Suas roupas num recanto de cristal."}'),
    ROW('val_basket', '{"es": "Bandeja de Regalos", "pt-br": "Cesta do Amor"}', '{"es": "Conquista con todo un detallazo", "pt-br": "Uma grata e carinhosa surpresa."}'),
    ROW('val_cauldron', '{"es": "Marmita", "pt-br": "Caldeirão Lendário"}', '{"es": "El Ojo Azul de Shakathuko es el principal ingrediente de su poción", "pt-br": "Poçoes mágicas ao seu alcance"}'),
    ROW('val_choco', '{"es": "Caja de Bombones", "pt-br": "Caixa de Chocolates"}', '{"es": "¡Menuda pinta tienen!", "pt-br": "Chocolate é tudo de bom"}'),
    ROW('val_hSeat*1', '{"es": "Silla Corazón Negra", "pt-br": "Cadeira Coração"}', '{"es": "¿Oyes sus latidos?", "pt-br": "O trono do amor na cor preta."}'),
    ROW('val_hSeat*2', '{"es": "Silla Corazón Blanca", "pt-br": "Cadeira Coração"}', '{"es": "¿Oyes sus latidos?", "pt-br": "O trono do amor na cor branca."}'),
    ROW('val_hSeat*3', '{"es": "Silla Corazón Marrón", "pt-br": "Cadeira Coração"}', '{"es": "¿Oyes sus latidos?", "pt-br": "O trono do amor na cor vermelha."}'),
    ROW('val_hSeat*4', '{"es": "Silla Corazón Gris", "pt-br": "Cadeira Coração"}', '{"es": "¿Oyes sus latidos?", "pt-br": "O trono do amor na cor rosa."}'),
    ROW('val_hSeat*5', '{"es": "Silla Corazón Verde", "pt-br": "Cadeira Coração"}', '{"es": "¿Oyes sus latidos?", "pt-br": "O trono do amor na cor bege."}'),
    ROW('val_hSeat*6', '{"es": "Silla Corazón Celeste", "pt-br": "Cadeira Coração"}', '{"es": "¿Oyes sus latidos?", "pt-br": "O trono do amor na cor violeta."}'),
    ROW('val_hSeat*7', '{"es": "Silla Corazón Azul", "pt-br": "Cadeira Coração"}', '{"es": "¿Oyes sus latidos?", "pt-br": "O trono do amor na cor azul."}'),
    ROW('val_heart', '{"es": "Corazón iluminado", "pt-br": "Coração Neon"}', '{"es": "¿Amor o desamor?", "pt-br": "...amar ou sofrer"}')) AS tr (classname, names, descs)
  ON f.`classname` = tr.classname
SET f.`localized_name` = CAST(tr.names AS JSON), f.`localized_desc` = CAST(tr.descs AS JSON);
UPDATE `definition_furniture` f JOIN (VALUES
    ROW('val_randomizer', '{"es": "Asientos del Amor", "pt-br": "Sorte no amor"}', '{"es": "¡Que viva el amor!", "pt-br": "E azar nos jogos..."}'),
    ROW('val_table1', '{"es": "Mesa Romántica para dos", "pt-br": "Mesa romântica"}', '{"es": "Conquista a tu amad@", "pt-br": "Reserva para dois, por favor!"}'),
    ROW('val_teddy*1', '{"es": "Oso de peluche gris", "pt-br": "Ursinho de Pelúcia Cinza"}', '{"es": "Mullidito para tu amad@ o para ti mism@", "pt-br": "Fofinho e meigo"}'),
    ROW('val_teddy*2', '{"es": "Osito Teddy Rosa", "pt-br": "Ursinho de Pelúcia Rosa"}', '{"es": "¡Dale un abrazo!", "pt-br": "Fofinho e meigo"}'),
    ROW('val_teddy*3', '{"es": "Oso de peluche verde", "pt-br": "Ursinho de Pelúcia Verde"}', '{"es": "Mullidito para tu amad@ o para ti mism@", "pt-br": "Fofinho e meigo"}'),
    ROW('val_teddy*4', '{"es": "Osito Teddy Marrón", "pt-br": "Ursinho de Pelúcia"}', '{"es": "¡Apriétalo bien fuerte!", "pt-br": "Fofinho e meigo"}'),
    ROW('val_teddy*5', '{"es": "Osito Teddy Amarillo", "pt-br": "Ursinho de Pelúcia Amarelo"}', '{"es": "Peluche Amarillo", "pt-br": "Fofinho e meigo"}'),
    ROW('val_teddy*6', '{"es": "Osito Teddy Azul", "pt-br": "Ursinho de Pelúcia Azul"}', '{"es": "¡Dale un abrazo!", "pt-br": "Fofinho e meigo"}'),
    ROW('valeduck', '{"es": "El Pato Love", "pt-br": "Pato Apaixonado"}', '{"es": "Para el baño del amor...", "pt-br": "Um banho de amor"}'),
    ROW('valentinescreen', '{"es": "Romance de Vacaciones", "pt-br": "Amor de verão"}', '{"es": "Asómate y sonríe", "pt-br": "Apareça e sorria!"}'),
    ROW('voting_ch', '{"es": "voting_ch name", "pt-br": "Cadeira de Festa"}', '{"es": "voting_ch desc", "pt-br": "Mas, afinal, quem vai querer num momento como esse?"}'),
    ROW('wall_china', '{"es": "Dragón Tatuado", "pt-br": "Biombo do Dragão"}', '{"es": "Se tatúa en las paredes", "pt-br": "Para sua Muralha"}'),
    ROW('wallmirror', '{"es": "Espejo", "pt-br": "Espelho"}', '{"es": "¡Espejito, espejito!...", "pt-br": "Espelho espelho meu..."}'),
    ROW('wallpaper', '{}', '{}'),
    ROW('waterbowl*1', '{"es": "Comedero rojo", "pt-br": "Balde de Água Vermelho"}', '{"es": "Para beber", "pt-br": "Água para todos os cantos"}'),
    ROW('waterbowl*2', '{"es": "Comedero verde", "pt-br": "Balde de Água Verde"}', '{"es": "Para beber", "pt-br": "Água para todos os cantos"}'),
    ROW('waterbowl*3', '{"es": "Comedero amarillo", "pt-br": "Balde de Água Amarelo"}', '{"es": "Para beber", "pt-br": "Água para todos os cantos"}'),
    ROW('waterbowl*4', '{"es": "Comedero azul", "pt-br": "Balde de Água Azul"}', '{"es": "Para beber", "pt-br": "Água para todos os cantos"}'),
    ROW('waterbowl*5', '{"es": "Comedero marrón", "pt-br": "Balde de Água Marrom"}', '{"es": "Para beber", "pt-br": "Água para todos os cantos"}'),
    ROW('wcandle', '{"es": "Velitas blancas", "pt-br": "Vela branca"}', '{"es": "Luz de blanca Navidad", "pt-br": "Vela de Natal"}'),
    ROW('wcandleset', '{"es": "A toda vela", "pt-br": "Velas Brancas"}', '{"es": "Sencillo, pero con estilo", "pt-br": "Simplicidade com estilo"}'),
    ROW('window_70s_narrow', '{"es": "Ventana de los 70", "pt-br": "Janela Retrô"}', '{"es": "¡Menudas vistas!", "pt-br": "Um toque dos anos 70 em seu Quarto"}'),
    ROW('window_70s_wide', '{"es": "Ventana de los 70", "pt-br": "Veneziana Retrô"}', '{"es": "¡Menudas vistas!", "pt-br": "Mais luz e estilo em sua decoração"}'),
    ROW('window_basic', '{"es": "Ventana", "pt-br": "Janela Simples"}', '{"es": "Ventana", "pt-br": "Aprecie a vista"}'),
    ROW('window_chinese_narrow', '{"es": "Ventana", "pt-br": "Janela Chinesa"}', '{"es": "Ventana", "pt-br": "Com o melhor estilo oriental"}'),
    ROW('window_chinese_wide', '{"es": "Ventana", "pt-br": "Janela Chinesa Grande"}', '{"es": "Ventana", "pt-br": "Uma obra de arte com formas fantásticas"}'),
    ROW('window_diner', '{"es": "Ventana Diner", "pt-br": "Janela"}', '{"es": "No sólo se ve comida ;)", "pt-br": "Janela"}'),
    ROW('window_diner2', '{"es": "Ventana Diner pequeña", "pt-br": "Janela"}', '{"es": "No sólo se ve comida ;)", "pt-br": "Janela"}'),
    ROW('window_double_default', '{"es": "Ventana", "pt-br": "Janela Fixa"}', '{"es": "Ventana", "pt-br": "Deslumbre a paisagem do Hotel"}'),
    ROW('window_golden', '{"es": "Ventana", "pt-br": "Janela Dourada"}', '{"es": "Ventana", "pt-br": "Revestida com o mais puro ouro!"}'),
    ROW('window_grunge', '{"es": "Ventana", "pt-br": "Janela Grunge"}', '{"es": "Ventana", "pt-br": "O importante mesmo é dar espaço para o ar!"}'),
    ROW('window_hole', '{"es": "Ventana", "pt-br": "Janela"}', '{"es": "Ventana", "pt-br": "Janela"}'),
    ROW('window_romantic_narrow', '{"es": "Ventana", "pt-br": "Janela Colonial"}', '{"es": "Ventana", "pt-br": "Mais chique, impossível"}'),
    ROW('window_romantic_wide', '{"es": "Ventana", "pt-br": "Janela Colonial"}', '{"es": "Ventana", "pt-br": "Não fique pendurado nela"}'),
    ROW('window_single_default', '{"es": "Ventana", "pt-br": "Janela Simples Fixa"}', '{"es": "Ventana", "pt-br": "Quartos arejados e iluminados"}'),
    ROW('window_skyscraper', '{"es": "Rascacielos", "pt-br": "Janela"}', '{"es": "¡Sube hasta lo más alto!", "pt-br": "Janela"}'),
    ROW('window_square', '{"es": "Ventana", "pt-br": "Janela Vitrô"}', '{"es": "Ventana", "pt-br": "Monte palácios de vidro"}'),
    ROW('window_triple', '{"es": "Ventana", "pt-br": "Janela Grande"}', '{"es": "Ventana", "pt-br": "Coloque no Quarto"}'),
    ROW('wood_tv', '{"es": "Televisor clásico", "pt-br": "TV Grande"}', '{"es": "Para toda la Habbo familia", "pt-br": "Para toda a família"}'),
    ROW('wooden_screen*0', '{"es": "Biombo Oriental Blanco", "pt-br": "Tela Oriental Branca"}', '{"es": "Un toque exótico para tu Sala", "pt-br": "Acrescente um toque exótico ao seu quarto"}'),
    ROW('wooden_screen*1', '{"es": "Biombo Oriental Rosa", "pt-br": "Tela Oriental Rosa"}', '{"es": "Un toque exótico para tu Sala", "pt-br": "Acrescente um toque exótico ao seu quarto"}'),
    ROW('wooden_screen*2', '{"es": "Biombo Oriental Rojo", "pt-br": "Tela Oriental Vermelha"}', '{"es": "Un toque exótico para tu Sala", "pt-br": "Acrescente um toque exótico ao seu quarto"}'),
    ROW('wooden_screen*3', '{"es": "Biombo Oriental Aqua", "pt-br": "Tela Oriental Azul-piscina"}', '{"es": "Un toque exótico para tu Sala", "pt-br": "Acrescente um toque exótico ao seu quarto"}'),
    ROW('wooden_screen*4', '{"es": "Biombo Oriental Dorado", "pt-br": "Tela Oriental Dourada"}', '{"es": "Un toque exótico para tu Sala", "pt-br": "Acrescente um toque exótico ao seu quarto"}'),
    ROW('wooden_screen*5', '{"es": "Biombo Oriental Gris", "pt-br": "Tela Oriental Cinza"}', '{"es": "Un toque exótico para tu Sala", "pt-br": "Acrescente um toque exótico ao seu quarto"}'),
    ROW('wooden_screen*6', '{"es": "Biombo Oriental Azul", "pt-br": "Tela Oriental Azul"}', '{"es": "Un toque exótico para tu Sala", "pt-br": "Acrescente um toque exótico ao seu quarto"}'),
    ROW('wooden_screen*7', '{"es": "Biombo Oriental Púrpura", "pt-br": "Tela Oriental Roxa"}', '{"es": "Un toque exótico para tu Sala", "pt-br": "Acrescente um toque exótico ao seu quarto"}'),
    ROW('wooden_screen*8', '{"es": "Biombo Oriental", "pt-br": "Tela Oriental Azul-escuro"}', '{"es": "Un toque exótico para tu Sala", "pt-br": "Acrescente um toque exótico ao seu quarto."}'),
    ROW('wooden_screen*9', '{"es": "Biombo Oriental Verde", "pt-br": "Tela Oriental Verde"}', '{"es": "Un toque exótico para tu Sala", "pt-br": "Acrescente um toque exótico ao seu quarto"}'),
    ROW('xm09_bauble_1', '{"es": "Adorno 1", "pt-br": "Enfeites de Natal"}', '{"es": "¡Feliz Navidad!", "pt-br": "Para tornar seu Natal ainda mais bonito!"}'),
    ROW('xm09_bauble_10', '{"es": "Adorno 10", "pt-br": "Enfeites de Natal"}', '{"es": "¡Feliz Navidad!", "pt-br": "Para tornar seu Natal ainda mais bonito!"}'),
    ROW('xm09_bauble_11', '{"es": "Adorno 11", "pt-br": "Enfeites de Natal"}', '{"es": "¡Feliz Navidad!", "pt-br": "Para tornar seu Natal ainda mais bonito!"}'),
    ROW('xm09_bauble_12', '{"es": "Adorno 12", "pt-br": "Enfeites de Natal"}', '{"es": "¡Feliz Navidad!", "pt-br": "Para tornar seu Natal ainda mais bonito!"}'),
    ROW('xm09_bauble_13', '{"es": "Adorno 13", "pt-br": "Enfeites de Natal"}', '{"es": "¡Feliz Navidad!", "pt-br": "Para tornar seu Natal ainda mais bonito!"}'),
    ROW('xm09_bauble_14', '{"es": "Adorno 14", "pt-br": "Enfeites de Natal"}', '{"es": "¡Feliz Navidad!", "pt-br": "Para tornar seu Natal ainda mais bonito!"}'),
    ROW('xm09_bauble_15', '{"es": "Adorno 15", "pt-br": "Enfeites de Natal"}', '{"es": "¡Feliz Navidad!", "pt-br": "Para tornar seu Natal ainda mais bonito!"}'),
    ROW('xm09_bauble_16', '{"es": "Adorno 16", "pt-br": "Enfeites de Natal"}', '{"es": "¡Feliz Navidad!", "pt-br": "Para tornar seu Natal ainda mais bonito!"}'),
    ROW('xm09_bauble_17', '{"es": "Adorno 17", "pt-br": "Enfeites de Natal"}', '{"es": "¡Feliz Navidad!", "pt-br": "Para tornar seu Natal ainda mais bonito!"}'),
    ROW('xm09_bauble_18', '{"es": "Adorno 18", "pt-br": "Enfeites de Natal"}', '{"es": "¡Feliz Navidad!", "pt-br": "Para tornar seu Natal ainda mais bonito!"}'),
    ROW('xm09_bauble_19', '{"es": "Adorno 19", "pt-br": "Enfeite de Natal 19"}', '{"es": "¡Feliz Navidad!", "pt-br": "Para enfeitar sua árvore"}'),
    ROW('xm09_bauble_2', '{"es": "Adorno 2", "pt-br": "Enfeites de Natal"}', '{"es": "¡Feliz Navidad!", "pt-br": "Para tornar seu Natal ainda mais bonito!"}'),
    ROW('xm09_bauble_20', '{"es": "Adorno 20", "pt-br": "Enfeites de Natal"}', '{"es": "¡Feliz Navidad!", "pt-br": "Para tornar seu Natal ainda mais bonito!"}'),
    ROW('xm09_bauble_21', '{"es": "Adorno 21", "pt-br": "Enfeites de Natal"}', '{"es": "¡Feliz Navidad!", "pt-br": "Para tornar seu Natal ainda mais bonito!"}'),
    ROW('xm09_bauble_22', '{"es": "Adorno 22", "pt-br": "Enfeites de Natal"}', '{"es": "¡Feliz Navidad!", "pt-br": "Para tornar seu Natal ainda mais bonito!"}'),
    ROW('xm09_bauble_23', '{"es": "Adorno 23", "pt-br": "Enfeites de Natal"}', '{"es": "¡Feliz Navidad!", "pt-br": "Para tornar seu Natal ainda mais bonito!"}'),
    ROW('xm09_bauble_24', '{"es": "Adorno 24", "pt-br": "Enfeites de Natal"}', '{"es": "¡Feliz Navidad!", "pt-br": "Para tornar seu Natal ainda mais bonito!"}'),
    ROW('xm09_bauble_25', '{"es": "Adorno 25", "pt-br": "Enfeites de Natal"}', '{"es": "¡Feliz Navidad!", "pt-br": "Para tornar seu Natal ainda mais bonito!"}'),
    ROW('xm09_bauble_26', '{"es": "Adorno 26", "pt-br": "Enfeites de Natal"}', '{"es": "¡Feliz Navidad!", "pt-br": "Para tornar seu Natal ainda mais bonito!"}'),
    ROW('xm09_bauble_27', '{"es": "Adorno 27", "pt-br": "Enfeites de Natal"}', '{"es": "¡Feliz Navidad!", "pt-br": "Para tornar seu Natal ainda mais bonito!"}'),
    ROW('xm09_bauble_3', '{"es": "Adorno 3", "pt-br": "Enfeites de Natal"}', '{"es": "¡Feliz Navidad!", "pt-br": "Para tornar seu Natal ainda mais bonito!"}'),
    ROW('xm09_bauble_4', '{"es": "Adorno 4", "pt-br": "Enfeites de Natal"}', '{"es": "¡Feliz Navidad!", "pt-br": "Para tornar seu Natal ainda mais bonito!"}'),
    ROW('xm09_bauble_5', '{"es": "Adorno 5", "pt-br": "Enfeites de Natal"}', '{"es": "¡Feliz Navidad!", "pt-br": "Para tornar seu Natal ainda mais bonito!"}'),
    ROW('xm09_bauble_6', '{"es": "Adorno 6", "pt-br": "Enfeites de Natal"}', '{"es": "¡Feliz Navidad!", "pt-br": "Para tornar seu Natal ainda mais bonito!"}'),
    ROW('xm09_bauble_7', '{"es": "Adorno 7", "pt-br": "Enfeites de Natal"}', '{"es": "¡Feliz Navidad!", "pt-br": "Para tornar seu Natal ainda mais bonito!"}'),
    ROW('xm09_bauble_8', '{"es": "Adorno 8", "pt-br": "Enfeites de Natal"}', '{"es": "¡Feliz Navidad!", "pt-br": "Para tornar seu Natal ainda mais bonito!"}'),
    ROW('xm09_bauble_9', '{"es": "Adorno 9", "pt-br": "Enfeites de Natal"}', '{"es": "¡Feliz Navidad!", "pt-br": "Para tornar seu Natal ainda mais bonito!"}'),
    ROW('xm09_bench', '{"es": "Banco Navideño", "pt-br": "Banco de Natal"}', '{"es": "¿Listo para el gran banquete?", "pt-br": "Cabe a família inteira!"}'),
    ROW('xm09_candyCane', '{"es": "Bastoncillos", "pt-br": "Doces Natalinos"}', '{"es": "¡Una Navidad muy dulce!", "pt-br": "Até os dentistas aprovam"}'),
    ROW('xm09_cocoa', '{"es": "Máquina de Cacao", "pt-br": "Máquina de Chocolate Quente"}', '{"es": "¿No te apetece un buen chocolate caliente?", "pt-br": "Quente, doce, raro, gostoso, chocolate"}'),
    ROW('xm09_firwall', '{"es": "Bosque Nevado", "pt-br": "Floresta Gelada"}', '{"es": "Ojo, podrías perderte", "pt-br": "Temperatura negativa"}'),
    ROW('xm09_forestwall', '{"es": "Bosquecillo Nevado", "pt-br": "Floresta Gelada 2"}', '{"es": "¿Qué habrá tras esos árboles?", "pt-br": "Temperatura Negativa"}'),
    ROW('xm09_frplc', '{"es": "Chimenea Navideña", "pt-br": "Lareira do Natal 2009"}', '{"es": "Un fuego cálido y acogedor", "pt-br": "Grande o suficiente para o Papai Noel descer"}'),
    ROW('xm09_infotv', '{"es": "Pantalla Habbo TV", "pt-br": "Televisão de Natal 2009"}', '{"es": "¡Feliz Navidad!", "pt-br": "Ho Ho Ho - Feliz Natal!"}'),
    ROW('xm09_lodgewall', '{"es": "Cabaña Helada", "pt-br": "Cabana do Natal"}', '{"es": "¿Refugio para el frío?", "pt-br": "Ho Ho Ho"}'),
    ROW('xm09_lrgBauble', '{"es": "Bola de Navidad", "pt-br": "Enfeite Grande de Natal"}', '{"es": "¡Ya está aquí la Navidad!", "pt-br": "Ho Ho Ho"}'),
    ROW('xm09_man_a', '{"es": "Muñeco de Nieve 1", "pt-br": "Boneco de Neve 1"}', '{"es": "¡Aquí tienes su base!", "pt-br": "Derretendo em 30 minutos..."}'),
    ROW('xm09_man_b', '{"es": "Muñeco de Nieve 2", "pt-br": "Boneco de Neve 2"}', '{"es": "¡Aquí tienes el tronco central!", "pt-br": "Derretendo em 30 minutos..."}'),
    ROW('xm09_man_c', '{"es": "Muñeco de Nieve 3", "pt-br": "Boneco de Neve 3"}', '{"es": "¡Aquí tienes su cabeza!", "pt-br": "Derretendo em 30 minutos..."}'),
    ROW('xm09_stocking', '{"es": "Calcetín de Navidad", "pt-br": "Meias de Natal"}', '{"es": "¿Lo llenarán de regalos?", "pt-br": "Cheia de presentes"}'),
    ROW('xm09_table', '{"es": "Mesa Navideña", "pt-br": "Mesa de Natal"}', '{"es": "A ver, a ver esos manjares", "pt-br": "Perfeita pra um banquete"}'),
    ROW('xm09_trophy', '{"es": "Habbo 2009", "pt-br": "Troféu 2009"}', '{"es": "Sólo para Habbos multivotados", "pt-br": "Para quem marcou o ano de 2009."}'),
    ROW('xmas08_chair', '{"es": "Silla Ártica", "pt-br": "Cadeira de Natal"}', '{"es": "¡Un asiento que combate la celulitis pixelar!", "pt-br": "Pelo menos é refrescante"}'),
    ROW('xmas08_cubetree', '{"es": "Bonsai Ártico", "pt-br": "Árvore de Cubos"}', '{"es": "¿Creías que los cubitos crecían en el congelador?", "pt-br": "Feita por Pablo Picasso"}'),
    ROW('xmas08_dvdr1', '{"es": "Murete Ártico", "pt-br": "Divisória Natalina"}', '{"es": "Robusto, pero deja pasar las bolas altas", "pt-br": "Sempre uma boa opção"}'),
    ROW('xmas08_dvdr2', '{"es": "Pozo Ártico", "pt-br": "Divisória de Natal"}', '{"es": "Saca de ahí tus deseos congelados", "pt-br": "Para ficar organizado"}'),
    ROW('xmas08_geysir', '{"es": "Géiser Ártico", "pt-br": "Gêiser"}', '{"es": "¡Un espectacular fenómeno de la naturaleza!", "pt-br": "Em portugal é géiser"}'),
    ROW('xmas08_hole', '{"es": "Agujero Ártico", "pt-br": "Buraco no Gelo"}', '{"es": "Vía directa a la pescadería", "pt-br": "Não vá cair"}'),
    ROW('xmas08_hottub', '{"es": "Bañera Ártica", "pt-br": "Tubo Quente"}', '{"es": "Mientras l@s otr@s tiritan, tú disfruta", "pt-br": "É preciso se aquecer"}'),
    ROW('xmas08_icerug', '{"es": "Suelo Ártico", "pt-br": "Piso de Gelo"}', '{"es": "Ajusta el nivel de nieve de tu sala", "pt-br": "Escorregadio"}'),
    ROW('xmas08_icetree', '{"es": "Árbol Ártico", "pt-br": "Árvore de Gelo"}', '{"es": "Estaría bien que crecieran regalos a sus pies", "pt-br": "Cuidado pra não derreter"}'),
    ROW('xmas08_icewall', '{"es": "Pared Ártica", "pt-br": "Parede de Gelo"}', '{"es": "No te apoyes mucho rato", "pt-br": "Uma casa de picolé"}'),
    ROW('xmas08_lantern', '{"es": "Lámpara Ártica", "pt-br": "Luminária Natalina"}', '{"es": "La luz está al otro lado de la nieve", "pt-br": "Ilumina bons caminhos"}'),
    ROW('xmas08_snowpl', '{"es": "Puf Ártico", "pt-br": "Boneco de Neve"}', '{"es": "Un asiento acolchado para tus doloridos píxeles", "pt-br": "Mais um amiguinho"}'),
    ROW('xmas08_table', '{"es": "Mesa Ártica", "pt-br": "Mesa de Natal"}', '{"es": "Recomendada para cenas frías", "pt-br": "Cabe a ceia inteira"}'),
    ROW('xmas08_telep', '{"es": "Teleportador Ártico", "pt-br": "Teleport Natalino"}', '{"es": "Volverás temblando", "pt-br": "Faz um pedido e entra"}'),
    ROW('xmas08_trph1', '{"es": "Pingüino Polo", "pt-br": "Troféu de Natal"}', '{"es": "Es un trofeo muy útil para el verano", "pt-br": "Para conquistas de fim de ano"}'),
    ROW('xmas08_trph2', '{"es": "Trofeo Pingüino", "pt-br": "Troféu de Pingüim"}', '{"es": "¡Totalmente helado!", "pt-br": "Mas olha só quem chegou..."}'),
    ROW('xmas08_wallpaper', '{"es": "Tapizado Ártico", "pt-br": "Papel de Parede de Natal"}', '{"es": "Vive la noche polar", "pt-br": "Casa Natalina"}'),
    ROW('xmas_cstl_gate', '{"es": "Puerta helada", "pt-br": "Portão de Gelo"}', '{"es": "¡A prueba de golpes!", "pt-br": "Castelo Gelado"}'),
    ROW('xmas_cstl_twr', '{"es": "Torre helada", "pt-br": "Torre de Gelo"}', '{"es": "¡Buenas vistas en invierno!", "pt-br": "Castelo Gelado"}'),
    ROW('xmas_cstl_wall', '{"es": "Pared helada", "pt-br": "Muralha de Gelo"}', '{"es": "¡A prueba de ruidos!", "pt-br": "Quatro Muros de Gelo Cmilla"}'),
    ROW('xmas_icelamp', '{"es": "Lámpara de Hielo", "pt-br": "Lanterna de Gelo"}', '{"es": "¡Centellea en medio del frío!", "pt-br": "Cinco Lanternas de Gelo"}'),
    ROW('xmas_icewall', '{"es": "Muro de Hielo", "pt-br": "Parede de Gelo"}', '{"es": "xmas_icewall desc", "pt-br": "Dura como água!"}'),
    ROW('xmas_light', '{"es": "Luces de Navidad", "pt-br": "Luz de Natal"}', '{"es": "¡Un toque navideño para tu Sala!", "pt-br": "Bate o sino pequenino..."}'),
    ROW('xmas_snow', '{"es": "Copos de Nieve", "pt-br": "Flocos de Neve"}', '{"es": "Muévete en la ventisca", "pt-br": "Parece mágica!"}'),
    ROW('xmasduck', '{"es": "El Pato de Navidad", "pt-br": "Pato de Natal"}', '{"es": "Cuak, cuak", "pt-br": "Com gorrinho."}'),
    ROW('year2010', '{"es": "Año 2010", "pt-br": "Pôster 2010"}', '{"es": "¡Éste va a ser el primero de los mejores años de tu vida!", "pt-br": "Para um ano novo brilhante!"}')) AS tr (classname, names, descs)
  ON f.`classname` = tr.classname
SET f.`localized_name` = CAST(tr.names AS JSON), f.`localized_desc` = CAST(tr.descs AS JSON);
