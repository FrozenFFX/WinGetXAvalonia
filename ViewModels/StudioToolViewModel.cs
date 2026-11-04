// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using CommunityToolkit.Mvvm.ComponentModel;
using Avalonia.Threading;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Localization;

namespace WinGetXAvalonia.ViewModels;

public partial class StudioToolViewModel : ObservableObject
{
    public StudioTool Tool { get; }

    private readonly LocalizationManager? _localization;

    [ObservableProperty]
    private bool isFavorite;

    public StudioToolViewModel(
        StudioTool tool,
        LocalizationManager? localization = null,
        bool isFavorite = false)
    {
        Tool = tool;
        _localization = localization;
        IsFavorite = isFavorite;

        if (_localization is not null)
            _localization.LanguageChanged += OnLanguageChanged;
    }

    public string Icon => Tool.Icon;

    public string Title => Translate(Tool.Title);

    public string Description => Translate(Tool.Description);

    public string Category => Translate(Tool.Category);

    public StudioToolStatus Status => Tool.Status;

    public bool CanOpen => Status == StudioToolStatus.Ready;

    public string Badge => Tool.Status switch
    {
        StudioToolStatus.Experimental => Translate("Experimental"),
        StudioToolStatus.ComingSoon => Translate("Coming Soon"),
        _ => Translate("Ready")
    };

    public string BadgeStatus => Tool.Status switch
    {
        StudioToolStatus.Experimental => "warning",
        StudioToolStatus.ComingSoon => "info",
        _ => "success"
    };

    private string Translate(string value)
        => _localization?.TranslateValue(value) ?? value;

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        void Notify()
        {
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(Description));
            OnPropertyChanged(nameof(Category));
            OnPropertyChanged(nameof(Badge));
        }

        if (Dispatcher.UIThread.CheckAccess())
            Notify();
        else
            Dispatcher.UIThread.Post(Notify);
    }
}
