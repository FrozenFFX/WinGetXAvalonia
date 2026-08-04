// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia;
using Avalonia.Controls;

namespace WinGetXAvalonia.Controls.Studio;

public partial class StudioBadge : UserControl
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<StudioBadge, string?>(nameof(Text));

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly StyledProperty<string> StatusProperty =
        AvaloniaProperty.Register<StudioBadge, string>(nameof(Status), "info");

    public string Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public StudioBadge()
    {
        InitializeComponent();
        ApplyTone(Status);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == StatusProperty && PartBadge is not null)
            ApplyTone(Status);
    }

    private void ApplyTone(string tone)
    {
        PartBadge.Classes.Set("info", tone == "info");
        PartBadge.Classes.Set("success", tone == "success");
        PartBadge.Classes.Set("warning", tone == "warning");
        PartBadge.Classes.Set("danger", tone == "danger");

        PartText.Classes.Set("info", tone == "info");
        PartText.Classes.Set("success", tone == "success");
        PartText.Classes.Set("warning", tone == "warning");
        PartText.Classes.Set("danger", tone == "danger");
    }
}
