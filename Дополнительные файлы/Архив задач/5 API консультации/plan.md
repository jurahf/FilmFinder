# План реализации: EsConsultationApi

Связанная постановка: `task.md`.

## 1. Итоговая архитектура

```
EsConsultationApi (ASP.NET Core Web API, net7.0)
  ├─ EsConsultationServices
  │    ├─ Engine/            — движок консультации (без изменений)
  │    ├─ Persistence/       — состояние сессии в EsDb (без изменений)
  │    ├─ Services/          — ConsultationService (без изменений)
  │    └─ Films/             — НОВОЕ: подбор фильма и описания советов
  │         ├─ FilmRecommendationService
  │         └─ AdviceConsultationDisplayProvider
  ├─ EsDb                   — оценки и таблица показанных фильмов
  └─ FilmDb                 — советы, свойства, фильмы

EsServices (сервисы данных и ViewModel для EsUi)
  ├─ Services/Advice/AdviceCatalogService      — остаётся, описания для UI
  ├─ Services/Genres/GenresService             — НОВОЕ, справочник для UI
  ├─ Services/CustomProperties/...             — НОВОЕ, справочник для UI
  └─ удаляются FilmRecommendationService и FilmResultVM

EsUi
  ├─ Program.cs — регистрации сервисов
  ├─ Consultation.razor — использует сервисы из EsConsultationServices/EsServices
  └─ FilmResultCard.razor — принимает RecommendedFilm
```

Принципы разделения:

- `EsServices` — доступ к данным, справочники и ViewModel, нужные `EsUi`;
- `EsConsultationServices` — логика консультации, советов и подбора фильма;
- `FilmDb` — модель и репозитории, в том числе единое построение описаний советов;
- `EsConsultationApi` — только HTTP-слой, оркестрация и хранение показов/оценок.

## 2. Изменения в базе данных (`EsDb`)

### Новая сущность `ConsultationShownFilm`

Файл `EsDb/Model/ConsultationShownFilm.cs`:

- `Id` — PK (наследуется от `BaseEntity`);
- `ConsultationId` — FK на `Consultations`, cascade delete;
- `FilmId` — Id фильма из `FilmsDb`, без FK между базами;
- `Order` — порядковый номер показа, начиная с 0;
- `ShownDate` — дата и время показа.

Назначение: хранить, какие фильмы уже показаны в консультации, чтобы «другой ответ»
не повторялся после перезапуска и при нескольких экземплярах API.

### Изменения `Consultation`

Файл `EsDb/Model/Consultation.cs`:

- добавить навигацию `ICollection<ConsultationShownFilm> ShownFilms`.

### Изменения `EsDbContext`

Файл `EsDb/EsDbContext.cs`:

- добавить `DbSet<ConsultationShownFilm> ConsultationShownFilms`;
- настроить связи и индексы:
  - `(ConsultationId, FilmId)` — уникальный;
  - `(ConsultationId, Order)` — уникальный;
  - FK на `Consultation` — cascade delete.

### Миграция

- Сгенерировать миграцию `ConsultationShownFilms`:
  ```
  dotnet ef migrations add ConsultationShownFilms --project EsDb --startup-project EsDb
  ```
- Проверить, что в миграции только новая таблица, индексы и FK.
- Применить к локальной базе:
  ```
  dotnet ef database update --project EsDb --startup-project EsDb
  ```
- Если установленная версия `dotnet ef` несовместима с EF Core 7, поставить локальный
  инструмент версии 7.0.16 через tool manifest и повторить команды.

## 3. Изменения в `FilmDb`

### `AdviceRepository` / `IAdviceRepository`

Файл `FilmDb/Repositories/AdviceRepository.cs`:

- добавить интерфейс `IAdviceRepository : IRepository<Advice>`;
- добавить метод
  `Task<IReadOnlyDictionary<string, string>> GetDescriptionsAsync(CancellationToken ct = default)`;
