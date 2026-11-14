# Задача 7. TG-бот TgBotWClient на WTelegramClient

## Постановка

В папке решения `Consultations / Frontends` (виртуальные папки `ЭС.sln`) нужно
создать новый проект `TgBotWClient` — Telegram-бот для консультаций по подбору
фильмов, по аналогии с `VkBotApi` и `EsConsultationApi`.

Требования:

1. Проект использует `WTelegramClient` для работы с Telegram API и
   `EsConsultationServices` для всей логики консультации.
2. Клиент авторизуется как бот (bot token).
3. Клиент умеет подключаться через MTProxy.
4. Функции аналогичны `VkBotApi`:
   - старт бота, приветствие и рассказ о возможностях;
   - старт новой консультации;
   - отправка вопросов, приём ответов и продвижение по выводу ЭС;
   - команда «другой фильм»;
   - простая оценка «ок / не ок»;
   - клавиатуры, в том числе собираемые по ходу диалога.
5. Настройки авторизации бота и MTProxy вынесены в конфиг.
6. Добавлен `secret.appsettings.json`, который попадает в `.gitignore` и хранит
   локальные секреты; в репозитории остаются только пустые значения.

## Выбранное решение

### 1. Тип и размещение проекта

- `TgBotWClient` — консольное приложение с Generic Host
  (`Microsoft.NET.Sdk`, `OutputType=Exe`, `net7.0`), без HTTP-сервера.
- Физически проект лежит в корне репозитория: `D:\PROJECTS\CSharp\FilmFinder\TgBotWClient`.
- В `ЭС.sln` проект добавляется в существующую папку решения
  `Consultations / Frontends` — так же, как `VkBotApi`.
- Тестовый проект `TgBotWClient.Tests` создаётся рядом, по образцу
  `VkBotApi.Tests`; в решение он не добавляется (как и `VkBotApi.Tests`).

Почему консоль с Generic Host: `WTelegramClient` — долгоживущий MTProto-клиент
с собственным соединением и обновлениями. Ему не нужен входящий HTTP; Generic
Host даёт DI, конфигурацию, логирование, `BackgroundService` и корректное
завершение, но не поднимает лишний веб-сервер.

### 2. Связь проектов

```
TgBotWClient (net7.0, консоль + Generic Host)
  ├─ EsConsultationServices (IConsultationWorkflow)
  ├─ EsDb
  └─ FilmDb
```

`TgBotWClient` — только транспорт Telegram: подключение, обновления, команды,
клавиатуры, отправка сообщений и постера. Бизнес-логика не дублируется:
используются `IConsultationWorkflow.StartAsync`, `GetCurrentAsync`,
`AnswerAsync`, `GetAnotherAsync`, `ReviewAsync`.

### 3. Клавиатуры: гибрид reply + inline

- **Reply-клавиатура** (под полем ввода) — для действий. Кнопки-тексты:
  - «Подобрать фильм» — новая консультация;
  - «Другой фильм» — следующий фильм по тому же совету;
  - «👍 Ок» и «👎 Не ок» — оценка;
  - «Начать заново» — новая консультация во время вопросов.

  Набор кнопок зависит от состояния: до старта — только «Подобрать фильм»;
  во время вопросов — «Начать заново»; после фильма — «Другой фильм»,
  «👍 Ок», «👎 Не ок», «Подобрать фильм».
- **Inline-клавиатура** — только варианты ответа на вопрос. Собирается из
  `ConsultationQuestion.Answers` по 2 кнопки в ряд.
- Текст кнопок reply-клавиатуры одновременно является текстовой командой,
  поэтому поддерживаются и `/start`, `/help`, `/another` (алиасы).

Callback data inline-кнопки:

```
answer|<sessionId>|<optionIndex>|<questionHash>
```

- `sessionId` — 32 символа (`Guid.NewGuid().ToString("N")` в `EsConsultationServices`);
- `optionIndex` — индекс варианта в текущем вопросе;
- `questionHash` — первые 8 hex-символов SHA-256 от `Name + "\n" + Text` вопроса.

Длина укладывается в лимит Telegram 64 байта. Хеш защищает от нажатия кнопки
старого вопроса: если вопрос уже сменился, бот не отвечает «не тем» значением,
а показывает текущий вопрос заново. Значение варианта не передаётся в callback:
оно берётся из актуального вопроса, полученного через `GetCurrentAsync`.

