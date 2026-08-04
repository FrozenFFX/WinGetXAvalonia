using System.Collections.ObjectModel;
using WinGetXAvalonia.Models.Operations;

namespace WinGetXAvalonia.Services.Operations;

public interface IOperationCenter
{
    ObservableCollection<OperationModel> Operations { get; }

    OperationModel Start(string title, string subtitle);

    void ReportProgress(Guid id, double progress, string subtitle);

    void Complete(Guid id);

    void Fail(Guid id, string error);

    void Remove(Guid id);

    void ClearCompleted();
}
