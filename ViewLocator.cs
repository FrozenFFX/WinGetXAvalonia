// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using WinGetXAvalonia.ViewModels;

namespace WinGetXAvalonia;

/// <summary>
/// Given a view model, returns the corresponding view if possible.
/// </summary>
[RequiresUnreferencedCode(
    "Default implementation of ViewLocator involves reflection which may be trimmed away.",
    Url = "https://docs.avaloniaui.net/docs/concepts/view-locator")]
// kaneki
public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is null)
            return null;

        var ns = param.GetType().Namespace?
            .Replace("ViewModels", "Views", StringComparison.Ordinal);

        var name = param.GetType().Name
            .Replace("ViewModel", "View", StringComparison.Ordinal);

        var type = param.GetType().Assembly.GetType($"{ns}.{name}");

        if (type == null && param is ViewModels.PlaceholderViewModel)
        {
            type = typeof(Views.PlaceholderView);
        }

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = "Not Found: " + $"{ns}.{name}" };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
// kaneki
