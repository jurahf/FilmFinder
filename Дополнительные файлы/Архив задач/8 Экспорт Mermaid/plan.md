# План реализации: экспорт базы знаний ЭС в Mermaid

Связанная постановка: `task.md`.

## 1. Итоговая картина

```
EsServices
  └─ Services/Export/
       ├─ ExpertSystemMermaidExporter.cs   — чистый форматтер ExpertSystemVM -> Mermaid
       ├─ IExpertSystemExportService.cs    — контракт и результат экспорта
       └─ ExpertSystemExportService.cs     — поиск ЭС «Фильмы», загрузка VM, имя файла

EsUi
  ├─ Program.cs                          — регистрация экспортёров
  ├─ Shared/NavMenu.razor                — пункт меню «Экспорт»
  ├─ Pages/ExpertSystemExport.razor      — предпросмотр и скачивание
  ├─ Pages/_Host.cshtml                  — подключение js/download.js
  ├─ wwwroot/js/download.js              — скачивание файла через Blob
  ├─ wwwroot/css/site.css                — стиль предпросмотра
  └─ _Imports.razor                      — using EsServices.Services.Export

EsServices.Tests
  ├─ ExpertSystemMermaidExporterTests.cs
  └─ ExpertSystemExportServiceTests.cs
```

Изменений в БД, миграций и изменений движка нет.

## 2. Формат Mermaid

### 2.1. Идея

Узлов правил нет. Правило — это цепочка условий в том порядке, в котором движок их
проверяет:

```text
[С кем смотреть?] -> (С семьей) -> [Какой состав?] -> (Есть дети до 10 лет) -> <[Совет: Волшебный (100); Детский (100)]>
```

- вопрос — прямоугольник (`Q1["..."]`);
- ответ — овал (`A1(["..."])`);
- лист-свойство — шестиугольник (`P1{{"Совет = ..."}}`);
- номер и имя правила — подпись на стрелке от последнего ответа к листу:
  `A4 -->|"1. ДетиДо10Лет"| P1`.

Так граф читается как дерево решений: от первого вопроса по ответам до совета.
Несколько условий одного правила — это последовательность вопросов в одной цепочке
(логическое «И»).

### 2.2. Пример файла

```text
%% Экспертная система: Фильмы (Id 18)
%% Дата экспорта: 2026-11-14
%% Цель консультации: Совет
%% Правил: 27
%% Вопрос — прямоугольник, ответ — овал, свойство (вывод) — шестиугольник.
%% Цепочка «вопрос → ответ → ... → лист» — условия одного правила в порядке проверки.
%% Номер и имя правила — на стрелке к листу.
%% Правила проверяются по возрастанию номера; срабатывает первое подходящее.

flowchart TD
    classDef question fill:#FFCC00,stroke:#333333,color:#000000
    classDef answer fill:#FFFFFF,stroke:#333333,color:#000000
    classDef property fill:#CCE5FF,stroke:#333333,color:#000000
    classDef invalid fill:#FFCCCC,stroke:#333333,color:#000000

    Q1["С кем собираетесь смотреть фильм?"]:::question
    A1(["С семьей"]):::answer
    Q2["Будут ли смотреть дети?"]:::question
    A2(["Есть дети до 10 лет"]):::answer
    P1{{"Совет = Волшебный (100); Детский (100)"}}:::property

    Q1 --> A1
    A1 --> Q2
    Q2 --> A2
    A2 -->|"1. ДетиДо10Лет"| P1
```

### 2.3. Построение графа (trie)

Каждое правило вставляет свою последовательность условий в дерево (бор):

- порядок правил: `Order`, затем `Name`, затем `Id`;
- порядок условий: `Condition.Order`, затем `Fact.Id`;
- ключ узла вопроса — «родительский ответ + имя переменной»; ключ ответа —
  «узел вопроса + значение». Поэтому:
  - правила с одинаковым префиксом используют общие узлы;
  - одинаковый вопрос, достигнутый по разным веткам, — это разные узлы
    (как `Какой-то праздник?` дважды в `Схема БЗ.graphml`);
- под каждым вопросом создаются все значения его домена, даже если из ответа
  не выведен ни один совет (тупиковая ветка);
- после последнего условия правила добавляется стрелка к листу результата
  с подписью `«{Order + 1}. {Name}»`;
- листья дедуплицируются по паре «имя переменной + значение»: одинаковый совет,
  выведенный разными правилами, — один шестиугольник с несколькими входящими стрелками;
- если условие относится к невыводимо-запрашиваемой переменной (нет вопроса),
  вместо пары «вопрос → ответ» используется узел-факт (шестиугольник);
- если результат правила используется как условие другого правила, этот факт —
  общий шестиугольник, связывающий цепочки;
