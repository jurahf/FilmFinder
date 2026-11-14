# План реализации Task 7: TgBotWClient

Основание: `task.md` (постановка + выбранное решение).

Изменения в БД: одна новая таблица `EsDb.FrontendChatSessions` + миграция.
Ответы пользователя и раньше сохранялись в `EsDb.ConsultationAnswers`; новая
таблица нужна только для привязки «чат Telegram → консультация», чтобы
reply-кнопки работали после перезапуска сервиса.

## 0. Подготовка и проверка API библиотеки

- [x] Собрать существующие проекты, чтобы убедиться, что база зелёная:
  `dotnet build VkBotApi/VkBotApi.csproj`.
- [x] Проверить актуальные сигнатуры `WTelegramClient` 4.4.10 по установленному
  пакету (а не по старым примерам): `Client.Messages_SendMessage`,
  `Client.Messages_SendMedia`, `Client.Messages_SetBotCallbackAnswer`,
  `Client.LoginBotIfNeeded`, `Client.MTProxyUrl`, `UpdatesBase.UpdateList`.
  В API layer 230 используются `ReplyKeyboardMarkup`, `ReplyInlineMarkup`,
  `KeyboardButtonRow`, `KeyboardInlineButtonRow`, `KeyboardInlineButton`,
  `InlineButtonTypeCallback` — не Bot API-имена `InlineKeyboardMarkup`.
- [x] Зафиксировать версию `WTelegramClient` 4.4.10 в `csproj`.
- [x] Проверить, что `dotnet ef` может работать с `EsDb` через
  `EsDbContextFactory` (EF tools 8.0.6, runtime EF 7.0.16).

## 1. EsDb: привязка чата к консультации

- [x] Добавить `EsDb/Model/FrontendChatSession.cs`:
  - `BaseEntity` (`Id`);
  - `Frontend` (строка, ключ фронтенда; для бота — `telegram`);
  - `ChatId` (строка — идентификатор чата Telegram);
  - `SessionId` (строка — `Sessions.SessionId`);
  - `UpdateDate`;
  - навигация `Session`.
- [x] В `EsDbContext`:
  - `DbSet<FrontendChatSession> FrontendChatSessions`;
  - уникальный индекс `(Frontend, ChatId)`;
  - индекс `SessionId`;
  - FK `SessionId` → `Sessions.SessionId` через `HasPrincipalKey` (alternate key)
    с `DeleteBehavior.Cascade`.
- [x] Создать миграцию:
  `dotnet ef migrations add FrontendChatSessions --project EsDb --startup-project EsDb`.
  Проверить `Up`/`Down` и `EsDbContextModelSnapshot`. Если инструмент не
  совместим с EF 7 — написать миграцию вручную по образцу существующих.
- [ ] Применить миграцию к локальной `EsDb` для ручной проверки.

## 2. Каркас проекта и решение

- [x] Создать `TgBotWClient/TgBotWClient.csproj`:
  - `Microsoft.NET.Sdk`, `OutputType=Exe`, `net7.0`, `Nullable`, `ImplicitUsings`;
  - пакеты: `WTelegramClient` 4.4.10,
    `Microsoft.EntityFrameworkCore.SqlServer` 7.0.16,
    `Microsoft.Extensions.Hosting` 7.0.0,
    `Serilog`, `Serilog.Extensions.Hosting`,
    `Serilog.Settings.Configuration`, `Serilog.Sinks.Console`;
  - ссылки: `EsConsultationServices`, `EsDb`, `FilmDb`.
- [x] Создать `TgBotWClient/Program.cs` на Generic Host:
  - `Host.CreateApplicationBuilder(args)`;
  - `AddJsonFile("secret.appsettings.json", optional: true)` после стандартных
    json-файлов, чтобы локальный файл перекрывал `appsettings.json`;
  - проверка `EsConnection`, `FilmConnection`, `Telegram:ApiId`, `ApiHash`, `BotToken`;
  - `AddDbContext<EsDbContext>`, `AddDbContext<FilmDbContext>`;
  - `IConsultationDisplayProvider` → `AdviceConsultationDisplayProvider`,
    затем `AddEsConsultationServices()`;
  - регистрация очереди, hosted-сервисов, команд, парсера, клавиатур,
    презентеров, sender, `ITgChatSessionStore`;
  - `await app.RunAsync()`.
