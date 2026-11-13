# План реализации: VkBotApi

Связанная постановка: `task.md`.

## 1. Итоговая архитектура

```
VkBotApi (ASP.NET Core Web API, net7.0)
  ├─ Controllers/VkBotController        — callback ВКонтакте
  ├─ Parsing/                           — разбор callback и команд
  ├─ Queue/                             — очередь обновлений и BackgroundService
  ├─ Bot/                               — команды, клавиатуры, тексты, отправка
  └─ EsConsultationServices
       ├─ Engine/                       — движок консультации (без изменений)
       ├─ Persistence/                  — состояние сессии в EsDb (без изменений)
       ├─ Films/                        — подбор фильма (без изменений)
       └─ Workflow/                     — НОВОЕ: общий сценарий консультации
            ├─ IConsultationWorkflow
            ├─ ConsultationWorkflow
            ├─ ConsultationExpertSystemResolver
            ├─ ConsultationShownFilmStore
            ├─ ConsultationReviewStore
            └─ ConsultationOptions

EsConsultationApi (net7.0)
  ├─ Controllers/ConsultationsController — HTTP API
  ├─ Mapping/ConsultationResponseMapper  — сервисный результат -> DTO
  └─ EsConsultationServices.Workflow
```

Принципы:

- `EsConsultationServices` — единственное место сценария «старт → вопрос → ответ →
  результат → другой фильм → оценка»;
- `EsConsultationApi` и `VkBotApi` — транспорты, бизнес-логики в них нет;
- `VkBotApi` не ссылается на `EsConsultationApi` и не использует
  `ExpertSystemDb`, `Logic`, `VkBotLib`;
- `EsService\Logic` остаётся в решении, но новый бот от него не зависит.

## 2. Изменения в базе данных

**Схема БД не меняется.** Миграции не создаются и не применяются.

Используются уже существующие таблицы:

- `EsDb` — `Sessions`, `Consultations`, `ConsultationShownFilms`, `Reviews`;
- `FilmsDb` — советы, свойства, фильмы.

## 3. EsConsultationServices: общий workflow

### 3.1. Новые файлы

Создаётся папка `EsConsultationServices/Workflow/`:

```
Workflow/
  ConsultationFlowContracts.cs
  ConsultationOptions.cs
  ConsultationExpertSystemResolver.cs
  ConsultationShownFilmStore.cs
  ConsultationReviewStore.cs
  ConsultationWorkflow.cs
```

Пространство имён — `EsConsultationServices.Workflow`.

### 3.2. `ConsultationFlowContracts.cs`

```csharp
public enum ConsultationFlowStatus
{
    Question,
    Completed,
    Unknown
}

public sealed class ConsultationFlowResult
{
    public string SessionId { get; init; } = string.Empty;
    public int ExpertSystemId { get; init; }
    public string ExpertSystemName { get; init; } = string.Empty;
    public ConsultationFlowStatus Status { get; init; }
    public ConsultationStep Step { get; init; } = new();
    public RecommendedFilm? Film { get; init; }
}

public interface IConsultationWorkflow
{
    Task<ConsultationFlowResult> StartAsync(
        int? expertSystemId,
        CancellationToken cancellationToken = default);

    Task<ConsultationFlowResult> GetCurrentAsync(
        string sessionId,
        CancellationToken cancellationToken = default);

    Task<ConsultationFlowResult> AnswerAsync(
        string sessionId,
        string variableName,
        string value,
        CancellationToken cancellationToken = default);

    Task<ConsultationFlowResult> GetAnotherAsync(
        string sessionId,
        CancellationToken cancellationToken = default);

    Task ReviewAsync(
        string sessionId,
        bool liked,
        string? comment,
        CancellationToken cancellationToken = default);
}
```

`Status` вычисляется так же, как старый `ConsultationStatuses` в API:

- `Step.Result == null` → `Question`;
- `Step.Result.IsUnknown` → `Unknown`;
- иначе → `Completed`.

Имена членов enum совпадают со строками `"Question"`, `"Completed"`, `"Unknown"`,
поэтому HTTP API сможет отдать `Status.ToString()` без отдельной таблицы.

### 3.3. Перенос классов

Из `EsConsultationApi` в `EsConsultationServices/Workflow/` переносятся
без изменения поведения:

- `ConsultationExpertSystemResolver` — приоритет Id из запроса → Id из настроек →
  имя из настроек; ошибки `KeyNotFoundException`/`InvalidOperationException`;
- `ConsultationShownFilmStore` — `GetIdsAsync`, `TryAddAsync`, повтор при конфликте
  уникального индекса;
