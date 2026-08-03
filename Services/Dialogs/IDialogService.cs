// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
using System;
using System.Threading.Tasks;

namespace WinGetXAvalonia.Services.Dialogs;

// kaneki
public interface IDialogService
{
    Task<bool> ConfirmAsync(string title, string message, string? confirmText = null, string? cancelText = null);

    Task ProgressAsync(string title, Func<IProgress<double>, Task> operation);

    Task ShowErrorAsync(string title, string message);
}
// kaneki