- реализация использует существующий `Fetch` (включает `AdviceFilm.Film` и
  `AdviceCustomProperty.CustomProperty`) и строит описание совета;
- текущее построение описания из `EsServices.Services.Advice.AdviceCatalogService`
  переносится сюда, чтобы не дублировать формат текста.

`AdviceRepository` продолжает реализовывать `IRepository<Advice>` для `FilmsUi`.

## 4. Изменения в `EsConsultationServices`

### Файл проекта

`EsConsultationServices/EsConsultationServices.csproj`:

- добавить `ProjectReference` на `..\FilmDb\FilmDb.csproj`.

### Новые файлы в `EsConsultationServices/Films/`

1. `FilmRecommendationContracts.cs` — UI-независимые DTO:
   - `RecommendedFilm` — Id, Name, Year, Rating, Description, Slogan, Poster, Link,
     KinopoiskId, Genres, Countries, Actors, Producers, Tags;
   - `FilmTag` — Name, Percent.

2. `IFilmRecommendationService.cs`:
   ```
   Task<RecommendedFilm?> FindByAdviceAsync(
       Guid adviceKey,
       IReadOnlyCollection<int>? excludedFilmIds = null,
       CancellationToken ct = default);

   Task<RecommendedFilm?> FindByFiltersAsync(
       IReadOnlyCollection<int> genreIds,
       IReadOnlyCollection<int> customPropertyIds,
       IReadOnlyCollection<int>? excludedFilmIds = null,
       CancellationToken ct = default);
   ```
   Методов `GetGenresAsync`/`GetCustomPropertiesAsync` здесь нет: это данные для UI,
   они остаются в `EsServices`.

3. `FilmRecommendationService.cs` — перенос логики `FindByAdviceAsync`/`FindByFiltersAsync`
   из `EsServices`:
   - `FindByAdviceAsync`:
     1. загружает совет с `AdviceFilm.Film` и `AdviceCustomProperty.CustomProperty`;
     2. сначала возвращает явно указанный фильм совета (`AdviceFilm`) с наибольшей
        степенью соответствия и рейтингом, исключая уже показанные;
     3. когда явные фильмы совета закончились — строит кандидатов по вектору свойств:
        отбрасывает конфликтующие знаки, считает квадратичное расстояние, сортирует
        по расстоянию и рейтингу;
     4. из лучших кандидатов выбирает фильм, не входящий в `excludedFilmIds`;
     5. загружает полные данные фильма;
   - `FindByFiltersAsync` — прежняя логика с добавленным исключением показанных;
   - `Map` возвращает `RecommendedFilm`.

4. `AdviceConsultationDisplayProvider.cs` — перенос из `EsServices`;
   зависит от `IAdviceRepository` и подставляет описания советов в домен `#Советы`.

### `ServiceCollectionExtensions`

Файл `EsConsultationServices/ServiceCollectionExtensions.cs`:

- добавить регистрации через `TryAddScoped`:
  - `IAdviceRepository` -> `FilmDb.Repositories.AdviceRepository`;
  - `IFilmRecommendationService` -> `FilmRecommendationService`.
- провайдер `IConsultationDisplayProvider` по умолчанию остаётся
  `DefaultConsultationDisplayProvider`: приложение, которому нужны описания советов,
  регистрирует `AdviceConsultationDisplayProvider` явно. Это позволяет тестам движка
  работать без `FilmDbContext`.

## 5. Изменения в `EsServices`

### Остаётся

- `Services/Advice/AdviceCatalogService.cs` — UI-сервис описаний советов; теперь
  делегирует `IAdviceRepository.GetDescriptionsAsync` и не строит текст сам;
- `Services/Advice/IAdviceCatalogService.cs`;
- `Services/Advice/AdviceSyncService.cs` — без изменений;
- `ViewModel/Film/FilmLookupItemVM.cs` — вынести в отдельный файл;
- `ViewModel/Film/FilmResultVM.cs` — удалить (`FilmResultVM`, `FilmTagVM`);
- `Services/Consultation/IFilmRecommendationService.cs` — удалить;
- `Services/Consultation/FilmRecommendationService.cs` — удалить;
- `Services/Consultation/AdviceConsultationDisplayProvider.cs` — удалить.

