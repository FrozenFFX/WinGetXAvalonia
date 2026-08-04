// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System;
using System.Threading.Tasks;

namespace WinGetXAvalonia.Services.Dialogs;

public interface IDialogService
{
    Task<bool> ConfirmAsync(string title, string message, string? confirmText = null, string? cancelText = null);

    Task ProgressAsync(string title, Func<IProgress<double>, Task> operation);

    Task ShowErrorAsync(string title, string message);
}
