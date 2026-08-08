// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WinGetXAvalonia.Models;

public class NavigationItem : INotifyPropertyChanged
{
    private bool _isExpanded = true;

    private NavigationItem? _parentCategory;

    private readonly List<NavigationItem> _children = new();

    public string Title { get; set; } = "";

    public string Icon { get; set; } = "";

    public Type Page { get; set; } = typeof(object);

    public string Category { get; set; } = "";

    public int BadgeCount { get; set; }

    public bool IsCategoryHeader { get; set; }

    // Başlığın altındaki öğeler. Başlık daraltılınca bildirim alırlar.
    public IReadOnlyList<NavigationItem> Children => _children;

    // Kategori başlığına bağlı alt öğe. Başlığı olmayan öğeler her zaman görünür.
    public NavigationItem? ParentCategory
    {
        get => _parentCategory;
        set
        {
            _parentCategory = value;
            OnPropertyChanged();
        }
    }

    // Kategori başlığının açık/kapalı durumu.
    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded == value)
                return;

            _isExpanded = value;
            OnPropertyChanged();

            // Alt öğelerin görünürlüğü başlığın durumuna bağlı.
            foreach (var child in _children)
                child.NotifyVisibilityChanged();
        }
    }

    // Alt öğenin görünürlüğü: başlığı yoksa ya da başlık açıksa görünür.
    public bool IsVisible => ParentCategory is null || ParentCategory.IsExpanded;

    public void AddChild(NavigationItem child)
        => _children.Add(child);

    public void NotifyVisibilityChanged()
        => OnPropertyChanged(nameof(IsVisible));

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
