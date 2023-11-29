using EZImGui.Core;

namespace EZImGui
{
    public static class App
    {
        public static string Name = null;
        public static MainWindow MainWindow;
        public static ISettings Settings;
        public static void CreateApp(string name, int sizeX, int sizeY)
        {
            Name = name;
            MainWindow = new MainWindow(name, sizeX, sizeY);
        }
        public static void AddSettings(ISettings settings)
        {
            Settings = settings;
        }
        public static void AddMenu(IMenu menu)
        {
            MainWindow.AddMenu(menu);
        }
        public static void AddPanel(IPanel panel)
        {
            MainWindow.AddPanel(panel);
        }
        public static void Start()
        {
            MainWindow.Run();
            if(Settings != null)
            {
                Settings.LoadSettings();
            }

        }

        public static void Stop()
        {
            if(Settings != null)
            {
                Settings.SaveSettings();
            }
        }

    }
}
