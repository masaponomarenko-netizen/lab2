# Лабораторна робота №2. Робота з файлами XML

**Варіант 20 — «Конференції»**

Застосунок .NET MAUI для аналізу XML-документа з науковими конференціями
трьома способами (SAX, DOM, LINQ to XML) та трансформації документа в HTML
за допомогою XSL.

## Як запустити

Потрібні .NET SDK 10, MAUI-навантаження `maui-maccatalyst` (macOS) або
`maui-windows` (Windows) і Xcode (для macOS).

```bash
dotnet build ConferencesLab/ConferencesLab.csproj -f net10.0-maccatalyst
dotnet run --project ConferencesLab/ConferencesLab.csproj -f net10.0-maccatalyst
```

На Windows цільова платформа `net10.0-windows10.0.19041.0` підключається
автоматично. У Rider проєкт `ConferencesLab` можна запустити звичайною
кнопкою Run.

Після старту натисніть **«Приклад»**, щоб відкрити вбудований
`conferences.xml`, або **«Обрати XML…»** і виберіть власний файл.

## Що реалізовано

### 1. Вхідний XML (`ConferencesLab/Resources/Raw/conferences.xml`)

- Головний вузол пошуку — `<conference>`.
- Кількість атрибутів у різних вузлів **різна**: від 4 до 9
  (`id`, `title`, `category`, `format`, `fee`, `currency`, `language`,
  `seats`, `website`).
- Структура має 3 рівні: `conferences → conference → dates/topic/venue/...`,
  тобто вимога «не менше 2 рівнів» виконана.
- Дані: назва, терміни проведення, тематика, організатор, місце,
  дедлайн подання рукописів, вартість участі тощо.

### 2. Шаблон «Стратегія»

```
IXmlSearchStrategy
├── SaxSearchStrategy   — потоковий XmlReader, без побудови дерева
├── DomSearchStrategy   — XmlDocument + динамічний XPath
└── LinqSearchStrategy  — XDocument + динамічний ланцюжок Where()
```

`XmlSearchContext` вибирає стратегію за назвою; UI (ViewModel) не знає
подробиць читання XML. Додати четвертий спосіб можна без зміни UI.

### 3. Аналіз вмісту

- Пошук за атрибутом і його значенням, які **реально присутні у файлі**.
- Пошук за ключовим словом (регістронезалежний, по атрибутах і тексту).
- Динамічна генерація запитів:
  - DOM — XPath `//conference[@category='...'][contains(...)]`;
  - LINQ — `doc.Descendants("conference").Where(...)`;
  - SAX — потокова перевірка під час читання вузла.
- У полі результату показується стратегія, згенерований запит, кількість
  знайдених вузлів, час виконання та всі атрибути/поля кожної конференції.

### 4. Трансформація XML → HTML

`ConferencesLab/Resources/Raw/conferences.xsl` — XSLT 1.0 шаблон, який
формує адаптивний HTML-звіт з картками конференцій. Трансформація
виконується через `XslCompiledTransform`, готує файл
`conferences-report.html` у робочій теці застосунку та відкриває його
в браузері.

### 5. Вимоги до інтерфейсу

- **Підгрузка даних**: після вибору файлу `AttributeCatalogService`
  збирає всі атрибути головного вузла та їхні значення. Списки «Атрибут»
  і «Значення» заповнюються лише тим, що є в документі.
- **Підтвердження виходу**: кнопка «Вихід» показує діалог
  «Чи дійсно ви хочете завершити роботу з програмою?» з кнопками «Так»/«Ні».
- **Clear**: очищує поле виводу, ключове слово, вибраний атрибут і значення.

## Структура рішення

```
lab2.sln
├── ConferencesLab.Core/            # незалежна від UI бібліотека
│   ├── Models/                     # Conference, SearchQuery, AttributeCatalog
│   ├── Strategies/                 # SAX / DOM / LINQ to XML
│   └── Services/                   # XmlSearchContext, каталог атрибутів, XSLT
└── ConferencesLab/                 # .NET MAUI застосунок
    ├── ViewModels/MainViewModel.cs # MVVM, команди, стани
    ├── Services/                   # FilePicker, діалоги, вбудовані файли
    ├── MainPage.xaml               # інтерфейс
    └── Resources/Raw/              # conferences.xml, conferences.xsl
```

## Приклади перевірки

| Параметри | Очікування |
|---|---|
| Атрибут `category`, значення `Artificial Intelligence` | 1 конференція |
| Атрибут `format`, значення `hybrid` | 2 конференції |
| Ключове слово `київ` | 3 конференції |
| Ключове слово `english` | 3 конференції |
| `category = Software Engineering` + слово `devops` | 1 конференція |

Усі три стратегії для однакового запиту повертають однаковий набір
результатів — це перевірено окремим smoke-тестом під час розробки.