- правило без вывода не роняет экспорт: цепочка завершается шестиугольником
  «Вывод не задан» со стилем `invalid`;
- правило без посылок — отдельный узел «Правило N. Имя: нет посылок» со стилем `invalid`;
- рёбра дедуплицируются; терминальные рёбра с разными подписями правил сохраняются.

Порядок создания узлов детерминирован, идентификаторы — счётчики `Q1`, `A1`, `P1`,
а не Id из БД (в тестовых VM Id могут быть нулевыми).

### 2.4. Экранирование

Подписи узлов и рёбер:

- `#` → `#35;` (первым, чтобы не испортить следующие замены);
- `&` → `#amp;`;
- `"` → `#quot;`;
- `<` → `#lt;`;
- `>` → `#gt;`;
- `\r\n`/`\r` → `\n`, затем `\n` → `<br/>`.

## 3. EsServices

### 3.1. `ExpertSystemMermaidExporter`

Файл: `EsServices/Services/Export/ExpertSystemMermaidExporter.cs`.

```csharp
public sealed class ExpertSystemMermaidExporter
{
    public string Export(ExpertSystemVM expertSystem, DateTime? exportedAt = null);
}
```

Внутри:

1. Упорядочить правила и условия.
2. Построить trie: узлы вопросов, ответов, фактов, листьев; рёбра с подписями.
3. Вывести шапку, `flowchart TD`, `classDef`, определения узлов и рёбра.

Вспомогательные методы:

- `BuildGraph(...)` — модель узлов/рёбер;
- `EscapeLabel(string)` — экранирование;
- `FormatRuleEdgeLabel(RuleVM)` — `«{Order + 1}. {Name}»`;
- `FormatFactLabel(FactVM)` — `FactVM.Display` (уже даёт `Совет = <описание>`);
- `GetQuestionText(VariableVM)` — `Question`, иначе `Name`.

Не зависит от `EsDb`, `FilmDb`, UI и `ConsultationEngine`.

### 3.2. `IExpertSystemExportService` и `ExpertSystemExportService`

Файлы:

- `EsServices/Services/Export/IExpertSystemExportService.cs`
- `EsServices/Services/Export/ExpertSystemExportService.cs`

```csharp
public interface IExpertSystemExportService
{
    Task<ExpertSystemExportResult> ExportMermaidAsync(CancellationToken cancellationToken = default);
}

public sealed class ExpertSystemExportResult
{
    public int ExpertSystemId { get; init; }
    public string ExpertSystemName { get; init; } = string.Empty;
    public int RuleCount { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
}
```

`ExpertSystemExportService`:

- константа `DefaultExpertSystemName = "Фильмы"`;
- зависит от `IExpertSystemService` и `ExpertSystemMermaidExporter`;
- `GetListAsync()` → фильтр по точному имени (`Trim`, `OrdinalIgnoreCase`),
  сортировка по `Id` по убыванию, берётся первая запись;
- если запись не найдена — `KeyNotFoundException` с текстом
  «Экспертная система 'Фильмы' не найдена.»;
- `GetAsync(id)` → `ExpertSystemMermaidExporter.Export(...)`;
- имя файла: `expert-system-{DateTime.Now:yyyy-MM-dd}.mmd`;
- возвращает `ExpertSystemExportResult`.

Загрузка через `IExpertSystemService.GetAsync` важна: этот метод уже подставляет
человекочитаемые описания советов (`Волшебный (100); Детский (100)`) через
`IAdviceCatalogService`, поэтому листья графа сразу получают вектор свойств.

## 4. EsUi

### 4.1. Регистрация

`EsUi/Program.cs`:

```csharp
builder.Services.AddScoped<ExpertSystemMermaidExporter>();
builder.Services.AddScoped<IExpertSystemExportService, ExpertSystemExportService>();
```

`EsUi/_Imports.razor`:

```razor
@using EsServices.Services.Export
```

### 4.2. Меню

`EsUi/Shared/NavMenu.razor` — после пункта «Экспертные системы»:

```razor
<div class="nav-item px-3">
    <NavLink class="nav-link" href="export">
        <span class="oi oi-data-transfer-download" aria-hidden="true"></span> Экспорт
    </NavLink>
</div>
```

### 4.3. Страница `Pages/ExpertSystemExport.razor`

Маршрут `@page "/export"`.

Состояние:

- `ExpertSystemExportResult? result`;
- `bool loading`;
- `string? error`;
- `bool downloading`.

`OnInitializedAsync` вызывает `ExportAsync()` — открытие пункта меню сразу формирует экспорт.

`ExportAsync`:

- сбрасывает ошибку, ставит `loading = true`;
- вызывает `ExportService.ExportMermaidAsync()`;
- ловит исключения в `error`, всегда снимает `loading`.

`DownloadAsync`:

