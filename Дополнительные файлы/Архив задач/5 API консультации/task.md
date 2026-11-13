# Задача 5. API консультаций EsConsultationApi

## Постановка

Нужно создать новый проект — WebAPI на .NET 7 с именем `EsConsultationApi`.
Проект должен использовать `EsConsultationServices` и предоставлять HTTP API для прохождения
консультаций экспертной системы «Фильмы».

Результат консультации — значение переменной-совета: GUID, соответствующий совету в `FilmDb`.
Поэтому API должно уметь по результату консультации подобрать фильм, как это делал
`FilmAndAdviceLogic.FindFilmByConsultResult`, и вернуть его клиенту.

Должны быть реализованы методы (по аналогии с классами из `WebApi.Classes.Vk.Commands`,
кроме `StartCommand`, которая нужна только для бота):

1. **Старт новой консультации** (аналог `StartConsultationCommand`).
   Возвращает новый `sessionId`, который используется в последующих запросах,
   и первый вопрос консультации.
2. **Ответ на вопрос** в текущей консультации (аналог `SetAnswerAndNextCommand`).
   Принимает `sessionId`, переменную, о которой был задан вопрос, и значение ответа.
   Возвращает следующий вопрос. Если цель консультации достигнута — завершает консультацию
   и возвращает результат: значение переменной-совета, его отображаемое описание и подобранный фильм.
3. **Другой ответ** (аналог `AnotherCommand`). Результатом консультации могут быть несколько
   фильмов или менее подходящие фильмы. Нужно вернуть другой результат консультации, похожий
   на предыдущие. Желательно не повторяться.
4. **Обратная связь** по результатам консультации (аналог `ReviewCommand`) — пока простая:
   понравилось или нет.

Существующий код можно брать за основу, но он может содержать ошибки и неточности —
допускается его улучшать.

## Выбранное решение

### 1. Связь проектов

`EsConsultationApi` — самостоятельный проект ASP.NET Core Web API на .NET 7:

- использует `EsConsultationServices` для проведения консультации и хранения её состояния;
- использует `FilmDb` для чтения советов, свойств фильмов и самих фильмов;
- использует `EsDb` для оценок (`Reviews`) и для хранения показанных в консультации фильмов;
- **не ссылается на `EsServices`**: общий для UI и API код переносится в
  `EsConsultationServices`, а не остаётся в слое, ориентированном на EsUi.

`FilmRecommendationService` из `EsServices` разделяется по ответственности:

- `FindByAdviceAsync`/`FindByFiltersAsync` и `AdviceConsultationDisplayProvider` переезжают
  в `EsConsultationServices` (раздел 7);
- `GetGenresAsync`/`GetCustomPropertiesAsync` остаются в `EsServices` как отдельные
  сервисы `GenresService` и `CustomPropertiesService` — это данные для UI;
- `AdviceCatalogService` остаётся в `EsServices` как UI-сервис описаний советов.

После переноса `EsConsultationServices` ссылается на `FilmDb`. `EsServices` остаётся
набором сервисов доступа к данным и ViewModel для `EsUi`.

Схема зависимостей:

```
EsConsultationApi
  ├─ EsConsultationServices
  │    ├─ EsDb
  │    └─ FilmDb
  ├─ EsDb
  └─ FilmDb
```

Логика консультации не дублируется: API вызывает
`EsConsultationServices.Contracts.IConsultationService`
(`StartAsync`, `AnswerAsync`, `GetCurrentAsync`, `GetExplanationAsync`).

### 2. Подбор фильма

Подбор реализуется в `EsConsultationServices` (после переноса из `EsServices`) отдельным
сервисом поверх `FilmDbContext`:

- по совету (GUID) сначала берутся явно указанные в совете фильмы (`AdviceFilm`) —
  это основные результаты, отсортированные по степени соответствия и рейтингу;
- когда явные фильмы совета закончились, подбираются похожие по вектору свойств
  (как в `FilmAndAdviceLogic`/`FilmRecommendationService`):
  - учитываются свойства совета и свойства фильмов;
  - отбрасываются фильмы с противоположным знаком свойства;
  - считается квадратичное расстояние по всем свойствам;
  - сортировка по расстоянию, затем по рейтингу;
