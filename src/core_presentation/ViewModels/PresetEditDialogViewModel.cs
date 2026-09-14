using ClipboardZenHanConverter.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text;

namespace ClipboardZenHanConverter.Presentation.ViewModels;

/// <summary>プリセット編集ダイアログのデータと検証を管理します。</summary>
/// <remarks>
/// 提供機能: <br/>
/// - プリセット名の入力と検証（組込み・使用不可文字・built-in キーワード・重複）<br/>
/// - 保存/削除の可否判定と実行<br/>
/// - 閉じる要求の通知<br/><br/>
/// 特徴: <br/>
/// - 検証は名前の変更時にリアルタイム実行し、メッセージと各ボタンの有効/無効を更新<br/>
/// - 検証ロジックを純粋関数（internal static）として公開し、UI なしでテスト可能<br/><br/>
/// 状態遷移: <br/>
/// | 条件 | メッセージ | 保存 | 削除 |<br/>
/// | --- | --- | --- | --- |<br/>
/// | 空文字 | なし | 無効 | 無効 |<br/>
/// | 組込みプリセット | 組込みプリセットです。 | 無効 | 無効 |<br/>
/// | 使用不可文字を含む | 使用できない文字が含まれます。 | 無効 | 既存なら有効 |<br/>
/// | built-in/builtin を含む | built-in または builtin は使用できません。 | 無効 | 既存なら有効 |<br/>
/// | 既存ユーザープリセット | 既に存在します。 | 有効 | 有効 |<br/>
/// | 新規の有効な名前 | なし | 有効 | 無効 |<br/><br/>
/// 注意点: <br/>
/// - ダイアログを閉じる処理は View が <see cref="SetCloseAction"/> で注入します
/// </remarks>
public partial class PresetEditDialogViewModel : ObservableObject
{
    /// <summary>設定画面の ViewModel（プリセット操作の委譲先）。</summary>
    private readonly SettingsViewModel _settings;

    /// <summary>ダイアログを閉じる要求を通知するアクション。View 側が設定します。</summary>
    private Action? _closeAction;

    /// <summary>入力されたプリセット名を取得または設定します。</summary>
    [ObservableProperty]
    public partial string PresetName { get; set; } = string.Empty;

    /// <summary>検証エラーメッセージを取得します。null の場合はエラーなし。</summary>
    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    /// <summary>保存ボタンが有効かどうかを取得します。</summary>
    [ObservableProperty]
    public partial bool CanSave { get; set; }

    /// <summary>削除ボタンが有効かどうかを取得します。</summary>
    [ObservableProperty]
    public partial bool CanDelete { get; set; }

    /// <summary>PresetEditDialogViewModel の新しいインスタンスを初期化します。</summary>
    /// <param name="settings">設定画面の ViewModel。</param>
    public PresetEditDialogViewModel(SettingsViewModel settings)
    {
        _settings = settings;
        Validate();
    }

    /// <summary>ダイアログを閉じるためのアクションを設定します。</summary>
    /// <param name="closeAction">ダイアログを閉じるアクション。</param>
    public void SetCloseAction(Action closeAction) => _closeAction = closeAction;

    /// <summary>入力された名前の変更時に検証を実行します。</summary>
    /// <param name="value">新しいプリセット名。</param>
    partial void OnPresetNameChanged(string value) => Validate();

    /// <summary>現在の入力内容を検証し、メッセージと各ボタンの有効/無効を更新します。</summary>
    private void Validate()
    {
        var name = PresetName;
        var userPresets = UserPresetNames();
        var isExistingUserPreset = userPresets.Contains(name, StringComparer.Ordinal);

        if (string.IsNullOrWhiteSpace(name))
        {
            ErrorMessage = null;
            CanSave = false;
            CanDelete = false;
            return;
        }

        if (ConvertConfig.IsBuiltInPreset(name))
        {
            ErrorMessage = "組込みプリセットです。";
            CanSave = false;
            CanDelete = false;
            return;
        }

        if (ContainsInvalidFileNameChars(name))
        {
            ErrorMessage = "使用できない文字が含まれます。";
            CanSave = false;
            CanDelete = isExistingUserPreset;
            return;
        }

        if (ContainsBuiltInKeyword(name))
        {
            ErrorMessage = "built-in または builtin は使用できません。";
            CanSave = false;
            CanDelete = isExistingUserPreset;
            return;
        }

        if (isExistingUserPreset)
        {
            ErrorMessage = "既に存在します。";
            CanSave = true;
            CanDelete = true;
            return;
        }

        ErrorMessage = null;
        CanSave = true;
        CanDelete = false;
    }

    /// <summary>現在の設定を入力された名前で保存し、ダイアログを閉じます。</summary>
    [RelayCommand]
    private void Save()
    {
        if (!CanSave) return;
        _settings.SavePreset(PresetName);
        _closeAction?.Invoke();
    }

    /// <summary>入力された名前のユーザープリセットを削除し、ダイアログを閉じます。</summary>
    [RelayCommand]
    private void Delete()
    {
        if (!CanDelete) return;
        _settings.DeletePreset(PresetName);
        _closeAction?.Invoke();
    }

    /// <summary>何もせずダイアログを閉じます。</summary>
    [RelayCommand]
    private void Close() => _closeAction?.Invoke();

    /// <summary>組込みプリセットを除いたユーザー定義プリセット名の一覧を返します。</summary>
    /// <returns>ユーザー定義プリセット名の一覧。</returns>
    private List<string> UserPresetNames()
        => [.. ConvertConfig.GetPresetNames().Where(n => !ConvertConfig.IsBuiltInPreset(n))];

    /// <summary>ファイル名に使用できない文字が含まれているかを判定します。</summary>
    /// <param name="name">判定する名前。</param>
    /// <returns>使用できない文字が含まれる場合は true。</returns>
    internal static bool ContainsInvalidFileNameChars(string name)
        => name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0;

    /// <summary>半角小文字に変換した際に "built-in" または "builtin" が含まれるかを判定します。</summary>
    /// <param name="name">判定する名前。</param>
    /// <returns>含まれる場合は true。</returns>
    /// <remarks>NFKC 正規化で全角英数字を半角へ変換し、ToLowerInvariant で小文字化した上で判定します。</remarks>
    internal static bool ContainsBuiltInKeyword(string name)
    {
        var normalized = name.Normalize(NormalizationForm.FormKC).ToLowerInvariant();
        return normalized.Contains("built-in") || normalized.Contains("builtin");
    }
}
