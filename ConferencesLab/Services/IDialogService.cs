namespace ConferencesLab.Services;

public interface IDialogService
{
    Task<bool> ConfirmAsync(string title, string message, string accept = "Так", string cancel = "Ні");

    Task AlertAsync(string title, string message);
}
