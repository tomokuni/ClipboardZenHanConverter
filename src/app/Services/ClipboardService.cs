using Windows.ApplicationModel.DataTransfer;
using ClipboardZenHanConverter.Core.Interfaces;

namespace ClipboardZenHanConverter.Services;

public class ClipboardService : IClipboardService
{
    public event EventHandler<object>? ContentChanged;

    public ClipboardService()
    {
        Clipboard.ContentChanged += OnClipboardContentChanged;
    }

    private void OnClipboardContentChanged(object? sender, object e)
        => ContentChanged?.Invoke(this, e);

    public void Dispose()
    {
        Clipboard.ContentChanged -= OnClipboardContentChanged;
    }

    public async Task<string?> GetTextAsync()
    {
        try
        {
            var dataPackageView = Clipboard.GetContent();
            if (dataPackageView.Contains(StandardDataFormats.Text))
                return await dataPackageView.GetTextAsync();
        }
        catch { }
        return null;
    }

    public void SetText(string text)
    {
        try
        {
            var data = new DataPackage();
            data.SetText(text);
            Clipboard.SetContent(data);
        }
        catch { }
    }

    public void Flush()
    {
        try { Clipboard.Flush(); }
        catch { }
    }
}