- [x] Добавить проект в `ЭС.sln` вручную, переиспользуя существующие GUID папок
  решения: `Consultations` = `{B00A7EE0-BBCA-47B1-A0F5-91575B7DF70F}`,
  `Frontends` = `{CACCED3E-8C21-4CAE-A20D-51E3AF31526D}`. Не запускать
  `dotnet sln add --solution-folder`, чтобы не создать дубли папок.
- [x] Создать `TgBotWClient/appsettings.json`:
  - строки подключения как в `VkBotApi`;
  - `Consultation` (`ExpertSystemId=0`, `ExpertSystemName=Фильмы`);
  - `Telegram` с `ApiId=0`, пустыми `ApiHash`/`BotToken`, `SessionPath`,
    пустой `MtProxy`;
  - `Serilog` (`MinimumLevel`, `WriteTo: Console`);
  - `AllowedHosts` не нужен (нет HTTP), `Logging` убирается или остаётся пустым.
- [x] Создать локальный `TgBotWClient/secret.appsettings.json` с теми же секциями
  и пустыми значениями; в `csproj` — `CopyToOutputDirectory=PreserveNewest`.
- [x] Добавить в `.gitignore`:
  - `secret.appsettings.json` (и `**/secret.appsettings.json`);
  - `*.session` (файл сессии WTelegram содержит ключи авторизации).

## 3. Serilog

- [x] В `Program.cs` до построения host:
  - bootstrap-логгер (`Log.Logger`) для ошибок старта;
  - после создания builder — `ReadFrom.Configuration(builder.Configuration)`,
    `Enrich.FromLogContext()`, консольный sink.
- [x] `builder.Logging.ClearProviders()` и регистрация Serilog единственным
  провайдером (`AddSerilog(..., dispose: true)` или `UseSerilog`).
- [x] В `finally` — `Log.CloseAndFlush()`.
- [x] `WTelegram.Helpers.Log` направить в `ILoggerFactory`/Serilog.
- [x] В `appsettings.json` — секция `Serilog` с уровнем и консольным sink,
  чтобы уровень менялся без пересборки.
- [x] Логировать: старт/остановку, успешный логин, входящую команду с chatId,
  ошибки команд и обновлений.

## 4. Конфигурация и подключение Telegram

- [x] `Options/TelegramOptions.cs`:
  - `SectionName = "Telegram"`;
  - `ApiId`, `ApiHash`, `BotToken`, `SessionPath`;
  - вложенный `MtProxyOptions` (`Server`, `Port`, `Secret`) с методом
    `IsConfigured` / `Validate`.
- [x] `Telegram/TelegramClientFactory.cs` (или фабрика в `Program.cs`):
  - путь сессии относительно `IHostEnvironment.ContentRootPath`;
  - создание каталога сессии;
  - `new Client(apiId, apiHash, sessionPath)`;
  - при заданном MTProxy — `client.MTProxyUrl = "https://t.me/proxy?server=...&port=...&secret=..."`;
  - регистрация `Client` singleton (DI сам вызовет `Dispose`).
- [x] `Telegram/TelegramClientHost.cs` (`IHostedService`):
  - `await client.LoginBotIfNeeded(botToken)`;
  - подписка на `client.OnUpdates`;
  - в обработчике: `foreach (var update in updates.UpdateList)`, фильтр
    `UpdateNewMessage` (`Message`, не `out_`, `peer_id is PeerUser`) и
    `UpdateBotCallbackQuery`;
  - преобразование в `TgIncomingMessage` и `queue.Enqueue(...)`;
  - все исключения логировать, не выпускать из `OnUpdates`;
  - отписка при остановке.
- [x] `Telegram/TgUpdateQueue.cs` — `Channel<TgIncomingMessage>` (unbounded),
  как `VkUpdateQueue`.
- [x] `Telegram/TgUpdateProcessor.cs` — `BackgroundService`, читает очередь
  последовательно, на каждое обновление создаёт scope, получает
  `TgBotHandler`, обрабатывает; ошибки логирует и продолжает.

## 5. Контракты, парсер, привязка чата

- [x] `Contracts/TgIncomingMessage.cs`:
  `ChatId`, `UserId`, `MessageId`, `Text`, `CallbackData`, `CallbackQueryId`,
  `IsCallback`.