### Создать

- `Services/Genres/IGenresService.cs`:
  `Task<List<FilmLookupItemVM>> GetListAsync(CancellationToken ct = default);`
- `Services/Genres/GenresService.cs` — читает `FilmDbContext.Genres`, сортирует по имени,
  возвращает `FilmLookupItemVM`.
- `Services/CustomProperties/ICustomPropertiesService.cs` и
  `CustomPropertiesService.cs` — аналогично для `CustomProperties`.

### Изменить

- `EsServices/EsServices.csproj` — ссылка на `EsConsultationServices` уже есть;
- `EsUi/Program.cs` — регистрации (см. раздел 6);
- `_Imports.razor` — добавить `@using EsConsultationServices.Films` и
  `@using EsServices.Services.Genres`, `@using EsServices.Services.CustomProperties`.

## 6. Изменения в `EsUi`

### `EsUi/Program.cs`

- оставить регистрацию `IAdviceCatalogService -> AdviceCatalogService`;
- добавить `IGenresService -> GenresService`, `ICustomPropertiesService -> CustomPropertiesService`;
- убрать регистрацию `EsServices...IFilmRecommendationService -> FilmRecommendationService`;
- добавить `using EsConsultationServices.Films;` и регистрацию
  `IConsultationDisplayProvider -> AdviceConsultationDisplayProvider` из
  `EsConsultationServices`;
- оставить `AddEsConsultationServices()` — он регистрирует `IFilmRecommendationService`,
  `IAdviceRepository` и кэш графа.

### `EsUi/Pages/Consultation.razor`

- `@inject EsConsultationServices.Films.IFilmRecommendationService FilmRecommendationService`;
- `@inject IGenresService GenresService`;
- `@inject ICustomPropertiesService CustomPropertiesService`;
- `private RecommendedFilm? film;`
- `ChooseWantsAsync`: `genres = await GenresService.GetListAsync();`
  `customProperties = await CustomPropertiesService.GetListAsync();`
- вызовы `FindByAdviceAsync`/`FindByFiltersAsync` остаются, но возвращают
  `RecommendedFilm?`.

### `EsUi/Components/FilmResultCard.razor`

- `@using EsConsultationServices.Films`;
- параметр `RecommendedFilm Film`.

## 7. Новый проект `EsConsultationApi`

### Файл проекта

`EsConsultationApi/EsConsultationApi.csproj`:

- `Sdk="Microsoft.NET.Sdk.Web"`;
- `TargetFramework` — `net7.0`;
- `ImplicitUsings` — `enable`;
- `Nullable` — `enable`;
- пакеты:
  - `Microsoft.EntityFrameworkCore.SqlServer` 7.0.16;
  - `Swashbuckle.AspNetCore` 6.5.0;
- ссылки:
  - `..\EsConsultationServices\EsConsultationServices.csproj`;
  - `..\EsDb\EsDb.csproj`;
  - `..\FilmDb\FilmDb.csproj`.

Добавить проект в `EsService/ЭС.sln`.

### Файлы

```
EsConsultationApi/
  EsConsultationApi.csproj
  Program.cs
  appsettings.json
  appsettings.Development.json
  Controllers/ConsultationsController.cs
  Contracts/ConsultationRequests.cs
  Contracts/ConsultationResponse.cs
  Contracts/FilmDto.cs
  Options/ConsultationOptions.cs
  Services/ConsultationWorkflow.cs
  Services/ConsultationShownFilmStore.cs
  Services/ConsultationReviewStore.cs
  Services/ConsultationExpertSystemResolver.cs
  Mapping/FilmMapper.cs
  Infrastructure/ApiExceptionMiddleware.cs
```

