using WinGetXAvalonia.ViewModels;
using WinGetXAvalonia.Views;

namespace WinGetXAvalonia.Tests;

public class ViewLocatorTests
{
    [Theory]
    [InlineData(typeof(WorkspacesViewModel))]
    [InlineData(typeof(BlueprintsViewModel))]
    [InlineData(typeof(AutomationViewModel))]
    [InlineData(typeof(SecurityViewModel))]
    [InlineData(typeof(DeveloperViewModel))]
    public void PlaceholderModules_ResolveToPlaceholderView(Type viewModelType)
    {
        var vm = (ViewModelBase)Activator.CreateInstance(viewModelType)!;
        Assert.IsType<PlaceholderView>(new ViewLocator().Build(vm));
    }
}
