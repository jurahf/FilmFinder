# Реализация Task 7: TgBotWClient

Бот Telegram на `WTelegramClient` (MTProto), консольное приложение .NET 7 на
Generic Host. Бизнес-логика не дублируется: используется
`IConsultationWorkflow` из `EsConsultationServices`.

## Что сделано

### Проект и решение

- `TgBotWClient/TgBotWClient.csproj` — `Microsoft.NET.Sdk`, `OutputType=Exe`,
  `net7.0`, пакеты `WTelegramClient` 4.4.10,
  `Microsoft.EntityFrameworkCore.SqlServer` 7.0.16,
  `Microsoft.Extensions.Hosting` 7.0.0, Serilog + консольный sink.
- Проект добавлен в `ЭС.sln` в папку `Consultations / Frontends`
  (GUID папок переиспользованы, дублей нет).
- `TgBotWClient.Tests` — xunit-тесты без реального Telegram.

### Подключение к Telegram

- `Options/TelegramOptions.cs`: `ApiId`, `ApiHash`, `BotToken`, `SessionPath`,
  вложенный `MtProxy` (`Server`, `Port`, `Secret`).
- Частично заполненный MTProxy и пустые обязательные поля — ошибка при старте
  с понятным сообщением.
- `Telegram/TelegramClientFactory.cs` создаёт единственный `Client`, каталог
  сессии, при необходимости задаёт `client.MTProxyUrl`
  (`https://t.me/proxy?server=...&port=...&secret=...`).
- `Telegram/TelegramClientHost.cs`: `LoginBotIfNeeded(BotToken)`, подписка на
  `OnUpdates`, фильтр `UpdateNewMessage` (не `out_`, `PeerUser`, текст) и
  `UpdateBotCallbackQuery`; обновления кладутся в `TgUpdateQueue`.
- `Telegram/TgUpdateProcessor.cs` последовательно обрабатывает очередь в
  отдельном DI-scope на каждое обновление.

### Клавиатуры

- Вопросы — inline-кнопки (`ReplyInlineMarkup`,
  `KeyboardInlineButton` + `InlineButtonTypeCallback`), по 2 в ряд.
- Действия — reply-кнопки (`ReplyKeyboardMarkup`, `resize | persistent`):
  «Подобрать фильм», «Другой фильм», «👍 Ок» / «👎 Не ок», «Начать заново».
- Callback data ответа: `answer|<sessionId>|<index>|<hash>`; hash — 8 hex
  SHA-256 от `Name + "\n" + Text` вопроса. Длина data проверяется (≤ 64 байт
  UTF-8); для `Guid.NewGuid().ToString("N")` (32 символа) запас есть.

### Привязка чата и сохранение состояния

- Новая таблица `EsDb.FrontendChatSessions`: `Id`, `Frontend`, `ChatId`,
  `SessionId`, `UpdateDate`, навигация на `Sessions`.
- Уникальный индекс `(Frontend, ChatId)`, индекс `SessionId`, FK на
  `Sessions.SessionId` через alternate key с каскадным удалением.
- `Bot/TgChatSessionStore.cs` делает upsert по `(Frontend = "telegram", ChatId)`,
  при гонке уникального индекса повторяет обновление.
- Ответы пользователя, факты, показанные фильмы и оценки уже сохраняются
  `EsConsultationServices` в `EsDb` (`ConsultationAnswers`,
  `ConsultationShownFilms`, `Reviews`). Дополнительного кода для этого не
  понадобилось.
- В памяти процесса состояние не хранится. От двойного нажатия и ответа на
  устаревший вопрос защищает hash текущего вопроса: при несовпадении ответ не
  записывается, бот показывает актуальный шаг.

### Команды

- `start` (`/start`, `/help`) — приветствие и возможности.
- `consultation` (`/consultation`, «Подобрать фильм», «Начать заново») — новая
  консультация, сохранение привязки, первый вопрос.
- `answer` — ответ на вопрос с проверкой hash и индекса.
- `another` (`/another`, «Другой фильм») — следующий фильм по привязке из БД.
- `review` (`/ok`, `/bad`, «👍 Ок», «👎 Не ок») — оценка, запись в
  `EsDb.Reviews` в прежнем формате.
- `TgBotHandler` подтверждает callback в `finally`, поэтому «часики» не висят
  даже при ошибке; ошибки логируются и превращаются в понятный текст.

### Логирование

- Serilog только с консольным sink; стандартные провайдеры отключены
  (`builder.Logging.ClearProviders()`).
- Уровень и шаблон — секция `Serilog` в `appsettings.json`.
- `WTelegram.Helpers.Log` (`Action<int, string>`) перенаправлен в
  `ILoggerFactory`, сообщения попадают в Serilog.
- Логируются старт/остановка, авторизация бота, входящие команды с `chatId`,
  ошибки обработки и отправки.

### Конфигурация и секреты

- `appsettings.json` — строки подключения, `Consultation`, `Telegram` с пустыми
  секретами, `Serilog`.
- `secret.appsettings.json` — локальный файл с реальными значениями;
  подключается после `appsettings.json`, в git не попадает.
- `.gitignore`: `secret.appsettings.json`, `**/secret.appsettings.json`,
  `*.session`.
