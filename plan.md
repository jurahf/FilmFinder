# План реализации: EsConsultationServices

Связанная постановка: `task.md`.

## 1. Итоговая архитектура

```
EsUi
  └─ EsServices.ConsultationService (фасад, UI ViewModel)
       └─ EsConsultationServices.ConsultationService
            ├─ ConsultationEngine (чистый движок, без EF)
            ├─ ConsultationStateStore (загрузка/сохранение состояния)
            └─ EsDb (Session, Consultation, GoalStack, ...)
```

- `EsConsultationServices` не ссылается на `EsServices`.
- `EsServices` ссылается на `EsConsultationServices`.
- `EsUi` по-прежнему работает через `EsServices.Services.Consultation.IConsultationService`,
  поэтому страница `Consultation.razor` не переписывается.
- Состояние консультации хранится только в `EsDb`; in-memory store удаляется.

### Что живёт в памяти, а что читается из БД

- **В памяти (кэш):** неизменяемый граф экспертной системы по `ExpertSystemId` —
  домены, переменные, факты, правила и их условия. Экспертная система после сохранения
  новой версии не меняется: создаётся новая запись `ExpertSystem`, а старая остаётся
  для старых консультаций. Поэтому граф можно безопасно кэшировать и переиспользовать
  для всех пользователей и всех сессий этой версии ЭС.
- **Из БД на каждый ответ/запрос:** только состояние конкретной сессии —
  `Session`, `Consultation`, цепочка `GoalStacks`, `ConsultationFacts`,
  `ConsultationRules`, `ConsultationAnswers`, `ConsultationUnknownVariables`,
  `FinalSolution`.
- Сами сессии в памяти не держим: это нужно для переживания перезапуска и для работы
  нескольких экземпляров API. При необходимости позже можно добавить распределённый
  кэш горячих сессий, но это не требуется сейчас.

Кэш графа — singleton-хранилище `IMemoryCache`; загрузчик кэша — scoped-сервис,
который при промахе использует scoped `EsDbContext` и `IConsultationDisplayProvider`.
Так кэш не держит scoped-зависимости, а холодная загрузка не блокирует остальные запросы.

## 2. Изменения в базе данных (`EsDb`)

### Новые таблицы

1. `ConsultationAnswers`
   - `Id` (PK);
   - `ConsultationId` (FK -> `Consultations`, cascade delete);
   - `VariableId` (FK -> `Variables`, no action);
   - `DomainValueId` (FK -> `DomainValues`, nullable, no action);
   - `Value` (строка, точный ответ пользователя);
   - `Order` (порядковый номер ответа).
   - Уникальный индекс `(ConsultationId, Order)`.
   - Назначение: явное хранение промежуточных ответов пользователя, в том числе когда
     для ответа нет готовой записи `Fact`.

2. `ConsultationUnknownVariables`
   - `Id` (PK);
   - `ConsultationId` (FK -> `Consultations`, cascade delete);
   - `VariableId` (FK -> `Variables`, no action).
   - Уникальный индекс `(ConsultationId, VariableId)`.
   - Назначение: сохранение переменных, которые не удалось вывести, чтобы объяснение
     не терялось после перезапуска.

### Изменения существующих сущностей

- `EsDb.Model.Consultation`:
  - добавить навигации `Answers` и `UnknownVariables`.
- `EsDb.EsDbContext`:
  - добавить `DbSet<ConsultationAnswer> ConsultationAnswers`;
  - добавить `DbSet<ConsultationUnknownVariable> ConsultationUnknownVariables`;
  - настроить связи и индексы в `OnModelCreating`.
- Существующие `GoalStacks`, `ConsultationFacts`, `ConsultationRules`, `FinalSolutions`,
  `Sessions` переиспользуются без изменения структуры.
- `Consultation.CurrentRuleId` остаётся неиспользуемым (как и сейчас).

### Миграция

- Сгенерировать миграцию `ConsultationState`:
  ```
  dotnet ef migrations add ConsultationState --project EsDb --startup-project EsDb
  ```