При нажатии callback-кнопки бот подтверждает запрос через
`Messages_SetBotCallbackAnswer`, чтобы у пользователя не крутился индикатор.

### 4. Состояние диалога и сохранение ответов

Ответы пользователя уже сохраняются в базу самим `EsConsultationServices`:
`ConsultationService.AnswerAsync` пишет ответ в `EsDb.ConsultationAnswers`,
а каждый следующий шаг (`GetCurrentAsync`, `AnswerAsync`, `GetAnotherAsync`)
заново загружает состояние консультации из `EsDb` через
`ConsultationStateStore`. Поэтому ответы и прогресс вывода переживают
перезапуск сервиса без дополнительного кода в боте.

Для reply-кнопок нужен только контекст, которого нет в тексте сообщения:
какая консультация относится к данному чату. Это тоже хранится в `EsDb`,
в новой таблице `FrontendChatSessions`:

| Поле | Назначение |
|---|---|
| `Id` | PK (как у остальных сущностей `EsDb`) |
| `Frontend` | ключ фронтенда, для бота — `telegram` |
| `ChatId` | идентификатор чата Telegram (строка) |
| `SessionId` | `EsDb.Sessions.SessionId` текущей консультации |
| `UpdateDate` | время последнего обновления привязки |

Индексы: уникальный `(Frontend, ChatId)` — у чата одна активная консультация;
индекс по `SessionId`; FK на `Sessions.SessionId` с каскадным удалением.

`TgChatSessionStore` (в `TgBotWClient`) читает и обновляет привязку при
старте консультации; при `KeyNotFoundException` от workflow привязка считается
устаревшей и удаляется. В памяти остаётся только `AnsweredQuestionHash` —
защита от двойного нажатия на один и тот же вопрос; после перезапуска эту роль
выполняет хеш в callback data.

Обновления обрабатываются одним последовательным `BackgroundService`
(как `VkUpdateProcessor` в `VkBotApi`), поэтому гонок по одному чату нет.

### 5. Подключение и авторизация

```
Telegram:
  ApiId: <api_id с my.telegram.org>
  ApiHash: <api_hash>
  BotToken: <токен от @BotFather>
  SessionPath: App_Data/telegram.session
  MtProxy:
    Server: ""
    Port: 0
    Secret: ""
```

- `WTelegram.Client` создаётся с `api_id`, `api_hash` и `session_pathname`.
- Авторизация: `await client.LoginBotIfNeeded(botToken)`.
- MTProxy: если `Server` задан, заполняется `client.MTProxyUrl` в виде
  `https://t.me/proxy?server=...&port=...&secret=...`; иначе прямое подключение.
- Частично заполненный MTProxy (задан сервер, но нет порта/секрета) — ошибка
  конфигурации при старте.
- Файл сессии создаётся в `App_Data/` (относительно content root) и добавлен
  в `.gitignore`: в нём ключи авторизации.
- Логи `WTelegramClient` перенаправляются в `ILogger`.

### 6. Логирование (Serilog)

- В `TgBotWClient` добавляется Serilog с консольным sink.
- Пакеты: `Serilog`, `Serilog.Extensions.Hosting`,
  `Serilog.Settings.Configuration`, `Serilog.Sinks.Console`.
- Конфигурация читается из секции `Serilog` в `appsettings.json`
  (`MinimumLevel`, `WriteTo: Console`), поэтому уровень логирования можно менять
  без пересборки.
- `builder.Logging.ClearProviders()` и регистрация Serilog как единственного
  провайдера логирования.
- Логи `WTelegramClient` через `WTelegram.Helpers.Log` попадают в тот же
  Serilog-конвейер и выводятся в консоль.
- Ошибки обработки обновлений и команд логируются с контекстом чата/команды.

### 7. Конфигурация и секреты

- `appsettings.json` содержит строки подключения `EsConnection`/`FilmConnection`,
  секцию `Consultation`, секцию `Telegram` с пустыми значениями и секцию
  `Serilog` с консольным выводом.
- `secret.appsettings.json` — локальный файл с теми же секциями и реальными
  значениями. Добавляется в `.gitignore`, копируется в выходную папку и
  подключается в конфигурацию после `appsettings*.json`; переменные окружения
  по-прежнему имеют приоритет.
