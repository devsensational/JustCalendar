using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace DesktopCalendarWidget;

/// <summary>
/// Lists every saved memo. Add / delete memos and toggle whether each one is shown as a desktop widget.
/// All changes are applied (and saved) immediately through the main window.
/// </summary>
public partial class MemoListWindow : Window
{
    private readonly MainWindow _host;
    private string? _selectedId = null;

    public MemoListWindow(MainWindow host)
    {
        InitializeComponent();
        _host = host;
        Owner = host;

        // Only the main calendar should show up in Alt+Tab
        SourceInitialized += (s, e) => WindowHelper.HideFromAltTab(this);
        Loaded += (s, e) => Refresh();
    }

    /// <summary>Rebuilds the memo rows from DataManager.Config.Memos.</summary>
    public void Refresh()
    {
        MemoRows.Children.Clear();
        var memos = DataManager.Config.Memos;

        if (_selectedId != null && !memos.Exists(m => m.Id == _selectedId))
        {
            _selectedId = null;
        }

        foreach (var memo in memos)
        {
            MemoRows.Children.Add(CreateRow(memo));
        }

        TxtEmpty.Visibility = memos.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        TxtCount.Text = $"{memos.Count}개";
        BtnDelete.IsEnabled = _selectedId != null;
    }

    private FrameworkElement CreateRow(MemoData memo)
    {
        bool isSelected = memo.Id == _selectedId;

        var border = new Border
        {
            Background = isSelected ? new SolidColorBrush(Color.FromArgb(0x55, 0x41, 0x92, 0xD9)) :
                                      new SolidColorBrush(Color.FromArgb(0x15, 0xFF, 0xFF, 0xFF)),
            BorderBrush = isSelected ? new SolidColorBrush(Color.FromArgb(0xAA, 0x41, 0x92, 0xD9)) :
                                       new SolidColorBrush(Color.FromArgb(0x22, 0xFF, 0xFF, 0xFF)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Margin = new Thickness(0, 0, 0, 4),
            Padding = new Thickness(8, 6, 6, 6),
            Cursor = Cursors.Hand
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        string? preview = GetPreview(memo.Content);
        var previewBlock = new TextBlock
        {
            Text = preview ?? "(빈 메모)",
            FontSize = 12,
            Foreground = preview == null ? new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF)) :
                                           new SolidColorBrush(Color.FromArgb(0xEE, 0xFF, 0xFF, 0xFF)),
            FontStyle = preview == null ? FontStyles.Italic : FontStyles.Normal,
            TextTrimming = TextTrimming.CharacterEllipsis,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 8, 0),
            ToolTip = string.IsNullOrWhiteSpace(memo.Content) ? null : memo.Content
        };
        Grid.SetColumn(previewBlock, 0);

        // Shows the current state and toggles it
        var toggle = new ToggleButton
        {
            IsChecked = memo.IsVisible,
            Content = memo.IsVisible ? "👁 표시 ON" : "표시 OFF",
            Style = (Style)FindResource("VisibleToggleStyle"),
            ToolTip = memo.IsVisible ? "바탕화면에서 숨기기" : "바탕화면에 표시하기"
        };
        toggle.Click += (s, e) =>
        {
            _selectedId = memo.Id;
            _host.SetMemoVisible(memo, toggle.IsChecked == true);
            Refresh();
            // Keep this list above the memo that was just shown
            Activate();
        };
        Grid.SetColumn(toggle, 1);

        grid.Children.Add(previewBlock);
        grid.Children.Add(toggle);
        border.Child = grid;

        border.MouseLeftButtonDown += (s, e) =>
        {
            _selectedId = memo.Id;
            Refresh();
        };

        return border;
    }

    /// <summary>First non-empty line of the memo, or null if the memo is empty.</summary>
    private static string? GetPreview(string content)
    {
        foreach (var line in content.Split('\n'))
        {
            string trimmed = line.Trim();
            if (trimmed.Length > 0) return trimmed;
        }
        return null;
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            this.DragMove();
        }
    }

    private void BtnAdd_Click(object sender, RoutedEventArgs e)
    {
        var memo = _host.AddMemo();
        _selectedId = memo.Id;
        Refresh();
        Activate();
    }

    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        var memo = DataManager.Config.Memos.Find(m => m.Id == _selectedId);
        if (memo == null) return;

        if (!string.IsNullOrWhiteSpace(memo.Content))
        {
            string preview = GetPreview(memo.Content) ?? string.Empty;
            var result = MessageBox.Show(this, $"이 메모를 삭제할까요?\n\n\"{preview}\"\n\n삭제한 메모는 되돌릴 수 없습니다.",
                                         "메모 삭제", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            if (result != MessageBoxResult.Yes) return;
        }

        _host.DeleteMemo(memo);
        _selectedId = null;
        Refresh();
    }

    private void BtnOk_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