- `ConsultationReviewStore` — запись `Review` по `SessionId`, `Estimate = 10/0`,
  `Comment` обрезается;
- `ConsultationOptions` — `ExpertSystemId`, `ExpertSystemName`, `SectionName = "Consultation"`.

Меняются только пространства имён.

### 3.4. `ConsultationWorkflow`

Логика переносится из `EsConsultationApi/Services/ConsultationWorkflow.cs`:

- `StartAsync` — резолвер ЭС, `IConsultationService.StartAsync`, результат;
- `GetCurrentAsync` — текущий шаг + данные ЭС из `EsDbContext`;
- `AnswerAsync` — ответ движку; если результат — совет (GUID), подобрать и
  зарегистрировать фильм;
- `GetAnotherAsync` — проверить, что результат является советом, подобрать следующий
  фильм с исключением показанных, иначе `KeyNotFoundException`/`InvalidOperationException`;
- `ReviewAsync` — делегировать в `ConsultationReviewStore`;
- `FindAndRegisterFilmAsync` — до 3 попыток: получить показанные, подобрать фильм,
  записать показ; при конфликте повторить;
- `GetAdviceKey` — результат должен быть не unknown, не пустой и парситься как `Guid`;
- `GetExpertSystemAsync` — Id и имя ЭС по `SessionId`.

Вместо `ConsultationResponse` workflow возвращает `ConsultationFlowResult`.
Маппинг в DTO APIs остаётся в соответствующем проекте.

### 3.5. Регистрация

`EsConsultationServices/ServiceCollectionExtensions.cs`:

```csharp
services.AddOptions();
services.AddMemoryCache();
services.TryAddScoped<IExpertSystemRepository, ExpertSystemRepository>();
services.TryAddScoped<IConsultationDisplayProvider, DefaultConsultationDisplayProvider>();
services.TryAddScoped<IAdviceRepository, AdviceRepository>();
services.TryAddScoped<IFilmRecommendationService, FilmRecommendationService>();

services.TryAddScoped<ConsultationExpertSystemResolver>();
services.TryAddScoped<ConsultationShownFilmStore>();
services.TryAddScoped<ConsultationReviewStore>();
services.TryAddScoped<IConsultationWorkflow, ConsultationWorkflow>();
```

`ConsultationOptions` приложение задаёт через
`Configure<ConsultationOptions>(configuration.GetSection("Consultation"))`.

В `EsConsultationServices.csproj` добавляется явная ссылка на пакет
`Microsoft.Extensions.Options` 7.0.0 (нужен для `IOptions<ConsultationOptions>`).

### 3.6. Проверка после переноса

```
dotnet build EsConsultationServices/EsConsultationServices.csproj
```

## 4. EsConsultationApi: переход на сервисный workflow

### 4.1. Удаляемые файлы

Удаляются:

```
EsConsultationApi/Services/ConsultationWorkflow.cs
EsConsultationApi/Services/ConsultationExpertSystemResolver.cs
EsConsultationApi/Services/ConsultationShownFilmStore.cs
EsConsultationApi/Services/ConsultationReviewStore.cs
EsConsultationApi/Options/ConsultationOptions.cs
```

Папки `Services/` и `Options/` удаляются, если остаются пустыми.

### 4.2. Контроллер

`ConsultationsController` вместо `ConsultationWorkflow` принимает
`IConsultationWorkflow` из `EsConsultationServices.Workflow`.

Маршруты и коды ответов не меняются:

| Метод | Маршрут |
|---|---|
| POST | `/api/consultations/Start` |
| GET | `/api/consultations/{sessionId}` |
| POST | `/api/consultations/{sessionId}/answers` |
| POST | `/api/consultations/{sessionId}/another` |
| POST | `/api/consultations/{sessionId}/review` |

### 4.3. Маппинг

Новый файл `EsConsultationApi/Mapping/ConsultationResponseMapper.cs`:

```csharp
public static ConsultationResponse ToResponse(ConsultationFlowResult result)
{
    return new ConsultationResponse
    {
        SessionId = result.SessionId,
        ExpertSystemId = result.ExpertSystemId,
        ExpertSystemName = result.ExpertSystemName,
        Status = result.Status.ToString(),
        Question = result.Step.Question,
        Result = result.Step.Result,
        Film = result.Film == null ? null : FilmMapper.ToDto(result.Film)
    };
}
```

`ConsultationResponse`, `FilmDto`, `FilmMapper`, `ApiExceptionMiddleware` не меняются.

### 4.4. `Program.cs`