- Проверить сгенерированный файл: только две новые таблицы, индексы и FK.
- Применить на локальной БД:
  ```
  dotnet ef database update --project EsDb --startup-project EsDb
  ```
- Если локального SQL Server нет, миграцию не применять, но файл миграции и snapshot
  должны быть в репозитории.

## 3. Новый проект `EsConsultationServices`

### Файл проекта

`EsConsultationServices/EsConsultationServices.csproj`:

- `TargetFramework` — `net7.0`;
- `ImplicitUsings` — `enable`;
- `Nullable` — `enable`;
- ссылки: `EsDb`, `CommonRepositories`;
- пакеты: `Microsoft.EntityFrameworkCore` 7.0.16,
  `Microsoft.Extensions.DependencyInjection.Abstractions` 7.0.0,
  `Microsoft.Extensions.Caching.Memory` 7.0.0.

Добавить проект в `EsService/ЭС.sln`:
```
dotnet sln "EsService/ЭС.sln" add EsConsultationServices/EsConsultationServices.csproj
```

### Доменная модель движка (`Engine/`)

Создать внутренние модели, не зависящие от EF и от ViewModel `EsServices`:

- `ConsultationExpertSystem` — Id, Name, Target, Variables, Rules, Facts;
- `ConsultationVariable` — Id, Name, Question, Reasoning, Type, Domain;
- `ConsultationDomain` — Id, Name, Values;
- `ConsultationDomainValue` — Id, Value, Display;
- `ConsultationFact` — Id, Variable, DomainValue, Display;
- `ConsultationRule` — Id, Order, Name, Reasoning, Result, Conditions;
- `ConsultationRuleFact` — Order, Fact.

`Display` заполняется при загрузке ЭС: либо исходное `DomainValue.Value`, либо значение
из `IConsultationDisplayProvider` (для домена `#Советы`).

Модель графа после загрузки должна быть **неизменяемой**: только `IReadOnlyList`/
`IReadOnlyDictionary`, без мутаций. Это важно, потому что один и тот же экземпляр
графа переиспользуется всеми сессиями и потоками.

### Кэш графа ЭС (`Persistence/ConsultationExpertSystemCache.cs`)

- `GetAsync(int expertSystemId, ct)`:
  - проверяет `IMemoryCache` по ключу `consultation-es:{id}`;
  - при промахе загружает граф через `IExpertSystemRepository`, получает описания
    через `IConsultationDisplayProvider`, строит неизменяемую модель и кладёт в кэш;
  - при повторных запросах граф из БД не читается.
- Кэш — singleton `IMemoryCache`; сам `ConsultationExpertSystemCache` — scoped,
  потому что при промахе использует scoped `EsDbContext`/провайдер отображения.
  Scoped-зависимости в кэш не попадают.
- Параметры записи: `SlidingExpiration` около 1 часа и `AbsoluteExpirationRelativeToNow`
  около 24 часов. ЭС-версии неизменяемы, но ограничение нужно, чтобы не держать
  в памяти все старые версии бесконечно.
- Добавить методы `Invalidate(int expertSystemId)` и `InvalidateAll()` для будущего
  администрирования (например, при изменении описаний советов).
- Холодная загрузка одного и того же `ExpertSystemId` из нескольких запросов может
  выполниться параллельно; это допустимо (результат одинаковый). Если понадобится,
  позже добавить per-key `SemaphoreSlim`, но на корректность это не влияет.

### Движок (`Engine/`)

- `ConsultationEngine.cs` — перенос логики `EsServices.Services.Consultation.ConsultationEngine`:
  - `Start(ConsultationExpertSystem)`;
  - `Next(ConsultationEngineState)`;
  - `Answer(ConsultationEngineState, string variableName, string value)`;
  - `Current(ConsultationEngineState)` — вернуть текущий вопрос или результат без продвижения;
  - `BuildExplanation(ConsultationEngineState)`;
  - внутренние `DoConsult`, `EvaluateRule`, `RecordAnswer`, `CreateResult` и т.д.