- `Program.cs` переходит в `AppContext.BaseDirectory`, поэтому конфигурация и
  файл сессии всегда берутся рядом с исполняемым файлом (и при `dotnet run`,
  и при запуске из IDE).

## Тонкие места и решения

1. **Смена API WTelegramClient.** В 4.4.10 (layer 230) клавиатуры — это
   `ReplyKeyboardMarkup` / `ReplyInlineMarkup` с `KeyboardButtonRow` /
   `KeyboardInlineButtonRow` и `InlineButtonTypeCallback`; привычных
   `InlineKeyboardMarkup` / `KeyboardButtonCallback` нет. Код проверен
   компиляцией против установленного пакета 4.4.10.
2. **Одна клавиатура на сообщение.** На вопрос нельзя одновременно повесить
   inline- и reply-клавиатуру. Поэтому «Начать заново» уходит отдельным
   сообщением при старте консультации; reply-клавиатура в Telegram остаётся,
   пока её не заменят. После фильма выставляется клавиатура фильма.
3. **Лимит callback data 64 байта.** В data только имя команды, `sessionId`,
   индекс и hash; значение варианта берётся из актуального вопроса, а не из
   callback. `BuildAnswerCallbackData` явно проверяет длину.
4. **Лимит сообщения 4096 символов.** `TgMessageSender` режет текст по границе
   строки на части до 4000 символов; клавиатура ставится на последнюю часть.
5. **Постер по URL.** Используется `InputMediaPhotoExternal`. Ошибка отправки
   постера логируется и не мешает отправить текстовую карточку.
6. **Peer для личного чата.** Отправка идёт через `InputPeerUser(chatId, 0)`:
   бот пишет пользователю, который уже нажал `/start`.
7. **Ошибки `OnUpdates`.** Обработчик только разбирает обновление и кладёт его
   в очередь; все исключения ловятся и логируются, чтобы не ломать клиент.
8. **`Host.CreateApplicationBuilder` в .NET 7.** Перегрузки с
   `HostApplicationBuilderSettings` нет (появилась в .NET 8), поэтому content
   root задаётся через `Directory.SetCurrentDirectory(AppContext.BaseDirectory)`
   до создания builder.
9. **Implicit usings консольного SDK.** В отличие от Web SDK, консольный шаблон
   не подключает `Microsoft.Extensions.*`; добавлен `GlobalUsings.cs`.
10. **`WTelegram.Helpers.Log`.** Это `Action<int, string>`, а не `ILogger`;
    уровень маппится в `LogLevel` (`<=1` — Error, `2` — Warning и т. д.).
11. **Уникальный индекс привязки.** Параллельной обработки одного чата нет, но
    на всякий случай upsert повторяется после `DbUpdateException`.
12. **Устаревшая привязка.** Если workflow не находит сессию
    (`KeyNotFoundException`), привязка удаляется, а пользователь получает
    понятное сообщение и кнопку начала подбора.

## Проверка

- `dotnet build TgBotWClient/TgBotWClient.csproj` — успешно.
- `dotnet test TgBotWClient.Tests/TgBotWClient.Tests.csproj` — 49 тестов,
  все прошли.
- `dotnet build VkBotApi/VkBotApi.csproj` и
  `dotnet build EsConsultationApi/EsConsultationApi.csproj` — без ошибок
  (проверка, что изменения `EsDb` ничего не сломали).
- `dotnet ef migrations list --project EsDb --startup-project EsDb --no-build`
  показывает `20261113083905_FrontendChatSessions (Pending)`.
- `dotnet run --project TgBotWClient/TgBotWClient.csproj` выводит через Serilog
  понятную ошибку валидации, пока в `secret.appsettings.json` пустые
  `ApiId`/`ApiHash`/`BotToken`.
- `git check-ignore` подтверждает, что `secret.appsettings.json` и `*.session`
  не попадут в git.

## Что нужно сделать для ручной проверки

1. Применить миграцию к локальной `EsDb`:
   `dotnet ef database update --project EsDb --startup-project EsDb`
   (или выполнить `20261113083905_FrontendChatSessions` вручную).
2. Заполнить `TgBotWClient/secret.appsettings.json`:
   `Telegram:ApiId`, `Telegram:ApiHash`, `Telegram:BotToken`,
   при необходимости `Telegram:MtProxy`.
3. Запустить `dotnet run --project TgBotWClient/TgBotWClient.csproj` и пройти
   сценарий в Telegram:
   `/start` → «Подобрать фильм» → ответы → карточка фильма →
   «Другой фильм» → «👍 Ок»/«👎 Не ок» → перезапуск сервиса →
   «Другой фильм» продолжает ту же консультацию.

## Всплывшие вопросы

- **Групповые чаты и каналы** не поддерживаются: обрабатываются только
  `PeerUser`. Для задачи этого достаточно.
- **Одновременный запуск двух экземпляров бота** с одним `*.session`
  невозможен: WTelegramClient не поддерживает такую схему. Ограничение
  задокументировано, но не решается в рамках задачи.
- **Гарантированная доставка** сообщений не реализована (как и в `VkBotApi`):
  при сетевой ошибке сообщение может не дойти, но консультация в БД остаётся
  консистентной.
- **Комментарий оценки** остаётся прежним (`dd.MM.yyyy HH:mm:ss - Positive/Negative`);
  отдельного текстового комментария от пользователя в Telegram не запрашивается,
  как и в VK-боте.
