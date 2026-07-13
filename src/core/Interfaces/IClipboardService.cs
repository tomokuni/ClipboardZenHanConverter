namespace ClipboardZenHanConverter.Core.Interfaces;

public interface IClipboardService : IDisposable
{
    event EventHandler<object>? ContentChanged;
    Task<string?> GetTextAsync();
    void SetText(string text);
    void Flush();
}
