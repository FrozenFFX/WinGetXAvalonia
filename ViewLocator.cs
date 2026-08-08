// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using WinGetXAvalonia.ViewModels;

namespace WinGetXAvalonia;

[RequiresUnreferencedCode("Reflection is used to map view models to views.")]
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

        if (type == null)
            return new TextBlock { Text = $"{ns}.{name} not found" };

        var control = (Control)Activator.CreateInstance(type)!;

        return control;
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
