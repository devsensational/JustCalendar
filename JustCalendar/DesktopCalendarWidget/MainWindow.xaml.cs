using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DesktopCalendarWidget;

public partial class MainWindow : Window
{
    private DateTime _displayedMonth;
    // Shown memo widgets, keyed by MemoData.Id (hidden memos have no window)
    private readonly Dictionary<string, MemoWindow> _memoWindows = new();
    private MemoListWindow? _memoListWindow = null;
    private RoutineWindow? _routineWindow = null;
    private DateTime? _currentlyEditingDate = null;
    private bool _isLoaded = false;
    private bool _isClosing = false;

    public MainWindow()
    {
        InitializeComponent();
        _displayedMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // Restore window position and size from Config
        var cfg = DataManager.Config;
        this.Left = cfg.MainWindowLeft;
        this.Top = cfg.MainWindowTop;
        this.Width = cfg.MainWindowWidth;
        this.Height = cfg.MainWindowHeight;
        
        SliderOpacity.Value = cfg.Opacity;
        this.Opacity = cfg.Opacity;
        BtnPin.IsChecked = cfg.IsPinned;
        ChkStartup.IsChecked = StartupHelper.IsStartupEnabled();

        ApplyPinState(cfg.IsPinned);

        // Render Calendar
        RenderCalendar();

        // Restore Weekly Routine widget (Checked handler opens it)
        BtnRoutine.IsChecked = cfg.IsRoutineVisible;

        // Restore Memos after the routine widget so they stack above it
        RestoreMemos();

        // Keep the whole widget group (calendar + owned routine/memos) at bottom of desktop
        WindowHelper.SendToDesktopBottom(this);

        _isLoaded = true;
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        _isClosing = true;
        SaveCurrentState();
        DataManager.SaveData();
    }