- `using EsConsultationServices.Workflow;`
- `Configure<ConsultationOptions>` с секцией `"Consultation"`;
- `AddEsConsultationServices()` теперь сам регистрирует резолвер, хранилища и
  workflow;
- убрать явные `AddScoped<ConsultationExpertSystemResolver>`,
  `AddScoped<ConsultationShownFilmStore>`, `AddScoped<ConsultationReviewStore>`,
  `AddScoped<ConsultationWorkflow>`;
- регистрация `IConsultationDisplayProvider -> AdviceConsultationDisplayProvider`
  остаётся до `AddEsConsultationServices()`, чтобы `TryAddScoped` не подставил
  провайдер по умолчанию.

### 4.5. Тесты `EsConsultationApi.Tests`

- `ApiTestHost`:
  - `using EsConsultationServices.Workflow;`
  - `WithWorkflowAsync` принимает `IConsultationWorkflow`;
  - из `BuildProvider` убираются ручные регистрации workflow/хранилищ/резолвера;
  - остаются `AddOptions()`, `Configure<ConsultationOptions>`, оба `DbContext`,
    `IConsultationDisplayProvider`, `AddEsConsultationServices()`.
- `ConsultationApiTests`:
  - статусы сравниваются с `ConsultationFlowStatus.Question` /
    `ConsultationFlowStatus.Completed`;
  - остальные проверки не меняются.
- Добавить тест `ConsultationResponseMapper_maps_status_and_film`:
  результат workflow с фильмом превращается в `ConsultationResponse` со строковым
  статусом и `FilmDto`.

### 4.6. Проверка

```
dotnet build EsConsultationApi/EsConsultationApi.csproj
dotnet test EsConsultationApi.Tests/EsConsultationApi.Tests.csproj
```

## 5. Новый проект VkBotApi

### 5.1. Файл проекта

`VkBotApi/VkBotApi.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net7.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="7.0.16" />
    <PackageReference Include="VkNet" Version="1.78.0" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\EsConsultationServices\EsConsultationServices.csproj" />
    <ProjectReference Include="..\EsDb\EsDb.csproj" />
    <ProjectReference Include="..\FilmDb\FilmDb.csproj" />
  </ItemGroup>
</Project>
```

`VkNet` 1.78.0 содержит сборку `net7.0`; других VK-библиотек не добавляем.

### 5.2. Структура

```
VkBotApi/
  VkBotApi.csproj
  Program.cs
  appsettings.json
  appsettings.Development.json
  Controllers/
    VkBotController.cs
  Contracts/
    VkCallbackUpdate.cs
    VkIncomingMessage.cs
    VkCommandRequest.cs
  Options/
    VkOptions.cs
  Parsing/
    VkCallbackParser.cs
    VkCommandParser.cs
  Queue/
    VkUpdateQueue.cs
    VkUpdateProcessor.cs
  Bot/
    IVkMessageSender.cs
    VkMessageSender.cs
    VkKeyboardFactory.cs
    FilmMessageFormatter.cs
    VkBotTexts.cs
    VkBotHandler.cs
    Commands/
      IVkCommand.cs
      StartCommand.cs
      StartConsultationCommand.cs
      SetAnswerAndNextCommand.cs
      AnotherCommand.cs
      ReviewCommand.cs
```

### 5.3. `Program.cs`

- читает `EsConnection` и `FilmConnection` (как `EsConsultationApi`);
- проверяет `Vk:AccessToken` и `Vk:ConfirmationToken`, падает с понятной ошибкой,
  если они пусты;
- `Configure<ConsultationOptions>` и `Configure<VkOptions>`;
- `AddDbContext<EsDbContext>` / `AddDbContext<FilmDbContext>`;
- `AddScoped<IConsultationDisplayProvider, AdviceConsultationDisplayProvider>()`;
- `AddEsConsultationServices()`;
- `AddHttpClient()` — для скачивания постера;
- `VkApi` — singleton, авторизация по access token из конфигурации;
- `VkUpdateQueue` — singleton;
- `VkUpdateProcessor` — hosted service;
- scoped: `VkBotHandler`, `IVkMessageSender -> VkMessageSender`,
  `VkKeyboardFactory`, `FilmMessageFormatter`;
- scoped `IVkCommand -> StartCommand`, `StartConsultationCommand`,
  `SetAnswerAndNextCommand`, `AnotherCommand`, `ReviewCommand`;
- `AddControllers()`, `MapControllers()`.

`appsettings.json`:

