namespace ConferencesLab.Services;

public sealed class DialogService : IDialogService
{
    private static Page? CurrentPage =>
        Application.Current?.Windows.FirstOrDefault()?.Page;

    public async Task<bool> ConfirmAsync(string title, string message, string accept = "Так", string cancel = "Ні")
    {
        var page = CurrentPage;
        if (page is null)
        {
            return false;
        }

        return await page.DisplayAlertAsync(title, message, accept, cancel);
    }

    public async Task AlertAsync(string title, string message)
    {
        var page = CurrentPage;
        if (page is not null)
        {
            await page.DisplayAlertAsync(title, message, "OK");
        }
    }
}
