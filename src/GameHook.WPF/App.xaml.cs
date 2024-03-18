using GameHook.Domain;
using RestoreWindowPlace;
using System.Windows;

namespace GameHook.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public static App? Singleton { get; private set; }
        public WindowPlace WindowPlace { get; }

        public App()
        {
            Singleton = this;

            WindowPlace = new WindowPlace(BuildEnvironment.ConfigurationDirectoryWpfConfigFilePath);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            if (WPF.MainWindow.Singleton != null && (WPF.MainWindow.Singleton.WindowState == WindowState.Minimized ||
                                                     WPF.MainWindow.Singleton.WindowState == WindowState.Normal))
            {
                WindowPlace.Save();
            }
        }
    }
}