```json
{
  "ConnectionStrings": {
    "EsConnection": "data source=(local);initial catalog=EsDb;TrustServerCertificate=true;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework",
    "FilmConnection": "data source=(local);initial catalog=FilmsDb;TrustServerCertificate=true;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework"
  },
  "Consultation": {
    "ExpertSystemId": 0,
    "ExpertSystemName": "Фильмы"
  },
  "Vk": {
    "AccessToken": "",
    "ConfirmationToken": "",
    "Secret": "",
    "GroupId": null
  },
  "Logging": { "...": "..." },
  "AllowedHosts": "*"
}
```

`appsettings.Development.json` — только уровни логирования, как в API.

`VkOptions`:

```csharp
public sealed class VkOptions
{
    public const string SectionName = "Vk";

    public string AccessToken { get; set; } = string.Empty;
    public string ConfirmationToken { get; set; } = string.Empty;
    public string? Secret { get; set; }
    public long? GroupId { get; set; }
}
```

Токены не коммитятся: реальные значения задаются переменными окружения
`Vk__AccessToken`, `Vk__ConfirmationToken`, `Vk__Secret`, `Vk__GroupId`.

### 5.4. Callback

`VkBotController`:

```csharp
[ApiController]
[Route("api/VkBot")]
public sealed class VkBotController : ControllerBase
{
    [HttpPost("VkMessageAccept")]
    public async Task<IActionResult> VkMessageAccept(
        [FromBody] JsonElement update,
        CancellationToken cancellationToken)
    {
        // 1. Проверить Vk:Secret, если он задан.
        // 2. Разобрать update.
        // 3. confirmation -> вернуть ConfirmationToken как text/plain.
        // 4. message_new -> положить сообщение в очередь.
        // 5. Всегда быстро вернуть "ok" как text/plain.
    }
}
```

- Маршрут в точности `api/VkBot/VkMessageAccept`.
- `confirmation` возвращает ключ подтверждения.
- `message_new` не обрабатывается в запросе: сообщение кладётся в
  `VkUpdateQueue`, ответ `ok` уходит сразу.
- При заданном `Vk:Secret` запрос с пустым/чужим секретом получает `403`.
- Некорректный JSON — `400`.
- Ответы `ok` и ключ подтверждения — `Content(..., "text/plain")`, чтобы ВКонтакте
  получил ровно строку, а не JSON-строку.

### 5.5. Разбор обновлений

`VkCallbackParser` поддерживает два формата:

1. Современный Callback API:

```json
{
  "type": "message_new",
  "object": {
    "message": {
      "id": 1,
      "peer_id": 2,
      "from_id": 2,
      "text": "Consultation",
      "payload": "{\"command\":\"Consultation\"}"
    }
  },
  "group_id": 3
}
```

2. Старый плоский формат `VkBotLib`:

```json
{
  "type": "message_new",
  "object": {
    "peer_id": 2,
    "from_id": 2,
    "text": "Consultation",
    "payload": "{\"command\":\"Consultation\"}"
  }
}
```

Правила:

- `type` сравнивается без учёта регистра (`confirmation`, `message_new`);
- сообщение берётся из `object.message`, если оно есть, иначе из `object`;
- `peer_id` и `text` — из сообщения;
- `payload` может быть JSON-строкой или объектом; сохраняется как исходный JSON;
- `confirmation` не требует message.

`VkCommandParser`:

- payload имеет приоритет над текстом;
- из payload читается поле `command`;
- строка команды делится по `|`: первый элемент — имя, остальные — аргументы;
- имя команды сравнивается без учёта регистра, **аргументы не приводятся к нижнему
  регистру и не обрезаются по смыслу** (только `Trim` по краям);
- если payload нет, берётся текст: при наличии `|` — тот же формат, иначе весь
  текст считается именем команды;
- пустой/битый payload не роняет обработку: команда считается отсутствующей.

### 5.6. Очередь и обработчик

`VkUpdateQueue` — обёртка над неограниченным `Channel<VkIncomingMessage>`:

- `EnqueueAsync(message, ct)`;
- `ReadAllAsync(ct)`.

`VkUpdateProcessor : BackgroundService`:

- читает очередь;
- на каждое сообщение создаёт DI-scope;
- получает `VkBotHandler` и вызывает `HandleAsync`;
- ловит и логирует ошибки, чтобы одно сообщение не останавливало обработку;
- при остановке приложения корректно завершается по `stoppingToken`.

Очередь в памяти: при падении процесса необработанные сообщения теряются. Это
не хуже старого `Task.Factory.StartNew`, но и не лучше; гарантированная доставка
в задачу не входит.

### 5.7. Отправка сообщений

`IVkMessageSender`:

```csharp
Task SendTextAsync(
    long peerId,
    string text,
    MessageKeyboard? keyboard = null,
    CancellationToken cancellationToken = default);

Task SendPhotoAsync(
    long peerId,
    string photoUrl,
    CancellationToken cancellationToken = default);
```

