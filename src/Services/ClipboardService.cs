using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using ClipboardZenHanConverter.Core.Services;

namespace ClipboardZenHanConverter.Services;

public class ClipboardService : IClipboardService, IDisposable
{
    public event EventHandler<object>? ContentChanged;

    private bool _disposed;

    public ClipboardService()
    {
        Clipboard.ContentChanged += OnClipboardContentChanged;
    }

    private void OnClipboardContentChanged(object? sender, object e)
    {
        ContentChanged?.Invoke(this, e);
    }

    public void Dispose()
    {
        if (_disposed) return;
        Clipboard.ContentChanged -= OnClipboardContentChanged;
        _disposed = true;
    }

    public async Task<string?> GetTextAsync()
    {
        try
        {
            var dataPackageView = Clipboard.GetContent();
            if (dataPackageView.Contains(StandardDataFormats.Text))
            {
                return await dataPackageView.GetTextAsync();
            }
        }
        catch
        {
            // Access denied or other errors
        }
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
        catch
        {
            // Ignore errors
        }
    }

    public void Flush()
    {
        try
        {
            Clipboard.Flush();
        }
        catch
        {
            // Ignore errors
        }
    }
}
