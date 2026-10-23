using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace DesktopCalendarWidget;

/// <summary>
/// Horizontal widget for fixed weekly routines (Mon ~ Sun).
/// Content is keyed by day of week, so it never resets when a week passes.
/// </summary>
public partial class RoutineWindow : Window
{
    private static readonly DayOfWeek[] DayOrder =
    {
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
        DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
    };

    private static readonly string[] DayNames = { "월", "화", "수", "목", "금", "토", "일" };

    private bool _isPinned = false;
    private bool _isLoaded = false;
    private DayOfWeek? _editingDay = null;
    private DateTime _renderedDate = DateTime.MinValue;
    private readonly DispatcherTimer _dayChangeTimer;

    /// <summary>Raised when the user clicks this window's close (✕) button.</summary>
    public event EventHandler? CloseRequested;

    public RoutineWindow()
    {
        InitializeComponent();

        // Restore position/size before the window is shown (avoids a visible jump)
        var cfg = DataManager.Config;
        if (cfg.RoutineWindowLeft.HasValue) this.Left = cfg.RoutineWindowLeft.Value;
        if (cfg.RoutineWindowTop.HasValue) this.Top = cfg.RoutineWindowTop.Value;
        if (cfg.RoutineWindowWidth.HasValue) this.Width = Math.Max(MinWidth, cfg.RoutineWindowWidth.Value);
        if (cfg.RoutineWindowHeight.HasValue) this.Height = Math.Max(MinHeight, cfg.RoutineWindowHeight.Value);

        // Re-render when the date changes (to move the "today" highlight at midnight)
        _dayChangeTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
        _dayChangeTimer.Tick += (s, e) =>
        {
            if (DateTime.Today != _renderedDate && EditOverlay.Visibility != Visibility.Visible)
            {
                RenderRoutines();
            }
        };
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        RenderRoutines();
        _dayChangeTimer.Start();

        // Keep at bottom of desktop like the other widgets
        WindowHelper.SendToDesktopBottom(this);

        _isLoaded = true;
    }

    private void Window_Closed(object? sender, EventArgs e)
    {
        _dayChangeTimer.Stop();
    }

    #region Position / Size / Pin
    private void Window_LocationChanged(object? sender, EventArgs e)
    {
        if (_isLoaded && !_isPinned)
        {
            DataManager.Config.RoutineWindowLeft = this.Left;
            DataManager.Config.RoutineWindowTop = this.Top;
            DataManager.SaveData();
        }
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_isLoaded && !_isPinned)
        {
            DataManager.Config.RoutineWindowWidth = this.Width;
            DataManager.Config.RoutineWindowHeight = this.Height;
            DataManager.SaveData();
        }
    }

    public void SetPinned(bool pinned)
    {
        _isPinned = pinned;

        // When the main calendar is pinned, freeze moving / resizing (closing stays allowed)
        this.ResizeMode = pinned ? ResizeMode.NoResize : ResizeMode.CanResizeWithGrip;
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Drag only allowed if not pinned
        if (!_isPinned && e.ButtonState == MouseButtonState.Pressed)
        {
            this.DragMove();
        }
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        // Let the main window handle it so its toggle button stays in sync
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
    #endregion

    #region Rendering
    private void RenderRoutines()
    {
        RoutineGrid.Children.Clear();
        _renderedDate = DateTime.Today;

        for (int i = 0; i < DayOrder.Length; i++)
        {
            RoutineGrid.Children.Add(CreateDayColumn(DayOrder[i], DayNames[i], DayOrder[i] == _renderedDate.DayOfWeek));
        }
    }

    private FrameworkElement CreateDayColumn(DayOfWeek day, string dayName, bool isToday)
    {
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

        var grid = new Grid { Margin = new Thickness(6, 4, 6, 4) };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        // Header: day name (+ "오늘" badge)
        var header = new StackPanel { Orientation = Orientation.Horizontal };
        header.Children.Add(new TextBlock
        {
            Text = dayName,
            FontSize = 13,
            FontWeight = FontWeights.Bold,
            Foreground = day == DayOfWeek.Sunday ? new SolidColorBrush(Color.FromRgb(0xFF, 0x80, 0x80)) :
                         day == DayOfWeek.Saturday ? new SolidColorBrush(Color.FromRgb(0x80, 0xB3, 0xFF)) :
                         new SolidColorBrush(Color.FromRgb(0xEE, 0xEE, 0xEE)),
            VerticalAlignment = VerticalAlignment.Center
        });

        if (isToday)
        {
            header.Children.Add(new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(0xCC, 0x41, 0x92, 0xD9)),
                CornerRadius = new CornerRadius(3),
                Padding = new Thickness(4, 0, 4, 1),
                Margin = new Thickness(6, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Child = new TextBlock { Text = "오늘", FontSize = 10, Foreground = Brushes.White }
            });
        }
        Grid.SetRow(header, 0);

        // Routine content preview
        DataManager.Config.WeeklyRoutines.TryGetValue(day.ToString(), out var routine);

        var contentBlock = new TextBlock
        {
            Text = routine ?? string.Empty,
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromArgb(0xEE, 0xFF, 0xFF, 0xFF)),
            TextWrapping = TextWrapping.Wrap,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Margin = new Thickness(0, 4, 0, 0)
        };
        Grid.SetRow(contentBlock, 1);

        grid.Children.Add(header);
        grid.Children.Add(contentBlock);
        border.Child = grid;

        // Double-click to edit this day's routine
        border.MouseLeftButtonDown += (s, e) =>
        {
            if (e.ClickCount == 2)
            {
                OpenEditor(day, dayName);
            }
        };

        return border;
    }
    #endregion

    #region Double-Click Editing
    private void OpenEditor(DayOfWeek day, string dayName)
    {
        _editingDay = day;
        TxtEditTitle.Text = $"{dayName}요일 루틴";

        DataManager.Config.WeeklyRoutines.TryGetValue(day.ToString(), out var existing);
        TxtEditContent.Text = existing ?? string.Empty;

        EditOverlay.Visibility = Visibility.Visible;
        TxtEditContent.Focus();
        TxtEditContent.SelectAll();
    }

    private void BtnSaveEdit_Click(object sender, RoutedEventArgs e)
    {
        if (_editingDay.HasValue)
        {
            string key = _editingDay.Value.ToString();
            string text = TxtEditContent.Text.Trim();

            if (string.IsNullOrEmpty(text))
            {
                DataManager.Config.WeeklyRoutines.Remove(key);
            }
            else
            {
                DataManager.Config.WeeklyRoutines[key] = text;
            }

            DataManager.SaveData();
            RenderRoutines();
        }
        CloseEditor();
    }

    private void BtnClearEdit_Click(object sender, RoutedEventArgs e)
    {
        TxtEditContent.Text = string.Empty;
    }

    private void BtnCancelEdit_Click(object sender, RoutedEventArgs e)
    {
        CloseEditor();
    }

    private void EditOverlay_MouseDown(object sender, MouseButtonEventArgs e)
    {
        // Clicking outside the edit card cancels
        if (e.OriginalSource == EditOverlay)
        {
            CloseEditor();
        }
    }

    private void CloseEditor()
    {
        EditOverlay.Visibility = Visibility.Collapsed;
        _editingDay = null;
    }
    #endregion
}