`VkMessageSender` использует `VkNet.VkApi`:

- `SendTextAsync`:

```csharp
await api.Messages.SendAsync(new MessagesSendParams
{
    PeerId = peerId,
    Message = text,
    RandomId = Random.Shared.NextInt64(),
    Keyboard = keyboard,
    GroupId = (ulong)(options.GroupId ?? 0)
}, cancellationToken);
```

- `SendPhotoAsync`:
  1. проверить, что `photoUrl` — абсолютный `http/https` URI;
  2. скачать байты через `IHttpClientFactory`;
  3. `api.Photo.GetMessagesUploadServerAsync(options.GroupId, ct)`;
  4. отправить multipart-поле `photo`;
  5. `api.Photo.SaveMessagesPhotoAsync(response, ct)`;
  6. отправить сообщение с `Attachments = new MediaAttachment[] { photo }`
     (эквивалент вложения `photo{ownerId}_{id}`);
  7. любые ошибки скачивания/загрузки логировать и не пробрасывать: карточка
     фильма должна уйти даже без постера.

`MessagesSendParams.Keyboard` имеет тип `MessageKeyboard`, а `KeyboardBuilder.Build()`
возвращает именно `MessageKeyboard`; сериализацию делает VkNet.

### 5.8. Клавиатуры

`VkKeyboardFactory` строит `MessageKeyboard` через `KeyboardBuilder`.

Важно: нельзя использовать перегрузку
`AddButton(string label, string extra, KeyboardButtonColor color)`, потому что она
оборачивает `extra` в `{"b":"..."}`. Нужен явный action:

```csharp
var action = new MessageKeyboardButtonAction
{
    Type = KeyboardButtonActionType.Text,
    Label = label,
    Payload = payload // например, "SetAnswerAndNext|session|Q|Да"
};
builder.AddButton(action, KeyboardButtonColor.Positive);
```

Клавиатуры:

1. `CreateConsultationKeyboard()`:
   - одна кнопка `"Подобрать!"`, payload `"Consultation"`, цвет `Positive`.

2. `CreateRetryKeyboard()`:
   - одна кнопка `"Попробовать еще раз"`, payload `"Consultation"`, цвет `Positive`.

3. `CreateQuestionKeyboard(sessionId, question)`:
   - по одной кнопке на `question.Answers`;
   - label — `Display`, если он не пуст, иначе `Value`;
   - payload — `SetAnswerAndNext|{sessionId}|{question.Name}|{option.Value}`;
   - 3 кнопки в ряд (`AddLine()` после каждых 3);
   - не более 27 вариантов (9 рядов), чтобы вместе с рядом «Начать подбор заново»
     уложиться в лимит 10 рядов; лишние варианты логируются и отбрасываются;
   - отдельный последний ряд: `"Начать подбор заново"`, payload `"Consultation"`,
     цвет `Negative`;
   - цвет вариантов — `Secondary` (в новой `EsDb` нет поля цвета домена).

4. `CreateFilmKeyboard(sessionId, filmId)`:
   - ряд 1: `"Подобрать еще один фильм!"`, `Consultation`, `Positive`;
   - ряд 2: `"Другой по тем же параметрам"`,
     `Another|{sessionId}|{filmId}`, `Secondary`;
   - ряд 3: `"👎🏻"` с `Review|{sessionId}|Negative` и
     `"👍🏻"` с `Review|{sessionId}|Positive`, обе `Secondary`.

### 5.9. Тексты

`VkBotTexts` — константы:

```
Hello = "Привет! Я помогу подобрать фильм под ситуацию и настроение."
SearchingFilm = "Сейчас что-нибудь подберу..."
SearchingAnother = "Сейчас поищу..."
ThanksForReview = "Спасибо за оценку!"
StartOver = "Начать подбор заново"
TryAgain = "Попробовать еще раз"
NotFound = "К сожалению, по данным параметрам ничего не найдено"
Error = "К сожалению, в работе сервиса произошла ошибка. Вы можете повторить попытку позже."
```

`FilmMessageFormatter.Format(RecommendedFilm film)`:

```
{WebUtility.HtmlDecode(film.Name)} ({film.Year})

Осторожно, возможен машинный перевод!

{WebUtility.HtmlDecode(film.Description)}

Рейтинг IMDb: {film.Rating:N2}

Основываясь на мнении пользователей, этот фильм:
{Percent}% {Name}
...
```

- теги сортируются по `Percent` по убыванию;
- пустые значения заменяются пустой строкой, как в старом коде;
- формат `N2` сохраняется.

