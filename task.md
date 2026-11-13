# Задача 6. VK-бот VkBotApi на EsConsultationServices

## Постановка

Нужно заменить старый проект `EsService\WebApi` новым проектом `VkBotApi` на .NET 7.
Бот ВКонтакте должен выполнять те же функции, что и раньше, но получать всю логику
консультации из `EsConsultationServices`, а не из `ExpertSystemDb`/`Logic`/`VkBotLib`.

Старый `WebApi` — это .NET Framework Web API с самодельной библиотекой `VkBotLib`:

- принимает callback ВКонтакте по маршруту `api/VkBot/VkMessageAccept`;
- обрабатывает сообщения командами `start`, `Consultation`, `SetAnswerAndNext`,
  `Another`, `Review`;
- сам работает с `ExpertSystemDb` и `Logic`, дублируя логику консультации;
- отправляет тексты, клавиатуры и постеры через `VkApiIntegrator`.

Новый проект должен:

1. Быть ASP.NET Core Web API на .NET 7 с именем `VkBotApi`.
2. Использовать официальную библиотеку ВКонтакте, совместимую с .NET 7.
3. Проводить консультацию только через `EsConsultationServices`; не использовать
   `ExpertSystemDb`, `Logic` и `VkBotLib`.
4. Сохранить маршрут callback `api/VkBot/VkMessageAccept` и совместимый формат
   payload `команда|sessionId|переменная|значение`.
5. Сохранить прежние команды, тексты, клавиатуры и порядок показа фильма.
6. Обрабатывать современный формат callback (`object.message`) и старый плоский
   формат (`object`), чтобы уже настроенный callback продолжил работать.
7. Не добавлять шаг «Знаю, чего хочу»: в боте его не было и он не нужен.
8. Удалить старый проект `EsService\WebApi` и убрать из решения ставший ненужным
   `VkBotLib`.
9. Оставить `EsService\Logic` в решении без изменений.

Бизнес-логика консультации не должна оставаться в HTTP-слое. Общий сценарий
«старт → вопрос → ответ → результат → другой фильм → оценка», который сейчас лежит
в `EsConsultationApi.Services.ConsultationWorkflow`, переезжает в
`EsConsultationServices`, чтобы им пользовались и HTTP API, и VK-бот.

## Выбранное решение

### 1. Связь проектов

```
VkBotApi (ASP.NET Core, net7.0)
  └─ EsConsultationServices
       ├─ EsDb
       └─ FilmDb
```

- `VkBotApi` — только транспорт VK: callback, очередь обновлений, команды,
  клавиатуры, отправка сообщений и постеров.
- `EsConsultationServices` — весь сценарий консультации, подбор фильма, показы
  и оценки.
- `EsConsultationApi` остаётся HTTP API и использует тот же workflow.

Схема зависимостей после задачи:

```
VkBotApi ─┐
          ├─ EsConsultationServices ─┬─ EsDb
EsConsultationApi ─┘                └─ FilmDb
```

`VkBotApi` не ссылается на `EsConsultationApi`: общий код находится в
`EsConsultationServices`, а не в одном из транспортов.

### 2. Перенос workflow в EsConsultationServices

В `EsConsultationServices` переезжают классы, которые сейчас лежат в
`EsConsultationApi`:

- `ConsultationWorkflow`;
- `ConsultationExpertSystemResolver`;
- `ConsultationShownFilmStore`;
- `ConsultationReviewStore`;
- `ConsultationOptions`.

Вместо возврата HTTP-модели `ConsultationResponse` workflow возвращает
сервисный результат:

- `sessionId`;
- `expertSystemId`, `expertSystemName`;
- статус `Question | Completed | Unknown`;
- текущий шаг (вопрос или результат);
- подобранный фильм `RecommendedFilm` (если есть).

Добавляется контракт `IConsultationWorkflow` с методами:

- `StartAsync(expertSystemId, ct)`;
- `GetCurrentAsync(sessionId, ct)`;
- `AnswerAsync(sessionId, variableName, value, ct)`;
- `GetAnotherAsync(sessionId, ct)`;
- `ReviewAsync(sessionId, liked, comment, ct)`.

`EsConsultationApi` перестаёт содержать бизнес-логику: контроллер вызывает
`IConsultationWorkflow`, а маппинг результата в `ConsultationResponse`/`FilmDto`
остаётся в API. `VkBotApi` использует результат напрямую.