### `Program.cs`

- читает строки подключения `EsConnection` и `FilmConnection` (как `EsUi`);
- `AddDbContext<EsDbContext>` и `AddDbContext<FilmDbContext>`;
- `AddControllers()`, `AddEndpointsApiExplorer()`, `AddSwaggerGen()`;
- `AddScoped<IConsultationDisplayProvider, AdviceConsultationDisplayProvider>()`;
- `AddEsConsultationServices()`;
- `AddScoped<ConsultationExpertSystemResolver>()`;
- `AddScoped<ConsultationShownFilmStore>()`;
- `AddScoped<ConsultationReviewStore>()`;
- `AddScoped<ConsultationWorkflow>()`;
- middleware обработки исключений;
- `UseSwagger()`/`UseSwaggerUI()` только в Development;
- `MapControllers()`.

`appsettings.json`:

- `EsConnection` — `EsDb`;
- `FilmConnection` — `FilmsDb`;
- `Consultation:ExpertSystemName` — `"Фильмы"`;
- `Consultation:ExpertSystemId` — `0` (если задан, имеет приоритет над именем).

### Контракты API

`StartConsultationRequest`:

```
{
  "expertSystemId": 1   // необязательно
}
```

`AnswerRequest`:

```
{
  "variableName": "Настроение",
  "value": "Хочу посмеяться"
}
```

`ReviewRequest`:

```
{
  "liked": true,
  "comment": "необязательно"
}
```

`ConsultationResponse`:

- `sessionId`;
- `expertSystemId` / `expertSystemName`;
- `status` — `"Question" | "Completed" | "Unknown"`;
- `question` — `{ name, text, answers: [{ value, display }] }`;
- `result` — `{ variableName, value, display, isUnknown, explanation }`
  (тип `EsConsultationServices.Contracts.ConsultationResult`);
- `film` — `FilmDto` (только если результат — совет и фильм подобран).

`FilmDto`:

- Id, Name, Year, Rating, Description, Slogan, Poster, Link, KinopoiskId;
- Genres, Countries, Actors, Producers;
- Tags — `[{ name, percent }]`.

### `ConsultationsController`

| Метод | Маршрут | Назначение |
|---|---|---|
| POST | `/api/consultations` | старт консультации, первый вопрос |
| GET | `/api/consultations/{sessionId}` | текущий вопрос/результат |
| POST | `/api/consultations/{sessionId}/answers` | ответ на вопрос |
| POST | `/api/consultations/{sessionId}/another` | другой фильм по тому же результату |
| POST | `/api/consultations/{sessionId}/review` | оценка «понравилось / нет» |

Коды ответов:

- старт/ответ/текущий шаг/другой фильм — `200 OK`;
- оценка — `204 No Content`;
- `400` — некорректный запрос;
- `404` — сессия/консультация не найдены, больше нет подходящих фильмов;
- `409` — консультация не завершена, результат не является советом, ответ не к текущему
  вопросу, попытка ответить в завершённую консультацию.

### `ConsultationWorkflow`

Основная бизнес-логика HTTP-слоя:

- `StartAsync(int? expertSystemId, ct)`:
  - через `ConsultationExpertSystemResolver` получить Id ЭС (запрос или конфиг);
  - вызвать `IConsultationService.StartAsync`;
  - вернуть `sessionId` и первый шаг.
- `GetCurrentAsync(sessionId, ct)`:
  - `IConsultationService.GetCurrentAsync`;
  - заполнить `expertSystemId`/`expertSystemName` из `EsDb` для полноты ответа.
- `AnswerAsync(sessionId, request, ct)`:
  - `IConsultationService.AnswerAsync(sessionId, variableName, value)`;
  - если вернулся вопрос — вернуть его;
  - если вернулся результат — вернуть результат, а для известного совета подобрать
    и зарегистрировать фильм (см. ниже).