### 5.10. Команды

`IVkCommand`:

```csharp
public interface IVkCommand
{
    string Name { get; }
    Task ExecuteAsync(
        VkIncomingMessage message,
        VkCommandRequest request,
        CancellationToken cancellationToken);
}
```

`VkBotHandler`:

- строит `VkCommandRequest` из сообщения;
- ищет команду по имени без учёта регистра;
- неизвестную команду молча игнорирует (как старый `CommandContainer`);
- любое исключение команды логирует и отправляет пользователю `Error` +
  `CreateRetryKeyboard()`;
- сам не падает.

Команды:

1. `StartCommand` (`start`):
   - `SendTextAsync(peerId, Hello, CreateConsultationKeyboard())`.

2. `StartConsultationCommand` (`Consultation`):
   - `workflow.StartAsync(null, ct)`;
   - если `Status == Question` — отправить текст вопроса и клавиатуру вопроса;
   - если завершено — отправить фильм или `NotFound` + retry.

3. `SetAnswerAndNextCommand` (`SetAnswerAndNext`):
   - требует аргументы `sessionId`, `variable`, `value`; если их нет — ошибка;
   - `workflow.AnswerAsync(sessionId, variable, value, ct)`;
   - если вопрос — отправить вопрос и клавиатуру;
   - если завершено:
     - отправить `SearchingFilm`;
     - если фильм есть — постер (если URL валиден), затем карточка и клавиатура фильма;
     - если фильма нет — `NotFound` + retry.

4. `AnotherCommand` (`Another`):
   - требует `sessionId` (второй аргумент `filmId` сохраняется в payload, но
     workflow его не использует);
   - отправить `SearchingAnother`;
   - `workflow.GetAnotherAsync(sessionId, ct)`;
   - если фильм есть — постер, карточка, клавиатура фильма;
   - `KeyNotFoundException` (кандидаты закончились) — `NotFound` + retry.

5. `ReviewCommand` (`Review`):
   - требует `sessionId` и оценку;
   - `Positive`/`Negative` сравниваются без учёта регистра;
   - `liked = оценка == Positive`;
   - `comment = $"{DateTime.Now:dd.MM.yyyy HH:mm:ss} - {canonicalEstimation}"`;
   - `workflow.ReviewAsync(sessionId, liked, comment, ct)`;
   - если сессия не найдена — залогировать и всё равно отправить благодарность,
     как старый бот;
   - отправить `ThanksForReview` + `CreateConsultationKeyboard()`.

## 6. Очистка решения и README

### 6.1. Удаление `WebApi`

Удаляется каталог `EsService/WebApi` целиком, включая `bin`, `obj`, `App_Data`,
`Web.config`, `.csproj` и `.user`.

Проверить, что на `WebApi` больше нет ссылок:

```
rg "WebApi" --glob "*.csproj" --glob "*.sln"
rg "VkBotLib" --glob "*.csproj" --glob "*.sln"
```

### 6.2. `EsService/ЭС.sln`

- удалить проект `WebApi` (`{D5ED968B-418B-47F5-A427-CDADECA41DDF}`):
  - `Project...EndProject`;
  - все строки `ProjectConfigurationPlatforms`;
  - строку в `NestedProjects`;
- удалить проект `VkBotLib` (`{22FEB85F-5291-4838-BE56-1B9849A2C8F9}`)
  аналогично;
- добавить `VkBotApi`:

```
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "VkBotApi", "..\VkBotApi\VkBotApi.csproj", "{1BDA6E9E-DFD8-4141-82BC-C302DFD9E708}"
EndProject
```

- добавить конфигурации `Debug/Release` для `Any CPU`, `x64`, `x86`
  (по образцу `EsConsultationApi`);
- `EsService\Logic` и папка `EsService` остаются.

### 6.3. README

- в разделе `EsConsultationServices` добавить:
  - `IConsultationWorkflow` — общий сценарий консультации;
  - `ConsultationExpertSystemResolver`, `ConsultationShownFilmStore`,
    `ConsultationReviewStore`.
- раздел `WebApi` заменить на `VkBotApi`:
  - назначение — VK-бот;
  - callback `POST /api/VkBot/VkMessageAccept`;
  - команды и payload;
  - зависимости `EsConsultationServices`, `EsDb`, `FilmDb`, VkNet;
  - настройки `Vk` и строки подключения;
  - запуск `dotnet run --project VkBotApi`.
- удалить подраздел `VkBotLib`.
- в разделе `Logic` отметить, что он остаётся в решении, но новый бот его не
  использует.

## 7. Тесты

