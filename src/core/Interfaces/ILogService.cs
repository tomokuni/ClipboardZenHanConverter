namespace ClipboardZenHanConverter.Core.Interfaces;

public interface ILogService
{
    void LogException(Exception? ex, string source);
    void LogTrace(string message);
}
