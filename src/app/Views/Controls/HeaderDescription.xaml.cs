using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ClipboardZenHanConverter.Views.Controls;

public sealed partial class HeaderDescription : UserControl
{
    public HeaderDescription()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // HeaderStyle が null → デフォルト Style を適用
        HeaderText.Style = HeaderStyle ?? (Style)Resources["DefaultHeaderStyle"];

        // DescriptionStyle が null → デフォルト Style を適用
        DescriptionText.Style = DescriptionStyle ?? (Style)Resources["DefaultDescriptionStyle"];
    }

    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(string), typeof(HeaderDescription), new PropertyMetadata(""));

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }
    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(string), typeof(HeaderDescription), new PropertyMetadata(""));

    public Style HeaderStyle
    {
        get => (Style)GetValue(HeaderStyleProperty);
        set => SetValue(HeaderStyleProperty, value);
    }
    public static readonly DependencyProperty HeaderStyleProperty =
        DependencyProperty.Register(nameof(HeaderStyle), typeof(Style), typeof(HeaderDescription), new PropertyMetadata(null));

    public Style DescriptionStyle
    {
        get => (Style)GetValue(DescriptionStyleProperty);
        set => SetValue(DescriptionStyleProperty, value);
    }
    public static readonly DependencyProperty DescriptionStyleProperty =
        DependencyProperty.Register(nameof(DescriptionStyle), typeof(Style), typeof(HeaderDescription), new PropertyMetadata(null));
}
