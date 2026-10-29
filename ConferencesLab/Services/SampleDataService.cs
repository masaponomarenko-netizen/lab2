namespace ConferencesLab.Services;


public sealed class SampleDataService
{
    public Task<string> EnsureSampleXmlAsync() => CopyAssetAsync("conferences.xml");

    public Task<string> EnsureSampleXslAsync() => CopyAssetAsync("conferences.xsl");

    private static async Task<string> CopyAssetAsync(string assetName)
    {
        var targetPath = Path.Combine(FileSystem.AppDataDirectory, assetName);

        await using var source = await FileSystem.OpenAppPackageFileAsync(assetName);
        await using var target = File.Create(targetPath);
        await source.CopyToAsync(target);

        return targetPath;
    }
}
