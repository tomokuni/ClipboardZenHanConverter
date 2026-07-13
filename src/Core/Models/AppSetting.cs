using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ClipboardZenHanConverter.Core.Models;

public partial class AppSetting : SettingsPersistenceBase<AppSetting>
{
    [ObservableProperty]
    public partial double WindowWidth { get; set; } = 1000;

    [ObservableProperty]
    public partial double WindowHeight { get; set; } = 800;

    protected override (JsonSerializerContext Context, Type Type) SerializeInfo
        => (AppJsonContext.Default, typeof(AppSetting));

    public AppSetting()
    {
        AutoSaveFileName = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ClipboardZenHanConverter",
            "AppSetting.json");
    }

    protected override void ApplyFrom(AppSetting other)
    {
        WindowWidth = other.WindowWidth;
        WindowHeight = other.WindowHeight;
    }
}