### 7.1. `EsConsultationApi.Tests`

Обновить под `IConsultationWorkflow` (раздел 4.5) и прогнать:

```
dotnet test EsConsultationApi.Tests/EsConsultationApi.Tests.csproj
```

### 7.2. Новый `VkBotApi.Tests`

`VkBotApi.Tests/VkBotApi.Tests.csproj`:

- `net7.0`, xunit, `Microsoft.NET.Test.Sdk`, `xunit.runner.visualstudio`;
- ссылки на `VkBotApi` и `EsConsultationServices`.

Тесты:

1. `CallbackParser_reads_modern_object_message` — `object.message`.
2. `CallbackParser_reads_legacy_flat_object` — старый формат.
3. `CallbackParser_reads_confirmation`.
4. `CommandParser_prefers_payload_over_text`.
5. `CommandParser_preserves_argument_case` — например,
   `SetAnswerAndNext|sid|Настроение|Хочу Посмеяться` не меняет регистр.
6. `QuestionKeyboard_has_three_buttons_per_row_and_reset` — payload и раскладка.
7. `FilmKeyboard_has_expected_payloads` — `Consultation`, `Another|...`, `Review|...`.
8. `FilmFormatter_matches_old_card` — имя, год, предупреждение, рейтинг, теги.
9. `StartCommand_sends_hello_and_consultation_keyboard`.
10. `ConsultationCommand_sends_first_question`.
11. `AnswerCommand_sends_film_card_and_keyboard` — проверяет
    `"Сейчас что-нибудь подберу..."`, постер, карточку, клавиатуру.
12. `AnotherCommand_sends_next_film`.
13. `ReviewCommand_maps_positive_and_negative` — `true/false` и формат комментария.
14. `Handler_sends_error_on_exception`.
15. `Handler_ignores_unknown_command`.

Для тестов без VK и БД:

- `FakeWorkflow : IConsultationWorkflow` — возвращает заранее заданные результаты
  и записывает вызовы;
- `RecordingMessageSender : IVkMessageSender` — записывает тексты, клавиатуры и
  URL постеров;
- `NullLogger<T>`.

### 7.3. Проверка всех тестов

```
dotnet test EsConsultationApi.Tests/EsConsultationApi.Tests.csproj
dotnet test VkBotApi.Tests/VkBotApi.Tests.csproj
dotnet test EsServices.Tests/EsServices.Tests.csproj
```

## 8. Порядок шагов

- [x] 1. Создать `EsConsultationServices/Workflow`: контракты, настройки, резолвер,
      хранилища, workflow.
- [x] 2. Зарегистрировать workflow в `AddEsConsultationServices`, добавить
      `Microsoft.Extensions.Options`; собрать `EsConsultationServices`.
- [x] 3. Перевести `EsConsultationApi` на `IConsultationWorkflow`, добавить маппер,
      удалить перенесённые файлы; собрать API.
- [x] 4. Обновить `EsConsultationApi.Tests`; прогнать тесты.
- [x] 5. Создать `VkBotApi`: проект, `Program.cs`, `appsettings*.json`,
      `VkOptions`.
- [x] 6. Реализовать callback, контракты обновления, парсеры, очередь и
      `VkUpdateProcessor`.
- [x] 7. Реализовать `VkMessageSender`, загрузку постера, `VkKeyboardFactory`,
      `FilmMessageFormatter`, тексты.
- [x] 8. Реализовать команды, `VkBotHandler`, регистрацию DI; собрать `VkBotApi`.
- [x] 9. Создать `VkBotApi.Tests`, реализовать фейки и тесты; прогнать.
- [x] 10. Удалить `EsService/WebApi`, обновить `EsService/ЭС.sln`.
- [x] 11. Обновить `README.md`.
- [x] 12. Собрать все net7-проекты и прогнать все тесты.
- [x] 13. Проверить отсутствие ссылок на `WebApi`, `VkBotLib`, `ExpertSystemDb`,
      `Logic` в новых проектах.

## 9. Как проверять

### Сборка

```
dotnet build EsConsultationServices/EsConsultationServices.csproj
dotnet build EsConsultationApi/EsConsultationApi.csproj
dotnet build VkBotApi/VkBotApi.csproj
```

Полная сборка `EsService/ЭС.sln` может падать на старых .NET Framework проектах;
это не регрессия задачи.

### Тесты

```
dotnet test EsConsultationApi.Tests/EsConsultationApi.Tests.csproj
dotnet test VkBotApi.Tests/VkBotApi.Tests.csproj
dotnet test EsServices.Tests/EsServices.Tests.csproj
```

### Callback вручную