- сервис возвращает следующего кандидата, исключая уже показанные в этой консультации фильмы;
- при исчерпании кандидатов API возвращает понятную ошибку «больше нет подходящих фильмов»;
- для результата-совета в ответе возвращаются:
  - `value` — GUID совета;
  - `display` — человекочитаемое описание совета;
  - `film` — подробные данные подобранного фильма.

Описания советов подставляются в движок через `AdviceConsultationDisplayProvider`,
который переносится в `EsConsultationServices` и читает советы из `FilmDb`.
`AdviceCatalogService` остаётся в `EsServices` для UI и использует то же построение
описаний из `FilmDb`.

### 3. Ответы на вопросы

Ответ адресуется по **имени переменной** (`variableName`), как в текущем движке
`EsConsultationServices`. Клиент получает имя переменной в вопросе и возвращает его вместе
с выбранным значением. Дополнительно API проверяет, что ответ относится к текущему вопросу.

### 4. Оценки

Оценка «понравилось / не понравилось» сохраняется в существующую таблицу `EsDb.Reviews`,
привязанную к `SessionId`:

- `Estimate = 10` — понравилось;
- `Estimate = 0` — не понравилось.

### 5. Неповторяющиеся фильмы

В `EsDb` добавляется таблица показанных фильмов консультации:

- `Id` — PK;
- `ConsultationId` — FK на `Consultations`, cascade delete;
- `FilmId` — Id фильма из `FilmsDb` (без FK между базами);
- `Order` — порядковый номер показа;
- `ShownDate` — дата показа.

Уникальные индексы:

- `(ConsultationId, FilmId)` — один фильм не показывается в консультации дважды;
- `(ConsultationId, Order)` — порядок показов однозначен.

Таблица переживает перезапуск и работает при нескольких экземплярах API.

### 6. Формат API

- ASP.NET Core controllers, JSON;
- Swagger/OpenAPI для ручной проверки;
- коды ошибок через `ProblemDetails`:
  - 400 — некорректный запрос;
  - 404 — консультация/сессия/результат не найдены;
  - 409 — консультация завершена, ответ не к текущему вопросу, нет подходящего результата;
- строка подключения `EsConnection` — `EsDb`, `FilmConnection` — `FilmsDb`
  (по аналогии с `EsUi`).

### 7. Разделение общих сервисов

Общий код разделяется по ответственности, чтобы API не зависел от слоя EsUi, а в
`EsServices` не оставалось логики консультации.

**Остаётся в `EsServices` (данные и ViewModel для EsUi):**

- `AdviceCatalogService` + `IAdviceCatalogService` — описания советов для UI;
- новые `GenresService` + `IGenresService` — справочник жанров для UI;
- новые `CustomPropertiesService` + `ICustomPropertiesService` — справочник свойств для UI;
- ViewModel-классы (`FilmLookupItemVM` и остальные).

**Переезжает в `EsConsultationServices` (логика консультации/советов):**

- `FindByAdviceAsync` и `FindByFiltersAsync` — как
  `IFilmRecommendationService`/`FilmRecommendationService`;
- `AdviceConsultationDisplayProvider`;
- UI-независимые DTO `RecommendedFilm` и `FilmTag`.

**Общий доступ к данным советов:**

- в `FilmDb` добавляется `IAdviceRepository.GetDescriptionsAsync` (реализация в
  `AdviceRepository`) — единое построение описаний советов;
- `EsServices.AdviceCatalogService` и
  `EsConsultationServices.AdviceConsultationDisplayProvider` используют этот метод,
  поэтому текст описания не дублируется.

**Изменения в `EsServices` и `EsUi`:**

- `EsServices.FilmRecommendationService`/`IFilmRecommendationService` удаляются:
  `EsUi` использует `EsConsultationServices.Films.IFilmRecommendationService` напрямую;
- `EsUi/Components/FilmResultCard.razor` принимает `RecommendedFilm`;
- `EsUi/Pages/Consultation.razor` использует `IFilmRecommendationService` из
  `EsConsultationServices`, а списки жанров/свойств берёт из `IGenresService` и
  `ICustomPropertiesService`;
- `FilmResultVM` и `FilmTagVM` удаляются как ненужные; `FilmLookupItemVM` остаётся;
- `EsUi/Program.cs` регистрирует `AdviceCatalogService`, `GenresService`,
  `CustomPropertiesService` и провайдер описаний из `EsConsultationServices`;