    private void Window_LocationChanged(object sender, EventArgs e)
    {
        if (_isLoaded && !DataManager.Config.IsPinned)
        {
            DataManager.Config.MainWindowLeft = this.Left;
            DataManager.Config.MainWindowTop = this.Top;
            DataManager.SaveData();
        }
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_isLoaded && !DataManager.Config.IsPinned)
        {
            DataManager.Config.MainWindowWidth = this.Width;
            DataManager.Config.MainWindowHeight = this.Height;
            DataManager.SaveData();
        }
    }

    #region Drag & Pin Logic
    private void TopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Drag allowed only when NOT pinned
        if (!DataManager.Config.IsPinned && e.ButtonState == MouseButtonState.Pressed)
        {
            this.DragMove();
        }
    }

    private void BtnPin_Checked(object sender, RoutedEventArgs e)
    {
        ApplyPinState(true);
    }

    private void BtnPin_Unchecked(object sender, RoutedEventArgs e)
    {
        ApplyPinState(false);
    }

    private void ApplyPinState(bool pinned)
    {
        DataManager.Config.IsPinned = pinned;

        // Pin only locks window positions; bar buttons stay usable

        // Update Pin Button Visual
        BtnPin.Content = pinned ? "🔒 고정됨" : "📌 고정";

        // Notify all memo windows to lock/unlock their position
        foreach (var memo in _memoWindows.Values)
        {
            memo.SetPinned(pinned);
        }
        _routineWindow?.SetPinned(pinned);

        DataManager.SaveData();
    }
    #endregion

    #region Opacity & Startup
    private void SliderOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_isLoaded) return;
        this.Opacity = e.NewValue;
        DataManager.Config.Opacity = e.NewValue;
        
        // Also update open memos opacity to match
        foreach (var memo in _memoWindows.Values)
        {
            memo.Opacity = e.NewValue;
        }
        if (_routineWindow != null)
        {
            _routineWindow.Opacity = e.NewValue;
        }

        DataManager.SaveData();
    }

    private void ChkStartup_Checked(object sender, RoutedEventArgs e)
    {
        if (!_isLoaded) return;
        StartupHelper.SetStartup(true);
        DataManager.Config.RunOnStartup = true;
        DataManager.SaveData();
    }

    private void ChkStartup_Unchecked(object sender, RoutedEventArgs e)
    {
        if (!_isLoaded) return;
        StartupHelper.SetStartup(false);
        DataManager.Config.RunOnStartup = false;
        DataManager.SaveData();
    }
    #endregion

    #region Month Navigation & Rendering
    private void BtnPrevMonth_Click(object sender, RoutedEventArgs e)
    {
        _displayedMonth = _displayedMonth.AddMonths(-1);
        RenderCalendar();
    }

    private void BtnNextMonth_Click(object sender, RoutedEventArgs e)
    {
        _displayedMonth = _displayedMonth.AddMonths(1);
        RenderCalendar();
    }

    private void BtnToday_Click(object sender, RoutedEventArgs e)
    {
        _displayedMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        RenderCalendar();
    }

    private void RenderCalendar()
    {
        TxtCurrentMonth.Text = $"{_displayedMonth:yyyy년 M월}";
        CalendarGrid.Children.Clear();

        // Calculate start day of calendar (find previous Sunday)
        DateTime firstDayOfMonth = new DateTime(_displayedMonth.Year, _displayedMonth.Month, 1);
        int dayOfWeekOffset = (int)firstDayOfMonth.DayOfWeek;
        DateTime startDate = firstDayOfMonth.AddDays(-dayOfWeekOffset);

        // 42 cells (6 rows * 7 columns)
        for (int i = 0; i < 42; i++)
        {
            DateTime currentDate = startDate.AddDays(i);
            var cell = CreateDayCell(currentDate);
            CalendarGrid.Children.Add(cell);
        }
    }

    private FrameworkElement CreateDayCell(DateTime date)
    {
        string dateKey = date.ToString("yyyy-MM-dd");
        bool isCurrentMonth = date.Month == _displayedMonth.Month;
        bool isToday = date.Date == DateTime.Today;

        var border = new Border
        {
            BorderBrush = new SolidColorBrush(Color.FromArgb(0x33, 0xFF, 0xFF, 0xFF)),
            BorderThickness = new Thickness(0.5),
            Background = isToday ? new SolidColorBrush(Color.FromArgb(0x35, 0x41, 0x92, 0xD9)) : 
                                   new SolidColorBrush(Color.FromArgb(0x15, 0xFF, 0xFF, 0xFF)),
            Margin = new Thickness(1),
            CornerRadius = new CornerRadius(3),
            Cursor = Cursors.Hand
        };

        var grid = new Grid { Margin = new Thickness(4) };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        // Day Number
        var dayNumBlock = new TextBlock
        {
            Text = date.Day.ToString(),
            FontSize = 12,
            FontWeight = isToday ? FontWeights.Bold : FontWeights.Normal,
            Foreground = !isCurrentMonth ? new SolidColorBrush(Color.FromArgb(0x55, 0xFF, 0xFF, 0xFF)) :
                         (date.DayOfWeek == DayOfWeek.Sunday ? new SolidColorBrush(Color.FromRgb(0xFF, 0x99, 0x99)) :
                          date.DayOfWeek == DayOfWeek.Saturday ? new SolidColorBrush(Color.FromRgb(0x99, 0xCC, 0xFF)) :
                          Brushes.White),
            HorizontalAlignment = HorizontalAlignment.Left
        };
        Grid.SetRow(dayNumBlock, 0);

        // Note Content preview
        string note = string.Empty;
        if (DataManager.Config.DayNotes.TryGetValue(dateKey, out var existingText) && !string.IsNullOrWhiteSpace(existingText))
        {
            note = existingText;
        }

        var noteBlock = new TextBlock
        {
            Text = note,
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromArgb(0xEE, 0xFF, 0xFF, 0xFF)),
            TextWrapping = TextWrapping.Wrap,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Margin = new Thickness(0, 2, 0, 0)
        };
        Grid.SetRow(noteBlock, 1);

        grid.Children.Add(dayNumBlock);
        grid.Children.Add(noteBlock);
        border.Child = grid;

        // Double-click to edit cell content
        border.MouseLeftButtonDown += (s, e) =>
        {
            if (e.ClickCount == 2)
            {
                OpenCellEditor(date);
            }
        };

        return border;
    }
    #endregion

    #region Cell Double-Click Editing
    private void OpenCellEditor(DateTime date)
    {
        _currentlyEditingDate = date;
        string dateKey = date.ToString("yyyy-MM-dd");
        TxtEditDateTitle.Text = $"{date:yyyy년 MM월 dd일 (ddd)}";

        if (DataManager.Config.DayNotes.TryGetValue(dateKey, out var existing))
        {
            TxtEditContent.Text = existing;
        }
        else
        {
            TxtEditContent.Text = string.Empty;
        }

        CellEditOverlay.Visibility = Visibility.Visible;
        TxtEditContent.Focus();
        TxtEditContent.SelectAll();
    }

    private void BtnSaveCellEdit_Click(object sender, RoutedEventArgs e)
    {
        if (_currentlyEditingDate.HasValue)
        {
            string dateKey = _currentlyEditingDate.Value.ToString("yyyy-MM-dd");
            string text = TxtEditContent.Text.Trim();

            if (string.IsNullOrEmpty(text))
            {
                DataManager.Config.DayNotes.Remove(dateKey);
            }
            else
            {
                DataManager.Config.DayNotes[dateKey] = text;
            }

            DataManager.SaveData();
            RenderCalendar();
        }
        CellEditOverlay.Visibility = Visibility.Collapsed;
        _currentlyEditingDate = null;
    }

    private void BtnClearCellEdit_Click(object sender, RoutedEventArgs e)
    {
        TxtEditContent.Text = string.Empty;
    }

    private void BtnCancelCellEdit_Click(object sender, RoutedEventArgs e)
    {
        CellEditOverlay.Visibility = Visibility.Collapsed;
        _currentlyEditingDate = null;
    }

    private void CellEditOverlay_MouseDown(object sender, MouseButtonEventArgs e)
    {
        // If user clicks outside the edit card, cancel
        if (e.OriginalSource == CellEditOverlay)
        {
            BtnCancelCellEdit_Click(sender, e);
        }
    }
    #endregion

    #region Memo Management
    private void BtnMemoList_Click(object sender, RoutedEventArgs e)
    {
        if (_memoListWindow != null)
        {
            _memoListWindow.Activate();
            return;
        }

        var window = new MemoListWindow(this);
        window.Closed += (s, e) => _memoListWindow = null;
        _memoListWindow = window;
        window.Show();
    }

    /// <summary>Creates a new memo, shows it on top of the other widgets and saves.</summary>
    public MemoData AddMemo()
    {
        int offset = _memoWindows.Count * 25;
        var data = new MemoData
        {
            Left = this.Left + 50 + offset,
            Top = this.Top + 50 + offset,
            Width = 260,
            Height = 240,
            IsVisible = true
        };

        DataManager.Config.Memos.Add(data);
        ShowMemoWindow(data);
        DataManager.SaveData();
        return data;
    }

    /// <summary>Permanently removes a memo (closes its widget if shown) and saves.</summary>
    public void DeleteMemo(MemoData data)
    {
        DataManager.Config.Memos.Remove(data);
        if (_memoWindows.TryGetValue(data.Id, out var window))
        {
            window.Close();
        }
        DataManager.SaveData();
    }

    /// <summary>Shows or hides a memo widget. The memo itself is kept either way.</summary>
    public void SetMemoVisible(MemoData data, bool visible)
    {
        if (visible)
        {
            data.IsVisible = true;
            ShowMemoWindow(data);
            DataManager.SaveData();
        }
        else if (_memoWindows.TryGetValue(data.Id, out var window))
        {
            // Closed handler marks it hidden and saves
            window.Close();
        }
        else
        {
            data.IsVisible = false;
            DataManager.SaveData();
        }
    }

    private void RestoreMemos()
    {
        foreach (var memoData in DataManager.Config.Memos)
        {
            if (memoData.IsVisible)
            {
                ShowMemoWindow(memoData);
            }
        }
    }

    private void ShowMemoWindow(MemoData data)
    {
        if (_memoWindows.TryGetValue(data.Id, out var existing))
        {
            existing.Activate();
            return;
        }

        var memo = new MemoWindow(data);
        memo.Owner = this; // Group with the calendar (one Alt+Tab entry, always above calendar)
        memo.Opacity = DataManager.Config.Opacity;
        memo.SetPinned(DataManager.Config.IsPinned);

        memo.ContentChanged += (s, e) => _memoListWindow?.Refresh();
        memo.Closed += (s, e) =>
        {
            // Owned windows close along with the calendar on exit; keep their saved state
            if (_isClosing) return;

            _memoWindows.Remove(data.Id);
            data.IsVisible = false;
            DataManager.SaveData();
            _memoListWindow?.Refresh();
        };

        _memoWindows[data.Id] = memo;
        // Shown (and activated) last, so it lands above the calendar and routine widgets
        memo.Show();
    }

    public void SaveCurrentState()
    {
        DataManager.Config.MainWindowLeft = this.Left;
        DataManager.Config.MainWindowTop = this.Top;
        DataManager.Config.MainWindowWidth = this.Width;
        DataManager.Config.MainWindowHeight = this.Height;
        DataManager.Config.Opacity = this.Opacity;

        // Shown memos write their window geometry/content back into their MemoData
        foreach (var m in _memoWindows.Values)
        {
            m.GetMemoData();
        }
    }
    #endregion

    #region Weekly Routine Window
    private void BtnRoutine_Checked(object sender, RoutedEventArgs e)
    {
        ShowRoutineWindow();
        DataManager.Config.IsRoutineVisible = true;
        DataManager.SaveData();
    }

    private void BtnRoutine_Unchecked(object sender, RoutedEventArgs e)
    {
        // Only a user action (toggle / window's ✕) hides it.
        // Closing during app shutdown keeps IsRoutineVisible so it reopens next launch.
        DataManager.Config.IsRoutineVisible = false;
        DataManager.SaveData();
        _routineWindow?.Close();
    }

    private void ShowRoutineWindow()
    {
        if (_routineWindow != null)
        {
            _routineWindow.Activate();
            return;
        }

        var cfg = DataManager.Config;

        // First time: place it right below the calendar with the same width
        if (!cfg.RoutineWindowLeft.HasValue || !cfg.RoutineWindowTop.HasValue)
        {
            const double defaultHeight = 200;
            double top = this.Top + this.Height + 8;
            double screenBottom = SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight;
            if (top + defaultHeight > screenBottom)
            {
                // Not enough room below -> place above the calendar
                top = Math.Max(SystemParameters.VirtualScreenTop, this.Top - defaultHeight - 8);
            }

            cfg.RoutineWindowLeft = this.Left;
            cfg.RoutineWindowTop = top;
            cfg.RoutineWindowWidth = this.Width;
            cfg.RoutineWindowHeight = defaultHeight;
        }

        var window = new RoutineWindow();
        window.Owner = this; // Group with the calendar (one Alt+Tab entry, activated together)
        window.Opacity = cfg.Opacity;
        window.SetPinned(cfg.IsPinned);

        // Window's own ✕ button -> go through the toggle so both stay in sync
        window.CloseRequested += (s, e) => BtnRoutine.IsChecked = false;
        window.Closed += (s, e) =>
        {
            if (_routineWindow == window)
            {
                _routineWindow = null;
            }
        };

        _routineWindow = window;
        window.Show();
    }
    #endregion

    private void BtnExit_Click(object sender, RoutedEventArgs e)
    {
        _isClosing = true;
        SaveCurrentState();
        DataManager.SaveData();
        Application.Current.Shutdown();
    }
}