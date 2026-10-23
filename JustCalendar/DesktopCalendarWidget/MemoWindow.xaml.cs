using System;
using System.Windows;
using System.Windows.Input;

namespace DesktopCalendarWidget;

public partial class MemoWindow : Window
{
    private readonly MemoData _data;
    private bool _isPinned = false;
    private double _previousHeight = 240;

    /// <summary>Raised when the memo text changes (so the memo list can update its preview).</summary>
    public event EventHandler? ContentChanged;

    public MemoWindow(MemoData data)
    {
        InitializeComponent();
        _data = data;

        // Only the main calendar should show up in Alt+Tab
        SourceInitialized += (s, e) => WindowHelper.HideFromAltTab(this);
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        this.Left = _data.Left;
        this.Top = _data.Top;
        this.Width = _data.Width;
        this.Height = _data.Height;
        TxtMemoContent.Text = _data.Content;

        if (_data.IsMinimized)
        {
            ApplyMinimize(true);
        }

        // Not sent to the bottom: a newly shown memo must stay above the calendar and routine widgets.
        // (The main window sends the whole widget group to the bottom on startup.)
    }

    public void SetPinned(bool pinned)
    {
        // When main calendar is pinned, only dragging is blocked (see MemoTitleBar_MouseLeftButtonDown)
        _isPinned = pinned;
    }

    private void MemoTitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Drag only allowed if not pinned
        if (!_isPinned && e.ButtonState == MouseButtonState.Pressed)
        {
            this.DragMove();
        }
    }

    private void BtnMinimize_Click(object sender, RoutedEventArgs e)
    {
        ApplyMinimize(!_data.IsMinimized);
        DataManager.SaveData();
    }

    private void ApplyMinimize(bool minimize)
    {
        _data.IsMinimized = minimize;
        if (minimize)
        {
            _previousHeight = this.Height;
            this.Height = 28;
            TxtMemoContent.Visibility = Visibility.Collapsed;
            BtnMinimize.Content = "□";
        }
        else
        {
            this.Height = _previousHeight > 30 ? _previousHeight : 240;
            TxtMemoContent.Visibility = Visibility.Visible;
            BtnMinimize.Content = "―";
        }
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        // Hides the widget only; the memo stays in the list (MainWindow marks it as not visible)
        this.Close();
    }

    private void TxtMemoContent_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        _data.Content = TxtMemoContent.Text;
        DataManager.SaveData();
        ContentChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Window_LocationChanged(object sender, EventArgs e)
    {
        if (!_isPinned)
        {
            _data.Left = this.Left;
            _data.Top = this.Top;
            DataManager.SaveData();
        }
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (!_isPinned && !_data.IsMinimized)
        {
            _data.Width = this.Width;
            _data.Height = this.Height;
            DataManager.SaveData();
        }
    }

    public MemoData GetMemoData()
    {
        _data.Left = this.Left;
        _data.Top = this.Top;
        _data.Width = this.Width;
        _data.Height = this.Height;
        _data.Content = TxtMemoContent.Text;
        return _data;
    }
}