```csharp
using var stream = new MemoryStream(Encoding.UTF8.GetBytes(result.Content));
using var streamRef = new DotNetStreamReference(stream);
await JS.InvokeVoidAsync("downloadFileFromStream", result.FileName, streamRef);
```

Разметка:

- заголовок «Экспорт базы знаний» и пояснение, что экспортируются только правила ЭС «Фильмы»;
- при `loading` — «Формирование экспорта...»;
- при `error` — `alert alert-danger` и кнопка «Повторить»;
- при результате — карточка: имя ЭС, число правил, имя файла, кнопка «Скачать»;
- предпросмотр содержимого в `<pre class="export-preview">` (прокрутка, моноширинный шрифт).

### 4.4. Скачивание

Новый файл `EsUi/wwwroot/js/download.js`:

```js
window.downloadFileFromStream = async (fileName, contentStreamReference) => {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer], { type: 'text/plain;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = fileName || 'expert-system.mmd';
    document.body.appendChild(anchor);
    anchor.click();
    anchor.remove();
    URL.revokeObjectURL(url);
};
```

`EsUi/Pages/_Host.cshtml` — подключить до `blazor.server.js`:

```html
<script src="js/download.js"></script>
```

Это стандартный для Blazor Server способ отдать файл без HTTP-контроллера;
размер экспорта (десятки килобайт) для SignalR некритичен.

### 4.5. Стиль

`EsUi/wwwroot/css/site.css`:

```css
.export-preview {
    max-height: 60vh;
    overflow: auto;
    padding: 0.75rem;
    background: #f8f9fa;
    border: 1px solid #dee2e6;
    border-radius: 0.25rem;
    font-size: 0.85rem;
}
```

## 5. Тесты

### 5.1. `EsServices.Tests/ExpertSystemMermaidExporterTests.cs`

Использовать `TestExpertSystemFactory.BuildSimple()` и при необходимости собирать VM вручную.

Тесты:

1. `Export_ContainsQuestionChainAndLeaf` — есть `flowchart TD`, вопрос, ответ,
   подпись правила на финальной стрелке, лист `T = Да`.
2. `Export_AllAnswerOptionsAreShown` — в графе есть и «Да», и «Нет», даже если «Нет»
   не участвует в правиле.
3. `Export_MultiConditionRuleBecomesChain` — правило с двумя посылками даёт
   `вопрос → ответ → вопрос → ответ → лист`, а не отдельный узел правила.
4. `Export_SharesCommonPrefix` — два правила с общим префиксом используют один узел
   ответа и один узел следующего вопроса.
5. `Export_DuplicatesQuestionInDifferentBranches` — одинаковый вопрос, достигнутый
   по разным веткам, даёт два узла (как в yEd-схеме).
6. `Export_MergesSameLeaf` — два правила с одинаковым выводом дают один шестиугольник.
7. `Export_SupportsIntermediateFact` — результат одного правила как условие другого:
   шестиугольник связан и с producing-, и с consuming-цепочкой.
8. `Export_EscapesQuotesAndLineBreaks` — кавычка → `#quot;`, перевод строки → `<br/>`.
9. `Export_RuleWithoutResult` — узел «Вывод не задан», экспорт не падает.
10. `Export_HeaderContainsNameTargetAndDate` — имя ЭС, цель, переданная дата.

Проверять содержимое строками (`Assert.Contains`) и количеством вхождений, не привязываясь
к конкретным сгенерированным Id.

### 5.2. `EsServices.Tests/ExpertSystemExportServiceTests.cs`

Fake `IExpertSystemService` (вложенный класс) с фиксированным списком:

- `Фильмы_auto_2026-09-09` (Id 1);
- `Фильмы` (Id 2);
- `Фильмы` (Id 3) — если хочется проверить выбор максимального Id;
- `GetAsync` возвращает `TestExpertSystemFactory.BuildSimple()`.

Тесты:

1. `Export_SelectsExactFilmsAndIgnoresArchived` — `ExpertSystemId` равен ожидаемому.
2. `Export_BuildsDefaultFileName` — имя соответствует
   `^expert-system-\d{4}-\d{2}-\d{2}\.mmd$`.
3. `Export_ThrowsWhenFilmsNotFound` — `KeyNotFoundException`.
4. `Export_ReturnsContentWithTargetVector` — содержимое содержит `T = Да`.

## 6. Документация

`README.md`:

- в разделе `EsServices` — новая строка про экспорт БЗ в Mermaid
  (`ExpertSystemMermaidExporter`, `ExpertSystemExportService`);
- в разделе `EsUi` — пункт меню «Экспорт»: формирует `.mmd` по правилам ЭС «Фильмы»
  и отдаёт файл `expert-system-{yyyy-MM-dd}.mmd`.

## 7. Порядок шагов для агента