- `AddEsConsultationServices()` регистрирует `IAdviceRepository` и
  `IFilmRecommendationService` через `TryAddScoped`; провайдер описаний советов
  приложение регистрирует явно (чтобы тесты движка могли работать без `FilmDb`).

## Что входит в задачу

1. Создать проект `EsConsultationApi` (.NET 7, Web API) и добавить его в `EsService/ЭС.sln`.
2. Подключить `EsConsultationServices`, `EsDb`, `FilmDb`, EF Core SQL Server, Swagger.
3. Разделить общие сервисы: перенести подбор фильма и провайдер описаний советов в
   `EsConsultationServices`, оставить в `EsServices` каталог советов и справочники
   жанров/свойств, вынести общее чтение описаний советов в `FilmDb`.
4. Настроить DI, строки подключения и провайдер описаний советов.
5. Реализовать API:
   - старт консультации и возврат первого вопроса;
   - ответ и возврат следующего вопроса или результата с фильмом;
   - «другой ответ» с исключением уже показанных фильмов;
   - обратную связь «понравилось / нет»;
   - вспомогательный `GET` текущего шага (для продолжения после перезапуска клиента).
6. Реализовать подбор фильма по совету и по фильтрам (на будущее) поверх `FilmDb`.
7. Добавить в `EsDb` сущность показанных фильмов, настроить `EsDbContext` и миграцию.
8. Добавить тесты API и подбора фильма.
9. Обновить `README.md`: описание нового API.

## Что не входит в задачу

- Удаление `Logic`, `WebApi`, `ExpertSystemDB`, `Utilities.LoadFilms`.
- Переписывание `WebApi` для VK на новый API (отдельная задача).
- Шаг «знаю, что хочу»: в API его нет; подбор идёт только по результату ЭС.
- Аутентификация и авторизация.
- Полноценная optimistic concurrency для одной сессии.
- Переписывание логики и разметки UI `EsUi`: страница `Consultation.razor` и
  `FilmResultCard.razor` только переходят на сервисы и DTO из
  `EsConsultationServices`, поведение не меняется.

## Критерии готовности

- `EsConsultationApi` собирается как .NET 7 проект.
- `EsConsultationServices`, `EsServices` и `EsUi` собираются после переноса общих сервисов.
- Существующий сценарий консультации в `EsUi` работает без регрессии.
- `POST` старта возвращает `sessionId` и первый вопрос.
- `POST` ответа возвращает следующий вопрос, а при достижении цели — результат и фильм.
- `POST` «другой ответ» возвращает фильм, отличный от уже показанных в этой консультации.
- `POST` оценки сохраняет запись в `EsDb.Reviews` по `SessionId`.
- Показанные фильмы сохраняются в новой таблице `EsDb` и не повторяются после перезапуска.
- Swagger открывается и позволяет пройти сценарий вручную.
- Тесты проходят.
- Миграция `EsDb` применяется к локальной базе.

## Проверка

- `dotnet build EsConsultationApi/EsConsultationApi.csproj`
- `dotnet build EsConsultationServices/EsConsultationServices.csproj`
- `dotnet build EsServices/EsServices.csproj`
- `dotnet build EsUi/EsUi.csproj`
- `dotnet test` для нового тестового проекта и `EsServices.Tests`
- `dotnet ef migrations script --project EsDb --startup-project EsDb` — в скрипте только новая таблица
- Ручной сценарий в Swagger:
  1. старт консультации;
  2. ответы на вопросы;
  3. получение результата и фильма;
  4. «другой ответ» — другой фильм;
  5. оценка;
  6. проверка строк в `Sessions`, `Consultations`, `FinalSolutions`, `Reviews`,
     новой таблице показанных фильмов.

## Замечания

- Полная сборка `EsService/ЭС.sln` через `dotnet build` уже сейчас падает на старых
  `WebApi` и `LoadFilms` (Framework 4.x, x86-задачи). Это не регрессия задачи;
  проверяются net7-проекты и тесты.
- Консультация и фильмы лежат в разных базах, поэтому между ними нет FK
  и распределённых транзакций. Сначала читается `FilmsDb`, затем пишется `EsDb`;
  при ошибке записи клиент получает ошибку и может повторить запрос.