- `GetAnotherAsync(sessionId, ct)`:
  - текущий шаг должен быть завершённым известным результатом;
  - если результат не совет — `409`;
  - подобрать следующий фильм, исключив показанные;
  - если кандидатов нет — `404`;
  - зарегистрировать показ и вернуть фильм.
- `ReviewAsync(sessionId, request, ct)`:
  - сохранить `EsDb.Model.Review` через `ConsultationReviewStore`;
  - `Estimate = 10` для `liked=true`, `0` для `liked=false`;
  - вернуть `204`.

Подбор и регистрация фильма:

```
for attempt in 0..2:
    shown = await shownStore.GetIdsAsync(sessionId)
    film = await films.FindByAdviceAsync(adviceKey, shown)
    if film == null: return null
    if await shownStore.TryAddAsync(sessionId, film.Id): return film
# конфликт уникального индекса — повторить с обновлённым списком
```

`ConsultationShownFilmStore`:

- `GetIdsAsync(sessionId, ct)` — находит `Consultation` по `Session.SessionId`,
  читает `ShownFilms.FilmId`;
- `TryAddAsync(sessionId, filmId, ct)` — в транзакции вычисляет следующий `Order`,
  добавляет строку; при `DbUpdateException` возвращает `false` (уникальный индекс),
  чтобы workflow повторил выбор.

`ConsultationReviewStore`:

- находит `Session` по `SessionId`;
- добавляет `Review { SessionId, Estimate, Comment }`;
- если сессии нет — `KeyNotFoundException`.

`ConsultationExpertSystemResolver`:

- `ResolveAsync(int? expertSystemId, ct)`;
- приоритет: явный Id -> `ConsultationOptions.ExpertSystemId` (> 0) ->
  поиск по `ConsultationOptions.ExpertSystemName` в `EsDb`;
- если ничего не найдено — `KeyNotFoundException` с понятным сообщением.

### `ApiExceptionMiddleware`

Маппинг исключений в `ProblemDetails`:

- `KeyNotFoundException` -> `404`;
- `ArgumentException` -> `400`;
- `InvalidOperationException` -> `409` (завершённая консультация, чужой вопрос,
  отсутствующее значение домена);
- `DbUpdateException` -> `409`;
- остальные -> `500`.

В `detail` — сообщение исключения; в `traceId` — `HttpContext.TraceIdentifier`.

## 8. Тесты

Новый проект `EsConsultationApi.Tests` (net7.0, xunit):

- ссылки: `EsConsultationApi`, `EsConsultationServices`, `EsDb`, `FilmDb`;
- пакеты: `Microsoft.EntityFrameworkCore.Sqlite` 7.0.16, `Microsoft.NET.Test.Sdk`,
  `xunit`, `xunit.runner.visualstudio`.

Тестовый хост:

- две SQLite in-memory базы (`EsDb` и `FilmsDb`) на отдельных соединениях;
- `EnsureCreatedAsync` для обеих;
- DI: оба `DbContext`, `AddEsConsultationServices()`,
  `IConsultationDisplayProvider -> AdviceConsultationDisplayProvider`,
  `ConsultationShownFilmStore`, `ConsultationReviewStore`, `ConsultationWorkflow`;
- `RestartAsync` пересоздаёт `ServiceProvider` на тех же соединениях (имитация
  перезапуска).

Тестовые данные `FilmsDb`:

- свойства `Смешное`, `Мрачное`;
- совет `GUID` с явными фильмами A (степень 100) и B (90) и свойствами
  `Смешное = 80`, `Мрачное = -50`;
- фильмы C/D/E, похожие по свойствам, для fallback-подбора.

Тестовые данные `EsDb`:

- ЭС с вопросом Q (Да/Нет) и правилом `Q = Да -> Совет = <GUID>`;
- цель типа `Detective`.

Тесты:

1. `Start_returns_session_and_first_question`.
2. `Answer_completes_and_returns_result_with_film` — фильм A, описание совета,
   запись в `ConsultationShownFilms`.
