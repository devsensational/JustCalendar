using System.Windows;

namespace DesktopCalendarWidget;

public partial class App : Application
{
    private void Application_Startup(object sender, StartupEventArgs e)
    {
        // Global configuration initialization if needed
        DataManager.LoadData();
        var mainWindow = new MainWindow();
        mainWindow.Show();
    }

    private void Application_Exit(object sender, ExitEventArgs e)
    {
        DataManager.SaveData();
    }
}