- В `.gitignore` добавляются `secret.appsettings.json` и `*.session`.

### 8. Сценарии бота

| Действие | Триггер | Поведение |
|---|---|---|
| Приветствие | `/start` | Текст о возможностях + reply-кнопка «Подобрать фильм» |
| Новая консультация | «Подобрать фильм», `/consultation`, «Начать заново» | `StartAsync`, первый вопрос + inline-варианты + reply «Начать заново» |
| Ответ | inline-кнопка варианта | Проверка хеша вопроса, `AnswerAsync`; следующий вопрос или фильм |
| Другой фильм | «Другой фильм» / `/another` | `GetAnotherAsync`; при отсутствии — понятное сообщение |
| Оценка | «👍 Ок» / «👎 Не ок» | `ReviewAsync`, благодарность |
| Помощь | `/help` | Краткая инструкция |
| Неизвестный текст | — | Подсказка с доступными действиями |

Оценка пишется в `EsDb.Reviews` как в `VkBotApi`:
`Estimate = 10/0`, комментарий `"{dd.MM.yyyy HH:mm:ss} - Positive/Negative"`.

Тексты и карточка фильма повторяют `VkBotApi`:

```
{Название} ({год})

Осторожно, возможен машинный перевод!

{описание}

Рейтинг IMDb: {рейтинг:N2}

Основываясь на мнении пользователей, этот фильм:
{процент}% {свойство}
...
```

Постер отправляется как внешнее фото Telegram (`InputMediaPhotoExternal`);
если ссылка пустая или отправка не удалась — текстовая карточка всё равно
отправляется.

### 9. Обработка обновлений

- `OnUpdates` фильтрует `UpdateNewMessage` (личные чаты) и
  `UpdateBotCallbackQuery`; остальное игнорируется.
- Сообщения самого бота и исходящие не обрабатываются.
- Обновления складываются в `Channel<TgIncomingMessage>` и обрабатываются
  последовательно в отдельном scope на обновление.
- Ошибка команды логируется; пользователь получает текст ошибки и актуальную
  reply-клавиатуру, процесс продолжает работу.
- Callback подтверждается даже при ошибке команды.

## Состав проекта

```
TgBotWClient/
  Program.cs                       — Generic Host, Serilog, DI, конфигурация
  TgBotWClient.csproj              — net7.0, WTelegramClient, Serilog, EF SqlServer
  appsettings.json                 — пустые секреты, строки подключения, Serilog
  secret.appsettings.json          — локальные секреты (в .gitignore)
  Options/TelegramOptions.cs       — ApiId, ApiHash, BotToken, SessionPath, MtProxy
  Telegram/TelegramClientHost.cs   — логин, MTProxy, подписка на OnUpdates
  Telegram/TgUpdateQueue.cs        — очередь обновлений
  Telegram/TgUpdateProcessor.cs    — последовательная обработка
  Contracts/TgIncomingMessage.cs   — chatId, userId, text, callback data, queryId
  Contracts/TgCommandRequest.cs    — имя команды и аргументы
  Parsing/TgCommandParser.cs       — callback data, слэш-команды, кнопки
  Bot/TgBotHandler.cs              — маршрутизация команд, ошибки, ack callback
  Bot/TgBotTexts.cs                — тексты
  Bot/ITgChatSessionStore.cs       — привязка чата к консультации
  Bot/TgChatSessionStore.cs        — чтение/запись FrontendChatSessions в EsDb
  Bot/TgKeyboardFactory.cs         — reply- и inline-клавиатуры
  Bot/ITgMessageSender.cs          — отправка текста, фото, ack callback
  Bot/TgMessageSender.cs           — реализация через WTelegramClient
  Bot/TgStepPresenter.cs           — вопрос / фильм / нет результата
  Bot/TgFilmPresenter.cs           — постер + карточка
  Bot/FilmMessageFormatter.cs      — текст карточки
  Bot/Commands/ITgCommand.cs
  Bot/Commands/StartCommand.cs
  Bot/Commands/StartConsultationCommand.cs
  Bot/Commands/SetAnswerAndNextCommand.cs
  Bot/Commands/AnotherCommand.cs
  Bot/Commands/ReviewCommand.cs
TgBotWClient.Tests/                — тесты без реального Telegram
```

## Изменения в базе данных

