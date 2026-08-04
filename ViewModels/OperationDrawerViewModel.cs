// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinGetXAvalonia.Models.Operations;
using WinGetXAvalonia.Services.Operations;

namespace WinGetXAvalonia.ViewModels;

public partial class OperationDrawerViewModel : ViewModelBase
{
    private readonly IOperationCenter _center;

    public OperationDrawerViewModel(IOperationCenter center)
    {
        _center = center;
        _center.Operations.CollectionChanged += OnOperationsChanged;
    }

    public ObservableCollection<OperationModel> Operations => _center.Operations;

    public bool HasOperations => Operations.Count > 0;

    [ObservableProperty]
    private bool isOpen;

    public int RunningCount => Operations.Count(x => x.Status == OperationStatus.Running);

    public int PendingCount => Operations.Count(x => x.Status == OperationStatus.Waiting);

    public int CompletedCount => Operations.Count(x => x.Status == OperationStatus.Completed);

    public int FailedCount => Operations.Count(x => x.Status == OperationStatus.Failed);

    public string Summary
        => $"{RunningCount} Running • {PendingCount} Pending • {CompletedCount} Completed";

    [RelayCommand]
    private void ClearCompleted()
    {
        foreach (var operation in Operations
            .Where(x => x.Status == OperationStatus.Completed)
            .ToList())
        {
            Operations.Remove(operation);
        }

        OnPropertyChanged(nameof(Summary));
        OnPropertyChanged(nameof(HasOperations));
    }

    private void OnOperationsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasOperations));
        OnPropertyChanged(nameof(Summary));
    }
}
