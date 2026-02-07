using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using ClipboardZenHanConverter.Core.Services;

namespace ClipboardZenHanConverter.Services;

public class ClipboardService : IClipboardService
{
    public event EventHandler<object>? ContentChanged;

    public ClipboardService()
    {
        Clipboard.ContentChanged += (s, e) => ContentChanged?.Invoke(this, e);
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
