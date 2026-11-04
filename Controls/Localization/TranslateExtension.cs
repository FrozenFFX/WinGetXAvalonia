// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using System.Globalization;
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

public sealed class TranslateBindingExtension : MarkupExtension
{
    public string Path { get; set; } = "";

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var manager = LocalizationManager.Current;
        if (manager is null)
            return new Binding(Path);

        var binding = new MultiBinding
        {
            Converter = new TranslateMultiValueConverter(manager)
        };
        binding.Bindings.Add(new Binding(Path));
        binding.Bindings.Add(new Binding(nameof(LocalizationManager.Language))
        {
            Source = manager
        });

        return binding;
    }

    private sealed class TranslateMultiValueConverter(LocalizationManager manager)
        : IMultiValueConverter
    {
        public object? Convert(
            IList<object?> values,
            Type targetType,
            object? parameter,
            CultureInfo culture)
            => values.Count > 0 && values[0] is string value
                ? manager.TranslateValue(value)
                : values.FirstOrDefault();
    }
}
