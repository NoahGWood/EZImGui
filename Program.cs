using EZImGui.Core;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZImGui
{
    public class MyPanel : IPanel
    {
        public void Render()
        {
            ImGui.Begin("TEST");
            ImGui.End();
            ImGui.ShowDemoWindow();
        }
    }

    public class MyMenu : IMenu
    {
        public void RenderMenu()
        {
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("Some Item"))
                    Logger.Debug("Some Item Clicked!");
                ImGui.EndMenu();
            }
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {// Create App
            App.CreateApp("My App Name", 800, 480);
            // Add Menus
            App.MainWindow.AddMenu(new MyMenu());
            // Add Panels
            App.MainWindow.AddPanel(new MyPanel());
            // Start the app
            App.Start();
        }
    }
}
