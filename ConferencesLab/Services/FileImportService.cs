namespace ConferencesLab.Services;





public sealed class FileImportService
{
    public async Task<string> ImportAsync(FileResult file)
    {
        var safeName = string.Concat(
            Path.GetFileName(file.FileName)
                .Select(character => Path.GetInvalidFileNameChars().Contains(character) ? '_' : character));

        var targetPath = Path.Combine(FileSystem.AppDataDirectory, $"imported-{safeName}");

        await using var source = await file.OpenReadAsync();
        await using var target = File.Create(targetPath);
        await source.CopyToAsync(target);

        return targetPath;
    }
}
