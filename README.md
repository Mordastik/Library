# Library

Наскрізний проєкт з крос-платформного програмування.

Предметна область: Бібліотека.
Сутності: Book, BookCopy, Reader, Loan.
Призначення: облік видач примірників книг читачам.

## Структура solution

~~~text
Library/
├── .gitignore
├── Library.slnx
├── README.md
└── src/
    ├── Core/ (Бібліотека класів)
    │   ├── Core.csproj
    │   └── EnvironmentInfo.cs
    └── Cli/ (Консольний застосунок)
        ├── Cli.csproj
        └── Program.cs
~~~

## Команди

**Збірка проєкту:**
~~~bash
dotnet build
~~~

**Запуск проєкту:**
~~~bash
dotnet run --project src/Cli
~~~

**Публікація (під Windows x64):**
~~~bash
dotnet publish src/Cli -c Release -r win-x64 --self-contained true
~~~

## Порівняння режимів публікації

| RID | Режим (Self-contained) | Розмір | Чи потрібен встановлений runtime |
| :--- | :--- | :--- | :--- |
| `win-x64` | `true` (Самодостатній) | 76 MB | Ні |
| `win-x64` | `false` (Залежний) | < 1 MB | Так |

## Середовище
.NET SDK 10.0, Windows 11 x64

## Інваріанти доменної моделі (Лабораторна 4)
- **BookCopy**: Ідентифікатор (`Id`) та `ISBN` не можуть бути порожніми.
- **BookCopy**: Не можна видати примірник, який уже виданий (повторна видача).
- **Loan**: Дата повернення при закритті не може бути раніше дати видачі.
- **Loan**: Не можна закрити видачу, яка вже була закрита.

## Сервісний шар та сховища (Лабораторна 5)
- **Інтерфейс сховища**: `IBookStore` (`src/Core/Abstractions/IBookStore.cs`)
- **Реалізації сховища**:
  - `InMemoryBookStore` — зберігання у пам'яті на базі `Dictionary` з початковими даними (`SampleData`).
  - `FileBookStore` — файлове JSON-сховище з кешуванням у пам'яті та серіалізацією через `BookCopyDto`.
- **Сервіс**: `LendingService` (`src/Core/Services/LendingService.cs`) — бізнес-операції `AddBook`, `IssueCopy`, `ReturnCopy`, `All`, `Find`.

## Доменні події та делегати (Лабораторна 6)
- **Подія:** Додано подію `Changed` у `LendingService` на базі стандартного делегата `EventHandler<TEventArgs>`.
- **Дані події:** Створено перелічення видів змін `LendingChangeKind` (`BookAdded`, `CopyIssued`, `CopyReturned`) та клас аргументів `LendingChangedEventArgs` (містить `CopyId`, `ReaderId`, `DueDate`, `At`).
- **Підписники:** У `Program.cs` реалізовано два підписники:
  1. Локальний метод `OnLendingChanged` для логування в консоль та файл `logs/app.log`.
  2. Лямбда-вираз для підрахунку подій (демонстрація замикання).
- **Фільтрація:** Використано делегат `Func<BookCopy, bool>` та LINQ-метод `Where` для пошуку за критерієм (`service.FindAll`).

## Звіти та відкладене виконання (Лабораторна 7)
У роботі реалізовано 4 LINQ-звіти (`Overdue`, `TopReaders`, `ByIsbn`, `Totals`) з використанням операторів `Where`, `GroupBy`, `Select`, `OrderBy` та агрегатних функцій.

**Демонстрація відкладеного виконання:**
LINQ-запити (наприклад, `Where`) не виконуються під час їхнього оголошення. Виконання відбувається лише під час ітерації або матеріалізації (виклику `Count()` чи `ToList()`).
Тому в демо-блоці перший виклик `query.Count()` і другий виклик дали різні числа: між ними до списку-джерела додали новий елемент, і запит перерахував колекцію заново. Водночас властивість `snapshot.Count` не змінилася після очищення джерела, оскільки `ToList()` зафіксував (матеріалізував) результат в окрему колекцію у пам'яті.

## Лабораторна робота 8 (Домен: Бібліотека)
Як запустити тести
Щоб запустити всі модульні тести (перевірка інваріантів, подій, звітів та скасування), виконайте команду в кореневій папці проєкту:dotnet test

Як перевірити асинхронне скасування (Ctrl+C)
Запустіть застосунок командою:dotnet run --project src/Cli -- --file

Під час виконання імпорту (якщо файл великий) швидко натисніть Ctrl+C у терміналі.

Програма має коректно перехопити скасування і вивести повідомлення: "Імпорт скасовано." замість довгого стектрейсу з помилкою OperationCanceledException.

## Налаштування та Конфігурація (Лабораторна 9)

### Профілі (Середовища)
Застосунок підтримує кілька профілів конфігурації:
- **Production** (за замовчуванням): використовує базовий `appsettings.json`, мінімальний лог `Warning`.
- **Development**: використовує `appsettings.Development.json` (перекриває базовий), детальне логування `Debug`.

### Таблиця ключів конфігурації
| Ключ в JSON | Опис | Типове значення |
| :--- | :--- | :--- |
| `EnvironmentName` | Назва поточного середовища | `Production` |
| `Logging:MinimumLevel` | Мінімальний рівень логування | `Warning` |
| `Storage:DataPath` | Шлях до папки з даними | `data` |
| `Storage:FileName` | Ім'я файлу каталогу | `items.json` |
| `Library:LoanDays` | Термін видачі книги (у днях) | `14` |
| `Integration:ApiKey` | API-ключ (Секрет) | *(зберігається у user-secrets)* |

### Перевизначення змінними середовища
Будь-який параметр можна перевизначити через змінні середовища (префікс `CROSSAPP_`). Для вкладених ключів використовується подвійне підкреслення `__`. 
Також підтримується legacy-змінна `CROSSAPP_DATA_PATH`, яка напряму перекриває шлях до даних.

**Приклади для PowerShell (Windows):**
```powershell
$env:CROSSAPP_ENVIRONMENT="Development"
$env:CROSSAPP_Storage__DataPath="C:\TempLibraryData"
$env:CROSSAPP_DATA_PATH="C:\LegacyData"
dotnet run --project src/Cli -- --file
```

**Приклади для Bash (Linux/macOS):**
```Bash
export CROSSAPP_ENVIRONMENT="Development"
export CROSSAPP_Storage__DataPath="/tmp/library_data"
export CROSSAPP_DATA_PATH="/tmp/legacy_data"
dotnet run --project src/Cli -- --file

# Або одноразово (inline) перед запуском:
CROSSAPP_ENVIRONMENT=Development dotnet run --project src/Cli -- --file
```
