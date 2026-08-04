using System.Collections.ObjectModel;
using WinGetXAvalonia.Models.Operations;

namespace WinGetXAvalonia.Services.Operations;

public class OperationCenter : IOperationCenter
{
    public ObservableCollection<OperationModel> Operations { get; }
        = new();

    public OperationModel Start(string title, string subtitle)
    {
        var op = new OperationModel
        {
            Title = title,
            Subtitle = subtitle,
            Status = OperationStatus.Running,
            Progress = 0
        };

        Operations.Insert(0, op);

        return op;
    }

    public void ReportProgress(Guid id, double progress, string subtitle)
    {
        var op = Operations.FirstOrDefault(x => x.Id == id);

        if (op is null)
            return;

        op.Progress = progress;
        op.Subtitle = subtitle;
    }

    public void Complete(Guid id)
    {
        var op = Operations.FirstOrDefault(x => x.Id == id);

        if (op is null)
            return;

        op.Progress = 100;
        op.Status = OperationStatus.Completed;
    }

    public void Fail(Guid id, string error)
    {
        var op = Operations.FirstOrDefault(x => x.Id == id);

        if (op is null)
            return;

        op.Status = OperationStatus.Failed;
        op.Subtitle = error;
    }

    public void Remove(Guid id)
    {
        var op = Operations.FirstOrDefault(x => x.Id == id);

        if (op != null)
            Operations.Remove(op);
    }

    public void ClearCompleted()
    {
        foreach (var item in Operations
            .Where(x => x.Status == OperationStatus.Completed)
            .ToList())
        {
            Operations.Remove(item);
        }
    }
}
