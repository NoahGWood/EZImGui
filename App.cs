using EZImGui.Core;
namespace EZImGui
{
    public static class App
    {
        public static string Name = null;
        public static MainWindow MainWindow;
        public static void CreateApp(string name, int sizeX, int sizeY)
        {
            Name = name;
            MainWindow = new MainWindow(name, sizeX, sizeY);
        }

        public static void Start()
        {
            MainWindow.Run();
        }

    }
}