- `ConsultationEngineState.cs` — стек целей, доказанные факты, транзитные факты,
  сработавшие правила, неизвестные переменные, результат.

Отличия от старого движка:

- типы модели новые, а не `ExpertSystemVM`/`FactVM`;
- `Current` нужен для продолжения после перезапуска;
- `RecordAnswer` доступен внутри сборки для восстановления состояния из
  `ConsultationAnswers`;
- поведение алгоритма сохраняется без изменений.

### Контракты (`Contracts/`)

- `IConsultationService`:
  - `Task<ConsultationSession> StartAsync(int expertSystemId, CancellationToken ct = default);`
  - `Task<ConsultationStep> GetCurrentAsync(string sessionId, CancellationToken ct = default);`
  - `Task<ConsultationStep> AnswerAsync(string sessionId, string variableName, string value, CancellationToken ct = default);`
  - `Task<ConsultationExplanation> GetExplanationAsync(string sessionId, CancellationToken ct = default);`
- DTO:
  - `ConsultationSession` — SessionId, ExpertSystemId, ExpertSystemName, Step;
  - `ConsultationStep` — Question, Result, IsCompleted;
  - `ConsultationQuestion` — Name, Text, Answers;
  - `ConsultationAnswerOption` — Value, Display;
  - `ConsultationResult` — VariableName, Value, Display, IsUnknown, Explanation;
  - `ConsultationExplanation` — Rules, ProvedFacts, UnknownVariables;
  - `ConsultationRuleExplanation` — Name, Reasoning, Work, Conditions, Result.
- `IConsultationDisplayProvider`:
  - `Task<IReadOnlyList<ConsultationDisplayValue>> GetDisplayValuesAsync(CancellationToken ct = default);`
  - `ConsultationDisplayValue` — DomainName, Value, Display.
- `DefaultConsultationDisplayProvider` — возвращает пустой список.

### Сервис и сохранение (`Services/`, `Persistence/`)

- `ConsultationStateStore`:
  - `CreateAsync(int expertSystemId, ct)` — создаёт `Session` и `Consultation`,
    берёт граф ЭС из `ConsultationExpertSystemCache`, возвращает состояние;
  - `LoadAsync(string sessionId, ct)` — загружает только состояние конкретной сессии
    (`Consultation` со связями), берёт граф ЭС из кэша, восстанавливает
    `ConsultationEngineState`;
  - `SaveAsync(ConsultationSessionState state, PendingAnswer? answer, ct)` —
    сохраняет стек целей, факты, правила, неизвестные переменные, итог и новый ответ.
- `ConsultationService`:
  - `StartAsync` — транзакция: создать сессию, запустить движок до первого вопроса/результата,
    сохранить состояние;
  - `GetCurrentAsync` — загрузить состояние сессии и граф из кэша, вернуть `engine.Current(...)`;
  - `AnswerAsync` — транзакция: загрузить состояние сессии, проверить ожидаемую переменную,
    вызвать `engine.Answer`, сохранить состояние и ответ;
  - `GetExplanationAsync` — загрузить состояние и вернуть объяснение.
- `ServiceCollectionExtensions.AddEsConsultationServices()`:
  - `services.AddMemoryCache()`;
  - `ConsultationExpertSystemCache` (scoped);
  - `ConsultationEngine` (scoped);
  - `IConsultationService` -> `ConsultationService` (scoped);
  - `IConsultationDisplayProvider` -> `DefaultConsultationDisplayProvider`
    через `TryAddScoped`, чтобы приложение могло зарегистрировать свой провайдер раньше.

### Восстановление состояния из БД

- `GoalStack` читается по цепочке `Consultation.GoalStackId -> NextGoalId`;
  верхушка стека — текущая цель.
- `ConsultationFacts` дают доказанные факты и их `FactTruly`.
- `ConsultationAnswers` проигрываются через `RecordAnswer`, чтобы восстановить
  ответы, для которых нет готового `Fact`, и восстановить транзитные факты.