- [x] `Contracts/TgCommandRequest.cs`: `Name`, `Arguments`, `GetArgument(i)`
  (копия подхода `VkCommandRequest`).
- [x] `Parsing/TgCommandParser.cs`:
  - callback data имеет приоритет; формат `command|arg|arg` (UTF-8);
  - текст: `/start`, `/help`, `/another`, `/consultation`, `/ok`, `/bad`;
  - тексты reply-кнопок: «Подобрать фильм», «Другой фильм», «Начать заново»,
    «👍 Ок», «👎 Не ок»;
  - неизвестный текст → `null`/`unknown`;
  - аргументы не теряют регистр.
- [x] `Bot/ITgChatSessionStore.cs`:
  - `GetSessionIdAsync(long chatId, ct)`;
  - `SetSessionIdAsync(long chatId, string sessionId, ct)`;
  - `ClearAsync(long chatId, ct)`.
- [x] `Bot/TgChatSessionStore.cs`:
  - работает через `EsDbContext.FrontendChatSessions`;
  - `Frontend = "telegram"`, `ChatId = chatId.ToString(CultureInfo.InvariantCulture)`;
  - `Set` — upsert (найти по `(Frontend, ChatId)`, обновить `SessionId`/`UpdateDate`
    или добавить строку);
  - при гонке уникального индекса — повторить update;
  - `Get` — вернуть `SessionId` актуальной привязки;
  - `Clear` — удалить строку.
- [x] Состояние в памяти не хранится: от двойного нажатия и от ответа на
  устаревший вопрос защищает hash текущего вопроса в callback data.

## 6. Клавиатуры и презентация

- [x] `Bot/TgKeyboardFactory.cs`:
  - reply-клавиатуры (`ReplyKeyboardMarkup`, `Flags.resize | Flags.persistent`):
    - старт: «Подобрать фильм»;
    - вопросы: «Начать заново»;
    - фильм: «Другой фильм», «👍 Ок»/«👎 Не ок», «Подобрать фильм»;
  - inline-клавиатура вопроса (`ReplyInlineMarkup`):
    - варианты из `ConsultationQuestion.Answers` по 2 в ряд;
    - callback data `answer|<sessionId>|<index>|<questionHash>`;
  - `questionHash` — 8 hex SHA-256 от `Name + "\n" + Text`;
  - проверка/обрезка вариантов, чтобы callback data ≤ 64 байт;
  - в тестах проверять длину в байтах UTF-8.
- [x] `Bot/TgBotTexts.cs` — тексты, аналогичные `VkBotTexts`, плюс
  приветствие/помощь/«нет фильмов»/«сессия устарела».
- [x] `Bot/FilmMessageFormatter.cs` — копия формата `VkBotApi`
  (название, год, предупреждение о переводе, описание, IMDb, теги).
- [x] `Bot/ITgMessageSender.cs`:
  - `SendTextAsync(long chatId, string text, ReplyMarkup? keyboard = null, ct)`;
  - `SendPhotoAsync(long chatId, string photoUrl, ReplyMarkup? keyboard = null, ct)`;
  - `AnswerCallbackAsync(long queryId, string? toast = null, ct)`.
- [x] `Bot/TgMessageSender.cs`:
  - `InputPeerUser(chatId, 0)` для личных чатов;
  - текст: `Messages_SendMessage(..., random_id, reply_markup: ...)`;
  - разбиение текста на части ≤ 4096 символов, клавиатура — на последнюю;
  - фото: `Messages_SendMedia` с `InputMediaPhotoExternal { url }`;
  - ack callback: `Messages_SetBotCallbackAnswer(queryId, 0, message: toast)`;
  - ошибки отправки фото логировать, не ронять карточку.
- [x] `Bot/TgStepPresenter.cs`:
  - вопрос → текст вопроса + inline-клавиатура + reply-клавиатура «Начать заново»
    (reply отправляется отдельным сообщением, т.к. на одном сообщении может быть
    только один `reply_markup`);
  - фильм → `TgFilmPresenter`;
  - нет результата → текст + reply-клавиатура старта.
- [x] `Bot/TgFilmPresenter.cs` — постер отдельным сообщением, затем текстовая
  карточка с reply-клавиатурой фильма.

## 7. Команды и обработчик

- [x] `Bot/Commands/ITgCommand.cs`:
  `Name`, `ExecuteAsync(TgIncomingMessage, TgCommandRequest, ct)`.
