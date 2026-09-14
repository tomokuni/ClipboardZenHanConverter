using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.Foundation;

namespace ClipboardZenHanConverter.App.WinUI.Views.Controls;

/// <summary>上下 2 分割の分割バー領域です。</summary>
/// <remarks>
/// 提供機能: <br/>
/// - 上下方向のリサイズカーソルの表示<br/>
/// - ドラッグ量の通知（<see cref="DeltaChanged"/>）<br/><br/>
/// 特徴: <br/>
/// - WinUI の <c>Thumb</c> はシール型でカーソルを設定できず、既定テンプレートも持たないため
///   （ヒットテスト可能な領域が無い）、ポインター操作を自前で扱います<br/>
/// - <see cref="UIElement.ProtectedCursor"/> は protected のため、派生クラスから設定します<br/>
/// - ドラッグ量はウィンドウ基準の座標で求めるため、分割バー自身が移動しても正しく算出できます
/// </remarks>
public sealed partial class SplitterRegion : Grid
{
    /// <summary>ドラッグ中かどうか。</summary>
    private bool _isDragging;

    /// <summary>直前のポインター位置（ウィンドウ基準）。</summary>
    private Point _lastPosition;

    /// <summary>SplitterRegion の新しいインスタンスを初期化します。</summary>
    public SplitterRegion()
    {
        ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.SizeNorthSouth);

        // 他の要素が処理済みにした場合でも確実に受け取る
        AddHandler(PointerPressedEvent, new PointerEventHandler(OnPointerPressed), handledEventsToo: true);
        AddHandler(PointerMovedEvent, new PointerEventHandler(OnPointerMoved), handledEventsToo: true);
        AddHandler(PointerReleasedEvent, new PointerEventHandler(OnPointerReleased), handledEventsToo: true);
        PointerCaptureLost += (_, _) => _isDragging = false;
    }

    /// <summary>ドラッグによる垂直方向の移動量を通知するイベントです。</summary>
    /// <remarks>前回の通知からの差分（下方が正）を通知します。</remarks>
    public event Action<double>? DeltaChanged;

    /// <summary>ポインターが押されたときにドラッグを開始します。</summary>
    /// <param name="sender">イベントソース。</param>
    /// <param name="e">ポインターイベントデータ。</param>
    private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        CapturePointer(e.Pointer);
        _isDragging = true;
        _lastPosition = e.GetCurrentPoint(null).Position;
    }

    /// <summary>ドラッグ中の移動量を通知します。</summary>
    /// <param name="sender">イベントソース。</param>
    /// <param name="e">ポインターイベントデータ。</param>
    /// <remarks>座標は分割バー自身ではなくウィンドウ基準で取得します。<br/>
    /// 分割バー基準にすると、バーが移動する分だけ移動量が打ち消されて算出できません。<br/>
    /// 座標の単位は XAML の論理ピクセルであり、分割比率の計算に使う高さ（<c>ActualHeight</c>）と一致します。</remarks>
    private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!_isDragging)
        {
            return;
        }

        var position = e.GetCurrentPoint(null).Position;
        var delta = position.Y - _lastPosition.Y;
        _lastPosition = position;

        if (delta == 0)
        {
            return;
        }

        DeltaChanged?.Invoke(delta);
    }

    /// <summary>ポインターが離されたときにドラッグを終了します。</summary>
    /// <param name="sender">イベントソース。</param>
    /// <param name="e">ポインターイベントデータ。</param>
    private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (!_isDragging)
        {
            return;
        }

        _isDragging = false;
        ReleasePointerCapture(e.Pointer);
    }
}