- `ConsultationRules` дают `WorkedRules`; при дублях по `RuleId` берётся последняя запись.
- `ConsultationUnknownVariables` дают `UnknownVariables`.
- Если стек целей пуст:
  - есть `FinalSolution` — результат известен;
  - нет `FinalSolution` — результат не выведен.
- Иначе текущий вопрос строится по верхушке стека.

### Сохранение шага

В одной транзакции:

1. обновить `Session.LastActivityDate`;
2. пересоздать цепочку `GoalStacks` из текущего стека:
   - обнулить `Consultation.GoalStackId`;
   - удалить старые строки;
   - создать новую цепочку от дна к верхушке;
3. удалить и заново вставить `ConsultationFacts` (кроме транзитных фактов);
4. удалить и заново вставить `ConsultationRules`;
5. удалить и заново вставить `ConsultationUnknownVariables`;
6. добавить `ConsultationAnswer` для нового ответа (если есть);
7. добавить/обновить `FinalSolution` для известного результата;
8. `SaveChanges` и commit.

Пересоздание строк выбрано ради простоты и корректности; объёмы состояния консультации
небольшие (стек ограничен 64, факты/правила — десятки).

## 4. Изменения в `EsServices`

### Удалить

- `Services/Consultation/ConsultationEngine.cs`;
- `Services/Consultation/ConsultationEngineState.cs`;
- `Services/Consultation/ConsultationSessionState.cs`;
- `Services/Consultation/ConsultationSessionStore.cs`.

### Изменить

- `EsServices/EsServices.csproj` — добавить `ProjectReference` на `EsConsultationServices`.
- `Services/Consultation/ConsultationService.cs` — сделать фасадом:
  - внедрить `EsConsultationServices.IConsultationService`;
  - мапить DTO нового сервиса в существующие `ConsultationSessionVM`, `ConsultationStep`,
    `ConsultationQuestionVM`, `ConsultationAnswerVM`, `ConsultationResultVM`,
    `ConsultationExplanationVM`;
  - реализовать новый метод `GetCurrentAsync`.
- `Services/Consultation/IConsultationService.cs` — добавить `GetCurrentAsync`.
- `Services/Consultation/ConsultationStep.cs` — убрать `Warning`, если он больше не нужен
  (ошибки сохранения теперь выбрасываются исключением, а не возвращаются предупреждением).
- `IConsultationService`/`ConsultationSessionVM` — убрать `PersistenceError`.
- `EsUi/Pages/Consultation.razor` — убрать отображение `warning`/`PersistenceError`,
  если поля удалены.

### Добавить

- `Services/Consultation/AdviceConsultationDisplayProvider.cs`:
  - реализует `IConsultationDisplayProvider`;
  - через существующий `IAdviceCatalogService` получает описания советов;
  - возвращает записи для домена `ExpertSystemConverter.AdviceDomainName`.
- Регистрация провайдера в `EsUi`.

### Оставить без изменений

- `FilmConsultationConstants`;
- `FilmRecommendationService`/`IFilmRecommendationService`;
- ViewModel-классы консультации, используемые UI;
- страницу консультации по структуре (кроме удаления warning-полей).

## 5. Изменения в `EsUi`

`EsUi/Program.cs`:

- удалить регистрации `ConsultationEngine` и `ConsultationSessionStore`;
- добавить:
  ```
  builder.Services.AddScoped<IConsultationDisplayProvider, AdviceConsultationDisplayProvider>();
  builder.Services.AddEsConsultationServices();
  builder.Services.AddScoped<IConsultationService, ConsultationService>();
  ```
- `using EsConsultationServices;` для extension-метода.
- `AddEsConsultationServices()` сам вызывает `AddMemoryCache()` и регистрирует кэш графа;
  отдельная регистрация `IMemoryCache` в `EsUi` не нужна.

`Consultation.razor`:

