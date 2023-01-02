using System.Windows;

namespace SMSWindow
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        void AppStartup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length >= 1)
            {
                Common.RecipientNumber = e.Args[0];
            }

            if (e.Args.Length >= 2)
            {
                Common.TerminalNumber = e.Args[1];
            }
            
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}