3. `Another_returns_next_film` — фильм B, затем фильм из fallback; все Id различны.
4. `Another_after_restart_does_not_repeat` — новый `ServiceProvider` на тех же БД.
5. `Another_when_candidates_exhausted_returns_not_found`.
6. `Review_saves_estimate` — `10` и `0` в `EsDb.Reviews`.
7. `FilmRecommendation_returns_explicit_advice_film_first`.
8. `FilmRecommendation_falls_back_to_similar_films`.
9. `FilmRecommendation_excludes_shown_films`.
10. `AdviceDisplayProvider_returns_human_readable_description`.

Дополнительно:

- прогнать существующий `EsServices.Tests`, чтобы переносы не сломали движок;
- проверить, что `AddEsConsultationServices` не требует `FilmDbContext` при разрешении
  только `IConsultationService`;
- проверить сборку `EsUi` после смены типов в `Consultation.razor` и `FilmResultCard`.

## 9. README

Добавить раздел `EsConsultationApi`:

- назначение — HTTP API консультаций;
- зависимости — `EsConsultationServices`, `EsDb`, `FilmDb`;
- список методов и краткий пример сценария;
- как запустить и открыть Swagger;
- как настроить строки подключения и имя/Id экспертной системы.

## 10. Порядок шагов для агента

- [x] 1. Добавить `ConsultationShownFilm`, навигацию в `Consultation`, настройки
      `EsDbContext`.
- [x] 2. Сгенерировать и проверить миграцию `ConsultationShownFilms`, применить её
      к локальной БД.
- [x] 3. Добавить `IAdviceRepository.GetDescriptionsAsync` в `FilmDb`, перенести туда
      построение описания совета.
- [x] 4. Перевести `EsServices.AdviceCatalogService` на `IAdviceRepository`.
- [x] 5. Добавить ссылку на `FilmDb` в `EsConsultationServices`.
- [x] 6. Создать `EsConsultationServices/Films`: DTO, интерфейс и сервис подбора,
      провайдер описаний советов; зарегистрировать в `AddEsConsultationServices`.
- [x] 7. Удалить из `EsServices` `FilmRecommendationService`,
      `IFilmRecommendationService`, `AdviceConsultationDisplayProvider`,
      `FilmResultVM`/`FilmTagVM`.
- [x] 8. Добавить `GenresService`/`IGenresService` и
      `CustomPropertiesService`/`ICustomPropertiesService` в `EsServices`.
- [x] 9. Обновить `EsUi`: `Program.cs`, `_Imports.razor`, `Consultation.razor`,
      `FilmResultCard.razor`.
- [x] 10. Собрать `EsConsultationServices`, `EsServices`, `EsUi`, прогнать
      `EsServices.Tests`.
- [x] 11. Создать `EsConsultationApi`, добавить в решение, настроить `Program.cs`,
      конфигурацию и Swagger.
- [x] 12. Реализовать контракты, контроллер, workflow, хранилища показанных фильмов
      и оценок, резолвер ЭС и middleware ошибок.
- [x] 13. Создать `EsConsultationApi.Tests`, реализовать тестовый хост и тесты.
- [x] 14. Собрать API, прогнать тесты.
- [x] 15. Обновить `README.md`.
- [x] 16. Проверить Swagger вручную на локальных `EsDb`/`FilmsDb`.

## 11. Как проверять

### Сборка

```
dotnet build EsConsultationServices/EsConsultationServices.csproj
dotnet build EsServices/EsServices.csproj
dotnet build EsUi/EsUi.csproj
dotnet build EsConsultationApi/EsConsultationApi.csproj
```

Полная сборка `EsService/ЭС.sln` по-прежнему не пройдёт из-за старых `WebApi`
и `LoadFilms`; это не регрессия задачи.

### Тесты

```
dotnet test EsServices.Tests/EsServices.Tests.csproj
dotnet test EsConsultationApi.Tests/EsConsultationApi.Tests.csproj
```

### Миграция