`AddEsConsultationServices()` регистрирует workflow, резолвер ЭС, хранилище
показов и хранилище оценок. Приложения задают только строки подключения и секцию
`Consultation` (`ExpertSystemId`/`ExpertSystemName`).

### 3. VkBotApi

Проект создаётся в корне репозитория: `D:\PROJECTS\CSharp\FilmFinder\VkBotApi`,
добавляется в `EsService\ЭС.sln`.

Состав:

- `Controllers/VkBotController.cs` — callback `POST api/VkBot/VkMessageAccept`;
- разбор callback: `confirmation`, `message_new`, современный и legacy формат;
- очередь обновлений и фоновый обработчик, чтобы callback отвечал `ok` сразу;
- команды `start`, `Consultation`, `SetAnswerAndNext`, `Another`, `Review`;
- построение клавиатур с прежними payload;
- отправка текста, клавиатуры и постера через VkNet;
- конфигурация `Vk` (токен, подтверждение, секрет, id группы).

Используется `VkNet` 1.78.0 — официальная библиотека с поддержкой `net7.0`.
Токен и ключ подтверждения читаются из конфигурации/переменных окружения,
а не хранятся в коде, как в старом `WebApi`.

Callback:

- `confirmation` → вернуть ключ подтверждения;
- `message_new` → положить сообщение в очередь и сразу вернуть `ok`;
- при заданном `Vk:Secret` обновление с чужим секретом отклоняется;
- payload имеет приоритет над текстом;
- имя команды сравнивается без учёта регистра, аргументы сохраняют регистр.

### 4. Совместимость команд

Сохраняются команды и payload:

| Команда | Payload | Назначение |
|---|---|---|
| `start` | — | приветствие и кнопка «Подобрать!» |
| `Consultation` | `Consultation` | новая консультация и первый вопрос |
| `SetAnswerAndNext` | `SetAnswerAndNext\|sessionId\|variable\|value` | ответ и следующий вопрос или результат |
| `Another` | `Another\|sessionId\|filmId` | другой фильм по тому же совету |
| `Review` | `Review\|sessionId\|Positive\|Negative` | оценка «понравилось / нет» |

Сохраняются прежние тексты:

- приветствие: «Привет! Я помогу подобрать фильм под ситуацию и настроение.»;
- «Сейчас что-нибудь подберу...»;
- «Сейчас поищу...»;
- «Спасибо за оценку!»;
- «К сожалению, по данным параметрам ничего не найдено»;
- «К сожалению, в работе сервиса произошла ошибка. Вы можете повторить попытку позже.»;
- «Начать подбор заново», «Попробовать еще раз».

Карточка фильма повторяет прежний текст:

```
{Название} ({год})

Осторожно, возможен машинный перевод!

{описание}

Рейтинг IMDb: {рейтинг:N2}

Основываясь на мнении пользователей, этот фильм:
{процент}% {свойство}
...
```

Клавиатура ответа на вопрос содержит варианты домена, разбитые по 3 кнопки в ряд,
и отдельную строку «Начать подбор заново». Клавиатура фильма содержит:

- «Подобрать еще один фильм!»;
- «Другой по тем же параметрам»;
- «👎🏻» и «👍🏻».

Цвета кнопок вариантов в новой `EsDb` не хранятся (в старой модели было поле
`Color`), поэтому варианты ответа оформляются нейтрально; остальные цвета
сохраняются.

### 5. Оценки

Оценка сохраняется в `EsDb.Reviews` по `SessionId`, как в старом боте:

- `Positive` → `Estimate = 10`;
- `Negative` → `Estimate = 0`;
- комментарий — `"{dd.MM.yyyy HH:mm:ss} - {оценка}"`.

Это сохраняет формат, который старый бот писал в `Comment`.

### 6. Постеры

Постер фильма скачивается по URL и загружается в сообщения ВКонтакте через
`photos.getMessagesUploadServer` / `photos.saveMessagesPhoto`, затем отправляется
отдельным сообщением с вложением. Если постер отсутствует или загрузка не удалась,
бот всё равно отправляет текстовую карточку — как и раньше.

### 7. Очистка и решение