- [x] `Bot/Commands/StartCommand.cs` — приветствие и описание возможностей,
  reply-клавиатура «Подобрать фильм».
- [x] `Bot/Commands/StartConsultationCommand.cs` —
  `workflow.StartAsync(null)`, сохранить `SessionId` в
  `ITgChatSessionStore`, отправить первый вопрос.
- [x] `Bot/Commands/SetAnswerAndNextCommand.cs`:
  - `sessionId`, `index`, `questionHash` из аргументов;
  - `GetCurrentAsync(sessionId)` → актуальный вопрос;
  - проверить, что это вопрос, хеш совпадает, индекс в диапазоне;
  - при несовпадении — сообщение «вопрос устарел» и повторный показ текущего
    вопроса (без `AnswerAsync`);
  - иначе `AnswerAsync(sessionId, question.Name, option.Value)`;
  - сохранить/сбросить `AnsweredQuestionHash`;
  - если результат не вопрос — отправить «Сейчас что-нибудь подберу...»;
  - показать результат через presenter.
- [x] `Bot/Commands/AnotherCommand.cs`:
  - `sessionId` из `ITgChatSessionStore`; если нет — попросить начать подбор;
  - `GetAnotherAsync`; `KeyNotFoundException` → очистить привязку,
    «Больше нет подходящих фильмов» и клавиатура старта;
    `InvalidOperationException` → понятный текст.
- [x] `Bot/Commands/ReviewCommand.cs`:
  - `sessionId` из store; `liked` из аргумента/текста кнопки;
  - комментарий `"{dd.MM.yyyy HH:mm:ss} - Positive/Negative"`;
  - `ReviewAsync`; `KeyNotFoundException` → очистить привязку,
    залогировать и всё равно поблагодарить (как в `VkBotApi`).
- [x] `Bot/TgBotHandler.cs`:
  - разбор через `TgCommandParser`;
  - поиск команды по имени, выполнение;
  - для callback — ack до/после выполнения (в `finally`), чтобы не висел spinner;
  - `OperationCanceledException` пробрасывать, остальные — логировать и слать
    текст ошибки + актуальную reply-клавиатуру;
  - callback с неизвестной командой — ack + подсказка.

## 8. Тесты (`TgBotWClient.Tests`)

- [x] Создать тестовый проект по образцу `VkBotApi.Tests`:
  xunit 2.6.6, `Microsoft.NET.Test.Sdk` 17.8.0,
  `Microsoft.EntityFrameworkCore.InMemory` 7.0.16,
  ссылки на `TgBotWClient` и `EsConsultationServices`.
- [x] `Fakes.cs`: `FakeWorkflow` (как в `VkBotApi.Tests`),
  `FakeChatSessionStore`, `RecordingMessageSender`, `FlowFactory`,
  помощники чтения клавиатур.
- [x] `ParsingTests.cs`: callback, слэш-команды, тексты reply-кнопок,
  регистр аргументов, мусор.
- [x] `KeyboardAndFormatterTests.cs`:
  - раскладка reply-клавиатур по состояниям;
  - количество/порядок inline-кнопок;
  - длина callback data ≤ 64 байт;
  - хеш вопроса;
  - формат карточки фильма.
- [x] `ChatSessionStoreTests.cs` (EF InMemory):
  - создание привязки;
  - перезапись при новой консультации;
  - чтение из нового `DbContext` (имитация перезапуска);
  - `Clear`.
- [x] `CommandTests.cs`: старт, консультация, ответ, устаревший вопрос,
  следующий вопрос, фильм, другой фильм, оценка, ошибка команды,
  восстановление привязки после «перезапуска».
- [x] `TgBotHandler` дополнительно: ack callback вызывается при успехе и ошибке.

## 9. Документация и проверка

- [x] Обновить `README.md`:
  - новая секция `TgBotWClient` в разделе «Проведение консультаций»;
  - библиотека, авторизация как бот, MTProxy, reply/inline-клавиатуры,
    Serilog, `secret.appsettings.json`;
  - упомянуть `EsDb.FrontendChatSessions`;
  - в «Планах» отметить Telegram как реализованный (не удаляя остальные пункты).
- [x] Сборка:
  `dotnet build TgBotWClient/TgBotWClient.csproj`.