```
dotnet ef migrations script --project EsDb --startup-project EsDb
```

В скрипте должна появиться только таблица `ConsultationShownFilms` с индексами и FK.

### Ручной сценарий

1. Запустить `EsConsultationApi`, открыть `/swagger`.
2. `POST /api/consultations` — получить `sessionId` и вопрос.
3. Отвечать через `POST /api/consultations/{sessionId}/answers`.
4. На завершающем ответе получить `result` с `display` совета и `film`.
5. `POST /api/consultations/{sessionId}/another` — получить другой фильм; повторить
   несколько раз до исчерпания.
6. `POST /api/consultations/{sessionId}/review` с `liked=true/false`.
7. Проверить в БД:
   - `Consultations`, `ConsultationAnswers`, `FinalSolutions`;
   - `ConsultationShownFilms` — показанные фильмы без повторов;
   - `Reviews` — оценки по `SessionId`.

## 12. Узкие и неочевидные места

1. **`AdviceFilm` — основные результаты.** В старом алгоритме явные фильмы совета
   игнорировались. Теперь они идут первыми (по `Value` и рейтингу), а векторный подбор —
   fallback. Это соответствует смыслу «несколько фильмов или чуть менее подходящие».
2. **Разные базы.** `EsDb` и `FilmsDb` — отдельные базы, между ними нет FK и общих
   транзакций. Сначала читается `FilmsDb`, затем пишется `EsDb`. Ошибка записи показов
   возвращается клиенту, повторный запрос безопасен благодаря уникальному индексу.
3. **Гонка при «другом ответе».** Два одновременных запроса могут выбрать один фильм.
   Уникальный индекс `(ConsultationId, FilmId)` и повторный выбор с обновлённым списком
   показанных предотвращают дубль.
4. **Порядок `Order`.** Вычисляется как `max + 1`; при конфликте уникального индекса
   workflow повторяет попытку. Полноценная optimistic concurrency не требуется.
5. **Кэш графа ЭС.** Описания советов попадают в кэш вместе с графом
   (`ConsultationExpertSystemCache`, TTL 1/24 часа). Изменения описаний в `FilmsDb`
   подхватятся после TTL или `Invalidate`.
6. **`EsServices.Tests` без `FilmDb`.** `AddEsConsultationServices` регистрирует
   film-сервисы и `IAdviceRepository`, но не разрешает их, а провайдер по умолчанию
   остаётся `DefaultConsultationDisplayProvider`. Поэтому тестам движка не нужен
   `FilmDbContext`.
7. **Совет без свойств и без явных фильмов.** Подбор вернёт `null`; API вернёт
   результат без фильма (при первом ответе) или `404` (при «другом ответе»).
8. **Результат не GUID.** Если ЭС вернула не совет (например, unknown или другую
   переменную), API вернёт результат без фильма, а «другой ответ» — `409`.
9. **Завершённая консультация.** Повторный ответ в неё `IConsultationService`
   отклонит; middleware вернёт `409` с понятным текстом.
10. **Оценки.** Существующая `Reviews` привязана к `Session`, а не к `Consultation`.
    Для API это ок: одна сессия — одна консультация. Если позже понадобится оценка
    конкретного фильма, таблицу можно расширить.
11. **Случайность в fallback.** Для похожих фильмов сохраняется выбор из топ-5, как
    в старом коде; это даёт разнообразие между консультациями. Внутри одной
    консультации повторы исключены списком показанных.
12. **`FilmLookupItemVM` и `FilmResultVM`.** `FilmResultVM`/`FilmTagVM` удаляются:
    UI переходит на `RecommendedFilm`. `FilmLookupItemVM` остаётся, потому что это
    модель справочников для UI, а не результат консультации.
13. **Swagger и локальная БД.** Для ручной проверки нужны доступные `EsDb` и `FilmsDb`
    с ЭС «Фильмы» и советами. Если их нет, API собирается и тесты проходят на SQLite.