- Удаляется каталог `EsService\WebApi` целиком.
- Из `EsService\ЭС.sln` удаляются проекты `WebApi` и `VkBotLib` со всеми
  конфигурациями и вложенностью.
- `EsService\Logic` остаётся в решении без изменений, хотя после удаления
  `WebApi` у него не остаётся потребителей в решении.
- В решение добавляется `VkBotApi`.
- `README.md` обновляется: раздел `WebApi` заменяется описанием `VkBotApi`,
  в `EsConsultationServices` добавляется workflow.

### 8. Тесты и проверка

- Существующие тесты `EsConsultationApi.Tests` переводятся на
  `IConsultationWorkflow` и должны проходить после переноса.
- Добавляется `VkBotApi.Tests`:
  - разбор payload и legacy/современного callback;
  - сохранение регистра аргументов;
  - точные payload и раскладка клавиатур;
  - формат карточки фильма;
  - команды: старт, консультация, ответ, другой фильм, оценка, ошибка.
- Собираются `EsConsultationServices`, `EsConsultationApi`, `VkBotApi`.
- `dotnet test` для `EsConsultationApi.Tests`, `VkBotApi.Tests`, `EsServices.Tests`.

## Что входит в задачу

1. Перенести workflow, резолвер ЭС, хранилища показов/оценок и настройки в
   `EsConsultationServices`, добавить `IConsultationWorkflow`.
2. Перевести `EsConsultationApi` на сервисный workflow и вынести маппинг ответа
   в API.
3. Создать `VkBotApi` на .NET 7 с VkNet 1.78.0.
4. Реализовать callback, разбор обновлений, очередь, команды, клавиатуры,
   тексты и загрузку постеров.
5. Сохранить совместимость маршрута и payload.
6. Удалить `EsService\WebApi`, убрать `VkBotLib` из решения, оставить `Logic`.
7. Добавить и обновить тесты.
8. Обновить `README.md`.

## Что не входит в задачу

- Изменения схемы `EsDb`/`FilmsDb` и миграции.
- Изменение алгоритма консультации и подбора фильмов.
- Шаг «Знаю, чего хочу» в боте.
- Поддержка других мессенджеров.
- Полноценная гарантированная доставка сообщений (очередь в памяти, как и раньше).
- Удаление `EsService\Logic` и `ExpertSystemDb` — только отвязка от них.

## Критерии готовности

- `VkBotApi` собирается как .NET 7 проект.
- `EsConsultationServices` и `EsConsultationApi` собираются после переноса.
- Callback `POST api/VkBot/VkMessageAccept` отвечает `ok` на `message_new`
  и ключом подтверждения на `confirmation`.
- Команды `start`, `Consultation`, `SetAnswerAndNext`, `Another`, `Review`
  работают через `EsConsultationServices`.
- Payload с переменной и значением не портит регистр.
- Бот отправляет те же тексты, клавиатуры и карточку фильма.
- `EsService\WebApi` удалён, `VkBotLib` отсутствует в решении, `Logic` остался.
- Тесты проходят.
- `README.md` описывает новый проект.

## Проверка

- `dotnet build EsConsultationServices/EsConsultationServices.csproj`
- `dotnet build EsConsultationApi/EsConsultationApi.csproj`
- `dotnet build VkBotApi/VkBotApi.csproj`
- `dotnet test EsConsultationApi.Tests/EsConsultationApi.Tests.csproj`
- `dotnet test VkBotApi.Tests/VkBotApi.Tests.csproj`
- `dotnet test EsServices.Tests/EsServices.Tests.csproj`
- Ручной сценарий в VK:
  1. `start` — приветствие;
  2. «Подобрать!» — первый вопрос;
  3. ответы на вопросы;
  4. результат и карточка фильма с постером;
  5. «Другой по тем же параметрам» — другой фильм;
  6. «👍🏻»/«👎🏻» — запись в `Reviews` и благодарность.

## Замечания

- Полная сборка `EsService\ЭС.sln` может падать на старых .NET Framework
  проектах (`LoadFilms`); проверяются net7-проекты по отдельности.
- Очередь обновлений хранится в памяти: при падении процесса необработанные
  сообщения теряются. Это не хуже старого поведения `Task.Factory.StartNew`.
- VK-токен не коммитится: в `appsettings.json` остаются пустые значения,
  реальные задаются переменными окружения или user secrets.
