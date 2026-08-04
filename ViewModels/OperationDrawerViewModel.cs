using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
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

    private void OnOperationsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => OnPropertyChanged(nameof(HasOperations));
}
