namespace WinGetXAvalonia.Core.Operations;

public interface IOperationProgress
{
    void Report(int percentage, string message);
}
