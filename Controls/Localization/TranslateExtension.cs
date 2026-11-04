// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia.Data;
using Avalonia.Markup.Xaml;
using WinGetXAvalonia.Services.Localization;

namespace WinGetXAvalonia.Controls.Localization;

public sealed class TranslateExtension : MarkupExtension
{
    public string Key { get; set; } = "";

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var manager = LocalizationManager.Current;
        if (manager is null)
            return Key;

        return new Binding(nameof(LocalizedText.Value))
        {
            Source = manager.GetText(Key)
        };
    }
}
