using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DesktopCalendarWidget;

public class AppConfig
{
    public double MainWindowLeft { get; set; } = 200;
    public double MainWindowTop { get; set; } = 100;
    public double MainWindowWidth { get; set; } = 920;
    public double MainWindowHeight { get; set; } = 640;
    public double Opacity { get; set; } = 0.85;
    public bool IsPinned { get; set; } = false;
    public bool RunOnStartup { get; set; } = false;

    // Key: "yyyy-MM-dd", Value: text note
    public Dictionary<string, string> DayNotes { get; set; } = new();

    // List of active memo stickies
    public List<MemoData> Memos { get; set; } = new();

    // Weekly routine widget
    // Key: DayOfWeek name ("Monday" ~ "Sunday"), Value: routine text.
    // Keyed by weekday (not date), so it repeats every week and never resets.
    public Dictionary<string, string> WeeklyRoutines { get; set; } = new();
    public bool IsRoutineVisible { get; set; } = false;
    // null = not placed yet (first open places it just below the calendar)
    public double? RoutineWindowLeft { get; set; }
    public double? RoutineWindowTop { get; set; }
    public double? RoutineWindowWidth { get; set; }
    public double? RoutineWindowHeight { get; set; }
}

public class MemoData
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public double Left { get; set; } = 250;
    public double Top { get; set; } = 150;
    public double Width { get; set; } = 260;
    public double Height { get; set; } = 240;
    public string Content { get; set; } = string.Empty;
    public bool IsMinimized { get; set; } = false;
}