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