- В `EsDb` добавляется сущность `FrontendChatSession`
  (`Frontend`, `ChatId`, `SessionId`, `UpdateDate`), `DbSet` и конфигурация
  в `EsDbContext`:
  - уникальный индекс `(Frontend, ChatId)`;
  - индекс `SessionId`;
  - FK на `Sessions.SessionId` (alternate key) с каскадным удалением.
- Создаётся миграция `FrontendChatSessions` в `EsDb/Migrations`.
- `FilmsDb` и существующие таблицы не меняются.

## Тесты

- Парсер: callback data, `/start`, `/another`, тексты reply-кнопок,
  регистр аргументов, мусорные данные.
- Клавиатуры: раскладка reply-кнопок по состояниям, количество и порядок
  inline-вариантов, длина callback data ≤ 64 байт, корректный хеш.
- Команды с фейковым `IConsultationWorkflow`, фейковым `ITgChatSessionStore`
  и фейковым отправителем: старт, ответ, устаревший вопрос, следующий вопрос,
  фильм, «другой фильм», оценка, ошибка.
- `TgChatSessionStore`: запись, перезапись при новой консультации, чтение
  привязки после «перезапуска» (новый scope/context).
- Формат карточки фильма.
- Сборка `TgBotWClient`, миграция `EsDb`, тесты `TgBotWClient.Tests`.

## Что не входит в задачу

- Изменения схемы `FilmsDb` и существующих таблиц `EsDb`.
- Изменения `EsConsultationServices` и алгоритма вывода.
- Вебхуки, HTTP API, health-check и админ-панель.
- Гарантированная доставка сообщений.
- Групповые чаты, каналы, несколько ботов на один процесс.
- Замена или рефакторинг `VkBotApi`.

## Критерии готовности

- `TgBotWClient` собирается как .NET 7 консольное приложение.
- Проект добавлен в `ЭС.sln` в `Consultations / Frontends`.
- Бот авторизуется по `BotToken` и работает через MTProxy, если он задан.
- `secret.appsettings.json` существует локально, не отслеживается git и
  перекрывает `appsettings.json`.
- Логи выводятся в консоль через Serilog, уровень настраивается в конфиге.
- `/start` показывает приветствие и возможности.
- «Подобрать фильм» запускает консультацию; вопросы приходят с inline-кнопками;
  ответы продвигают вывод ЭС; в конце приходит карточка фильма с постером.
- «Другой фильм» и «👍 Ок»/«👎 Не ок» работают через reply-клавиатуру.
- Ответы сохраняются в `EsDb.ConsultationAnswers`, привязка чата — в
  `EsDb.FrontendChatSessions`; после перезапуска сервиса «Другой фильм» и оценки
  продолжают работать для активной консультации.
- Оценки пишутся в `EsDb.Reviews` в прежнем формате.
- Миграция `FrontendChatSessions` создана.
- `dotnet build TgBotWClient/TgBotWClient.csproj` и
  `dotnet test TgBotWClient.Tests/TgBotWClient.Tests.csproj` проходят.
- `README.md` описывает новый проект.

## Проверка

- `dotnet ef migrations add FrontendChatSessions --project EsDb`
  (проверить сгенерированный `Up`/`Down`, применить к локальной `EsDb`).
- `dotnet build TgBotWClient/TgBotWClient.csproj`
- `dotnet test TgBotWClient.Tests/TgBotWClient.Tests.csproj`
- `dotnet build VkBotApi/VkBotApi.csproj` — убедиться, что общие проекты не сломаны.
- Ручной сценарий в Telegram:
  1. `/start` — приветствие и кнопки;
  2. «Подобрать фильм» — первый вопрос;
  3. ответы на вопросы;
  4. карточка фильма с постером;
  5. «Другой фильм» — следующий фильм;
  6. «👍 Ок» / «👎 Не ок» — запись в `Reviews` и благодарность;
  7. «Начать заново» во время вопросов — новая консультация;
  8. перезапустить сервис в середине консультации и нажать «Другой фильм» —
     бот продолжает ту же консультацию из `EsDb`.
- Проверка логов: в консоли видны старт, подключение, входящие команды и ошибки.
- Проверка MTProxy: задать `Telegram:MtProxy` в `secret.appsettings.json`,
  перезапустить бота и убедиться, что соединение устанавливается.
