using ConferencesLab.Core.Services;
using ConferencesLab.Core.Strategies;
using ConferencesLab.Services;
using ConferencesLab.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ConferencesLab;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        
        builder.Services.AddSingleton<IXmlSearchStrategy, SaxSearchStrategy>();
        builder.Services.AddSingleton<IXmlSearchStrategy, DomSearchStrategy>();
        builder.Services.AddSingleton<IXmlSearchStrategy, LinqSearchStrategy>();

        builder.Services.AddSingleton<XmlSearchContext>();
        builder.Services.AddSingleton<AttributeCatalogService>();
        builder.Services.AddSingleton<XsltTransformService>();

        builder.Services.AddSingleton<SampleDataService>();
        builder.Services.AddSingleton<FileImportService>();
        builder.Services.AddSingleton<IDialogService, DialogService>();

        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<MainPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