- продолжить использовать `IConsultationService` из `EsServices`;
- при старте по-прежнему вызывать `StartAsync`;
- при необходимости использовать `GetCurrentAsync` для восстановления, но в текущем UI
  `sessionId` живёт в компоненте, поэтому отдельное восстановление после перезагрузки
  страницы не требуется (это задача будущего API).

## 6. `Logic` и `WebApi`

По решению пользователя **не удаляются** в этой задаче. В `Logic` остаётся старый
`EsAlgorithm`; это известный технический долг, который будет убран вместе с `WebApi`
в отдельной задаче.

## 7. Тесты

`EsServices.Tests`:

- добавить `ProjectReference` на `EsConsultationServices`;
- добавить пакет `Microsoft.EntityFrameworkCore.Sqlite` 7.0.16 для тестов сохранения
  (SQLite in-memory поддерживает транзакции и связи, в отличие от EF InMemory);
- обновить `TestExpertSystemFactory` под модели нового движка;
- обновить `ConsultationEngineTests`:
  - вопрос -> ответ -> результат;
  - ложное условие -> unknown;
  - `QuerriableDetective` -> вопрос после неудачных правил.
- добавить `ConsultationPersistenceTests`:
  1. `StartAsync` создаёт `Session`, `Consultation`, `GoalStack` и возвращает вопрос.
  2. После «перезапуска» (новый `EsDbContext` на той же SQLite in-memory БД и новый
     экземпляр сервиса) `GetCurrentAsync(sessionId)` возвращает тот же вопрос.
  3. `AnswerAsync` сохраняет `ConsultationAnswer` и возвращает результат.
  4. Итог сохраняется в `FinalSolution`.
  5. Ответ, для которого нет готового `Fact`, сохраняется и не приводит к повторному
     вопросу после перезапуска.
  6. Unknown-результат: `ConsultationUnknownVariables` сохраняются, повторный
     `GetCurrentAsync` возвращает завершённый unknown-результат.
- добавить тест кэша `ConsultationExpertSystemCacheTests`:
  - два обращения к одному `ExpertSystemId` загружают граф из БД один раз
    (считать вызовы репозитория/провайдера или подменить загрузчик);
  - после `Invalidate(id)` граф загружается заново.

## 8. Порядок шагов для агента

- [x] 1. Добавить сущности `ConsultationAnswer` и `ConsultationUnknownVariable` в `EsDb`,
      настроить `EsDbContext`, навигации `Consultation`, индексы и FK.
- [x] 2. Сгенерировать и проверить миграцию `ConsultationState`.
- [x] 3. Создать проект `EsConsultationServices`, добавить его в решение и ссылки.
- [x] 4. Перенести доменную модель и движок консультации в новый проект.
- [x] 5. Реализовать `IConsultationDisplayProvider` и default-провайдер.
- [x] 6. Реализовать `ConsultationExpertSystemCache` на `IMemoryCache`.
- [x] 7. Реализовать `ConsultationStateStore` (загрузка/создание/сохранение состояния).
- [x] 8. Реализовать `EsConsultationServices.IConsultationService`/`ConsultationService`
      и DI-extension.
- [x] 9. Переписать `EsServices.ConsultationService` в фасад, добавить мапинг DTO.
- [x] 10. Добавить `AdviceConsultationDisplayProvider` в `EsServices`.
- [x] 11. Удалить старый `ConsultationEngine`, `ConsultationEngineState`,
      `ConsultationSessionState`, `ConsultationSessionStore`.
- [x] 12. Обновить `EsUi/Program.cs` и `Consultation.razor`.
- [x] 13. Обновить `EsServices.Tests`, добавить тесты сохранения, продолжения и кэша.
- [x] 14. Собрать net7-проекты и прогнать тесты.
- [x] 15. Проверить миграцию и ручной сценарий в `EsUi`, если доступна локальная БД.

## 9. Как проверять

### Сборка

```
dotnet build EsConsultationServices/EsConsultationServices.csproj
dotnet build EsServices/EsServices.csproj
dotnet build EsUi/EsUi.csproj
```

`dotnet build "EsService/ЭС.sln"` полностью не пройдёт из-за старых `WebApi` и `LoadFilms`
(это было и до задачи). Ориентироваться на net7-проекты.

