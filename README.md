# Library

Наскрізний проєкт з крос-платформного програмування.

Предметна область: Бібліотека.
Сутності: Book, BookCopy, Reader, Loan.
Призначення: облік видач примірників книг читачам.

## Структура solution

Library/
├── .gitignore
├── Library.slnx
├── README.md
└── src/
....├── Core/ (Бібліотека класів)
....│   ├── Core.csproj
....│   └── EnvironmentInfo.cs
....└── Cli/ (Консольний застосунок)
........├── Cli.csproj
........└── Program.cs

## Команди

Збірка проєкту: dotnet build
Запуск проєкту: dotnet run --project src/Cli
Публікація (збирання готового файлу під Windows x64): dotnet publish src/Cli -c Release -r win-x64 --self-contained true

## Порівняння режимів публікації

RID         Режим (Self-contained)          Розмір          Чи потрібен встановлений runtime
win-x64     true (Самодостатній)            76 MB           Ні
win-x64     false (Залежний)                < 1 MB          Так

Середовище: .NET SDK 10.0, Windows 11 x64

