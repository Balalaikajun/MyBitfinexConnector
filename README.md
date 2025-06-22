# MyBitfinexConnector
## Описание
Данный проект является решением [тестового задания](Docs/ТестовоеЗадание.docx) на позицию C# Junior разработчика. 

**Задача:** 
1. Реализовать коннектор под исходных [интерфейс](Docs/TestData/ITestConnector.cs) на C# (Class Library), для API биржи Bitfinex .
2. Покрыть библиотеку интеграционными тестами.
3. Реализовать простой UI (WPF) на основе MVVM для показа возможностей библиотеки.

Решение разрабатывалось на протяжении 4 дней кропотливой работы. Описание выбранных решений и проекта приведено в [отчёте](Docs/REPORT.md).

## Установка и запуск
1. Клонируйте репозиторий.
```bash
git clone https://github.com/Balalaikajun/MyBitfinexConnector
```
2. Соберите решение.
```bash
cd MyBitfinexConnector
dotnet build
```
3. Запустите UI 
```bash
dotnet run --project ./TestHQ.UI/TestHQ.UI.csproj
```