- [x] Тесты:
  `dotnet test TgBotWClient.Tests/TgBotWClient.Tests.csproj`.
- [x] Регрессия:
  `dotnet build VkBotApi/VkBotApi.csproj`, `dotnet build EsConsultationApi/EsConsultationApi.csproj`,
  `dotnet test VkBotApi.Tests/VkBotApi.Tests.csproj`.
- [x] Проверить миграцию: `dotnet ef migrations list --project EsDb`.
- [x] Проверить `git status`: `secret.appsettings.json` и `*.session` не должны
  попадать в индекс.
- [ ] Ручной сценарий: `/start` → консультация → фильм → «Другой фильм» →
  оценка → перезапуск сервиса → «Другой фильм» продолжает ту же консультацию.

## Изменения в базе данных

- Новая таблица `EsDb.FrontendChatSessions`:
  `Id`, `Frontend`, `ChatId`, `SessionId`, `UpdateDate`.
- Уникальный индекс `(Frontend, ChatId)`, индекс `SessionId`,
  FK на `Sessions.SessionId` (alternate key) с каскадным удалением.
- Новая миграция `FrontendChatSessions`.
- Существующие таблицы `EsDb` и `FilmsDb` не меняются.

## Изменения в логике

- Новая транспортная логика Telegram в `TgBotWClient`; бизнес-логика не меняется.
- `EsConsultationServices` переиспользуется без изменений; ответы, факты,
  показанные фильмы и оценки уже сохраняются в `EsDb`.
- Привязка чата к консультации теперь хранится в `EsDb.FrontendChatSessions`
  и переживает перезапуск.
- Логирование переводится на Serilog (консоль).

## Узкие и неочевидные места

1. **API layer WTelegramClient.** В 4.4.10 (layer 230) типы клавиатур называются
   `ReplyKeyboardMarkup`, `ReplyInlineMarkup`, `KeyboardInlineButtonRow`,
   `InlineButtonTypeCallback`; старые примеры с `InlineKeyboardMarkup` не
   компилируются. Компиляция против установленного пакета — обязательный шаг.
2. **Один `reply_markup` на сообщение.** Нельзя одновременно отправить reply-
   и inline-клавиатуру. Reply-клавиатура остаётся в чате, пока её не заменят,
   поэтому вопрос отправляется только с inline, а reply обновляется отдельным
   сообщением.
3. **Лимит callback data 64 байта.** Поэтому в callback только index и hash,
   а значение берётся из актуального вопроса. Хеш защищает от ответа на старый
   вопрос; дополнительно хранится `AnsweredQuestionHash` от двойного нажатия.
4. **Лимит сообщения Telegram 4096 символов.** Длинное описание фильма нужно
   разбивать на части, иначе `Messages_SendMessage` вернёт ошибку.
5. **Файл сессии.** `WTelegram.session` содержит ключи; он и
   `secret.appsettings.json` обязаны быть в `.gitignore`. Сессию нельзя
   использовать двумя процессами одновременно.
6. **Привязка чата в БД.** Уникальный индекс `(Frontend, ChatId)` должен
   обновляться upsert-ом; параллельных обработчиков одного чата нет, но
   повторный запуск второго процесса с тем же ботом не поддерживается.
7. **FK на alternate key.** `Sessions.SessionId` — не PK, поэтому нужен
   `HasPrincipalKey(x => x.SessionId)`; иначе EF создаст неверную связь.
8. **`OnUpdates` — событие.** Ошибка внутри не должна всплывать в клиент:
   обработчик только кладёт обновление в очередь; вся работа — в processor.
9. **Bots API vs MTProto.** Бот, авторизованный через `LoginBotIfNeeded`, не
   может писать пользователю первым, пока тот не нажал `/start`. Это ожидаемо.
10. **Фото по URL.** `InputMediaPhotoExternal` загружает фото на стороне
    Telegram; при недоступной ссылке отправка может упасть — нужен fallback
    на текст.
11. **Serilog и Generic Host.** Провайдеры логирования по умолчанию нужно
    очистить, иначе будет двойной вывод; `Log.CloseAndFlush()` обязателен
    в `finally`.
12. **.NET 7 + SDK 9.** Целевая платформа остаётся `net7.0`, пакеты 7.0.x;
    сборка проверяется установленным SDK 9, targeting pack 7.0.20 есть.
