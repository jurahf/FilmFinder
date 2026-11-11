# Реализация: EsConsultationServices

Связанные документы: `task.md`, `plan.md`.

## Итог

Движок консультации вынесен из `EsServices` в отдельный проект
`EsConsultationServices` (net7.0). Состояние каждой сессии хранится в `EsDb`
и переживает перезапуск приложения. `EsServices` оставлен тонким фасадом,
который мапит DTO нового сервиса в существующие ViewModel для Blazor.
Старый in-memory `ConsultationEngine` и `ConsultationSessionStore` удалены.

```
EsUi
  └─ EsServices.ConsultationService (фасад + ViewModel)
       └─ EsConsultationServices.ConsultationService
            ├─ ConsultationEngine (чистый движок)
            ├─ ConsultationStateStore (состояние сессии в EsDb)
            └─ ConsultationExpertSystemCache (граф ЭС в IMemoryCache)
```

## Что изменено

### EsDb

- Добавлены сущности:
  - `ConsultationAnswer` — ответ пользователя (`Value`, `Order`, `VariableId`,
    nullable `DomainValueId`);
  - `ConsultationUnknownVariable` — переменные, которые не удалось вывести.
- В `Consultation` добавлены навигации `Answers` и `UnknownVariables`,
  в `EsDbContext` — `DbSet`-ы, связи и уникальные индексы:
  - `(ConsultationId, Order)` для ответов;
  - `(ConsultationId, VariableId)` для неизвестных переменных.
- Сгенерирована миграция `20261110122624_ConsultationState`.

### EsConsultationServices

Новый проект `EsConsultationServices/EsConsultationServices.csproj`:

- `Engine/` — неизменяемая модель графа ЭС и перенесённый движок.
  Добавлен метод `Current(...)`, который возвращает текущий вопрос или результат
  без продвижения консультации. `RecordAnswer` доступен внутри сборки и
  используется при восстановлении состояния из БД.
- `Contracts/` — DTO и интерфейсы `IConsultationService`,
  `IConsultationDisplayProvider`.
- `Persistence/ConsultationExpertSystemCache.cs` — общий кэш графа ЭС
  по `ExpertSystemId`. Реализован как singleton с `IServiceScopeFactory`:
  при промахе создаётся отдельный scope для scoped `EsDbContext` и
  `IConsultationDisplayProvider`. TTL: sliding 1 час, absolute 24 часа.
  Есть `Invalidate(id)` и `InvalidateAll()`.
- `Persistence/ConsultationStateStore.cs` — создание, загрузка и сохранение
  состояния сессии.
- `Persistence/ConsultationSessionState.cs` — состояние сессии и `PendingAnswer`.
- `Services/ConsultationService.cs` — прикладной сервис с транзакциями.
- `ServiceCollectionExtensions.AddEsConsultationServices()` — регистрация
  `AddMemoryCache`, репозитория, провайдера отображения по умолчанию, кэша,
  движка, store и сервиса.

### EsServices

- `ConsultationService` переписан в фасад над
  `EsConsultationServices.Contracts.IConsultationService`.
- В `IConsultationService` добавлен `GetCurrentAsync`.
- Из `ConsultationStep` и `ConsultationSessionVM` убраны `Warning` и
  `PersistenceError`: ошибки сохранения теперь выбрасываются исключением.
- Добавлен `AdviceConsultationDisplayProvider`: берёт описания советов из
  `IAdviceCatalogService` и подставляет их в домен `#Советы`, чтобы UI не
  показывал GUID вместо текста.
- Удалены `ConsultationEngine`, `ConsultationEngineState`,
  `ConsultationSessionState`, `ConsultationSessionStore`.
- Добавлена ссылка на `EsConsultationServices`.

### EsUi

- Удалены регистрации старого движка и store.
- Добавлены регистрации `AdviceConsultationDisplayProvider` и
  `AddEsConsultationServices()`.
- Из `Consultation.razor` убрано отображение warning/`PersistenceError`.

## Как сохраняется и восстанавливается состояние

На каждый ответ/запрос из БД читается только конкретная сессия, граф ЭС берётся
из кэша.

### Сохранение шага

В одной транзакции:

1. обновляется `Session.LastActivityDate`;
2. пересоздаётся цепочка `GoalStacks`;
3. удаляются и заново вставляются `ConsultationFacts` (кроме транзитных фактов)
   и `ConsultationRules`;
4. удаляются и заново вставляются `ConsultationUnknownVariables`;
5. добавляется новый `ConsultationAnswer`, если был ответ;
6. добавляется/обновляется `FinalSolution` для известного результата.

