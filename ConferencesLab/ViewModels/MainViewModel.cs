using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using ConferencesLab.Core.Models;
using ConferencesLab.Core.Services;
using ConferencesLab.Services;

namespace ConferencesLab.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly XmlSearchContext _searchContext;
    private readonly AttributeCatalogService _catalogService;
    private readonly XsltTransformService _xsltService;
    private readonly SampleDataService _sampleData;
    private readonly FileImportService _fileImport;
    private readonly IDialogService _dialogs;

    private string? _xmlPath;
    private AttributeCatalog? _catalog;

    private string _selectedStrategy = string.Empty;
    private string? _selectedAttribute;
    private string? _selectedValue;
    private string _keyword = string.Empty;
    private string _output = string.Empty;
    private string _status = "Виберіть XML-файл або відкрийте вбудований приклад.";
    private string _fileName = "Файл не вибрано";
    private string _documentInfo = string.Empty;
    private bool _isBusy;
    private bool _hasDocument;
    private bool _isReportVisible;
    private HtmlWebViewSource? _reportSource;

    public MainViewModel(
        XmlSearchContext searchContext,
        AttributeCatalogService catalogService,
        XsltTransformService xsltService,
        SampleDataService sampleData,
        FileImportService fileImport,
        IDialogService dialogs)
    {
        _searchContext = searchContext;
        _catalogService = catalogService;
        _xsltService = xsltService;
        _sampleData = sampleData;
        _fileImport = fileImport;
        _dialogs = dialogs;

        foreach (var name in _searchContext.Names)
        {
            Strategies.Add(name);
        }

        _selectedStrategy = Strategies.FirstOrDefault() ?? string.Empty;

        PickFileCommand = new Command(() => _ = PickFileSafeAsync());
        LoadSampleCommand = new Command(() => _ = RunAsync(LoadSampleAsync));
        SearchCommand = new Command(() => _ = RunAsync(SearchAsync));
        ClearCommand = new Command(Clear);
        TransformCommand = new Command(() => _ = RunAsync(TransformAsync));
        CloseReportCommand = new Command(() => IsReportVisible = false);
        ExitCommand = new Command(() => _ = RunAsync(ExitAsync));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<string> Strategies { get; } = new();

    public ObservableCollection<string> AttributeNames { get; } = new();

    public ObservableCollection<string> AttributeValues { get; } = new();

    public string SelectedStrategy
    {
        get => _selectedStrategy;
        set => SetProperty(ref _selectedStrategy, value);
    }

    public string? SelectedAttribute
    {
        get => _selectedAttribute;
        set
        {
            if (SetProperty(ref _selectedAttribute, value))
            {
                RefreshAttributeValues();
            }
        }
    }

    public string? SelectedValue
    {
        get => _selectedValue;
        set => SetProperty(ref _selectedValue, value);
    }

    public string Keyword
    {
        get => _keyword;
        set => SetProperty(ref _keyword, value);
    }

    public string Output
    {
        get => _output;
        set => SetProperty(ref _output, value);
    }

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public string FileName
    {
        get => _fileName;
        set => SetProperty(ref _fileName, value);
    }

    public string DocumentInfo
    {
        get => _documentInfo;
        set => SetProperty(ref _documentInfo, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public bool HasDocument
    {
        get => _hasDocument;
        set => SetProperty(ref _hasDocument, value);
    }

    public bool IsReportVisible
    {
        get => _isReportVisible;
        set => SetProperty(ref _isReportVisible, value);
    }

    public HtmlWebViewSource? ReportSource
    {
        get => _reportSource;
        set => SetProperty(ref _reportSource, value);
    }

    public ICommand PickFileCommand { get; }

    public ICommand LoadSampleCommand { get; }

    public ICommand SearchCommand { get; }

    public ICommand ClearCommand { get; }

    public ICommand TransformCommand { get; }

    public ICommand CloseReportCommand { get; }

    public ICommand ExitCommand { get; }

    
    
    
    
    private async Task PickFileSafeAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            Status = "Відкрито системний діалог вибору файлу. Якщо його не видно — натисніть Esc.";

            var file = await FilePicker.Default
                .PickAsync(new PickOptions { PickerTitle = "Оберіть XML-файл з конференціями" })
                .WaitAsync(TimeSpan.FromMinutes(2));

            if (file is null)
            {
                Status = "Вибір файлу скасовано.";
                return;
            }

            if (!file.FileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            {
                await _dialogs.AlertAsync("Непідтримуваний файл", "Будь ласка, оберіть файл з розширенням .xml.");
                return;
            }

            IsBusy = true;

            try
            {
                var localPath = await _fileImport.ImportAsync(file);
                LoadDocument(localPath, file.FileName);
            }
            finally
            {
                IsBusy = false;
            }
        }
        catch (TimeoutException)
        {
            Status = "Діалог вибору файлу не відповів. Спробуйте ще раз або натисніть «Приклад XML».";
            await _dialogs.AlertAsync(
                "Файл не вибрано",
                "Системний діалог не відповів. Натисніть «Приклад XML», щоб відкрити вбудований файл, або спробуйте вибрати файл ще раз.");
        }
        catch (Exception exception)
        {
            Status = $"Помилка вибору файлу: {exception.Message}";
            await _dialogs.AlertAsync("Помилка", exception.Message);
        }
    }

    private async Task LoadSampleAsync()
    {
        var path = await _sampleData.EnsureSampleXmlAsync();
        LoadDocument(path, "conferences.xml (вбудований приклад)");
    }

    private void LoadDocument(string path, string displayName)
    {
        _catalog = _catalogService.Load(path);
        _xmlPath = path;
        HasDocument = true;

        FileName = displayName;
        DocumentInfo = $"Конференцій: {_catalog.ConferenceCount} · різних атрибутів: {_catalog.AttributeNames.Count}";

        AttributeNames.Clear();
        foreach (var attributeName in _catalog.AttributeNames)
        {
            AttributeNames.Add(attributeName);
        }

        SelectedAttribute = null;
        SelectedAttribute = AttributeNames.FirstOrDefault();

        Output = string.Empty;
        Status = "Файл завантажено. Значення атрибутів підвантажено з документа — оберіть потрібне.";
    }

    private void RefreshAttributeValues()
    {
        AttributeValues.Clear();
        SelectedValue = null;

        if (_catalog is null || string.IsNullOrWhiteSpace(SelectedAttribute))
        {
            return;
        }

        foreach (var value in _catalog.GetValues(SelectedAttribute))
        {
            AttributeValues.Add(value);
        }

        SelectedValue = AttributeValues.FirstOrDefault();
    }

    private async Task SearchAsync()
    {
        if (_xmlPath is null)
        {
            await _dialogs.AlertAsync("Файл не вибрано", "Спочатку виберіть XML-файл або відкрийте приклад.");
            return;
        }

        if (string.IsNullOrWhiteSpace(SelectedAttribute) && string.IsNullOrWhiteSpace(Keyword))
        {
            await _dialogs.AlertAsync(
                "Немає параметрів пошуку",
                "Оберіть атрибут зі списку або введіть ключове слово.");
            return;
        }

        var query = new SearchQuery(SelectedAttribute, SelectedValue, Keyword);
        var result = _searchContext.Search(SelectedStrategy, _xmlPath, query);

        Output = FormatResult(result);
        Status = $"«{SelectedStrategy}»: знайдено {result.Items.Count} з {result.TotalScanned} " +
                 $"за {result.Elapsed.TotalMilliseconds:F1} мс.";
    }

    private string FormatResult(SearchResult result)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Стратегія: {SelectedStrategy}");
        builder.AppendLine($"Запит: {result.QueryDescription}");
        builder.AppendLine($"Знайдено: {result.Items.Count} з {result.TotalScanned}");
        builder.AppendLine($"Час виконання: {result.Elapsed.TotalMilliseconds:F2} мс");
        builder.AppendLine(new string('─', 64));

        if (result.Items.Count == 0)
        {
            builder.AppendLine("Нічого не знайдено. Змініть атрибут, значення або ключове слово.");
            return builder.ToString();
        }

        for (var i = 0; i < result.Items.Count; i++)
        {
            builder.AppendLine(result.Items[i].ToDisplayString(i + 1));
            builder.AppendLine();
        }

        return builder.ToString().TrimEnd();
    }

    private void Clear()
    {
        Output = string.Empty;
        Keyword = string.Empty;
        SelectedAttribute = null;
        SelectedValue = null;
        Status = "Поле виводу та параметри пошуку очищено.";
    }

    private async Task TransformAsync()
    {
        if (_xmlPath is null)
        {
            await _dialogs.AlertAsync("Файл не вибрано", "Спочатку виберіть XML-файл або відкрийте приклад.");
            return;
        }

        var xslPath = await _sampleData.EnsureSampleXslAsync();
        var outputPath = Path.Combine(FileSystem.AppDataDirectory, "conferences-report.html");

        _xsltService.Transform(_xmlPath, xslPath, outputPath);

        Output = $"HTML-звіт успішно створено за XSL-шаблоном:\n{outputPath}";
        Status = "Трансформацію XML → HTML виконано.";

        try
        {
            var html = await File.ReadAllTextAsync(outputPath);
            ReportSource = new HtmlWebViewSource { Html = html };
            IsReportVisible = true;
        }
        catch (Exception exception)
        {
            Status = $"HTML створено, але не вдалося відкрити перегляд: {exception.Message}";
        }
    }

    private async Task ExitAsync()
    {
        var confirmed = await _dialogs.ConfirmAsync(
            "Завершення роботи",
            "Чи дійсно ви хочете завершити роботу з програмою?");

        if (confirmed)
        {
            Application.Current?.Quit();
        }
    }

    private async Task RunAsync(Func<Task> action)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;

        try
        {
            await action();
        }
        catch (Exception exception)
        {
            Status = $"Помилка: {exception.Message}";
            await _dialogs.AlertAsync("Помилка", exception.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