- [ ] 1. Создать `ExpertSystemMermaidExporter`: trie цепочек, экранирование, вывод.
- [ ] 2. Создать `IExpertSystemExportService`, `ExpertSystemExportResult`,
      `ExpertSystemExportService`.
- [ ] 3. Зарегистрировать сервисы в `EsUi/Program.cs`, добавить `using` в `_Imports.razor`.
- [ ] 4. Добавить пункт меню «Экспорт» в `NavMenu.razor`.
- [ ] 5. Создать страницу `ExpertSystemExport.razor`, JS `download.js`, стиль `site.css`,
      подключить JS в `_Host.cshtml`.
- [ ] 6. Написать тесты форматтера и сервиса экспорта.
- [ ] 7. Собрать `EsServices` и `EsUi`, прогнать `EsServices.Tests`.
- [ ] 8. Обновить `README.md`.
- [ ] 9. Ручная проверка: запустить `EsUi`, открыть «Экспорт», скачать файл, проверить
      в Mermaid Live Editor; убедиться, что архивных версий и консультаций в файле нет.

## 8. Как проверять

### Сборка

```text
dotnet build EsServices/EsServices.csproj
dotnet build EsUi/EsUi.csproj
```

### Тесты

```text
dotnet test EsServices.Tests/EsServices.Tests.csproj
```

### Ручной сценарий

1. Запустить `EsUi` (локальные `EsDb` и `FilmsDb` должны быть доступны).
2. Меню → «Экспорт».
3. Убедиться, что страница показала «Фильмы», 27 правил и имя файла
   `expert-system-<сегодня>.mmd`.
4. Нажать «Скачать» — файл сохраняется с этим именем.
5. Открыть файл в Mermaid Live Editor: должна строиться диаграмма
   «вопрос → ответ → следующий вопрос → … → лист-совет».
6. Проверить в тексте отсутствие `_auto_`, `Consultations`, `Session` и GUID-консультаций.

## 9. Узкие и неочевидные места

1. **Контекстные узлы вопросов.** Нельзя дедуплицировать вопросы и ответы глобально:
   один и тот же вопрос «Праздник?» в ветке «дети старше 10» и в ветке «только взрослые»
   ведёт к разным советам. Поэтому trie строит отдельные узлы на каждую ветку,
   как в исходной yEd-схеме.
2. **Общий префикс.** Правила с одинаковым началом должны переиспользовать узлы,
   иначе дерево раздуется; ключ узла — родительский ответ + имя переменной.
3. **Порядок условий и правил.** Движок проверяет правила по `Order` и условия по
   `Condition.Order`; цепочка должна строиться в этом же порядке, иначе граф
   будет описывать не тот алгоритм.
4. **Экранирование Mermaid.** Кавычки, `<`, `>`, `&`, `#`, переводы строк ломают
   подписи. Замены делать в правильном порядке (`#` первым), иначе `#quot;` испортится.
5. **Все ответы домена.** Даже неиспользуемый ответ показывается как тупиковая ветка:
   это часть вопроса. Не путать с правилами, которые не сработали.
6. **Длинные подписи листьев.** Вектор совета может быть длинным; это осознанно.
   При отсутствии описания в `FilmsDb` `ExpertSystemConverter` оставляет GUID —
   экспорт не падает.
7. **Только активная ЭС.** Поиск по точному имени `Фильмы`: архивные записи содержат
   `_auto_`. При нескольких точных совпадениях берётся максимальный `Id`.
8. **Правила без вывода/посылок.** Валидатор их не пропускает, но старые/битые данные
   не должны ронять экспорт: рисуется узел со стилем `invalid`.
9. **Промежуточные выводы.** Сейчас их нет, но если результат правила используется
   как условие другого, он должен быть общим шестиугольником, а не теряться.
10. **Консультации не читаем.** Экспорт строится только по `ExpertSystemVM`
    (домены, переменные, факты, правила). Таблицы `Consultations*`, `Sessions`, `Reviews`
    не затрагиваются.
11. **`GetAsync` и `FilmsDb`.** Описания советов приходят из `FilmsDb`; при недоступной
    `FilmsDb` экспорт вернёт ошибку на странице. Без описаний листья были бы только GUID-ами.
12. **Дата в имени и шапке.** Один и тот же `DateTime.Now` в сервисе, чтобы имя и
    содержимое не разошлись около полуночи.
13. **Blazor Server и скачивание.** `DotNetStreamReference` — рекомендуемый способ,
    не требующий контроллера; файл небольшой, ограничения SignalR не достигаются.
14. **Порядок правил не виден на промежуточных стрелках.** Подпись правила только
    на финальной стрелке; если два правила имеют один и тот же путь условий,
    будут две подписи на разных стрелках к своим листьям. Это ожидаемо.