### Восстановление

- Стек целей читается по цепочке `GoalStackId -> NextGoalId`.
- `ConsultationFacts` восстанавливают доказанные факты и их `FactTruly`.
- `ConsultationAnswers` проигрываются через `RecordAnswer` — это нужно для
  ответов, для которых нет готовой записи `Fact` (транзитные факты).
- `ConsultationUnknownVariables` восстанавливают список невыведенных переменных.
- Пустой стек целей означает завершённую консультацию: результат берётся из
  доказанного целевого факта, а при его отсутствии — unknown-результат.

## Проверка

Собраны проекты:

```
dotnet build EsConsultationServices/EsConsultationServices.csproj
dotnet build EsServices/EsServices.csproj
dotnet build EsUi/EsUi.csproj
```

Тесты:

```
dotnet test EsServices.Tests/EsServices.Tests.csproj
```

Результат: 13 тестов пройдено, 0 ошибок. Покрыты:

- три сценария движка (вопрос -> результат, ложное условие -> unknown,
  `QuerriableDetective` -> вопрос после неудачных правил);
- создание сессии и сохранение вопроса;
- восстановление текущего вопроса после «перезапуска»;
- сохранение ответа, `FinalSolution` и результата после перезапуска;
- сохранение unknown-результата и `ConsultationUnknownVariables`;
- ответ без готового `Fact` и отсутствие повторного вопроса после перезапуска;
- загрузка графа из кэша один раз и повторная загрузка после `Invalidate`.

Проверка локальной БД: SQL Server доступен, база `EsDb` существует, но в
`__EFMigrationsHistory` есть только `InitialCreate`, таблиц
`ConsultationAnswers` и `ConsultationUnknownVariables` пока нет. Поэтому
миграцию нужно применить перед ручной проверкой UI:

```
dotnet ef database update --project EsDb --startup-project EsDb
```

## Тонкие места

1. **Цепочка GoalStack.** Перед удалением старых целей сначала обнуляется
   `Consultation.GoalStackId` и сохраняется, потом разрываются связи
   `NextGoalId` и только затем строки удаляются. Иначе SQL Server может
   запретить удаление из-за self-FK с `NoAction`.
2. **Кэш и транзакция.** В `StartAsync` граф прогревается до открытия
   транзакции. Так холодная загрузка ЭС не выполняется внутри транзакции и
   не создаёт лишних блокировок.
3. **Транзитные факты.** Ответ, для которого нет записи `Fact`, сохраняется в
   `ConsultationAnswers`, а факт создаётся только в памяти. При загрузке он
   восстанавливается проигрыванием ответов.
4. **Несколько фактов на переменную.** `RecordAnswer` помечает все факты
   переменной: совпавший — `IsTrue`, остальные — `IsFalse`. Поэтому нельзя
   хранить только один факт на переменную.
5. **Завершённый unknown.** Пустой стек целей без `FinalSolution` — это
   завершённая консультация с неизвестным результатом, а не «ещё не начатая».
6. **Описания советов** кэшируются вместе с графом. Изменения описаний в
   `FilmsDb` подхватятся после TTL или явного `Invalidate`.
7. **Одновременные ответы в одну сессию.** Уникальный индекс
   `(ConsultationId, Order)` не даст записать два ответа с одним номером.
   Полноценной optimistic concurrency пока нет: при конфликте возвращается
   понятная ошибка с предложением запросить текущий шаг.
8. **Тесты на SQLite.** EF InMemory не поддерживает транзакции, поэтому
   тесты сохранения используют SQLite in-memory.
9. **`Logic` и `WebApi`.** По решению пользователя не удалялись в этой задаче.
   В `Logic` остаётся старый `EsAlgorithm`; это технический долг отдельной
   задачи.
10. **UI и перезапуск страницы.** `sessionId` по-прежнему живёт в Blazor-
    компоненте. `GetCurrentAsync` уже готов для будущего API, но UI пока не
    восстанавливает сессию после перезагрузки страницы.

## Открытые вопросы

- Применить миграцию `ConsultationState` на локальной БД и пройти ручной
  сценарий в `EsUi` (начать консультацию, ответить, проверить таблицы).
- Нужна ли optimistic concurrency для одной сессии (одновременные ответы)?
- Нужен ли распределённый кэш графа при горизонтальном масштабировании API?
- Когда удалять `Logic`/`WebApi` и старый `EsAlgorithm`.