### Тесты

```
dotnet test EsServices.Tests/EsServices.Tests.csproj
```

Ожидаемо: старые тесты движка + новые тесты сохранения/восстановления проходят.

### Миграция

```
dotnet ef migrations script --project EsDb --startup-project EsDb
```

Проверить, что скрипт создаёт только две новые таблицы и индексы.

### Ручной сценарий

1. Запустить `EsUi`.
2. Открыть экспертную систему, начать консультацию.
3. Ответить на несколько вопросов.
4. Убедиться, что в `Consultations` появилась запись, в `ConsultationAnswers` —
   ответы, в `GoalStacks` — текущий стек.
5. Завершить консультацию и проверить `FinalSolutions`.
6. Для проверки восстановления — использовать тест с новым экземпляром сервиса
   (UI не хранит `sessionId` между перезагрузками страницы).
7. В режиме подбора фильма проверить, что описания советов по-прежнему отображаются
   человекочитаемо, а не GUID-ами.

## 10. Узкие и неочевидные места

1. **Цепочка `GoalStack`**. Это связный список с `NextGoalId`; при сохранении нужно
   сначала обнулить `Consultation.GoalStackId`, потом удалять старые строки, иначе
   SQL Server не даст удалить ссылаемую строку. Новую цепочку строить от дна к верхушке.
2. **Транзитные факты**. Если пользователь выбрал значение, для которого нет записи
   `Fact`, движок создаёт факт только в памяти. В БД его нужно хранить в
   `ConsultationAnswers`, а при загрузке восстановить через `RecordAnswer`.
3. **Несколько фактов на переменную**. `RecordAnswer` помечает все факты переменной:
   совпавший — `IsTrue`, остальные — `IsFalse`. Нельзя хранить только один факт.
4. **Завершённый unknown-результат**. Пустой стек целей без `FinalSolution` — это
   завершённая консультация с неизвестным результатом, а не «ещё не начатая».
5. **Правила с одинаковым `RuleId`**. Старый движок может записывать правило несколько
   раз (означилось/не означилось). При загрузке брать последнюю запись, при сохранении —
   одну актуальную строку на правило.
6. **Описания советов**. `DomainValue.Value` для советов — GUID; нельзя терять
   `Display` из `IAdviceCatalogService`, иначе UI покажет GUID.
7. **Транзакции и тесты**. EF InMemory не поддерживает транзакции; для тестов
   сохранения использовать SQLite in-memory.
8. **Одновременные ответы в одну сессию**. Разные пользователи/сессии независимы.
   Для одной сессии транзакция + уникальный индекс `(ConsultationId, Order)` не дадут
   записать два ответа с одним порядковым номером; при конфликте нужно вернуть понятную
   ошибку. Полноценная optimistic concurrency — возможное развитие, не обязательный
   минимум задачи.
9. **Версии ЭС**. Консультация ссылается на `ExpertSystemId`; при сохранении новой
   версии старая не удаляется, а переименовывается, поэтому история консультаций
   остаётся целой.
10. **Кэш графа ЭС**. Закэшированная модель должна быть неизменяемой: движок читает её
    из многих потоков одновременно. Нельзя менять `Display`, коллекции или другие поля
    после помещения в кэш. `Display` советов попадает в кэш вместе с графом, поэтому
    изменения описаний советов подхватятся только после истечения TTL или явного
    `Invalidate`. Это осознанный компромисс; при необходимости описания можно вынести
    в отдельный кэш с более коротким TTL.
11. **Кэш и несколько экземпляров API**. `IMemoryCache` локальный для процесса. При
    горизонтальном масштабировании каждый экземпляр загрузит граф один раз сам; это
    корректно. Распределённый кэш (Redis) не требуется для текущей задачи.
12. **`Logic` остаётся**. В репозитории временно будет два движка консультации
    (`Logic.EsAlgorithm` и новый). Это осознанное решение; удаление — отдельная задача.