```
curl -X POST http://localhost:5000/api/VkBot/VkMessageAccept ^
  -H "Content-Type: application/json" ^
  -d "{\"type\":\"confirmation\"}"
```

Ожидается ключ подтверждения из `Vk:ConfirmationToken`.

```
curl -X POST http://localhost:5000/api/VkBot/VkMessageAccept ^
  -H "Content-Type: application/json" ^
  -d "{\"type\":\"message_new\",\"object\":{\"message\":{\"peer_id\":1,\"text\":\"start\"}}}"
```

Ожидается `ok`; в очереди появляется сообщение, бот отправляет приветствие
(если задан рабочий VK-токен).

### Ручной сценарий в VK

1. `start` — приветствие и кнопка «Подобрать!».
2. «Подобрать!» — первый вопрос.
3. Ответы на вопросы — следующие вопросы.
4. Завершение — «Сейчас что-нибудь подберу...», постер, карточка фильма,
   кнопки «Другой по тем же параметрам», «👍🏻», «👎🏻».
5. «Другой по тем же параметрам» — «Сейчас поищу...» и другой фильм.
6. «👍🏻»/«👎🏻» — «Спасибо за оценку!» и запись в `EsDb.Reviews`
   (`Estimate = 10/0`, `Comment = "dd.MM.yyyy HH:mm:ss - Positive/Negative"`).

## 10. Узкие и неочевидные места

1. **Регистр аргументов.** Старый `ClearMessage` приводил весь payload к нижнему
   регистру, из-за чего значения домена могли не совпадать. Новый парсер
   приводит к нижнему регистру только имя команды; `variable` и `value`
   передаются в движок как есть.
2. **Payload в VkNet.** `AddButton(label, extra, color)` оборачивает `extra` в
   `{"b":"..."}`. Нужен `MessageKeyboardButtonAction` с `Payload`, иначе бот
   перестанет понимать собственные кнопки.
3. **Лимиты клавиатуры.** VK допускает не более 5 кнопок в ряду и 10 рядов.
   Варианты ответа раскладываются по 3, ряд «Начать подбор заново» занимает
   последний ряд; поэтому вариантов не больше 27. При большем домене лишние
   отбрасываются с предупреждением в лог.
4. **Цвета вариантов.** В новой `EsDb` у `DomainValue` нет цвета и флага новой
   строки, поэтому цвета вариантов нейтральные, а раскладка фиксированная.
   Остальные кнопки сохраняют прежние цвета.
5. **`Another` и `filmId`.** Старый payload содержал Id фильма, но новый workflow
   выбирает следующий фильм по совету и показанным. Второй аргумент принимается
   для совместимости и игнорируется.
6. **Оценка.** `ConsultationReviewStore` принимает `bool`, а старый бот писал в
   `Comment` дату и строку оценки. Команда `Review` сама формирует
   `"{date} - Positive/Negative"`, поэтому формат сохраняется.
7. **Сессия не найдена при оценке.** Старый бот молча пропускал запись и всё равно
   благодарил. Новый делает так же: `KeyNotFoundException` логируется, ответ
   «Спасибо за оценку!» отправляется.
8. **Нет фильма.** Если по совету ничего не найдено, приходит `NotFound` + retry;
   при «другом фильме» кандидаты могут закончиться — тоже `NotFound` + retry,
   вместо падения, как в старом `AnotherCommand`.
9. **Постер.** Ошибка скачивания/загрузки не должна мешать карточке: она
   логируется, текст уходит без вложения.
10. **Секрет callback.** `Vk:Secret` необязателен. Если он задан, чужие запросы
    получают `403`; если пуст, проверка пропускается.
11. **`GroupId`.** В `MessagesSendParams.GroupId` тип `ulong`; в настройках
    хранится `long?`, поэтому передаётся `(ulong)(GroupId ?? 0)`.
12. **Дубли обновлений.** VK может повторить callback, если не получил `ok`.
    Очередь возвращает `ok` сразу, но при падении между постановкой в очередь и
    ответом возможен повтор. Дедупликация по `event_id` в задачу не входит;
    старый бот вёл себя так же.
13. **Фоновая обработка и DI.** `VkUpdateProcessor` — singleton, поэтому на каждое
    сообщение создаётся scope и из него берутся scoped workflow/команды.
14. **Строки подключения.** Для запуска нужны доступные `EsDb` и `FilmsDb`;
    без них собираются проект и тесты на фейках, но бот не проведёт консультацию.
15. **Полная сборка решения.** `EsService/ЭС.sln` содержит старые .NET Framework
    проекты; проверяются net7-проекты по отдельности.
